namespace ExpenseTracker.Domain.Exceptions;

// Exceptions in C# inherit from `Exception`. Creating a custom type
// lets the API middleware distinguish "user did something invalid"
// (DomainException → 400) from "the database fell over" (generic Exception → 500).
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}