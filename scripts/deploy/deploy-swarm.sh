#!/usr/bin/env bash
set -euo pipefail

STACK_FILES=(
  secrets
  gateway
  apps
  workers
  infra
  monitoring
)

# Format: <priority>:<stack>:<service>. Services with the same priority roll
# concurrently; the next priority starts only after the current wave is ready.
RUNTIME_DEPLOY_PLAN=(
  "10:apps:main-api"
  "20:apps:search-api"
  "20:apps:analytics-api"
  "30:apps:pricing-api"
  "40:workers:main-worker"
  "40:workers:pricing-worker"
  "40:workers:analytics-worker"
  "50:gateway:gateway"
)

APP_SERVICES=()
for runtime_plan_item in "${RUNTIME_DEPLOY_PLAN[@]}"; do
  APP_SERVICES+=("${runtime_plan_item#*:}")
done
unset runtime_plan_item

INFRA_SERVICES=(
  "infra:postgres"
  "infra:postgres-backup"
  "infra:redis"
  "infra:rabbitmq"
  "infra:opensearch"
)

MONITORING_SERVICES=(
  "monitoring:prometheus"
  "monitoring:node-exporter"
  "monitoring:loki"
  "monitoring:tempo"
  "monitoring:grafana"
  "monitoring:opensearch-dashboards"
)

GATEWAY_SERVICES=(
  "gateway:traefik"
  "gateway:gateway"
  "gateway:portainer-agent"
  "gateway:portainer"
)

MIGRATORS=(
  "main-migrator:main"
  "pricing-migrator:pricing"
  "analytics-migrator:analytics"
)

PERSISTENT_VOLUMES=(
  traefik_letsencrypt
  portainer_data
  pg_data
  redis_data
  rabbitmq_data
  opensearch_data
  prometheus_data
  grafana_data
  loki_data
  tempo_data
)

REQUIRED_NODE_LABELS=(
  workload.traefik=true
  workload.gateway=true
  management.portainer=true
  workload.api=true
  workload.worker=true
  infra.postgres=true
  infra.redis=true
  infra.rabbitmq=true
  infra.opensearch=true
  monitoring.grafana=true
  monitoring.loki=true
  monitoring.tempo=true
  monitoring.prometheus=true
  monitoring.opensearch-dashboards=true
)

ACTIVE_MIGRATOR_SERVICE=""
ACTIVE_DIRECTORY_SERVICE=""
ACTIVE_WALG_PREFLIGHT_SERVICE=""
ACTIVE_PORTAINER_PREFLIGHT_SERVICE=""

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

cleanup_temporary_services() {
  if [ -n "$ACTIVE_MIGRATOR_SERVICE" ]; then
    sudo docker service rm "$ACTIVE_MIGRATOR_SERVICE" >/dev/null 2>&1 || true
  fi

  if [ -n "$ACTIVE_DIRECTORY_SERVICE" ]; then
    sudo docker service rm "$ACTIVE_DIRECTORY_SERVICE" >/dev/null 2>&1 || true
  fi

  if [ -n "$ACTIVE_WALG_PREFLIGHT_SERVICE" ]; then
    sudo docker service rm "$ACTIVE_WALG_PREFLIGHT_SERVICE" >/dev/null 2>&1 || true
  fi

  if [ -n "$ACTIVE_PORTAINER_PREFLIGHT_SERVICE" ]; then
    sudo docker service rm "$ACTIVE_PORTAINER_PREFLIGHT_SERVICE" >/dev/null 2>&1 || true
  fi
}

trap cleanup_temporary_services EXIT
trap 'exit 130' INT TERM

stack_name() {
  echo "${STACK_NAME}-$1"
}

service_name() {
  local stack_suffix="$1"
  local service="$2"
  echo "$(stack_name "$stack_suffix")_${service}"
}

render_stack_file() {
  local source_file="$1"
  local target_file="$2"

  docker compose -f "$source_file" config |
    sed '/^name:/d' |
    sed -E \
      -e 's/^([[:space:]]+published: )"([0-9]+)"$/\1\2/' \
      -e 's/^([[:space:]]+mode: )"([0-9]+)"$/\1\2/' \
    > "$target_file"
}

render_stack_files() {
  local stack

  for stack in "${STACK_FILES[@]}"; do
    render_stack_file \
      "deploy/stack.${stack}.yml" \
      "deploy/stack.${stack}.rendered.yml"
  done
}

ensure_overlay_network() {
  local network="$1"

  if sudo docker network inspect "$network" >/dev/null 2>&1; then
    local driver
    local scope
    driver="$(sudo docker network inspect "$network" --format '{{.Driver}}')"
    scope="$(sudo docker network inspect "$network" --format '{{.Scope}}')"

    if [ "$driver" != "overlay" ] || [ "$scope" != "swarm" ]; then
      echo "Network ${network} exists but is not a Swarm overlay network."
      exit 1
    fi

    return
  fi

  sudo docker network create \
    --driver overlay \
    --attachable \
    "$network" >/dev/null
}

ensure_external_networks() {
  log "Ensure external overlay networks"
  ensure_overlay_network "$PUBLIC_NETWORK"
  ensure_overlay_network "$BACKEND_NETWORK"
  ensure_overlay_network "$MONITORING_NETWORK"
}

ensure_persistent_volumes() {
  log "Ensure persistent volumes"

  local volume
  for volume in "${PERSISTENT_VOLUMES[@]}"; do
    sudo docker volume inspect "${VOLUME_PREFIX}_${volume}" >/dev/null 2>&1 ||
      sudo docker volume create "${VOLUME_PREFIX}_${volume}" >/dev/null
  done
}

validate_swarm_manager() {
  if [ "$(sudo docker info --format '{{.Swarm.LocalNodeState}}')" != "active" ]; then
    echo "Docker Swarm is not active on this host."
    exit 1
  fi

  if [ "$(sudo docker info --format '{{.Swarm.ControlAvailable}}')" != "true" ]; then
    echo "Production deploy must run on a Docker Swarm manager."
    exit 1
  fi
}

validate_required_swarm_secrets() {
  local secret

  for secret in "$POSTGRES_WALG_SECRET"; do
    if ! sudo docker secret inspect "$secret" >/dev/null 2>&1; then
      echo "Required Docker Swarm secret ${secret} is missing"
      exit 1
    fi
  done
}

