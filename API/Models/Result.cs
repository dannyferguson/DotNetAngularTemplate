namespace DotNetAngularTemplate.Models;

public class Result
{
    public bool IsSuccess { get; private set; }
    public string SuccessMessage { get; private set; }
    public string ErrorMessage { get; private set; }

    public Result(bool isSuccess, string successMessage, string errorMessage)
    {
        IsSuccess = isSuccess;
        SuccessMessage = successMessage;
        ErrorMessage = errorMessage;
    }

    public static Result Success(string message = "Success")
    {
        return new Result(true, message, "");
    }

    public static Result Failure(string message = "Failure")
    {
        return new Result(false, "", message);
    }
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public string SuccessMessage { get; }
    public string ErrorMessage { get; }
    public T? Value { get; }

    private Result(bool isSuccess, string successMessage, string errorMessage, T? value)
    {
        IsSuccess = isSuccess;
        SuccessMessage = successMessage;
        ErrorMessage = errorMessage;
        Value = value;
    }

    public static Result<T> Success(T value, string message = "Success") =>
        new(true, message, "", value);

    public static Result<T> Failure(string message = "Failure") =>
        new(false, "", message, default);
}