using FinanceTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinanceTrack.IntegrationTests.Infrastructure;

public class DatabaseSetupTests
{
    [Fact]
    public async Task CanInitializeDbContextWithSqliteInMemory()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.OpenConnectionAsync();
        var canConnect = await context.Database.CanConnectAsync();

        Assert.True(canConnect);
    }
}
