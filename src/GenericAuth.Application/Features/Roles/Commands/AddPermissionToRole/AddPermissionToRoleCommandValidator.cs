using FluentValidation;

namespace GenericAuth.Application.Features.Roles.Commands.AddPermissionToRole;

/// <summary>
/// Validator for AddPermissionToRoleCommand.
/// </summary>
public class AddPermissionToRoleCommandValidator : AbstractValidator<AddPermissionToRoleCommand>
{
    public AddPermissionToRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty()
            .WithMessage("Permission ID is required.");
    }
}
