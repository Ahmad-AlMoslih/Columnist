using System.ComponentModel.DataAnnotations;

namespace Columnist.Contracts.Dtos;

// What the API's create/update sends to SaveTaskCommand (the web creates with just Title and ColumnToken).
public sealed class TaskFormDto
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 330;
    public const int MaxAssignees = 20;

    public string? Token { get; set; }   // encrypted task id; null/empty = new task

    public string Title { get; set; } = "";

    public string? Description { get; set; }

    [Display(Name = "Status")]
    public string? ColumnToken { get; set; }

    [Display(Name = "Assignees")]
    public List<string> AssigneeTokens { get; set; } = [];   // encrypted user ids

    [DataType(DataType.Date)]
    [Display(Name = "Due date")]
    public DateTime? DueDate { get; set; }

    public string? RowVersion { get; set; }   // Base64 of Tasks.RowVersion; null for a new task

    public bool IsNew => string.IsNullOrEmpty(Token);
}
