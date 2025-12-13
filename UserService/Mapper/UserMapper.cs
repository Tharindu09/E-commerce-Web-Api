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
                Phone = dto.Phone,
                Address = new Address
                        {
                            AddressLine1 = dto.address.AddressLine1,
                            AddressLine2 = dto.address.AddressLine2,
                            City = dto.address.City,
                            District = dto.address.District,
                            Province = dto.address.Province,
                            PostalCode = dto.address.PostalCode
                        }
            };
        }
    }
}
