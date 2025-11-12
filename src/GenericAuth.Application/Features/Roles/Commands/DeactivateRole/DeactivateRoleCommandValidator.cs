using FluentValidation;

namespace GenericAuth.Application.Features.Roles.Commands.DeactivateRole;

/// <summary>
/// Validator for DeactivateRoleCommand.
/// </summary>
public class DeactivateRoleCommandValidator : AbstractValidator<DeactivateRoleCommand>
{
    public DeactivateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");
    }
}
