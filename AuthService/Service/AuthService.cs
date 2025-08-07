using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using HotelBookingSystem.Models.Auth;
using HotelBookingSystem.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HotelBookingSystem.Services
{
    public class AuthService
    {
        private readonly IConfiguration _config;
        private readonly IAuthService _userStore;

        public AuthService(IConfiguration config, IAuthService userStore)
        {
            _config = config;
            _userStore = userStore;
        }

        public AuthResponse Authenticate(LoginRequest request)
        {
            var user = _userStore.ValidateUser(request.Username, request.Password);
            if (user == null) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AuthResponse
            {
                Token = tokenHandler.WriteToken(token),
                Role = user.Role
            };
        }
    }
}