using Microsoft.AspNetCore.Mvc;
using UserService.Dtos;
using UserService.Models;
using UserService.Services;
using UserService.Mappers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: api/User
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            var userDtos = users.Select(UserMapper.ToReadDto);
            return Ok(userDtos);
        }

        // GET: api/User/5
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(UserMapper.ToReadDto(user));
        }


        // PUT: api/User/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserCreateDto userDto)
        {
            var user = UserMapper.ToUser(userDto);
            user.Id = id; // make sure ID matches

            var result = await _userService.UpdateUserAsync(id, user);
            if (!result)
                return NotFound();

            return Ok();
        }

        // DELETE: api/User/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        //get user profile
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserReadDto>> GetMyProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("Missing user id claim.");

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user id claim.");

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(UserMapper.ToReadDto(user));
        }

        [Authorize]
        [HttpPost("address")]
        public async Task<IActionResult> AddAddress(AddressDto addressDto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("Missing user id claim.");

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user id claim.");

            var result = await _userService.AddAddressAsync(userId, addressDto);
            if (!result)
                return BadRequest("Failed to add address.");

            return Ok();
        }

        [Authorize]
        [HttpGet("address")]
        public async Task<ActionResult<List<AddressDto>>> GetMyAddress()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userIdStr))
                return Unauthorized("Missing user id claim.");

            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user id claim.");

            List<Address> addresses = await _userService.GetAddressByUserIdAsync(userId);
            if (addresses == null || addresses.Count == 0)
                return NotFound();

            var addressDtos = addresses.Select(address => new AddressDto
            {   UserId = address.UserId,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                Country = address.Country,
                Phone = address.Phone,
                PostalCode = address.PostalCode
            });

            return Ok(addressDtos);
        }
    }
}