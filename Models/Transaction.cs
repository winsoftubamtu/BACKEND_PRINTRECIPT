using System;
using System.Collections.Generic;

namespace finalhotelAPI.Models;

public partial class Transaction
{
    public int Transactionid { get; set; }

    public int Userid { get; set; }

    public decimal Totalamount { get; set; }

    public string Paymentmethod { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public virtual ICollection<Transactionitem> Transactionitems { get; set; } = new List<Transactionitem>();

    public virtual User User { get; set; } = null!;
}
