using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Columnist.Services.Tokens.Extensions;
using Columnist.Tasks.Features.Tasks.Queries.GetBoard.Models;
using Dapper;

namespace Columnist.Tasks.Features.Tasks.Queries.GetBoard;

// The board filter's people (GetBoard: the chosen ones; GetBoardFilterMembers: the panel's full list).
internal static class BoardFilterPeople
{
    // The people the task form offers for this board.
    public static async Task<IReadOnlyList<UserOptionDto>> GetUsersAsync(IDbExecutor db, int boardId, int? userId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId);
        parameters.Add("@TaskId", null);
        parameters.Add("@UserId", userId);
        return await db.QueryAsync<UserOptionDto>("dbo.GetUsersForDropdown", parameters, ct);
    }

    // Keep the token each person was sent with, so the open filter panel and the chips match.
    public static Dictionary<int, string> SentTokens(IEnumerable<string>? tokens)
    {
        var sent = new Dictionary<int, string>();
        foreach (var token in tokens ?? [])
        {
            var id = token.Decrypt<int>();
            if (id is not null)
                sent.TryAdd(id.Value, token);
        }
        return sent;
    }

    // The board's people without the current user.
    public static IEnumerable<BoardFilterMemberModel> Members(IEnumerable<UserOptionDto> users, int meId,
        IReadOnlyDictionary<int, string> sentTokens, IReadOnlyDictionary<int, string> photos) =>
        users
            .Where(u => u.Id != meId)
            .Select(u => new BoardFilterMemberModel
            {
                Token = sentTokens.GetValueOrDefault(u.Id) ?? u.Id.Encrypt(),
                Name = u.Name,
                PhotoUrl = photos.GetValueOrDefault(u.Id),
                HasAccess = u.HasAccess,
                Selected = sentTokens.ContainsKey(u.Id)
            });
}
