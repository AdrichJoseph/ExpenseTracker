namespace ExpenseTracker.Domain.Common;

// Abstract = can't be instantiated directly. You can only create subclasses
// like `User : Entity` or `Expense : Entity`. This forces every concrete
// entity in our system to inherit the common fields below.
public abstract class Entity
{
    // Guid = globally unique 128-bit identifier. We use Guids instead of
    // auto-incrementing ints because:
    //  1) They can be generated client-side without a DB round-trip.
    //  2) They don't leak business info ("oh, this company has 47 expenses").
    //  3) They're safer in distributed systems.
    public Guid Id { get; protected set; } = Guid.NewGuid();

    // The "set" is `protected` not `public` — outside code can read these
    // but only the entity itself (or a subclass) can mutate them.
    // This prevents controllers from accidentally rewriting timestamps.
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    // Soft-delete: instead of DELETE FROM Expenses, we set IsDeleted = true
    // and filter it out in queries. Audit/compliance friendly.
    // The brief calls this out as a requirement.
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }

    // Domain method (not a property) — verbs go on the entity itself.
    // This is "rich domain model" style: behavior lives next to data.
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
}