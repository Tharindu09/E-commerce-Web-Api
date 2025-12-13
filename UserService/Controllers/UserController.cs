using Microsoft.AspNetCore.Mvc;
using UserService.Dtos;
using UserService.Models;
using UserService.Services;
using UserService.Mappers;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            var userDtos = users.Select(UserMapper.ToReadDto);
            return Ok(userDtos);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(UserMapper.ToReadDto(user));
        }

        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<UserReadDto>> CreateUser(UserCreateDto userDto)
        {
            var user = UserMapper.ToUser(userDto);
            var createdUser = await _userService.CreateUserAsync(user);
            var readDto = UserMapper.ToReadDto(createdUser);

            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, readDto);
        }

        // PUT: api/User/5
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
