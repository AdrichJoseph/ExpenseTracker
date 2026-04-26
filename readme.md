# ExpenseTracker API

A multi-user expense management REST API built with ASP.NET Core 10. Employees submit expenses with receipts, managers approve or reject them through a stateful approval workflow, and admins view aggregate reports.

## Status

🚧 Under active development.

## Architecture

Clean Architecture, four projects:

- `ExpenseTracker.Domain` — entities, enums, business rules. Zero dependencies.
- `ExpenseTracker.Application` — service interfaces, DTOs, validators.
- `ExpenseTracker.Infrastructure` — EF Core, Azure SDK, file I/O.
- `ExpenseTracker.Api` — controllers, middleware, JWT auth.

## Tech stack

- C# 13 / .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- Azure SQL Database / Azure Blob Storage / Azure Key Vault / App Insights
- xUnit + Moq for testing
- Docker
- GitHub Actions CI/CD

## Local development

Coming soon.