FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8082

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
RUN mkdir /app/https
RUN dotnet dev-certs https -ep /app/https/aspnetapp.pfx -p e2e

FROM base AS final
WORKDIR /app
COPY --from=node /src/wwwroot ./wwwroot
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Frontend.dll"]
