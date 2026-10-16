# Docker Swarm deployment

Production uses one Docker Swarm cluster and six independently deployed stacks:

| Stack | Services |
| --- | --- |
| `${STACK_NAME}-secrets` | Global one-shot Cloud.ru configuration sync |
| `${STACK_NAME}-infra` | PostgreSQL, Redis, RabbitMQ and OpenSearch |
| `${STACK_NAME}-apps` | Main, Analytics, Pricing and Search APIs |
| `${STACK_NAME}-workers` | Main, Analytics and Pricing workers |
| `${STACK_NAME}-monitoring` | Prometheus, Grafana, Loki, Tempo and OpenSearch Dashboards |
| `${STACK_NAME}-gateway` | Traefik, Gateway/YARP and Arcane Manager |

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
docker network create --driver overlay --attachable arcane_agent_network
```

The deployment script also creates missing networks idempotently and rejects an existing network if it is not a Swarm overlay network.

Internal services use stable aliases such as `postgres`, `redis`, `rabbitmq`, `opensearch`, `main-api`, `pricing-api`, `search-api`, `analytics-api`, `gateway`, `prometheus`, `loki`, and `tempo`.

## Node labels

One node may carry several labels:

```bash
docker node update --label-add workload.traefik=true <manager-node>
docker node update --label-add workload.gateway=true <gateway-node>
docker node update --label-add workload.api=true <api-node>
docker node update --label-add workload.worker=true <worker-node>
docker node update --label-add management.arcane=true <arcane-manager-node>

docker node update --label-add infra.postgres=true <postgres-node>
docker node update --label-add infra.redis=true <redis-node>
docker node update --label-add infra.rabbitmq=true <rabbitmq-node>
docker node update --label-add infra.opensearch=true <opensearch-node>

