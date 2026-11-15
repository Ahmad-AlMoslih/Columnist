using Columnist.Utilities.Response;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Columnist.Users.Features.Profile.Commands.SavePhoto;

// Uploads (or replaces) the signed-in user's profile photo. The old file is deleted.
public sealed record SavePhotoCommand(IFormFile? Photo) : IRequest<ApiResponse<bool>>
{
    public const long MaxBytes = 2 * 1024 * 1024;
    public static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
}
