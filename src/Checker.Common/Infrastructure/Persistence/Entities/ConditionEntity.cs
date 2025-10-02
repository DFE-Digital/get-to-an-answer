using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Checker.Common.Infrastructure.Persistence.Entities;

[Table("Branching")]
public class ConditionEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /**
     * Maps to tid claim
     */
    public string TenantId { get; set; }   
    
    /**
     * Maps to Oid claim
     */
    public string OwnerId { get; set; }

    [Required]
    public int QuestionId { get; set; }

    [MinLength(1)]
    public List<int> AnswerIds { get; set; }

    // Foreign key
    public int BranchingId { get; set; }
    public BranchingEntity Branching { get; set; }
}