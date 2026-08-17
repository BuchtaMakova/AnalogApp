# Build context: ./backend

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AnalogHub.sln .
COPY src/AnalogHub.Domain/AnalogHub.Domain.csproj src/AnalogHub.Domain/
COPY src/AnalogHub.Application/AnalogHub.Application.csproj src/AnalogHub.Application/
COPY src/AnalogHub.Infrastructure/AnalogHub.Infrastructure.csproj src/AnalogHub.Infrastructure/
COPY src/AnalogHub.Api/AnalogHub.Api.csproj src/AnalogHub.Api/
RUN dotnet restore src/AnalogHub.Api/AnalogHub.Api.csproj

COPY src/ src/
RUN dotnet publish src/AnalogHub.Api/AnalogHub.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# curl backs the container HEALTHCHECK below (and docker-compose's depends_on: condition: service_healthy).
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

EXPOSE 8080
HEALTHCHECK --interval=5s --timeout=5s --start-period=20s --retries=10 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AnalogHub.Api.dll"]
