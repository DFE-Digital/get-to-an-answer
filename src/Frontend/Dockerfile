FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Frontend/Frontend.csproj", "Frontend/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Frontend/Frontend.csproj"
COPY . .
WORKDIR "/src/Frontend"
RUN dotnet build "./Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=node /src/wwwroot ./wwwroot
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Frontend.dll"]