cloudru_sync_is_configured() {
  local is_configured=true
  local secret

  if [ -z "${CLOUD_RU_SECRET_PROJECT_ID:-}" ]; then
    echo "WARNING: CLOUD_RU_SECRET_PROJECT_ID is not configured; application config synchronization will be skipped." >&2
    is_configured=false
  fi

  for secret in cloudru_key_id cloudru_key_secret; do
    if ! sudo docker secret inspect "$secret" >/dev/null 2>&1; then
      echo "WARNING: Docker Swarm secret ${secret} is missing; application config synchronization will be skipped." >&2
      is_configured=false
    fi
  done

  [ "$is_configured" = true ]
}

ensure_versioned_config() {
  local prefix="$1"
  local source_file="$2"
  local hash
  local name

  if [ ! -f "$source_file" ]; then
    echo "Required config source ${source_file} does not exist." >&2
    exit 1
  fi

  hash="$(sha256sum "$source_file" | cut -c1-12)"
  name="${prefix}_${hash}"

  if ! sudo docker config inspect "$name" >/dev/null 2>&1; then
    sudo docker config create "$name" "$source_file" >/dev/null
  fi

  echo "$name"
}

prepare_versioned_configs() {
  CLOUDRU_SECRETS_SCRIPT_CONFIG="$(
    ensure_versioned_config \
      cloudru_secrets_script \
      scripts/deploy/load-secrets.py
  )"
  POSTGRES_INIT_SQL_CONFIG="$(
    ensure_versioned_config \
      "${STACK_NAME}_postgres_init_sql" \
      init/clear-expired-coef.sql
  )"
  POSTGRES_INIT_SCRIPT_CONFIG="$(
    ensure_versioned_config \
      "${STACK_NAME}_postgres_init_script" \
      init/create-multiple-postgresql-databases.sh
  )"
  PROMETHEUS_CONFIG="$(
    ensure_versioned_config \
      "${STACK_NAME}_prometheus_config" \
      prometheus.yml
  )"
  TEMPO_CONFIG="$(
    ensure_versioned_config \
      "${STACK_NAME}_tempo_config" \
      deploy/tempo/tempo.yml
  )"
  GRAFANA_DATASOURCES_CONFIG="$(
    ensure_versioned_config \
      "${STACK_NAME}_grafana_datasources" \
      deploy/grafana/provisioning/datasources/datasources.yml
  )"
  GRAFANA_DASHBOARDS_CONFIG="$(
    ensure_versioned_config \
      "${STACK_NAME}_grafana_dashboards" \
      deploy/grafana/provisioning/dashboards/swarm.yml
  )"
  GRAFANA_SWARM_NODES_DASHBOARD_CONFIG="$(
    ensure_versioned_config \
      "${STACK_NAME}_grafana_swarm_nodes_dashboard" \
      deploy/grafana/dashboards/swarm-nodes.json
  )"

  export CLOUDRU_SECRETS_SCRIPT_CONFIG
  export POSTGRES_INIT_SQL_CONFIG
  export POSTGRES_INIT_SCRIPT_CONFIG
  export PROMETHEUS_CONFIG
  export TEMPO_CONFIG
  export GRAFANA_DATASOURCES_CONFIG
  export GRAFANA_DASHBOARDS_CONFIG
  export GRAFANA_SWARM_NODES_DASHBOARD_CONFIG
}

validate_node_labels() {
  log "Validate Swarm node labels"

  local label
  for label in "${REQUIRED_NODE_LABELS[@]}"; do
    if ! sudo docker node ls \
      --filter "node.label=${label}" \
      --format '{{.Status}}' |
      grep -q '^Ready$'; then
      echo "No ready Swarm node has label ${label}."
      exit 1
    fi
  done

  local traefik_manager_found=false
  local node_id
  for node_id in $(sudo docker node ls \
    --filter "node.label=workload.traefik=true" \
    --format '{{.ID}}'); do
    if [ "$(sudo docker node inspect "$node_id" --format '{{.Spec.Role}}')" = "manager" ] &&
      [ "$(sudo docker node inspect "$node_id" --format '{{.Status.State}}')" = "ready" ]; then
      traefik_manager_found=true
      break
    fi
  done

  if [ "$traefik_manager_found" != "true" ]; then
    echo "No ready Swarm manager has label workload.traefik=true."
    exit 1
  fi

  local portainer_manager_count=0
  for node_id in $(sudo docker node ls \
    --filter "node.label=management.portainer=true" \
    --format '{{.ID}}'); do
    if [ "$(sudo docker node inspect "$node_id" --format '{{.Spec.Role}}')" = "manager" ] &&
      [ "$(sudo docker node inspect "$node_id" --format '{{.Status.State}}')" = "ready" ] &&
      [ "$(sudo docker node inspect "$node_id" --format '{{.Spec.Availability}}')" = "active" ]; then
      portainer_manager_count=$((portainer_manager_count + 1))
    fi
  done

  if [ "$portainer_manager_count" -ne 1 ]; then
    echo "Exactly one active ready Swarm manager must have label management.portainer=true."
    exit 1
  fi

  local postgres_node_count=0
  for node_id in $(sudo docker node ls \
    --filter "node.label=infra.postgres=true" \
    --format '{{.ID}}'); do
    if [ "$(sudo docker node inspect "$node_id" --format '{{.Status.State}}')" = "ready" ] &&
      [ "$(sudo docker node inspect "$node_id" --format '{{.Spec.Availability}}')" = "active" ]; then
      postgres_node_count=$((postgres_node_count + 1))
    fi
  done

  if [ "$postgres_node_count" -ne 1 ]; then
    echo "Exactly one active ready Swarm node must have label infra.postgres=true."
    exit 1
  fi

  local labels
  for node_id in $(sudo docker node ls --format '{{.ID}}'); do
    if [ "$(sudo docker node inspect "$node_id" --format '{{.Status.State}}')" != "ready" ] ||
      [ "$(sudo docker node inspect "$node_id" --format '{{.Spec.Availability}}')" != "active" ]; then
      continue
    fi

    labels="$(sudo docker node inspect "$node_id" --format '{{json .Spec.Labels}}')"
    if [[ "$labels" == *'"secrets.sync":"false"'* ]] &&
      {
        [[ "$labels" =~ \"workload\.(gateway|api|worker)\":\"true\" ]] ||
          [[ "$labels" == *'"infra.postgres":"true"'* ]]
      }; then
      echo "Node $(sudo docker node inspect "$node_id" --format '{{.Description.Hostname}}') hosts config-consuming workloads but has secrets.sync=false."
      exit 1
    fi
  done
}

eligible_sync_nodes() {
  local node_id
  local sync_label

  for node_id in $(sudo docker node ls --format '{{.ID}}'); do
    if [ "$(sudo docker node inspect "$node_id" --format '{{.Status.State}}')" != "ready" ] ||
      [ "$(sudo docker node inspect "$node_id" --format '{{.Spec.Availability}}')" != "active" ]; then
      continue
    fi

    sync_label="$(
      sudo docker node inspect "$node_id" \
        --format '{{index .Spec.Labels "secrets.sync"}}'
    )"
    if [ "$sync_label" != "false" ]; then
      sudo docker node inspect "$node_id" --format '{{.Description.Hostname}}'
    fi
  done
}

print_global_job_diagnostics() {
  local service="$1"

  echo "--- ${service} tasks ---"
  sudo docker service ps "$service" --no-trunc || true
  echo "--- ${service} logs ---"
  sudo docker service logs "$service" --raw --tail 200 || true
}

wait_for_global_job() {
  local service="$1"
  local display_name="$2"
  local eligible
  local eligible_count
  eligible="$(eligible_sync_nodes)"
  eligible_count="$(printf '%s\n' "$eligible" | sed '/^$/d' | wc -l)"

  if [ "$eligible_count" -eq 0 ]; then
    echo "No active eligible Swarm nodes are available for ${display_name}."
    return 1
  fi

  for _ in $(seq 1 120); do
    local completed=0
    local failed=0
    local task_name
    local node
    local current_state
    local error
    local state

    while IFS='|' read -r task_name node current_state error; do
      case "$task_name" in
        \\_*|_*) continue ;;
      esac

      if ! printf '%s\n' "$eligible" | grep -Fxq "$node"; then
        continue
      fi

      state="${current_state%% *}"
      case "$state" in
        Complete)
          completed=$((completed + 1))
          ;;
        Failed|Rejected)
          echo "${display_name} failed on node ${node}: ${current_state}${error:+; ${error}}"
          failed=1
          ;;
      esac
    done < <(
      sudo docker service ps "$service" \
        --no-trunc \
        --format '{{.Name}}|{{.Node}}|{{.CurrentState}}|{{.Error}}'
    )

    if [ "$failed" -ne 0 ]; then
      print_global_job_diagnostics "$service"
      return 1
    fi

    if [ "$completed" -eq "$eligible_count" ]; then
      echo "${display_name} completed on ${completed} eligible node(s)."
      return 0
    fi

    sleep 2
  done

  echo "${display_name} did not complete on every eligible node in time."
  print_global_job_diagnostics "$service"
  return 1
}

