using System.ComponentModel.DataAnnotations;

namespace UserService.Dtos;

public class UserCreateDto
{
    [Required]
    public string Name { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    
    
}


