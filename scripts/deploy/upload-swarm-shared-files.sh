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
    "mkdir -p '${SWARM_PATH}' && rm -rf '${SWARM_PATH}/configs' '${SWARM_PATH}/init' && mkdir -p '${SWARM_PATH}/configs' '${SWARM_PATH}/init' '${SWARM_PATH}/certs'"

  scp $SSH_OPTS -r \
    deploy-payload/configs deploy-payload/init deploy-payload/prometheus.yml \
    "$user@$target_host:${SWARM_PATH}/"
done
