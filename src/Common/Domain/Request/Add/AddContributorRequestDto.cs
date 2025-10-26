using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Add;

public class AddContributorRequestDto
{
    [Required(ErrorMessage = "Enter a DfE email")]
    [MaxLength(250, ErrorMessage = "email title must be 250 characters or fewer")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@education\.gov\.uk$", ErrorMessage = "Enter a valid DfE email address")]
    public required string Email { get; set; }
} 
