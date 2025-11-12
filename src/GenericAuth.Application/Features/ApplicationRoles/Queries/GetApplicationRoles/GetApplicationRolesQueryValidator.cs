using FluentValidation;

namespace GenericAuth.Application.Features.ApplicationRoles.Queries.GetApplicationRoles;

/// <summary>
/// Validator for GetApplicationRolesQuery.
/// </summary>
public class GetApplicationRolesQueryValidator : AbstractValidator<GetApplicationRolesQuery>
{
    public GetApplicationRolesQueryValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
    }
}
