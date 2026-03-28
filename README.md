# ECommerce technical challenge

Mini e-commerce backend bootstrapped with DDD-oriented folders, Clean Architecture boundaries, Minimal APIs, EF Core, SQL Server, and a background worker prepared for Outbox publication.

## Current phase

Phase 1 is complete:

- solution and projects were created
- project references follow the intended dependency direction
- Web API starts with Minimal APIs and Swagger
- Infrastructure is configured with EF Core and SQL Server
- Worker is configured as a hosted service
- Docker Compose starts the API and SQL Server base services

## Solution structure

```text
src/
  ECommerce.Core/
  ECommerce.UseCases/
  ECommerce.Infrastructure/
  ECommerce.WebApi/
  ECommerce.Worker/

tests/
  ECommerce.UnitTests/
```

## Running locally

1. Start the API and SQL Server with `docker compose up --build`.
2. Or run the projects directly with:
   - `dotnet run --project src/ECommerce.WebApi`
   - `dotnet run --project src/ECommerce.Worker`

Swagger is available at `/swagger` when running the API in development.

## Runtime baseline

This repository is now aligned with `.NET 10` and pins SDK `10.0.201` via `global.json`.
