using ExpenseTracker.Application.Approvals;
using ExpenseTracker.Application.Expenses;
using ExpenseTracker.Application.Reports;
using ExpenseTracker.Infrastructure.Approvals;
using ExpenseTracker.Infrastructure.Expenses;
using ExpenseTracker.Infrastructure.Persistence;
using ExpenseTracker.Infrastructure.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' not found in configuration.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        // The service registration that was missing � this fixes the 500.
        // Interface lives in Application, implementation lives in Infrastructure.
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ExpenseTracker.Application.Auth.IAuthService, ExpenseTracker.Infrastructure.Auth.AuthService>();
        services.AddScoped<ExpenseTracker.Infrastructure.Auth.JwtTokenGenerator>();

        return services;
    }
}
