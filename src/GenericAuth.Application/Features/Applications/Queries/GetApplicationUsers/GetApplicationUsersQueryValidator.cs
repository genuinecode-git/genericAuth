using FluentValidation;

namespace GenericAuth.Application.Features.Applications.Queries.GetApplicationUsers;

/// <summary>
/// Validator for GetApplicationUsersQuery.
/// </summary>
public class GetApplicationUsersQueryValidator : AbstractValidator<GetApplicationUsersQuery>
{
    public GetApplicationUsersQueryValidator()
    {
        RuleFor(x => x.ApplicationCode)
            .NotEmpty().WithMessage("Application code is required.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
    }
}
