using System.Text.Json.Serialization;
using Columnist.Utilities.ExceptionHandling;
using Columnist.Utilities.Response.Enums;
using Columnist.Utilities.Response.Errors;
using Columnist.Utilities.Response.Interfaces;
using Columnist.Utilities.Response.Metadata;

namespace Columnist.Utilities.Response;

public sealed class ApiResponse<T> : IValidationResponse<ApiResponse<T>>, IAuthorizationResponse<ApiResponse<T>>
{
    private const string Version = "1.0";

    private ApiResponse(ResponseStatus status, T? data, IReadOnlyList<ApiError> errors, PaginationMetadata? pagination = null)
    {
        Status = status;
        IsSuccess = (int)status is >= 200 and < 300;
        Data = data;
        Errors = errors;
        Metadata = new ResponseMetadata(DateTime.UtcNow, Version, pagination);
    }

    // Named IsSuccess because a property can't share its name with the Success factory; still serialized as "success".
    [JsonPropertyName("success")]
    public bool IsSuccess { get; }
    public ResponseStatus Status { get; }
    public int StatusCode => (int)Status;
    public T? Data { get; }
    public IReadOnlyList<ApiError> Errors { get; }
    public ResponseMetadata Metadata { get; }

    public static ApiResponse<T> Success(T data, PaginationMetadata? pagination = null) =>
        new(ResponseStatus.Success, data, [], pagination);

    public static ApiResponse<T> Created(T data) =>
        new(ResponseStatus.Created, data, []);

    public static ApiResponse<T> BadRequest(IEnumerable<ApiError> errors) =>
        new(ResponseStatus.BadRequest, default, errors.ToList());

    public static ApiResponse<T> BadRequest(string message, string? field = null) =>
        Fail(ResponseStatus.BadRequest, ErrorClassification.ValidationError, message, field);

    public static ApiResponse<T> Unauthorized(string message = ErrorMessages.Unauthorized) =>
        Fail(ResponseStatus.Unauthorized, ErrorClassification.AuthenticationError, message);

    public static ApiResponse<T> Forbidden(string message = ErrorMessages.Forbidden) =>
        Fail(ResponseStatus.Forbidden, ErrorClassification.AuthorizationError, message);

    public static ApiResponse<T> NotFound(string message = ErrorMessages.NotFound) =>
        Fail(ResponseStatus.NotFound, ErrorClassification.NotFoundError, message);

    public static ApiResponse<T> Conflict(string message) =>
        Fail(ResponseStatus.Conflict, ErrorClassification.ConflictError, message);

    public static ApiResponse<T> ServerError(string message = ErrorMessages.UnexpectedError) =>
        Fail(ResponseStatus.ServerError, ErrorClassification.UnhandledError, message);

    public static ApiResponse<T> FromError(ClassifiedError error) =>
        Fail(error.Status, error.Classification, error.Message);

    public static ApiResponse<T> ValidationFailed(IEnumerable<ApiError> errors) => BadRequest(errors);

    private static ApiResponse<T> Fail(ResponseStatus status, ErrorClassification classification, string message, string? field = null) =>
        new(status, default, [new ApiError { Classification = classification, Field = field, Message = message }]);
}