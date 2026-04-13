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

    public string? Phonenumber { get; set; }

    public virtual ICollection<Hoteltable> Hoteltables { get; set; } = new List<Hoteltable>();

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    public virtual ICollection<Staffmember> Staffmembers { get; set; } = new List<Staffmember>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
