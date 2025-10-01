using Checker.Common.Domain;
using Checker.Common.Enum;

namespace Checker.Common.Domain.Request.Update;

public class UpdateBranchingRequestDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
} 
