namespace WatchLog.MVC.Models.ApiModels;

// ── Film ────────────────────────────────────────────────────
public class MovieModel
{
    public int     Id          { get; set; }
    public string  Title       { get; set; } = null!;
    public string? Description { get; set; }
    public int?    ReleaseYear { get; set; }
    public int?    Duration    { get; set; }
    public string? PosterUrl   { get; set; }
    public string? TrailerUrl  { get; set; }
    public string? Director    { get; set; }
    public string? Country     { get; set; }
    public string? Language    { get; set; }
    public string? GenreNames  { get; set; }
    public string? ActorNames  { get; set; }
    public DateTime CreatedAt  { get; set; }
}

public class MovieDetailModel
{
    public int     Id          { get; set; }
    public string  Title       { get; set; } = null!;
    public string? Description { get; set; }
    public int?    ReleaseYear { get; set; }
    public int?    Duration    { get; set; }
    public string? PosterUrl   { get; set; }
    public string? TrailerUrl  { get; set; }
    public string? Director    { get; set; }
    public string? Country     { get; set; }
    public string? Language    { get; set; }
    public DateTime CreatedAt  { get; set; }
    public List<ActorModel>  Actors { get; set; } = new();
    public List<GenreModel>  Genres { get; set; } = new();
    // MVC'de hesaplanan alanlar
    public double? AverageRating  { get; set; }
    public int     RatingCount    { get; set; }
    public bool    IsFavorited    { get; set; }
    public int?    UserRating     { get; set; }
}

// ── Dizi ─────────────────────────────────────────────────────
public class SeriesModel
{
    public int     Id           { get; set; }
    public string  Title        { get; set; } = null!;
    public string? Description  { get; set; }
    public int?    StartYear    { get; set; }
    public int?    EndYear      { get; set; }
    public int?    SeasonCount  { get; set; }
    public int?    EpisodeCount { get; set; }
    public string? PosterUrl    { get; set; }
    public string? TrailerUrl   { get; set; }
    public string? Director     { get; set; }
    public string? Country      { get; set; }
    public string? Language     { get; set; }
    public string? GenreNames   { get; set; }
    public string? ActorNames   { get; set; }
    public DateTime CreatedAt   { get; set; }
}

public class SeriesDetailModel
{
    public int     Id           { get; set; }
    public string  Title        { get; set; } = null!;
    public string? Description  { get; set; }
    public int?    StartYear    { get; set; }
    public int?    EndYear      { get; set; }
    public int?    SeasonCount  { get; set; }
    public int?    EpisodeCount { get; set; }
    public string? PosterUrl    { get; set; }
    public string? TrailerUrl   { get; set; }
    public string? Director     { get; set; }
    public string? Country      { get; set; }
    public string? Language     { get; set; }
    public DateTime CreatedAt   { get; set; }
    public List<ActorModel>  Actors { get; set; } = new();
    public List<GenreModel>  Genres { get; set; } = new();
    public double? AverageRating   { get; set; }
    public int     RatingCount     { get; set; }
    public bool    IsFavorited     { get; set; }
    public int?    UserRating      { get; set; }
}

// ── Ortak ───────────────────────────────────────────────────
public class ActorModel
{
    public int     Id          { get; set; }
    public string  FullName    { get; set; } = null!;
    public string? PhotoUrl    { get; set; }
    public string? Nationality { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Biography   { get; set; }
}

public class GenreModel
{
    public int    Id   { get; set; }
    public string Name { get; set; } = null!;
}

public class SearchResultModel
{
    public string  ContentType { get; set; } = null!;
    public int     Id          { get; set; }
    public string  Name        { get; set; } = null!;
    public string? PosterUrl   { get; set; }
    public int?    Year        { get; set; }
}

public class GenreStatsModel
{
    public int    Id           { get; set; }
    public string Name         { get; set; } = null!;
    public int    MovieCount   { get; set; }
    public int    SeriesCount  { get; set; }
}

public class MovieReportModel
{
    public int     Id          { get; set; }
    public string  Title       { get; set; } = null!;
    public int?    ReleaseYear { get; set; }
    public int?    Duration    { get; set; }
    public string? Director    { get; set; }
    public string? Country     { get; set; }
    public string? Language    { get; set; }
    public string? PosterUrl   { get; set; }
    public DateTime CreatedAt  { get; set; }
    public string? Genres      { get; set; }
    public string? Actors      { get; set; }
}

public class SeriesReportModel
{
    public int     Id           { get; set; }
    public string  Title        { get; set; } = null!;
    public int?    StartYear    { get; set; }
    public int?    EndYear      { get; set; }
    public int?    SeasonCount  { get; set; }
    public int?    EpisodeCount { get; set; }
    public string? Director     { get; set; }
    public string? Country      { get; set; }
    public string? Language     { get; set; }
    public string? PosterUrl    { get; set; }
    public DateTime CreatedAt   { get; set; }
    public string? Genres       { get; set; }
    public string? Actors       { get; set; }
}
