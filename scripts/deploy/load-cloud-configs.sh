#!/usr/bin/env bash
set -euo pipefail

# Loads JSON appsettings from Cloud.ru Secret Management into local config files.
#
# Required env:
#   CLOUD_RU_KEY_ID
#   CLOUD_RU_KEY_SECRET
#
# Optional env:
#   CLOUD_RU_SECRET_PREFIX=/appsettings
#   CLOUD_RU_CONFIGS_OUT=./configs
#   CLOUD_RU_SECRET_NAMES="/appsettings/main/Staging,/appsettings/search/Staging"
#   CLOUD_RU_SECRET_PROJECT_ID=<Cloud.ru project id>
#   CLOUD_RU_AUTH_URL=https://iam.api.cloud.ru/api/v1/auth/token
#   CLOUD_RU_SECRET_API_URL=https://secretmanager.api.cloud.ru
#   CLOUD_RU_SECRET_DEPTH=-1
#   CLOUD_RU_SECRET_VERSION=<specific version number>
#   CLOUD_RU_LIST_URL_TEMPLATE="{api}/v1/secret/search?projectId={project_id}&folderPath={folder_path}&depth={depth}"
#   CLOUD_RU_ACCESS_URL_TEMPLATE="{api}/v1/secret/{secret_path}:access?projectId={project_id}"
#
# Naming:
#   /appsettings/main/Staging -> configs/main.Staging.json

AUTH_URL="${CLOUD_RU_AUTH_URL:-https://iam.api.cloud.ru/api/v1/auth/token}"
SECRET_API_URL="${CLOUD_RU_SECRET_API_URL:-https://secretmanager.api.cloud.ru}"
SECRET_PREFIX="${CLOUD_RU_SECRET_PREFIX:-/appsettings}"
OUT_DIR="${CLOUD_RU_CONFIGS_OUT:-./configs}"
SECRET_DEPTH="${CLOUD_RU_SECRET_DEPTH:--1}"
SECRET_VERSION="${CLOUD_RU_SECRET_VERSION:-}"
LIST_URL_TEMPLATE="${CLOUD_RU_LIST_URL_TEMPLATE:-{api}/v1/secret/search?projectId={project_id}&folderPath={folder_path}&depth={depth}}"
ACCESS_URL_TEMPLATE="${CLOUD_RU_ACCESS_URL_TEMPLATE:-{api}/v1/secret/{secret_path}:access?projectId={project_id}}"

usage() {
  cat <<'EOF'
Usage:
  load-cloud-configs.sh [secret-path...]

Examples:
  CLOUD_RU_KEY_ID=... CLOUD_RU_KEY_SECRET=... \
    scripts/deploy/load-cloud-configs.sh /appsettings/main/Staging /appsettings/search/Staging

  CLOUD_RU_KEY_ID=... CLOUD_RU_KEY_SECRET=... \
  CLOUD_RU_SECRET_NAMES="/appsettings/main/Staging,/appsettings/search/Staging" \
    scripts/deploy/load-cloud-configs.sh

  CLOUD_RU_KEY_ID=... CLOUD_RU_KEY_SECRET=... CLOUD_RU_SECRET_PROJECT_ID=... \
    scripts/deploy/load-cloud-configs.sh
EOF
}

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Required command not found: $1" >&2
    exit 1
  fi
}

require_env() {
  if [ -z "${!1:-}" ]; then
    echo "Required env is missing: $1" >&2
    exit 1
  fi
}

url_encode() {
  python3 -c 'import sys, urllib.parse; print(urllib.parse.quote(sys.argv[1], safe=""))' "$1"
}

url_encode_path() {
  python3 -c 'import sys, urllib.parse; print(urllib.parse.quote(sys.argv[1], safe="/"))' "$1"
}

json_get_token() {
  python3 -c '
import json, sys
data = json.load(sys.stdin)
token = data.get("access_token") or data.get("token")
if not token:
    raise SystemExit("Token response does not contain access_token/token")
print(token)
'
}

extract_secret_names() {
  local prefix="$1"

  python3 - "$prefix" <<'PY'
import json
import sys

prefix = sys.argv[1].rstrip("/")
data = json.load(sys.stdin)

def walk(value):
    if isinstance(value, dict):
        yield value
        for child in value.values():
            yield from walk(child)
    elif isinstance(value, list):
        for item in value:
            yield from walk(item)

names = set()
for item in walk(data):
    candidates = []
    for key in ("path", "name", "key"):
        val = item.get(key)
        if isinstance(val, str):
            candidates.append(val)

    path = item.get("path")
    name = item.get("name")
    if isinstance(path, str) and isinstance(name, str):
        candidates.append(f"{path.rstrip('/')}/{name.lstrip('/')}")

    for candidate in candidates:
        normalized = "/" + candidate.strip("/")
        if normalized == prefix or normalized.startswith(prefix + "/"):
            names.add(normalized)

for name in sorted(names):
    print(name)
PY
}

