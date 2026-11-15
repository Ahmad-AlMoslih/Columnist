using Columnist.Tasks.Features.Tasks.Commands.SaveComment;
using Columnist.Utilities.Response.Errors;

namespace Columnist.UnitTests.Tasks;

public class SaveCommentCommandValidatorTests
{
    private readonly SaveCommentCommandValidator _validator = new();

    [Fact]
    public void Valid_comment_passes()
    {
        var result = _validator.Validate(new SaveCommentCommand("task-token", null, "Looks good"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_comment_fails(string body)
    {
        var result = _validator.Validate(new SaveCommentCommand("task-token", null, body));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorMessages.CommentRequired);
    }

    [Fact]
    public void Comment_longer_than_max_fails()
    {
        var body = new string('a', SaveCommentCommand.BodyMaxLength + 1);

        var result = _validator.Validate(new SaveCommentCommand("task-token", null, body));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Comment can't be longer than 1000 characters.");
    }

    [Fact]
    public void Comment_of_exactly_max_length_passes()
    {
        var body = new string('a', SaveCommentCommand.BodyMaxLength);

        var result = _validator.Validate(new SaveCommentCommand("task-token", null, body));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Missing_task_token_fails()
    {
        var result = _validator.Validate(new SaveCommentCommand("", null, "Looks good"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorMessages.TaskNotFound);
    }
}