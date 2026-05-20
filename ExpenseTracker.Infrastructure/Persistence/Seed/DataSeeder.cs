using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ExpenseTracker.Infrastructure.Identity;

namespace ExpenseTracker.Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    // Stable Guids for FK references
    private static readonly Guid AdminId    = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ManagerId  = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid EmployeeId = new("33333333-3333-3333-3333-333333333333");

    private static readonly Guid TravelCategoryId   = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid MealsCategoryId    = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid SuppliesCategoryId = new("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid SoftwareCategoryId = new("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid TrainingCategoryId = new("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

        if (await db.Users.AnyAsync()) return;

        await SeedRolesAsync(roleManager);
        await SeedCategoriesAsync(db);
        await SeedUsersAsync(userManager);
        await SeedExpensesAsync(db);
    }

    private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
    {
        // Match the names to our Role enum values so role checks work consistently.
        foreach (var name in Enum.GetNames(typeof(Role)))
        {
            if (!await roleManager.RoleExistsAsync(name))
                await roleManager.CreateAsync(new AppRole(name));
        }
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        var categories = new List<Category>
        {
            CreateCategory(TravelCategoryId,   "Travel",                 5000m),
            CreateCategory(MealsCategoryId,    "Meals",                  1000m),
            CreateCategory(SuppliesCategoryId, "Office Supplies",         500m),
            CreateCategory(SoftwareCategoryId, "Software",               2000m),
            CreateCategory(TrainingCategoryId, "Training & Conferences", 3000m),
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager)
    {
        await CreateUserAsync(userManager, AdminId,    "Alice Admin",    "alice@expensetracker.local",  "Admin123!",    Role.Admin,    "Operations",  null);
        await CreateUserAsync(userManager, ManagerId,  "Bob Manager",    "bob@expensetracker.local",    "Manager123!",  Role.Manager,  "Engineering", null);
        await CreateUserAsync(userManager, EmployeeId, "Carol Employee", "carol@expensetracker.local",  "Employee123!", Role.Employee, "Engineering", ManagerId);
    }

    private static async Task SeedExpensesAsync(AppDbContext context)
    {
        var expenses = new List<Expense>
        {
            new()
            {
                UserId = EmployeeId,
                CategoryId = TravelCategoryId,
                Amount = 487.50m,
                Currency = "CAD",
                Description = "Flight to Vancouver for client kickoff",
                ExpenseDate = DateTime.UtcNow.AddDays(-12),
            },
            new()
            {
                UserId = EmployeeId,
                CategoryId = MealsCategoryId,
                Amount = 64.25m,
                Currency = "CAD",
                Description = "Team dinner during Vancouver trip",
                ExpenseDate = DateTime.UtcNow.AddDays(-11),
            },
            new()
            {
                UserId = EmployeeId,
                CategoryId = SoftwareCategoryId,
                Amount = 199.00m,
                Currency = "CAD",
                Description = "JetBrains Rider annual license",
                ExpenseDate = DateTime.UtcNow.AddDays(-30),
            },
        };

        expenses[0].Submit();
        expenses[0].Approve(ManagerId, "Approved on receipt review");
        expenses[1].Submit();

        await context.Expenses.AddRangeAsync(expenses);
        await context.SaveChangesAsync();
    }

    private static Category CreateCategory(Guid id, string name, decimal? limit)
    {
        var c = new Category { Name = name, MonthlyLimit = limit };
        typeof(Domain.Common.Entity)
            .GetProperty(nameof(Domain.Common.Entity.Id))!
            .SetValue(c, id);
        return c;
    }

    private static async Task CreateUserAsync(
        UserManager<User> userManager,
        Guid id,
        string name,
        string email,
        string password,
        Role role,
        string dept,
        Guid? managerId)
    {
        var user = new User
        {
            Id = id,                       // explicit Guid so seed data is stable
            Name = name,
            Email = email,
            UserName = email,              // Identity uses UserName for login; we make it == email
            Role = role,
            Department = dept,
            ManagerId = managerId,
            EmailConfirmed = true          // skip email confirmation for seeded users
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not create user {email}: {errors}");
        }

        // Assign the role
        await userManager.AddToRoleAsync(user, role.ToString());
    }
}
