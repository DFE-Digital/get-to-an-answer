using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Common.Enum;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence.Entities;

[Index(nameof(QuestionnaireId), nameof(IsDeleted))] // frequent filter: by questionnaire and not deleted
[Index(nameof(QuestionnaireId), nameof(Order))]     // initial question and reordering lookups
[Index(nameof(QuestionnaireId), nameof(Id))]        // access checks that join questionnaire scope + id
[Table("Questions")]
public class QuestionEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public int Order { get; set; }

    [Required]
    [MaxLength(500)]
    public string Content { get; set; }

    public string? Description { get; set; }

    [Required]
    public QuestionType Type { get; set; }

    public bool IsDeleted { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<AnswerEntity> Answers { get; set; } = new List<AnswerEntity>();
    
    public Guid QuestionnaireId { get; set; }
    
    [JsonIgnore]
    public QuestionnaireEntity? Questionnaire { get; set; }
    
    [MaxLength(100)]
    [NotMapped]
    public string? SyncId { get; set; }
    
    // TODO: Add fields for:
    // Custom continue button text
    // Custom error message from user error
}

