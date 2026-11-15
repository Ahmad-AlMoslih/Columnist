using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Dapper;

namespace Columnist.Users.Features.Profile;

// The profile procedures shared by the profile query and commands.
internal static class ProfileStore
{
    public static async Task<UserProfileDto?> GetAsync(IDbExecutor db, int userId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        return await db.QuerySingleOrDefaultAsync<UserProfileDto>("dbo.GetUserProfile", parameters, ct);
    }

    // Sets (or clears, with null) the user's photo and returns the old file name.
    public static async Task<string?> SavePhotoAsync(IDbExecutor db, int userId, string? photoName, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@PhotoName", photoName);
        return await db.QuerySingleOrDefaultAsync<string?>("dbo.SaveUserPhoto", parameters, ct);
    }
}
