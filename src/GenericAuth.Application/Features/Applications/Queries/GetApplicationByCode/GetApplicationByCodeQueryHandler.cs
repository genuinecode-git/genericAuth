using Dapper;
using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Applications.Queries.GetApplicationByCode;

public class GetApplicationByCodeQueryHandler : IRequestHandler<GetApplicationByCodeQuery, Result<ApplicationDto>>
{
    private readonly IQueryDbConnection _queryDb;

    public GetApplicationByCodeQueryHandler(IQueryDbConnection queryDb)
    {
        _queryDb = queryDb;
    }

    public async Task<Result<ApplicationDto>> Handle(GetApplicationByCodeQuery request, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                Id,
                Name,
                Code,
                IsActive,
                CreatedAt
            FROM Applications
            WHERE Code = @Code";

        var application = await _queryDb.Connection.QuerySingleOrDefaultAsync<ApplicationDto>(
            sql,
            new { Code = request.ApplicationCode });

        if (application == null)
        {
            return Result<ApplicationDto>.Failure($"Application with code '{request.ApplicationCode}' not found.");
        }

        return Result<ApplicationDto>.Success(application);
    }
}
