using System;
using System.Collections.Generic;

namespace finalhotelAPI.Models;

public partial class Orderitem
{
    public int Orderitemid { get; set; }

    public int Orderid { get; set; }

    public int Itemid { get; set; }

    public int Quantity { get; set; }

    public string? Note { get; set; }

    public string? Status { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
