using System.ComponentModel.DataAnnotations;

namespace WatchLog.MVC.Models.Entities;

public class Review
{
    public int    Id          { get; set; }
    public string UserId      { get; set; } = null!;
    public int    ContentId   { get; set; }
    public string ContentType { get; set; } = null!; // "Movie" | "Series"
    [Required, MaxLength(2000)]
    public string Text        { get; set; } = null!;
    public bool   IsActive    { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ApplicationUser User { get; set; } = null!;
}

public class Rating
{
    public int    Id          { get; set; }
    public string UserId      { get; set; } = null!;
    public int    ContentId   { get; set; }
    public string ContentType { get; set; } = null!; // "Movie" | "Series"
    [Range(1, 10)]
    public int    Score       { get; set; }           // 1-10
    public bool   IsActive    { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ApplicationUser User { get; set; } = null!;
}

public class Favorite
{
    public int    Id          { get; set; }
    public string UserId      { get; set; } = null!;
    public int    ContentId   { get; set; }
    public string ContentType { get; set; } = null!; // "Movie" | "Series"
    public bool   IsActive    { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ApplicationUser User { get; set; } = null!;
}
