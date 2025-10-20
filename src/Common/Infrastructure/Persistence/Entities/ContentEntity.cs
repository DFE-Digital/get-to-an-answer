using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Common.Enum;

namespace Common.Infrastructure.Persistence.Entities;

[Table("Contents")]
public class ContentEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public Guid QuestionnaireId { get; set; }
    
    /*
     * Shorthand name for this content.
     */
    public string? RefName { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; }

    [Required]
    [MaxLength(10000)]
    public string Content { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    [MaxLength(100)]
    public string? SyncId { get; set; }
}