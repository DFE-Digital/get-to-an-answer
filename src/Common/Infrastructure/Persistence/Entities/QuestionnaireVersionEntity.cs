using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enum;

namespace Common.Infrastructure.Persistence.Entities;

[Table("QuestionnaireVersions")]
public class QuestionnaireVersionEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public Guid QuestionnaireId { get; set; }
    
    [Required(ErrorMessage = "Enter a questionnaire json")]
    public required string QuestionnaireJson { get; set; }
    
    public int Version { get; set; }
    
    public DateTime CreatedAt { get; set; }
}