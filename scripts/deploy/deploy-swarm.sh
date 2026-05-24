#!/usr/bin/env bash
set -euo pipefail

render_stack_file() {
  local source_file="$1"
  local target_file="$2"

  docker compose -f "$source_file" config |
    sed '/^name:/d' |
    sed -E 's/^([[:space:]]+published: )"([0-9]+)"$/\1\2/' \
    > "$target_file"
}

print_service_diagnostics() {
  local service_name="$1"

  sudo docker service ps "$service_name" --no-trunc || true
  sudo docker service logs "$service_name" --raw --tail 200 || true
}

wait_for_postgres() {
  echo "=== Wait for PostgreSQL ==="

  for attempt in $(seq 1 60); do
    if sudo docker run --rm --network "$TRAEFIK_NETWORK" postgres:18 \
      pg_isready -h pgql -p 5432 -U "$PGQL_USER"; then
      return 0
    fi

              if [ "$attempt" -eq 60 ]; then
                echo "PostgreSQL did not become ready in time."
                print_service_diagnostics "${STACK_NAME}_pgql"
                return 1
              fi

    sleep 5
  done
}

run_migrator() {
  local migrator_name="$1"
  local database_name="$2"
  local service_name="${STACK_NAME}-${migrator_name}-${DEPLOY_RUN_ID}"
  local image="${ARTIFACT_REGISTRY_HOST}/${migrator_name}:${IMAGE_TAG}"

  echo "=== Run ${migrator_name} ==="
  sudo docker service rm "$service_name" >/dev/null 2>&1 || true

  sudo --preserve-env=DOCKER_CONFIG docker service create \
    --name "$service_name" \
    --detach=true \
    --restart-condition none \
    --constraint 'node.labels.role == app' \
    --network "$TRAEFIK_NETWORK" \
    --env DOTNET_ENVIRONMENT="$DOTNET_ENVIRONMENT" \
    --env ASPNETCORE_ENVIRONMENT="$ASPNETCORE_ENVIRONMENT" \
    --env "ConnectionString=Host=pgql;Port=5432;Database=${database_name};Username=${PGQL_USER};Password=${PGQL_PASSWORD}" \
    --mount type=bind,src="$CONFIGS_PATH",dst=/app/configs,readonly \
    --with-registry-auth \
    "$image"

  for attempt in $(seq 1 120); do
    state="$(sudo docker service ps "$service_name" --no-trunc --format '{{.CurrentState}}|{{.Error}}' | head -n 1)"

    case "$state" in
      Complete*)
        sudo docker service logs "$service_name" --raw --tail 200 || true
        sudo docker service rm "$service_name" >/dev/null
        return 0
        ;;
      Failed*|Rejected*|Shutdown*non-zero\ exit*|*non-zero\ exit*|*exit\ \(*)
        echo "$state"
        print_service_diagnostics "$service_name"
        sudo docker service rm "$service_name" >/dev/null
        return 1
        ;;
    esac

    sleep 5
  done

  echo "${migrator_name} did not complete in time."
  print_service_diagnostics "$service_name"
  sudo docker service rm "$service_name" >/dev/null
  return 1
}

require_env() {
  local name="$1"

  if [ -z "${!name:-}" ]; then
    echo "${name} is required."
    exit 1
  fi
}

require_env STACK_NAME
require_env ARTIFACT_REGISTRY_HOST
require_env ARTIFACT_REGISTRY_USERNAME
require_env ARTIFACT_REGISTRY_PASSWORD
require_env IMAGE_TAG
require_env DEPLOY_RUN_ID
require_env DOTNET_ENVIRONMENT
require_env ASPNETCORE_ENVIRONMENT
require_env PGQL_USER
require_env PGQL_PASSWORD
require_env TRAEFIK_NETWORK
require_env CONFIGS_PATH
require_env DOCKER_CONFIG

mkdir -p "$DOCKER_CONFIG"
sudo chown -R "$(id -u):$(id -g)" "$DOCKER_CONFIG"

echo "$ARTIFACT_REGISTRY_PASSWORD" | docker login \
  "$ARTIFACT_REGISTRY_HOST" \
  --username "$ARTIFACT_REGISTRY_USERNAME" \
  --password-stdin

render_stack_file compose.stack.yaml compose.stack.rendered.yaml
render_stack_file compose.stack.database.yaml compose.stack.database.rendered.yaml

echo "=== Deploy database ==="
sudo --preserve-env=DOCKER_CONFIG docker stack deploy \
  -c compose.stack.database.rendered.yaml \
  "$STACK_NAME" \
  --with-registry-auth

wait_for_postgres

run_migrator main-migrator main
run_migrator pricing-migrator pricing
run_migrator analytics-migrator analytics

echo "=== Deploy stack ==="
sudo --preserve-env=DOCKER_CONFIG docker stack deploy \
  -c compose.stack.rendered.yaml \
  "$STACK_NAME" \
  --with-registry-auth

echo "=== Stack services ==="
sudo docker stack services "$STACK_NAME"
