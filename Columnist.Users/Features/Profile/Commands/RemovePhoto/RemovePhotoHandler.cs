using Columnist.Auth.Security.CurrentUser;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Profile.Commands.RemovePhoto;

public sealed class RemovePhotoHandler : IRequestHandler<RemovePhotoCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IUserContext _userContext;

    public RemovePhotoHandler(IDbExecutor db, IDmsService dms, IUserContext userContext)
    {
        _db = db;
        _dms = dms;
        _userContext = userContext;
    }

    public async Task<ApiResponse<bool>> Handle(RemovePhotoCommand request, CancellationToken ct)
    {
        var oldPhoto = await ProfileStore.SavePhotoAsync(_db, _userContext.UserId!.Value, null, ct);
        _dms.Delete(DmsFolder.Profile, oldPhoto);
        return ApiResponse<bool>.Success(true);
    }
}
