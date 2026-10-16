#!/usr/bin/env python3
"""Untag obsolete CI images; Cloud.ru Premium controls untagged image retention."""

import argparse
import base64
from concurrent.futures import Future, ThreadPoolExecutor, as_completed
from datetime import datetime, timezone
from functools import lru_cache
import getpass
import hashlib
from http.client import HTTPException
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import threading
import time
import traceback
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
    def __init__(self, message, status=None):
        super().__init__(message)
        self.status = status


class CleanupDeferred(Exception):
    pass


class Budget:
    def __init__(self, seconds):
        self.deadline = time.monotonic() + seconds

    def check(self):
        if time.monotonic() >= self.deadline:
            raise CleanupDeferred("Time budget reached; remaining tags will be reconsidered on the next run.")


def log(message):
    print(f"[{datetime.now(timezone.utc).strftime('%H:%M:%S')} UTC] {message}", flush=True)


def safe_error(error):
    if isinstance(error, (CleanupError, CleanupDeferred)):
        return str(error)
    frames = traceback.extract_tb(error.__traceback__)
    location = f" at {Path(frames[-1].filename).name}:{frames[-1].lineno} ({frames[-1].name})" if frames else ""
    # Unexpected exception messages can contain authentication details.
    return type(error).__name__ + location


class Report:
    def __init__(self, path=None, apply=False):
        self.path = Path(path) if path else None
        self.data = {"mode": "apply" if apply else "preview", "status": "running", "untagged": 0,
                     "already_absent": 0, "repositories": [], "error": None}

    def save(self):
        if self.path:
            self.path.write_text(json.dumps(self.data, indent=2) + "\n")

    def finish(self, status, error=None):
        self.data.update(status=status, error=error)
        self.save()
        if summary := os.environ.get("GITHUB_STEP_SUMMARY"):
            lines = ["## Registry cleanup", "", f"Status: **{status}**; mode: **{self.data['mode']}**; "
                     f"tags removed: **{self.data['untagged']}**; already absent: **{self.data['already_absent']}**.", "",
                     "| Repository | Eligible tags | Protected manifests | Tags removed |", "| --- | ---: | ---: | ---: |"]
            for row in self.data["repositories"]:
                lines.append(f"| {row['name']} | {row.get('eligible', '—')} | {row.get('protected', '—')} | {row['untagged']} |")
            if error:
                lines.extend(["", "Reason: " + error.replace("`", "'").replace("\n", " ")])
            if reference := self.data.get("last_untag"):
                lines.extend(["", f"Last DELETE request: `{reference}`."])
            with open(summary, "a") as stream:
                stream.write("\n".join(lines) + "\n")


def request_error(method, url, status, headers, body=b""):
    # Registry messages/detail may contain credentials or signed URLs. Keep only structured error codes.
    codes = []
    try:
        payload = json.loads(body)
        if isinstance(payload, dict):
            for error in payload.get("errors", []):
                code = error.get("code", "") if isinstance(error, dict) else ""
                if isinstance(code, str) and re.fullmatch(r"[A-Z][A-Z0-9_]{0,63}", code):
                    codes.append(code)
    except (ValueError, TypeError, AttributeError):
        pass
    detail = f"; registry codes: {', '.join(codes[:3])}" if codes else ""
    request_id = headers.get("X-Request-Id", "") or headers.get("X-Correlation-Id", "")
    if re.fullmatch(r"[a-zA-Z0-9._:-]{1,128}", request_id):
        detail += f"; request ID: {request_id}"
    if status in (401, 403):
        detail += "; verify registry credentials and tag-removal permission (Cloud.ru: ar.admin)"
    if status == 405:
        detail += "; registry endpoint does not allow this HTTP method"
    return CleanupError(f"{method} {urlsplit(url).path}: HTTP {status}{detail}", status=status)


def parallel(items, action, workers, phase, budget=None):
    items = list(items)
    result = {}
    log(f"{phase}: 0/{len(items)}")
    last_log = time.monotonic()
    with ThreadPoolExecutor(max_workers=workers) as executor:
        pending = {executor.submit(action, item): item for item in items}
        try:
            for future in as_completed(pending):
                if budget:
                    budget.check()
                result[pending[future]] = future.result()
                if len(result) % 50 == 0 or len(result) == len(items) or time.monotonic() - last_log >= 10:
                    log(f"{phase}: {len(result)}/{len(items)}")
                    last_log = time.monotonic()
        except BaseException:
            for future in pending:
                future.cancel()
            raise
    return result


