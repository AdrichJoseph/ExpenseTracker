using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Seed;

// Static class with a single entry point — gets called from Program.cs at startup.
public static class DataSeeder
{
    // Hardcoded GUIDs so seed data is stable across runs.
    // (Random Guids on each run would orphan foreign keys.)
    private static readonly Guid AdminId    = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ManagerId  = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid EmployeeId = new("33333333-3333-3333-3333-333333333333");

    private static readonly Guid TravelCategoryId   = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid MealsCategoryId    = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid SuppliesCategoryId = new("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid SoftwareCategoryId = new("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid TrainingCategoryId = new("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    public static async Task SeedAsync(AppDbContext context)
    {
        // Idempotency: if any users exist, assume we've already seeded.
        // Lets the app restart cleanly without duplicating data.
        if (await context.Users.AnyAsync()) return;

        await SeedCategoriesAsync(context);
        await SeedUsersAsync(context);
        await SeedExpensesAsync(context);
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

    private static async Task SeedUsersAsync(AppDbContext context)
    {
        // NOTE: in Phase 6 we'll swap to ASP.NET Identity which hashes properly.
        // For now, "seedhash" is a placeholder. These users can't actually log in
        // until Phase 6 wires up real password hashing.
        var users = new List<User>
        {
            CreateUser(AdminId,    "Alice Admin",    "alice@expensetracker.local", Role.Admin,    "Operations",   null),
            CreateUser(ManagerId,  "Bob Manager",    "bob@expensetracker.local",   Role.Manager,  "Engineering",  null),
            CreateUser(EmployeeId, "Carol Employee", "carol@expensetracker.local", Role.Employee, "Engineering",  ManagerId),
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedExpensesAsync(AppDbContext context)
    {
        // Mix of statuses so reports have something to aggregate from day one.
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

        // Push them through the domain state machine so they land in interesting states:
        expenses[0].Submit();
        expenses[0].Approve(ManagerId, "Approved on receipt review");
        expenses[1].Submit();   // pending — shows up in /api/approvals/pending in Phase 7
        // expenses[2] stays as Draft

        await context.Expenses.AddRangeAsync(expenses);
        await context.SaveChangesAsync();
    }

    // Helper factories — `required` properties + setting Id awkwardly
    // need a small workaround. Reflection bypasses the protected setter on Entity.Id.
    private static Category CreateCategory(Guid id, string name, decimal? limit)
    {
        var c = new Category { Name = name, MonthlyLimit = limit };
        typeof(Domain.Common.Entity)
            .GetProperty(nameof(Domain.Common.Entity.Id))!
            .SetValue(c, id);
        return c;
    }

    private static User CreateUser(Guid id, string name, string email, Role role, string dept, Guid? managerId)
    {
        var u = new User
        {
            Name = name,
            Email = email,
            Role = role,
            Department = dept,
            ManagerId = managerId,
            PasswordHash = "seedhash"  // placeholder, replaced in Phase 6
        };
        typeof(Domain.Common.Entity)
            .GetProperty(nameof(Domain.Common.Entity.Id))!
            .SetValue(u, id);
        return u;
    }
}