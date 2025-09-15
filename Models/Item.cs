using System;
using System.Collections.Generic;

namespace finalhotelAPI.Models;

public partial class Item
{
    public int Itemid { get; set; }

    public int Userid { get; set; }

    public string Itemname { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<Transactionitem> Transactionitems { get; set; } = new List<Transactionitem>();

    public virtual User User { get; set; } = null!;
}
