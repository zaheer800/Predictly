using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Predictly.Infrastructure.Persistence;

namespace Predictly.Api.Tests.Fixtures;

/// <summary>
/// Spins up the full ASP.NET Core pipeline with an in-memory EF Core database.
/// Each test class that uses this factory gets an isolated DB instance.
/// </summary>
public class PredictlyWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real Npgsql DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PredictlyDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Add in-memory database
            services.AddDbContext<PredictlyDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>Returns a scoped DbContext for test setup/assertions.</summary>
    public PredictlyDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<PredictlyDbContext>();
    }
}
