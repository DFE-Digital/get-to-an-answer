using System.ComponentModel.DataAnnotations;
using Common.Domain;
using Common.Enum;

namespace Common.Domain.Request.Add;

public class AddContributorRequestDto
{
    [Required(ErrorMessage = "Enter an Id")]
    [MaxLength(250, ErrorMessage = "Id must be 250 characters or fewer")]
    public required string Id { get; set; }
} 
