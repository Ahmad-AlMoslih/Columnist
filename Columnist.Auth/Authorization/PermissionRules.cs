using Columnist.Contracts.Dtos;
using Columnist.Utilities.Response.Errors;

namespace Columnist.Auth.Authorization;

// Write needs Read in the same group: give Read first, remove Write first.
public static class PermissionRules
{
    private const string Read = "Read";
    private const string Write = "Write";

    public static string Label(PermissionDto permission) => $"{permission.GroupName} · {permission.FunctionalityName}";

    public static PermissionDto? ReadFor(PermissionDto permission, IReadOnlyList<PermissionDto> catalog) =>
        permission.FunctionalityName == Write
            ? catalog.FirstOrDefault(p => p.GroupId == permission.GroupId && p.FunctionalityName == Read)
            : null;

    public static PermissionDto? WriteFor(PermissionDto permission, IReadOnlyList<PermissionDto> catalog) =>
        permission.FunctionalityName == Read
            ? catalog.FirstOrDefault(p => p.GroupId == permission.GroupId && p.FunctionalityName == Write)
            : null;

    // Returns the error message, or null when the change is allowed.
    public static string? Check(PermissionDto permission, bool grant, IReadOnlySet<string> current, IReadOnlyList<PermissionDto> catalog)
    {
        if (grant && ReadFor(permission, catalog) is { } read && !current.Contains(read.Name))
            return string.Format(ErrorMessages.GivePermissionFirst, PermissionRules.Label(read));

        if (!grant && WriteFor(permission, catalog) is { } write && current.Contains(write.Name))
            return string.Format(ErrorMessages.RemovePermissionFirst, PermissionRules.Label(write));

        return null;
    }
}
