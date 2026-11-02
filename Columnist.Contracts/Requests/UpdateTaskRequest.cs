using System.ComponentModel.DataAnnotations;

namespace Columnist.Contracts.Requests;

public sealed record UpdateTaskRequest(
    [Required(ErrorMessage = "Title is required.")] string Title, string? Description, int ColumnId, int? AssigneeId, DateTime? DueDate);
