using System.ComponentModel.DataAnnotations;

namespace EmailVerificationSystem.Models;

public class EmailSubmissionRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}