using UserService.Dtos;
using UserService.Models;

namespace UserService.Services;

public interface IUserService
{   
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(int id, User user);
    Task<bool> DeleteUserAsync(int id);
    bool VerifyPassword(User user, string password);
    Task<User> GetUserByEmailAsync(string email);

    Task<bool> AddAddressAsync(int userId, AddressDto addressDto);

    Task<List<Address>?> GetAddressByUserIdAsync(int userId);


}
