using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Common;

// Represents the authenticated caller. Built from JWT claims in the controller,
// passed down to services so they can filter data by who's asking.
public record CurrentUser(Guid Id, Role Role);