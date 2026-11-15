namespace Columnist.Auth.Security.Photos;

public interface IUserPhotoService
{
    // UserId → profile photo URL, for every user who has a photo.
    Task<IReadOnlyDictionary<int, string>> GetUrlsAsync(CancellationToken ct = default);
}
