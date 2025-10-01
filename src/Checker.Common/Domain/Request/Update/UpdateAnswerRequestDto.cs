using Checker.Common.Domain;
using Checker.Common.Enum;

namespace Checker.Common.Domain.Request.Update;

public class UpdateAnswerRequestDto
{
    public required string Content { get; set; }
    public required string Description { get; set; }
} 
