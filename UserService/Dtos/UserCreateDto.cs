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
    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]
    public string Phone { get; set; }
    [Required]
    public AddressDto address { get; set; }
}

public class AddressDto
{   
    
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }

    public string City { get; set; }

    public string District { get; set; }
    public string Province { get; set; }

    public string PostalCode { get; set; }

}
