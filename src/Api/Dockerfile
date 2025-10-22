FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

# Use absolute path for clarity
WORKDIR /src

# Copy only csproj files first (paths are relative to build context = repo root)
COPY src/Api/Api.csproj Api/
COPY src/Common/Common.csproj Common/


# Copy the rest of the source from repo root's src folder into /src
COPY src .

WORKDIR /src/Api

RUN dotnet restore "Api.csproj"

RUN dotnet build "Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/Api
RUN dotnet publish "Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]