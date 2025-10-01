using Checker.Common.Domain;
using Checker.Common.Enum;

namespace Checker.Common.Domain.Request.Create;

public class CreateBranchingRequestDto
{
    public required int QuestionnaireId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
} 
