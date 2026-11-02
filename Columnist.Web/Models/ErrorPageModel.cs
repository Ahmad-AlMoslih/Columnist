using Columnist.Utilities.Response.Enums;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Web.Models;

public sealed record ErrorPageModel(int StatusCode, string Title, string Message)
{
    public static ErrorPageModel For(int statusCode) => (ResponseStatus)statusCode switch
    {
        ResponseStatus.BadRequest => new(statusCode, "Something's not right", ErrorMessages.InvalidData),
        ResponseStatus.Unauthorized => new(statusCode, "Access denied", ErrorMessages.SignInRequired),
        ResponseStatus.Forbidden => new(statusCode, "Not allowed", ErrorMessages.Forbidden),
        ResponseStatus.NotFound => new(statusCode, "Page not found", ErrorMessages.NotFound),
        ResponseStatus.Conflict => new(statusCode, "Conflict", ErrorMessages.Conflict),
        _ => new(statusCode, "Something went wrong", ErrorMessages.UnexpectedError)
    };
}
