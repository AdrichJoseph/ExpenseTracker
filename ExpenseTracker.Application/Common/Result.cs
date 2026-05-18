namespace ExpenseTracker.Application.Common;

// Discriminated-style result type. Wraps the outcome of a service method.
// IsSuccess = true → use Value. IsSuccess = false → use Error.
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultError ErrorKind { get; }

    private Result(bool isSuccess, T? value, string? error, ResultError errorKind)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorKind = errorKind;
    }

    public static Result<T> Success(T value) =>
        new(true, value, null, ResultError.None);

    public static Result<T> NotFound(string error) =>
        new(false, default, error, ResultError.NotFound);

    public static Result<T> Validation(string error) =>
        new(false, default, error, ResultError.Validation);

    public static Result<T> Conflict(string error) =>
        new(false, default, error, ResultError.Conflict);
}

public enum ResultError
{
    None,
    NotFound,
    Validation,
    Conflict
}