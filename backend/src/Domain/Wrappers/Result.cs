namespace Domain.Wrappers;

using Domain.Errors;

public class Result<TValue>
{
    public bool IsSuccess { get; }
    public TValue? Value { get; }
    public Error? Error { get; }

    private Result(TValue value) => (IsSuccess, Value, Error) = (true, value, null);
    private Result(Error error) => (IsSuccess, Value, Error) = (false, default, error);

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(Error error) => new(error);
}