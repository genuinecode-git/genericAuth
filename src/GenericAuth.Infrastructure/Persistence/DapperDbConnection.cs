using System.Data;
using GenericAuth.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GenericAuth.Infrastructure.Persistence;

public class DapperDbConnection : IQueryDbConnection, IDisposable
{
    private readonly IDbConnection _connection;

    public DapperDbConnection(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        _connection = new SqlConnection(connectionString);
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection;
        }
    }

    public void Dispose()
    {
        if (_connection.State == ConnectionState.Open)
        {
            _connection.Close();
        }
        _connection.Dispose();
    }
}
