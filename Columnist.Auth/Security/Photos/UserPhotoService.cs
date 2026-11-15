using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Dms;

namespace Columnist.Auth.Security.Photos;

// Profile photos (dbo.GetUserPhotos), loaded at most once per HTTP request. Registered as Scoped.
public sealed class UserPhotoService : IUserPhotoService
{
    private readonly IDbExecutor _db;
    private readonly IDmsService _dms;
    private IReadOnlyDictionary<int, string>? _urls;

    public UserPhotoService(IDbExecutor db, IDmsService dms)
    {
        _db = db;
        _dms = dms;
    }

    public async Task<IReadOnlyDictionary<int, string>> GetUrlsAsync(CancellationToken ct = default)
    {
        if (_urls is not null)
            return _urls;

        var rows = await _db.QueryAsync<UserPhotoDto>("dbo.GetUserPhotos", ct: ct);
        return _urls = rows.ToDictionary(r => r.UserId, r => _dms.GetUrl(DmsFolder.Profile, r.PhotoName)!);
    }
}
