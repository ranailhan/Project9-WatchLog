namespace WatchLog.API.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? Duration { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string? Director { get; set; }
    public string? Country { get; set; }
    public string? Language { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    // Computed from JOINs
    public string? GenreNames { get; set; }
    public string? ActorNames { get; set; }
}
