using GetSense.API.ApiResult;
using Infrastructure.Common.Errors;
using Infrastructure.Common.Errors.Common;
using Infrastructure.Common.ResultPattern;
using Microsoft.AspNetCore.Diagnostics;

namespace GetSense.API.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var result = ApiResults.ToProblemDetails(Result.Failure(ApplicationErrors.ApplicationError));

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = ApiResults.GetStatusCode(ApplicationErrors.ApplicationError.Type);

        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);

        return true;
    }
}