using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using finalhotelAPI.Data;
using finalhotelAPI.Models;

namespace finalhotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require token for all actions
    public class ItemsController : ControllerBase
    {
        private readonly WebDbContext _context;

        public ItemsController(WebDbContext context)
        {
            _context = context;
        }

        // GET: api/items
        [HttpGet]
        public async Task<IActionResult> GetMyItems()
        {
            // Get userId from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            var items = await _context.Items
                .Where(i => i.Userid == userId)
                .ToListAsync();

            return Ok(items);
        }

        // POST: api/items
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] Item item)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            item.Userid = userId; // Force item to belong to logged-in user

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return Ok(item);
        }
    }
}
