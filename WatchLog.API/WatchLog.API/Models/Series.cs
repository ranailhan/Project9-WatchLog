namespace WatchLog.API.Models;

public class Series
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public int? SeasonCount { get; set; }
    public int? EpisodeCount { get; set; }
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
