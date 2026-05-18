using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Reserved for Application-layer registrations (validators, handlers).
        // Service implementations live in Infrastructure and are registered there.
        return services;
    }
}
