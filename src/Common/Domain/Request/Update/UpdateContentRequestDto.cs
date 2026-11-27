namespace Common.Domain.Request.Update;

public class UpdateContentRequestDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ReferenceName { get; set; }
    public bool? ShowResultsSummary { get; set; }
} 
