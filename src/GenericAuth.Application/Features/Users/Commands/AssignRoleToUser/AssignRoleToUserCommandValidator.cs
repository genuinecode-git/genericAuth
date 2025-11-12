using FluentValidation;

namespace GenericAuth.Application.Features.Users.Commands.AssignRoleToUser;

/// <summary>
/// Validator for AssignRoleToUserCommand.
/// </summary>
public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");
    }
}
