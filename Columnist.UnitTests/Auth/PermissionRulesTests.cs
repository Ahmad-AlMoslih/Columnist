using Columnist.Auth.Authorization;
using Columnist.Contracts.Dtos;

namespace Columnist.UnitTests.Auth;

public class PermissionRulesTests
{
    // Fake catalog: "Deleted tasks" has Read + Write, "Tasks" has only Write.
    private static readonly IReadOnlyList<PermissionDto> Catalog =
    [
        new() { Id = 1, Name = "DeletedTasksRead",  GroupId = 10, GroupName = "Deleted tasks", FunctionalityName = "Read" },
        new() { Id = 2, Name = "DeletedTasksWrite", GroupId = 10, GroupName = "Deleted tasks", FunctionalityName = "Write" },
        new() { Id = 3, Name = "TasksWrite",        GroupId = 20, GroupName = "Tasks",         FunctionalityName = "Write" }
    ];

    private static PermissionDto Get(string name) => Catalog.Single(p => p.Name == name);

    [Fact]
    public void Giving_write_without_read_is_blocked()
    {
        var error = PermissionRules.Check(Get("DeletedTasksWrite"), grant: true, current: new HashSet<string>(), Catalog);

        Assert.Equal("Give \"Deleted tasks · Read\" first.", error);
    }

    [Fact]
    public void Giving_write_with_read_is_allowed()
    {
        var error = PermissionRules.Check(Get("DeletedTasksWrite"), grant: true, current: new HashSet<string> { "DeletedTasksRead" }, Catalog);

        Assert.Null(error);
    }

    [Fact]
    public void Removing_read_while_write_is_there_is_blocked()
    {
        var current = new HashSet<string> { "DeletedTasksRead", "DeletedTasksWrite" };

        var error = PermissionRules.Check(Get("DeletedTasksRead"), grant: false, current, Catalog);

        Assert.Equal("Remove \"Deleted tasks · Write\" first.", error);
    }

    [Fact]
    public void Removing_read_without_write_is_allowed()
    {
        var error = PermissionRules.Check(Get("DeletedTasksRead"), grant: false, current: new HashSet<string> { "DeletedTasksRead" }, Catalog);

        Assert.Null(error);
    }

    [Fact]
    public void Write_with_no_read_in_its_group_is_allowed()
    {
        var error = PermissionRules.Check(Get("TasksWrite"), grant: true, current: new HashSet<string>(), Catalog);

        Assert.Null(error);
    }
}