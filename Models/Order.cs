using System;
using System.Collections.Generic;

namespace finalhotelAPI.Models;

public partial class Order
{
    public int Orderid { get; set; }

    public int Userid { get; set; }

    public int? Tableid { get; set; }

    public int? Waiterid { get; set; }

    public string? Status { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();

    public virtual Hoteltable? Table { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;

    public virtual Staffmember? Waiter { get; set; }
}
