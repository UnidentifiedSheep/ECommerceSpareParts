#!/usr/bin/env bash
set -euo pipefail

if [ -z "${SWARM_WORKER_HOSTS:-}" ]; then
  echo "SWARM_WORKER_HOSTS is empty, skipping worker upload."
  exit 0
fi

printf '%s\n' "$SWARM_SSH_KEY" > swarm_key.pem
chmod 600 swarm_key.pem
ssh-keygen -y -f swarm_key.pem > /dev/null

SSH_OPTS="-i swarm_key.pem -o BatchMode=yes -o IdentitiesOnly=yes -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -o LogLevel=ERROR -o ConnectTimeout=20"

for host in $SWARM_WORKER_HOSTS; do
  user="$SWARM_SSH_USER"
  target_host="$host"

  if echo "$host" | grep -q '@'; then
    user="${host%@*}"
    target_host="${host#*@}"
  fi

  echo "Uploading shared configs to worker node"

  ssh $SSH_OPTS "$user@$target_host" \
    "mkdir -p '${SWARM_PATH}' '${SWARM_PATH}/certs' && \
     rm -rf '${SWARM_PATH}/configs.next' '${SWARM_PATH}/init.next' && \
     mkdir -p '${SWARM_PATH}/configs.next' '${SWARM_PATH}/init.next'"

  scp $SSH_OPTS -r \
    deploy-payload/configs/. \
    "$user@$target_host:${SWARM_PATH}/configs.next/"

  scp $SSH_OPTS -r \
    deploy-payload/init/. \
    "$user@$target_host:${SWARM_PATH}/init.next/"

  scp $SSH_OPTS \
    deploy-payload/prometheus.yml \
    "$user@$target_host:${SWARM_PATH}/prometheus.yml"

  ssh $SSH_OPTS "$user@$target_host" \
    "rm -rf '${SWARM_PATH}/configs.old' '${SWARM_PATH}/init.old' && \
     if [ -d '${SWARM_PATH}/configs' ]; then mv '${SWARM_PATH}/configs' '${SWARM_PATH}/configs.old'; fi && \
     if [ -d '${SWARM_PATH}/init' ]; then mv '${SWARM_PATH}/init' '${SWARM_PATH}/init.old'; fi && \
     mv '${SWARM_PATH}/configs.next' '${SWARM_PATH}/configs' && \
     mv '${SWARM_PATH}/init.next' '${SWARM_PATH}/init'"
done
