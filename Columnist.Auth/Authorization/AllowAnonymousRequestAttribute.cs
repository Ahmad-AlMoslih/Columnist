namespace Columnist.Auth.Authorization;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AllowAnonymousRequestAttribute : Attribute
{
}