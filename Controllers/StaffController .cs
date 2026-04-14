//namespace finalhotelAPI.Controllers
//{
//    public class StaffController
//    {
//    }
//}


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using finalhotelAPI.Data;
using finalhotelAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace finalhotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly WebDbContext _context;

        public StaffController(WebDbContext context)
        {
            _context = context;
        }

        // ─── Helpers — same pattern as your old controllers ──────────
        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetRole()
        {
            return User.FindFirst("Role")?.Value ?? "";
        }

        private int GetStaffId()
        {
            var val = User.FindFirst("StaffId")?.Value;
            return string.IsNullOrEmpty(val) ? 0 : int.Parse(val);
        }

        // ─────────────────────────────────────────────────────────────
        // 1. GET ALL STAFF — Owner/Manager only
        // GET /api/staff
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAllStaff()
        {
            var role = GetRole();
            if (role != "Owner" && role != "Manager")
                return Forbid();

            var userId = GetUserId();
            var staffList = await _context.Staffmembers
                .AsNoTracking()
                .Where(s => s.Userid == userId)
                .Select(s => new
                {
                    s.Staffid,
                    s.Fullname,
                    s.Username,
                    s.Role,
                    s.Isactive,
                    s.Createdat
                })
                .ToListAsync();

            return Ok(staffList);
        }

        // ─────────────────────────────────────────────────────────────
        // 2. GET SINGLE STAFF BY ID — Owner/Manager only
        // GET /api/staff/5
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            var role = GetRole();
            if (role != "Owner" && role != "Manager")
                return Forbid();

            var userId = GetUserId();
            var staff = await _context.Staffmembers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Staffid == id && s.Userid == userId);

            if (staff == null)
                return NotFound("Staff not found");

            return Ok(new
            {
                staff.Staffid,
                staff.Fullname,
                staff.Username,
                staff.Role,
                staff.Isactive,
                staff.Createdat
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 3. GET STAFF BY ROLE — Owner/Manager only
        // GET /api/staff/byrole/Waiter
        // GET /api/staff/byrole/Chef
        // GET /api/staff/byrole/Manager
        // ─────────────────────────────────────────────────────────────
        [HttpGet("byrole/{roleName}")]
        public async Task<IActionResult> GetStaffByRole(string roleName)
        {
            var role = GetRole();
            if (role != "Owner" && role != "Manager")
                return Forbid();

            var userId = GetUserId();
            var staffList = await _context.Staffmembers
                .AsNoTracking()
                .Where(s => s.Userid == userId &&
                            s.Role == roleName &&
                            s.Isactive == true)
                .Select(s => new
                {
                    s.Staffid,
                    s.Fullname,
                    s.Username,
                    s.Role,
                    s.Isactive
                })
                .ToListAsync();

            return Ok(staffList);
        }

        // ─────────────────────────────────────────────────────────────
        // 4. ADD NEW STAFF — Owner only
        // POST /api/staff
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AddStaff([FromBody] StaffRequest request)
        {
            var role = GetRole();
            if (role != "Owner")
                return Forbid();

            var userId = GetUserId();

            // Validate role
            var validRoles = new[] { "Waiter", "Chef", "Manager" };
            if (!validRoles.Contains(request.Role))
                return BadRequest("Role must be Waiter, Chef, or Manager");

            // Check username already exists
            var exists = await _context.Staffmembers
                .AnyAsync(s => s.Username == request.Username);
            if (exists)
                return BadRequest("Username already taken");

            var newStaff = new Staffmember
            {
                Userid = userId,
                Fullname = request.Fullname,
                Username = request.Username,
                Passwordhash = request.Password,
                Role = request.Role,
                Isactive = true,
                Createdat = DateTime.Now
            };

            _context.Staffmembers.Add(newStaff);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Staff added successfully",
                StaffId = newStaff.Staffid,
                newStaff.Fullname,
                newStaff.Username,
                newStaff.Role
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 5. UPDATE STAFF — Owner only
        // PUT /api/staff/5
        // ─────────────────────────────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] StaffRequest request)
        {
            var role = GetRole();
            if (role != "Owner")
                return Forbid();

            // Validate role
            var validRoles = new[] { "Waiter", "Chef", "Manager" };
            if (!validRoles.Contains(request.Role))
                return BadRequest("Role must be Waiter, Chef, or Manager");

            var userId = GetUserId();
            var staff = await _context.Staffmembers
                .FirstOrDefaultAsync(s => s.Staffid == id && s.Userid == userId);

            if (staff == null)
                return NotFound("Staff not found");

            // Check username conflict with other staff
            var usernameExists = await _context.Staffmembers
                .AnyAsync(s => s.Username == request.Username && s.Staffid != id);
            if (usernameExists)
                return BadRequest("Username already taken by another staff");

            // Update fields
            staff.Fullname = request.Fullname;
            staff.Username = request.Username;
            staff.Role = request.Role;

            // Only update password if provided
            if (!string.IsNullOrEmpty(request.Password))
                staff.Passwordhash = request.Password;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Staff updated successfully",
                staff.Staffid,
                staff.Fullname,
                staff.Username,
                staff.Role
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 6. ACTIVATE / DEACTIVATE STAFF — Owner only
        // PATCH /api/staff/5/toggle
        // ─────────────────────────────────────────────────────────────
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleStaffStatus(int id)
        {
            var role = GetRole();
            if (role != "Owner")
                return Forbid();

            var userId = GetUserId();
            var staff = await _context.Staffmembers
                .FirstOrDefaultAsync(s => s.Staffid == id && s.Userid == userId);

            if (staff == null)
                return NotFound("Staff not found");

            staff.Isactive = !staff.Isactive;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = staff.Isactive == true ? "Staff activated" : "Staff deactivated",
                staff.Staffid,
                staff.Fullname,
                IsActive = staff.Isactive
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 7. DELETE STAFF — Owner only
        // DELETE /api/staff/5
        // ─────────────────────────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var role = GetRole();
            if (role != "Owner")
                return Forbid();

            var userId = GetUserId();
            var staff = await _context.Staffmembers
                .FirstOrDefaultAsync(s => s.Staffid == id && s.Userid == userId);

            if (staff == null)
                return NotFound("Staff not found");

            _context.Staffmembers.Remove(staff);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Staff deleted successfully" });
        }
    }

    // ─── Request Model ──────────────────────────────────────────────
    public class StaffRequest
    {
        public string Fullname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
