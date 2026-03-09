using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Predictly.Infrastructure.Persistence;

namespace Predictly.Tests.Integration;

/// <summary>
/// Integration test factory using an in-memory SQLite database.
/// Replace with a test PostgreSQL container for full DB-level lock testing.
/// </summary>
public class PredictlyWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the real PostgreSQL DbContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PredictlyDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Use in-memory database for integration tests
            services.AddDbContext<PredictlyDbContext>(options =>
                options.UseInMemoryDatabase("predictly_test"));

            // Ensure the database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PredictlyDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
