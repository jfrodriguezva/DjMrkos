# Build context is backend/ (not this directory) — the API needs its sibling projects.
# docker compose sets `context: ../backend` / `dockerfile: src/DjMrkos.Api/Dockerfile`.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project files first so `dotnet restore` is cached across builds that only
# change .cs/.sql files — the expensive NuGet restore only reruns when a .csproj changes.
COPY src/DjMrkos.Domain/DjMrkos.Domain.csproj src/DjMrkos.Domain/
COPY src/DjMrkos.Application/DjMrkos.Application.csproj src/DjMrkos.Application/
COPY src/DjMrkos.Infrastructure/DjMrkos.Infrastructure.csproj src/DjMrkos.Infrastructure/
COPY src/DjMrkos.Api/DjMrkos.Api.csproj src/DjMrkos.Api/
RUN dotnet restore src/DjMrkos.Api/DjMrkos.Api.csproj

COPY src/DjMrkos.Domain/ src/DjMrkos.Domain/
COPY src/DjMrkos.Application/ src/DjMrkos.Application/
COPY src/DjMrkos.Infrastructure/ src/DjMrkos.Infrastructure/
COPY src/DjMrkos.Api/ src/DjMrkos.Api/
RUN dotnet publish src/DjMrkos.Api/DjMrkos.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
# curl backs the container HEALTHCHECK (and is handy for debugging from inside the container);
# the aspnet runtime image doesn't ship it by default.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=5s --retries=5 CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "DjMrkos.Api.dll"]
