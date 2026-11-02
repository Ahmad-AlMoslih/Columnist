using Columnist.Utilities.Response.Errors;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Columnist.Web.Application.Extensions;

public static class ModelStateExtensions
{
    public static void AddApiErrors(this ModelStateDictionary modelState, IEnumerable<ApiError> errors)
    {
        foreach (var error in errors)
            modelState.AddModelError(error.Field ?? string.Empty, error.Message);
    }
}
