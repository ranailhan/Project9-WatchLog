using System.ComponentModel.DataAnnotations;

namespace WatchLog.MVC.Models.Entities;

public class Watchlist
{
    public int    Id          { get; set; }
    public string UserId      { get; set; } = null!;
    [Required, MaxLength(200)]
    public string Name        { get; set; } = null!;
    public string? Description { get; set; }
    public bool   IsPublic    { get; set; } = false;
    public bool   IsActive    { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ApplicationUser User  { get; set; } = null!;
    public ICollection<WatchlistItem> Items { get; set; } = new List<WatchlistItem>();
}

public class WatchlistItem
{
    public int    Id          { get; set; }
    public int    WatchlistId { get; set; }
    public int    ContentId   { get; set; }
    public string ContentType { get; set; } = null!; // "Movie" | "Series"
    public bool   IsActive    { get; set; } = true;
    public DateTime AddedAt   { get; set; } = DateTime.Now;

    // Navigation
    public Watchlist Watchlist { get; set; } = null!;
}
