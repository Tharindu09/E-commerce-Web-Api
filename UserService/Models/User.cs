using System.ComponentModel.DataAnnotations;

namespace UserService.Models;

public class User
{       [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        public List<Address> Address { get; set; } = default!;

        public string Role { get; set; } = "User";

}

public enum Role
{
        User,
        Admin,
        Vendor
}
