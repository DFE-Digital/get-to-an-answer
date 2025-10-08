using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enum;

namespace Common.Infrastructure.Persistence.Entities;

[Table("Questionnaires")]
public class QuestionnaireEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /**
     * Maps to tid claim
     */
    public string TenantId { get; set; }   
    
    /**
     * Maps to Oid claim
     */
    public string OwnerId { get; set; }
    
    /**
     * List of authenticated government users invited by the owner/members
     */
    public List<string> InvitedUsers { get; set; } = new();

    [Required]
    [MaxLength(500)]
    public string Title { get; set; }

    public string Description { get; set; }
    
    public int Version { get; set; }
    
    public EntityStatus Status { get; set; } = EntityStatus.Draft;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }

    // Navigation properties
    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
}

