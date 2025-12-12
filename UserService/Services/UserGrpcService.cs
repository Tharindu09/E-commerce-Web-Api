using System;
using Grpc.Core;
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
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        }

        return new UserProfileResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            AddressLine1 = user.address.AddressLine1,
            AddressLine2 = user.address.AddressLine2,
            City = user.address.City,
            District = user.address.District,
            Province = user.address.Province,
            PostalCode = user.address.PostalCode

        };
    }
}
