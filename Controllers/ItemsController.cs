using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using finalhotelAPI.Data;
using finalhotelAPI.Models;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

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
        //[HttpGet]
        //public async Task<IActionResult> GetMyItems()
        //{
        //    // Get userId from JWT claims
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (string.IsNullOrEmpty(userIdClaim))
        //        return Unauthorized("Invalid token.");

        //    int userId = int.Parse(userIdClaim);

        //    var items = await _context.Items
        //        .Where(i => i.Userid == userId)
        //        .ToListAsync();

        //    return Ok(items);
        //}

        //// POST: api/items
        //[HttpPost]
        //public async Task<IActionResult> AddItem([FromBody] Item item)
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (string.IsNullOrEmpty(userIdClaim))
        //        return Unauthorized("Invalid token.");

        //    int userId = int.Parse(userIdClaim);

        //    item.Userid = userId; // Force item to belong to logged-in user

        //    _context.Items.Add(item);
        //    await _context.SaveChangesAsync();

        //    return Ok(item);
        //}




        //Update optimized code below
        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
        // GET: api/items
        [HttpGet]
        public async Task<IActionResult> GetMyItems()
        {
            int userId = GetUserId();

            var items = await _context.Items
                .AsNoTracking()   // 🚀 PERFORMANCE BOOST
                .Where(i => i.Userid == userId)
                .Select(i => new ItemDto
                {
                    Itemid = i.Itemid,
                    Itemname = i.Itemname,
                    Price = i.Price
                })
                .ToListAsync();

            return Ok(items);
        }

        // POST: api/items
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] CreateItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Itemname) || dto.Price <= 0)
                return BadRequest("Invalid item data");

            int userId = GetUserId();

            var item = new Item
            {
                Userid = userId,
                Itemname = dto.Itemname,
                Price = dto.Price
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                item.Itemid,
                item.Itemname,
                item.Price
            });
        }


        // PUT: api/items/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] CreateItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Itemname) || dto.Price <= 0)
                return BadRequest("Invalid item data");

            int userId = GetUserId();

            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Itemid == id && i.Userid == userId);

            if (item == null)
                return NotFound("Item not found");

            item.Itemname = dto.Itemname;
            item.Price = dto.Price;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                item.Itemid,
                item.Itemname,
                item.Price
            });
        }


        // DELETE: api/items/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            int userId = GetUserId();

            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Itemid == id && i.Userid == userId);

            if (item == null)
                return NotFound("Item not found");

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Item deleted successfully" });
        }





        public class ItemDto
        {
            public int Itemid { get; set; }
            public string Itemname { get; set; }
            public decimal Price { get; set; }
        }

        public class CreateItemDto
        {
            public string Itemname { get; set; }
            public decimal Price { get; set; }
        }

    }
}
