using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateQuestionnaireRequestDto
{
    public Guid Id { get; set; }
    public string? DisplayTitle { get; set; }
    public string Title { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
} 
