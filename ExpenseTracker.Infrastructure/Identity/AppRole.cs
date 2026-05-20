using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Identity;

public class AppRole : IdentityRole<Guid>
{
    public AppRole() : base() { }
    public AppRole(string roleName) : base(roleName) { }
}
