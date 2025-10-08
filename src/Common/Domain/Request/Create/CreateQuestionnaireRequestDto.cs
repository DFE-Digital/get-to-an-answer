using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Create;

public class CreateQuestionnaireRequestDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
} 
