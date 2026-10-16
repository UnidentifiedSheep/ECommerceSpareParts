#!/usr/bin/env python3
"""Provision pinned Arcane node agents from a temporary Swarm manager task.

Only the Docker socket and Swarm-mounted secrets are needed; no SSH or third-party
Python packages. API response bodies and deployment snippets contain credentials
and must never appear in errors or logs.
"""

import base64
import hashlib
import http.client
import json
import os
from pathlib import Path
import shlex
import socket
import sys
import time
from urllib.parse import quote, urlsplit


OWNER = "ecommerce.arcane.stack"
SPEC_HASH = "ecommerce.arcane.spec-hash"


class BootstrapError(Exception):
    pass


class UnixConnection(http.client.HTTPConnection):
    def __init__(self):
        super().__init__("localhost", timeout=15)

    def connect(self):
        self.sock = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        self.sock.settimeout(self.timeout)
        self.sock.connect("/var/run/docker.sock")


def request(connection, method, path, body=None, headers=None, allow_missing=False):
    try:
        connection.request(
            method, path,
            body=None if body is None else json.dumps(body),
            headers={"Content-Type": "application/json", **(headers or {})},
        )
        response = connection.getresponse()
        payload = response.read()
        if allow_missing and response.status == 404:
            return None
        if not 200 <= response.status < 300:
            raise BootstrapError(f"{method} {path.split('?')[0]}: HTTP {response.status}")
        return json.loads(payload) if payload else {}
    except (OSError, http.client.HTTPException, ValueError) as error:
        # Never include exception messages, response bodies, or request headers.
        raise BootstrapError(f"{method} {path.split('?')[0]}: {type(error).__name__}") from None
    finally:
        connection.close()


class Docker:
    def call(self, method, path, body=None, allow_missing=False):
        return request(UnixConnection(), method, path, body, allow_missing=allow_missing)


class Arcane:
    def __init__(self, url, api_key):
        self.url = urlsplit(url)
        self.headers = {"X-API-Key": api_key}

    def call(self, method, path, body=None):
        cls = http.client.HTTPSConnection if self.url.scheme == "https" else http.client.HTTPConnection
        return request(cls(self.url.hostname, self.url.port, timeout=15), method,
                       self.url.path.rstrip("/") + "/api" + path, body, self.headers)


def token_from_snippet(snippet):
    # Parse the pinned API's docker-run snippet as data; never execute it.
    tokens = [part.removeprefix("AGENT_TOKEN=")
              for part in shlex.split(snippet.replace("\\\n", " "))
              if part.startswith("AGENT_TOKEN=")]
    if len(tokens) != 1 or not tokens[0] or any(c.isspace() for c in tokens[0]):
        raise BootstrapError("Arcane returned an unsupported agent deployment snippet")
    return tokens[0]


def secret_ref(secret, target):
    return {"SecretID": secret["ID"], "SecretName": secret["Spec"]["Name"],
            "File": {"Name": target, "UID": "65532", "GID": "65532", "Mode": 0o400}}


def ensure_token_secret(docker, stack, node_id, token):
    digest = hashlib.sha256(token.encode()).hexdigest()[:16]
    name = f"{stack[:12]}_arcane_{node_id}_{digest}"
    path = "/secrets/" + quote(name, safe="")
    secret = docker.call("GET", path, allow_missing=True)
    if secret is None:
        docker.call("POST", "/secrets/create", {
            "Name": name, "Labels": {OWNER: stack, "ecommerce.arcane.node": node_id},
            "Data": base64.b64encode(token.encode()).decode(),
        })
        secret = docker.call("GET", path)
    if secret["Spec"].get("Labels", {}).get(OWNER) != stack:
        raise BootstrapError(f"Refusing to reuse an unowned agent secret for node {node_id}")
    return secret


