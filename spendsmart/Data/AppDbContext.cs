using Microsoft.EntityFrameworkCore;
using spendsmart.Models;

namespace spendsmart.Data;

public class AppDbContext : DbContext
{
    private const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=SpendSmartDb;Trusted_Connection=True;TrustServerCertificate=True;";

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(DefaultConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUser(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureTransaction(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.UserId);

            entity.Property(user => user.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(user => user.Password)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(user => user.CreatedAt)
                .IsRequired();

            entity.HasIndex(user => user.Email)
                .IsUnique();
        });
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(category => category.CategoryId);

            entity.Property(category => category.Name)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(category => category.Type)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(category => category.IconName)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(category => category.Color)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(category => category.User)
                .WithMany(user => user.Categories)
                .HasForeignKey(category => category.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(category => new { category.UserId, category.Type, category.Name })
                .IsUnique();
        });
    }

    private static void ConfigureTransaction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(transaction => transaction.TransactionId);

            entity.Property(transaction => transaction.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(transaction => transaction.Type)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(transaction => transaction.Date)
                .IsRequired();

            entity.Property(transaction => transaction.Note)
                .HasMaxLength(255);

            entity.Property(transaction => transaction.CreatedAt)
                .IsRequired();

            entity.HasOne(transaction => transaction.User)
                .WithMany(user => user.Transactions)
                .HasForeignKey(transaction => transaction.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(transaction => transaction.Category)
                .WithMany(category => category.Transactions)
                .HasForeignKey(transaction => transaction.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(transaction => new { transaction.UserId, transaction.Date });
        });
    }
}
