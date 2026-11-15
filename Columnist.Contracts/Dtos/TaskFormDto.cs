using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Columnist.Contracts.Dtos;

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

    public string? ReturnBoardToken { get; set; }

    public string? RowVersion { get; set; }   // Base64 of Tasks.RowVersion; null for a new task

    [Display(Name = "Images")]
    public List<IFormFile>? Images { get; set; } = [];

    [Display(Name = "Remove all images")]
    public bool RemoveImages { get; set; }

    public List<string>? ImageUrls { get; set; } = [];

    [BindNever]
    public IReadOnlyList<TokenOptionDto> Columns { get; set; } = [];

    [BindNever]
    public IReadOnlyList<AssigneeOptionDto> Users { get; set; } = [];

    public bool IsNew => string.IsNullOrEmpty(Token);
}
