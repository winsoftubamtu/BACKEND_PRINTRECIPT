//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using finalhotelAPI.Models;
//using Microsoft.EntityFrameworkCore;
//using finalhotelAPI.Data;
//using System.Net;

//namespace finalhotelAPI.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AuthController : ControllerBase
//    {
//        private readonly WebDbContext _context;
//        private readonly IConfiguration _config;

//        public AuthController(WebDbContext context, IConfiguration config)
//        {
//            _context = context;
//            _config = config;
//        }

//        [HttpPost("login")]
//        public IActionResult Login([FromBody] LoginRequest request)
//        {
//            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username && u.Passwordhash == request.Password);

//            if (user == null)
//            {
//                return Unauthorized("Invalid credentials");
//            }

//            var token = GenerateJwtToken(user);
//            return Ok(new { Token = token, StoreName = user.Storename, Passwordhash = user.Passwordhash, Expirydate=user.Expirydate, Address=user.Address, PhoneNo=user.Phonenumber, Username=user.Username });
//        }

//        private string GenerateJwtToken(User user)
//        {
//            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
//            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);



//            var claims = new[]
//            {
//                 new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()),
//                 new Claim("Username", user.Username),
//                 new Claim("StoreName", user.Storename),
//                 new Claim("Passwordhash", user.Passwordhash),
//                 new Claim("Expirydate", user.Expirydate.HasValue ? user.Expirydate.Value.ToString("yyyy-MM-dd") : ""),
//                 new Claim("Address",user.Address), // Example role claim
//                 new Claim("PhoneNo",user.Phonenumber)
//            };


//            var token = new JwtSecurityToken(
//                issuer: _config["Jwt:Issuer"],
//                audience: _config["Jwt:Audience"],
//                claims: claims,
//                signingCredentials: credentials
//            // ❌ no expiry added
//            );

//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }
//    }

//    public class LoginRequest
//    {
//        public string Username { get; set; }
//        public string Password { get; set; }
//    }
//}





//below is UpdateDataOperation change
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using finalhotelAPI.Models;
using Microsoft.EntityFrameworkCore;
using finalhotelAPI.Data;

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
            // ✅ First check Users table (Owner)
            var user = _context.Users.FirstOrDefault(u =>
                u.Username == request.Username &&
                u.Passwordhash == request.Password);

            if (user != null)
            {
                var token = GenerateOwnerToken(user);
                return Ok(new
                {
                    Token = token,
                    Role = "Owner",
                    UserId = user.Userid,
                    StaffId = (int?)null,      // Owner has no StaffId
                    StoreName = user.Storename,
                    Username = user.Username,
                    Address = user.Address,
                    PhoneNo = user.Phonenumber,
                    Expirydate = user.Expirydate
                });
            }

            // ✅ Then check StaffMembers table (Waiter, Chef, Manager)
            var staff = _context.Staffmembers.FirstOrDefault(s =>
                s.Username == request.Username &&
                s.Passwordhash == request.Password &&
                s.Isactive == true);

            if (staff != null)
            {
                // Get owner's store info
                var owner = _context.Users.FirstOrDefault(u => u.Userid == staff.Userid);
                var token = GenerateStaffToken(staff, owner);
                return Ok(new
                {
                    Token = token,
                    Role = staff.Role,           // 'Waiter', 'Chef', 'Manager'
                    UserId = staff.Userid,        // Owner's UserId (for data queries)
                    StaffId = staff.Staffid,
                    StoreName = owner?.Storename,
                    Username = staff.Username,
                    Address = (string?)null,
                    PhoneNo = (string?)null,
                    Expirydate = (DateOnly?)null
                });
            }

            return Unauthorized("Invalid credentials");
        }

        // ✅ Token for Owner
        private string GenerateOwnerToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()),
                new Claim("UserId", user.Userid.ToString()),
                new Claim("Username", user.Username),
                new Claim("Role", "Owner"),
                new Claim("StaffId", ""),
                new Claim("StoreName", user.Storename),
                new Claim("Expirydate", user.Expirydate.HasValue
                    ? user.Expirydate.Value.ToString("yyyy-MM-dd") : ""),
                new Claim("Address", user.Address ?? ""),
                new Claim("PhoneNo", user.Phonenumber ?? "")
            };
            return BuildToken(claims);
        }

        // ✅ Token for Staff (Waiter/Chef/Manager)
        private string GenerateStaffToken(Staffmember staff, User? owner)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, staff.Userid.ToString()),
                new Claim("UserId", staff.Userid.ToString()),   // Owner's UserId
                new Claim("Username", staff.Username),
                new Claim("Role", staff.Role),
                new Claim("StaffId", staff.Staffid.ToString()),
                new Claim("StoreName", owner?.Storename ?? ""),
                new Claim("Expirydate", ""),
                new Claim("Address", ""),
                new Claim("PhoneNo", "")
            };
            return BuildToken(claims);
        }

        // ✅ Common token builder
        private string BuildToken(Claim[] claims)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                signingCredentials: credentials
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
