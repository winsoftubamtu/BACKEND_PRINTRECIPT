using System;
using System.Collections.Generic;

namespace finalhotelAPI.Models;

public partial class Staffmember
{
    public int Staffid { get; set; }

    public int Userid { get; set; }

    public string Fullname { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool? Isactive { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual ICollection<Hoteltable> Hoteltables { get; set; } = new List<Hoteltable>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
