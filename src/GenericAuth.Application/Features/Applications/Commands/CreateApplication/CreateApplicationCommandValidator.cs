using FluentValidation;

namespace GenericAuth.Application.Features.Applications.Commands.CreateApplication;

public class CreateApplicationCommandValidator : AbstractValidator<CreateApplicationCommand>
{
    public CreateApplicationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Application name is required.")
            .MaximumLength(200).WithMessage("Application name must not exceed 200 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Application code is required.")
            .Matches(@"^[a-zA-Z0-9_-]{3,50}$")
            .WithMessage("Application code must be 3-50 characters and contain only letters, numbers, hyphens, or underscores.");

        RuleFor(x => x.InitialRoles)
            .NotNull().WithMessage("Initial roles are required.")
            .Must(roles => roles != null && roles.Count > 0)
            .WithMessage("At least one initial role must be provided.");

        RuleForEach(x => x.InitialRoles)
            .ChildRules(role =>
            {
                role.RuleFor(r => r.Name)
                    .NotEmpty().WithMessage("Role name is required.")
                    .MaximumLength(100).WithMessage("Role name must not exceed 100 characters.");

                role.RuleFor(r => r.Description)
                    .MaximumLength(500).WithMessage("Role description must not exceed 500 characters.");
            });

        // Ensure only one role is marked as default
        RuleFor(x => x.InitialRoles)
            .Must(roles => roles.Count(r => r.IsDefault) <= 1)
            .WithMessage("Only one role can be marked as default.");
    }
}
