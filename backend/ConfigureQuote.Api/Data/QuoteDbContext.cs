using ConfigureQuote.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfigureQuote.Api.Data;

public sealed class QuoteDbContext(DbContextOptions<QuoteDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<QuoteEntity> Quotes => Set<QuoteEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Name).HasMaxLength(160);
            entity.Property(product => product.Category).HasMaxLength(80);
            entity.Property(product => product.BasePrice).HasPrecision(18, 2);
            entity.Property(product => product.OptionsJson).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<QuoteEntity>(entity =>
        {
            entity.HasKey(quote => quote.Id);
            entity.HasIndex(quote => quote.QuoteNumber).IsUnique();
            entity.Property(quote => quote.QuoteNumber).HasMaxLength(32);
            entity.Property(quote => quote.CustomerName).HasMaxLength(160);
            entity.Property(quote => quote.CustomerEmail).HasMaxLength(320);
            entity.Property(quote => quote.ProductName).HasMaxLength(160);
            entity.Property(quote => quote.Total).HasPrecision(18, 2);
            entity.Property(quote => quote.Status).HasMaxLength(40);
            entity.Property(quote => quote.SelectedOptionsJson).HasColumnType("nvarchar(max)");
            entity.Property(quote => quote.Notes).HasMaxLength(2000);
        });
    }
}
