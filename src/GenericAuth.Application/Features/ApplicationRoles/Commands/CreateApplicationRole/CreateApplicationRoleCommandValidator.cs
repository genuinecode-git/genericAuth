using FluentValidation;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.CreateApplicationRole;

/// <summary>
/// Validator for CreateApplicationRoleCommand.
/// </summary>
public class CreateApplicationRoleCommandValidator : AbstractValidator<CreateApplicationRoleCommand>
{
    public CreateApplicationRoleCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MinimumLength(2).WithMessage("Role name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Role name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Role description is required.")
            .MaximumLength(500).WithMessage("Role description must not exceed 500 characters.");
    }
}
