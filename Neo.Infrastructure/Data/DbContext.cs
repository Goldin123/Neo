namespace Neo.Infrastructure.Data;

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Provides SQL Server connection management for the infrastructure layer.
/// </summary>
public sealed class DbContext(IConfiguration config)
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Missing DefaultConnection");

    /// <summary>
    /// Creates and returns a new SQL database connection.
    /// </summary>
    /// <returns>A new <see cref="IDbConnection"/> instance.</returns>
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
