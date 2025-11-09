using FluentValidation;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.DeleteApplicationRole;

/// <summary>
/// Validator for DeleteApplicationRoleCommand.
/// </summary>
public class DeleteApplicationRoleCommandValidator : AbstractValidator<DeleteApplicationRoleCommand>
{
    public DeleteApplicationRoleCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.");
    }
}
