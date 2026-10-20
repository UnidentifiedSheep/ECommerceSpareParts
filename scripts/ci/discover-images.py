#!/usr/bin/env python3
"""Select application images independently of local Compose and infrastructure."""

import json
import os
from pathlib import Path
import subprocess
import sys

from image_catalog import catalog


def discover(projects, changed, event, republish):
    root = Path.cwd()
    images = catalog(root)

    unknown = republish - {item["image"] for item in images}
    if unknown:
        raise ValueError(f"Unknown application images: {', '.join(sorted(unknown))}")

    affected = {
        Path(os.path.relpath(Path(project).parent / "Dockerfile", root)).as_posix()
        for project in projects
    }
    affected.update(path for path in changed if Path(path).name == "Dockerfile")
    api_changed = any(
        item["service"].endswith("-api") and item["dockerfile"] in affected
        for item in images
    )
    graphql_changed = any(
        path.startswith(("schemas/graphql/", "src/Services/GraphQL.Common/"))
        or "/GraphQl/" in path
        or path in {
            ".config/dotnet-tools.json", ".github/workflows/graphql.yml",
            "scripts/ci/build-graphql.sh",
        }
        for path in changed
    )
    build_inputs_changed = any(path in {
        ".dockerignore", "Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props",
        "NuGet.Config", "nuget.config", "global.json", ".github/workflows/publish-images.yml",
    } for path in changed)
    selected = [item for item in images if (
        item["image"] in republish if republish else (
            event == "workflow_dispatch"
            or build_inputs_changed
            or item["dockerfile"] in affected
            or (item["service"] == "gateway" and (api_changed or graphql_changed))
        )
    )]
    return {
        "has_images": bool(selected),
        "should_publish": bool(selected),
        "should_deploy": bool(selected) and not republish,
        "matrix": {"include": selected},
        "services": ",".join(item["service"] for item in selected),
    }


def main():
    projects_file, output_file = sys.argv[1:]
    projects = Path(projects_file).read_text(encoding="utf-8-sig").splitlines()
    changed = subprocess.check_output([
        "git", "diff", "--name-only", os.environ["BASE_SHA"], os.environ["HEAD_SHA"],
    ], text=True).splitlines()
    republish = {name.strip() for name in os.getenv("REPUBLISH_IMAGES", "").split(",") if name.strip()}
    result = discover(projects, changed, os.environ["EVENT_NAME"], republish)
    with open(output_file, "a", encoding="utf-8") as stream:
        for key, value in result.items():
            encoded = value if isinstance(value, str) else json.dumps(value, separators=(",", ":"))
            stream.write(f"{key}={encoded}\n")
    print(f"Affected application services: {result['services'] or 'none'}")
    if republish:
        print("Republish without deployment.")


if __name__ == "__main__":
    main()
