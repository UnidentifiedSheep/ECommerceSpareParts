#!/usr/bin/env python3
"""Deploy immutable application images through the Arcane Swarm API."""

import argparse
import copy
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import signal
import sys
import time
from urllib.error import HTTPError, URLError
from urllib.parse import quote, urlsplit, urlencode
from urllib.request import HTTPRedirectHandler, Request, build_opener

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "ci"))
from release import DIGEST, Registries, ReleaseError, require, save, validate

RUNTIME = {"main-api": (10, "api_main-api"), "search-api": (20, "api_search-api"),
           "analytics-api": (20, "api_analytics-api"), "pricing-api": (30, "api_pricing-api"),
           "main-worker": (40, "workers_main-worker"), "analytics-worker": (40, "workers_analytics-worker"),
           "pricing-worker": (40, "workers_pricing-worker"), "gateway": (50, "gateway_gateway")}
STACKS = {"api", "workers", "gateway", "migrators"}
LABEL = "io.ecommerce.cd"


def log(message):
    print(f"[{datetime.now(timezone.utc):%H:%M:%S UTC}] {message}", flush=True)


def setting(name, default, low=1, high=3600):
    value = os.getenv(name, str(default))
    require(value.isdigit() and low <= int(value) <= high, f"Invalid {name}")
    return int(value)


def fingerprint(spec):
    return hashlib.sha256(json.dumps(spec, sort_keys=True, separators=(",", ":")).encode()).hexdigest()


class APIError(ReleaseError):
    def __init__(self, method, path, status):
        super().__init__(f"Arcane {method} {path.split('?')[0]} failed (HTTP {status})")
        self.status = status


