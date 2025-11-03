using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Common.Enum;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence.Entities;

[Index(nameof(QuestionId), nameof(IsDeleted))]  // GetAnswers and lookups by question, not deleted
[Index(nameof(QuestionnaireId), nameof(IsDeleted))] // cross-questionnaire filters and cascades
[Table("Answers")]
public class AnswerEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public string Content { get; set; }
    
    public string? Description { get; set; }
    
    public float Score { get; set; }
    
    public DestinationType? DestinationType { get; set; }
    
    [MaxLength(250)]
    public string? DestinationUrl { get; set; }
    
    public Guid? DestinationQuestionId { get; set; }
    
    public Guid? DestinationContentId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Foreign key
    public Guid QuestionId { get; set; }
    
    [JsonIgnore]
    public QuestionEntity? Question { get; set; }
    
    public Guid QuestionnaireId { get; set; }
    public bool IsDeleted { get; set; }
}