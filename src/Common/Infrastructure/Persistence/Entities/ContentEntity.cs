using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Common.Enum;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence.Entities;

[Index(nameof(QuestionnaireId))] // GetContents by questionnaire
[Table("Contents")]
public class ContentEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; }

    [Required]
    [MaxLength(10000)]
    public string Content { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
    
    public Guid QuestionnaireId { get; set; }
    
    [JsonIgnore]
    public QuestionnaireEntity? Questionnaire { get; set; }
}