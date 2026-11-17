using Microsoft.EntityFrameworkCore;

namespace FinanceTrack.Infrastructure.Data;

public class ApplicationDbContext : FinanceTrackDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
