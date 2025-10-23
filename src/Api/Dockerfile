FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

# working directory in the container
WORKDIR /src

# copy to /src/Api and /src/Common
COPY ["./src/Api/Api.csproj", "Api/"]
COPY ["./src/Common/Common.csproj", "Common/"]

# Restore dependencies
RUN dotnet restore "Api/Api.csproj"

# Copy the rest of the source code
COPY *.sln .
COPY Directory.Packages.props .
COPY ./src ./

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
