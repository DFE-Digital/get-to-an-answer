FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

# working directory in the container
WORKDIR /src

# Copy the rest of the source code
COPY *.sln .
COPY Directory.Packages.props .
COPY ./src/Frontend ./Frontend
COPY ./src/Common ./Common

# Restore dependencies
RUN dotnet restore "Frontend/Frontend.csproj"

WORKDIR "/src/Frontend"
RUN dotnet build "./Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/Frontend
RUN dotnet publish "./Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM node:20 AS node
WORKDIR /src
COPY "src/Frontend/package.json" "src/Frontend/yarn.lock" "src/Frontend/Gulpfile.js" ./
COPY "src/Frontend/AssetSrc/" "AssetSrc/"
RUN yarn install --ignore-scripts
RUN npm install --ignore-scripts -g gulp
RUN gulp dev

FROM base AS final
WORKDIR /app
COPY --from=node /src/wwwroot ./wwwroot
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Frontend.dll"]
EXPOSE 8080
EXPOSE 8081
