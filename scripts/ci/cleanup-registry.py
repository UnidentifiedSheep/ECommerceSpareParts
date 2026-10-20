#!/usr/bin/env python3
"""Remove obsolete GHCR versions while preserving deployed and rollback images."""

import argparse
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import time
from urllib.error import HTTPError, URLError
from urllib.parse import quote, urlencode
from urllib.request import HTTPRedirectHandler, Request, build_opener

from image_catalog import catalog
from release import DIGEST, SHA, ReleaseError, require, save


class Deferred(ReleaseError):
    pass


class NoRedirect(HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


def log(message):
    print(f"[{datetime.now(timezone.utc):%H:%M:%S UTC}] {message}", flush=True)


class GitHub:
    def __init__(self, repository, token, run_id, seconds):
        require(re.fullmatch(r"[\w.-]+/[\w.-]+", repository), "Invalid GitHub repository")
        require(token, "GITHUB_TOKEN is required")
        self.repository, self.run_id = repository, str(run_id)
        self.owner = repository.split("/")[0]
        self.headers = {"Authorization": "Bearer " + token, "Accept": "application/vnd.github+json",
                        "X-GitHub-Api-Version": "2022-11-28", "Cache-Control": "no-cache"}
        self.opener = build_opener(NoRedirect())
        self.deadline, self.next_check = time.monotonic() + seconds, 0
        self.owner_kind = None

    def check_time(self):
        if time.monotonic() >= self.deadline:
            raise Deferred("Cleanup time budget reached; remaining versions will be checked on the next run")

    def request(self, method, path, missing=False):
        self.check_time()
        for attempt in range(4):
            try:
                with self.opener.open(Request("https://api.github.com/" + path, headers=self.headers, method=method), timeout=30) as response:
                    return json.load(response) if response.status != 204 else True
            except HTTPError as error:
                status, request_id = error.code, error.headers.get("X-GitHub-Request-Id", "")
                limited = error.headers.get("X-RateLimit-Remaining") == "0"
                error.close()
                if status == 404 and missing:
                    return None
                if method != "GET" or status not in (429, 500, 502, 503, 504) or attempt == 3:
                    suffix = "; rate limit exhausted" if limited else ""
                    if re.fullmatch(r"[a-zA-Z0-9:-]+", request_id):
                        suffix += "; request ID: " + request_id
                    raise ReleaseError(f"GitHub {method} {path.split('?')[0]}: HTTP {status}{suffix}") from None
            except (OSError, URLError, ValueError) as error:
                if method != "GET" or attempt == 3:
                    raise ReleaseError(f"GitHub {method} failed ({type(error).__name__})") from None
            time.sleep(2 ** attempt)

    def busy(self):
        if time.monotonic() < self.next_check:
            return False
        for state in ("queued", "in_progress", "waiting", "pending", "requested"):
            data = self.request("GET", f"repos/{self.repository}/actions/runs?" + urlencode({"status": state, "per_page": 100}))
            runs = data.get("workflow_runs")
            require(isinstance(runs, list), "Invalid GitHub workflow list")
            if any(str(run["id"]) != self.run_id for run in runs):
                return True
        self.next_check = time.monotonic() + 15
        return False

    def package_path(self, name):
        if self.owner_kind is None:
            owner = self.request("GET", "users/" + quote(self.owner, safe=""))
            require(owner.get("type") in ("User", "Organization"), "Unsupported GitHub package owner")
            self.owner_kind = "orgs" if owner["type"] == "Organization" else "users"
        return f"{self.owner_kind}/{quote(self.owner, safe='')}/packages/container/{quote(name, safe='')}"

    def versions(self, name):
        path = self.package_path(name)
        package = self.request("GET", path, missing=True)
        if package is None:
            return []
        require((package.get("repository") or {}).get("full_name", "").lower() == self.repository.lower(),
                "GHCR package is not linked to this repository; cleanup stopped")
        rows = []
        for page in range(1, 1001):
            batch = self.request("GET", path + "/versions?" + urlencode({"state": "active", "per_page": 100, "page": page}))
            require(isinstance(batch, list), "Invalid GHCR version list")
            rows.extend(batch)
            if len(batch) < 100:
                return rows
        raise ReleaseError("GHCR pagination limit exceeded")


def tags(row):
    value = row.get("metadata", {}).get("container", {}).get("tags")
    require(isinstance(value, list) and all(isinstance(t, str) for t in value), "Invalid GHCR tags")
    return set(value)


def eligible(row, ancestors, deployed):
    names = tags(row)
    # Untagged manifests can be children of multi-platform images. Keep them.
    return bool(names) and DIGEST.fullmatch(row.get("name", "")) and all(
        SHA.fullmatch(tag) and tag in ancestors and tag != deployed for tag in names)


def cleanup(github, names, ancestors, deployed, report, output, apply=False, workers=8, limit=0):
    if github.busy():
        raise Deferred("Another GitHub workflow is active; cleanup deferred")
    # Resolve owner kind once before the independent repository reads.
    github.package_path(names[0])
    with ThreadPoolExecutor(max_workers=workers) as pool:
        snapshots = list(pool.map(github.versions, names))
    count = 0
    for name, versions in zip(names, snapshots):
        candidates = [row for row in versions if eligible(row, ancestors, deployed)]
        row = {"name": name, "versions": len(versions), "eligible": len(candidates), "deleted": 0}
        report["repositories"].append(row)
        save(output, report)
        log(f"{name}: {len(candidates)} obsolete version(s), {len(versions) - len(candidates)} protected.")
        for candidate in candidates:
            github.check_time()
            if limit and count >= limit:
                raise Deferred("Deletion limit reached")
            if github.busy():
                raise Deferred("Another GitHub workflow started; cleanup deferred")
            version_id = candidate.get("id")
            require(type(version_id) is int and version_id > 0, "Invalid GHCR version ID")
            path = github.package_path(name) + "/versions/" + str(version_id)
            current = github.request("GET", path, missing=True)
            if current is None:
                continue
            if current["name"] != candidate["name"] or not eligible(current, ancestors, deployed):
                raise Deferred("GHCR version tags changed; cleanup deferred")
            log(f"{'DELETE' if apply else 'PREVIEW'} {name}@{current['name']}")
            if apply:
                try:
                    github.request("DELETE", path)
                except ReleaseError:
                    # A lost response may mean deletion succeeded. Never blindly repeat it.
                    if github.request("GET", path, missing=True) is not None:
                        raise
                row["deleted"] += 1
                report["deleted"] += 1
                save(output, report)
            count += 1


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--deployed-sha", required=True)
    parser.add_argument("--apply", action="store_true")
    parser.add_argument("--workers", type=int, default=8)
    parser.add_argument("--max-runtime-seconds", type=int, default=1200)
    parser.add_argument("--max-versions", type=int, default=0)
    parser.add_argument("--report", default="registry-cleanup-report.json")
    args = parser.parse_args()
    require(SHA.fullmatch(args.deployed_sha) and 1 <= args.workers <= 16
            and 60 <= args.max_runtime_seconds <= 1500 and args.max_versions >= 0, "Invalid cleanup settings")
    result = subprocess.run(["git", "rev-list", args.deployed_sha, "--"], capture_output=True, text=True, timeout=60)
    require(result.returncode == 0, "Cannot determine deployed commit ancestry")
    ancestors = set(result.stdout.splitlines())
    github = GitHub(os.environ["GITHUB_REPOSITORY"], os.getenv("GH_TOKEN") or os.getenv("GITHUB_TOKEN", ""),
                    os.getenv("GITHUB_RUN_ID", "local"), args.max_runtime_seconds)
    report = {"version": 1, "registry": "ghcr.io/" + github.owner.lower(), "mode": "apply" if args.apply else "preview",
              "status": "running", "deleted": 0, "repositories": []}
    save(args.report, report)
    try:
        cleanup(github, [row["image"] for row in catalog()], ancestors, args.deployed_sha, report, args.report,
                args.apply, args.workers, args.max_versions)
        report["status"] = "completed"
    except Deferred as error:
        report.update(status="deferred", reason=str(error))
        log(str(error))
    except BaseException as error:
        report.update(status="failed", reason=str(error) if isinstance(error, ReleaseError) else type(error).__name__)
        raise
    finally:
        save(args.report, report)


if __name__ == "__main__":
    try:
        main()
    except (ReleaseError, ValueError, KeyError, OSError, TypeError) as error:
        print(f"Cleanup failed: {error if isinstance(error, ReleaseError) else type(error).__name__}", file=sys.stderr)
        sys.exit(1)
