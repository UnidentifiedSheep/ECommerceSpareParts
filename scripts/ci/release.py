#!/usr/bin/env python3
"""Validate CI artifacts and move registry aliases after a successful deployment."""

import argparse
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import tempfile
import time

from image_catalog import catalog

SHA = re.compile(r"[0-9a-f]{40}")
DIGEST = re.compile(r"sha256:[0-9a-f]{64}")

def registry_prefix():
    owner = os.environ.get("GITHUB_REPOSITORY_OWNER") or os.environ.get("GITHUB_REPOSITORY", "").split("/")[0]
    require(re.fullmatch(r"[a-zA-Z0-9][a-zA-Z0-9-]*", owner), "GitHub repository owner is required")
    return "ghcr.io/" + owner.lower()


class ReleaseError(RuntimeError):
    pass


def require(condition, message):
    if not condition:
        raise ReleaseError(message)


def save(path, data):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    fd, temporary = tempfile.mkstemp(dir=path.parent, prefix=path.name + ".")
    try:
        with os.fdopen(fd, "w") as stream:
            json.dump(data, stream, indent=2)
            stream.write("\n")
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(temporary, path)
    finally:
        if os.path.exists(temporary):
            os.unlink(temporary)


def command(args, *, input=None, timeout=120, missing=False):
    for attempt in range(3):
        try:
            result = subprocess.run(args, input=input, capture_output=True, text=True, timeout=timeout)
        except (OSError, subprocess.TimeoutExpired) as error:
            if attempt == 2:
                raise ReleaseError(f"{args[0]} {args[1]} failed ({type(error).__name__})") from None
        else:
            if result.returncode == 0:
                return result.stdout
            if missing and re.search(r"manifest unknown|MANIFEST_UNKNOWN|404 Not Found|: not found", result.stderr):
                require(not re.search(r"unauthori[sz]ed|failed to authorize|oauth", result.stderr, re.I),
                        "Registry authorization failed")
                return None
            if attempt == 2:
                # Tool output can contain signed URLs or credentials; do not forward it to Actions logs.
                raise ReleaseError(f"{args[0]} {args[1]} failed (exit {result.returncode})")
        time.sleep(2 ** attempt)


def validate(manifest, sha=None):
    require(isinstance(manifest, dict) and manifest.get("version") == 3,
            "Unsupported release artifact; run the updated CI workflow first")
    require(set(manifest) <= {"version", "sha", "should_deploy", "registry", "images"}, "Unexpected release artifact fields")
    require(isinstance(manifest.get("sha"), str) and SHA.fullmatch(manifest["sha"]), "Invalid release SHA")
    require(sha is None or manifest["sha"] == sha, "CI SHA does not match the release artifact")
    require(type(manifest.get("should_deploy")) is bool, "Invalid deployment flag")
    images = manifest.get("images")
    require(isinstance(images, list), "Release images must be an array")
    known = {item["service"]: item for item in catalog()}
    seen = set()
    for item in images:
        require(isinstance(item, dict), "Invalid release image")
        require(set(item) == {"service", "image", "digest"}, "Unexpected release image fields")
        service = item.get("service")
        require(service in known and service not in seen, "Unknown or duplicate application image")
        require(item.get("image") == known[service]["image"], "Unexpected image repository")
        require(isinstance(item.get("digest"), str) and DIGEST.fullmatch(item["digest"]), "Invalid image digest")
        seen.add(service)
    require(not manifest["should_deploy"] or images, "Deployment has no images")
    require(manifest.get("registry") == registry_prefix(), "Release registry does not match this GitHub owner")
    return manifest


class Registries:
    """Runner-only GHCR access. Never forward the job token to Swarm."""

    def __init__(self):
        self.prefix = registry_prefix()
        self.logged_in = False

    def login(self, image):
        require(image.startswith(self.prefix + "/"), "Image is outside this project's GHCR namespace")
        if not self.logged_in:
            token = os.getenv("GH_TOKEN") or os.getenv("GITHUB_TOKEN", "")
            require(token, "GITHUB_TOKEN is required for GHCR")
            username = os.getenv("GITHUB_ACTOR") or self.prefix.split("/")[1]
            Path(os.environ["DOCKER_CONFIG"]).mkdir(mode=0o700, parents=True, exist_ok=True)
            command(["docker", "login", "ghcr.io", "--username", username, "--password-stdin"], input=token + "\n")
            self.logged_in = True

    def digest(self, image, missing=False):
        self.login(image)
        output = command(["docker", "buildx", "imagetools", "inspect", image], missing=missing)
        if output is None:
            return None
        match = re.search(r"^Digest:\s*(sha256:[0-9a-f]{64})\s*$", output, re.M)
        require(match is not None, "GHCR did not return a valid image digest")
        return match[1]

    def tag(self, image, digest, tag):
        require(DIGEST.fullmatch(digest) and tag in ("current", "previous"), "Invalid registry alias update")
        self.login(image)
        command(["docker", "buildx", "imagetools", "create", "--prefer-index=false",
                 "--tag", image + ":" + tag, image + "@" + digest])
        require(self.digest(image + ":" + tag) == digest, "GHCR alias does not match the expected digest")


