using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Tasks.Features.Tasks.Commands.AddTaskImages;

// File count, size and type are checked by the DMS upload (DmsOptions).
public sealed class AddTaskImagesCommandValidator : AbstractValidator<AddTaskImagesCommand>
{
    public AddTaskImagesCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ErrorMessages.TaskNotFound);

        RuleFor(x => x.Images)
            .NotEmpty().WithMessage(ErrorMessages.ChooseImages);
    }
}
