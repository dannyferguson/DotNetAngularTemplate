namespace DotNetAngularTemplate.Models;

public class ApiResultWithValue<T>
{
    public bool IsSuccess { get; }
    public string SuccessMessage { get; }
    public string ErrorMessage { get; }
    public T? Value { get; }

    private ApiResultWithValue(bool isSuccess, string successMessage, string errorMessage, T? value)
    {
        IsSuccess = isSuccess;
        SuccessMessage = successMessage;
        ErrorMessage = errorMessage;
        Value = value;
    }

    public static ApiResultWithValue<T> Success(T value, string message = "Success") =>
        new(true, message, "", value);

    public static ApiResultWithValue<T> Failure(string message = "Failure") =>
        new(false, "", message, default);
}