# Build context is backend/ (see DjMrkos.Api/Dockerfile for why).
# The Migrator has no ProjectReference to Domain/Application/Infrastructure (see
# docs/ARCHITECTURE.md#por-qué-dapper-y-no-ef-core) — it's a standalone DbUp console app,
# so this Dockerfile only ever needs its own project directory.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/DjMrkos.Migrator/DjMrkos.Migrator.csproj src/DjMrkos.Migrator/
RUN dotnet restore src/DjMrkos.Migrator/DjMrkos.Migrator.csproj

COPY src/DjMrkos.Migrator/ src/DjMrkos.Migrator/
RUN dotnet publish src/DjMrkos.Migrator/DjMrkos.Migrator.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .

# The connection string is passed as the container command (see deploy/docker-compose.prod.yml)
# rather than baked in — this image runs once per deploy, applies whatever scripts haven't
# run yet, and exits 0/1; DbUp's own bookkeeping table makes re-running it a no-op.
ENTRYPOINT ["dotnet", "DjMrkos.Migrator.dll"]
