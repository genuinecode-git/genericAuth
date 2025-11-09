using FluentValidation;

namespace GenericAuth.Application.Features.Users.Commands.RemoveRoleFromUser;

/// <summary>
/// Validator for RemoveRoleFromUserCommand.
/// </summary>
public class RemoveRoleFromUserCommandValidator : AbstractValidator<RemoveRoleFromUserCommand>
{
    public RemoveRoleFromUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");
    }
}
