using Columnist.Utilities.Response;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.API.Extensions;

public static class ApiResponseExtensions
{
    public static IActionResult ToActionResult<T>(this ApiResponse<T> response) =>
        new ObjectResult(response) { StatusCode = (int)response.Status };
}