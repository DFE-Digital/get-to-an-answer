using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateContentRequestDto
{
    public int Id { get; set; }
    public int QuestionnaireId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
} 
