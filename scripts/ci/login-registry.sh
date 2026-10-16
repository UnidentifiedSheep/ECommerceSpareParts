#!/usr/bin/env bash
set -euo pipefail

: "${ARTIFACT_REGISTRY_HOST:?ARTIFACT_REGISTRY_HOST is required}"
: "${ARTIFACT_REGISTRY_USERNAME:?ARTIFACT_REGISTRY_USERNAME is required}"
: "${ARTIFACT_REGISTRY_PASSWORD:?ARTIFACT_REGISTRY_PASSWORD is required}"

max_attempts=5
max_delay=30
delay=5

for ((attempt = 1; attempt <= max_attempts; attempt++)); do
  if printf '%s' "$ARTIFACT_REGISTRY_PASSWORD" |
    docker login "$ARTIFACT_REGISTRY_HOST" \
      --username "$ARTIFACT_REGISTRY_USERNAME" \
      --password-stdin; then
    exit 0
  else
    status=$?
  fi

  if ((attempt == max_attempts)); then
    echo "Registry login failed after ${max_attempts} attempts (exit ${status})." >&2
    exit "$status"
  fi

  wait_seconds=$((1 + RANDOM % delay))
  echo "Registry login failed (exit ${status}), attempt ${attempt}/${max_attempts}; retrying in ${wait_seconds}s." >&2
  sleep "$wait_seconds"
  delay=$((delay * 2))
  if ((delay > max_delay)); then
    delay=$max_delay
  fi
done
