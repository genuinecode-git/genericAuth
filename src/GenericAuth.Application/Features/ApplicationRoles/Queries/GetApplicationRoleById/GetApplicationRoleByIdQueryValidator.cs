using FluentValidation;

namespace GenericAuth.Application.Features.ApplicationRoles.Queries.GetApplicationRoleById;

/// <summary>
/// Validator for GetApplicationRoleByIdQuery.
/// </summary>
public class GetApplicationRoleByIdQueryValidator : AbstractValidator<GetApplicationRoleByIdQuery>
{
    public GetApplicationRoleByIdQueryValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required.");
    }
}
