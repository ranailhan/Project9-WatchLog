using Microsoft.AspNetCore.Identity;

namespace WatchLog.MVC.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName    { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? Bio            { get; set; }
    public bool    IsActive       { get; set; } = true;
    public DateTime CreatedAt     { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Watchlist> Watchlists { get; set; } = new List<Watchlist>();
    public ICollection<Review>    Reviews    { get; set; } = new List<Review>();
    public ICollection<Rating>    Ratings    { get; set; } = new List<Rating>();
    public ICollection<Favorite>  Favorites  { get; set; } = new List<Favorite>();
}
