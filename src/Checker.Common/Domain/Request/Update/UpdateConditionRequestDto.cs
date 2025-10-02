using Checker.Common.Domain;
using Checker.Common.Enum;

namespace Checker.Common.Domain.Request.Update;

public class UpdateConditionRequestDto
{
    public required int QuestionId { get; set; }
    public required List<int> AnswerIds { get; set; }
} 
