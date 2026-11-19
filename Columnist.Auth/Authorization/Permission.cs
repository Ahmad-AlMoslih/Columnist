namespace Columnist.Auth.Authorization;

// Each name must match Permissions.Name in the database.
public enum Permission
{
    TasksWrite,
    TasksDelete,
    DeletedTasksRead,
    DeletedTasksWrite,
    LogsRead,
    ReportsRead
}