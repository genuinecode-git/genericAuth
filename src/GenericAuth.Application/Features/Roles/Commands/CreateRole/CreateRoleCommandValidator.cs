using FluentValidation;

namespace GenericAuth.Application.Features.Roles.Commands.CreateRole;

/// <summary>
/// Validator for CreateRoleCommand.
/// </summary>
public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
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
