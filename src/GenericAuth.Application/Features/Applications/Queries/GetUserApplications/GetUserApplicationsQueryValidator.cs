using FluentValidation;

namespace GenericAuth.Application.Features.Applications.Queries.GetUserApplications;

/// <summary>
/// Validator for GetUserApplicationsQuery.
/// </summary>
public class GetUserApplicationsQueryValidator : AbstractValidator<GetUserApplicationsQuery>
{
    public GetUserApplicationsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
