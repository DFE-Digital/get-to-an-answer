#!/bin/bash
# Script to list all tables in the dbo schema using the connection string from DEFAULT_CONNECTION_STRING
set -e

CONN_STR="${DEFAULT_CONNECTION_STRING}"

# Parse connection string
server=$(echo "$CONN_STR" | sed -n 's/.*Server=\([^;,]*\).*/\1/p')
db=$(echo "$CONN_STR" | sed -n 's/.*Database=\([^;,]*\).*/\1/p')
user=$(echo "$CONN_STR" | sed -n 's/.*User Id=\([^;,]*\).*/\1/p')
password=$(echo "$CONN_STR" | sed -n 's/.*Password=\([^;,]*\).*/\1/p')
port=$(echo "$CONN_STR" | sed -n 's/.*Server=[^,]*,\([0-9]*\).*/\1/p')

# Default port to 1433 if not specified
if [[ -z "$port" ]]; then
  port=1433
fi

if [[ -z "$server" || -z "$db" || -z "$user" || -z "$password" ]]; then
  echo "Could not parse connection string."
  exit 1
fi

# Query to list tables in dbo schema
query="SET NOCOUNT ON; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';"

# Run sqlcmd with full server and port
/opt/mssql-tools18/bin/sqlcmd -S "tcp:$server,$port" -d "$db" -U "$user" -P "$password" -Q "$query"