def prepare_source(operation, run_id, output_file):
    require(re.fullmatch(r"[1-9][0-9]*", run_id), "A numeric GitHub Actions run ID is required")
    repository = os.environ["GITHUB_REPOSITORY"]
    require(re.fullmatch(r"[\w.-]+/[\w.-]+", repository), "Invalid GitHub repository")
    run = json.loads(command(["gh", "api", f"repos/{repository}/actions/runs/{run_id}"]))
    expected = "CD" if operation == "rollback" else "CI"
    require(run.get("name") == expected and run.get("head_repository", {}).get("full_name") == repository,
            f"The source must be a {expected} run from this repository")
    require(run.get("path", "").split("@")[0] == f".github/workflows/{expected.lower()}.yml",
            "Unexpected source workflow")
    if operation == "rollback":
        require(run.get("status") == "completed" and run.get("head_branch") == "master"
                and run.get("event") in ("workflow_run", "workflow_dispatch"),
                "Rollback requires a completed master CD run")
    else:
        require(run.get("conclusion") == "success" and run.get("head_branch") == "master"
                and run.get("event") in ("push", "workflow_dispatch"), "Production requires a successful master CI run")
    artifact = "arcane-deployment" if operation == "rollback" else "release-manifest"
    directory = Path("release-input")
    directory.mkdir(exist_ok=False)
    command(["gh", "run", "download", run_id, "--repo", repository, "--name", artifact, "--dir", str(directory)])
    filename = "deployment-report.json" if operation == "rollback" else "release-manifest.json"
    data = json.loads((directory / filename).read_text())
    if operation == "rollback":
        require(data.get("version") == 2 and str(data.get("run_id")) == run_id
                and data.get("repository") == repository and data.get("status") == "deployed",
                "Source report is not a completed deployment from the selected run")
        require(data.get("operation") == "deploy", "A rollback report cannot be rolled back again")
        require(isinstance(data.get("previous_sha"), str)
                and (not data["previous_sha"] or SHA.fullmatch(data["previous_sha"])), "Invalid previous deployment marker")
        manifest = validate(data.get("release"))
        should_deploy = True
    else:
        manifest = validate(data, run.get("head_sha"))
        should_deploy = manifest["should_deploy"]
    deployed = subprocess.run(["git", "ls-remote", "--exit-code", "--refs", "origin", "refs/tags/deployed/production"],
                              capture_output=True, text=True, timeout=30)
    require(deployed.returncode in (0, 2), "Cannot read the previous deployment marker")
    previous = deployed.stdout.split()[0] if deployed.stdout.strip() else ""
    require(not previous or SHA.fullmatch(previous), "Invalid deployment marker")
    if operation == "rollback":
        incomplete = run.get("conclusion") in ("failure", "cancelled", "timed_out")
        require(previous == manifest["sha"] or (incomplete and previous == data["previous_sha"]),
                "Another release is deployed; select its CD run for rollback")
    if previous and operation != "rollback" and previous != manifest["sha"]:
        command(["git", "fetch", "--no-tags", "origin", previous])
        older = subprocess.run(["git", "merge-base", "--is-ancestor", manifest["sha"], previous]).returncode
        require(older in (0, 1), "Cannot compare deployment commits")
        if older == 0 and operation != "plan":
            if os.getenv("GITHUB_EVENT_NAME") == "workflow_run":
                should_deploy = False
                print("Skipping a CI release older than the deployed commit.")
            else:
                raise ReleaseError("This CI release is older than the deployed commit; use an explicit rollback")
        elif operation == "deploy":
            ancestor = subprocess.run(["git", "merge-base", "--is-ancestor", previous, manifest["sha"]], capture_output=True).returncode
            require(ancestor == 0, "Release history diverged from the deployed commit")
    with open(output_file, "a") as stream:
        for key, value in {"sha": manifest["sha"],
                           "should_deploy": str(should_deploy).lower(), "previous_sha": previous}.items():
            stream.write(f"{key}={value}\n")
    print(f"Verified {expected} run {run_id}; release {manifest['sha']}; {len(manifest['images'])} image(s).")


