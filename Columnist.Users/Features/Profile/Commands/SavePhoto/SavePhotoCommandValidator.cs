using Columnist.Utilities.Response.Errors;
using FluentValidation;

namespace Columnist.Users.Features.Profile.Commands.SavePhoto;

public sealed class SavePhotoCommandValidator : AbstractValidator<SavePhotoCommand>
{
    public SavePhotoCommandValidator()
    {
        RuleFor(x => x.Photo)
            .Cascade(CascadeMode.Stop)
            .Must(photo => photo is { Length: > 0 }).WithMessage(ErrorMessages.PhotoRequired)
            .Must(photo => SavePhotoCommand.AllowedExtensions.Contains(Path.GetExtension(photo!.FileName).ToLowerInvariant()))
            .WithMessage(ErrorMessages.PhotoTypeNotAllowed)
            .Must(photo => photo!.Length <= SavePhotoCommand.MaxBytes).WithMessage(ErrorMessages.PhotoTooLarge);
    }
}
