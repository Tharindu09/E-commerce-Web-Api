namespace UserService.Dtos;

public record class UserReadDto
{   
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;

}
