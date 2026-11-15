using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Profile.Commands.RemovePhoto;

// Removes the signed-in user's profile photo and deletes its file.
public sealed record RemovePhotoCommand : IRequest<ApiResponse<bool>>;
