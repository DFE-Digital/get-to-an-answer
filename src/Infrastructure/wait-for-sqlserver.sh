#!/bin/bash
until nc -z sqlserver 1433; do
  echo "Waiting for SQL Server..."
  sleep 5
done
exec dotnet Api.dll