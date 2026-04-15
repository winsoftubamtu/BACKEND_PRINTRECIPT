//namespace finalhotelAPI.Controllers
//{
//    public class ChefController
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
    public class ChefController : ControllerBase
    {
        private readonly WebDbContext _context;

        public ChefController(WebDbContext context)
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
        // 1. GET ALL PENDING ORDERS — Chef, Owner, Manager
        // GET /api/chef/orders
        // ─────────────────────────────────────────────────────────────
        [HttpGet("orders")]
        public async Task<IActionResult> GetPendingOrders()
        {
            var role = GetRole();
            if (role == "Waiter")
                return Forbid();

            var userId = GetUserId();

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Userid == userId &&
                           (o.Status == "Pending" || o.Status == "Preparing"))
                .Select(o => new
                {
                    o.Orderid,
                    o.Status,
                    o.Createdat,
                    o.Updatedat,
                    Table = _context.Hoteltables
                        .Where(t => t.Tableid == o.Tableid)
                        .Select(t => t.Tablenumber)
                        .FirstOrDefault(),
                    Waiter = _context.Staffmembers
                        .Where(s => s.Staffid == o.Waiterid)
                        .Select(s => s.Fullname)
                        .FirstOrDefault(),
                    Items = _context.Orderitems
                        .Where(oi => oi.Orderid == o.Orderid)
                        .Select(oi => new
                        {
                            oi.Orderitemid,
                            oi.Quantity,
                            oi.Note,
                            oi.Status,
                            ItemName = _context.Items
                                .Where(i => i.Itemid == oi.Itemid)
                                .Select(i => i.Itemname)
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .OrderBy(o => o.Createdat) // Oldest first — FIFO kitchen queue
                .ToListAsync();

            return Ok(orders);
        }

        // ─────────────────────────────────────────────────────────────
        // 2. GET ALL READY ORDERS — Chef, Owner, Manager
        // GET /api/chef/orders/ready
        // ─────────────────────────────────────────────────────────────
        [HttpGet("orders/ready")]
        public async Task<IActionResult> GetReadyOrders()
        {
            var role = GetRole();
            if (role == "Waiter")
                return Forbid();

            var userId = GetUserId();

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Userid == userId && o.Status == "Ready")
                .Select(o => new
                {
                    o.Orderid,
                    o.Status,
                    o.Createdat,
                    o.Updatedat,
                    Table = _context.Hoteltables
                        .Where(t => t.Tableid == o.Tableid)
                        .Select(t => t.Tablenumber)
                        .FirstOrDefault(),
                    Waiter = _context.Staffmembers
                        .Where(s => s.Staffid == o.Waiterid)
                        .Select(s => s.Fullname)
                        .FirstOrDefault(),
                    Items = _context.Orderitems
                        .Where(oi => oi.Orderid == o.Orderid)
                        .Select(oi => new
                        {
                            oi.Orderitemid,
                            oi.Quantity,
                            oi.Note,
                            ItemName = _context.Items
                                .Where(i => i.Itemid == oi.Itemid)
                                .Select(i => i.Itemname)
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .OrderByDescending(o => o.Updatedat)
                .ToListAsync();

            return Ok(orders);
        }

        // ─────────────────────────────────────────────────────────────
        // 3. GET SINGLE ORDER DETAIL — Chef, Owner, Manager
        // GET /api/chef/orders/5
        // ─────────────────────────────────────────────────────────────
        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var role = GetRole();
            if (role == "Waiter")
                return Forbid();

            var userId = GetUserId();

            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Orderid == id && o.Userid == userId)
                .Select(o => new
                {
                    o.Orderid,
                    o.Status,
                    o.Createdat,
                    o.Updatedat,
                    Table = _context.Hoteltables
                        .Where(t => t.Tableid == o.Tableid)
                        .Select(t => t.Tablenumber)
                        .FirstOrDefault(),
                    Waiter = _context.Staffmembers
                        .Where(s => s.Staffid == o.Waiterid)
                        .Select(s => s.Fullname)
                        .FirstOrDefault(),
                    Items = _context.Orderitems
                        .Where(oi => oi.Orderid == o.Orderid)
                        .Select(oi => new
                        {
                            oi.Orderitemid,
                            oi.Quantity,
                            oi.Note,
                            oi.Status,
                            ItemName = _context.Items
                                .Where(i => i.Itemid == oi.Itemid)
                                .Select(i => i.Itemname)
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound("Order not found");

            return Ok(order);
        }

        // ─────────────────────────────────────────────────────────────
        // 4. START PREPARING ORDER — Chef, Owner, Manager
        // PATCH /api/chef/orders/5/preparing
        // ─────────────────────────────────────────────────────────────
        [HttpPatch("orders/{id}/preparing")]
        public async Task<IActionResult> StartPreparing(int id)
        {
            var role = GetRole();
            if (role == "Waiter")
                return Forbid();

            var userId = GetUserId();
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Orderid == id && o.Userid == userId);

            if (order == null)
                return NotFound("Order not found");

            if (order.Status != "Pending")
                return BadRequest($"Cannot prepare. Current status is {order.Status}");

            order.Status = "Preparing";
            order.Updatedat = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Order is now being prepared",
                OrderId = order.Orderid,
                Status = order.Status
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 5. MARK ORDER READY — Chef, Owner, Manager
        // PATCH /api/chef/orders/5/ready
        // ─────────────────────────────────────────────────────────────
        [HttpPatch("orders/{id}/ready")]
        public async Task<IActionResult> MarkOrderReady(int id)
        {
            var role = GetRole();
            if (role == "Waiter")
                return Forbid();

            var userId = GetUserId();
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Orderid == id && o.Userid == userId);

            if (order == null)
                return NotFound("Order not found");

            if (order.Status != "Preparing")
                return BadRequest($"Cannot mark ready. Current status is {order.Status}");

            order.Status = "Ready";
            order.Updatedat = DateTime.Now;

            // Mark all order items as Ready too
            var orderItems = await _context.Orderitems
                .Where(oi => oi.Orderid == id)
                .ToListAsync();

            foreach (var item in orderItems)
                item.Status = "Ready";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Order is Ready! Notify waiter to serve",
                OrderId = order.Orderid,
                Status = order.Status,
                TableId = order.Tableid
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 6. MARK ORDER SERVED — Owner, Manager, Waiter
        // PATCH /api/chef/orders/5/served
        // ─────────────────────────────────────────────────────────────
        [HttpPatch("orders/{id}/served")]
        public async Task<IActionResult> MarkOrderServed(int id)
        {
            var role = GetRole();
            if (role == "Chef")
                return Forbid(); // Chef cannot mark served — waiter does this

            var userId = GetUserId();
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Orderid == id && o.Userid == userId);

            if (order == null)
                return NotFound("Order not found");

            if (order.Status != "Ready")
                return BadRequest($"Cannot mark served. Current status is {order.Status}");

            order.Status = "Served";
            order.Updatedat = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Order marked as Served",
                OrderId = order.Orderid,
                Status = order.Status
            });
        }
    }
}
