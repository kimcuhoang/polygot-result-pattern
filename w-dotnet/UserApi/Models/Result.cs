namespace UserApi.Models;

public class Result
{
    public bool IsSuccess { get; }
    public Reason? Reason { get; }

    protected Result(bool isSuccess, Reason? reason)
    {
        IsSuccess = isSuccess;
        Reason = reason;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Reason reason) => new(false, reason);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, Reason? reason): base(isSuccess, reason)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static new Result<T> Failure(Reason reason) => new(false, default, reason);
}