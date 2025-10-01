using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Checker.Common.Enum;

namespace Checker.Common.Infrastructure.Persistence.Entities;

[Table("Branching")]
public class BranchingEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    /**
     * Maps to tid claim
     */
    public string TenantId { get; set; }   
    
    /**
     * Maps to Oid claim
     */
    public string OwnerId { get; set; }
    
    [Required]
    public DestinationType DestinationType { get; set; }
    
    [Required]
    public string Destination { get; set; }

    // Foreign key
    public int QuestionnaireId { get; set; }
    public QuestionnaireEntity Questionnaire { get; set; }

    public ICollection<ConditionEntity>? Conditions { get; set; }
}