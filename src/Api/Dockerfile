FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release

# working directory in the container
WORKDIR /src

# Copy the rest of the source code
COPY *.sln .
COPY Directory.Packages.props .
COPY ./src/Api ./Api
COPY ./src/Common ./Common
    
# Restore dependencies
RUN dotnet restore "Api/Api.csproj"

WORKDIR "/src/Api"
RUN dotnet build "./Api.csproj" -c $BUILD_CONFIGURATION  -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/Api
RUN dotnet publish "./Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
EXPOSE 8080
EXPOSE 8081
