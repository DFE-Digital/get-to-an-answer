using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Update;

public class UpdateQuestionnaireRequestDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
} 
