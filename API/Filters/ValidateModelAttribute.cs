namespace DotNetAngularTemplate.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errorMessage = string.Join("; ",
                context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            context.Result = new ObjectResult(new
            {
                Success = false,
                Message = errorMessage
            })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
    }
}