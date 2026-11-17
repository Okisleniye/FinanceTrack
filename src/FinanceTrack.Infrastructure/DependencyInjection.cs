using FinanceTrack.Application.Common.Interfaces;
using FinanceTrack.Infrastructure.Data;
using FinanceTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=FinanceTrack.db";

        // Extract and ensure database directory exists if specified in connection string
        EnsureDatabaseDirectory(connectionString);

        services.AddDbContext<FinanceTrackDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<FinanceTrackDbContext>());

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }

    private static void EnsureDatabaseDirectory(string connectionString)
    {
        try
        {
            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var kv = part.Split('=', 2);
                if (kv.Length == 2 && (kv[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase) || kv[0].Trim().Equals("DataSource", StringComparison.OrdinalIgnoreCase)))
                {
                    var filePath = kv[1].Trim();
                    if (!string.IsNullOrEmpty(filePath) && !filePath.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
                    {
                        var directory = Path.GetDirectoryName(filePath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                    }
                    break;
                }
            }
        }
        catch
        {
            // Fallback gracefully if connection string parsing encounters non-standard formats
        }
    }
}
