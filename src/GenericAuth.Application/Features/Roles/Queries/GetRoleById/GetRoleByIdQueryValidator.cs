using FluentValidation;

namespace GenericAuth.Application.Features.Roles.Queries.GetRoleById;

/// <summary>
/// Validator for GetRoleByIdQuery.
/// </summary>
public class GetRoleByIdQueryValidator : AbstractValidator<GetRoleByIdQuery>
{
    public GetRoleByIdQueryValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required.");
    }
}
