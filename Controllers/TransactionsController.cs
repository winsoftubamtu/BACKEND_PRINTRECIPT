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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly WebDbContext _context;

        public TransactionsController(WebDbContext context)
        {
            _context = context;
        }

        // POST: api/transactions
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            // Create transaction header
            var transaction = new Transaction
            {
                Userid = userId,
                Totalamount = dto.TotalAmount,
                Paymentmethod = dto.PaymentMethod,
                Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified) // 👈 Fix
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(); // so we get TransactionId

            // Create transaction items
            foreach (var itemDto in dto.Items)
            {
                var transactionItem = new Transactionitem
                {
                    Transactionid = transaction.Transactionid,
                    Itemid = itemDto.ItemId,
                    Quantity = itemDto.Quantity,
                    Priceatsale = itemDto.PriceAtSale
                };

                _context.Transactionitems.Add(transactionItem);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Transaction saved successfully", TransactionId = transaction.Transactionid });
        }

        // GET: api/transactions
        [HttpGet]
        public async Task<IActionResult> GetMyTransactions()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            var transactions = await _context.Transactions
        .Where(t => t.Userid == userId)
        .Include(t => t.Transactionitems)
            .ThenInclude(ti => ti.Item)
        .OrderByDescending(t => t.Createdat)
        .Select(t => new CreateTransactionDto
        {
            TransactionId = t.Transactionid,
            TotalAmount = t.Totalamount,
            PaymentMethod = t.Paymentmethod,
            CreatedAt = t.Createdat,
            Items = t.Transactionitems.Select(ti => new TransactionItemDto
            {
                ItemId = ti.Itemid,
                ItemName = ti.Item.Itemname,   // 👈 taking from Item table
                Quantity = ti.Quantity,
                PriceAtSale = ti.Priceatsale
            }).ToList()
        })
        .ToListAsync();

            return Ok(transactions);
        }


        // GET: api/transactions/byDate?fromDate=2025-09-01&toDate=2025-09-18
        [HttpGet("byDate")]
        public async Task<IActionResult> GetTransactionsByDate([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            var query = _context.Transactions
                .Where(t => t.Userid == userId)
                .Include(t => t.Transactionitems)
                    .ThenInclude(ti => ti.Item)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(t => t.Createdat >= fromDate.Value);

            //if (toDate.HasValue)
            //    query = query.Where(t => t.Createdat <= toDate.Value);

            if (toDate.HasValue)
            {
                var nextDay = toDate.Value.AddDays(1); // move to start of next day
                query = query.Where(t => t.Createdat < nextDay);
            }

            var transactions = await query
                .OrderByDescending(t => t.Createdat)
                .Select(t => new CreateTransactionDto
                {
                    TransactionId = t.Transactionid,
                    TotalAmount = t.Totalamount,
                    PaymentMethod = t.Paymentmethod,
                    CreatedAt = t.Createdat,
                    Items = t.Transactionitems.Select(ti => new TransactionItemDto
                    {
                        ItemId = ti.Itemid,
                        ItemName = ti.Item.Itemname,
                        Quantity = ti.Quantity,
                        PriceAtSale = ti.Priceatsale
                    }).ToList()
                })
                .ToListAsync();

            return Ok(transactions);
        }
    }

    // DTOs
    public class CreateTransactionDto
    {
        public int TransactionId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? CreatedAt { get; set; }   // 👈 Nullable
        public List<TransactionItemDto> Items { get; set; } = new();
    }

    public class TransactionItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal PriceAtSale { get; set; }
    }
}
