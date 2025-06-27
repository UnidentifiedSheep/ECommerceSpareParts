using System;
using System.Collections.Generic;

namespace MonoliteUnicorn.PostGres.Main;

public partial class AspNetUser
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public bool IsSupplier { get; set; }

    public virtual ICollection<ArticleSupplierBuyInfo> ArticleSupplierBuyInfos { get; set; } = new List<ArticleSupplierBuyInfo>();

    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();

    public virtual ICollection<Purchase> PurchaseCreatedUsers { get; set; } = new List<Purchase>();

    public virtual ICollection<Purchase> PurchaseSuppliers { get; set; } = new List<Purchase>();

    public virtual ICollection<Purchase> PurchaseUpdatedUsers { get; set; } = new List<Purchase>();

    public virtual ICollection<Sale> SaleBuyers { get; set; } = new List<Sale>();

    public virtual ICollection<Sale> SaleCreatedUsers { get; set; } = new List<Sale>();

    public virtual ICollection<Sale> SaleUpdatedUsers { get; set; } = new List<Sale>();

    public virtual ICollection<StorageContentReservation> StorageContentReservations { get; set; } = new List<StorageContentReservation>();

    public virtual ICollection<StorageMovement> StorageMovements { get; set; } = new List<StorageMovement>();

    public virtual ICollection<Transaction> TransactionDeletedByNavigations { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionReceivers { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionSenders { get; set; } = new List<Transaction>();

    public virtual ICollection<TransactionVersion> TransactionVersionReceivers { get; set; } = new List<TransactionVersion>();

    public virtual ICollection<TransactionVersion> TransactionVersionSenders { get; set; } = new List<TransactionVersion>();

    public virtual ICollection<Transaction> TransactionWhoMadeUsers { get; set; } = new List<Transaction>();

    public virtual ICollection<UserBalance> UserBalances { get; set; } = new List<UserBalance>();

    public virtual UserDiscount? UserDiscount { get; set; }

    public virtual ICollection<UserMail> UserMails { get; set; } = new List<UserMail>();

    public virtual ICollection<UserSearchHistory> UserSearchHistories { get; set; } = new List<UserSearchHistory>();

    public virtual ICollection<UserVehicle> UserVehicles { get; set; } = new List<UserVehicle>();

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
