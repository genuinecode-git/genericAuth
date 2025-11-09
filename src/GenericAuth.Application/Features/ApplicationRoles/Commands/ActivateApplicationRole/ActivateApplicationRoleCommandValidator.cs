using FluentValidation;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.ActivateApplicationRole;

/// <summary>
/// Validator for ActivateApplicationRoleCommand.
/// </summary>
public class ActivateApplicationRoleCommandValidator : AbstractValidator<ActivateApplicationRoleCommand>
{
    public ActivateApplicationRoleCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.");
    }
}
