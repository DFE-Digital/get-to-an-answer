using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enum;

namespace Common.Infrastructure.Persistence.Entities;

[Table("QuestionnaireVersions")]
public class QuestionnaireVersionEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int QuestionnaireId { get; set; }
    public required string QuestionnaireJson { get; set; }
    
    public int Version { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

