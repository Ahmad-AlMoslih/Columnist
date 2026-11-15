using Columnist.Auth.Security.CurrentUser;
using Columnist.DataAccess;
using Columnist.Services.Dms;
using Columnist.Utilities.Response;
using MediatR;

namespace Columnist.Users.Features.Profile.Commands.SavePhoto;

public sealed class SavePhotoHandler : IRequestHandler<SavePhotoCommand, ApiResponse<bool>>
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private readonly IUserContext _userContext;

    public SavePhotoHandler(IDbExecutor db, IDmsService dms, IUserContext userContext)
    {
        _db = db;
        _dms = dms;
        _userContext = userContext;
    }

    public async Task<ApiResponse<bool>> Handle(SavePhotoCommand request, CancellationToken ct)
    {
        // The new file is deleted automatically on an early return or exception, unless committed.
        using var upload = _dms.BeginUpload(DmsFolder.Profile);
        var result = await upload.AddAsync(request.Photo!, ct);
        if (!result.Success)
            return ApiResponse<bool>.BadRequest(result.Error!, nameof(SavePhotoCommand.Photo));

        var oldPhoto = await ProfileStore.SavePhotoAsync(_db, _userContext.UserId!.Value, result.FileName, ct);
        upload.Commit();

        _dms.Delete(DmsFolder.Profile, oldPhoto);
        return ApiResponse<bool>.Success(true);
    }
}
