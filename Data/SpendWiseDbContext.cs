using Microsoft.EntityFrameworkCore;
using SpendWise.Models;

namespace SpendWise.Data;

public sealed class SpendWiseDbContext(DbContextOptions<SpendWiseDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<CategoryRule> CategoryRules => Set<CategoryRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Balance).HasPrecision(18, 2);
            entity.Property(x => x.Operation).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ClassificationSource).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.TransactionDate, x.CategoryId });
            entity.HasIndex(x => new { x.TransactionDate, x.Amount, x.Description, x.Reference }).IsUnique();
            entity.HasOne(x => x.Category).WithMany(x => x.Transactions).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ImportBatch).WithMany(x => x.Transactions).HasForeignKey(x => x.ImportBatchId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<ImportBatch>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        });

        modelBuilder.Entity<CategoryRule>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Field).HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.Match).HasConversion<string>().HasMaxLength(50);
            entity.Property(x => x.Pattern).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => new { x.Field, x.Match, x.Pattern }).IsUnique();
            entity.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
