"""Application image names shared by discovery, releases and registry cleanup."""

from pathlib import Path


def catalog(root=None):
    root = Path(root or Path(__file__).resolve().parents[2])
    images = []
    for dockerfile in sorted(root.glob("src/Services/**/Dockerfile")):
        project = dockerfile.parent.name
        if project != "Gateway" and not project.endswith((".Api", ".Worker", ".Migrator")):
            continue
        if (dockerfile.parent / f"{project}.csproj").is_file():
            name = project.replace(".", "-").lower()
            images.append({"service": name, "image": name, "context": ".",
                           "dockerfile": dockerfile.relative_to(root).as_posix()})
    if not images or len({item["image"] for item in images}) != len(images):
        raise ValueError("Application image catalog is empty or contains duplicate names")
    return images