class NoRedirect(HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


def http(method, url, headers=None, allow_missing=False, budget=None):
    """Never forward credentials through a redirect or log response bodies."""
    opener = build_opener(NoRedirect())
    for attempt in range(3):
        delay = 2 ** attempt
        if budget:
            budget.check()
        try:
            with opener.open(Request(url, method=method, headers=headers or {}), timeout=30) as response:
                return response.status, response.headers, response.read()
        except HTTPError as error:
            if error.code == 404 and allow_missing:
                error.close()
                return 404, error.headers, b""
            if error.code in (401, 301, 302, 303, 307, 308):
                error.close()
                return error.code, error.headers, b""
            try:
                body = error.read(65536)
            except (OSError, HTTPException):
                body = b""
            finally:
                error.close()
            failure = request_error(method, url, error.code, error.headers, body)
            if error.code not in (429, 500, 502, 503, 504) or attempt == 2:
                raise failure from None
            retry_after = error.headers.get("Retry-After", "")
            if retry_after.isdigit():
                delay = max(delay, min(int(retry_after), 30))
            log(f"Retry {attempt + 1}/3: {failure}")
        except (OSError, URLError, HTTPException) as error:
            reason = getattr(error, "reason", error)
            failure = f"{method} {urlsplit(url).path}: transport error ({type(reason).__name__})"
            if attempt == 2:
                raise CleanupError(failure) from None
            log(f"Retry {attempt + 1}/3: {failure}")
        time.sleep(delay)
    raise CleanupError("Request retry limit exceeded")


def document(body):
    try:
        return json.loads(body)
    except (ValueError, UnicodeError):
        raise CleanupError("Server returned invalid JSON") from None


class Registry:
    def __init__(self, host, username, password, budget=None):
        if not re.fullmatch(r"[a-zA-Z0-9.-]+(?::[0-9]+)?", host):
            raise CleanupError("Registry host must contain only a hostname and optional port")
        self.base = "https://" + host
        self.basic = "Basic " + base64.b64encode(f"{username}:{password}".encode()).decode()
        self.tokens = {}
        self.budget = budget
        self.token_lock = threading.Lock()
        self.cache_lock = threading.Lock()
        self.cache = {}

    def cached(self, key, action):
        with self.cache_lock:
            owner = key not in self.cache
            if owner:
                self.cache[key] = Future()
            future = self.cache[key]
        if owner:
            try:
                future.set_result(action())
            except BaseException as error:
                future.set_exception(error)
        return future.result()

    def request(self, method, path, repository, allow_missing=False):
        url = urljoin(self.base + "/", path)
        if urlsplit(url).netloc != urlsplit(self.base).netloc or urlsplit(url).scheme != urlsplit(self.base).scheme:
            raise CleanupError("Registry returned a link outside its own origin")
        headers = {"Accept": ACCEPT, "Authorization": self.tokens.get(repository, self.basic)}
        status, response_headers, body = http(method, url, headers, allow_missing, self.budget)
        if status == 401:
            challenge = response_headers.get("WWW-Authenticate", "")
            scheme, _, parameters = challenge.partition(" ")
            if scheme.lower() != "bearer":
                raise request_error(method, url, status, response_headers)
            values = parse_keqv_list(parse_http_list(parameters))
            realm = values.get("realm", "")
            if urlsplit(realm).scheme != "https":
                raise CleanupError("Registry authentication requires an HTTPS token endpoint")
            query = {"scope": f"repository:{repository}:pull,delete"}
            if values.get("service"):
                query["service"] = values["service"]
            token_url = realm + ("&" if "?" in realm else "?") + urlencode(query)
            with self.token_lock:
                if repository not in self.tokens or self.tokens[repository] == headers["Authorization"]:
                    token_status, _, token_body = http("GET", token_url, {"Authorization": self.basic}, budget=self.budget)
                    if token_status != 200:
                        raise CleanupError("Registry token authentication failed")
                    payload = document(token_body)
                    token = payload.get("token") or payload.get("access_token")
                    if not isinstance(token, str) or not token:
                        raise CleanupError("Registry did not return an access token")
                    self.tokens[repository] = "Bearer " + token
                headers["Authorization"] = self.tokens[repository]
            status, response_headers, body = http(method, url, headers, allow_missing, self.budget)
        if status != 404 and not 200 <= status < 300:
            raise request_error(method, url, status, response_headers)
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
        if DIGEST.fullmatch(reference):
            return self.cached(("manifest", repository, reference), lambda: self._manifest(repository, reference))
        return self._manifest(repository, reference)

    def _manifest(self, repository, reference, allow_missing=False):
        path = f"/v2/{quote(repository, safe='/')}/manifests/{quote(reference, safe=':')}"
        status, headers, body = self.request("GET", path, repository, allow_missing=True)
        if status == 404:
            if allow_missing:
                return None
            raise CleanupError(f"Manifest disappeared in {repository}; retry on the next deployment")
        digest = "sha256:" + hashlib.sha256(body).hexdigest()
        advertised = headers.get("Docker-Content-Digest", digest)
        if advertised != digest or (DIGEST.fullmatch(reference) and reference != digest):
            raise CleanupError("Registry manifest digest does not match its content")
        payload = document(body)
        if payload.get("schemaVersion") != 2:
            raise CleanupError("Cleanup supports only Docker schema 2 and OCI manifests")
        if not DIGEST.fullmatch(reference):
            self.cached(("manifest", repository, digest), lambda: (digest, payload))
        return digest, payload

    def reference_digest(self, repository, reference, allow_missing=False):
        path = f"/v2/{quote(repository, safe='/')}/manifests/{quote(reference, safe=':')}"
        try:
            status, headers, _ = self.request("HEAD", path, repository, allow_missing=allow_missing)
        except CleanupError as error:
            if error.status not in (405, 501):
                raise
        else:
            if status == 404:
                return None
            digest = headers.get("Docker-Content-Digest", "")
            if DIGEST.fullmatch(digest):
                return digest
        # Some registries omit the digest header or do not implement HEAD.
        resolved = self._manifest(repository, reference, allow_missing=allow_missing)
        return resolved[0] if resolved else None

    def untag(self, repository, tag):
        if not SHA.fullmatch(tag):
            raise CleanupError("Tag removal requires a full commit SHA tag")
        # OCI tag deletion removes a name only. Never fall back to deleting a digest.
        status, _, _ = self.request("DELETE", f"/v2/{quote(repository, safe='/')}/manifests/{tag}", repository, allow_missing=True)
        return status


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


@lru_cache(maxsize=4)
def ancestors(root, deployed):
    result = subprocess.run(["git", "rev-list", deployed, "--"], cwd=root, capture_output=True, text=True)
    if result.returncode:
        raise CleanupError("Unable to determine commit ancestry")
    return frozenset(result.stdout.splitlines())


def ancestor(root, commit, deployed):
    if not SHA.fullmatch(commit):
        return False
    # Unknown commits are kept: they can belong to an upcoming release.
    return commit in ancestors(root, deployed)


class GitHub:
    def __init__(self, repository, token, run_id, budget=None):
        if not re.fullmatch(r"[a-zA-Z0-9_.-]+/[a-zA-Z0-9_.-]+", repository):
            raise CleanupError("Invalid GitHub repository")
        self.base = f"https://api.github.com/repos/{repository}/actions/runs"
        self.headers = {"Authorization": "Bearer " + token, "Accept": "application/vnd.github+json",
                        "X-GitHub-Api-Version": "2022-11-28", "Cache-Control": "no-cache"}
        self.run_id = str(run_id)
        self.next_check = 0
        self.budget = budget

    def busy(self):
        if time.monotonic() < self.next_check:
            return False
        for state in ("queued", "in_progress", "waiting", "pending", "requested"):
            status, _, body = http("GET", self.base + "?" + urlencode({"status": state, "per_page": 100}), self.headers, budget=self.budget)
            if status != 200:
                raise CleanupError("Unable to verify running GitHub workflows; cleanup stopped")
            runs = document(body).get("workflow_runs")
            if not isinstance(runs, list):
                raise CleanupError("GitHub returned an invalid workflow run list")
            if any(str(run["id"]) != self.run_id for run in runs):
                return True
        self.next_check = time.monotonic() + 15
        return False


def snapshot(registry, repository, workers=8):
    return parallel(registry.tags(repository), lambda tag: registry.reference_digest(repository, tag), workers,
                    f"{repository}: verify tags", registry.budget)


def plan(registry, repository, root, deployed, workers=8):
    tags = registry.tags(repository)
    if "current" not in tags:
        print(f"{repository}: skipped (no current tag).", flush=True)
        return {}, [], set()
    manifests, mapping = {}, {}
    resolved = parallel(tags, lambda tag: registry.manifest(repository, tag), workers,
                        f"{repository}: read manifests", registry.budget)
    for tag, (digest, payload) in resolved.items():
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

    graphs = parallel(set(mapping.values()), closure, workers, f"{repository}: resolve manifest trees", registry.budget)
    protected, candidates = set(), set()
    for tag, digest in mapping.items():
        if not SHA.fullmatch(tag) or not ancestor(root, tag, deployed) or tag == deployed:
            protected.update(graphs[digest])
        else:
            candidates.add(tag)

    return mapping, sorted(tag for tag in candidates if mapping[tag] not in protected), protected


def cleanup(registry, github, repositories, root, deployed, apply=False,
            workers=8, max_tags=0, report=None):
    if apply and github is None:
        raise CleanupError("GitHub workflow checks are required before removing tags")
    report = report or Report(apply=apply)
    untagged = 0
    if github and github.busy():
        raise CleanupDeferred("Another GitHub workflow is queued or running.")
    for repository in repositories:
        if registry.budget:
            registry.budget.check()
        row = {"name": repository, "untagged": 0}
        report.data["repositories"].append(row)
        report.save()
        started = time.monotonic()
        mapping, candidates, protected = plan(registry, repository, root, deployed, workers)
        row.update(eligible=len(candidates), protected=len(protected))
        report.save()
        log(f"{repository}: {len(candidates)} obsolete tag(s) eligible; {len(protected)} manifest(s) protected; "
            f"planning took {time.monotonic() - started:.1f}s.")
        if not candidates:
            continue
        if snapshot(registry, repository, workers) != mapping:
            raise CleanupError(f"Tags changed in {repository}; cleanup stopped before removing tags")
        for index, tag in enumerate(candidates, 1):
            if registry.budget:
                registry.budget.check()
            if max_tags and untagged >= max_tags:
                raise CleanupDeferred(f"Tag removal limit of {max_tags} reached; remaining tags will be reconsidered on the next run.")
            if github and github.busy():
                raise CleanupDeferred("Another GitHub workflow started.")
            # CD uses one concurrency group. Also detect out-of-band promotion in Arcane.
            if apply:
                for protected_tag in ("current", "previous"):
                    if registry.reference_digest(repository, protected_tag, allow_missing=True) != mapping.get(protected_tag):
                        raise CleanupError(f"Protected tag changed in {repository}; cleanup stopped")
                actual = registry.reference_digest(repository, tag, allow_missing=True)
                if actual is not None and actual != mapping[tag]:
                    raise CleanupError(f"Candidate tag changed in {repository}; cleanup stopped")
            reference = f"{repository}:{tag}"
            if apply:
                report.data["last_untag"] = reference
                report.save()
                log(f"UNTAG {index}/{len(candidates)} {reference} ({mapping[tag]})")
                started = time.monotonic()
                status = registry.untag(repository, tag) if actual is not None else 404
                if status == 404:
                    report.data["already_absent"] += 1
                else:
                    untagged += 1
                    row["untagged"] += 1
                    report.data["untagged"] = untagged
                report.save()
                if registry.reference_digest(repository, tag, allow_missing=True) is not None:
                    raise CleanupError(f"Tag still resolves after removal in {repository}; cleanup stopped")
                log(f"{'ALREADY ABSENT' if status == 404 else 'UNTAGGED'} {reference}: HTTP {status}, "
                    f"{time.monotonic() - started:.1f}s; tags removed this run: {untagged}.")
            else:
                log(f"WOULD UNTAG {index}/{len(candidates)} {reference} ({mapping[tag]})")
    log(f"Cleanup complete: {untagged} tag(s) removed; image retention is managed by Cloud.ru Premium."
        if apply else "Preview complete; registry was not modified.")
    return untagged


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--deployed-sha", required=True, help="Full SHA of the successfully deployed release")
    parser.add_argument("--local", action="store_true", help="Prompt for missing registry credentials; preview needs no GitHub token")
    parser.add_argument("--repository", action="append", help="Limit cleanup to this project image repository; repeatable")
    parser.add_argument("--apply", action="store_true", help="Remove eligible SHA tags; default is preview")
    parser.add_argument("--workers", type=int, default=8, help="Concurrent read requests (1-32); tag removals stay sequential")
    parser.add_argument("--max-runtime-seconds", type=int, default=1200, help="Stop cleanly after this time budget")
    parser.add_argument("--max-tags", "--max-deletions", dest="max_tags", type=int,
                        help="Maximum tags to remove; default: 1 locally, unlimited in CI; 0 means unlimited")
    parser.add_argument("--report", help="Write a progress/error report as JSON")
    args = parser.parse_args()
    if args.max_tags is None:
        args.max_tags = 1 if args.local else 0
    if not SHA.fullmatch(args.deployed_sha):
        parser.error("Use a full deployed commit SHA")
    if not 1 <= args.workers <= 32 or args.max_runtime_seconds < 1 or args.max_tags < 0:
        parser.error("Use 1-32 workers, a positive runtime, and a nonnegative tag removal limit")
    report = Report(args.report, args.apply)
    try:
        report.save()
        return run_cleanup(args, report)
    except CleanupDeferred as error:
        log(f"Cleanup deferred: {error} Tags removed this run: {report.data['untagged']}.")
        report.finish("deferred", str(error))
        return 0
    except Exception as error:
        message = safe_error(error)
        log(f"Registry cleanup stopped: {message}; tags removed before failure: {report.data['untagged']}.")
        if os.environ.get("GITHUB_ACTIONS") == "true":
            escaped = message.replace("%", "%25").replace("\r", "%0D").replace("\n", "%0A")
            print(f"::error title=Registry cleanup failed::{escaped}", flush=True)
        report.finish("failed", message)
        return 1


def required_environment(*keys):
    values = []
    for key in keys:
        value = os.environ.get(key)
        if not value:
            raise CleanupError(f"{key} is required")
        values.append(value)
    return values


def registry_credentials(local):
    required = ("ARTIFACT_REGISTRY_HOST", "ARTIFACT_REGISTRY_USERNAME", "ARTIFACT_REGISTRY_PASSWORD")
    if not local:
        return required_environment(*required)
    prompts = ("Registry host (without https://): ", "Registry username: ", "Registry password: ")
    values = []
    for key, prompt in zip(required, prompts):
        value = os.environ.get(key)
        if not value:
            if not sys.stdin.isatty():
                raise CleanupError(f"{key} is required; run --local in a terminal or set this environment variable")
            try:
                value = getpass.getpass(prompt) if key.endswith("PASSWORD") else input(prompt).strip()
            except (EOFError, KeyboardInterrupt):
                raise CleanupError("Registry credential input cancelled") from None
        if not value:
            raise CleanupError(f"{key} must not be empty")
        values.append(value)
    return values


def gh_output(root, arguments, failure):
    try:
        result = subprocess.run(["gh", *arguments], cwd=root, capture_output=True, text=True, timeout=30)
    except (OSError, subprocess.TimeoutExpired):
        raise CleanupError(failure) from None
    if result.returncode or not result.stdout.strip():
        raise CleanupError(failure)
    return result.stdout.strip()


def github_client(root, local, apply, budget):
    if not local:
        return GitHub(*required_environment("GITHUB_REPOSITORY", "GITHUB_TOKEN", "GITHUB_RUN_ID"), budget=budget)
    if not apply:
        log("Local preview: workflow checks skipped; no DELETE requests will be sent.")
        return None
    token = os.environ.get("GITHUB_TOKEN") or os.environ.get("GH_TOKEN")
    if not token:
        token = gh_output(root, ["auth", "token", "--hostname", "github.com"],
                          "Local tag removal requires GitHub access: run 'gh auth login' or set GITHUB_TOKEN")
    repository = os.environ.get("GITHUB_REPOSITORY")
    if not repository:
        repository = gh_output(root, ["repo", "view", "--json", "nameWithOwner", "--jq", ".nameWithOwner"],
                               "Unable to detect GitHub repository; set GITHUB_REPOSITORY=owner/repo")
    # Local invocations must consider every active workflow, even if CI variables are present.
    return GitHub(repository, token, run_id="", budget=budget)


def run_cleanup(args, report):
    root = Path(__file__).resolve().parents[2]
    repositories = project_repositories(root)
    if args.repository:
        if not set(args.repository) <= set(repositories):
            raise CleanupError("--repository must name an image repository from the project's compose files")
        repositories = sorted(set(args.repository))
    result = subprocess.run(["git", "cat-file", "-e", args.deployed_sha + "^{commit}"], cwd=root,
                            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    if result.returncode:
        raise CleanupError("Deployed commit is unavailable; checkout must fetch the full history")
    credentials = registry_credentials(args.local)
    budget = Budget(args.max_runtime_seconds)
    github = github_client(root, args.local, args.apply, budget)
    registry = Registry(*credentials, budget=budget)
    log(f"Starting registry cleanup: mode={'apply' if args.apply else 'preview'}, repositories={len(repositories)}, "
        f"read workers={args.workers}, time budget={args.max_runtime_seconds}s, "
        f"tag removal limit={args.max_tags or 'unlimited'}.")
    cleanup(registry, github, repositories, root, args.deployed_sha, args.apply,
            args.workers, args.max_tags, report)
    report.finish("complete" if args.apply else "preview")
    return 0


if __name__ == "__main__":
    sys.exit(main())
