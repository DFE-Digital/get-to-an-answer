using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Common.Enum;

namespace Common.Infrastructure.Persistence.Entities;

[Table("Questionnaires")]
public class QuestionnaireEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /**
     * List of authenticated government users invited by the owner/members
     */
    public List<string> Contributors { get; set; } = new();

    /**
     * Service user url path reference to the questionnaire
     */
    [MaxLength(500)]
    public string? Slug { get; set; }

    [MaxLength(500)]
    public string? DisplayTitle { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; }
    
    [MaxLength(10000)]
    public string? Description { get; set; }
    
    public int Version { get; set; }
    
    public EntityStatus Status { get; set; } = EntityStatus.Draft;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string CreatedBy { get; set; }
    
    [MaxLength(500)]
    public string? PublishedBy { get; set; }
    
    public DateTime? PublishedAt { get; set; }

    // Navigation properties
    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
    
    [MaxLength(100)]
    [NotMapped]
    public string? SyncId { get; set; }
}

