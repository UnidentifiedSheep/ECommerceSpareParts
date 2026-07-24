# PostgreSQL backup and recovery

Production PostgreSQL uses WAL-G for base backups and continuous WAL archiving to an S3-compatible object storage.

## S3 configuration

Create `postgres-walg.json` outside the repository:

```json
{
  "WALG_S3_PREFIX": "s3://database-backups/postgresql/production",
  "AWS_ENDPOINT": "https://s3.example.com",
  "AWS_REGION": "example-region-1",
  "AWS_ACCESS_KEY_ID": "replace-me",
  "AWS_SECRET_ACCESS_KEY": "replace-me",
  "AWS_S3_FORCE_PATH_STYLE": true,
  "WALG_COMPRESSION_METHOD": "zstd",
  "WALG_PREVENT_WAL_OVERWRITE": true
}
```

Replace the bucket, prefix, endpoint, region and credentials with values from the selected S3-compatible provider. The bucket must already exist; the path following the bucket name is an object prefix and does not need to be created manually.

Use a dedicated service-account key restricted to the backup bucket and prefix. Keep the bucket private and enable versioning or object lock according to the required retention policy.

Create the external Swarm secret on the manager:

```bash
chmod 600 postgres-walg.json
docker secret create postgres_walg_config ./postgres-walg.json
rm postgres-walg.json
```

Do not commit this file and do not place its contents in `SWARM_ENV`.

Docker Swarm secrets are immutable. To rotate credentials:

```bash
docker secret create postgres_walg_config_v2 ./postgres-walg-v2.json
```

Set the following in `SWARM_ENV` and deploy:

```bash
POSTGRES_WALG_SECRET=postgres_walg_config_v2
```

After a successful deployment, remove the old secret only when no service uses it:

```bash
docker secret rm postgres_walg_config
```

## Backup policy

The `postgres-backup` service uses:

```text
WALG_BACKUP_HOUR_UTC=2
WALG_RETENTION_FULL_BACKUPS=7
WALG_BACKUP_RETRY_SECONDS=900
WALG_CREATE_INITIAL_BACKUP=true
```

At startup it checks S3 using `wal-g backup-list`. When the prefix contains no base backups, it creates the initial full backup after PostgreSQL becomes ready. It then runs daily at the configured UTC hour.

PostgreSQL itself runs with:

```text
archive_mode=on
archive_command=/usr/local/bin/wal-g --config /run/secrets/postgres-walg.json wal-push "%p"
archive_timeout=60s
```

This combination provides base backups plus the WAL segments required for point-in-time recovery.

## Monitoring

Check the scheduler:

```bash
docker service ps ecommerce-infra_postgres-backup --no-trunc
docker service logs ecommerce-infra_postgres-backup --tail 200
```

Check PostgreSQL archiving:

```sql
SELECT
    archived_count,
    failed_count,
    last_archived_wal,
    last_archived_time,
    last_failed_wal,
    last_failed_time
FROM pg_stat_archiver;
```

Alert on increasing `failed_count`, an old `last_archived_time`, failed backup tasks, and the absence of recent base backups in S3.

## Recovery

Treat recovery as a destructive maintenance operation. Do not run `backup-fetch` into the active `pg_data` volume.

The safe procedure is:

1. Create a new empty restore volume on the node carrying `infra.postgres=true`.
2. Run the same `postgres-cron` image with `postgres_walg_config` and execute:

   ```bash
   wal-g backup-fetch "$PGDATA" LATEST
   ```

3. Configure `restore_command` for the restored cluster:

   ```text
   restore_command=/usr/local/bin/wal-g wal-fetch "%f" "%p"
   ```

4. For point-in-time recovery, create `recovery.signal` and set the required `recovery_target_time`, `recovery_target_lsn`, or other PostgreSQL recovery target.
5. Start an isolated PostgreSQL service against the restored volume.
6. Verify databases, application migrations, row counts and the intended recovery point.
7. Perform a controlled cutover only after verification.

Run restore drills regularly. A backup that has never been restored should not be considered verified.

WAL-G command behavior and configuration are documented in the [official WAL-G repository](https://github.com/wal-g/wal-g) and [PostgreSQL guide](https://wal-g.readthedocs.io/PostgreSQL/).
