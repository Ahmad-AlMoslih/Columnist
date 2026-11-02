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
    public const string InvalidAssignee = "Please choose a valid assignee.";
    public const string DueDateInPast = "Due date can't be in the past.";
    public const string SaveFailed = "The selected status or assignee doesn't exist.";

    public const string UsernameRequired = "Please enter your username.";
    public const string PasswordRequired = "Please enter your password.";
    public const string InvalidCredentials = "Wrong username or password.";
}
