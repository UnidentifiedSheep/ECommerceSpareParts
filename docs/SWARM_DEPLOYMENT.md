# Docker Swarm deployment

Production uses one Docker Swarm cluster and six independently deployed stacks:

| Stack | Services |
| --- | --- |
| `${STACK_NAME}-secrets` | Global one-shot Cloud.ru configuration sync |
| `${STACK_NAME}-infra` | PostgreSQL, Redis, RabbitMQ and OpenSearch |
| `${STACK_NAME}-apps` | Main, Analytics, Pricing and Search APIs |
| `${STACK_NAME}-workers` | Main, Analytics and Pricing workers |
| `${STACK_NAME}-monitoring` | Prometheus, Grafana, Loki and OpenSearch Dashboards |
| `${STACK_NAME}-gateway` | Traefik, Gateway/YARP, Portainer CE and global Portainer agents |

MinIO is intentionally not deployed. Applications must use the configured external S3-compatible storage.

## Create the cluster

Run on the manager using its Tailscale address:

```bash
docker swarm init --advertise-addr <manager-tailscale-ip>
docker swarm join-token worker
```

Run the command printed by `join-token` on every worker:

```bash
docker swarm join \
  --token <worker-token> \
  <manager-tailscale-ip>:2377
```

Verify the cluster on the manager:

```bash
docker node ls
```

Swarm nodes must be able to reach each other on TCP `2377`, TCP/UDP `7946`, and UDP `4789`. Tailscale/Headscale connects physical nodes only. Docker services continue to use overlay networking and Docker DNS; do not put Tailscale node IPs into application connection strings.

Production deployment connects over SSH only to the manager. It does not need SSH access to workers.

## External overlay networks

Create the networks once on the manager:

```bash
docker network create --driver overlay --attachable public
docker network create --driver overlay --attachable backend
docker network create --driver overlay --attachable monitoring
```

The deployment script also creates missing networks idempotently and rejects an existing network if it is not a Swarm overlay network.

Internal services use stable aliases such as `postgres`, `redis`, `rabbitmq`, `opensearch`, `main-api`, `pricing-api`, `search-api`, `analytics-api`, `gateway`, `prometheus`, and `loki`.

## Node labels

One node may carry several labels:

```bash
docker node update --label-add workload.traefik=true <manager-node>
docker node update --label-add workload.gateway=true <gateway-node>
docker node update --label-add workload.api=true <api-node>
docker node update --label-add workload.worker=true <worker-node>
docker node update --label-add management.portainer=true <portainer-manager-node>

docker node update --label-add infra.postgres=true <postgres-node>
docker node update --label-add infra.redis=true <redis-node>
docker node update --label-add infra.rabbitmq=true <rabbitmq-node>
docker node update --label-add infra.opensearch=true <opensearch-node>

docker node update --label-add monitoring.grafana=true <grafana-node>
docker node update --label-add monitoring.loki=true <loki-node>
docker node update --label-add monitoring.prometheus=true <prometheus-node>
docker node update --label-add monitoring.opensearch-dashboards=true <dashboards-node>
```

`workload.traefik=true` must be assigned to a manager because Traefik reads the local Docker socket to discover Swarm services. `workload.gateway=true` belongs to Gateway/YARP and may be assigned to a worker node.

Exactly one active, ready manager must carry `management.portainer=true`. Portainer Server keeps its state in a node-local external volume, so the label gives it a stable placement. Portainer Agent does not need a label: its global service runs on every Linux Swarm node.

Generic workload labels are kept only for services that can be freely distributed within a class of nodes. Infrastructure and monitoring use specialized labels because their services are stateful or comparatively heavy. Migrators run on the node labeled `infra.postgres=true`.

`secrets-sync` runs on every active, ready node unless it has:

```bash
docker node update --label-add secrets.sync=false <node>
```

A node with `secrets.sync=false` must not carry `workload.gateway=true`, `workload.api=true`, `workload.worker=true`, or `infra.postgres=true`, because those workloads and migrators read `/app/configs`.

The deploy script automatically creates `CONFIGS_PATH` on every eligible node before running `secrets-sync`. It uses a temporary global one-shot service with the host root mounted at `/host`, executes only `mkdir -p`, verifies successful completion on every node, and immediately removes the service. `CONFIGS_PATH` must be a safe absolute path other than `/`.

## Docker Swarm secrets

Cloud.ru credentials are external Swarm secrets and are created only once on the manager:

```bash
printf '%s' '<cloudru-key-id>' |
  docker secret create cloudru_key_id -

printf '%s' '<cloudru-key-secret>' |
  docker secret create cloudru_key_secret -
```