extract_secret_payload() {
  python3 <<'PY'
import base64
import json
import sys

data = json.load(sys.stdin)

def find_payload(value):
    if isinstance(value, dict):
        for path in (
            ("payload",),
            ("payload", "data", "value"),
            ("payload", "value"),
            ("data", "value"),
            ("value",),
            ("secret", "value"),
        ):
            current = value
            for key in path:
                if not isinstance(current, dict) or key not in current:
                    break
                current = current[key]
            else:
                if isinstance(current, str):
                    return current

        for child in value.values():
            found = find_payload(child)
            if found is not None:
                return found

    if isinstance(value, list):
        for item in value:
            found = find_payload(item)
            if found is not None:
                return found

    return None

payload = find_payload(data)
if payload is None:
    raise SystemExit("Secret response does not contain payload value")

try:
    decoded = base64.b64decode(payload, validate=True)
    text = decoded.decode("utf-8")
    stripped = text.lstrip()
    if stripped.startswith("{") or stripped.startswith("["):
        print(text, end="")
    else:
        print(payload, end="")
except Exception:
    print(payload, end="")
PY
}

to_config_path() {
  local secret_name="$1"
  local prefix="$2"

  python3 - "$secret_name" "$prefix" "$OUT_DIR" <<'PY'
import pathlib
import sys

secret_name = "/" + sys.argv[1].strip("/")
prefix = "/" + sys.argv[2].strip("/")
out_dir = pathlib.Path(sys.argv[3])

if secret_name == prefix:
    raise SystemExit(f"Secret path equals prefix and cannot become file name: {secret_name}")

if secret_name.startswith(prefix + "/"):
    relative = secret_name[len(prefix) + 1:]
else:
    relative = secret_name.strip("/")

file_name = relative.replace("/", ".") + ".json"
print(out_dir / file_name)
PY
}

render_template() {
  local template="$1"
  local secret_name="${2:-}"
  local secret_encoded="${3:-}"
  python3 - \
    "$template" \
    "$SECRET_API_URL" \
    "${CLOUD_RU_SECRET_PROJECT_ID:-}" \
    "${SECRET_PREFIX#/}" \
    "$SECRET_DEPTH" \
    "$secret_encoded" \
    "$SECRET_VERSION" <<'PY'
import sys
from urllib.parse import quote

template, api, project_id, folder_path, depth, secret_path, version = sys.argv[1:]

values = {
    "api": api.rstrip("/"),
    "project_id": quote(project_id, safe=""),
    "folder_path": quote(folder_path, safe=""),
    "depth": quote(depth, safe=""),
    "secret": secret_path,
    "secret_path": secret_path,
    "version": quote(version, safe=""),
}

url = template
for key, value in values.items():
    url = url.replace("{" + key + "}", value)

if version and "version=" not in url:
    url += ("&" if "?" in url else "?") + "version=" + values["version"]

print(url, end="")
PY
}

get_token() {
  local encoded_key_id
  local encoded_secret
  encoded_key_id="$(url_encode "$CLOUD_RU_KEY_ID")"
  encoded_secret="$(url_encode "$CLOUD_RU_KEY_SECRET")"

  curl -fsS --location --request POST \
    "${AUTH_URL}?key_id=${encoded_key_id}&secret=${encoded_secret}" |
    json_get_token
}

list_secret_names() {
  if [ -n "${CLOUD_RU_SECRET_NAMES:-}" ]; then
    printf '%s\n' "$CLOUD_RU_SECRET_NAMES" |
      tr ',;' '\n' |
      sed '/^[[:space:]]*$/d'
    return
  fi

  if [ "$#" -gt 0 ]; then
    printf '%s\n' "$@"
    return
  fi

  require_env CLOUD_RU_SECRET_PROJECT_ID

  local list_url
  list_url="$(render_template "$LIST_URL_TEMPLATE")"

  curl -fsS \
    --header "Authorization: Bearer ${TOKEN}" \
    "$list_url" |
    extract_secret_names "$SECRET_PREFIX"
}

load_secret() {
  local secret_name="$1"
  local normalized_secret
  local encoded_secret
  local access_url
  local target_path
  local tmp_path

  normalized_secret="/${secret_name#/}"
  encoded_secret="$(url_encode_path "${normalized_secret#/}")"
  access_url="$(render_template "$ACCESS_URL_TEMPLATE" "$normalized_secret" "$encoded_secret")"
  target_path="$(to_config_path "$normalized_secret" "$SECRET_PREFIX")"
  tmp_path="${target_path}.tmp"

  mkdir -p "$(dirname "$target_path")"

  curl -fsS \
    --header "Authorization: Bearer ${TOKEN}" \
    "$access_url" |
    extract_secret_payload > "$tmp_path"

  python3 -m json.tool "$tmp_path" > "${tmp_path}.formatted"
  mv "${tmp_path}.formatted" "$target_path"
  rm -f "$tmp_path"

  echo "Loaded ${normalized_secret} -> ${target_path}"
}

main() {
  if [ "${1:-}" = "-h" ] || [ "${1:-}" = "--help" ]; then
    usage
    exit 0
  fi

  require_command curl
  require_command python3
  require_env CLOUD_RU_KEY_ID
  require_env CLOUD_RU_KEY_SECRET

  mkdir -p "$OUT_DIR"

  TOKEN="$(get_token)"
  export TOKEN

  mapfile -t secrets < <(list_secret_names "$@")
  if [ "${#secrets[@]}" -eq 0 ]; then
    echo "No secrets found for prefix: ${SECRET_PREFIX}" >&2
    exit 1
  fi

  for secret in "${secrets[@]}"; do
    load_secret "$secret"
  done
}

main "$@"
