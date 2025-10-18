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
    public Guid Id { get; set; }
    
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
    
    [MaxLength(250)]
    public string? DestinationUrl { get; set; }
    
    public Guid? DestinationQuestionId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Foreign key
    public Guid QuestionId { get; set; }
    
    [JsonIgnore]
    public QuestionEntity? Question { get; set; }
    
    public Guid QuestionnaireId { get; set; }
    public bool IsDeleted { get; set; }
}