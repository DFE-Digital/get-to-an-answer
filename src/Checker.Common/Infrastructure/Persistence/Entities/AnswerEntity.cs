using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Checker.Common.Enum;

namespace Checker.Common.Infrastructure.Persistence.Entities;

[Table("Answers")]
public class AnswerEntity
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
    [MaxLength(250)]
    public string Content { get; set; }
    
    public string Description { get; set; }
    
    public float Score { get; set; }
    
    public DestinationType DestinationType { get; set; }
    
    public string Destination { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Foreign key
    public int QuestionId { get; set; }
    public QuestionEntity Question { get; set; }
}