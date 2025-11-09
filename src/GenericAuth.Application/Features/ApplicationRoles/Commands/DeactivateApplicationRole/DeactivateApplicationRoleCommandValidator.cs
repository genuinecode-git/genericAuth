using FluentValidation;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.DeactivateApplicationRole;

/// <summary>
/// Validator for DeactivateApplicationRoleCommand.
/// </summary>
public class DeactivateApplicationRoleCommandValidator : AbstractValidator<DeactivateApplicationRoleCommand>
{
    public DeactivateApplicationRoleCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.");
    }
}
