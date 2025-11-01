using Common.Enum;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence;

public class GetToAnAnswerDbContext(DbContextOptions<GetToAnAnswerDbContext> options) : DbContext(options)
{
    public DbSet<QuestionEntity> Questions { get; set; }
    public DbSet<AnswerEntity> Answers { get; set; }
    public DbSet<QuestionnaireEntity> Questionnaires { get; set; }
    
    public DbSet<QuestionnaireVersionEntity> QuestionnaireVersions { get; set; }
    public DbSet<ContentEntity> Contents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
    
    public EntityAccess HasAccessToEntity<TEntityType>(string email, Guid id)
    {
        if (typeof(TEntityType) == typeof(QuestionnaireEntity))
        {
            var access = Questionnaires
                .Where(qq => qq.Id == id && qq.Status != EntityStatus.Deleted)
                .Select(qq => new { IsContributor = qq.Contributors.Contains(email) })
                .FirstOrDefault();

            return access is null
                ? EntityAccess.NotFound
                : (access.IsContributor ? EntityAccess.Allow : EntityAccess.Deny);
        }

        if (typeof(TEntityType) == typeof(QuestionEntity))
        {
            var access = Questions
                .Where(q => q.Id == id && !q.IsDeleted)
                .Select(q => Questionnaires
                    .Where(qq => qq.Id == q.QuestionnaireId)
                    .Select(qq => new { IsContributor = qq.Contributors.Contains(email) }).FirstOrDefault())
                .FirstOrDefault();

            return access is null
                ? EntityAccess.NotFound
                : (access.IsContributor ? EntityAccess.Allow : EntityAccess.Deny);
        }
        
        if (typeof(TEntityType) == typeof(AnswerEntity))
        {
            var access = Answers
                .Where(q => q.Id == id && !q.IsDeleted)
                .Select(q => Questionnaires
                    .Where(qq => qq.Id == q.QuestionnaireId)
                    .Select(qq => new { IsContributor = qq.Contributors.Contains(email) }).FirstOrDefault())
                .FirstOrDefault();

            return access is null
                ? EntityAccess.NotFound
                : (access.IsContributor ? EntityAccess.Allow : EntityAccess.Deny);
        }

        if (typeof(TEntityType) == typeof(ContentEntity))
        {
            var access = Contents
                .Where(q => q.Id == id && !q.IsDeleted)
                .Select(q => Questionnaires
                    .Where(qq => qq.Id == q.QuestionnaireId)
                    .Select(qq => new { IsContributor = qq.Contributors.Contains(email) }).FirstOrDefault())
                .FirstOrDefault();
            
            return access is null
                ? EntityAccess.NotFound
                : (access.IsContributor ? EntityAccess.Allow : EntityAccess.Deny);
        }
        
        return EntityAccess.Deny;
    }

    public async Task ResetQuestionnaireToDraft(Guid questionnaireId)
    {
        var questionnaire = await Questionnaires.FirstOrDefaultAsync(q => q.Id == questionnaireId);
        
        if (questionnaire == null)
            return;
        
        questionnaire.Status = EntityStatus.Draft;
        questionnaire.UpdatedAt = DateTime.UtcNow;
        
        await SaveChangesAsync();
    }
}

public enum EntityAccess
{
    Allow,
    Deny,
    NotFound
}