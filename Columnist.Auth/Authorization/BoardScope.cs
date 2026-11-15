namespace Columnist.Auth.Authorization;

// The board a request is about, given by any of its tokens. Every token given must point to a board the user may use.
public readonly record struct BoardScope(string? BoardToken = null, string? ColumnToken = null, string? TaskToken = null)
{
    public bool IsEmpty => string.IsNullOrEmpty(BoardToken) && string.IsNullOrEmpty(ColumnToken) && string.IsNullOrEmpty(TaskToken);
}
