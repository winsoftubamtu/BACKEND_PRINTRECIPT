using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using finalhotelAPI.Models;

namespace finalhotelAPI.Data;

public partial class WebDbContext : DbContext
{
    public WebDbContext()
    {
    }

    public WebDbContext(DbContextOptions<WebDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Hoteltable> Hoteltables { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderitem> Orderitems { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<Staffmember> Staffmembers { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<Transactionitem> Transactionitems { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=192.168.10.48;Database=fooddbhosted;Username=postgres;Password=root");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hoteltable>(entity =>
        {
            entity.HasKey(e => e.Tableid).HasName("hoteltables_pkey");

            entity.ToTable("hoteltables");

            entity.Property(e => e.Tableid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("tableid");
            entity.Property(e => e.Assignedwaiterid).HasColumnName("assignedwaiterid");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Empty'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Tablenumber)
                .HasMaxLength(20)
                .HasColumnName("tablenumber");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Assignedwaiter).WithMany(p => p.Hoteltables)
                .HasForeignKey(d => d.Assignedwaiterid)
                .HasConstraintName("hoteltables_assignedwaiterid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Hoteltables)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("hoteltables_userid_fkey");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Itemid).HasName("items_pkey");

            entity.ToTable("items");

            entity.Property(e => e.Itemid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("itemid");
            entity.Property(e => e.Itemname)
                .HasMaxLength(100)
                .HasColumnName("itemname");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Items)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("items_userid_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Orderid).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.Property(e => e.Orderid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("orderid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Tableid).HasColumnName("tableid");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Waiterid).HasColumnName("waiterid");

            entity.HasOne(d => d.Table).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Tableid)
                .HasConstraintName("orders_tableid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_userid_fkey");

            entity.HasOne(d => d.Waiter).WithMany(p => p.Orders)
                .HasForeignKey(d => d.Waiterid)
                .HasConstraintName("orders_waiterid_fkey");
        });

        modelBuilder.Entity<Orderitem>(entity =>
        {
            entity.HasKey(e => e.Orderitemid).HasName("orderitems_pkey");

            entity.ToTable("orderitems");

            entity.Property(e => e.Orderitemid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("orderitemid");
            entity.Property(e => e.Itemid).HasColumnName("itemid");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Item).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.Itemid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orderitems_itemid_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("orderitems_orderid_fkey");
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.Purchaseid).HasName("purchases_pkey");

            entity.ToTable("purchases");

            entity.Property(e => e.Purchaseid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("purchaseid");
            entity.Property(e => e.Itemname)
                .HasMaxLength(100)
                .HasColumnName("itemname");
            entity.Property(e => e.Paymentmethod)
                .HasMaxLength(20)
                .HasColumnName("paymentmethod");
            entity.Property(e => e.Priceatpurchase)
                .HasPrecision(10, 2)
                .HasColumnName("priceatpurchase");
            entity.Property(e => e.Purchasedate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("purchasedate");
            entity.Property(e => e.Quantity)
                .HasMaxLength(50)
                .HasColumnName("quantity");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Purchases)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("purchases_userid_fkey");
        });

        modelBuilder.Entity<Staffmember>(entity =>
        {
            entity.HasKey(e => e.Staffid).HasName("staffmembers_pkey");

            entity.ToTable("staffmembers");

            entity.HasIndex(e => e.Username, "staffmembers_username_key").IsUnique();

            entity.Property(e => e.Staffid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("staffid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(200)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.User).WithMany(p => p.Staffmembers)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("staffmembers_userid_fkey");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Transactionid).HasName("transactions_pkey");

            entity.ToTable("transactions");

            entity.Property(e => e.Transactionid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("transactionid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Paymentmethod)
                .HasMaxLength(20)
                .HasColumnName("paymentmethod");
            entity.Property(e => e.Staffid).HasColumnName("staffid");
            entity.Property(e => e.Totalamount)
                .HasPrecision(10, 2)
                .HasColumnName("totalamount");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Order).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.Orderid)
                .HasConstraintName("transactions_orderid_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.Staffid)
                .HasConstraintName("transactions_staffid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_userid_fkey");
        });

        modelBuilder.Entity<Transactionitem>(entity =>
        {
            entity.HasKey(e => e.Transactionitemid).HasName("transactionitems_pkey");

            entity.ToTable("transactionitems");

            entity.Property(e => e.Transactionitemid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("transactionitemid");
            entity.Property(e => e.Itemid).HasColumnName("itemid");
            entity.Property(e => e.Priceatsale)
                .HasPrecision(10, 2)
                .HasColumnName("priceatsale");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Transactionid).HasColumnName("transactionid");

            entity.HasOne(d => d.Item).WithMany(p => p.Transactionitems)
                .HasForeignKey(d => d.Itemid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactionitems_itemid_fkey");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Transactionitems)
                .HasForeignKey(d => d.Transactionid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactionitems_transactionid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Userid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("userid");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");
            entity.Property(e => e.Expirydate).HasColumnName("expirydate");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(200)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(15)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Startdate).HasColumnName("startdate");
            entity.Property(e => e.Storename)
                .HasMaxLength(100)
                .HasColumnName("storename");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
