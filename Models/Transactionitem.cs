using System;
using System.Collections.Generic;

namespace finalhotelAPI.Models;

public partial class Transactionitem
{
    public int Transactionitemid { get; set; }

    public int Transactionid { get; set; }

    public int Itemid { get; set; }

    public int Quantity { get; set; }

    public decimal Priceatsale { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
