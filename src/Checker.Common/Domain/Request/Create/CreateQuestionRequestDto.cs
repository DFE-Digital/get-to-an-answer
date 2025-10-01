using Checker.Common.Domain;
using Checker.Common.Enum;

namespace Checker.Common.Domain.Request.Create;

public class CreateQuestionRequestDto
{
    public required int QuestionnaireId { get; set; }
    public required string Content { get; set; }
    public required string Description { get; set; }
    public required QuestionType Type { get; set; }
} 
