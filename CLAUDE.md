# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Stack & Project Layout

- **.NET 8** target everywhere (`net8.0`) — trust the csproj files; all projects use `net8.0`. Install the **.NET 8 SDK**.
- Multi-project: `SenaPro.slnx`
  - **SenaPro.API** — ASP.NET Core Web API host + Hangfire / Swagger
  - **SenaPro.Application** — use-cases, services, DTOs
  - **SenaPro.Domain** — entities, repositories interface, business rules
  - **SenaPro.Infrastructure** — EF Core (Npgsql), repos, external integrations (`servicebus2.caixa.gov.br`), EPPlus Excel imports
  - **SenaPro.Tests** — unit tests
- `sena-pro-frontend/` — Angular (LTS) + Dockerfile

## Local dev endpoints

- Swagger UI → `http://localhost:5000/swagger` (somente ambiente **Development**)
- Hangfire Dashboard → `http://localhost:5000/hangfire` (somente ambiente **Development**)

## Pós-clone (antes de subir os containers)

1. Instalar o **.NET 8 SDK** — [dot.net/download](https://dotnet.microsoft.com/en-us/download).
2. Verificar se a imagem `node:20-alpine` está disponível localmente; caso não esteja, executar `docker pull node:20-alpine`.

## Commands

```bash
# Full build
dotnet restore && dotnet build SenaPro.slnx

# Run a single test class or method
dotnet test SenaPro.Tests --filter "Category=Unit&FullyQualifiedName~RandomGenerationTests"
dotnet test SenaPro.Tests --filter "FullyQualifiedName=SenaPro.Tests.Application.Random.GeneratorServiceTests.CalculateStatistics_WithAllAnalyzes_ShouldReturnAverageAndMostFrequent"

# All tests (also runs lint by convention in CI) — must pass before committing
dotnet test SenaPro.Tests

# Database migration / seeding for local dev
cd SenaPro.API
dotnet ef database update
dotnet run                     # → http://localhost:5000

# Frontend only
cd sena-pro-frontend
npm install
ng serve                      # → http://localhost:4200
```

## Frontend no Docker

- O container de frontend usa **nginx:alpine** para servir a build estática — nunca `ng serve` dentro do contêiner.
- A porta 4200 exposta no host está mapeada para a porta 80 interna do nginx; não há hot-reload via Docker Compose.

## Environment & DB

- Local Postgres (matching `docker-compose.yml`): user=`senapro`, password=`senapro`, db=`senapro`. **Credentials are intentionally trivial here; do not use these values in shared/dev deployments.**
- Docker Compose one-liner: `docker-compose up -d` — starts PostgreSQL, API (`:5000`), Angular dev server (`:4200`).
- DB environment variable format (used by Docker): `ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=senapro;Username=senapro;Password=senapro`. Adjust if changing containers.

## Testing conventions

- xUnit + Moq + FluentAssertions; in-memory EF Core (`Microsoft.EntityFrameworkCore.InMemory`) is the default provider in tests — no Postgres needed.
- Tests live in `SenaPro.Tests/`; structure mirrors the layer under test (e.g. application-layer tests use `Mock<IExternalLotteryService>`).

## Key implementation notes

- **TDD workflow** required: write failing test first, then minimal code to green it, then refactor with tests still passing. Do not ship a feature that has no corresponding failing test already written.
- Hangfire persists scheduled tasks against PostgreSQL — dropping the DB removes queued jobs; be aware when running `drop`/re-create flows in local dev.
