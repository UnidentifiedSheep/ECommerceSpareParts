#!/usr/bin/env python3
"""Remove old CI images through Registry V2; preview unless --apply is given."""

import argparse
import base64
from datetime import datetime, timedelta, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import time
from urllib.error import HTTPError, URLError
from urllib.parse import quote, urlencode, urljoin, urlsplit
from urllib.request import HTTPRedirectHandler, Request, build_opener, parse_http_list, parse_keqv_list


SHA = re.compile(r"[0-9a-f]{40}\Z")
DIGEST = re.compile(r"sha256:[0-9a-f]{64}\Z")
ACCEPT = ", ".join((
    "application/vnd.oci.image.index.v1+json",
    "application/vnd.oci.image.manifest.v1+json",
    "application/vnd.docker.distribution.manifest.list.v2+json",
    "application/vnd.docker.distribution.manifest.v2+json",
))


class CleanupError(Exception):
    pass


class NoRedirect(HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


def http(method, url, headers=None, allow_missing=False):
    """Never forward credentials through a redirect or log response bodies."""
    opener = build_opener(NoRedirect())
    for attempt in range(3):
        try:
            with opener.open(Request(url, method=method, headers=headers or {}), timeout=30) as response:
                return response.status, response.headers, response.read()
        except HTTPError as error:
            if error.code == 404 and allow_missing:
                return 404, error.headers, b""
            if error.code in (401, 301, 302, 303, 307, 308):
                return error.code, error.headers, b""
            if error.code not in (429, 500, 502, 503, 504) or attempt == 2:
                raise CleanupError(f"{method} {urlsplit(url).path}: HTTP {error.code}") from None
        except (OSError, URLError):
            if attempt == 2:
                raise CleanupError(f"{method} {urlsplit(url).path}: connection failed") from None
        time.sleep(2 ** attempt)
    raise CleanupError("Request retry limit exceeded")


def document(body):
    try:
        return json.loads(body)
    except (ValueError, UnicodeError):
        raise CleanupError("Server returned invalid JSON") from None


class Registry:
    def __init__(self, host, username, password):
        if not re.fullmatch(r"[a-zA-Z0-9.-]+(?::[0-9]+)?", host):
            raise CleanupError("Registry host must contain only a hostname and optional port")
        self.base = "https://" + host
        self.basic = "Basic " + base64.b64encode(f"{username}:{password}".encode()).decode()
        self.tokens = {}

    def request(self, method, path, repository, allow_missing=False, allow_redirect=False, accept=ACCEPT):
        url = urljoin(self.base + "/", path)
        if urlsplit(url).netloc != urlsplit(self.base).netloc or urlsplit(url).scheme != urlsplit(self.base).scheme:
            raise CleanupError("Registry returned a link outside its own origin")
        headers = {"Accept": accept, "Authorization": self.tokens.get(repository, self.basic)}
        status, response_headers, body = http(method, url, headers, allow_missing)
        if status == 401:
            challenge = response_headers.get("WWW-Authenticate", "")
            scheme, _, parameters = challenge.partition(" ")
            if scheme.lower() != "bearer":
                raise CleanupError("Registry rejected credentials")
            values = parse_keqv_list(parse_http_list(parameters))
            realm = values.get("realm", "")
            if urlsplit(realm).scheme != "https":
                raise CleanupError("Registry authentication requires an HTTPS token endpoint")
            query = {"scope": f"repository:{repository}:pull,delete"}
            if values.get("service"):
                query["service"] = values["service"]
            token_url = realm + ("&" if "?" in realm else "?") + urlencode(query)
            token_status, _, token_body = http("GET", token_url, {"Authorization": self.basic})
            if token_status != 200:
                raise CleanupError("Registry token authentication failed")
            payload = document(token_body)
            token = payload.get("token") or payload.get("access_token")
            if not isinstance(token, str) or not token:
                raise CleanupError("Registry did not return an access token")
            self.tokens[repository] = "Bearer " + token
            headers["Authorization"] = self.tokens[repository]
            status, response_headers, body = http(method, url, headers, allow_missing)
        if status != 404 and not 200 <= status < 300 and not (allow_redirect and status in (301, 302, 303, 307, 308)):
            raise CleanupError(f"Registry request failed: HTTP {status}")
        return status, response_headers, body

    def tags(self, repository):
        path = f"/v2/{quote(repository, safe='/')}/tags/list?n=100"
        tags, visited = set(), set()
        while path:
            if path in visited:
                raise CleanupError("Registry repeated a pagination link")
            visited.add(path)
            status, headers, body = self.request("GET", path, repository, allow_missing=True)
            if status == 404:
                return []
            values = document(body).get("tags") or []
            if not isinstance(values, list) or not all(isinstance(tag, str) for tag in values):
                raise CleanupError("Registry returned an invalid tag list")
            tags.update(values)
            links = re.findall(r'<([^>]+)>;\s*rel="?next"?', headers.get("Link", ""))
            path = links[0] if links else None
        return sorted(tags)

    def manifest(self, repository, reference):
        path = f"/v2/{quote(repository, safe='/')}/manifests/{quote(reference, safe=':')}"
        status, headers, body = self.request("GET", path, repository, allow_missing=True)
        if status == 404:
            raise CleanupError(f"Manifest disappeared in {repository}; retry on the next deployment")
        digest = "sha256:" + hashlib.sha256(body).hexdigest()
        advertised = headers.get("Docker-Content-Digest", digest)
        if advertised != digest or (DIGEST.fullmatch(reference) and reference != digest):
            raise CleanupError("Registry manifest digest does not match its content")
        payload = document(body)
        if payload.get("schemaVersion") != 2:
            raise CleanupError("Cleanup supports only Docker schema 2 and OCI manifests")
        return digest, payload

    def created(self, repository, config_digest):
        if not DIGEST.fullmatch(config_digest):
            raise CleanupError("Registry returned an invalid config digest")
        path = f"/v2/{quote(repository, safe='/')}/blobs/{config_digest}"
        status, headers, body = self.request("GET", path, repository, allow_redirect=True,
                                             accept="application/octet-stream, application/json")
        location = self.base + path
        for _ in range(5):
            if status not in (301, 302, 303, 307, 308):
                break
            # Blob storage may redirect to a signed URL. Send no registry credentials.
            location = urljoin(location, headers.get("Location", ""))
            if urlsplit(location).scheme != "https":
                raise CleanupError("Registry blob redirect must use HTTPS")
            status, headers, body = http("GET", location)
        if status != 200:
            raise CleanupError("Unable to read image creation time")
        if "sha256:" + hashlib.sha256(body).hexdigest() != config_digest:
            raise CleanupError("Image config digest does not match its content")
        try:
            value = document(body).get("created", "")
            created = datetime.fromisoformat(value.replace("Z", "+00:00"))
            return created if created.tzinfo else None
        except (ValueError, TypeError, AttributeError):
            return None

    def delete(self, repository, digest):
        if not DIGEST.fullmatch(digest):
            raise CleanupError("Deletion requires a sha256 manifest digest")
        self.request("DELETE", f"/v2/{quote(repository, safe='/')}/manifests/{digest}", repository, allow_missing=True)


def project_repositories(root):
    repositories = set()
    pattern = re.compile(r"\s*image:\s*\$\{ARTIFACT_REGISTRY_HOST\}/([a-z0-9][a-z0-9._/-]*):\$\{IMAGE_TAG\}\s*")
    for filename in ("compose.registry.yaml", "migrator-compose.registry.yaml"):
        for line in (root / filename).read_text().splitlines():
            if re.match(r"\s*image:", line):
                match = pattern.fullmatch(line)
                if not match:
                    raise CleanupError(f"Unsupported project image declaration in {filename}")
                repositories.add(match[1])
    if not repositories:
        raise CleanupError("No project repositories found; refusing cleanup")
    return sorted(repositories)


def ancestor(root, commit, deployed):
    if not SHA.fullmatch(commit):
        return False
    result = subprocess.run(["git", "merge-base", "--is-ancestor", commit, deployed], cwd=root,
                            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    if result.returncode not in (0, 1, 128):
        raise CleanupError("Unable to determine commit ancestry")
    # Unknown commits are kept: they can belong to an upcoming release.
    return result.returncode == 0


class GitHub:
    def __init__(self, repository, token, run_id):
        if not re.fullmatch(r"[a-zA-Z0-9_.-]+/[a-zA-Z0-9_.-]+", repository):
            raise CleanupError("Invalid GitHub repository")
        self.base = f"https://api.github.com/repos/{repository}/actions/runs"
        self.headers = {"Authorization": "Bearer " + token, "Accept": "application/vnd.github+json",
                        "X-GitHub-Api-Version": "2022-11-28", "Cache-Control": "no-cache"}
        self.run_id = str(run_id)
        self.next_check = 0

    def busy(self):
        if time.monotonic() < self.next_check:
            return False
        for state in ("queued", "in_progress", "waiting", "pending", "requested"):
            status, _, body = http("GET", self.base + "?" + urlencode({"status": state, "per_page": 100}), self.headers)
            if status != 200:
                raise CleanupError("Unable to verify running GitHub workflows; cleanup stopped")
            runs = document(body).get("workflow_runs")
            if not isinstance(runs, list):
                raise CleanupError("GitHub returned an invalid workflow run list")
            if any(str(run["id"]) != self.run_id for run in runs):
                return True
        self.next_check = time.monotonic() + 15
        return False


def snapshot(registry, repository):
    return {tag: registry.manifest(repository, tag)[0] for tag in registry.tags(repository)}


def plan(registry, repository, root, deployed, cutoff):
    tags = registry.tags(repository)
    if "current" not in tags:
        print(f"{repository}: skipped (no current tag).", flush=True)
        return {}, [], set()
    manifests, mapping = {}, {}
    for tag in tags:
        digest, payload = registry.manifest(repository, tag)
        mapping[tag] = digest
        manifests[digest] = payload

    def closure(digest, visiting=None):
        visiting = set() if visiting is None else visiting
        if digest in visiting:
            raise CleanupError("Cyclic manifest references")
        if not DIGEST.fullmatch(digest):
            raise CleanupError("Invalid child manifest digest")
        if digest not in manifests:
            resolved, manifests[digest] = registry.manifest(repository, digest)
            if resolved != digest:
                raise CleanupError("Child manifest digest mismatch")
        result = {digest}
        payload = manifests[digest]
        children = list(payload.get("manifests", []))
        if payload.get("subject"):
            children.append(payload["subject"])
        for child in children:
            result.update(closure(child["digest"], visiting | {digest}))
        return result

    graphs = {digest: closure(digest) for digest in set(mapping.values())}
    protected, candidates = set(), set()
    for tag, digest in mapping.items():
        if not SHA.fullmatch(tag) or not ancestor(root, tag, deployed) or tag == deployed:
            protected.update(graphs[digest])
        else:
            candidates.add(digest)

    for digest in candidates.copy():
        dates = []
        for child in graphs[digest]:
            payload = manifests[child]
            if "manifests" not in payload:
                config = payload.get("config", {})
                if config.get("mediaType") not in ("application/vnd.oci.image.config.v1+json",
                                                   "application/vnd.docker.container.image.v1+json"):
                    dates.append(None)
                else:
                    dates.append(registry.created(repository, config["digest"]))
        if not dates or any(date is None or date > cutoff for date in dates):
            protected.update(graphs[digest])

    # Delete tagged roots only. Never directly delete layers, configs, or untagged children.
    return mapping, sorted(candidates - protected), protected


def cleanup(registry, github, repositories, root, deployed, min_age_hours, apply=False):
    cutoff = datetime.now(timezone.utc) - timedelta(hours=min_age_hours)
    deleted = 0
    if github.busy():
        print("Cleanup deferred: another GitHub workflow is queued or running.", flush=True)
        return deleted
    for repository in repositories:
        mapping, candidates, protected = plan(registry, repository, root, deployed, cutoff)
        print(f"{repository}: {len(candidates)} old manifest(s) eligible; {len(protected)} protected.", flush=True)
        if not candidates:
            continue
        if snapshot(registry, repository) != mapping:
            raise CleanupError(f"Tags changed in {repository}; cleanup stopped before deletion")
        for digest in candidates:
            if github.busy():
                print("Cleanup deferred: another GitHub workflow started.", flush=True)
                return deleted
            # CD uses one concurrency group. Also detect out-of-band promotion in Arcane.
            for tag in ("current", "previous"):
                if tag in mapping and registry.manifest(repository, tag)[0] != mapping[tag]:
                    raise CleanupError(f"Protected tag changed in {repository}; cleanup stopped")
            aliases = [tag for tag, value in mapping.items() if value == digest]
            print(f"{'DELETE' if apply else 'WOULD DELETE'} {repository}@{digest} ({', '.join(aliases)})", flush=True)
            if apply:
                registry.delete(repository, digest)
                deleted += 1
    print(f"Cleanup complete: {deleted} manifest(s) deleted." if apply else "Preview complete; registry was not modified.", flush=True)
    return deleted


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--deployed-sha", required=True)
    parser.add_argument("--min-age-hours", type=int, default=168)
    parser.add_argument("--apply", action="store_true", help="Delete eligible manifests; default is preview")
    args = parser.parse_args()
    if not SHA.fullmatch(args.deployed_sha) or args.min_age_hours < 1:
        parser.error("Use a full deployed commit SHA and a minimum age of at least one hour")
    required = ("ARTIFACT_REGISTRY_HOST", "ARTIFACT_REGISTRY_USERNAME", "ARTIFACT_REGISTRY_PASSWORD",
                "GITHUB_REPOSITORY", "GITHUB_TOKEN", "GITHUB_RUN_ID")
    for key in required:
        if not os.environ.get(key):
            raise CleanupError(f"{key} is required")
    root = Path(__file__).resolve().parents[2]
    result = subprocess.run(["git", "cat-file", "-e", args.deployed_sha + "^{commit}"], cwd=root,
                            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    if result.returncode:
        raise CleanupError("Deployed commit is unavailable; checkout must fetch the full history")
    registry = Registry(*(os.environ[key] for key in required[:3]))
    github = GitHub(*(os.environ[key] for key in required[3:]))
    cleanup(registry, github, project_repositories(root), root, args.deployed_sha, args.min_age_hours, args.apply)


if __name__ == "__main__":
    try:
        main()
    except CleanupError as error:
        print(f"Registry cleanup stopped: {error}", file=sys.stderr)
        sys.exit(1)
    except Exception as error:
        # Exceptions can contain HTTP authentication details; never print their messages.
        print(f"Registry cleanup stopped: {type(error).__name__}", file=sys.stderr)
        sys.exit(1)
