using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enum;
using Common.Local;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence.Entities;

[Index(nameof(QuestionnaireId), nameof(Version), IsUnique = true)] // point lookup by version; also OrderBy on Version
[Index(nameof(QuestionnaireId))]                                   // list versions by questionnaire
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
    
    [MaxLength(500)]
    public required string CreatedBy { get; set; }
    
    public string? ChangeDescription { get; set; }
    
    public string? ChangeLog { get; set; }
}