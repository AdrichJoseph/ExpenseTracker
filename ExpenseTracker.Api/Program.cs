using ExpenseTracker.Application;
using ExpenseTracker.Infrastructure;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---- SERVICE REGISTRATION ----
builder.Services.AddControllers();

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health checks
builder.Services.AddHealthChecks();

// Wire up Infrastructure (DbContext, repositories, services)
builder.Services.AddInfrastructure(builder.Configuration);

// Wire up Application (validators, handlers — empty for now)
builder.Services.AddApplication();

var app = builder.Build();

// ---- STARTUP TASKS (Development only) ----
// Apply migrations and seed data automatically.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await ExpenseTracker.Infrastructure.Persistence.Seed.DataSeeder.SeedAsync(db);
}

// ---- HTTP REQUEST PIPELINE ----
// Middleware order matters — each piece processes the request in sequence.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // exposes the raw OpenAPI JSON at /swagger/v1/swagger.json
    app.UseSwaggerUI();    // serves the interactive UI at /swagger
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
