FROM postgres:18

ARG WALG_VERSION=v3.0.7

RUN apt-get update && \
    apt-get install -y \
        postgresql-18-cron \
        curl \
        ca-certificates \
        tar && \
    rm -rf /var/lib/apt/lists/*

RUN curl --fail --show-error --silent --location \
    "https://github.com/wal-g/wal-g/releases/download/${WALG_VERSION}/wal-g-pg-ubuntu-20.04-amd64.tar.gz" \
    -o /tmp/wal-g.tar.gz && \
    tar -xzf /tmp/wal-g.tar.gz -C /usr/local/bin && \
    mv /usr/local/bin/wal-g-pg-ubuntu-20.04-amd64 /usr/local/bin/wal-g && \
    chmod +x /usr/local/bin/wal-g && \
    rm /tmp/wal-g.tar.gz

COPY --chmod=0555 .dockerfiles/pgql-cron/run-walg-backups.sh \
    /usr/local/bin/run-walg-backups
