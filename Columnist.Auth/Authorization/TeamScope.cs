namespace Columnist.Auth.Authorization;

// The team a request is about: the board's team when BoardToken is given, otherwise TeamToken's team.
public readonly record struct TeamScope(string? TeamToken = null, string? BoardToken = null)
{
    public bool IsEmpty => string.IsNullOrEmpty(TeamToken) && string.IsNullOrEmpty(BoardToken);
}