ensure_configs_path_on_nodes() {
  local service="${STACK_NAME}-configs-path-${DEPLOY_RUN_ID}"

  if [[ "$CONFIGS_PATH" != /* ]] ||
    [ "$CONFIGS_PATH" = "/" ] ||
    [[ "$CONFIGS_PATH" == *"/../"* ]] ||
    [[ "$CONFIGS_PATH" == *"/.." ]]; then
    echo "CONFIGS_PATH must be a safe absolute path other than /."
    exit 1
  fi

  sudo docker service rm "$service" >/dev/null 2>&1 || true
  ACTIVE_DIRECTORY_SERVICE="$service"

  sudo docker service create \
    --name "$service" \
    --mode global \
    --detach=true \
    --restart-condition none \
    --constraint "node.labels.secrets.sync != false" \
    --env CONFIGS_PATH="$CONFIGS_PATH" \
    --mount type=bind,src=/,dst=/host \
    "$CONFIGS_PATH_INIT_IMAGE" \
    sh -ec 'mkdir -p "/host${CONFIGS_PATH}"' >/dev/null

  wait_for_global_job "$service" "CONFIGS_PATH initialization"
  sudo docker service rm "$service" >/dev/null
  ACTIVE_DIRECTORY_SERVICE=""
}

wait_for_secrets_sync() {
  wait_for_global_job \
    "$(service_name secrets secrets-sync)" \
    "Application config synchronization"
}

sync_application_configs() {
  if ! cloudru_sync_is_configured; then
    echo "WARNING: Continuing deployment with the existing files in ${CONFIGS_PATH}." >&2
    return 0
  fi

  if ! deploy_stack secrets; then
    echo "WARNING: Unable to deploy the secrets-sync service." >&2
    echo "WARNING: Continuing deployment with the existing files in ${CONFIGS_PATH}." >&2
    return 0
  fi

  if ! wait_for_secrets_sync; then
    echo "WARNING: Cloud.ru application config synchronization failed." >&2
    echo "WARNING: Continuing deployment with the existing files in ${CONFIGS_PATH}." >&2
    return 0
  fi
}

validate_walg_storage() {
  local service="${STACK_NAME}-walg-preflight-${DEPLOY_RUN_ID}"
  local image="${ARTIFACT_REGISTRY_HOST}/postgres-cron:${IMAGE_TAG}"

  sudo docker service rm "$service" >/dev/null 2>&1 || true
  ACTIVE_WALG_PREFLIGHT_SERVICE="$service"

  docker_sudo service create \
    --name "$service" \
    --detach=true \
    --restart-condition none \
    --constraint "node.labels.infra.postgres == true" \
    --network "$BACKEND_NETWORK" \
    --env WALG_CONFIG_PATH=/run/secrets/postgres-walg.json \
    --secret source="$POSTGRES_WALG_SECRET",target=postgres-walg.json,mode=0444 \
    --entrypoint /bin/sh \
    --with-registry-auth \
    "$image" \
    -ec \
    'config=/run/secrets/postgres-walg.json
    if [ ! -s "$config" ]; then
      echo "WAL-G config secret is missing or empty." >&2
      exit 1
    fi
    if ! grep -Eq '"'"'"WALG_S3_PREFIX"[[:space:]]*:'"'"' "$config"; then
      echo "WAL-G config does not contain WALG_S3_PREFIX." >&2
      exit 1
    fi
    exec /usr/local/bin/wal-g --config "$config" backup-list' >/dev/null

  for _ in $(seq 1 60); do
    local state
    state="$(sudo docker service ps "$service" \
      --no-trunc \
      --format '{{.CurrentState}}|{{.Error}}' |
      head -n 1)"

    case "$state" in
      Complete*)
        sudo docker service rm "$service" >/dev/null
        ACTIVE_WALG_PREFLIGHT_SERVICE=""
        echo "WAL-G S3 storage is accessible."
        return 0
        ;;
      Failed*|Rejected*|Shutdown*non-zero\ exit*|*non-zero\ exit*|*exit\ \(*)
        echo "$state"
        print_service_diagnostics "$service"
        return 1
        ;;
    esac

    sleep 2
  done

  echo "WAL-G storage preflight did not complete in time."
  print_service_diagnostics "$service"
  return 1
}

cleanup_stale_migrators() {
  log "Clean stale migrator services"

  local id
  local name
  while read -r id name; do
    case "$name" in
      "${STACK_NAME}-"*-migrator-*)
        echo "Removing stale migrator service ${name}."
        sudo docker service rm "$id" >/dev/null
        ;;
    esac
  done < <(sudo docker service ls --format '{{.ID}} {{.Name}}')
}

print_service_diagnostics() {
  local name="$1"

  echo "--- ${name} tasks ---"
  sudo docker service ps "$name" --no-trunc || true

  echo "--- ${name} logs ---"
  sudo docker service logs "$name" --raw --tail 200 || true
}

wait_for_stack_removed() {
  local name="$1"

  for _ in $(seq 1 60); do
    if [ -z "$(sudo docker service ls \
      --filter "label=com.docker.stack.namespace=${name}" \
      --format '{{.ID}}')" ]; then
      return 0
    fi

    sleep 2
  done

  echo "Stack ${name} services were not removed in time."
  return 1
}

migrate_legacy_stack_if_needed() {
  local legacy_postgres="${STACK_NAME}_pgql"
  local split_postgres
  split_postgres="$(service_name infra postgres)"

  if sudo docker service inspect "$split_postgres" >/dev/null 2>&1 ||
    ! sudo docker service inspect "$legacy_postgres" >/dev/null 2>&1; then
    return
  fi

  if [ "$MIGRATE_LEGACY_STACK" != "true" ]; then
    echo "Legacy stack ${STACK_NAME} is still deployed."
    echo "Set MIGRATE_LEGACY_STACK=true for the one-time split-stack migration."
    exit 1
  fi

  log "Remove legacy monolithic stack"
  sudo docker stack rm "$STACK_NAME"
  wait_for_stack_removed "$STACK_NAME"
}

wait_for_postgres() {
  log "Wait for PostgreSQL"

  for attempt in $(seq 1 60); do
    if sudo docker run --rm --network "$BACKEND_NETWORK" postgres:18 \
      pg_isready -h postgres -p 5432 -U "$PGQL_USER" >/dev/null 2>&1; then
      echo "PostgreSQL is ready."
      return 0
    fi

    if [ "$attempt" -eq 60 ]; then
      echo "PostgreSQL did not become ready in time."
      print_service_diagnostics "$(service_name infra postgres)"
      return 1
    fi

    sleep 5
  done
}

wait_for_service_running() {
  local name="$1"
  local attempts="${2:-60}"
  local expected_image="${3:-}"

  for _ in $(seq 1 "$attempts"); do
    local desired_running
    local current_running
    local expected_replicas
    local configured_image
    local update_status
    local update_state
    local update_message

    desired_running="$(sudo docker service ps "$name" \
      --filter desired-state=running \
      --format '{{.ID}}' | wc -l)"
    current_running="$(sudo docker service ps "$name" \
      --filter desired-state=running \
      --format '{{.CurrentState}}' | grep -c '^Running' || true)"
    update_status="$(sudo docker service inspect "$name" \
      --format '{{if .UpdateStatus}}{{.UpdateStatus.State}}|{{.UpdateStatus.Message}}{{end}}' \
      2>/dev/null || true)"
    update_state="${update_status%%|*}"
    update_message="${update_status#*|}"

    if [ "$update_state" = "paused" ] ||
      [ "$update_state" = "rollback_paused" ]; then
      echo "${name} update failed: ${update_message}"
      print_service_diagnostics "$name"
      return 1
    fi

    if [ -n "$expected_image" ]; then
      if [ "$update_state" = "rollback_started" ] ||
        [ "$update_state" = "rollback_completed" ]; then
        echo "${name} rolled back instead of deploying ${expected_image}: ${update_message}"
        print_service_diagnostics "$name"
        return 1
      fi

      # With start-first updates the old task remains running while the new
      # task starts. Do not treat that old task as a successful deployment.
      if [ "$update_state" != "completed" ]; then
        sleep 5
        continue
      fi

      configured_image="$(sudo docker service inspect "$name" \
        --format '{{.Spec.TaskTemplate.ContainerSpec.Image}}')"
      case "$configured_image" in
        "$expected_image"|"${expected_image}@"*) ;;
        *)
          sleep 5
          continue
          ;;
      esac

      expected_replicas="$(sudo docker service inspect "$name" \
        --format '{{if .Spec.Mode.Replicated}}{{.Spec.Mode.Replicated.Replicas}}{{end}}')"
      desired_running="$(sudo docker service ps "$name" \
        --filter desired-state=running \
        --format '{{.Image}}' |
        awk -v image="$expected_image" '$0 == image || index($0, image "@") == 1 { count++ } END { print count + 0 }')"
      current_running="$(sudo docker service ps "$name" \
        --filter desired-state=running \
        --format '{{.Image}}|{{.CurrentState}}' |
        awk -F '|' -v image="$expected_image" \
          '($1 == image || index($1, image "@") == 1) && $2 ~ /^Running/ { count++ } END { print count + 0 }')"

      if [ -n "$expected_replicas" ] &&
        [ "$desired_running" -eq "$expected_replicas" ] &&
        [ "$current_running" -eq "$expected_replicas" ]; then
        return 0
      fi
    elif [ "$update_state" != "updating" ] &&
      [ "$update_state" != "rollback_started" ] &&
      [ "$desired_running" -gt 0 ] &&
      [ "$desired_running" -eq "$current_running" ]; then
      return 0
    fi

    sleep 5
  done

  echo "${name} did not become running in time."
  print_service_diagnostics "$name"
  return 1
}

wait_for_selected_service_health() {
  local service="$1"
  local swarm_service

  case "$service" in
    gateway)
      swarm_service="$(service_name gateway gateway)"
      if ! wait_for_http Gateway "$BACKEND_NETWORK" "http://gateway:8080/health"; then
        print_service_diagnostics "$swarm_service"
        return 1
      fi
      ;;
    *-api)
      swarm_service="$(service_name apps "$service")"
      if ! wait_for_http "$service" "$BACKEND_NETWORK" "http://${service}:8080/health"; then
        print_service_diagnostics "$swarm_service"
        return 1
      fi
      ;;
  esac
}

wait_for_services_running() {
  local item

  for item in "$@"; do
    wait_for_service_running \
      "$(service_name "${item%%:*}" "${item##*:}")" \
      60
  done
}

validate_portainer_agents() {
  log "Validate connectivity to every Portainer Agent task"

  local agent_service
  local expected_agents
  local service
  agent_service="$(service_name gateway portainer-agent)"
  expected_agents="$(sudo docker service ps "$agent_service" \
    --filter desired-state=running \
    --format '{{.ID}}' |
    wc -l)"
  service="${STACK_NAME}-portainer-agent-preflight-${DEPLOY_RUN_ID}"

  if [ "$expected_agents" -eq 0 ]; then
    echo "No running Portainer Agent tasks were found."
    return 1
  fi

  sudo docker service rm "$service" >/dev/null 2>&1 || true
  ACTIVE_PORTAINER_PREFLIGHT_SERVICE="$service"

  sudo docker service create \
    --name "$service" \
    --detach=true \
    --restart-condition none \
    --constraint "node.labels.management.portainer == true" \
    --network "$PORTAINER_AGENT_NETWORK" \
    --env EXPECTED_AGENTS="$expected_agents" \
    --entrypoint python \
    "$CONFIGS_PATH_INIT_IMAGE" \
    -c \
    'import http.client
import os
import socket
import ssl
import sys
import time

host = "tasks.portainer-agent"
port = 9001
expected = int(os.environ["EXPECTED_AGENTS"])
context = ssl._create_unverified_context()
deadline = time.monotonic() + 60

while True:
    addresses = sorted({
        item[4][0]
        for item in socket.getaddrinfo(host, port, type=socket.SOCK_STREAM)
    })
    failed = []

    if len(addresses) != expected:
        failed.append(
            f"expected {expected} address(es), resolved {len(addresses)}: "
            + ", ".join(addresses)
        )
    else:
        for address in addresses:
            try:
                connection = http.client.HTTPSConnection(
                    address,
                    port,
                    timeout=5,
                    context=context,
                )
                connection.request("GET", "/ping", headers={"Host": host})
                response = connection.getresponse()
                response.read()
                connection.close()
                if response.status != 204:
                    failed.append(f"{address}: HTTP {response.status}")
            except Exception as error:
                failed.append(f"{address}: {error}")

    if not failed:
        print(f"All {len(addresses)} Portainer Agent task(s) are reachable.")
        sys.exit(0)

    if time.monotonic() >= deadline:
        print(
            "Unreachable Portainer Agent task(s): " + "; ".join(failed),
            file=sys.stderr,
        )
        sys.exit(1)

    time.sleep(2)' >/dev/null

  for _ in $(seq 1 60); do
    local state
    state="$(sudo docker service ps "$service" \
      --no-trunc \
      --format '{{.CurrentState}}|{{.Error}}' |
      head -n 1)"

    case "$state" in
      Complete*)
        sudo docker service logs "$service" --raw --tail 20 || true
        sudo docker service rm "$service" >/dev/null
        ACTIVE_PORTAINER_PREFLIGHT_SERVICE=""
        return 0
        ;;
      Failed*|Rejected*|Shutdown*non-zero\ exit*|*non-zero\ exit*|*exit\ \(*)
        echo "$state"
        print_service_diagnostics "$service"
        return 1
        ;;
    esac

    sleep 2
  done

  echo "Portainer Agent connectivity preflight did not complete in time."
  print_service_diagnostics "$service"
  return 1
}

cleanup_legacy_portainer_network() {
  local legacy_network="${STACK_NAME}-gateway-portainer-agent"
  local gateway_stack
  local owner
  gateway_stack="$(stack_name gateway)"

  if [ "$legacy_network" = "$PORTAINER_AGENT_NETWORK" ] ||
    ! sudo docker network inspect "$legacy_network" >/dev/null 2>&1; then
    return
  fi

  owner="$(sudo docker network inspect "$legacy_network" \
    --format '{{index .Labels "com.docker.stack.namespace"}}')"
  if [ "$owner" != "$gateway_stack" ]; then
    echo "Legacy network ${legacy_network} was not removed because it is not owned by stack ${gateway_stack}."
    return
  fi

  if sudo docker network rm "$legacy_network" >/dev/null 2>&1; then
    echo "Removed unused legacy Portainer network ${legacy_network}."
  else
    echo "Legacy network ${legacy_network} is still in use and was left unchanged."
  fi
}

wait_for_http() {
  local display_name="$1"
  local network="$2"
  local url="$3"
  local credentials="${4:-}"
  local insecure="${5:-false}"
  local curl_output

  for attempt in $(seq 1 60); do
    local args=(
      --fail
      --silent
      --show-error
      --connect-timeout 3
      --max-time 10
    )

    if [ -n "$credentials" ]; then
      args+=(--user "$credentials")
    fi

    if [ "$insecure" = "true" ]; then
      args+=(--insecure)
    fi

    if curl_output="$(sudo docker run \
      --rm \
      --network "$network" \
      "$CURL_IMAGE" \
      "${args[@]}" \
      "$url" 2>&1)"; then
      echo "${display_name} is ready."
      return 0
    fi

    if [ "$attempt" -eq 1 ] ||
      [ $((attempt % 10)) -eq 0 ]; then
      echo "${display_name} readiness attempt ${attempt}/60 failed: ${curl_output}" >&2
    fi

    if [ "$attempt" -eq 60 ]; then
      echo "${display_name} did not become ready in time."
      return 1
    fi

    sleep 5
  done
}

wait_for_redis() {
  log "Wait for Redis"

  for attempt in $(seq 1 60); do
    if sudo docker run \
      --rm \
      --network "$BACKEND_NETWORK" \
      redis/redis-stack:latest \
      redis-cli \
      -h redis \
      -a "$REDIS_PASSWORD" \
      --no-auth-warning \
      ping 2>/dev/null |
      grep -q '^PONG$'; then
      echo "Redis is ready."
      return 0
    fi

    if [ "$attempt" -eq 60 ]; then
      echo "Redis did not become ready in time."
      print_service_diagnostics "$(service_name infra redis)"
      return 1
    fi

    sleep 5
  done
}

wait_for_infrastructure() {
  log "Wait for infrastructure services"
  wait_for_services_running "${INFRA_SERVICES[@]}"
  wait_for_postgres
  wait_for_redis
  wait_for_http \
    RabbitMQ \
    "$BACKEND_NETWORK" \
    "http://rabbitmq:15672/api/health/checks/alarms" \
    "${RABBITMQ_DEFAULT_USER}:${RABBITMQ_DEFAULT_PASS}"
  wait_for_http \
    OpenSearch \
    "$BACKEND_NETWORK" \
    "https://opensearch:9200/_cluster/health?wait_for_status=yellow&timeout=10s" \
    "admin:${OPENSEARCH_PASSWORD}" \
    true
}

wait_for_application_endpoints() {
  log "Wait for application health endpoints"
  wait_for_http Traefik "$PUBLIC_NETWORK" "http://traefik:8080/ping"
  wait_for_http Portainer "$PUBLIC_NETWORK" "http://portainer:9000/api/status"
  wait_for_http Gateway "$BACKEND_NETWORK" "http://gateway:8080/health"
  wait_for_http "Main API" "$BACKEND_NETWORK" "http://main-api:8080/health"
  wait_for_http "Analytics API" "$BACKEND_NETWORK" "http://analytics-api:8080/health"
  wait_for_http "Pricing API" "$BACKEND_NETWORK" "http://pricing-api:8080/health"
  wait_for_http "Search API" "$BACKEND_NETWORK" "http://search-api:8080/health"
  wait_for_http "Gateway metrics" "$BACKEND_NETWORK" "http://gateway:8080/metrics"
  wait_for_http "Main API metrics" "$BACKEND_NETWORK" "http://main-api:8080/metrics"
  wait_for_http "Analytics API metrics" "$BACKEND_NETWORK" "http://analytics-api:8080/metrics"
  wait_for_http "Pricing API metrics" "$BACKEND_NETWORK" "http://pricing-api:8080/metrics"
  wait_for_http "Search API metrics" "$BACKEND_NETWORK" "http://search-api:8080/metrics"
}

wait_for_monitoring_endpoints() {
  log "Wait for monitoring endpoints"
  wait_for_http Prometheus "$MONITORING_NETWORK" "http://prometheus:9090/-/ready"
  wait_for_http Loki "$MONITORING_NETWORK" "http://loki:3100/ready"
  wait_for_http Tempo "$MONITORING_NETWORK" "http://tempo:3200/ready"
  wait_for_http Grafana "$MONITORING_NETWORK" "http://grafana:3000/api/health"
  wait_for_http \
    "OpenSearch Dashboards" \
    "$MONITORING_NETWORK" \
    "http://opensearch-dashboards:5601/api/status" \
    "admin:${OPENSEARCH_PASSWORD}"
}

run_migrator() {
  local migrator_name="$1"
  local database_name="$2"
  local name="${STACK_NAME}-${migrator_name}-${DEPLOY_RUN_ID}"
  local image="${ARTIFACT_REGISTRY_HOST}/${migrator_name}:${IMAGE_TAG}"

  log "Run ${migrator_name}"
  sudo docker service rm "$name" >/dev/null 2>&1 || true
  ACTIVE_MIGRATOR_SERVICE="$name"

  docker_sudo service create \
    --name "$name" \
    --detach=true \
    --restart-condition none \
    --constraint "$MIGRATOR_NODE_CONSTRAINT" \
    --network "$BACKEND_NETWORK" \
    --env DOTNET_ENVIRONMENT="$DOTNET_ENVIRONMENT" \
    --env ASPNETCORE_ENVIRONMENT="$ASPNETCORE_ENVIRONMENT" \
    --env "ConnectionString=Host=postgres;Port=5432;Database=${database_name};Username=${PGQL_USER};Password=${PGQL_PASSWORD}" \
    --mount type=bind,src="$CONFIGS_PATH",dst=/app/configs,readonly \
    --with-registry-auth \
    "$image" >/dev/null

  for _ in $(seq 1 120); do
    local state
    state="$(sudo docker service ps "$name" \
      --no-trunc \
      --format '{{.CurrentState}}|{{.Error}}' |
      head -n 1)"

    case "$state" in
      Complete*)
        sudo docker service logs "$name" --raw --tail 100 || true
        sudo docker service rm "$name" >/dev/null
        ACTIVE_MIGRATOR_SERVICE=""
        return 0
        ;;
      Failed*|Rejected*|Shutdown*non-zero\ exit*|*non-zero\ exit*|*exit\ \(*)
        echo "$state"
        print_service_diagnostics "$name"
        sudo docker service rm "$name" >/dev/null
        ACTIVE_MIGRATOR_SERVICE=""
        return 1
        ;;
    esac

    sleep 5
  done

  echo "${migrator_name} did not complete in time."
  print_service_diagnostics "$name"
  sudo docker service rm "$name" >/dev/null
  ACTIVE_MIGRATOR_SERVICE=""
  return 1
}

run_migrators() {
  local item

  for item in "${MIGRATORS[@]}"; do
    run_migrator "${item%%:*}" "${item##*:}"
  done
}

deploy_stack() {
  local stack="$1"

  log "Deploy ${stack} stack"
  docker_sudo stack deploy \
    --detach=true \
    --prune \
    -c "deploy/stack.${stack}.rendered.yml" \
    "$(stack_name "$stack")" \
    --with-registry-auth
}

deploy_selected_services() {
  local selected_services
  local service
  local migrator
  local item
  local swarm_service
  local migrator_found
  local runtime_found
  local -a updated_services=()
  IFS=',' read -r -a selected_services <<< "$DEPLOY_SERVICES"

  service_is_selected() {
    local expected="$1"
    local selected

    for selected in "${selected_services[@]}"; do
      if [ "$selected" = "$expected" ]; then
        return 0
      fi
    done

    return 1
  }

  rollback_updated_runtime() {
    local updated_service
    local rollback_failed=false

    if [ "${#updated_services[@]}" -eq 0 ]; then
      return 0
    fi

    log "Roll back runtime services"

    for updated_service in "${updated_services[@]}"; do
      if ! docker_sudo service rollback \
        --detach=true \
        "$updated_service"; then
        echo "Unable to start rollback for ${updated_service}." >&2
        rollback_failed=true
      fi
    done

    for updated_service in "${updated_services[@]}"; do
      if ! wait_for_service_running "$updated_service" 60; then
        rollback_failed=true
      fi
    done

    [ "$rollback_failed" = false ]
  }

  update_runtime_service() {
    local definition="$1"
    local stack="${definition%%:*}"
    local runtime_service="${definition##*:}"
    local runtime_swarm_service
    local runtime_image
    local -a update_args=(
      service update
      --detach=true
      --force
      --env-add "DEPLOY_RUN_ID=${DEPLOY_RUN_ID}"
      --update-failure-action pause
      --update-monitor 30s
      --update-max-failure-ratio 0
      --rollback-failure-action pause
      --rollback-monitor 30s
      --rollback-max-failure-ratio 0
      --with-registry-auth
    )

    runtime_swarm_service="$(service_name "$stack" "$runtime_service")"

    if service_is_selected "$runtime_service"; then
      runtime_image="${ARTIFACT_REGISTRY_HOST}/${runtime_service}:${IMAGE_TAG}"
      update_args+=(--image "$runtime_image")
    fi

    if ! docker_sudo "${update_args[@]}" "$runtime_swarm_service"; then
      echo "Unable to update ${runtime_swarm_service}." >&2
      return 1
    fi

    updated_services+=("$runtime_swarm_service")
  }

  wait_for_runtime_service() {
    local definition="$1"
    local stack="${definition%%:*}"
    local runtime_service="${definition##*:}"
    local runtime_swarm_service
    local runtime_expected_image=""

    runtime_swarm_service="$(service_name "$stack" "$runtime_service")"

    if service_is_selected "$runtime_service"; then
      runtime_expected_image="${ARTIFACT_REGISTRY_HOST}/${runtime_service}:${IMAGE_TAG}"
    fi

    wait_for_service_running \
      "$runtime_swarm_service" \
      60 \
      "$runtime_expected_image" &&
      wait_for_selected_service_health "$runtime_service"
  }

  deploy_runtime_wave() {
    local display_name="$1"
    local definition
    shift

    log "Deploy runtime wave: ${display_name}"

    # Start every service in the wave before waiting, so independent services
    # can roll in parallel while dependent waves remain behind a health barrier.
    for definition in "$@"; do
      update_runtime_service "$definition" || return 1
    done

    for definition in "$@"; do
      wait_for_runtime_service "$definition" || return 1
    done
  }

  deploy_runtime_plan() {
    local entry
    local priority
    local definition
    local current_priority=""
    local -a wave=()

    for entry in "${RUNTIME_DEPLOY_PLAN[@]}"; do
      priority="${entry%%:*}"
      definition="${entry#*:}"

      if ! [[ "$priority" =~ ^[0-9]+$ ]]; then
        echo "Invalid runtime deploy priority in ${entry}." >&2
        return 1
      fi

      if [ -n "$current_priority" ] &&
        [ "$priority" -lt "$current_priority" ]; then
        echo "Runtime deploy plan must be ordered by priority." >&2
        return 1
      fi

      if [ -n "$current_priority" ] &&
        [ "$priority" != "$current_priority" ]; then
        deploy_runtime_wave \
          "priority ${current_priority}: ${wave[*]}" \
          "${wave[@]}" || return 1
        wave=()
      fi

      current_priority="$priority"
      wave+=("$definition")
    done

    if [ "${#wave[@]}" -gt 0 ]; then
      deploy_runtime_wave \
        "priority ${current_priority}: ${wave[*]}" \
        "${wave[@]}"
    fi
  }

  for service in "${selected_services[@]}"; do
    if [ -z "$service" ]; then
      continue
    fi

    runtime_found=false
    for item in "${APP_SERVICES[@]}"; do
      if [ "${item##*:}" = "$service" ]; then
        runtime_found=true
        break
      fi
    done

    migrator_found=false
    for migrator in "${MIGRATORS[@]}"; do
      if [ "${migrator%%:*}" = "$service" ]; then
        migrator_found=true
        break
      fi
    done

    if [ "$runtime_found" != true ] && [ "$migrator_found" != true ]; then
      echo "Unsupported affected service: ${service}" >&2
      return 1
    fi
  done

  for item in "${APP_SERVICES[@]}"; do
    swarm_service="$(service_name "${item%%:*}" "${item##*:}")"
    if ! sudo docker service inspect "$swarm_service" >/dev/null 2>&1; then
      echo "Swarm service ${swarm_service} does not exist." >&2
      return 1
    fi
  done

  log "Run affected database migrations"

  for service in "${selected_services[@]}"; do
    case "$service" in
      *-migrator)
        migrator_found=false
        for migrator in "${MIGRATORS[@]}"; do
          if [ "${migrator%%:*}" != "$service" ]; then
            continue
          fi

          run_migrator "${migrator%%:*}" "${migrator##*:}"
          migrator_found=true
          break
        done

        if [ "$migrator_found" != "true" ]; then
          echo "Unsupported affected migrator: ${service}" >&2
          return 1
        fi
        ;;
    esac
  done

  if ! deploy_runtime_plan; then
    rollback_updated_runtime || true
    return 1
  fi
}

show_stack_services() {
  log "Stack services"

  local stack
  for stack in "${STACK_FILES[@]}"; do
    sudo docker stack services "$(stack_name "$stack")"
  done
}

config_is_used() {
  local config_id="$1"
  local service_ids
  service_ids="$(sudo docker service ls -q)"

  if [ -z "$service_ids" ]; then
    return 1
  fi

  sudo docker service inspect $service_ids \
    --format '{{range .Spec.TaskTemplate.ContainerSpec.Configs}}{{.ConfigID}}{{"\n"}}{{end}}' |
    grep -Fxq "$config_id"
}

cleanup_unused_versioned_configs() {
  local prefix
  local config_id
  local config_name
  local current_configs

  current_configs="$(
    printf '%s\n' \
      "$CLOUDRU_SECRETS_SCRIPT_CONFIG" \
      "$POSTGRES_INIT_SQL_CONFIG" \
      "$POSTGRES_INIT_SCRIPT_CONFIG" \
      "$PROMETHEUS_CONFIG" \
      "$TEMPO_CONFIG" \
      "$GRAFANA_DATASOURCES_CONFIG" \
      "$GRAFANA_DASHBOARDS_CONFIG" \
      "$GRAFANA_SWARM_NODES_DASHBOARD_CONFIG"
  )"

  for prefix in \
    cloudru_secrets_script_ \
    "${STACK_NAME}_postgres_init_sql_" \
    "${STACK_NAME}_postgres_init_script_" \
    "${STACK_NAME}_prometheus_config_" \
    "${STACK_NAME}_tempo_config_" \
    "${STACK_NAME}_grafana_datasources_" \
    "${STACK_NAME}_grafana_dashboards_" \
    "${STACK_NAME}_grafana_swarm_nodes_dashboard_"; do
    while read -r config_id config_name; do
      [ -n "$config_id" ] || continue

      if printf '%s\n' "$current_configs" | grep -Fxq "$config_name"; then
        continue
      fi

      if config_is_used "$config_id"; then
        echo "Keeping Docker config ${config_name}: it is still used by a service."
        continue
      fi

      sudo docker config rm "$config_id" >/dev/null
      echo "Removed unused Docker config ${config_name}."
    done < <(
      sudo docker config ls \
        --format '{{.ID}} {{.Name}}' |
        awk -v prefix="$prefix" 'index($2, prefix) == 1'
    )
  done
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
require_env REDIS_PASSWORD
require_env RABBITMQ_DEFAULT_USER
require_env RABBITMQ_DEFAULT_PASS
require_env OPENSEARCH_PASSWORD
require_env CONFIGS_PATH
require_env DOCKER_CONFIG

PUBLIC_NETWORK="${PUBLIC_NETWORK:-public}"
BACKEND_NETWORK="${BACKEND_NETWORK:-backend}"
MONITORING_NETWORK="${MONITORING_NETWORK:-monitoring}"
PORTAINER_AGENT_NETWORK="${PORTAINER_AGENT_NETWORK:-portainer_agent_network}"
VOLUME_PREFIX="${VOLUME_PREFIX:-$STACK_NAME}"
MIGRATOR_NODE_CONSTRAINT="${MIGRATOR_NODE_CONSTRAINT:-node.labels.infra.postgres == true}"
MIGRATE_LEGACY_STACK="${MIGRATE_LEGACY_STACK:-false}"
CURL_IMAGE="${CURL_IMAGE:-curlimages/curl:8.12.1}"
CLOUD_RU_SECRET_PREFIX="${CLOUD_RU_SECRET_PREFIX:-/appsettings}"
CLOUD_RU_SECRET_DEPTH="${CLOUD_RU_SECRET_DEPTH:--1}"
CLOUD_RU_SECRET_NAMES="${CLOUD_RU_SECRET_NAMES:-}"
CLOUD_RU_SECRET_VERSION="${CLOUD_RU_SECRET_VERSION:-}"
CONFIGS_PATH_INIT_IMAGE="${CONFIGS_PATH_INIT_IMAGE:-python:3.13-slim}"
POSTGRES_WALG_SECRET="${POSTGRES_WALG_SECRET:-postgres_walg_config}"

export PUBLIC_NETWORK BACKEND_NETWORK MONITORING_NETWORK
export PORTAINER_AGENT_NETWORK VOLUME_PREFIX
export CLOUD_RU_SECRET_PREFIX CLOUD_RU_SECRET_DEPTH
export CLOUD_RU_SECRET_NAMES CLOUD_RU_SECRET_VERSION
export POSTGRES_WALG_SECRET

mkdir -p "$DOCKER_CONFIG"
sudo chown -R "$(id -u):$(id -g)" "$DOCKER_CONFIG"

log "[1/8] Validate Swarm manager and Docker registry"
validate_swarm_manager

if ! printf '%s' "$ARTIFACT_REGISTRY_PASSWORD" | docker login \
  "$ARTIFACT_REGISTRY_HOST" \
  --username "$ARTIFACT_REGISTRY_USERNAME" \
  --password-stdin >/dev/null; then
  if [ "${ARTIFACT_REGISTRY_FALLBACK_ENABLED:-false}" != "true" ]; then
    echo "Unable to log in to Artifact Registry ${ARTIFACT_REGISTRY_HOST}." >&2
    exit 1
  fi

  require_env BACKUP_ARTIFACT_REGISTRY_HOST
  require_env BACKUP_ARTIFACT_REGISTRY_USERNAME
  require_env BACKUP_ARTIFACT_REGISTRY_PASSWORD

  echo "Primary Artifact Registry login failed; trying backup registry."

  printf '%s' "$BACKUP_ARTIFACT_REGISTRY_PASSWORD" | docker login \
    "$BACKUP_ARTIFACT_REGISTRY_HOST" \
    --username "$BACKUP_ARTIFACT_REGISTRY_USERNAME" \
    --password-stdin >/dev/null

  ARTIFACT_REGISTRY_HOST="$BACKUP_ARTIFACT_REGISTRY_HOST"
  ARTIFACT_REGISTRY_USERNAME="$BACKUP_ARTIFACT_REGISTRY_USERNAME"
  ARTIFACT_REGISTRY_PASSWORD="$BACKUP_ARTIFACT_REGISTRY_PASSWORD"
  export ARTIFACT_REGISTRY_HOST
  export ARTIFACT_REGISTRY_USERNAME
  export ARTIFACT_REGISTRY_PASSWORD
fi

if [ "${DEPLOY_RUNTIME:-false}" = true ]; then
  log "Prepare and synchronize application configs"
  ensure_external_networks
  ensure_configs_path_on_nodes
  prepare_versioned_configs
  render_stack_files
  sync_application_configs
  deploy_selected_services
  cleanup_unused_versioned_configs
  exit 0
fi

log "[2/8] Validate required Docker Swarm secrets"
validate_required_swarm_secrets

log "[3/8] Prepare networks, volumes and versioned Docker configs"
ensure_external_networks
ensure_persistent_volumes
ensure_configs_path_on_nodes
prepare_versioned_configs
render_stack_files

log "[4/8] Synchronize application configs"
sync_application_configs

log "[5/8] Validate Swarm placement"
validate_node_labels
validate_walg_storage
migrate_legacy_stack_if_needed

log "[6/8] Deploy infrastructure and run migrations"
cleanup_stale_migrators
deploy_stack infra
wait_for_infrastructure
run_migrators
wait_for_postgres

log "[7/8] Deploy applications, workers, gateway and monitoring"
deploy_stack gateway
deploy_stack apps
deploy_stack workers
deploy_stack monitoring

wait_for_services_running "${GATEWAY_SERVICES[@]}"
wait_for_services_running "${APP_SERVICES[@]}"
wait_for_services_running "${MONITORING_SERVICES[@]}"
validate_portainer_agents
wait_for_application_endpoints
wait_for_monitoring_endpoints

log "[8/8] Cleanup obsolete Docker configs"
cleanup_legacy_portainer_network
cleanup_unused_versioned_configs
show_stack_services