class NoRedirect(HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


class Arcane:
    def __init__(self, url, key, environment):
        parsed = urlsplit(url)
        require(parsed.scheme == "https" or (parsed.scheme == "http" and parsed.hostname in ("127.0.0.1", "localhost")),
                "ARCANE_URL must use HTTPS")
        require(parsed.hostname and not parsed.username and not parsed.password and not parsed.query and not parsed.fragment,
                "Invalid ARCANE_URL")
        require(key and "\n" not in key and "\r" not in key, "ARCANE_API_KEY is required")
        require(re.fullmatch(r"[a-zA-Z0-9-]+", environment), "Invalid ARCANE_ENVIRONMENT_ID")
        self.url, self.key, self.environment = url.rstrip("/"), key, environment
        self.opener = build_opener(NoRedirect())

    def request(self, method, path, body=None, *, environment=None):
        env = self.environment if environment is None else environment
        url = f"{self.url}/api/environments/{quote(env, safe='')}/{path}"
        data = json.dumps(body).encode() if body is not None else None
        for attempt in range(4):
            try:
                request = Request(url, data=data, method=method,
                                  headers={"X-Api-Key": self.key, "Content-Type": "application/json", "Accept": "application/json"})
                with self.opener.open(request, timeout=20) as response:
                    result = json.load(response)
                require(isinstance(result, dict) and result.get("success") is True, "Arcane returned an unsuccessful response")
                require(not result.get("failedEnvironments"), "Arcane returned incomplete environment results")
                return result
            except HTTPError as error:
                status = error.code
                error.close()
                failure = APIError(method, path, status)
                if method != "GET" or status not in (429, 500, 502, 503, 504) or attempt == 3:
                    raise failure from None
            except (OSError, URLError, ValueError) as error:
                if method != "GET" or attempt == 3:
                    raise ReleaseError(f"Arcane {method} {path.split('?')[0]} failed ({type(error).__name__})") from None
            time.sleep(min(2 ** attempt, 4))

    def get(self, path, **kwargs):
        return self.request("GET", path, **kwargs)["data"]

    def listing(self, path):
        rows, seen = [], set()
        for _ in range(1000):
            page = self.request("GET", path + ("&" if "?" in path else "?") + urlencode({"start": len(rows), "limit": 100}))
            items = page.get("data") or []
            require(isinstance(items, list), "Arcane returned an invalid list")
            for item in items:
                require(item["id"] not in seen, "Arcane pagination returned duplicate items")
                seen.add(item["id"])
            rows.extend(items)
            pagination = page.get("pagination")
            if pagination is None or len(rows) >= pagination["totalItems"]:
                return rows
            require(items, "Arcane pagination stopped before all items were read")
        raise ReleaseError("Arcane pagination limit exceeded")


class Deployment:
    def __init__(self, api, registries, release, report_file, operation="deploy", source=None):
        self.api, self.registries, self.release = api, registries, validate(release)
        self.report_file, self.operation, self.source = report_file, operation, source
        self.run_id = os.getenv("GITHUB_RUN_ID", "local")
        self.attempt = os.getenv("GITHUB_RUN_ATTEMPT", "1")
        require(re.fullmatch(r"[a-zA-Z0-9-]+", self.run_id + "-" + self.attempt), "Invalid deployment run ID")
        self.repository = os.getenv("GITHUB_REPOSITORY", "local/local")
        self.environment = os.getenv("DEPLOY_ENVIRONMENT", "Production")
        require(re.fullmatch(r"[a-zA-Z0-9_-]+", self.environment), "Invalid DEPLOY_ENVIRONMENT")
        self.runtime_timeout = setting("ROLLOUT_TIMEOUT_SECONDS", 600)
        self.migration_timeout = setting("MIGRATOR_TIMEOUT_SECONDS", 1200)
        self.stability = setting("DEPLOY_STABILITY_SECONDS", 45, 10, 300)
        self.memory = setting("MIGRATOR_MEMORY_MB", 768, 128, 4096) * 1024 * 1024
        self.deadline = time.monotonic() + setting("DEPLOY_TIMEOUT_SECONDS", 3600, 60)
        self.nodes, self.services, self.before, self.journal = {}, {}, {}, []
        self.active_migrator = None
        self.report = {"version": 1, "operation": operation, "run_id": self.run_id, "attempt": self.attempt,
                       "repository": self.repository, "arcane_url": api.url, "environment_id": api.environment,
                       "release": release, "previous_sha": os.getenv("PREVIOUS_DEPLOYED_SHA", ""),
                       "status": "preparing", "services": [], "images": [], "migrators": [],
                       "started_at": datetime.now(timezone.utc).isoformat()}

    def checkpoint(self):
        save(self.report_file, self.report)

    def check_time(self, deadline):
        require(time.monotonic() < min(deadline, self.deadline), "Deployment time budget exceeded")

    def tasks(self, service_id):
        return self.api.listing(f"swarm/services/{service_id}/tasks")

    def inspect(self, service_id):
        return self.api.get(f"swarm/services/{service_id}")

    def node_environment(self, node_id):
        require(node_id in self.nodes, "Task is assigned to an unknown node")
        node = self.nodes[node_id]
        require(node["status"] == "ready", f"Node {node['hostname']} is not ready")
        agent = node.get("agent", {})
        if agent.get("bindingKind") == "local":
            return self.api.environment
        require(agent.get("state") == "connected" and agent.get("environmentId"),
                f"Arcane agent is unavailable on {node['hostname']}")
        return agent["environmentId"]

    def container(self, task):
        require(task.get("containerId"), "Task has no container ID")
        return self.api.get(f"containers/{task['containerId']}", environment=self.node_environment(task["nodeId"]))

    def eligible(self, spec):
        nodes = list(self.nodes.values())
        for constraint in spec.get("TaskTemplate", {}).get("Placement", {}).get("Constraints", []):
            match = re.fullmatch(r"\s*(node\.(?:labels\.[\w.-]+|role|id|hostname))\s*(==|!=)\s*([\w.-]+)\s*", constraint)
            require(match is not None, "Unsupported application placement constraint")
            field, operator, expected = match.groups()
            def matches(node):
                actual = node.get("labels", {}).get(field[12:]) if field.startswith("node.labels.") else node.get(field[5:])
                return (actual == expected) if operator == "==" else (actual != expected)
            nodes = [node for node in nodes if matches(node)]
        require(nodes, "No node satisfies application placement constraints")
        for node in nodes:
            require(node["status"] == "ready" and node["availability"] == "active", f"Placement node {node['hostname']} is unavailable")
            self.node_environment(node["id"])

    def prepare(self):
        self.nodes = {n["id"]: n for n in self.api.listing("swarm/nodes")}
        managers = [n for n in self.nodes.values() if n["role"] == "manager"]
        require(any(n.get("managerStatus") == "leader" for n in managers), "Swarm has no leader")
        require(sum(n.get("reachability") == "reachable" and n["status"] == "ready" for n in managers) > len(managers) // 2,
                "Swarm manager quorum is unavailable")
        syncs = self.api.listing("gitops-syncs")
        for sync in syncs:
            if (sync.get("projectName") or sync.get("name")) not in STACKS or not sync.get("autoSync"):
                continue
            require(self.operation != "plan", f"Disable Auto Sync for {sync.get('name')} before deployment")
            self.api.request("PUT", f"gitops-syncs/{sync['id']}", {"autoSync": False})
            require(self.api.get(f"gitops-syncs/{sync['id']}")["autoSync"] is False, "Failed to disable application Auto Sync")
            log(f"Disabled Auto Sync for {sync.get('name')}.")
        summaries = self.api.listing("swarm/services")
        self.services = {s["name"]: s for s in summaries}
        require(len(self.services) == len(summaries), "Duplicate Swarm service names")
        for row in self.release["images"]:
            service = row["service"]
            if service not in RUNTIME:
                require(service.endswith("-migrator"), "Unsupported release service")
                continue
            name = RUNTIME[service][1]
            require(name in self.services, f"Deploy the {name.split('_')[0]} stack from the infrastructure repository first: {name} is missing")
            inspected = self.inspect(self.services[name]["id"])
            spec = inspected["spec"]
            require(spec.get("Mode", {}).get("Replicated", {}).get("Replicas", 0) > 0, f"{name} is scaled to zero")
            require("@sha256:" in spec["TaskTemplate"]["ContainerSpec"]["Image"], f"{name} has no pinned image for rollback")
            self.registries.auth(spec["TaskTemplate"]["ContainerSpec"]["Image"])
            old_image = spec["TaskTemplate"]["ContainerSpec"]["Image"]
            require(self.registries.digest(old_image) == old_image.rsplit("@", 1)[1], "Previous application image is unavailable for rollback")
            self.eligible(spec)
            if not service.endswith("-worker"):
                health = spec["TaskTemplate"]["ContainerSpec"].get("Healthcheck", {}).get("Test", [])
                require(health and health[0] != "NONE", f"{name} requires a Docker healthcheck")
            self.before[service] = inspected
        profile = self.registries.profile(kind=self.release["registry_kind"])
        for row in self.release["images"]:
            reference = profile["host"] + "/" + row["image"]
            require(self.registries.digest(reference + "@" + row["digest"]) == row["digest"], "Published image digest is unavailable")
            self.report["images"].append(dict(row, reference=reference,
                                               before_current=self.registries.digest(reference + ":current", missing=True)))
        if any(i["service"].endswith("-migrator") for i in self.release["images"]):
            network = [n for n in self.api.listing("networks") if n["name"] == "backend"]
            require(len(network) == 1 and network[0].get("driver") == "overlay", "The backend overlay network is missing")
            self.backend = network[0]["id"]
            self.secrets = {s["spec"]["Name"]: s["id"] for s in self.api.get("swarm/secrets")}
            self.secret_names = json.loads(os.getenv("MIGRATOR_SECRET_NAMES", "{}"))
            require(isinstance(self.secret_names, dict), "MIGRATOR_SECRET_NAMES must be a JSON object")
            for row in self.report["images"]:
                if row["service"].endswith("-migrator"):
                    self.migrator_spec(row)  # Validate every dependency before starting the first migration.
        self.report["status"] = "planned"
        self.checkpoint()
        log(f"Preflight passed: {len(self.before)} application service(s), {len(self.report['images'])} image(s).")

    def migrator_spec(self, row):
        prefix = row["service"].removesuffix("-migrator")
        secret = self.secret_names.get(prefix, prefix + ".migrator_v1")
        require(secret in self.secrets, f"Swarm migrator secret {secret} is missing")
        spec = {"Name": f"ecom-cd-{row['service']}-{self.run_id}-{self.attempt}",
                "Labels": {LABEL + ".repository": self.repository, LABEL + ".run": self.run_id + "-" + self.attempt},
                "Mode": {"Replicated": {"Replicas": 1}},
                "TaskTemplate": {"ContainerSpec": {"Image": row["reference"] + "@" + row["digest"],
                    "Env": ["DOTNET_ENVIRONMENT=" + self.environment, "ASPNETCORE_ENVIRONMENT=" + self.environment],
                    "StopGracePeriod": 30_000_000_000,
                    "Ulimits": [{"Name": "core", "Soft": 0, "Hard": 0}],
                    "Secrets": [{"SecretID": self.secrets[secret], "SecretName": secret,
                                 "File": {"Name": f"/app/configs/migrator.{prefix}.settings.json", "UID": "0", "GID": "0", "Mode": 0o444}}]},
                    "Networks": [{"Target": self.backend}], "ForceUpdate": 0,
                    "Placement": {"Constraints": ["node.labels.infra.postgres == true"]},
                    "Resources": {"Limits": {"MemoryBytes": self.memory, "NanoCPUs": 1_000_000_000, "Pids": 256}},
                    "RestartPolicy": {"Condition": "none"},
                    "LogDriver": {"Name": "json-file", "Options": {"max-size": "15m", "max-file": "3"}}}}
        self.eligible(spec)
        return spec

    def update(self, service_id, spec, *, previous=None):
        current = self.inspect(service_id)
        if previous is not None:
            require(fingerprint(current["spec"]) == fingerprint(previous), "Service configuration changed during deployment")
        if fingerprint(current["spec"]) == fingerprint(spec):
            return
        image = spec["TaskTemplate"]["ContainerSpec"]["Image"]
        try:
            self.api.request("PUT", f"swarm/services/{service_id}",
                             {"version": current["version"]["Index"], "spec": spec,
                              "options": {"encodedRegistryAuth": self.registries.auth(image), "queryRegistry": False}})
        except ReleaseError:
            # A lost response is not a reason to blindly issue a second mutation.
            if fingerprint(self.inspect(service_id)["spec"]) != fingerprint(spec):
                raise

    def wait(self, service_id, spec, *, stable=None, timeout=None, restoring=False):
        deadline = time.monotonic() + (timeout or self.runtime_timeout)
        stable_since, last_ids = None, None
        target = spec["TaskTemplate"]["ContainerSpec"]["Image"]
        replicas = spec["Mode"]["Replicated"]["Replicas"]
        health_required = bool(spec["TaskTemplate"]["ContainerSpec"].get("Healthcheck", {}).get("Test"))
        last_log = 0
        while True:
            self.check_time(deadline)
            inspected = self.inspect(service_id)
            require(fingerprint(inspected["spec"]) == fingerprint(spec), "Service spec changed or Swarm reverted the deployment")
            state = inspected.get("updateStatus", {}).get("State", "")
            require(state not in ("paused", "rollback_paused") and (restoring or not state.startswith("rollback_")), f"Swarm rollout failed: {state}")
            tasks = self.tasks(service_id)
            active = [t for t in tasks if t["desiredState"] == "running"]
            ready = len(active) == replicas and state in (("", "completed", "rollback_completed") if restoring else ("", "completed"))
            identities = tuple(sorted(t["id"] for t in active))
            for task in active:
                if task["currentState"] != "running" or task.get("image") != target:
                    ready = False
                    continue
                try:
                    container = self.container(task)
                except APIError as error:
                    if error.status != 404:
                        raise
                    ready = False
                    continue
                cstate = container["state"]
                require(not cstate.get("health", {}).get("status") == "unhealthy", "Application container is unhealthy")
                if not cstate.get("running") or (health_required and cstate.get("health", {}).get("status") != "healthy"):
                    ready = False
            if any(t["currentState"] in ("running", "starting") and t["desiredState"] != "running" for t in tasks):
                ready = False
            if not ready or identities != last_ids:
                stable_since = None
            last_ids = identities
            if ready:
                stable_since = stable_since or time.monotonic()
                if time.monotonic() - stable_since >= (self.stability if stable is None else stable):
                    return identities
            if time.monotonic() - last_log >= 15:
                log(f"Waiting for {spec['Name']}: {sum(t['currentState'] == 'running' for t in active)}/{replicas} running; update={state or 'none'}.")
                last_log = time.monotonic()
            time.sleep(3)

    def clean_migrator(self):
        if self.active_migrator is None:
            return
        service_id = self.active_migrator
        try:
            current = self.inspect(service_id)
        except APIError as error:
            if error.status != 404:
                raise
            self.active_migrator = None
            return
        require(current["spec"].get("Labels", {}).get(LABEL + ".run") == self.run_id + "-" + self.attempt,
                "Refusing to remove a migrator owned by another deployment")
        tasks = self.tasks(service_id)
        if any(t["currentState"] in ("running", "starting", "preparing") for t in tasks):
            spec = copy.deepcopy(current["spec"])
            spec["Mode"] = {"Replicated": {"Replicas": 0}}
            self.update(service_id, spec, previous=current["spec"])
            deadline = time.monotonic() + 90
            while any(t["currentState"] in ("running", "starting", "preparing") for t in self.tasks(service_id)):
                require(time.monotonic() < deadline, "Migrator did not stop; inspect it in Arcane before retrying")
                time.sleep(2)
        try:
            self.api.request("DELETE", f"swarm/services/{service_id}")
        except APIError as error:
            if error.status != 404:
                raise
        self.active_migrator = None

    def migrate(self, row):
        spec = self.migrator_spec(row)
        log(f"Starting {row['service']} (one replica, {self.memory // 1024 // 1024} MiB, one CPU).")
        existing = [s for s in self.api.listing("swarm/services") if s["name"] == spec["Name"]]
        require(not existing, "This deployment already has a migrator service; inspect it before retrying")
        try:
            result = self.api.request("POST", "swarm/services", {"spec": spec,
                                      "options": {"encodedRegistryAuth": self.registries.auth(spec["TaskTemplate"]["ContainerSpec"]["Image"]),
                                                  "queryRegistry": False}})
            self.active_migrator = result["data"]["id"]
        except ReleaseError:
            created = [s for s in self.api.listing("swarm/services") if s["name"] == spec["Name"]]
            if len(created) != 1:
                raise
            inspected = self.inspect(created[0]["id"])
            require(inspected["spec"].get("Labels", {}).get(LABEL + ".run") == self.run_id + "-" + self.attempt
                    and inspected["spec"]["TaskTemplate"]["ContainerSpec"]["Image"] == spec["TaskTemplate"]["ContainerSpec"]["Image"],
                    "Cannot reconcile migrator creation")
            self.active_migrator = created[0]["id"]
        record = {"service": row["service"], "id": self.active_migrator, "status": "running"}
        self.report["migrators"].append(record)
        self.checkpoint()
        deadline = time.monotonic() + self.migration_timeout
        try:
            while True:
                self.check_time(deadline)
                tasks = self.tasks(self.active_migrator)
                require(len(tasks) <= 1, "Migrator created more than one task; deployment stopped")
                if tasks:
                    task = tasks[0]
                    record["task_id"] = task["id"]
                    require(task["currentState"] not in ("failed", "rejected", "orphaned", "shutdown"),
                            f"{row['service']} failed; inspect its task logs in Arcane")
                    if task["currentState"] == "complete":
                        state = self.container(task)["state"]
                        require(not state.get("running") and state.get("exitCode", 0) == 0, "Migrator exited unsuccessfully")
                        record["status"] = "complete"
                        self.checkpoint()
                        log(f"{row['service']} completed successfully.")
                        return
                time.sleep(3)
        finally:
            self.clean_migrator()

    def deploy_runtime(self, row):
        before = self.inspect(self.before[row["service"]]["id"])
        require(fingerprint(before["spec"]) == fingerprint(self.before[row["service"]]["spec"]), "Service changed since preflight")
        spec = copy.deepcopy(before["spec"])
        spec["TaskTemplate"]["ContainerSpec"]["Image"] = row["reference"] + "@" + row["digest"]
        labels = spec["TaskTemplate"]["ContainerSpec"].setdefault("Labels", {})
        if labels is None:
            labels = spec["TaskTemplate"]["ContainerSpec"]["Labels"] = {}
        labels["com.getarcaneapp.arcane.updater"] = "false"
        record = {"service": row["service"], "id": before["id"], "name": spec["Name"],
                  "before_image": before["spec"]["TaskTemplate"]["ContainerSpec"]["Image"],
                  "image": spec["TaskTemplate"]["ContainerSpec"]["Image"],
                  "before_hash": fingerprint(before["spec"]), "applied_hash": fingerprint(spec), "status": "updating"}
        record["check"] = "healthcheck" if spec["TaskTemplate"]["ContainerSpec"].get("Healthcheck", {}).get("Test") else "stable_process"
        rollback_spec = copy.deepcopy(spec)
        rollback_spec["TaskTemplate"]["ContainerSpec"]["Image"] = record["before_image"]
        record["rollback_hash"] = fingerprint(rollback_spec)
        self.report["services"].append(record)
        if record["before_hash"] != record["applied_hash"]:
            self.journal.append((before, spec, record))
        self.checkpoint()  # The public journal contains image refs and hashes, never full specs or credentials.
        log(f"Updating {spec['Name']} to {row['digest']}.")
        self.update(before["id"], spec, previous=before["spec"])
        record["tasks"] = list(self.wait(before["id"], spec))
        record["status"] = "healthy"
        self.checkpoint()

    def revert(self):
        self.deadline = time.monotonic() + 900
        errors = []
        for before, applied, record in reversed(self.journal):
            try:
                current = self.inspect(before["id"])
                if fingerprint(current["spec"]) != fingerprint(before["spec"]):
                    require(fingerprint(current["spec"]) == fingerprint(applied), "Service was modified by another deployment; automatic rollback stopped")
                    self.update(before["id"], before["spec"], previous=applied)
                self.wait(before["id"], before["spec"], stable=10, timeout=180, restoring=True)
                record["status"] = "rolled_back"
                log(f"Restored {before['spec']['Name']}.")
            except ReleaseError as error:
                errors.append(str(error))
                record["status"] = "rollback_failed"
            self.checkpoint()
        require(not errors, "Rollback needs attention: " + "; ".join(errors))

    def rollback(self):
        source = self.source
        require(source["arcane_url"] == self.api.url and source["environment_id"] == self.api.environment,
                "Rollback report belongs to another Arcane environment")
        self.nodes = {n["id"]: n for n in self.api.listing("swarm/nodes")}
        self.report["images"] = copy.deepcopy(source["images"])
        self.report["previous_sha"] = source.get("previous_sha", "")
        planned = []
        for row in source["services"]:
            current = self.inspect(row["id"])
            require(current["spec"]["Name"] == RUNTIME[row["service"]][1], "Unexpected rollback service")
            current_hash = fingerprint(current["spec"])
            require(current_hash in (row["before_hash"], row["applied_hash"], row.get("rollback_hash")), "A service changed after this release; refusing stale rollback")
            spec = copy.deepcopy(current["spec"])
            spec["TaskTemplate"]["ContainerSpec"]["Image"] = row["before_image"]
            require("@sha256:" in row["before_image"], "Rollback requires an immutable image")
            self.registries.auth(row["before_image"])
            require(self.registries.digest(row["before_image"]) == row["before_image"].rsplit("@", 1)[1], "Rollback image is unavailable")
            self.eligible(spec)
            planned.append((row, current, spec))
        self.report["status"] = "rolling_back"
        self.checkpoint()
        errors = []
        for row, current, spec in reversed(planned):
            try:
                self.update(current["id"], spec, previous=current["spec"])
                tasks = self.wait(current["id"], spec, restoring=True)
                self.report["services"].append(dict(row, status="rolled_back", tasks=list(tasks)))
            except ReleaseError as error:
                errors.append(str(error))
                self.report["services"].append(dict(row, status="rollback_failed", tasks=[]))
            self.checkpoint()
        self.report["status"] = "rollback_failed" if errors else "rolled_back"
        self.checkpoint()
        require(not errors, "Rollback needs attention: " + "; ".join(errors))

    def run(self):
        self.checkpoint()
        if self.operation == "rollback":
            try:
                self.rollback()
            except BaseException:
                self.report["status"] = "rollback_failed" if self.report["status"] in ("rolling_back", "rollback_failed") else "failed"
                self.checkpoint()
                raise
            return
        try:
            self.prepare()
            if self.operation == "plan":
                return
            self.report["status"] = "deploying"
            self.checkpoint()
            for row in self.report["images"]:
                if row["service"].endswith("-migrator"):
                    self.migrate(row)
            for row in sorted((r for r in self.report["images"] if r["service"] in RUNTIME), key=lambda r: RUNTIME[r["service"]]):
                self.deploy_runtime(row)
            self.report["status"] = "deployed"
            self.checkpoint()
            log("All selected services passed rollout checks. Registry aliases can now be promoted.")
        except BaseException:
            self.report["status"] = "failed"
            try:
                self.clean_migrator()
                self.revert()
                if self.journal:
                    self.report["status"] = "failed_rolled_back"
            except ReleaseError as rollback_error:
                self.report["status"] = "rollback_failed"
                log(str(rollback_error))
            self.checkpoint()
            raise


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--operation", choices=("plan", "deploy", "rollback"), required=True)
    parser.add_argument("--input", required=True)
    parser.add_argument("--report", default="deployment-report.json")
    args = parser.parse_args()
    source = json.loads(Path(args.input).read_text())
    release = source["release"] if args.operation == "rollback" else source
    api = Arcane(os.getenv("ARCANE_URL", "https://arcane.all-vp-n.ru"), os.getenv("ARCANE_API_KEY", ""),
                 os.getenv("ARCANE_ENVIRONMENT_ID", "0"))
    deployment = Deployment(api, Registries(), release, args.report, args.operation, source)
    def cancelled(signum, frame):
        signal.signal(signum, signal.SIG_IGN)
        raise ReleaseError("Deployment was interrupted")
    signal.signal(signal.SIGTERM, cancelled)
    signal.signal(signal.SIGINT, cancelled)
    deployment.run()


if __name__ == "__main__":
    try:
        main()
    except (ReleaseError, ValueError, KeyError, OSError, TypeError) as error:
        log(f"Deployment failed: {error if isinstance(error, ReleaseError) else type(error).__name__}")
        sys.exit(1)
