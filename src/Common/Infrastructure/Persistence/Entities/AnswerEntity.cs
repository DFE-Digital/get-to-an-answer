using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Common.Enum;

namespace Common.Infrastructure.Persistence.Entities;

[Table("Answers")]
public class AnswerEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /*
     * Shorthand name for this answer.
     */
    public string? RefName { get; set; }

    [Required]
    [MaxLength(250)]
    public string Content { get; set; }
    
    public string? Description { get; set; }
    
    public float Score { get; set; }
    
    public DestinationType? DestinationType { get; set; }
    
    public string? Destination { get; set; }
    
    public int? DestinationQuestionId { get; set; }
    
    public int? DestinationContentId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Foreign key
    public int QuestionId { get; set; }
    
    [JsonIgnore]
    public QuestionEntity? Question { get; set; }
    
    public int QuestionnaireId { get; set; }
    
    [MaxLength(100)]
    public string? SyncId { get; set; }
}