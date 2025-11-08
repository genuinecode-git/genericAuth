using System.Data;

namespace GenericAuth.Application.Common.Interfaces;

/// <summary>
/// Interface for Dapper-based read-only database operations
/// </summary>
public interface IQueryDbConnection
{
    IDbConnection Connection { get; }
}
