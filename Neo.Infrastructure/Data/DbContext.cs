namespace Neo.Infrastructure.Data;

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Provides SQL Server connection management for the infrastructure layer.
/// </summary>
public sealed class DbContext
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContext"/> class.
    /// </summary>
    /// <param name="config">The configuration from which to read the connection string.</param>
    /// <param name="connectionName">The name of the connection string to use (default is 'DefaultConnection').</param>
    /// <exception cref="InvalidOperationException">Thrown if the specified connection string is missing.</exception>
    public DbContext(IConfiguration config, string connectionName = "DefaultConnection")
    {
        _connectionString = config.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException($"Missing connection string '{connectionName}'");
    }

    /// <summary>
    /// Creates and returns a new SQL database connection.
    /// </summary>
    /// <returns>A new <see cref="IDbConnection"/> instance.</returns>
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
