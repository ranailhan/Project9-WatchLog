using Dapper;
using System.Data;
using WatchLog.API.Models;
using WatchLog.API.Models.DTOs;
using WatchLog.API.Repositories.Interfaces;

namespace WatchLog.API.Repositories.Implementations;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnection _db;
    public MovieRepository(IDbConnection db) => _db = db;

    public async Task<IEnumerable<Movie>> GetAllAsync()
        => await _db.QueryAsync<Movie>("sp_GetAllMovies", commandType: CommandType.StoredProcedure);

    public async Task<MovieDetailDto?> GetByIdAsync(int id)
    {
        var movie = await _db.QueryFirstOrDefaultAsync<Movie>(
            "sp_GetMovieById", new { Id = id }, commandType: CommandType.StoredProcedure);

        if (movie == null) return null;

        var actors = await _db.QueryAsync<ActorDto>(
            "sp_GetMovieActors", new { MovieId = id }, commandType: CommandType.StoredProcedure);

        var genres = await _db.QueryAsync<GenreDto>(
            "sp_GetMovieGenres", new { MovieId = id }, commandType: CommandType.StoredProcedure);

        return new MovieDetailDto
        {
            Id          = movie.Id,
            Title       = movie.Title,
            Description = movie.Description,
            ReleaseYear = movie.ReleaseYear,
            Duration    = movie.Duration,
            PosterUrl   = movie.PosterUrl,
            TrailerUrl  = movie.TrailerUrl,
            Director    = movie.Director,
            Country     = movie.Country,
            Language    = movie.Language,
            CreatedAt   = movie.CreatedAt,
            Actors      = actors.ToList(),
            Genres      = genres.ToList()
        };
    }

    public async Task<int> CreateAsync(CreateMovieDto dto)
    {
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_CreateMovie",
            new
            {
                dto.Title, dto.Description, dto.ReleaseYear, dto.Duration,
                dto.PosterUrl, dto.TrailerUrl, dto.Director, dto.Country, dto.Language
            },
            commandType: CommandType.StoredProcedure);

        int newId = (int)result!.NewId;

        foreach (var gid in dto.GenreIds)
            await _db.ExecuteAsync("sp_AddMovieGenre", new { MovieId = newId, GenreId = gid }, commandType: CommandType.StoredProcedure);

        foreach (var aid in dto.ActorIds)
            await _db.ExecuteAsync("sp_AddMovieActor", new { MovieId = newId, ActorId = aid }, commandType: CommandType.StoredProcedure);

        return newId;
    }

    public async Task<bool> UpdateAsync(UpdateMovieDto dto)
    {
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_UpdateMovie",
            new
            {
                dto.Id, dto.Title, dto.Description, dto.ReleaseYear, dto.Duration,
                dto.PosterUrl, dto.TrailerUrl, dto.Director, dto.Country, dto.Language
            },
            commandType: CommandType.StoredProcedure);

        // Refresh genres
        var existingGenres = await _db.QueryAsync<GenreDto>("sp_GetMovieGenres", new { MovieId = dto.Id }, commandType: CommandType.StoredProcedure);
        foreach (var g in existingGenres)
            await RemoveGenreAsync(dto.Id, g.Id);
        foreach (var gid in dto.GenreIds)
            await AddGenreAsync(dto.Id, gid);

        // Refresh actors
        var existingActors = await _db.QueryAsync<ActorDto>("sp_GetMovieActors", new { MovieId = dto.Id }, commandType: CommandType.StoredProcedure);
        foreach (var a in existingActors)
            await RemoveActorAsync(dto.Id, a.Id);
        foreach (var aid in dto.ActorIds)
            await AddActorAsync(dto.Id, aid);

        return (int)result!.AffectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_DeleteMovie", new { Id = id }, commandType: CommandType.StoredProcedure);
        return (int)result!.AffectedRows > 0;
    }

    public async Task AddActorAsync(int movieId, int actorId)
        => await _db.ExecuteAsync("sp_AddMovieActor", new { MovieId = movieId, ActorId = actorId }, commandType: CommandType.StoredProcedure);

    public async Task RemoveActorAsync(int movieId, int actorId)
        => await _db.ExecuteAsync("sp_RemoveMovieActor", new { MovieId = movieId, ActorId = actorId }, commandType: CommandType.StoredProcedure);

    public async Task AddGenreAsync(int movieId, int genreId)
        => await _db.ExecuteAsync("sp_AddMovieGenre", new { MovieId = movieId, GenreId = genreId }, commandType: CommandType.StoredProcedure);

    public async Task RemoveGenreAsync(int movieId, int genreId)
        => await _db.ExecuteAsync("sp_RemoveMovieGenre", new { MovieId = movieId, GenreId = genreId }, commandType: CommandType.StoredProcedure);

    public async Task<IEnumerable<Movie>> GetByGenreAsync(int genreId)
        => await _db.QueryAsync<Movie>("sp_GetMoviesByGenre", new { GenreId = genreId }, commandType: CommandType.StoredProcedure);

    public async Task<IEnumerable<Movie>> GetByActorAsync(int actorId)
        => await _db.QueryAsync<Movie>("sp_GetMoviesByActor", new { ActorId = actorId }, commandType: CommandType.StoredProcedure);

    public async Task<IEnumerable<MovieReportDto>> GetWithDetailsAsync()
        => await _db.QueryAsync<MovieReportDto>("sp_GetMoviesWithDetails", commandType: CommandType.StoredProcedure);
}
