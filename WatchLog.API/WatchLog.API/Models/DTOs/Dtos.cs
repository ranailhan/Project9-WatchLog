namespace WatchLog.API.Models.DTOs;

// ── Movie DTOs ──────────────────────────────────────────────

public class CreateMovieDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? Duration { get; set; }
    public string? PosterUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string? Director { get; set; }
    public string? Country { get; set; }
    public string? Language { get; set; }
    public List<int> GenreIds { get; set; } = new();
    public List<int> ActorIds { get; set; } = new();
}

public class UpdateMovieDto
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
    public List<int> GenreIds { get; set; } = new();
    public List<int> ActorIds { get; set; } = new();
}

// ── Series DTOs ─────────────────────────────────────────────

public class CreateSeriesDto
{
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
    public List<int> GenreIds { get; set; } = new();
    public List<int> ActorIds { get; set; } = new();
}

public class UpdateSeriesDto
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
    public List<int> GenreIds { get; set; } = new();
    public List<int> ActorIds { get; set; } = new();
}

// ── Actor DTOs ──────────────────────────────────────────────

public class CreateActorDto
{
    public string FullName { get; set; } = null!;
    public DateTime? BirthDate { get; set; }
    public string? Biography { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Nationality { get; set; }
}

public class UpdateActorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime? BirthDate { get; set; }
    public string? Biography { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Nationality { get; set; }
}

// ── Genre DTOs ──────────────────────────────────────────────

public class CreateGenreDto
{
    public string Name { get; set; } = null!;
}

public class UpdateGenreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

// ── Search & Report DTOs ────────────────────────────────────

public class SearchResultDto
{
    public string ContentType { get; set; } = null!;  // Movie, Series, Actor
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? PosterUrl { get; set; }
    public int? Year { get; set; }
}

public class MovieDetailDto
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
    public DateTime CreatedAt { get; set; }
    public List<ActorDto> Actors { get; set; } = new();
    public List<GenreDto> Genres { get; set; } = new();
}

public class SeriesDetailDto
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
    public DateTime CreatedAt { get; set; }
    public List<ActorDto> Actors { get; set; } = new();
    public List<GenreDto> Genres { get; set; } = new();
}

public class ActorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime? BirthDate { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Nationality { get; set; }
}

public class GenreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class MovieReportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int? ReleaseYear { get; set; }
    public int? Duration { get; set; }
    public string? Director { get; set; }
    public string? Country { get; set; }
    public string? Language { get; set; }
    public string? PosterUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Genres { get; set; }
    public string? Actors { get; set; }
}

public class SeriesReportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public int? SeasonCount { get; set; }
    public int? EpisodeCount { get; set; }
    public string? Director { get; set; }
    public string? Country { get; set; }
    public string? Language { get; set; }
    public string? PosterUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Genres { get; set; }
    public string? Actors { get; set; }
}

public class GenreStatsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int MovieCount { get; set; }
    public int SeriesCount { get; set; }
}
