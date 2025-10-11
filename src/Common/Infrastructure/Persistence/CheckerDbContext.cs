using Common.Enum;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence;

public class CheckerDbContext(DbContextOptions<CheckerDbContext> options) : DbContext(options)
{
    public DbSet<QuestionEntity> Questions { get; set; }
    public DbSet<AnswerEntity> Answers { get; set; }
    public DbSet<QuestionnaireEntity> Questionnaires { get; set; }
    public DbSet<QuestionnaireEntity> QuestionnaireHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Optional: add global query filters for multi-tenancy
        // modelBuilder.Entity<QuestionEntity>()
        //     .HasQueryFilter(q => q.TenantId == _currentTenantId);
        
        
    }
}

public static class CheckerDbContextExtensions
{
    public static async Task<bool> HasAccessToEntity<TEntityType>(this CheckerDbContext db, string email, int id)
    {
        if (typeof(TEntityType) == typeof(QuestionEntity))
        {
            return await db.Questions
                .Where(q => q.Id == id
                            && q.Status != EntityStatus.Deleted)
                .Select(q => db.Questionnaires
                    .Where(qq => qq.Id == q.QuestionnaireId)
                    .SelectMany(qq => qq.Contributors)
                    .Any(e => e == email))
                .FirstOrDefaultAsync();
        }
        
        if (typeof(TEntityType) == typeof(QuestionnaireEntity))
        {
            return await db.Questions
                .Select(q => db.Questionnaires
                    .Where(qq => qq.Id == id)
                    .SelectMany(qq => qq.Contributors)
                    .Any(e => e == email))
                .FirstOrDefaultAsync();
        }
        
        if (typeof(TEntityType) == typeof(AnswerEntity))
        {
            return await db.Answers
                .Where(q => q.Id == id)
                .Select(q => db.Questionnaires
                    .Where(qq => qq.Id == q.QuestionnaireId)
                    .SelectMany(qq => qq.Contributors)
                    .Any(e => e == email))
                .FirstOrDefaultAsync();
        }
        
        return false;
    }
}