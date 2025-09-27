using System;
using System.Collections.Generic;

namespace finalhotelAPI.Models;

public partial class Purchase
{
    public int Purchaseid { get; set; }

    public int Userid { get; set; }

    public string Itemname { get; set; } = null!;

    public string Quantity { get; set; } = null!;

    public decimal Priceatpurchase { get; set; }

    public string Paymentmethod { get; set; } = null!;

    public DateTime? Purchasedate { get; set; }

    public virtual User User { get; set; } = null!;
}
