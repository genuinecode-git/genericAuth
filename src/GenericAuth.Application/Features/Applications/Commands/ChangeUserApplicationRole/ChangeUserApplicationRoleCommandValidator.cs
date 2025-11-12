using FluentValidation;

namespace GenericAuth.Application.Features.Applications.Commands.ChangeUserApplicationRole;

/// <summary>
/// Validator for ChangeUserApplicationRoleCommand.
/// </summary>
public class ChangeUserApplicationRoleCommandValidator : AbstractValidator<ChangeUserApplicationRoleCommand>
{
    public ChangeUserApplicationRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ApplicationCode)
            .NotEmpty().WithMessage("Application code is required.");

        RuleFor(x => x.NewApplicationRoleId)
            .NotEmpty().WithMessage("New application role ID is required.");
    }
}
