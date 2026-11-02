using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdQueryValidator : AbstractValidator<GetTaskByIdQuery>
{
    public GetTaskByIdQueryValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ErrorMessages.TaskNotFound);
    }
}
