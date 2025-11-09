using FluentValidation;

namespace GenericAuth.Application.Features.Roles.Commands.ActivateRole;

/// <summary>
/// Validator for ActivateRoleCommand.
/// </summary>
public class ActivateRoleCommandValidator : AbstractValidator<ActivateRoleCommand>
{
    public ActivateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");
    }
}
