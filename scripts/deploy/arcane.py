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

import yaml
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


def redact(value):
    for name in ("ARCANE_API_KEY", "GH_TOKEN", "GITHUB_TOKEN"):
        secret = os.getenv(name, "")
        if secret:
            value = value.replace(secret, "[REDACTED]")
    value = re.sub(r"(?i)([a-z][a-z0-9+.-]*://)[^\s/@:]+:[^\s/@]+@", r"\1[REDACTED]@", value)
    value = re.sub(r"(?i)(\b(?:Bearer|Basic)\s+)[a-zA-Z0-9._~+/-]+=*", r"\1[REDACTED]", value)
    return re.sub(
        r'''(?ix)(["']?(?:password|pwd|secret|client_secret|access_token|api_key|authorization|IssuerSigningKey|InternalToken)["']?\s*[:=]\s*)
        ("[^"\n]*"|'[^'\n]*'|[^;\s,}]+)''',
        r"\1[REDACTED]", value)


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

    def request(self, method, path, body=None, *, environment=None, global_api=False):
        env = self.environment if environment is None else environment
        prefix = f"{self.url}/api" if global_api else f"{self.url}/api/environments/{quote(env, safe='')}"
        url = f"{prefix}/{path}"
        data = json.dumps(body).encode() if body is not None else None
        for attempt in range(4):
            try:
                request = Request(url, data=data, method=method,
                                  headers={"X-Api-Key": self.key, "Content-Type": "application/json", "Accept": "application/json"})
                with self.opener.open(request, timeout=90 if method == "POST" else 20) as response:
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

    def download_logs(self, container_id, *, environment):
        path = f"containers/{quote(container_id, safe='')}/logs/download"
        url = f"{self.url}/api/environments/{quote(environment, safe='')}/{path}"
        maximum = 4 * 1024 * 1024
        try:
            request = Request(url, headers={"X-Api-Key": self.key, "Accept": "text/plain"})
            with self.opener.open(request, timeout=20) as response:
                data = response.read(maximum + 1)
            return data[:maximum].decode("utf-8", errors="replace"), len(data) > maximum
        except HTTPError as error:
            status = error.code
            error.close()
            raise APIError("GET", path, status) from None
        except (OSError, URLError) as error:
            raise ReleaseError(f"Arcane log download failed ({type(error).__name__})") from None

    def listing(self, path, **kwargs):
        rows, seen = [], set()
        for _ in range(1000):
            page = self.request("GET", path + ("&" if "?" in path else "?") + urlencode({"start": len(rows), "limit": 100}), **kwargs)
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
        self.migrator_stack = None
        self.plans = []
        self.report = {"version": 2, "operation": operation, "run_id": self.run_id, "attempt": self.attempt,
                       "repository": self.repository, "arcane_url": api.url, "environment_id": api.environment,
                       "release": release, "previous_sha": os.getenv("PREVIOUS_DEPLOYED_SHA", ""),
                       "status": "preparing", "services": [], "stacks": [], "images": [], "migrators": [],
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

    def stack_source(self, name):
        source = self.api.get(f"swarm/stacks/{name}/source")
        require(source.get("name") == name and source.get("composeContent"), "Arcane has no saved source for the application stack")
        return {"name": name, "composeContent": source["composeContent"],
                "overrideContent": source.get("overrideContent") or "", "envContent": source.get("envContent") or "",
                "files": source.get("files") or []}

    def stack_payload(self, source, images):
        try:
            overlay = yaml.safe_load(source["overrideContent"]) or {}
        except yaml.YAMLError:
            raise ReleaseError("Unsupported Compose override in Arcane; apply the infrastructure stack first") from None
        require(isinstance(overlay, dict), "Invalid Compose override")
        services = overlay.setdefault("services", {})
        require(isinstance(services, dict), "Invalid services in Compose override")
        for name, image in images.items():
            prefix = source["name"] + "_"
            require(name.startswith(prefix) and "@sha256:" in image, "Stack deployment requires immutable images")
            item = services.setdefault(name[len(prefix):], {})
            require(isinstance(item, dict), "Invalid service in Compose override")
            item["image"] = image
        result = dict(source, overrideContent=json.dumps(overlay, sort_keys=True))
        return dict(result, withRegistryAuth=True, resolveImage="never", prune=False)

    @staticmethod
    def source_hash(source):
        return fingerprint({k: source.get(k) or ([] if k == "files" else "")
                            for k in ("name", "composeContent", "overrideContent", "envContent", "files")})

    def render(self, payload, expected):
        result = self.api.request("POST", "swarm/stacks/config/render",
                                  {k: payload[k] for k in ("name", "composeContent", "overrideContent", "envContent")})["data"]
        require(set(result["services"]) == set(expected), "Saved stack source does not match live application services")

    def check_registry(self):
        matches = []
        for registry in self.api.listing("container-registries", global_api=True):
            url = registry.get("url", "")
            parsed = urlsplit(url if "://" in url else "https://" + url)
            if parsed.hostname == "ghcr.io" and registry.get("enabled"):
                matches.append(registry)
        require(len(matches) == 1 and matches[0].get("username"), "Add one enabled GHCR registry with a read:packages PAT to Arcane")
        if self.operation != "plan":
            self.api.request("POST", f"container-registries/{matches[0]['id']}/test", global_api=True)
        log("Using GHCR credentials stored in Arcane.")

    def prepare(self):
        self.check_registry()
        self.nodes = {n["id"]: n for n in self.api.listing("swarm/nodes")}
        managers = [n for n in self.nodes.values() if n["role"] == "manager"]
        require(any(n.get("managerStatus") == "leader" for n in managers), "Swarm has no leader")
        require(sum(n.get("reachability") == "reachable" and n["status"] == "ready" for n in managers) > len(managers) // 2,
                "Swarm manager quorum is unavailable")
        for sync in self.api.listing("gitops-syncs"):
            if (sync.get("projectName") or sync.get("name")) not in STACKS or not sync.get("autoSync"):
                continue
            require(self.operation != "plan", f"Disable Auto Sync for {sync.get('name')} before deployment")
            self.api.request("PUT", f"gitops-syncs/{sync['id']}", {"autoSync": False})
            require(self.api.get(f"gitops-syncs/{sync['id']}")["autoSync"] is False, "Failed to disable application Auto Sync")
            log(f"Disabled Auto Sync for {sync.get('name')}.")
        summaries = self.api.listing("swarm/services")
        self.services = {row["name"]: row for row in summaries}
        require(len(self.services) == len(summaries), "Duplicate Swarm service names")
        replacements = {}
        for row in self.release["images"]:
            reference = self.release["registry"] + "/" + row["image"]
            require(self.registries.digest(reference + "@" + row["digest"]) == row["digest"], "Published GHCR digest is unavailable")
            self.report["images"].append(dict(row, reference=reference,
                                               before_current=self.registries.digest(reference + ":current", missing=True)))
            if row["service"] in RUNTIME:
                replacements[RUNTIME[row["service"]][1]] = reference + "@" + row["digest"]
        affected = {name.split("_", 1)[0] for name in replacements}
        for stack in sorted(affected, key=lambda name: min(v[0] for v in RUNTIME.values() if v[1].startswith(name + "_"))):
            names = {v[1] for v in RUNTIME.values() if v[1].startswith(stack + "_")}
            require(names <= self.services.keys(), f"Apply the {stack} stack from the infrastructure repository first")
            source = self.stack_source(stack)
            snapshots, images = {}, {}
            for name in sorted(names):
                inspected = self.inspect(self.services[name]["id"])
                spec = inspected["spec"]
                image = spec["TaskTemplate"]["ContainerSpec"]["Image"]
                require("@sha256:" in image, f"{name} has no immutable image for rollback")
                if image.startswith(self.registries.prefix + "/"):
                    require(self.registries.digest(image) == image.rsplit("@", 1)[1], "Previous GHCR image is unavailable for rollback")
                self.eligible(spec)
                if name in replacements:
                    require(spec.get("Mode", {}).get("Replicated", {}).get("Replicas", 0) > 0, f"{name} is scaled to zero")
                    if not name.endswith("-worker"):
                        health = spec["TaskTemplate"]["ContainerSpec"].get("Healthcheck", {}).get("Test", [])
                        require(health and health[0] != "NONE", f"{name} requires a Docker healthcheck")
                snapshots[name], images[name] = inspected, image
            target = {name: replacements.get(name, image) for name, image in images.items()}
            payload = self.stack_payload(source, target)
            self.render(payload, names)
            self.plans.append(dict(name=stack, source=source, before=snapshots, images=target,
                                   payload=payload, rollback=self.stack_payload(source, images), after={}))
        if any(i["service"].endswith("-migrator") for i in self.report["images"]):
            networks = [n for n in self.api.listing("networks") if n["name"] == "backend"]
            require(len(networks) == 1 and networks[0].get("driver") == "overlay", "The backend overlay network is missing")
            self.backend = networks[0]["id"]
            self.secrets = {item["spec"]["Name"]: item["id"] for item in self.api.get("swarm/secrets")}
            self.secret_names = json.loads(os.getenv("MIGRATOR_SECRET_NAMES", "{}"))
            require(isinstance(self.secret_names, dict), "MIGRATOR_SECRET_NAMES must be a JSON object")
            for row in self.report["images"]:
                if row["service"].endswith("-migrator"):
                    payload = self.migrator_payload(row)
                    self.render(payload, {payload["name"] + "_run"})
        self.report["status"] = "planned"
        self.checkpoint()
        log(f"Preflight passed: {len(self.plans)} stack(s), {len(self.report['images'])} GHCR image(s).")

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
        try:
            self.api.request("PUT", f"swarm/services/{service_id}",
                             {"version": current["version"]["Index"], "spec": spec,
                              "options": {"registryAuthFrom": "spec", "queryRegistry": False}})
        except ReleaseError:
            # A lost response is not a reason to blindly issue a second mutation.
            if fingerprint(self.inspect(service_id)["spec"]) != fingerprint(spec):
                raise

    def wait(self, service_id, spec, *, stable=None, timeout=None, restoring=False, once=False):
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
                if once or time.monotonic() - stable_since >= (self.stability if stable is None else stable):
                    return identities
            require(not once, f"Unchanged service {spec['Name']} is not ready")
            if time.monotonic() - last_log >= 15:
                log(f"Waiting for {spec['Name']}: {sum(t['currentState'] == 'running' for t in active)}/{replicas} running; update={state or 'none'}.")
                last_log = time.monotonic()
            time.sleep(3)

    def migrator_payload(self, row):
        spec = self.migrator_spec(row)
        prefix = row["service"].removesuffix("-migrator")
        secret = self.secret_names.get(prefix, prefix + ".migrator_v1")
        labels = {LABEL + ".run": self.run_id + "-" + self.attempt, LABEL + ".repository": self.repository}
        compose = {"services": {"run": {"image": row["reference"] + "@" + row["digest"],
            "environment": {"DOTNET_ENVIRONMENT": self.environment, "ASPNETCORE_ENVIRONMENT": self.environment},
            "networks": ["backend"], "stop_grace_period": "30s", "ulimits": {"core": 0},
            "secrets": [{"source": "database", "target": f"/app/configs/migrator.{prefix}.settings.json"}],
            "labels": {"com.getarcaneapp.arcane.updater": "false"},
            "logging": {"driver": "json-file", "options": {"max-size": "15m", "max-file": "3"}},
            "deploy": {"mode": "replicated", "replicas": 1, "labels": labels,
                "restart_policy": {"condition": "none"}, "placement": {"constraints": ["node.labels.infra.postgres == true"]},
                "resources": {"limits": {"memory": str(self.memory), "cpus": "1.0", "pids": 256}}}}},
            "networks": {"backend": {"external": True, "name": "backend"}},
            "secrets": {"database": {"external": True, "name": secret}}}
        return {"name": spec["Name"], "composeContent": json.dumps(compose), "overrideContent": "", "envContent": "",
                "withRegistryAuth": True, "resolveImage": "never", "prune": False}

    def clean_migrator(self):
        if self.migrator_stack is None:
            return
        payload = self.migrator_stack
        members = [s for s in self.api.listing("swarm/services") if s["name"].startswith(payload["name"] + "_")]
        for member in members:
            current = self.inspect(member["id"])
            require(current["spec"].get("Labels", {}).get(LABEL + ".run") == self.run_id + "-" + self.attempt,
                    "Refusing to remove a migrator owned by another deployment")
            if any(t["currentState"] in ("running", "starting", "preparing") for t in self.tasks(member["id"])):
                spec = copy.deepcopy(current["spec"])
                spec["Mode"] = {"Replicated": {"Replicas": 0}}
                self.update(member["id"], spec, previous=current["spec"])
                deadline = time.monotonic() + 90
                while any(t["currentState"] in ("running", "starting", "preparing") for t in self.tasks(member["id"])):
                    require(time.monotonic() < deadline, "Migrator did not stop; inspect it in Arcane before retrying")
                    time.sleep(2)
        try:
            source = self.stack_source(payload["name"])
        except APIError as error:
            if error.status != 404:
                raise
            source = None
        if source is not None:
            require(self.source_hash(source) == self.source_hash(payload), "Migrator stack source changed; cleanup stopped")
        if members or source is not None:
            try:
                self.api.request("DELETE", f"swarm/stacks/{payload['name']}")
            except APIError as error:
                if error.status != 404:
                    raise
        self.active_migrator, self.migrator_stack = None, None

    def migration_diagnostics(self, row, record, task, error):
        """Capture diagnostics while the failed task still exists, before cleanup."""
        try:
            if task is None and self.active_migrator:
                tasks = self.tasks(self.active_migrator)
                task = tasks[-1] if tasks else None
            details = [f"{row['service']}: {error if isinstance(error, ReleaseError) else type(error).__name__}"]
            if record is not None:
                record["status"] = "failed"
            if task:
                details.append(f"Task: {task['id']}; state: {task.get('currentState', 'unknown')}; node: {task.get('nodeId', 'unassigned')}")
                if task.get("error"):
                    details.append("Task error: " + task["error"])
                if record is not None:
                    record.update(task_id=task["id"], node_id=task.get("nodeId"), container_id=task.get("containerId"),
                                  task_state=task.get("currentState"), task_error=redact(task.get("error", "")))
                if task.get("containerId"):
                    try:
                        text, truncated = self.api.download_logs(task["containerId"], environment=self.node_environment(task["nodeId"]))
                        details.extend(["", "Container logs:", text])
                        if truncated:
                            details.append("[Log download truncated at 4 MiB]")
                    except Exception as log_error:
                        details.append(f"Container logs unavailable: {log_error if isinstance(log_error, ReleaseError) else type(log_error).__name__}")
                else:
                    details.append("No container was created; use the task error above.")
            else:
                details.append("No migration task was available.")
            text = redact("\n".join(details))
            directory = Path(self.report_file).parent / "deployment-diagnostics"
            directory.mkdir(parents=True, exist_ok=True)
            filename = directory / f"{row['service']}.log"
            fd = os.open(filename, os.O_WRONLY | os.O_CREAT | os.O_TRUNC, 0o600)
            with os.fdopen(fd, "w") as stream:
                stream.write(text + "\n")
            if record is not None:
                record["log_file"] = str(filename)
            self.checkpoint()
            log(f"Migration diagnostics saved to {filename}. Last log lines:")
            for line in text[-24_000:].splitlines()[-100:]:
                # Prevent application output from being interpreted as Actions workflow commands.
                print("[migrator] " + line, flush=True)
        except Exception as diagnostic_error:
            log(f"Could not collect migration diagnostics ({type(diagnostic_error).__name__}).")

    def migrate(self, row):
        payload = self.migrator_payload(row)
        name = payload["name"] + "_run"
        require(not any(s["name"] == name for s in self.api.listing("swarm/services")), "This deployment already has a migrator; inspect it before retrying")
        self.migrator_stack = payload
        record, task = None, None
        log(f"Starting {row['service']} through Arcane (one replica, {self.memory // 1024 // 1024} MiB, one CPU).")
        try:
            try:
                self.api.request("POST", "swarm/stacks", payload)
            except ReleaseError:
                created = [s for s in self.api.listing("swarm/services") if s["name"] == name]
                if len(created) != 1:
                    raise
                current = self.inspect(created[0]["id"])
                require(current["spec"].get("Labels", {}).get(LABEL + ".run") == self.run_id + "-" + self.attempt
                        and current["spec"]["TaskTemplate"]["ContainerSpec"]["Image"] == row["reference"] + "@" + row["digest"],
                        "Cannot reconcile migrator stack creation")
            created = [s for s in self.api.listing("swarm/services") if s["name"] == name]
            require(len(created) == 1, "Arcane did not create exactly one migrator service")
            self.active_migrator = created[0]["id"]
            record = {"service": row["service"], "id": self.active_migrator, "stack": payload["name"], "status": "running"}
            self.report["migrators"].append(record)
            self.checkpoint()
            deadline = time.monotonic() + self.migration_timeout
            while True:
                self.check_time(deadline)
                tasks = self.tasks(self.active_migrator)
                require(len(tasks) <= 1, "Migrator created more than one task; deployment stopped")
                if tasks:
                    task = tasks[0]
                    record["task_id"] = task["id"]
                    require(task["currentState"] not in ("failed", "rejected", "orphaned", "shutdown"),
                            f"{row['service']} failed; see migration diagnostics in this CD run")
                    if task["currentState"] == "complete":
                        state = self.container(task)["state"]
                        require(not state.get("running") and state.get("exitCode", 0) == 0, "Migrator exited unsuccessfully")
                        record["status"] = "complete"
                        self.checkpoint()
                        log(f"{row['service']} completed successfully.")
                        return
                time.sleep(3)
        except BaseException as error:
            self.migration_diagnostics(row, record, task, error)
            raise
        finally:
            self.clean_migrator()

    def capture(self, plan):
        for name, before in plan["before"].items():
            plan["after"][name] = self.inspect(before["id"])

    def deploy_stack(self, plan):
        require(self.source_hash(self.stack_source(plan["name"])) == self.source_hash(plan["source"]), "Stack source changed since preflight")
        for before in plan["before"].values():
            require(fingerprint(self.inspect(before["id"])["spec"]) == fingerprint(before["spec"]), "Service changed since preflight")
        record = {"name": plan["name"], "before_source_hash": self.source_hash(plan["source"]),
                  "applied_source_hash": self.source_hash(plan["payload"]), "rollback_source_hash": self.source_hash(plan["rollback"]),
                  "before_images": {name: row["spec"]["TaskTemplate"]["ContainerSpec"]["Image"] for name, row in plan["before"].items()},
                  "images": plan["images"], "status": "updating"}
        plan["record"] = record
        self.journal.append(plan)
        self.report["stacks"].append(record)
        self.checkpoint()
        # Keep per-service rollout order even though credentials are applied through the stack API.
        images = dict(record["before_images"])
        changed = sorted((name for name in plan["images"] if images[name] != plan["images"][name]),
                         key=lambda name: RUNTIME[name.split("_", 1)[1]])
        rows = {}
        for name in changed or [None]:
            if name is not None:
                images[name] = plan["images"][name]
            if plan["after"]:
                require(self.source_hash(self.stack_source(plan["name"])) == self.source_hash(plan["payload"]), "Stack source changed between service updates")
                for previous in plan["after"].values():
                    require(fingerprint(self.inspect(previous["id"])["spec"]) == fingerprint(previous["spec"]), "Service changed between rollout steps")
            plan["payload"] = self.stack_payload(plan["source"], images)
            record["applied_source_hash"] = self.source_hash(plan["payload"])
            self.checkpoint()
            log(f"Updating {name or plan['name']} using the registry saved in Arcane.")
            try:
                self.api.request("POST", "swarm/stacks", plan["payload"])
            except ReleaseError:
                self.capture(plan)
                raise
            self.capture(plan)
            for full, after in plan["after"].items():
                before, spec = plan["before"][full], after["spec"]
                require(spec["TaskTemplate"]["ContainerSpec"]["Image"] == images[full], "Stack image differs from the requested digest")
                if full not in rows:
                    rows[full] = {"service": full.split("_", 1)[1], "id": before["id"], "name": full, "stack": plan["name"],
                                  "before_image": record["before_images"][full], "before_hash": fingerprint(before["spec"]), "status": "updating"}
                    self.report["services"].append(rows[full])
                row = rows[full]
                rollback_spec = copy.deepcopy(spec)
                rollback_spec["TaskTemplate"]["ContainerSpec"]["Image"] = row["before_image"]
                row.update(image=images[full], applied_hash=fingerprint(spec), rollback_hash=fingerprint(rollback_spec))
                row["check"] = "healthcheck" if spec["TaskTemplate"]["ContainerSpec"].get("Healthcheck", {}).get("Test") else "stable_process"
            self.checkpoint()
            targets = [name] if name is not None else list(rows)
            for full in targets:
                current = plan["after"][full]
                unchanged = rows[full]["before_hash"] == rows[full]["applied_hash"]
                rows[full]["tasks"] = list(self.wait(current["id"], current["spec"], restoring=unchanged, once=unchanged))
                rows[full]["status"] = "healthy"
                self.checkpoint()
        for full, row in rows.items():
            if row["status"] == "healthy":
                continue
            current = plan["after"][full]
            unchanged = row["before_hash"] == row["applied_hash"]
            row["tasks"] = list(self.wait(current["id"], current["spec"], restoring=True, once=unchanged))
            row["status"] = "healthy"
            self.checkpoint()
        record["status"] = "healthy"
        self.checkpoint()

    def revert(self):
        self.deadline = time.monotonic() + 900
        errors = []
        for plan in reversed(self.journal):
            try:
                require(self.source_hash(self.stack_source(plan["name"])) in {self.source_hash(plan[k]) for k in ("source", "payload", "rollback")},
                        "Stack source was modified by another deployment; automatic rollback stopped")
                for name, before in plan["before"].items():
                    current = self.inspect(before["id"])
                    allowed = {fingerprint(before["spec"])}
                    if name in plan["after"]:
                        allowed.add(fingerprint(plan["after"][name]["spec"]))
                    require(fingerprint(current["spec"]) in allowed, "Service was modified by another deployment; automatic rollback stopped")
                self.api.request("POST", "swarm/stacks", plan["rollback"])
                for before in plan["before"].values():
                    current = self.inspect(before["id"])
                    require(current["spec"]["TaskTemplate"]["ContainerSpec"]["Image"] == before["spec"]["TaskTemplate"]["ContainerSpec"]["Image"], "Rollback image mismatch")
                    self.wait(before["id"], current["spec"], stable=10, timeout=180, restoring=True)
                    for row in self.report["services"]:
                        if row["id"] == before["id"]:
                            row.update(status="rolled_back", rollback_hash=fingerprint(current["spec"]))
                plan["record"]["status"] = "rolled_back"
                log(f"Restored {plan['name']}.")
            except ReleaseError as error:
                errors.append(str(error))
                plan["record"]["status"] = "rollback_failed"
            self.checkpoint()
        require(not errors, "Rollback needs attention: " + "; ".join(errors))

    def rollback(self):
        self.check_registry()
        source = self.source
        require(source.get("version") == 2 and source["arcane_url"] == self.api.url and source["environment_id"] == self.api.environment,
                "Rollback requires a deployment report from this Arcane environment and deployment version")
        self.nodes = {n["id"]: n for n in self.api.listing("swarm/nodes")}
        self.report["images"] = copy.deepcopy(source["images"])
        self.report["previous_sha"] = source.get("previous_sha", "")
        plans = []
        for record in source["stacks"]:
            name = record["name"]
            require(name in STACKS - {"migrators"}, "Unexpected rollback stack")
            current_source = self.stack_source(name)
            require(self.source_hash(current_source) in {record[k] for k in ("applied_source_hash", "rollback_source_hash")}, "Stack source changed after this release; refusing stale rollback")
            rows = [row for row in source["services"] if row["stack"] == name]
            require({row["name"] for row in rows} == set(record["before_images"]), "Incomplete rollback service report")
            for row in rows:
                current = self.inspect(row["id"])
                require(current["spec"]["Name"] == RUNTIME[row["service"]][1], "Unexpected rollback service")
                require(fingerprint(current["spec"]) in {row["before_hash"], row["applied_hash"], row.get("rollback_hash")}, "Service changed after this release; refusing stale rollback")
                self.eligible(current["spec"])
                image = row["before_image"]
                if image.startswith(self.registries.prefix + "/"):
                    require(self.registries.digest(image) == image.rsplit("@", 1)[1], "Rollback GHCR image is unavailable")
            plans.append((record, rows, self.stack_payload(current_source, record["before_images"])))
        self.report["status"] = "rolling_back"
        self.checkpoint()
        errors = []
        for record, rows, payload in reversed(plans):
            try:
                self.api.request("POST", "swarm/stacks", payload)
                for row in rows:
                    current = self.inspect(row["id"])
                    require(current["spec"]["TaskTemplate"]["ContainerSpec"]["Image"] == row["before_image"], "Rollback image mismatch")
                    tasks = self.wait(row["id"], current["spec"], restoring=True)
                    self.report["services"].append(dict(row, status="rolled_back", tasks=list(tasks), rollback_hash=fingerprint(current["spec"])))
                self.report["stacks"].append(dict(record, status="rolled_back"))
            except ReleaseError as error:
                errors.append(str(error))
                self.report["stacks"].append(dict(record, status="rollback_failed"))
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
            for plan in self.plans:
                self.deploy_stack(plan)
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
