using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Dtos;
using UserService.Services;

namespace UserService.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(JwtService jwtService, IUserService userService, ILogger<AuthController> logger)
        {
            _jwtService = jwtService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login( LoginRequestDto request)
        {
            
            try
            {
                var user = await _userService.GetUserByEmailAsync(request.Email);
                _logger.LogInformation("Attempting login for {Email}", user.Email);
                bool isPasswordValid = _userService.VerifyPassword(user, request.Password);
                if (user == null || !isPasswordValid)
                {
                    return Unauthorized("Invalid credentials");
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email, user.Role);

                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
            
        } 
    }
}
