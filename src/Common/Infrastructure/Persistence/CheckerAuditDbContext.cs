using Common.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Persistence;

public class CheckerAuditDbContext : DbContext
{
    public CheckerAuditDbContext(DbContextOptions<CheckerAuditDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Optional: add global query filters for multi-tenancy
        // modelBuilder.Entity<QuestionEntity>()
        //     .HasQueryFilter(q => q.TenantId == _currentTenantId);
    }
}