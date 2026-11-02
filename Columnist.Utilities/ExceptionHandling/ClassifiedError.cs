using Columnist.Utilities.Response.Enums;

namespace Columnist.Utilities.ExceptionHandling;

public sealed record ClassifiedError(ResponseStatus Status, ErrorClassification Classification, string Message);
