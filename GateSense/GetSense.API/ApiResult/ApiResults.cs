using Infrastructure.Common.Errors;
using Infrastructure.Common.ResultPattern;
using Microsoft.AspNetCore.Mvc;

namespace GetSense.API.ApiResult;

public static class ApiResults
{
    public static IActionResult ToProblemDetails(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException();
        }

        var firstError = result.Errors.FirstOrDefault();

        var problemDetails = new ProblemDetails
        {
            Status = GetStatusCode(firstError.Type),
            Title = firstError.Code,
            Type = GetType(firstError.Type),
            Detail = firstError.Description,
            Extensions = new Dictionary<string, object?>
            {
                { "errors", result.Errors }
            }
        };

        return new ObjectResult(problemDetails);
    }

    public static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError,
        };

    public static string GetType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3", //403
            ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1", // 401 Unauthorized
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1", // 500 Internal Server Error
        };
}