using FluentValidation;

namespace GenericAuth.Application.Features.Applications.Commands.RemoveUserFromApplication;

/// <summary>
/// Validator for RemoveUserFromApplicationCommand.
/// </summary>
public class RemoveUserFromApplicationCommandValidator : AbstractValidator<RemoveUserFromApplicationCommand>
{
    public RemoveUserFromApplicationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ApplicationCode)
            .NotEmpty().WithMessage("Application code is required.");
    }
}
