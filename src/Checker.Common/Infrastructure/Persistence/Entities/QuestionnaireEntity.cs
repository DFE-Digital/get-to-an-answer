using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Checker.Common.Enum;

namespace Checker.Common.Infrastructure.Persistence.Entities;

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

    [Required]
    [MaxLength(500)]
    public string Title { get; set; }

    public string Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
}

