using Columnist.Contracts.Dtos;
using Columnist.DataAccess;
using Dapper;

namespace Columnist.Tasks.Features.Reports.Shared;

// "Activity per person" (report and CSV): the last ActivityRules.Days days, board members and the owner only,
// people with no changes left out, most changes first. Only the Assignee filter applies.
public static class BoardActivity
{
    public static async Task<IReadOnlyList<(UserOptionDto Person, ActivityRules.PersonActivity Activity)>> LoadAsync(
        IDbExecutor db, BoardReportData data, int boardId, int? assigneeId, CancellationToken ct)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@BoardId", boardId);
        parameters.Add("@Since", data.Now.AddDays(-ActivityRules.Days));
        var perPerson = ActivityRules.PerPerson(await db.QueryAsync<ActivityCountDto>("dbo.GetBoardActivityCounts", parameters, ct));

        return data.People
            .Where(p => assigneeId is null || p.Id == assigneeId)
            .Select(p => (Person: p, Activity: perPerson.GetValueOrDefault(p.Id)))
            .Where(p => p.Activity is { Changes: > 0 })
            .Select(p => (p.Person, p.Activity!))
            .OrderByDescending(p => p.Item2.Changes)
            .ThenBy(p => p.Person.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
