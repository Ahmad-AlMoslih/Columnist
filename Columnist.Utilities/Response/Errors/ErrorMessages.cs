namespace Columnist.Utilities.Response.Errors;

public static class ErrorMessages
{
    // General
    public const string Unauthorized = "Authentication is required.";
    public const string SignInRequired = "You need to be signed in to do this.";
    public const string Forbidden = "You are not allowed to do this.";
    public const string NotFound = "The requested item was not found.";
    public const string InvalidData = "The request contains invalid data.";
    public const string AlreadyExists = "This item already exists.";
    public const string LinkedToOtherData = "This item is linked to other data.";
    public const string Conflict = "This change clashes with existing data.";
    public const string ValueTooLong = "One of the values is too long.";
    public const string ServiceUnavailable = "The service is temporarily unavailable. Please try again.";
    public const string UnexpectedError = "An unexpected error occurred.";

    // Tasks
    public const string TaskNotFound = "Task not found.";
    public const string BoardNotFound = "Board not found.";
    public const string TitleRequired = "Please enter a title.";
    public const string TitleTooLong = "Title can't be longer than {1} characters.";   // {1} = max length, as DataAnnotations formats it
    public const string DescriptionTooLong = "Description can't be longer than {1} characters.";
    public const string ChooseStatus = "Please choose a status.";
    public const string AssigneeNotFound = "One of the people you picked can't be assigned.";
    public const string TooManyAssignees = "You can assign up to {0} people.";
    public const string DueDateInPast = "Due date can't be in the past.";
    public const string SaveFailed = "The task couldn't be saved. Check the column and try again.";
    public const string TaskChangedByOther = "This task was changed by someone else. Reload it to see the latest version.";
    public const string CommentNotFound = "Comment not found.";
    public const string CommentRequired = "Please write a comment.";
    public const string CommentTooLong = "Comment can't be longer than {MaxLength} characters.";   // {MaxLength} = FluentValidation placeholder
    public const string StatusNotFound = "That column colour no longer exists.";
    public const string ChecklistNotFound = "That checklist or item no longer exists.";
    public const string ChecklistTitleRequired = "Please enter a checklist title.";
    public const string ChecklistTitleTooLong = "Checklist title can't be longer than {MaxLength} characters.";   // {MaxLength} = FluentValidation placeholder
    public const string ChecklistItemRequired = "Please enter the item text.";
    public const string ChecklistItemTooLong = "Item text can't be longer than {MaxLength} characters.";

    public const string UsernameRequired = "Please enter your username.";
    public const string PasswordRequired = "Please enter your password.";
    public const string InvalidCredentials = "Wrong username or password.";

    // Profile
    public const string FullNameRequired = "Please enter your full name.";
    public const string FullNameTooLong = "Full name can't be longer than {MaxLength} characters.";   // {MaxLength} = FluentValidation placeholder
    public const string PhotoRequired = "Please choose a photo.";
    public const string PhotoTypeNotAllowed = "Please choose a JPG or PNG photo.";
    public const string PhotoTooLarge = "The photo can't be larger than 2 MB.";
    public const string CurrentPasswordRequired = "Please enter your current password.";
    public const string CurrentPasswordWrong = "Your current password is wrong.";
    public const string NewPasswordRequired = "Please enter a new password.";
    public const string NewPasswordTooShort = "Your new password must be at least {MinLength} characters.";
    public const string NewPasswordTooLong = "Your new password can't be longer than {MaxLength} characters.";
    public const string PasswordsDontMatch = "The new passwords don't match.";

    // Permissions
    public const string PermissionChangeFailed = "This permission can't be changed.";
    public const string GivePermissionFirst = "Give \"{0}\" first.";
    public const string RemovePermissionFirst = "Remove \"{0}\" first.";

    // Teams
    public const string TeamNotFound = "Team not found.";
    public const string TeamMemberNotFound = "That person isn't a member of this team.";
    public const string TeamHasBoards = "Delete or move this team's boards first.";
    public const string DeleteNameMismatch = "Type the name exactly as shown to confirm.";
    public const string TeamNameRequired = "Please enter a team name.";
    public const string TeamNameTooLong = "Team name can't be longer than {MaxLength} characters.";
    public const string InviteCodeRequired = "Please enter an invite code.";
    public const string InviteCodeInvalid = "That code is wrong or has expired. Ask the team's Super Admin for today's code.";
    public const string AlreadyTeamOwner = "You're already the Super Admin of this team.";
    public const string BoardNameRequired = "Please enter a board name.";
    public const string BoardNameTooLong = "Board name can't be longer than {MaxLength} characters.";
    public const string ColumnNameRequired = "Every column needs a name.";
    public const string ColumnNameTooLong = "Column names can't be longer than {MaxLength} characters.";
    public const string BoardNeedsColumn = "A board needs at least one column.";
    public const string ColumnHasTasks = "A column you removed still has tasks (deleted ones count too). Move or restore them first.";
}
