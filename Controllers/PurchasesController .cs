using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using finalhotelAPI.Data;
using finalhotelAPI.Models;

namespace finalhotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require token for all actions
    public class PurchasesController : ControllerBase
    {
        private readonly WebDbContext _context;

        public PurchasesController(WebDbContext context)
        {
            _context = context;
        }

        // POST: api/purchases
        [HttpPost]
        public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            var purchase = new Purchase
            {
                Userid = userId,
                Itemname = dto.ItemName,
                Quantity = dto.Quantity,
                Priceatpurchase = dto.PriceAtPurchase,
                Paymentmethod = dto.PaymentMethod,
                Purchasedate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Purchase saved successfully", PurchaseId = purchase.Purchaseid });
        }

        // GET: api/purchases
        [HttpGet]
        public async Task<IActionResult> GetMyPurchases()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            var purchases = await _context.Purchases
                .Where(p => p.Userid == userId)
                .OrderByDescending(p => p.Purchasedate)
                .Select(p => new CreatePurchaseDto
                {
                    PurchaseId = p.Purchaseid,
                    ItemName = p.Itemname,
                    Quantity = p.Quantity,
                    PriceAtPurchase = p.Priceatpurchase,
                    PaymentMethod = p.Paymentmethod,
                    Purchasedate = p.Purchasedate
                })
                .ToListAsync();

            return Ok(purchases);
        }

        // GET: api/purchases/byDate?fromDate=2025-09-01&toDate=2025-09-18
        [HttpGet("byDate")]
        public async Task<IActionResult> GetPurchasesByDate([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            var query = _context.Purchases
                .Where(p => p.Userid == userId)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(p => p.Purchasedate >= fromDate.Value);

            if (toDate.HasValue)
            {
                var nextDay = toDate.Value.AddDays(1);
                query = query.Where(p => p.Purchasedate < nextDay);
            }

            var purchases = await query
                .OrderByDescending(p => p.Purchasedate)
                .Select(p => new CreatePurchaseDto
                {
                    PurchaseId = p.Purchaseid,
                    ItemName = p.Itemname,
                    Quantity = p.Quantity,
                    PriceAtPurchase = p.Priceatpurchase,
                    PaymentMethod = p.Paymentmethod,
                    Purchasedate = p.Purchasedate
                })
                .ToListAsync();

            return Ok(purchases);
        }
    }

    // DTOs
    public class CreatePurchaseDto
    {
        public int PurchaseId { get; set; }
        public string ItemName { get; set; } = null!;
        public string Quantity { get; set; } = null!;
        public decimal PriceAtPurchase { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public DateTime? Purchasedate { get; set; }
    }
}
