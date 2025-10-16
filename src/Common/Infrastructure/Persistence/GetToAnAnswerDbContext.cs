using Common.Enum;
using Common.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence;

public class GetToAnAnswerDbContext(DbContextOptions<GetToAnAnswerDbContext> options) : DbContext(options)
{
    //public DbSet<QuestionEntity> Questions { get; set; }
    //public DbSet<AnswerEntity> Answers { get; set; }
    public DbSet<QuestionnaireEntity> Questionnaires { get; set; }
    
    //public DbSet<QuestionnaireVersionEntity> QuestionnaireVersions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<QuestionnaireEntity>()
            .Property(e => e.Id)
            .HasColumnType("uniqueidentifier")
            .HasDefaultValueSql("NEWSEQUENTIALID()");
    }
}