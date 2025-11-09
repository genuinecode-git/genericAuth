using FluentValidation;

namespace GenericAuth.Application.Features.Roles.Commands.UpdateRole;

/// <summary>
/// Validator for UpdateRoleCommand.
/// </summary>
public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Role name is required.")
            .MinimumLength(2)
            .WithMessage("Role name must be at least 2 characters.")
            .MaximumLength(100)
            .WithMessage("Role name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Role description is required.")
            .MaximumLength(500)
            .WithMessage("Role description cannot exceed 500 characters.");
    }
}
