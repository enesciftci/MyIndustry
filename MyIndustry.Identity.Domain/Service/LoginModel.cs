using System.ComponentModel.DataAnnotations;

namespace MyIndustry.Identity.Domain.Service;

public record LoginModel
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}