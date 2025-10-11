using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enum;

namespace Common.Infrastructure.Persistence.Entities;

[Table("Questionnaires")]
public class QuestionnaireSnapshotEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public QuestionnaireEntity Snapshot { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

