FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

# working directory in the container
WORKDIR /src

# copy to /src/Api and /src/Frontend
COPY ["./src/Frontend/Frontend.csproj", "Frontend/"]
COPY ["./src/Common/Common.csproj", "Common/"]

# Restore dependencies
RUN dotnet restore "Frontend/Frontend.csproj"

# Copy the rest of the source code
COPY *.sln .
COPY Directory.Packages.props .
COPY ./src ./

WORKDIR "/src/Frontend"
RUN dotnet build "./Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/Api
RUN dotnet publish "./Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=node /src/wwwroot ./wwwroot
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Frontend.dll"]
