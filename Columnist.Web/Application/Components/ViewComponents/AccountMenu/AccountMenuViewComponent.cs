using Columnist.Auth.Security.CurrentUser;
using Columnist.Users.Features.Profile.Queries.GetAccountMenu;
using Columnist.Users.Features.Profile.Queries.GetAccountMenu.Models;
using Columnist.Web.Application.Teams;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Columnist.Web.Application.Components.ViewComponents.AccountMenu;

// The account menu's button (part "button") or header (part "head") in _Layout; the profile popup also returns both for its hx-select-oob.
// GetAccountMenuQuery runs at most once per request. In the board shell (no database work) the photo and subtitle are skeletons.
public sealed class AccountMenuViewComponent : ViewComponent
{
    private const string ItemsKey = "Columnist.AccountMenu";
    private const string ButtonPath = "~/Application/Components/ViewComponents/AccountMenu/Button.cshtml";
    private const string HeadPath = "~/Application/Components/ViewComponents/AccountMenu/Head.cshtml";

    private readonly IMediator _mediator;
    private readonly IUserContext _userContext;

    public AccountMenuViewComponent(IMediator mediator, IUserContext userContext)
    {
        _mediator = mediator;
        _userContext = userContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string part)
    {
        var shell = ViewContext.ViewData["BoardShell"] is true;
        var fullName = _userContext.FullName ?? "";
        var model = new AccountMenuViewModel(
            fullName,
            string.Concat(fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => char.ToUpperInvariant(w[0]))),
            shell,
            shell ? null : await GetMenuAsync());

        return View(part == "head" ? HeadPath : ButtonPath, model);
    }

    private async Task<GetAccountMenuModel?> GetMenuAsync()
    {
        if (HttpContext.Items.TryGetValue(ItemsKey, out var cached))
            return (GetAccountMenuModel?)cached;

        var response = await _mediator.Send(new GetAccountMenuQuery(TeamCookie.Get(HttpContext.Request)), HttpContext.RequestAborted);
        var menu = response.IsSuccess ? response.Data : null;
        HttpContext.Items[ItemsKey] = menu;
        return menu;
    }
}

// Shell = board shell: skeletons instead of the photo and the subtitle (Menu is then null).
public sealed record AccountMenuViewModel(string FullName, string Initials, bool Shell, GetAccountMenuModel? Menu);
