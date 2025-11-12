using FluentValidation;

namespace GenericAuth.Application.Features.Roles.Commands.DeleteRole;

/// <summary>
/// Validator for DeleteRoleCommand.
/// </summary>
public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");
    }
}
