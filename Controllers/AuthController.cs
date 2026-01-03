using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using finalhotelAPI.Models;
using Microsoft.EntityFrameworkCore;
using finalhotelAPI.Data;
using System.Net;

namespace finalhotelAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly WebDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(WebDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username && u.Passwordhash == request.Password);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, StoreName = user.Storename, Passwordhash = user.Passwordhash, Expirydate=user.Expirydate, Address=user.Address, PhoneNo=user.Phonenumber, Username=user.Username });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      

            var claims = new[]
            {
                 new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()),
                 new Claim("Username", user.Username),
                 new Claim("StoreName", user.Storename),
                 new Claim("Passwordhash", user.Passwordhash),
                 new Claim("Expirydate", user.Expirydate.HasValue ? user.Expirydate.Value.ToString("yyyy-MM-dd") : ""),
                 new Claim("Address",user.Address), // Example role claim
                 new Claim("PhoneNo",user.Phonenumber)
            };


            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                signingCredentials: credentials
            // ❌ no expiry added
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