def ensure_agent(docker, stack, node_id, token_secret, encryption_secret, network_id, image, volume_prefix, manager_url):
    name = f"{stack}-arcane-agent-{node_id}"
    spec = {
        "Name": name,
        "Labels": {OWNER: stack, "ecommerce.arcane.node": node_id},
        "TaskTemplate": {
            "ContainerSpec": {
                "Image": image,
                "Env": ["AGENT_MODE=true", "EDGE_AGENT=true", "EDGE_TRANSPORT=websocket",
                        "PORT=3553", f"MANAGER_API_URL={manager_url}",
                        "AGENT_TOKEN_FILE=/run/secrets/arcane_agent_token",
                        "ENCRYPTION_KEY_FILE=/run/secrets/arcane_encryption_key"],
                "Mounts": [
                    {"Type": "bind", "Source": "/var/run/docker.sock", "Target": "/var/run/docker.sock"},
                    {"Type": "volume", "Source": f"{volume_prefix}_arcane_agent_data", "Target": "/app/data"},
                ],
                "Secrets": [secret_ref(token_secret, "arcane_agent_token"),
                            secret_ref(encryption_secret, "arcane_encryption_key")],
                "Healthcheck": {"Test": ["CMD", "/app/arcane-agent", "health", "--timeout", "2s"],
                                "Interval": 15_000_000_000, "Timeout": 5_000_000_000,
                                "Retries": 3, "StartPeriod": 40_000_000_000},
            },
            "Placement": {"Constraints": [f"node.id == {node_id}"]},
            "Networks": [{"Target": network_id}],
            "RestartPolicy": {"Condition": "any", "Delay": 5_000_000_000},
        },
        "Mode": {"Replicated": {"Replicas": 1}},
        "UpdateConfig": {"Parallelism": 1, "Order": "stop-first", "FailureAction": "rollback"},
    }
    digest = hashlib.sha256(json.dumps(spec, sort_keys=True).encode()).hexdigest()
    spec["Labels"][SPEC_HASH] = digest
    existing = docker.call("GET", "/services/" + name, allow_missing=True)
    if existing is None:
        docker.call("POST", "/services/create", spec)
    else:
        labels = existing["Spec"].get("Labels", {})
        if labels.get(OWNER) != stack:
            raise BootstrapError(f"Refusing to replace an unowned service for node {node_id}")
        if labels.get(SPEC_HASH) != digest:
            docker.call("POST", f"/services/{existing['ID']}/update?version={existing['Version']['Index']}", spec)


def initialize_password(arcane, password):
    users = arcane.call("GET", "/users?start=0&limit=100")["data"]
    for user in users:
        if user["username"] == "arcane" and user.get("requiresPasswordChange"):
            arcane.call("PUT", "/users/" + quote(user["id"], safe=""), {"password": password})
            print("Initial Arcane administrator password configured.", flush=True)


def bootstrap(docker, arcane, settings):
    deadline = time.monotonic() + 120
    while True:
        try:
            initialize_password(arcane, settings["initial_password"])
            break
        except BootstrapError:
            if time.monotonic() >= deadline:
                raise
            time.sleep(2)

    encryption = docker.call("GET", "/secrets/" + quote(settings["encryption_secret"], safe=""))
    network = docker.call("GET", "/networks/" + quote(settings["network"], safe=""))
    pending = []
    for node in docker.call("GET", "/nodes"):
        node_id = node["ID"]
        hostname = node["Description"]["Hostname"]
        if node_id == settings["manager_node"]:
            continue
        if (node["Status"]["State"] != "ready" or node["Spec"]["Availability"] != "active"
                or node["Description"]["Platform"]["OS"] != "linux"):
            print(f"Skipping ineligible node {hostname} ({node_id}).", flush=True)
            continue
        deployment = arcane.call("POST", f"/environments/0/swarm/nodes/{node_id}/agent/deployment",
                                 {"rotate": False})["data"]
        token = token_from_snippet(deployment["dockerRun"])
        secret = ensure_token_secret(docker, settings["stack"], node_id, token)
        ensure_agent(docker, settings["stack"], node_id, secret, encryption, network["Id"],
                     settings["image"], settings["volume_prefix"], settings["manager_url"])
        pending.append((node_id, hostname))
        print(f"Agent service prepared for {hostname} ({node_id}).", flush=True)

    deadline = time.monotonic() + 180
    while pending:
        for node_id, hostname in pending[:]:
            status = arcane.call("GET", f"/environments/0/swarm/nodes/{node_id}")["data"].get("agent", {}).get("state")
            if status == "connected":
                pending.remove((node_id, hostname))
                print(f"Agent connected: {hostname} ({node_id}).", flush=True)
            elif status in ("mismatched", "ambiguous"):
                raise BootstrapError(f"Agent identity check failed for {hostname}: {status}")
        if pending and time.monotonic() >= deadline:
            raise BootstrapError("Agents did not connect: " + ", ".join(name for _, name in pending))
        if pending:
            time.sleep(3)
    print("Arcane bootstrap complete; all eligible nodes are covered.", flush=True)


def main():
    manager_url = "http://arcane:3552"
    bootstrap(Docker(), Arcane(manager_url, Path("/run/secrets/arcane_admin_api_key").read_text().strip()), {
        "initial_password": Path("/run/secrets/arcane_initial_password").read_text().strip(),
        "stack": os.environ["STACK_NAME"], "manager_node": os.environ["ARCANE_MANAGER_NODE"],
        "encryption_secret": os.environ["ARCANE_ENCRYPTION_SECRET"],
        "network": os.environ["ARCANE_AGENT_NETWORK"], "image": os.environ["ARCANE_AGENT_IMAGE"],
        "volume_prefix": os.environ["VOLUME_PREFIX"], "manager_url": manager_url,
    })


if __name__ == "__main__":
    try:
        main()
    except Exception as error:
        print(str(error) if isinstance(error, BootstrapError) else f"Arcane bootstrap failed: {type(error).__name__}",
              file=sys.stderr)
        sys.exit(1)
