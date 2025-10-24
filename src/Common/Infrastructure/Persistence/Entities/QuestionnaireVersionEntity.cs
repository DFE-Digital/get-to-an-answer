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
    public required string QuestionnaireJson { get; set; }
    
    public int Version { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    [MaxLength(500)]
    public required string CreatedBy { get; set; }
    
    public required string ChangeDescription { get; set; }
    
    [MaxLength(100)]
    [NotMapped]
    public string? SyncId { get; set; }
}

