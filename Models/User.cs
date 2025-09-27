using System;
using System.Collections.Generic;

namespace finalhotelAPI.Models;

public partial class User
{
    public int Userid { get; set; }

    public string Username { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Storename { get; set; } = null!;

    public string? Address { get; set; }

    public DateOnly? Startdate { get; set; }

    public DateOnly? Expirydate { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
