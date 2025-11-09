using FluentValidation;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.AddPermissionToApplicationRole;

/// <summary>
/// Validator for AddPermissionToApplicationRoleCommand.
/// </summary>
public class AddPermissionToApplicationRoleCommandValidator : AbstractValidator<AddPermissionToApplicationRoleCommand>
{
    public AddPermissionToApplicationRoleCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("Permission ID is required.");
    }
}
