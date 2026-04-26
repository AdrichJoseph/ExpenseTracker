// In .NET 6+, "top-level statements" let us write code at the file level
// without a Main() method. This whole file IS the Main method.

var builder = WebApplication.CreateBuilder(args);

// ---- SERVICE REGISTRATION ----
// "Services" in ASP.NET = anything we want injected into our classes.
// We register them here; the DI container hands them out at runtime.

// Allows our controllers (we'll add them later) to be discovered.
builder.Services.AddControllers();

// Adds OpenAPI/Swagger so we get auto-generated API docs at /swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health checks — a built-in feature of ASP.NET Core.
// Calling /health will return "Healthy" if the app is responding.
// Later we'll add database checks, blob storage checks, etc.
builder.Services.AddHealthChecks();

var app = builder.Build();

// ---- HTTP REQUEST PIPELINE ----
// Middleware order matters — each piece processes the request in sequence.

// Show Swagger UI only when running in Development mode.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map the /health endpoint. Returns 200 OK + body "Healthy" by default.
app.MapHealthChecks("/health");

// Wire up the controllers we registered above.
app.MapControllers();

app.Run();
