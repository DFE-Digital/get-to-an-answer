using Checker.Common.Domain;
using Checker.Common.Enum;

namespace Checker.Common.Domain
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public QuestionType Type { get; set; }
        public List<AnswerDto> Answers { get; set; } = new();
    
        public DateTime CreatedAt { get; set; }
    
        public DateTime UpdatedAt { get; set; }
    }

    

    

    
}
