#!/usr/bin/env bash
set -euo pipefail

APP_SERVICES=(
  gateway
  main-api
  pricing-api
  search-api
  analytics-api
  analytics-worker
)

MIGRATORS=(
  "main-migrator:main"
  "pricing-migrator:pricing"
  "analytics-migrator:analytics"
)

log() {
  echo
  echo "=== $* ==="
}

require_env() {
  local name="$1"

  if [ -z "${!name:-}" ]; then
    echo "${name} is required."
    exit 1
  fi
}

docker_sudo() {
  sudo --preserve-env=DOCKER_CONFIG docker "$@"
}

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

  echo "--- ${service_name} tasks ---"
  sudo docker service ps "$service_name" --no-trunc || true

  echo "--- ${service_name} logs ---"
  sudo docker service logs "$service_name" --raw --tail 200 || true
}

wait_for_postgres() {
  log "Wait for PostgreSQL"

  for attempt in $(seq 1 60); do
    if sudo docker run --rm --network "$TRAEFIK_NETWORK" postgres:18 \
      pg_isready -h pgql -p 5432 -U "$PGQL_USER" >/dev/null 2>&1; then
      echo "PostgreSQL is ready."
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

wait_for_service_running() {
  local service_name="$1"
  local attempts="${2:-60}"

  for attempt in $(seq 1 "$attempts"); do
    local desired_running
    local current_running
    local failed_count

    desired_running="$(sudo docker service ps "$service_name" \
      --filter desired-state=running \
      --format '{{.ID}}' | wc -l)"
    current_running="$(sudo docker service ps "$service_name" \
      --filter desired-state=running \
      --format '{{.CurrentState}}' | grep -c '^Running' || true)"
    failed_count="$(sudo docker service ps "$service_name" \
      --format '{{.CurrentState}}|{{.Error}}' | grep -Eci 'Failed|Rejected|non-zero exit|No such image' || true)"

    if [ "$desired_running" -gt 0 ] && [ "$desired_running" -eq "$current_running" ]; then
      return 0
    fi

    if [ "$failed_count" -gt 0 ]; then
      echo "${service_name} has failed tasks."
      print_service_diagnostics "$service_name"
      return 1
    fi

    sleep 5
  done

  echo "${service_name} did not become running in time."
  print_service_diagnostics "$service_name"
  return 1
}

run_migrator() {
  local migrator_name="$1"
  local database_name="$2"
  local service_name="${STACK_NAME}-${migrator_name}-${DEPLOY_RUN_ID}"
  local image="${ARTIFACT_REGISTRY_HOST}/${migrator_name}:${IMAGE_TAG}"

  log "Run ${migrator_name}"
  sudo docker service rm "$service_name" >/dev/null 2>&1 || true

  docker_sudo service create \
    --name "$service_name" \
    --detach=true \
    --restart-condition none \
    --constraint "$MIGRATOR_NODE_CONSTRAINT" \
    --network "$TRAEFIK_NETWORK" \
    --env DOTNET_ENVIRONMENT="$DOTNET_ENVIRONMENT" \
    --env ASPNETCORE_ENVIRONMENT="$ASPNETCORE_ENVIRONMENT" \
    --env "ConnectionString=Host=pgql;Port=5432;Database=${database_name};Username=${PGQL_USER};Password=${PGQL_PASSWORD}" \
    --mount type=bind,src="$CONFIGS_PATH",dst=/app/configs,readonly \
    --with-registry-auth \
    "$image" >/dev/null

  for attempt in $(seq 1 120); do
    local state
    state="$(sudo docker service ps "$service_name" --no-trunc --format '{{.CurrentState}}|{{.Error}}' | head -n 1)"

    case "$state" in
      Complete*)
        sudo docker service logs "$service_name" --raw --tail 100 || true
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

run_migrators() {
  local item

  for item in "${MIGRATORS[@]}"; do
    run_migrator "${item%%:*}" "${item##*:}"
  done
}

force_update_app_services() {
  log "Restart app services"

  local service
  for service in "${APP_SERVICES[@]}"; do
    docker_sudo service update \
      --detach=true \
      --force \
      "${STACK_NAME}_${service}" >/dev/null
  done
}

wait_for_app_services() {
  log "Wait for app services"

  local service
  for service in "${APP_SERVICES[@]}"; do
    wait_for_service_running "${STACK_NAME}_${service}" 60
  done
}

show_stack_services() {
  log "Stack services"
  sudo docker stack services "$STACK_NAME"
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

MIGRATOR_NODE_CONSTRAINT="${MIGRATOR_NODE_CONSTRAINT:-node.labels.role == infra}"

mkdir -p "$DOCKER_CONFIG"
sudo chown -R "$(id -u):$(id -g)" "$DOCKER_CONFIG"

log "Docker login"
echo "$ARTIFACT_REGISTRY_PASSWORD" | docker login \
  "$ARTIFACT_REGISTRY_HOST" \
  --username "$ARTIFACT_REGISTRY_USERNAME" \
  --password-stdin >/dev/null

log "Render stack files"
render_stack_file compose.stack.yaml compose.stack.rendered.yaml
render_stack_file compose.stack.database.yaml compose.stack.database.rendered.yaml

log "Deploy database"
docker_sudo stack deploy \
  --detach=true \
  -c compose.stack.database.rendered.yaml \
  "$STACK_NAME" \
  --with-registry-auth

wait_for_postgres
run_migrators
wait_for_postgres

log "Deploy stack"
docker_sudo stack deploy \
  --detach=true \
  -c compose.stack.rendered.yaml \
  "$STACK_NAME" \
  --with-registry-auth

wait_for_postgres
force_update_app_services
wait_for_app_services
show_stack_services
