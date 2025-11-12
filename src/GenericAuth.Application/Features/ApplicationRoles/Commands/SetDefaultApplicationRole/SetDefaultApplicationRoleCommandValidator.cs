using FluentValidation;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.SetDefaultApplicationRole;

/// <summary>
/// Validator for SetDefaultApplicationRoleCommand.
/// </summary>
public class SetDefaultApplicationRoleCommandValidator : AbstractValidator<SetDefaultApplicationRoleCommand>
{
    public SetDefaultApplicationRoleCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.");
    }
}
