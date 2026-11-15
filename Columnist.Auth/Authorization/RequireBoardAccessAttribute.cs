namespace Columnist.Auth.Authorization;

// The user must be able to open the request's board (Super Admin, or a member with active access). No specific permission needed.
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RequireBoardAccessAttribute : Attribute
{
}
