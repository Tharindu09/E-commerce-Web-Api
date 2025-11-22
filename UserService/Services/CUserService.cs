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

    public async Task<bool> UpdateUserAsync(int id, User user)
    {
        User existUser = appDbContext.Users.Find(id);
        if (existUser != null)
        {
            // Update properties
            existUser.Name = user.Name;
            existUser.Email = user.Email;

            // Save changes
            await appDbContext.SaveChangesAsync();
            return true; // update successful

        } 
        
        return false;
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

    
}
