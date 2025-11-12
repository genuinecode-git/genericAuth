using FluentValidation;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.RemovePermissionFromApplicationRole;

/// <summary>
/// Validator for RemovePermissionFromApplicationRoleCommand.
/// </summary>
public class RemovePermissionFromApplicationRoleCommandValidator : AbstractValidator<RemovePermissionFromApplicationRoleCommand>
{
    public RemovePermissionFromApplicationRoleCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("Permission ID is required.");
    }
}
