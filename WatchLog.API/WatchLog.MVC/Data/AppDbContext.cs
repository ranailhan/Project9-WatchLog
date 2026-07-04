using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WatchLog.MVC.Models.Entities;

namespace WatchLog.MVC.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Watchlist>     Watchlists     { get; set; }
    public DbSet<WatchlistItem> WatchlistItems { get; set; }
    public DbSet<Review>        Reviews        { get; set; }
    public DbSet<Rating>        Ratings        { get; set; }
    public DbSet<Favorite>      Favorites      { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Watchlist → User (1:N)
        builder.Entity<Watchlist>()
            .HasOne(w => w.User)
            .WithMany(u => u.Watchlists)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // WatchlistItem → Watchlist (1:N)
        builder.Entity<WatchlistItem>()
            .HasOne(wi => wi.Watchlist)
            .WithMany(w => w.Items)
            .HasForeignKey(wi => wi.WatchlistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Review → User (1:N)
        builder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Rating → User (1:N)
        builder.Entity<Rating>()
            .HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Favorite → User (1:N)
        builder.Entity<Favorite>()
            .HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique: Bir kullanıcı aynı içeriğe tek puan verebilir
        builder.Entity<Rating>()
            .HasIndex(r => new { r.UserId, r.ContentId, r.ContentType })
            .IsUnique();

        // Unique: Bir kullanıcı aynı içeriği tek favori edebilir
        builder.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.ContentId, f.ContentType })
            .IsUnique();
    }
}
