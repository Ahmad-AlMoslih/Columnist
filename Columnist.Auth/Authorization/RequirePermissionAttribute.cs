namespace Columnist.Auth.Authorization;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RequirePermissionAttribute : Attribute
{
    public RequirePermissionAttribute(Permission permission)
    {
        Permission = permission;
    }

    public Permission Permission { get; }
}
