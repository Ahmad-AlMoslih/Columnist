using System.ComponentModel.DataAnnotations;
using Columnist.Utilities.Response.Enums;
using Columnist.Utilities.Response.Errors;
using Microsoft.Data.SqlClient;

namespace Columnist.Utilities.ExceptionHandling;

// Maps an exception to a safe, user-facing error. Never exposes the exception's own message.
public static class ExceptionClassifier
{
    public static ClassifiedError Classify(Exception exception) => exception switch
    {
        UnauthorizedAccessException =>
            new(ResponseStatus.Unauthorized, ErrorClassification.AuthenticationError, ErrorMessages.SignInRequired),
        KeyNotFoundException =>
            new(ResponseStatus.NotFound, ErrorClassification.NotFoundError, ErrorMessages.NotFound),
        ValidationException =>
            new(ResponseStatus.BadRequest, ErrorClassification.ValidationError, ErrorMessages.InvalidData),
        SqlException { Number: SqlErrorNumbers.DuplicateKey or SqlErrorNumbers.DuplicateIndex } =>
            new(ResponseStatus.Conflict, ErrorClassification.ConflictError, ErrorMessages.AlreadyExists),
        SqlException { Number: SqlErrorNumbers.ForeignKeyConflict } =>
            new(ResponseStatus.Conflict, ErrorClassification.ConflictError, ErrorMessages.LinkedToOtherData),
        SqlException { Number: SqlErrorNumbers.TextTooLong or SqlErrorNumbers.TextTooLongColumn } =>
            new(ResponseStatus.BadRequest, ErrorClassification.ValidationError, ErrorMessages.ValueTooLong),
        SqlException { Number: SqlErrorNumbers.Timeout or SqlErrorNumbers.ServerNotFound or SqlErrorNumbers.ConnectionFailed } =>
            new(ResponseStatus.ServerError, ErrorClassification.InfrastructureError, ErrorMessages.ServiceUnavailable),
        _ =>
            new(ResponseStatus.ServerError, ErrorClassification.UnhandledError, ErrorMessages.UnexpectedError)
    };
}