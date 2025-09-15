using System;
using System.Collections.Generic;

namespace finalhotelAPI.Models;

public partial class User
{
    public int Userid { get; set; }

    public string Username { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Storename { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
