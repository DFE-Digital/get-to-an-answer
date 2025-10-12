using Common.Domain;
using Common.Enum;

namespace Common.Domain;

public class DestinationDto
{
    public DestinationType? Type { get; set; }
        
    public string? Content { get; set; } // Markdown
    public QuestionDto? Question { get; set; }
}