docker node update --label-add monitoring.grafana=true <grafana-node>
docker node update --label-add monitoring.loki=true <loki-node>
docker node update --label-add monitoring.tempo=true <tempo-node>
docker node update --label-add monitoring.prometheus=true <prometheus-node>
docker node update --label-add monitoring.opensearch-dashboards=true <dashboards-node>
```

`workload.traefik=true` must be assigned to a manager because Traefik reads the local Docker socket to discover Swarm services. `workload.gateway=true` belongs to Gateway/YARP and may be assigned to a worker node.

Exactly one active, ready manager must carry `management.arcane=true`. Arcane keeps its database in the node-local external `arcane_data` volume, so the label gives it a stable placement. Node agents do not need labels: bootstrap creates a service constrained to each eligible node ID.

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

Do not place these values in GitHub Actions. When either Cloud.ru secret is absent, the deployment skips synchronization and continues with the existing files in `CONFIGS_PATH`.

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
- `BACKUP_ARTIFACT_REGISTRY_HOST`
- `BACKUP_ARTIFACT_REGISTRY_USERNAME`
- `BACKUP_ARTIFACT_REGISTRY_PASSWORD`
- `SWARM_ENV`

Existing optional Secrets:

- `SWARM_PATH`, default `/opt/ecommerce`
- `SWARM_STACK_NAME`, default `ecommerce`

Optional GitHub Variable:

- `CLOUD_RU_SECRET_PROJECT_ID`

Optional GitHub Variables:

- `DEPLOY_ENVIRONMENT`, default `Production`
- `CLOUD_RU_SECRET_PREFIX`, default `/appsettings`
- `CLOUD_RU_SECRET_DEPTH`, default `-1`
- `CLOUD_RU_SECRET_NAMES`
- `CLOUD_RU_SECRET_VERSION`

`SWARM_ENV` keeps the existing non-Cloud.ru stack values, including database/cache/broker credentials, public host names,
Traefik ACME email, and optional replica/network/volume settings. Grafana administrator values are bootstrap settings
used when `grafana_data` is initialized; an existing Grafana instance keeps its administrator in its database. For example:

```dotenv
ARCANE_HOST=arcane.example.com
RABBITMQ_HOST=rabbitmq.example.com
```

WAL-G scheduling values may also be placed in `SWARM_ENV`:

- `WALG_BACKUP_HOUR_UTC`, default `2`;
- `WALG_RETENTION_FULL_BACKUPS`, default `7`;
- `WALG_BACKUP_RETRY_SECONDS`, default `900`;
- `WALG_CREATE_INITIAL_BACKUP`, default `true`;
- `POSTGRES_WALG_SECRET`, default `postgres_walg_config`.

Deployment uses SSH access to the manager only. Cloud.ru credentials are stored in Swarm secrets.

Before building images, the publish workflow tries the primary Artifact Registry. It tries the backup only when primary
login fails. Images are built, promoted to `current`, and deployed from the selected registry only; the release is not
mirrored between registries. If neither registry accepts login, publishing stops. Runtime registry fallback is disabled
because the other registry is not guaranteed to contain the same release.

The selected registry kind is passed as a non-secret reusable-workflow output to the deploy workflow; registry
credentials are never placed in workflow outputs.

Compose and Swarm files continue to use `${ARTIFACT_REGISTRY_HOST}`. The workflow assigns that variable to the selected primary or backup host, so separate copies of stack and migrator compose files are not required.

## Deployment modes

Application-only changes use the ordered runtime deployment plan: affected images and migrators are updated, while the
complete API/worker/Gateway release is restarted in compatibility waves and checked for readiness. Changes that do not
affect a deployable image or deployment infrastructure do not start a production deployment.

Changes under `deploy/stack.*`, `deploy/grafana`, `deploy/tempo`, `scripts/deploy`, `init`, `prometheus.yml`, or the
PostgreSQL image directory require a full deployment. A manual `workflow_dispatch` does the same. Full deployment
reconciles all six Swarm stacks, applies versioned Docker configs, runs all migrators, and executes infrastructure,
application, Arcane, and monitoring health checks.

## Configuration synchronization

The existing `scripts/deploy/load-secrets.py` remains the Cloud.ru client and JSON generator. The deploy script:

1. calculates the script's SHA-256 hash;
2. creates immutable Docker config `cloudru_secrets_script_<short-hash>` when missing;
3. passes its exact name to `stack.secrets.yml`;
4. deploys or updates the global `secrets-sync` service;
5. waits for the current task on every eligible active node to reach `Complete`;
6. prints task and service diagnostics on `Failed` or `Rejected`;
7. continues the deployment with the existing files in `CONFIGS_PATH` when Cloud.ru is unavailable or synchronization fails.

The sync task writes into `${CONFIGS_PATH}/.next`. It also requires at least one generated JSON file. Only a successful, non-empty Cloud.ru fetch replaces the files in `${CONFIGS_PATH}`, so a failed fetch does not erase the previous working configuration.

`CLOUD_RU_SECRET_PROJECT_ID`, `cloudru_key_id`, and `cloudru_key_secret` are optional for an individual deployment run. If any of them is missing, synchronization is skipped with a warning. This fallback assumes that valid JSON files already exist on the target nodes; on a new node or first deployment, applications can still fail to start if `CONFIGS_PATH` is empty.

The Python process is one-shot and exits after writing JSON. Therefore the completed global service normally shows `0/N` running replicas; task state `Complete` is the success criterion. Raw loader output is suppressed inside the task because the current Python client can include credentials in an authentication error URL. Docker logs contain only a safe generic failure message, so deployment diagnostics can print task status and service logs without exposing credentials.

Every production run sets a unique `DEPLOY_RUN_ID=<run-id>-<attempt>`. Since APIs, workers, Gateway, and `secrets-sync` include it in their service environment, `docker stack deploy` creates new tasks even if `IMAGE_TAG` did not change.

Docker configs are immutable. PostgreSQL init files and `prometheus.yml` use the same content-hash approach. After a fully successful deployment, old versioned configs are deleted only when no Swarm service references them.

## Persistent volumes

Stateful data uses external volumes named with `VOLUME_PREFIX`, which defaults to `STACK_NAME`:

```text
ecommerce_traefik_letsencrypt
ecommerce_arcane_data
ecommerce_pg_data
ecommerce_redis_data
ecommerce_rabbitmq_data
ecommerce_opensearch_data
ecommerce_prometheus_data
ecommerce_grafana_data
ecommerce_loki_data
ecommerce_tempo_data
```

The default `local` volume driver stores data on one node. Before moving a stateful placement label, migrate its volume data or configure shared storage. Stack removal does not delete these external volumes.

Exactly one active ready node must carry `infra.postgres=true`. PostgreSQL and its backup service share the node-local `pg_data` volume, so allowing multiple eligible PostgreSQL nodes could place them over different local volumes. Arcane has the same node-local storage consideration: migrate `arcane_data` and preserve the encryption secret before moving `management.arcane=true` to another manager.

Prometheus retains metrics for 30 days in `prometheus_data`. Tempo retains
traces for seven days in `tempo_data`. The monitoring
stack runs `node-exporter` as a global service and discovers its tasks through
`tasks.node-exporter` on the private `monitoring` overlay network. Grafana
automatically provisions the Prometheus, Loki, and Tempo data sources and the
**Swarm / Node resources** dashboard from versioned Docker configs.

## Arcane

Arcane Manager and Agent are pinned to `v2.16.1`. Manager runs on the node labeled `management.arcane=true`; its local Docker socket provides the Swarm view and access to that node's resources. Bootstrap creates `${STACK_NAME}-arcane-agent-<node-id>` services for the other active, ready Linux nodes. These services live outside the gateway stack so stack pruning cannot remove them.

Agents use outbound WebSocket connections to `http://arcane:3552` over `arcane_agent_network`. No agent or manager ports are published on hosts. Only the manager UI is exposed through Traefik at `https://${ARCANE_HOST}`. `TRUSTED_PROXIES` is derived from the public overlay subnet; `ARCANE_TRUSTED_PROXIES` can override it when another trusted proxy is present. Physical node traffic uses Tailscale, as with the other overlays.

