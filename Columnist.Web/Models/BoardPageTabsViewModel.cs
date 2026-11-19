namespace Columnist.Web.Models;

// The Board · Dashboard · Report tabs (_BoardPageTabs), shown to those who can see the board's reports. Active = the page shown.
public sealed record BoardPageTabsViewModel(string BoardToken, string Active)
{
    public const string Board = "board";
    public const string Dashboard = "dashboard";
    public const string Report = "report";
}
