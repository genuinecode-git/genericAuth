namespace GenericAuth.Application.Common.Models;

/// <summary>
/// Represents the status of a result, which can be mapped to HTTP status codes.
/// </summary>
public enum ResultStatus
{
    /// <summary>
    /// Operation succeeded (HTTP 200/201).
    /// </summary>
    Success,

    /// <summary>
    /// Validation or business rule violation (HTTP 400).
    /// </summary>
    ValidationError,

    /// <summary>
    /// Resource not found (HTTP 404).
    /// </summary>
    NotFound,

    /// <summary>
    /// Access forbidden (HTTP 403).
    /// </summary>
    Forbidden,

    /// <summary>
    /// Conflict with current state (HTTP 409).
    /// </summary>
    Conflict,

    /// <summary>
    /// Internal server error (HTTP 500).
    /// </summary>
    InternalError
}

public class Result
{
    public bool IsSuccess { get; }
    public string[] Errors { get; }
    public ResultStatus Status { get; init; }

    protected Result(bool isSuccess, string[] errors, ResultStatus status = ResultStatus.Success)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        Status = isSuccess ? ResultStatus.Success : status;
    }

    public static Result Success() => new(true, Array.Empty<string>());
    public static Result Failure(params string[] errors) => new(false, errors, ResultStatus.ValidationError);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string[] errors, ResultStatus status = ResultStatus.Success)
        : base(isSuccess, errors, status)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());
    public static new Result<T> Failure(params string[] errors) => new(false, default, errors, ResultStatus.ValidationError);

    /// <summary>
    /// Creates a NotFound result with the specified error message.
    /// </summary>
    public static Result<T> NotFound(string error) => new(false, default, new[] { error }, ResultStatus.NotFound);

    /// <summary>
    /// Creates a Forbidden result with the specified error message.
    /// </summary>
    public static Result<T> Forbidden(string error) => new(false, default, new[] { error }, ResultStatus.Forbidden);

    /// <summary>
    /// Creates a Conflict result with the specified error message.
    /// </summary>
    public static Result<T> Conflict(string error) => new(false, default, new[] { error }, ResultStatus.Conflict);
}
