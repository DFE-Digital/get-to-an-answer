#!/bin/sh
# wait-for-sqlserver.sh
set -e
host="$1"
port="$2"
shift 2
until nc -z "$host" "$port"; do
  >&2 echo "SQL Server is unavailable - sleeping"
  sleep 2
done
>&2 echo "SQL Server is up - executing command"
exec "$@"
