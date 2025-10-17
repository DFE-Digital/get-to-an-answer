#!/bin/sh
until nc -z sqlserver 1433; do
  echo 'SQL Server is unavailable - sleeping'
  sleep 2
done
echo 'SQL Server is up - executing command'
exec dotnet Api.dll

