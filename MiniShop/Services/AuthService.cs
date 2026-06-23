using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniShop.Data;
using MiniShop.DTOs;
using MiniShop.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace MiniShop.Services
{
    public class AuthService : IAuthService
    {
        private readonly MiniShopDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(MiniShopDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.OrderBy(u => u.Id).FirstOrDefaultAsync(u => u.Email == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

            return GenerateJwtToken(user);
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Username))
            {
                return false; // Username already exists
            }

            var user = new Models.User
            {
                FullName = request.Username,
                Email = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User" // Default role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> GoogleLoginAsync(string googleToken)
        {
            try
            {
                // Validate Google Token
                var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(googleToken);

                var email = payload.Email;
                var user = await _context.Users.OrderBy(u => u.Id).FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    user = new Models.User
                    {
                        FullName = payload.Name,
                        Email = payload.Email,
                        Role = "User",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString() + Guid.NewGuid().ToString())
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                return GenerateJwtToken(user);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GenerateJwtToken(Models.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new System.Security.Claims.Claim[]
                {
                    new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new System.Security.Claims.Claim(ClaimTypes.Name, user.Email),
                    new System.Security.Claims.Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