### First deployment

1. Set `ARCANE_HOST` in `SWARM_ENV` to the desired domain and point its DNS record to the Traefik host. Label one manager `management.arcane=true`.
2. Run a **full deployment** through the existing CI/CD workflow. Changes under `deploy/` and `scripts/deploy/` already select full deployment. The SSH manager needs `openssl` for one-time key generation. No manual Arcane installation, worker SSH, new GitHub credential, or additional host port is needed. Nodes must be able to pull `ghcr.io/getarcaneapp/manager:v2.16.1` and `ghcr.io/getarcaneapp/agent:v2.16.1`.
3. On the SSH manager used by deployment, read the initial password (default deployment directory shown):

   ```bash
   sudo cat /opt/ecommerce/.arcane-initial-password
   ```

4. Open the configured HTTPS domain and sign in as `arcane` with that password. Set your own password in the account settings. Repeated deployments preserve it.
5. Check **Swarm → Nodes**: the manager is covered locally and other eligible nodes should show connected agents. Bootstrap waits for this before completing. After adding a Swarm node or reactivating a drained node, run another full deployment to install its agent. Offline, paused, drained, and non-Linux nodes are skipped with a log message; removed-node registrations/services are not automatically deleted.
6. Add the Cloud.ru registry in Arcane's registry settings: server = the existing `ARTIFACT_REGISTRY_HOST` (without an image path), username = service-account **Key ID**, password = **Key Secret**. Use a registry-scoped service account with pull access; add delete permissions only if needed. Deleting a manifest by digest can remove multiple tags pointing to it, including `current` and `previous`, so keep these digests protected.

The deploy script creates three immutable Swarm secrets once: `${STACK_NAME}_arcane_encryption_key`, `${STACK_NAME}_arcane_admin_api_key`, and `${STACK_NAME}_arcane_initial_password`. The initial password is also saved in `.arcane-initial-password` with root-only permissions on the deployment manager; it is never printed in CI logs. Manager and agents read keys through `_FILE` variables, and per-node agent tokens have separate Swarm secrets. Keep the manager database and encryption key together in backups; losing or replacing the key makes saved registry credentials and agent tokens unreadable. Custom secret names can be set with `ARCANE_ENCRYPTION_SECRET`, `ARCANE_ADMIN_API_KEY_SECRET`, and `ARCANE_INITIAL_PASSWORD_SECRET`.

On a fresh installation the Traefik route stays disabled until bootstrap replaces Arcane's built-in default password and verifies agents. A failed deployment rolls back changed services and removes newly created Arcane agents.

Application stacks remain managed by this repository's deployment workflow. Use Arcane to inspect and operate the existing Swarm resources; saving a different stack definition in the UI can be overwritten by the next deployment.

See the official [Arcane installation](https://getarcane.app/docs/get-started/installation), [Swarm nodes and agents](https://getarcane.app/docs/remote/swarm-nodes-agents), and [reverse proxy](https://getarcane.app/docs/networking/reverse-proxy) documentation.

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
3. Create the required `postgres_walg_config` secret. Create `cloudru_key_id` and `cloudru_key_secret` when Cloud.ru synchronization is enabled.
4. Create the overlay networks, or let the deploy script create them.
5. Create/verify the external persistent volumes, or let the deploy script create missing volumes.
6. Configure the GitHub Secrets and Variables above.
7. Run the production workflow. It creates `CONFIGS_PATH` automatically on eligible nodes.

## Deployment flow

```text
GitHub runner -> check primary and backup registry login
GitHub runner -> build/push images to the selected registry
GitHub runner -> SSH/SCP manager (stack, script and static deployment files)
manager -> selected registry login
manager -> validate Swarm, secrets, networks and versioned configs
Swarm global initializer -> mkdir -p CONFIGS_PATH on each eligible node
Swarm global secrets-sync -> Cloud.ru -> each eligible node CONFIGS_PATH
manager -> wait for every sync task; on failure keep the previous CONFIGS_PATH
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
- Traefik is the only service publishing ports `80` and `443`. Arcane agents, Arcane Manager, PostgreSQL, Redis, RabbitMQ internal ports, OpenSearch API, Prometheus, and Loki remain private behind overlay networks.
