using FluentValidation;

namespace GenericAuth.Application.Features.Authentication.Commands.Logout;

/// <summary>
/// Validator for LogoutCommand.
/// </summary>
public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
