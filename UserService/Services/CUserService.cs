using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Services;

public class CUserService : IUserService
{
    private readonly AppDbContext appDbContext;
    private readonly PasswordHasher<User> passwordHasher;
    public CUserService(AppDbContext context)
    {
        appDbContext = context;
        passwordHasher = new PasswordHasher<User>();
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await appDbContext.Users.ToListAsync();
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await appDbContext.Users.FindAsync(id);
    }

   public async Task<User> CreateUserAsync(User user)
    {
        // Hash the password before saving
        user.Password = passwordHasher.HashPassword(user, user.Password);
        try
        {
            var existingUser = await appDbContext.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                throw new Exception("Email already in use");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error checking existing user: {ex.Message}");
        }
        appDbContext.Users.Add(user);
        await appDbContext.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateUserAsync(int id, User dto)
{
    var user = await appDbContext.Users
        .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return false;

        user.Name = dto.Name;
        user.Email = dto.Email;

        
        await appDbContext.SaveChangesAsync();
        return true;
        }
    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await appDbContext.Users.FindAsync(id);
        if (user != null)
        {
            appDbContext.Users.Remove(user);
            await appDbContext.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        return user;
    }
    
    public bool VerifyPassword(User user, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);
        return result == PasswordVerificationResult.Success;
    }

    public async Task<bool> AddAddressAsync(int userId, AddressDto addressDto)
    {
        appDbContext.Address.Add(new Address
        {
            UserId = userId,
            AddressLine1 = addressDto.AddressLine1,
            AddressLine2 = addressDto.AddressLine2,
            City = addressDto.City,
            Country = addressDto.Country,
            Phone = addressDto.Phone,
            PostalCode = addressDto.PostalCode
        });
        await appDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<Address>?> GetAddressByUserIdAsync(int userId)
    {
        return await appDbContext.Address.Where(a => a.UserId == userId).ToListAsync();
    }
}
