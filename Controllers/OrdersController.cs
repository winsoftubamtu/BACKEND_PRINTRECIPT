//namespace finalhotelAPI.Controllers
//{
//    public class OrdersController
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
    public class OrdersController : ControllerBase
    {
        private readonly WebDbContext _context;

        public OrdersController(WebDbContext context)
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

        private int GetStaffId()
        {
            var val = User.FindFirst("StaffId")?.Value;
            return string.IsNullOrEmpty(val) ? 0 : int.Parse(val);
        }

        // ─────────────────────────────────────────────────────────────
        // 1. GET ALL ORDERS TODAY — Owner, Manager
        // GET /api/orders
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetTodayOrders()
        {
            var role = GetRole();
            if (role == "Chef" || role == "Waiter")
                return Forbid();

            var userId = GetUserId();
            var today = DateTime.Now.Date;

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Userid == userId &&
                            o.Createdat.HasValue &&
                            o.Createdat.Value.Date == today)
                .Select(o => new
                {
                    o.Orderid,
                    o.Status,
                    o.Createdat,
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
                                .FirstOrDefault(),
                            Price = _context.Items
                                .Where(i => i.Itemid == oi.Itemid)
                                .Select(i => i.Price)
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .OrderByDescending(o => o.Createdat)
                .ToListAsync();

            return Ok(orders);
        }

        // ─────────────────────────────────────────────────────────────
        // 2. GET ORDERS BY TABLE — Owner, Manager, Waiter
        // GET /api/orders/table/5
        // ─────────────────────────────────────────────────────────────
        [HttpGet("table/{tableId}")]
        public async Task<IActionResult> GetOrdersByTable(int tableId)
        {
            var role = GetRole();
            if (role == "Chef")
                return Forbid();

            var userId = GetUserId();

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Userid == userId &&
                            o.Tableid == tableId &&
                            o.Status != "Billed")
                .Select(o => new
                {
                    o.Orderid,
                    o.Status,
                    o.Createdat,
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
                                .FirstOrDefault(),
                            Price = _context.Items
                                .Where(i => i.Itemid == oi.Itemid)
                                .Select(i => i.Price)
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .OrderByDescending(o => o.Createdat)
                .ToListAsync();

            return Ok(orders);
        }

        // ─────────────────────────────────────────────────────────────
        // 3. GET SINGLE ORDER — Owner, Manager, Waiter
        // GET /api/orders/5
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var role = GetRole();
            if (role == "Chef")
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
                                .FirstOrDefault(),
                            Price = _context.Items
                                .Where(i => i.Itemid == oi.Itemid)
                                .Select(i => i.Price)
                                .FirstOrDefault(),
                            LineTotal = _context.Items
                                .Where(i => i.Itemid == oi.Itemid)
                                .Select(i => i.Price * oi.Quantity)
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
        // 4. PLACE NEW ORDER — Owner, Manager, Waiter
        // POST /api/orders
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            var role = GetRole();
            if (role == "Chef")
                return Forbid();

            if (request.Items == null || request.Items.Count == 0)
                return BadRequest("Order must have at least one item");

            var userId = GetUserId();
            var staffId = GetStaffId();

            // Verify table belongs to this hotel
            var table = await _context.Hoteltables
                .FirstOrDefaultAsync(t => t.Tableid == request.TableId && t.Userid == userId);
            if (table == null)
                return NotFound("Table not found");

            // Verify all items belong to this hotel
            var itemIds = request.Items.Select(i => i.ItemId).ToList();
            var validItems = await _context.Items
                .Where(i => itemIds.Contains(i.Itemid) && i.Userid == userId)
                .ToListAsync();
            if (validItems.Count != itemIds.Count)
                return BadRequest("One or more items are invalid");

            // Create order
            var order = new Order
            {
                Userid = userId,
                Tableid = request.TableId,
                Waiterid = staffId == 0 ? null : staffId, // null if Owner places order
                Status = "Pending",
                Createdat = DateTime.Now,
                Updatedat = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Save to get OrderId

            // Add order items
            foreach (var item in request.Items)
            {
                var itemDetails = validItems.First(i => i.Itemid == item.ItemId);
                var orderItem = new Orderitem
                {
                    Orderid = order.Orderid,
                    Itemid = item.ItemId,
                    Quantity = item.Quantity,
                    Note = item.Note ?? "",
                    Status = "Pending"
                };
                _context.Orderitems.Add(orderItem);
            }

            // Update table status to Occupied
            table.Status = "Occupied";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Order placed successfully",
                OrderId = order.Orderid,
                TableNumber = table.Tablenumber,
                Status = order.Status,
                ItemsCount = request.Items.Count
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 5. ADD MORE ITEMS TO EXISTING ORDER — Owner, Manager, Waiter
        // POST /api/orders/5/additems
        // ─────────────────────────────────────────────────────────────
        [HttpPost("{id}/additems")]
        public async Task<IActionResult> AddItemsToOrder(int id, [FromBody] List<OrderItemRequest> items)
        {
            var role = GetRole();
            if (role == "Chef")
                return Forbid();

            if (items == null || items.Count == 0)
                return BadRequest("No items provided");

            var userId = GetUserId();
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Orderid == id && o.Userid == userId);

            if (order == null)
                return NotFound("Order not found");

            if (order.Status == "Billed")
                return BadRequest("Cannot add items to a billed order");

            // Verify items
            var itemIds = items.Select(i => i.ItemId).ToList();
            var validItems = await _context.Items
                .Where(i => itemIds.Contains(i.Itemid) && i.Userid == userId)
                .ToListAsync();
            if (validItems.Count != itemIds.Count)
                return BadRequest("One or more items are invalid");

            foreach (var item in items)
            {
                _context.Orderitems.Add(new Orderitem
                {
                    Orderid = order.Orderid,
                    Itemid = item.ItemId,
                    Quantity = item.Quantity,
                    Note = item.Note ?? "",
                    Status = "Pending"
                });
            }

            order.Updatedat = DateTime.Now;
            order.Status = "Pending"; // Reset to pending for chef
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Items added to order successfully",
                OrderId = order.Orderid,
                NewItemsCount = items.Count
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 6. CANCEL ORDER — Owner, Manager only
        // PATCH /api/orders/5/cancel
        // ─────────────────────────────────────────────────────────────
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var role = GetRole();
            if (role != "Owner" && role != "Manager")
                return Forbid();

            var userId = GetUserId();
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Orderid == id && o.Userid == userId);

            if (order == null)
                return NotFound("Order not found");

            if (order.Status == "Billed")
                return BadRequest("Cannot cancel a billed order");

            order.Status = "Cancelled";
            order.Updatedat = DateTime.Now;

            // Free the table
            if (order.Tableid.HasValue)
            {
                var table = await _context.Hoteltables
                    .FirstOrDefaultAsync(t => t.Tableid == order.Tableid);
                if (table != null)
                    table.Status = "Empty";
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order cancelled successfully" });
        }

        // ─────────────────────────────────────────────────────────────
        // 7. DELETE ORDER — Owner only
        // DELETE /api/orders/5
        // ─────────────────────────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var role = GetRole();
            if (role != "Owner")
                return Forbid();

            var userId = GetUserId();
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Orderid == id && o.Userid == userId);

            if (order == null)
                return NotFound("Order not found");

            // Delete order items first
            var orderItems = _context.Orderitems
                .Where(oi => oi.Orderid == id);
            _context.Orderitems.RemoveRange(orderItems);

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order deleted successfully" });
        }
    }

    // ─── Request Models ─────────────────────────────────────────────
    public class PlaceOrderRequest
    {
        public int TableId { get; set; }
        public List<OrderItemRequest> Items { get; set; }
    }

    public class OrderItemRequest
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }
}
