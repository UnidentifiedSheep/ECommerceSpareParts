#!/usr/bin/env python3
import argparse
import base64
import json
import os
import sys
import time
import urllib.parse
import urllib.request
from urllib.error import HTTPError, URLError
from pathlib import Path


DEFAULT_AUTH_URL = "https://iam.api.cloud.ru/api/v1/auth/token"
DEFAULT_SECRET_API_URL = "https://secretmanager.api.cloud.ru"
DEFAULT_HTTP_RETRIES = 5
DEFAULT_HTTP_TIMEOUT_SECONDS = 30
DEFAULT_HTTP_RETRY_DELAY_SECONDS = 2


def get_retry_count() -> int:
    return int(os.environ.get("CLOUD_RU_HTTP_RETRIES", str(DEFAULT_HTTP_RETRIES)))


def get_timeout_seconds() -> int:
    return int(os.environ.get("CLOUD_RU_HTTP_TIMEOUT_SECONDS", str(DEFAULT_HTTP_TIMEOUT_SECONDS)))


def get_retry_delay_seconds() -> float:
    return float(os.environ.get("CLOUD_RU_HTTP_RETRY_DELAY_SECONDS", str(DEFAULT_HTTP_RETRY_DELAY_SECONDS)))


def should_retry_http_error(exc: HTTPError) -> bool:
    return exc.code == 429 or 500 <= exc.code < 600


def sleep_before_retry(attempt: int) -> None:
    delay = get_retry_delay_seconds() * attempt
    print(f"Cloud.ru request failed, retrying in {delay:.1f}s...", file=sys.stderr)
    time.sleep(delay)


def get_cloud_ru_token(
    key_id: str,
    key_secret: str,
    auth_url: str = DEFAULT_AUTH_URL,
) -> str:
    if not key_id:
        raise ValueError("key_id is required")

    if not key_secret:
        raise ValueError("key_secret is required")

    query = urllib.parse.urlencode({
        "key_id": key_id,
        "secret": key_secret,
    })
    request = urllib.request.Request(
        f"{auth_url}?{query}",
        method="POST",
        headers={
            "Accept": "application/json",
        },
    )

    payload = open_json_with_retries(request)

    token = payload.get("access_token")
    if not token:
        raise RuntimeError("Cloud.ru auth response does not contain access_token")

    return token


def request_json(
    url: str,
    token: str,
    method: str = "GET",
) -> dict:
    headers = {
        "Accept": "application/json",
        "Authorization": f"Bearer {token}",
    }

    if method.upper() not in {"GET", "HEAD"}:
        headers["Content-Type"] = "application/json"

    request = urllib.request.Request(
        url,
        method=method,
        headers=headers,
    )

    return open_json_with_retries(request)


def open_json_with_retries(request: urllib.request.Request) -> dict:
    retries = get_retry_count()
    timeout = get_timeout_seconds()
    method = request.get_method()
    url = request.full_url

    for attempt in range(1, retries + 1):
        try:
            with urllib.request.urlopen(request, timeout=timeout) as response:
                return json.loads(response.read().decode("utf-8"))
        except HTTPError as exc:
            body = exc.read().decode("utf-8", errors="replace")
            if attempt < retries and should_retry_http_error(exc):
                sleep_before_retry(attempt)
                continue

            raise RuntimeError(
                f"Cloud.ru request failed: {method} {url} -> HTTP {exc.code} {exc.reason}\n{body}"
            ) from exc
        except (TimeoutError, URLError) as exc:
            if attempt < retries:
                sleep_before_retry(attempt)
                continue

            raise RuntimeError(
                f"Cloud.ru request failed after {retries} attempts: {method} {url}\n{exc}"
            ) from exc

    raise RuntimeError(f"Cloud.ru request failed after {retries} attempts: {method} {url}")


