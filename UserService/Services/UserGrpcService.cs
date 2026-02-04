using System;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Grpc;

namespace UserService.Services;

public class UserGrpcService : UserProfileService.UserProfileServiceBase
{
    private readonly AppDbContext _context;

    public UserGrpcService(AppDbContext context)
    {
        _context = context;
    }


    public override async Task<UserProfileResponse> GetUserProfile(GetUserProfileRequest request, ServerCallContext context)
    {
        var user = await _context.Users
                        
                        .FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        }

        return new UserProfileResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
           

        };
    }
}
