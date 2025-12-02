using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Common.Enum;
using Microsoft.EntityFrameworkCore;

namespace Common.Infrastructure.Persistence.Entities;

[Index(nameof(Slug), nameof(Status))] // WebController: Any(q => q.Slug == slug && Status != Deleted)
[Index(nameof(Slug), IsUnique = true)] // if Slug must be unique across all questionnaires
[Table("Questionnaires")]
public class QuestionnaireEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /**
     * List of authenticated government users invited by the owner/members
     */
    public List<string> Contributors { get; set; } = new();

    /**
     * Service user url path reference to the questionnaire
     */
    [MaxLength(500)]
    public string? Slug { get; set; }

    [MaxLength(500)]
    public string? DisplayTitle { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; }
    
    [MaxLength(10000)]
    public string? Description { get; set; }
    
    public int Version { get; set; }
    
    public EntityStatus Status { get; set; } = EntityStatus.Draft;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string CreatedBy { get; set; }
    
    [MaxLength(500)]
    public string? PublishedBy { get; set; }
    
    public DateTime? PublishedAt { get; set; }

    // Child entities
    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
    
    // Custom content final pages
    public ICollection<ContentEntity> Contents { get; set; } = new List<ContentEntity>();
    
    // ~~~ Completion ~~~

    [Column(TypeName = "nvarchar(max)")] public Dictionary<CompletableTask, CompletionStatus>? CompletionTrackingMap { get; set; } = new();
    
    // ~~~ Look and feel ~~~
    
    [MaxLength(7)] public string? TextColor { get; set; } = "#0b0c0c";
    [MaxLength(7)] public string? BackgroundColor { get; set; } = "#ffffff";
    [MaxLength(7)] public string? PrimaryButtonColor { get; set; } = "#00703c";
    [MaxLength(7)] public string? SecondaryButtonColor { get; set; } = "#1d70b8";
    [MaxLength(7)] public string? StateColor { get; set; } = "#ffdd00";
    [MaxLength(7)] public string? ErrorMessageColor { get; set; } = "#c3432b";
    
    [MaxLength(250)] public string? DecorativeImage { get; set; }
    [MaxLength(50)] public string? ContinueButtonText { get; set; } = "Continue";
    public bool IsAccessibilityAgreementAccepted { get; set; }
    
    
}
