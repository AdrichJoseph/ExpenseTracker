# Copilot instructions for ExpenseTracker

- This is a Clean Architecture .NET 10 solution with four layers: `ExpenseTracker.Domain`, `ExpenseTracker.Application`, `ExpenseTracker.Infrastructure`, and `ExpenseTracker.Api`.
- `ExpenseTracker.Domain` contains entities, enums, and domain exceptions. Business behavior lives on entities, e.g. `ExpenseTracker.Domain.Entities.Expense` implements state transitions like `Submit()`, `Approve()`, `Reject()`, and `MarkReimbursed()`.
- `ExpenseTracker.Infrastructure` is the persistence layer. `AppDbContext` in `ExpenseTracker.Infrastructure/Persistence/AppDbContext.cs` configures EF Core and overrides `SaveChanges` to enforce soft delete and timestamp updates.
- `ExpenseTracker.Infrastructure/DependencyInjection.cs` exposes `AddInfrastructure(builder.Configuration)` and currently wires SQLite from `ExpenseTracker.Api/appsettings.Development.json` using `ConnectionStrings:Default`.
- `ExpenseTracker.Api/Program.cs` is the application startup. It registers controllers, Swagger, health checks, and infrastructure, then maps `/health` and Swagger UI in Development.
- There are currently no controller classes in `ExpenseTracker.Api`; the API surface is minimal and likely still being built.
- A seeder exists at `ExpenseTracker.Infrastructure/Persistence/Seed/DataSeeder.cs` with stable GUIDs and sample users/categories/expenses, but it is not currently wired into startup.

Build and run:
- `dotnet build ExpenseTracker.slnx`
- `dotnet run --project ExpenseTracker.Api`
- Local database is SQLite at `Data Source=expensetracker.db` from `appsettings.Development.json`.

Important conventions:
- Domain model is "rich" and enforces invariants on the entity itself rather than by external services.
- Entities use protected setters for `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, and soft-delete is implemented in `Domain.Common.Entity` plus `AppDbContext` change tracking.
- EF Core entity configurations are discovered by `modelBuilder.ApplyConfigurationsFromAssembly(...)` in `AppDbContext`.
- The repo currently has no test project under the solution, even though `readme.md` mentions xUnit and Moq.

When editing:
- Keep business rules inside domain entities where possible.
- Match the current startup pattern: `Program.cs` bootstraps services, `DependencyInjection.cs` extends DI registration, and persistence lives in `Infrastructure`.
- Avoid introducing direct repository or query patterns unless they are wired through an application-level service interface first.
