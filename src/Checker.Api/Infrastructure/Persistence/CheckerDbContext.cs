using Checker.Common.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Checker.Api.Infrastructure.Persistence;

public class CheckerDbContext : DbContext
{
    public CheckerDbContext(DbContextOptions<CheckerDbContext> options)
        : base(options) { }

    public DbSet<QuestionEntity> Questions { get; set; }
    public DbSet<AnswerEntity> Answers { get; set; }
    public DbSet<BranchingEntity> Branching { get; set; }
    public DbSet<ConditionEntity> Conditions { get; set; }
    public DbSet<QuestionnaireEntity> Questionnaires { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Optional: add global query filters for multi-tenancy
        // modelBuilder.Entity<QuestionEntity>()
        //     .HasQueryFilter(q => q.TenantId == _currentTenantId);
    }
}