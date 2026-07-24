#!/usr/bin/env bash
set -euo pipefail

backup_hour_utc="${WALG_BACKUP_HOUR_UTC:-2}"
retention_count="${WALG_RETENTION_FULL_BACKUPS:-7}"
retry_seconds="${WALG_BACKUP_RETRY_SECONDS:-900}"
create_initial_backup="${WALG_CREATE_INITIAL_BACKUP:-true}"

require_positive_integer() {
    local name="$1"
    local value="$2"

    if ! [[ "$value" =~ ^[0-9]+$ ]] || [ "$value" -lt 1 ]; then
        echo "${name} must be a positive integer." >&2
        exit 1
    fi
}

if ! [[ "$backup_hour_utc" =~ ^([0-9]|1[0-9]|2[0-3])$ ]]; then
    echo "WALG_BACKUP_HOUR_UTC must be an integer from 0 to 23." >&2
    exit 1
fi

require_positive_integer WALG_RETENTION_FULL_BACKUPS "$retention_count"
require_positive_integer WALG_BACKUP_RETRY_SECONDS "$retry_seconds"

if [ "$create_initial_backup" != "true" ] &&
    [ "$create_initial_backup" != "false" ]; then
    echo "WALG_CREATE_INITIAL_BACKUP must be true or false." >&2
    exit 1
fi

: "${PGDATA:?PGDATA is required}"
: "${WALG_CONFIG_PATH:?WALG_CONFIG_PATH is required}"

seconds_until_next_backup() {
    local now
    local target
    local today

    now="$(date -u +%s)"
    today="$(date -u +%F)"
    target="$(date -u -d "${today} ${backup_hour_utc}:00:00" +%s)"

    if [ "$target" -le "$now" ]; then
        target=$((target + 86400))
    fi

    echo $((target - now))
}

run_backup() {
    while ! pg_isready --quiet; do
        echo "PostgreSQL is not ready; checking again in 5 seconds."
        sleep 5
    done

    echo "Starting WAL-G full backup."

    while ! wal-g backup-push "$PGDATA"; do
        echo "WAL-G backup failed; retrying in ${retry_seconds} seconds." >&2
        sleep "$retry_seconds"
    done

    echo "WAL-G full backup completed."

    if ! wal-g delete retain FULL "$retention_count" --confirm; then
        echo "WAL-G retention cleanup failed; the completed backup is preserved." >&2
    fi
}

echo "Checking WAL-G storage access."
backup_list="$(
    wal-g backup-list --json |
        tr -d '[:space:]'
)"

if [ "$create_initial_backup" = "true" ] &&
    { [ -z "$backup_list" ] || [ "$backup_list" = "[]" ]; }; then
    echo "No WAL-G base backup exists; creating the initial backup."
    run_backup
fi

while true; do
    wait_seconds="$(seconds_until_next_backup)"
    echo "Next WAL-G full backup starts in ${wait_seconds} seconds at ${backup_hour_utc}:00 UTC."
    sleep "$wait_seconds"
    run_backup
done
