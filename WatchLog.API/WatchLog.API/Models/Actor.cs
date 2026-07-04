namespace WatchLog.API.Models;

public class Actor
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime? BirthDate { get; set; }
    public string? Biography { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Nationality { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
