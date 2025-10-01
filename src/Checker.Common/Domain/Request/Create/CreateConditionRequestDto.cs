using Checker.Common.Domain;
using Checker.Common.Enum;

namespace Checker.Common.Domain.Request.Create;

public class CreateConditionRequestDto
{
    public required int BranchingId { get; set; }
    public required int QuestionId { get; set; }
    public required List<int> AnswerIds { get; set; }
} 
