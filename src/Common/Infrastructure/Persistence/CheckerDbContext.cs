using Common.Enum;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence;

public class CheckerDbContext(DbContextOptions<CheckerDbContext> options) : DbContext(options)
{
    public DbSet<QuestionEntity> Questions { get; set; }
    public DbSet<AnswerEntity> Answers { get; set; }
    public DbSet<QuestionnaireEntity> Questionnaires { get; set; }
    public DbSet<QuestionnaireVersionEntity> QuestionnaireVersions { get; set; }
    public DbSet<ContentEntity> Contents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        /*modelBuilder.Entity<QuestionnaireVersionEntity>(b =>
        {
            b.ToTable("QuestionnaireVersions");

            var propertyBuilder = b.Property(e => e.Questionnaire);
            propertyBuilder.HasColumnType("nvarchar(max)");

            // Optional JSON validity check (SQL Server)
            b.ToTable(t => t.HasCheckConstraint(
                "CK_QuestionnaireVersions_Questionnaire_IsJson",
                "ISJSON([Questionnaire]) > 0"));
        });*/
        
        modelBuilder.Entity<AnswerEntity>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .HasPrincipalKey(q => q.Id); 
        
        modelBuilder.Entity<QuestionEntity>()
            .HasOne(a => a.Questionnaire)
            .WithMany(q => q.Questions)
            .HasForeignKey(a => a.QuestionnaireId)
            .HasPrincipalKey(q => q.Id);
        
        modelBuilder.Entity<QuestionnaireVersionEntity>()
            .HasKey(q => new { q.QuestionnaireId, q.Version });
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

        if (typeof(TEntityType) == typeof(ContentEntity))
        {
            return await db.Contents
                .Select(q => db.Questionnaires
                    .Where(qq => qq.Id == id)
                    .SelectMany(qq => qq.Contributors)
                    .Any(e => e == email))
                .FirstOrDefaultAsync();
        }
        
        return false;
    }

    public static async Task ResetQuestionnaireToDraft(this CheckerDbContext db, int questionnaireId)
    {
        var questionnaire = await db.Questionnaires.FirstOrDefaultAsync(q => q.Id == questionnaireId);
        
        if (questionnaire == null)
            return;
        
        questionnaire.Status = EntityStatus.Draft;
        questionnaire.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync();
    }
}