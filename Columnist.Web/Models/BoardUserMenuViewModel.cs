namespace Columnist.Web.Models;

// The "This board" items of the account menu, shown on the board and its Deleted tasks, Logs and Manage pages.
public sealed record BoardUserMenuViewModel(string BoardToken, bool CanSeeDeleted, bool CanSeeLogs, bool CanManage)
{
    public bool Any => CanSeeDeleted || CanSeeLogs || CanManage;
}