Do not place these values in GitHub Actions. A deployment fails before changing application services when either secret is absent.

PostgreSQL backups require one additional external secret. Create a local JSON file as described in [PostgreSQL backup and recovery](POSTGRES_BACKUP.md), then create the secret:

```bash
docker secret create postgres_walg_config ./postgres-walg.json
```

The default secret name can be changed through `POSTGRES_WALG_SECRET`. This allows credential rotation with a new immutable Swarm secret.

## GitHub configuration

Required GitHub Secrets:

- `SWARM_LEADER_HOST`
- `SWARM_SSH_USER`
- `SWARM_SSH_KEY`
- `ARTIFACT_REGISTRY_HOST`
- `ARTIFACT_REGISTRY_USERNAME`
- `ARTIFACT_REGISTRY_PASSWORD`
- `SWARM_ENV`

Existing optional Secrets:

- `SWARM_PATH`, default `/opt/ecommerce`
- `SWARM_STACK_NAME`, default `ecommerce`

Required GitHub Variable:

- `CLOUD_RU_SECRET_PROJECT_ID`

Optional GitHub Variables:

- `CLOUD_RU_SECRET_PREFIX`, default `/appsettings`
- `CLOUD_RU_SECRET_DEPTH`, default `-1`
- `CLOUD_RU_SECRET_NAMES`
- `CLOUD_RU_SECRET_VERSION`

`SWARM_ENV` keeps the existing non-Cloud.ru stack values, including database/cache/broker credentials, public host names, Traefik ACME email, and optional replica/network/volume settings. It must include the public Portainer host:

```dotenv
PORTAINER_HOST=portainer.example.com
```

WAL-G scheduling values may also be placed in `SWARM_ENV`:

- `WALG_BACKUP_HOUR_UTC`, default `2`;
- `WALG_RETENTION_FULL_BACKUPS`, default `7`;
- `WALG_BACKUP_RETRY_SECONDS`, default `900`;
- `WALG_CREATE_INITIAL_BACKUP`, default `true`;
- `POSTGRES_WALG_SECRET`, default `postgres_walg_config`.

The following old GitHub Secrets are no longer needed:

- `SWARM_WORKER_HOSTS`;
- Cloud.ru key ID and key secret, if they were stored in GitHub;
- `CLOUD_RU_SECRET_PROJECT_ID`, `CLOUD_RU_SECRET_PREFIX`, `CLOUD_RU_SECRET_DEPTH`, `CLOUD_RU_SECRET_NAMES`, and `CLOUD_RU_SECRET_VERSION` after the non-secret values are moved to GitHub Variables.

Per-worker SSH credentials are no longer used. Cloud.ru credentials are no longer passed through GitHub Actions.

## Configuration synchronization

The existing `scripts/deploy/load-secrets.py` remains the Cloud.ru client and JSON generator. The deploy script:

1. calculates the script's SHA-256 hash;
2. creates immutable Docker config `cloudru_secrets_script_<short-hash>` when missing;
3. passes its exact name to `stack.secrets.yml`;
4. deploys or updates the global `secrets-sync` service;
5. waits for the current task on every eligible active node to reach `Complete`;
6. stops immediately on `Failed` or `Rejected`;
7. deploys application services only after all nodes succeed.

The sync task writes into `${CONFIGS_PATH}/.next`. It also requires at least one generated JSON file. Only a successful, non-empty Cloud.ru fetch replaces the files in `${CONFIGS_PATH}`, so a failed fetch does not erase the previous working configuration.

The Python process is one-shot and exits after writing JSON. Therefore the completed global service normally shows `0/N` running replicas; task state `Complete` is the success criterion. Raw loader output is suppressed inside the task because the current Python client can include credentials in an authentication error URL. Docker logs contain only a safe generic failure message, so deployment diagnostics can print task status and service logs without exposing credentials.

Every production run sets a unique `DEPLOY_RUN_ID=<run-id>-<attempt>`. Since APIs, workers, Gateway, and `secrets-sync` include it in their service environment, `docker stack deploy` creates new tasks even if `IMAGE_TAG` did not change.

Docker configs are immutable. PostgreSQL init files and `prometheus.yml` use the same content-hash approach. After a fully successful deployment, old versioned configs are deleted only when no Swarm service references them.

## Persistent volumes

Stateful data uses external volumes named with `VOLUME_PREFIX`, which defaults to `STACK_NAME`:

```text
ecommerce_traefik_letsencrypt
ecommerce_portainer_data
ecommerce_pg_data
ecommerce_redis_data
ecommerce_rabbitmq_data
ecommerce_opensearch_data
ecommerce_grafana_data
ecommerce_loki_data
```

