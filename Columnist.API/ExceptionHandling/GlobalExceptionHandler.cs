using Columnist.Utilities.ExceptionHandling;
using Columnist.Utilities.Response;
using Microsoft.AspNetCore.Diagnostics;

namespace Columnist.API.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var error = ExceptionClassifier.Classify(exception);

        _logger.LogError(exception, "Unhandled exception ({Status}) on {Method} {Path}",
            (int)error.Status, httpContext.Request.Method, httpContext.Request.Path);

        var response = ApiResponse<object>.FromError(error);
        httpContext.Response.StatusCode = response.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}
