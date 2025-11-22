using System.ComponentModel.DataAnnotations;

namespace UserService.Models;

public class User
{       [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!;

}