def list_secret_paths(
    token: str,
    project_id: str,
    prefix: str = "/appsettings",
    secret_api_url: str = DEFAULT_SECRET_API_URL,
    depth: int = -1,
) -> list[str]:
    if not project_id:
        raise ValueError("project_id is required")

    normalized_prefix = prefix.strip("/")
    parent_id = os.environ.get("CLOUD_RU_SECRET_PARENT_ID", "")

    if parent_id:
        query = urllib.parse.urlencode({
            "parentId": parent_id,
            "page.limit": "1000",
            "page.offset": "0",
        })
        url = os.environ.get(
            "CLOUD_RU_SECRET_LIST_URL",
            f"{secret_api_url.rstrip('/')}/v1/secrets?{query}",
        )
    else:
        query = urllib.parse.urlencode({
            "projectId": project_id,
            "folderPath": normalized_prefix,
            "depth": str(depth),
        })
        url = os.environ.get(
            "CLOUD_RU_SECRET_LIST_URL",
            f"{secret_api_url.rstrip('/')}/v2/secret?{query}",
        )
    response = request_json(url, token)

    result: list[str] = []
    for secret in response.get("secrets", []):
        path = secret.get("path")
        if not path:
            continue

        normalized_path = "/" + str(path).strip("/")
        if normalized_path == "/" + normalized_prefix or normalized_path.startswith(f"/{normalized_prefix}/"):
            result.append(normalized_path)

    return sorted(set(result))


def get_secret_payload(
    token: str,
    project_id: str,
    secret_path: str,
    secret_api_url: str = DEFAULT_SECRET_API_URL,
    version: int | None = None,
) -> str:
    if not project_id:
        raise ValueError("project_id is required")

    normalized_path = secret_path.strip("/")
    query_params: dict[str, str] = {
        "projectId": project_id,
    }
    if version is not None:
        query_params["version"] = str(version)

    secret_id = os.environ.get(f"CLOUD_RU_SECRET_ID_{normalized_path.replace('/', '_').replace('-', '_').replace('.', '_').upper()}", "")

    if secret_id:
        version_id = str(version) if version is not None else os.environ.get("CLOUD_RU_SECRET_VERSION_ID", "latest")
        url = os.environ.get(
            "CLOUD_RU_SECRET_ACCESS_URL",
            f"{secret_api_url.rstrip('/')}/v1/secrets/{urllib.parse.quote(secret_id, safe='')}/versions/{urllib.parse.quote(version_id, safe='')}:access",
        )
    else:
        encoded_path = urllib.parse.quote(normalized_path, safe="/")
        query = urllib.parse.urlencode(query_params)
        url = os.environ.get(
            "CLOUD_RU_SECRET_ACCESS_URL",
            f"{secret_api_url.rstrip('/')}/v2/version/{encoded_path}?{query}",
        )
    response = request_json(url, token)

    payload = response.get("payload") or response.get("data")
    if not payload:
        raise RuntimeError(f"Secret response does not contain payload: /{normalized_path}")

    try:
        return base64.b64decode(payload, validate=True).decode("utf-8")
    except Exception as exc:
        raise RuntimeError(f"Secret payload is not valid base64 UTF-8: /{normalized_path}") from exc


def parse_secret_names(value: str) -> list[str]:
    return [
        "/" + item.strip().strip("/")
        for item in value.replace(";", ",").split(",")
        if item.strip()
    ]


def secret_path_to_config_path(
    secret_path: str,
    prefix: str,
    output_dir: str,
) -> Path:
    normalized_path = "/" + secret_path.strip("/")
    normalized_prefix = "/" + prefix.strip("/")

    if normalized_path == normalized_prefix:
        raise ValueError(f"Secret path equals prefix and cannot become file name: {normalized_path}")

    if normalized_path.startswith(normalized_prefix + "/"):
        relative = normalized_path[len(normalized_prefix) + 1:]
    else:
        relative = normalized_path.strip("/")

    return Path(output_dir) / f"{relative.replace('/', '.')}.json"


