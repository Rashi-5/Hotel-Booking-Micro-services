using Microsoft.AspNetCore.Mvc;
using HotelBookingSystem.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBookingSystem.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HotelBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserStore _userStore;
        private readonly JwtTokenService _jwtService;

        public AuthController(IUserStore userStore, JwtTokenService jwtService)
        {
            _userStore = userStore;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _userStore.ValidateUser(login.Username, login.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            // Generate JWT token
            var token = _jwtService.GenerateToken(user.Username, user.Role);

            // Add cookie-based authentication if needed
            if (HttpContext.Request.Headers.ContainsKey("X-Use-Cookies"))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };
                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync("CookieAuth", principal);
            }

            return Ok(new AuthResponse 
            { 
                Token = token, 
                Role = user.Role,
                Username = user.Username
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if (HttpContext.User.Identity.AuthenticationType == "CookieAuth")
            {
                await HttpContext.SignOutAsync("CookieAuth");
            }
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var username = HttpContext.User.Identity.Name;
            var role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new { username, role });
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllUsers()
        {
            var users = _userStore.GetAllUsers().Select(u => new { u.Username, u.Role });
            return Ok(users);
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user already exists
            if (_userStore.UserExists(request.Username))
                return Conflict(new { message = "Username already exists" });

            // Create new user with "User" role by default
            var newUser = new User
            {
                Username = request.Username,
                Password = request.Password,  
                Role = "User"
            };

            _userStore.AddUser(newUser);

            return Ok(new { message = "User registered successfully" });
        }
    }
}