using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enum;

namespace Common.Infrastructure.Persistence.Entities;

[Table("Questions")]
public class QuestionEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int QuestionnaireId { get; set; }

    [Required]
    public int Order { get; set; }

    [Required]
    [MaxLength(500)]
    public string Content { get; set; }

    public string? Description { get; set; }

    [Required]
    public QuestionType Type { get; set; }

    [Required] public EntityStatus Status { get; set; } = EntityStatus.Draft;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<AnswerEntity> Answers { get; set; } = new List<AnswerEntity>();
}

