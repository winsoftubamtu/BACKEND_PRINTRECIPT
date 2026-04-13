using System;
using System.Collections.Generic;

namespace finalhotelAPI.Models;

public partial class Hoteltable
{
    public int Tableid { get; set; }

    public int Userid { get; set; }

    public string Tablenumber { get; set; } = null!;

    public string? Status { get; set; }

    public int? Assignedwaiterid { get; set; }

    public virtual Staffmember? Assignedwaiter { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}
