using Microsoft.Extensions.Configuration;
using Neo.Infrastructure.Data;

namespace Neo.Tests.Integration;

/// <summary>
/// Provides shared database context for integration tests using the 'TestConnection' string.
/// </summary>
public sealed class DbFixture : IDisposable
{
    /// <summary>
    /// Gets the initialized <see cref="DbContext"/> for integration test access.
    /// </summary>
    public DbContext DbContext { get; }

    /// <summary>
    /// Initializes the <see cref="DbContext"/> using the test database connection string from appsettings.json.
    /// </summary>
    public DbFixture()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        DbContext = new DbContext(config, "TestConnection");
    }

    /// <summary>
    /// Disposes resources used by the test fixture.
    /// </summary>
    public void Dispose()
    {
        // Optional: add cleanup or teardown logic here
    }
}
