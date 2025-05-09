namespace DotNetAngularTemplate.Models;

public class ApiResult
{
    public bool IsSuccess { get; private set; }
    public string SuccessMessage { get; private set; }
    public string ErrorMessage { get; private set; }

    public ApiResult(bool isSuccess, string successMessage, string errorMessage)
    {
        IsSuccess = isSuccess;
        SuccessMessage = successMessage;
        ErrorMessage = errorMessage;
    }

    public static ApiResult Success(string message = "Success")
    {
        return new ApiResult(true, message, "");
    }

    public static ApiResult Failure(string message = "Failure")
    {
        return new ApiResult(false, "", message);
    }
}