def promote(report_file, rollback=False):
    report = json.loads(Path(report_file).read_text())
    require(report.get("status") in ("deployed", "rolled_back"), "Registry promotion requires a completed rollout")
    registries = Registries()
    for row in report["images"]:
        require(registries.digest(row["reference"] + ":current", missing=True) in (row.get("before_current"), row["digest"]),
                "Registry current changed since this deployment; alias updates stopped")
    for row in report["images"]:
        image, target, before = row["reference"], row["digest"], row.get("before_current")
        if rollback:
            if not before or before == target:
                continue
            current = registries.digest(image + ":current", missing=True)
            require(current in (before, target), "Registry current changed after this deployment")
            if current != before:
                registries.tag(image, target, "previous")
                registries.tag(image, before, "current")
        else:
            current = registries.digest(image + ":current", missing=True)
            require(current in (before, target), "Registry current changed during deployment")
            if current == target:
                continue
            if before:
                registries.tag(image, before, "previous")
            registries.tag(image, target, "current")
        print(f"Updated aliases for {row['service']}.")
    report["aliases_updated"] = True
    save(report_file, report)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest="command", required=True)
    p = sub.add_parser("image")
    p.add_argument("--output", required=True)
    p = sub.add_parser("manifest")
    p.add_argument("--images-dir", required=True)
    p.add_argument("--output", required=True)
    p = sub.add_parser("source")
    p.add_argument("--operation", choices=("plan", "deploy", "rollback"), required=True)
    p.add_argument("--run-id", required=True)
    p.add_argument("--outputs", required=True)
    p = sub.add_parser("promote")
    p.add_argument("--report", required=True)
    p.add_argument("--rollback", action="store_true")
    p = sub.add_parser("marker")
    p.add_argument("--report", required=True)
    p.add_argument("--outputs", required=True)
    sub.add_parser("logout")
    args = parser.parse_args()
    if args.command == "image":
        row = {"service": os.environ["RELEASE_SERVICE"], "image": os.environ["RELEASE_SERVICE"], "digest": os.environ["IMAGE_DIGEST"]}
        validate({"version": 3, "sha": os.environ["GITHUB_SHA"], "should_deploy": False, "registry": registry_prefix(), "images": [row]})
        save(args.output, row)
    elif args.command == "manifest":
        matrix = json.loads(os.environ["IMAGE_MATRIX"])["include"]
        images = [json.loads(p.read_text()) for p in Path(args.images_dir).rglob("image-digest.json")]
        require({i["service"] for i in images} == {i["service"] for i in matrix} and len(images) == len(matrix),
                "Published image digests do not match the build matrix")
        result = validate({"version": 3, "sha": os.environ["GITHUB_SHA"],
                           "should_deploy": os.environ["SHOULD_DEPLOY"] == "true",
                           "registry": registry_prefix(), "images": sorted(images, key=lambda i: i["service"])})
        save(args.output, result)
        print(f"Release manifest contains {len(images)} published image digest(s).")
    elif args.command == "source":
        prepare_source(args.operation, args.run_id, args.outputs)
    elif args.command == "promote":
        promote(args.report, args.rollback)
    elif args.command == "marker":
        report = json.loads(Path(args.report).read_text())
        require(report.get("status") in ("deployed", "rolled_back") and report.get("aliases_updated"),
                "Deployment marker requires a completed rollout and registry promotion")
        sha = report["previous_sha"] if report["operation"] == "rollback" else report["release"]["sha"]
        require(not sha or SHA.fullmatch(sha), "Invalid deployment marker")
        with open(args.outputs, "a") as stream:
            stream.write(f"sha={sha}\n")
    else:
        subprocess.run(["docker", "logout", "ghcr.io"], capture_output=True)


if __name__ == "__main__":
    try:
        main()
    except (ReleaseError, ValueError, KeyError, OSError, TypeError) as error:
        print(f"Release failed: {error if isinstance(error, ReleaseError) else type(error).__name__}", file=sys.stderr)
        sys.exit(1)