def write_json_config(
    secret_path: str,
    payload: str,
    prefix: str,
    output_dir: str,
) -> Path:
    target_path = secret_path_to_config_path(secret_path, prefix, output_dir)
    target_path.parent.mkdir(parents=True, exist_ok=True)

    try:
        parsed = json.loads(payload)
    except json.JSONDecodeError as exc:
        raise RuntimeError(f"Secret payload is not valid JSON: {secret_path}") from exc

    target_path.write_text(
        json.dumps(parsed, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )

    return target_path


def load_secrets_to_configs(
    token: str,
    project_id: str,
    prefix: str = "/appsettings",
    output_dir: str = "./configs",
    secret_api_url: str = DEFAULT_SECRET_API_URL,
    secret_names: list[str] | None = None,
    depth: int = -1,
    version: int | None = None,
) -> list[Path]:
    paths = secret_names or list_secret_paths(
        token=token,
        project_id=project_id,
        prefix=prefix,
        secret_api_url=secret_api_url,
        depth=depth,
    )

    if not paths:
        raise RuntimeError(f"No secrets found for prefix: {prefix}")

    written_files: list[Path] = []
    for secret_path in paths:
        payload = get_secret_payload(
            token=token,
            project_id=project_id,
            secret_path=secret_path,
            secret_api_url=secret_api_url,
            version=version,
        )
        written_files.append(write_json_config(secret_path, payload, prefix, output_dir))

    return written_files


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Load appsettings secrets from Cloud.ru Secret Management.",
    )
    parser.add_argument(
        "--print-token",
        action="store_true",
        help="Print access token. Intended only for local diagnostics.",
    )
    parser.add_argument(
        "--list",
        action="store_true",
        help="List secret paths under CLOUD_RU_SECRET_PREFIX.",
    )
    parser.add_argument(
        "--get",
        metavar="SECRET_PATH",
        help="Print decoded secret payload for the specified path.",
    )
    parser.add_argument(
        "--out",
        default=os.environ.get("CLOUD_RU_CONFIGS_OUT", "./configs"),
        help="Output directory for generated JSON configs. Default: CLOUD_RU_CONFIGS_OUT or ./configs.",
    )

    args = parser.parse_args()

    key_id = os.environ.get("CLOUD_RU_KEY_ID", "")
    key_secret = os.environ.get("CLOUD_RU_KEY_SECRET", "")
    project_id = os.environ.get("CLOUD_RU_SECRET_PROJECT_ID", "")
    prefix = os.environ.get("CLOUD_RU_SECRET_PREFIX", "/appsettings")
    secret_api_url = os.environ.get("CLOUD_RU_SECRET_API_URL", DEFAULT_SECRET_API_URL)
    depth = int(os.environ.get("CLOUD_RU_SECRET_DEPTH", "-1"))
    version_value = os.environ.get("CLOUD_RU_SECRET_VERSION")
    version = int(version_value) if version_value else None
    explicit_secret_names = os.environ.get("CLOUD_RU_SECRET_NAMES", "")

    token = get_cloud_ru_token(
        key_id=key_id,
        key_secret=key_secret,
        auth_url=os.environ.get("CLOUD_RU_AUTH_URL", DEFAULT_AUTH_URL),
    )

    if args.print_token:
        print(token)
    elif args.list:
        for secret_path in list_secret_paths(
            token=token,
            project_id=project_id,
            prefix=prefix,
            secret_api_url=secret_api_url,
            depth=depth,
        ):
            print(secret_path)
    elif args.get:
        payload = get_secret_payload(
            token=token,
            project_id=project_id,
            secret_path=args.get,
            secret_api_url=secret_api_url,
            version=version,
        )
        print(payload)
    else:
        written_files = load_secrets_to_configs(
            token=token,
            project_id=project_id,
            prefix=prefix,
            output_dir=args.out,
            secret_api_url=secret_api_url,
            secret_names=parse_secret_names(explicit_secret_names) if explicit_secret_names else None,
            depth=depth,
            version=version,
        )

        for path in written_files:
            print(f"Loaded {path}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
