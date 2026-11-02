using Columnist.Utilities.ExceptionHandling;
using Microsoft.AspNetCore.Diagnostics;

namespace Columnist.Web.Application.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var error = ExceptionClassifier.Classify(exception);

        _logger.LogError(exception, "Unhandled exception ({Status}) on {Method} {Path}",
            (int)error.Status, httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.Redirect($"/Error/{(int)error.Status}");
        return ValueTask.FromResult(true);
    }
}
