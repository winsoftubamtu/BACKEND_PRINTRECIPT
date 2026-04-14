//namespace finalhotelAPI.Controllers
//{
//    public class TablesController
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
    public class TablesController : ControllerBase
    {
        private readonly WebDbContext _context;

        public TablesController(WebDbContext context)
        {
            _context = context;
        }

        // ─── Helpers ─────────────────────────────────────────────────
        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private string GetRole()
        {
            return User.FindFirst("Role")?.Value ?? "";
        }

        // ─────────────────────────────────────────────────────────────
        // 1. GET ALL TABLES — Owner, Manager, Waiter
        // GET /api/tables
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAllTables()
        {
            var role = GetRole();
            if (role == "Chef")
                return Forbid(); // Chef has no table view

            var userId = GetUserId();

            var tables = await _context.Hoteltables
                .AsNoTracking()
                .Where(t => t.Userid == userId)
                .Select(t => new
                {
                    t.Tableid,
                    t.Tablenumber,
                    t.Status,
                    AssignedWaiter = t.Assignedwaiterid != null ?
                        _context.Staffmembers
                            .Where(s => s.Staffid == t.Assignedwaiterid)
                            .Select(s => new { s.Staffid, s.Fullname })
                            .FirstOrDefault()
                        : null
                })
                .ToListAsync();

            return Ok(tables);
        }

        // ─────────────────────────────────────────────────────────────
        // 2. GET SINGLE TABLE — Owner, Manager, Waiter
        // GET /api/tables/5
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTableById(int id)
        {
            var role = GetRole();
            if (role == "Chef")
                return Forbid();

            var userId = GetUserId();
            var table = await _context.Hoteltables
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Tableid == id && t.Userid == userId);

            if (table == null)
                return NotFound("Table not found");

            return Ok(new
            {
                table.Tableid,
                table.Tablenumber,
                table.Status,
                table.Assignedwaiterid
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 3. GET TABLES BY STATUS — Owner, Manager, Waiter
        // GET /api/tables/bystatus/Empty
        // GET /api/tables/bystatus/Occupied
        // GET /api/tables/bystatus/Billed
        // ─────────────────────────────────────────────────────────────
        [HttpGet("bystatus/{status}")]
        public async Task<IActionResult> GetTablesByStatus(string status)
        {
            var role = GetRole();
            if (role == "Chef")
                return Forbid();

            var userId = GetUserId();
            var tables = await _context.Hoteltables
                .AsNoTracking()
                .Where(t => t.Userid == userId && t.Status == status)
                .Select(t => new
                {
                    t.Tableid,
                    t.Tablenumber,
                    t.Status,
                    t.Assignedwaiterid
                })
                .ToListAsync();

            return Ok(tables);
        }

        // ─────────────────────────────────────────────────────────────
        // 4. ADD NEW TABLE — Owner only
        // POST /api/tables
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AddTable([FromBody] TableRequest request)
        {
            var role = GetRole();
            if (role != "Owner")
                return Forbid();

            var userId = GetUserId();

            // Check table number already exists for this hotel
            var exists = await _context.Hoteltables
                .AnyAsync(t => t.Tablenumber == request.Tablenumber && t.Userid == userId);
            if (exists)
                return BadRequest("Table number already exists");

            var newTable = new Hoteltable
            {
                Userid = userId,
                Tablenumber = request.Tablenumber,
                Status = "Empty",
                Assignedwaiterid = null
            };

            _context.Hoteltables.Add(newTable);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Table added successfully",
                newTable.Tableid,
                newTable.Tablenumber,
                newTable.Status
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 5. UPDATE TABLE NUMBER — Owner only
        // PUT /api/tables/5
        // ─────────────────────────────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] TableRequest request)
        {
            var role = GetRole();
            if (role != "Owner")
                return Forbid();

            var userId = GetUserId();
            var table = await _context.Hoteltables
                .FirstOrDefaultAsync(t => t.Tableid == id && t.Userid == userId);

            if (table == null)
                return NotFound("Table not found");

            // Check table number conflict
            var exists = await _context.Hoteltables
                .AnyAsync(t => t.Tablenumber == request.Tablenumber
                            && t.Userid == userId
                            && t.Tableid != id);
            if (exists)
                return BadRequest("Table number already taken");

            table.Tablenumber = request.Tablenumber;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Table updated successfully",
                table.Tableid,
                table.Tablenumber
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 6. ASSIGN WAITER TO TABLE — Owner, Manager
        // PATCH /api/tables/5/assignwaiter/3
        // ─────────────────────────────────────────────────────────────
        [HttpPatch("{id}/assignwaiter/{waiterId}")]
        public async Task<IActionResult> AssignWaiter(int id, int waiterId)
        {
            var role = GetRole();
            if (role != "Owner" && role != "Manager")
                return Forbid();

            var userId = GetUserId();
            var table = await _context.Hoteltables
                .FirstOrDefaultAsync(t => t.Tableid == id && t.Userid == userId);

            if (table == null)
                return NotFound("Table not found");

            // Verify waiter exists and belongs to this hotel
            var waiter = await _context.Staffmembers
                .FirstOrDefaultAsync(s => s.Staffid == waiterId
                                       && s.Userid == userId
                                       && s.Role == "Waiter"
                                       && s.Isactive == true);

            if (waiter == null)
                return NotFound("Waiter not found or inactive");

            table.Assignedwaiterid = waiterId;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Waiter {waiter.Fullname} assigned to Table {table.Tablenumber}",
                table.Tableid,
                table.Tablenumber,
                AssignedWaiter = waiter.Fullname
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 7. UPDATE TABLE STATUS — Owner, Manager, Waiter
        // PATCH /api/tables/5/status
        // ─────────────────────────────────────────────────────────────
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTableStatus(int id, [FromBody] TableStatusRequest request)
        {
            var role = GetRole();
            if (role == "Chef")
                return Forbid();

            var validStatuses = new[] { "Empty", "Occupied", "Billed" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest("Status must be Empty, Occupied or Billed");

            var userId = GetUserId();
            var table = await _context.Hoteltables
                .FirstOrDefaultAsync(t => t.Tableid == id && t.Userid == userId);

            if (table == null)
                return NotFound("Table not found");

            table.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Table status updated",
                table.Tableid,
                table.Tablenumber,
                table.Status
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 8. DELETE TABLE — Owner only
        // DELETE /api/tables/5
        // ─────────────────────────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var role = GetRole();
            if (role != "Owner")
                return Forbid();

            var userId = GetUserId();
            var table = await _context.Hoteltables
                .FirstOrDefaultAsync(t => t.Tableid == id && t.Userid == userId);

            if (table == null)
                return NotFound("Table not found");

            _context.Hoteltables.Remove(table);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Table deleted successfully" });
        }
    }

    // ─── Request Models ─────────────────────────────────────────────
    public class TableRequest
    {
        public string Tablenumber { get; set; }
    }

    public class TableStatusRequest
    {
        public string Status { get; set; }
    }
}


