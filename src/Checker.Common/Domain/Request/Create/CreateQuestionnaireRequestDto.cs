using Checker.Common.Domain;
using Checker.Common.Enum;

namespace Checker.Common.Domain.Request.Create;

public class CreateQuestionnaireRequestDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
} 
