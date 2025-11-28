namespace Main.Core.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = null!;

    public string NormalizedUserName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string PasswordHash { get; set; } = null!;

    public bool TwoFactorEnabled { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public int AccessFailedCount { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<ArticleSupplierBuyInfo> ArticleSupplierBuyInfos { get; set; } = new List<ArticleSupplierBuyInfo>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Purchase> PurchaseCreatedUsers { get; set; } = new List<Purchase>();

    public virtual ICollection<Purchase> PurchaseSuppliers { get; set; } = new List<Purchase>();

    public virtual ICollection<Purchase> PurchaseUpdatedUsers { get; set; } = new List<Purchase>();

    public virtual ICollection<Sale> SaleBuyers { get; set; } = new List<Sale>();

    public virtual ICollection<Sale> SaleCreatedUsers { get; set; } = new List<Sale>();

    public virtual ICollection<Sale> SaleUpdatedUsers { get; set; } = new List<Sale>();

    public virtual ICollection<StorageContentReservation> StorageContentReservationUsers { get; set; } = new List<StorageContentReservation>();

    public virtual ICollection<StorageContentReservation> StorageContentReservationWhoCreatedNavigations { get; set; } = new List<StorageContentReservation>();

    public virtual ICollection<StorageContentReservation> StorageContentReservationWhoUpdatedNavigations { get; set; } = new List<StorageContentReservation>();

    public virtual ICollection<StorageMovement> StorageMovements { get; set; } = new List<StorageMovement>();

    public virtual ICollection<Transaction> TransactionDeletedByNavigations { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionReceivers { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionSenders { get; set; } = new List<Transaction>();

    public virtual ICollection<TransactionVersion> TransactionVersionReceivers { get; set; } = new List<TransactionVersion>();

    public virtual ICollection<TransactionVersion> TransactionVersionSenders { get; set; } = new List<TransactionVersion>();

    public virtual ICollection<Transaction> TransactionWhoMadeUsers { get; set; } = new List<Transaction>();

    public virtual ICollection<UserBalance> UserBalances { get; set; } = new List<UserBalance>();

    public virtual UserDiscount? UserDiscount { get; set; }

    public virtual ICollection<UserEmail> UserEmails { get; set; } = new List<UserEmail>();

    public virtual UserInfo? UserInfo { get; set; }

    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

    public virtual ICollection<UserPhone> UserPhones { get; set; } = new List<UserPhone>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<UserSearchHistory> UserSearchHistories { get; set; } = new List<UserSearchHistory>();

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();

    public virtual ICollection<UserVehicle> UserVehicles { get; set; } = new List<UserVehicle>();
}
