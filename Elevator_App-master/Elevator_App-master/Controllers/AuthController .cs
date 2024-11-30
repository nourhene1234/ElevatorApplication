using crudmongo.Models;
using crudmongo.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace crudmongo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtTokenService _tokenService;

        public AuthController(UserService userService, JwtTokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        // Register a new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = request.Password,
                Role = request.Role
                
            };

            try
            {
                await _userService.RegisterUserAsync(user);
                return Ok("User registered successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Username or email already taken
            }
        }

        // Login and issue JWT token
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            bool isValidUser = await _userService.ValidateUserCredentials(request.Username, request.Password);
            if (!isValidUser)
            {
                return Unauthorized("Invalid username or password.");
            }

            var user = await _userService.GetByUsernameAsync(request.Username);
            var token = _tokenService.GenerateToken(user);
            return Ok(new { Token = token });
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
