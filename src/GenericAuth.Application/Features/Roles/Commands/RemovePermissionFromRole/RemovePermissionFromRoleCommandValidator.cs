using FluentValidation;

namespace GenericAuth.Application.Features.Roles.Commands.RemovePermissionFromRole;

/// <summary>
/// Validator for RemovePermissionFromRoleCommand.
/// </summary>
public class RemovePermissionFromRoleCommandValidator : AbstractValidator<RemovePermissionFromRoleCommand>
{
    public RemovePermissionFromRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty()
            .WithMessage("Permission ID is required.");
    }
}