The default `local` volume driver stores data on one node. Before moving a stateful placement label, migrate its volume data or configure shared storage. Stack removal does not delete these external volumes.

Exactly one active ready node must carry `infra.postgres=true`. PostgreSQL and its backup service share the node-local `pg_data` volume, so allowing multiple eligible PostgreSQL nodes could place them over different local volumes. Portainer has the same node-local storage consideration: migrate `portainer_data` before moving `management.portainer=true` to another manager.

## Portainer CE

`portainer-agent` runs in global mode on every Linux Swarm node and connects to Portainer Server through a dedicated encrypted overlay network. Portainer Server runs as one replica on the manager labeled `management.portainer=true`.

The Portainer UI is available only through Traefik at `https://${PORTAINER_HOST}`. The stack does not publish Portainer ports `9000`, `9443`, `9001`, or `8000` on host nodes. Edge Agent features that require port `8000` are therefore not enabled.

On the first launch, open the UI and create the initial administrator promptly. If Portainer expires the initial setup session, restart only its service:

```bash
docker service update --force ecommerce-gateway_portainer
```

The global agents mount the Docker socket and Docker volume directory, which gives Portainer administrative access to every Swarm node. Protect the public hostname and administrator account accordingly.

See the official [Portainer CE Swarm installation](https://docs.portainer.io/2.33-lts/start/install-ce/server/swarm/linux) and [Traefik reverse proxy](https://docs.portainer.io/2.33-lts/advanced/reverse-proxy/traefik) documentation.

## PostgreSQL backups

PostgreSQL continuously archives WAL segments to S3 using `archive_command`. A separate `postgres-backup` service on the same node:

- verifies S3 access;
- creates an initial full backup when the backup prefix is empty;
- creates a full backup daily at `02:00 UTC` by default;
- retains the latest seven full backups;
- retries a failed backup without deleting the last successful backup.

Before changing the infra stack, deployment runs a one-shot `wal-g backup-list` preflight using the configured Swarm secret. Deployment stops if the S3 storage cannot be reached.

See [PostgreSQL backup and recovery](POSTGRES_BACKUP.md) for S3 configuration, credential rotation, monitoring and restoration.

## First production run

1. Initialize the Swarm and join workers.
2. Apply all workload and specialized labels.
3. Create `cloudru_key_id`, `cloudru_key_secret`, and `postgres_walg_config`.
4. Create the three overlay networks, or let the deploy script create them.
5. Create/verify the external persistent volumes, or let the deploy script create missing volumes.
6. Configure the GitHub Secrets and Variables above.
7. Run the production workflow. It creates `CONFIGS_PATH` automatically on eligible nodes.

For the one-time transition from the old monolithic `${STACK_NAME}` stack, `MIGRATE_LEGACY_STACK=true` lets the script remove it only after configuration sync and placement validation succeed. The new infra stack then reuses the explicitly named external volumes. Back up stateful volumes before this transition.

## Deployment flow

Old flow:

```text
GitHub runner -> Cloud.ru -> generated JSON
GitHub runner -> SCP manager
GitHub runner -> SSH/SCP every worker
manager -> deploy stacks
```

New flow:

```text
GitHub runner -> SSH/SCP manager (stack, script and static deployment files)
manager -> validate Swarm, secrets, networks and versioned configs
Swarm global initializer -> mkdir -p CONFIGS_PATH on each eligible node
Swarm global secrets-sync -> Cloud.ru -> each eligible node CONFIGS_PATH
manager -> wait for every sync task to complete
manager -> deploy infra -> migrators -> apps/workers/gateway/monitoring
manager -> cleanup only unused old versioned configs
```

The deploy script uses `set -euo pipefail`, checks real task states rather than relying on a fixed sleep, validates service readiness, and removes temporary migrator services on exit.

## Operational caveats

- The automatic directory initializer temporarily mounts the host root read-write. Its command is restricted to creating the validated `CONFIGS_PATH`, and the service is removed immediately after all tasks complete.
- Every eligible node needs outbound access to Cloud.ru and access to pull `python:3.13-slim`.
- Node-local persistent volumes do not follow a label to another physical node.
- A completed one-shot `secrets-sync` service showing zero running tasks is expected.
- A credentials-related sync failure intentionally suppresses raw Python output; inspect Cloud.ru credentials by rotating the Swarm secrets rather than printing them.
- Traefik is the only service publishing ports `80` and `443`. Portainer agents, Portainer Server, PostgreSQL, Redis, RabbitMQ internal ports, OpenSearch API, Prometheus, and Loki remain private behind overlay networks.
