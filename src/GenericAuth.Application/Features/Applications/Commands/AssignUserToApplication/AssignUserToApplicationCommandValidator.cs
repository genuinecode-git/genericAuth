using FluentValidation;

namespace GenericAuth.Application.Features.Applications.Commands.AssignUserToApplication;

public class AssignUserToApplicationCommandValidator : AbstractValidator<AssignUserToApplicationCommand>
{
    public AssignUserToApplicationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ApplicationCode)
            .NotEmpty().WithMessage("Application code is required.");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(100).WithMessage("Role name must not exceed 100 characters.");
    }
}
