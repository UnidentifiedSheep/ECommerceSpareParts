using System.Reflection;
using Main.Entities;
using Main.Entities.Auth;
using Main.Entities.Cart;
using Main.Entities.Currency;
using Main.Entities.Order;
using Main.Entities.Producer;
using Main.Entities.Product;
using Main.Entities.Purchase;
using Main.Entities.Sale;
using Main.Entities.Storage;
using Main.Entities.Transaction;
using Main.Entities.User;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;
using Persistence.Interceptors;

namespace Main.Persistence.Context;

public partial class DContext : DbContext
{
    public DContext()
    {
    }

    public DContext(DbContextOptions<DContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; }
    
    public virtual DbSet<ProductCross> ProductCrosses { get; set; }

    public virtual DbSet<ProductCharacteristic> ProductCharacteristics { get; set; }

    public virtual DbSet<ProductCoefficient> ProductCoefficients { get; set; }

    public virtual DbSet<ProductEan> ProductEans { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductSize> ProductSizes { get; set; }

    public virtual DbSet<ProductWeight> ProductWeights { get; set; }

    public virtual DbSet<ProductContent> ProductContents { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Coefficient> Coefficients { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<CurrencyHistory> CurrencyHistories { get; set; }

    public virtual DbSet<CurrencyToUsd> CurrencyToUsds { get; set; }

    public virtual DbSet<DefaultSetting> DefaultSettings { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderVersion> OrderVersions { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Producer> Producers { get; set; }

    public virtual DbSet<ProducerOtherName> ProducersOtherNames { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<PurchaseContent> PurchaseContents { get; set; }

    public virtual DbSet<PurchaseContentLogistic> PurchaseContentLogistics { get; set; }

    public virtual DbSet<PurchaseLogistic> PurchaseLogistics { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleContent> SaleContents { get; set; }

    public virtual DbSet<SaleContentDetail> SaleContentDetails { get; set; }

    public virtual DbSet<Storage> Storages { get; set; }

    public virtual DbSet<StorageContent> StorageContents { get; set; }

    public virtual DbSet<StorageContentReservation> StorageContentReservations { get; set; }

    public virtual DbSet<StorageOwner> StorageOwners { get; set; }

    public virtual DbSet<StorageRoute> StorageRoutes { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionVersion> TransactionVersions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBalance> UserBalances { get; set; }

    public virtual DbSet<UserDiscount> UserDiscounts { get; set; }

    public virtual DbSet<UserEmail> UserEmails { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    public virtual DbSet<UserPermission> UserPermissions { get; set; }

    public virtual DbSet<UserPhone> UserPhones { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserSearchHistory> UserSearchHistories { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    public virtual DbSet<UserVehicle> UserVehicles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        RegisterBaseInterceptors(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddInboxStateEntity();

        modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessage", "msg");
        modelBuilder.Entity<OutboxState>().ToTable("OutboxState", "msg");
        modelBuilder.Entity<InboxState>().ToTable("InboxState", "msg");

        modelBuilder
            .HasPostgresEnum("car_types", new[] { "PassengerCar", "CommercialVehicle", "Motorbike" })
            .HasPostgresExtension("dblink")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto");

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(GetType())!);

        modelBuilder.AddFieldsForAuditableEntities();

        modelBuilder.HasSequence<int>("storage_movement_id_seq");

        modelBuilder.AllDateTimesToUtc()
            .AllEnumsToString();

        OnModelCreatingPartial(modelBuilder);
    }

    private void RegisterBaseInterceptors(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new SelectForUpdateCommandInterceptor());
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}