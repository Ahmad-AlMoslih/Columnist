namespace Columnist.Utilities.Response.Interfaces;

public interface IAuthorizationResponse<TSelf> where TSelf : IAuthorizationResponse<TSelf>
{
    static abstract TSelf Unauthorized(string message);
    static abstract TSelf Forbidden(string message);
}
