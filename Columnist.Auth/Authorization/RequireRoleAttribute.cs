namespace Columnist.Auth.Authorization;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RequireRoleAttribute : Attribute
{
    public RequireRoleAttribute(params string[] roles)
    {
        Roles = roles;
    }

    public string[] Roles { get; }
}
