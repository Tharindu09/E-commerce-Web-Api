using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
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

        appDbContext.Users.Add(user);
        await appDbContext.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateUserAsync(int id, User dto)
{
    var user = await appDbContext.Users
        .Include(u => u.Address)
        .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return false;

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.Phone = dto.Phone;

        user.Address ??= new Address();

        user.Address.AddressLine1 = dto.Address.AddressLine1;
        user.Address.AddressLine2 = dto.Address.AddressLine2;
        user.Address.City = dto.Address.City;
        user.Address.District = dto.Address.District;
        user.Address.Province = dto.Address.Province;
        user.Address.PostalCode = dto.Address.PostalCode;

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
}
