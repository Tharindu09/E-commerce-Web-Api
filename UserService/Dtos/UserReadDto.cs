namespace UserService.Dtos;

public record class UserReadDto
{   
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;

}
