using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Frontend;

public class GetNextStateRequest
{
    public Guid CurrentQuestionId { get; set; }
    
    public int CurrentQuestionOrder { get; set; }

    [MinLength(length: 1, ErrorMessage = "Select which answer(s) apply")]
    public List<Guid> SelectedAnswerIds { get; set; } = new();
}