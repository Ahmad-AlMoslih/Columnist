using Columnist.Utilities.Response.Errors;

namespace Columnist.Utilities.Response.Interfaces;

public interface IValidationResponse<TSelf> where TSelf : IValidationResponse<TSelf>
{
    static abstract TSelf ValidationFailed(IEnumerable<ApiError> errors);
}
