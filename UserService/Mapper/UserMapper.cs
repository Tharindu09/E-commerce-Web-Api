using UserService.Dtos;
using UserService.Models;

namespace UserService.Mappers
{
    public static class UserMapper
    {
        public static UserReadDto ToReadDto(User user)
        {
            return new UserReadDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }

       public static User ToUser(UserCreateDto dto)
        {
            return new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password,
                
            };
        }

    }
}
