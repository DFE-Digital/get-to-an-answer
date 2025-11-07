FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

# working directory in the container
WORKDIR /src

# Copy the rest of the source code
COPY *.sln .
COPY Directory.Packages.props .
COPY ./src/Admin ./Admin
COPY ./src/Common ./Common

# Restore dependencies
RUN dotnet restore "Admin/Admin.csproj"

WORKDIR "/src/Admin"
RUN dotnet build "./Admin.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/Admin
RUN dotnet publish "./Admin.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM node:20 AS node
WORKDIR /src
COPY "src/Admin/package.json" "src/Admin/yarn.lock" "src/Admin/Gulpfile.js" ./
COPY "src/Admin/AssetSrc/" "AssetSrc/"
RUN yarn install --ignore-scripts
RUN npm install --ignore-scripts -g gulp
RUN gulp dev

FROM base AS final
WORKDIR /app
COPY --from=node /src/wwwroot ./wwwroot
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Admin.dll"]
EXPOSE 8080
EXPOSE 8081
