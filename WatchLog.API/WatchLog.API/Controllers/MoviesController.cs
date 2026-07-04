using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WatchLog.API.Models;
using WatchLog.API.Models.DTOs;

namespace WatchLog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IDbConnection _db;
    public MoviesController(IDbConnection db) => _db = db;

    // GET api/movies
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var movies = await _db.QueryAsync<Movie>(
            "sp_GetAllMovies", commandType: CommandType.StoredProcedure);
        return Ok(movies);
    }

    // GET api/movies/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var movie = await _db.QueryFirstOrDefaultAsync<Movie>(
            "sp_GetMovieById", new { Id = id }, commandType: CommandType.StoredProcedure);

        if (movie == null) return NotFound();

        var actors = await _db.QueryAsync<ActorDto>(
            "sp_GetMovieActors", new { MovieId = id }, commandType: CommandType.StoredProcedure);

        var genres = await _db.QueryAsync<GenreDto>(
            "sp_GetMovieGenres", new { MovieId = id }, commandType: CommandType.StoredProcedure);

        var detail = new MovieDetailDto
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

        return Ok(detail);
    }

    // POST api/movies
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMovieDto dto)
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
            await _db.ExecuteAsync("sp_AddMovieGenre",
                new { MovieId = newId, GenreId = gid }, commandType: CommandType.StoredProcedure);

        foreach (var aid in dto.ActorIds)
            await _db.ExecuteAsync("sp_AddMovieActor",
                new { MovieId = newId, ActorId = aid }, commandType: CommandType.StoredProcedure);

        return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
    }

    // PUT api/movies/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMovieDto dto)
    {
        dto.Id = id;
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_UpdateMovie",
            new
            {
                dto.Id, dto.Title, dto.Description, dto.ReleaseYear, dto.Duration,
                dto.PosterUrl, dto.TrailerUrl, dto.Director, dto.Country, dto.Language
            },
            commandType: CommandType.StoredProcedure);

        if ((int)result!.AffectedRows == 0) return NotFound();

        // Türleri güncelle
        var existingGenres = await _db.QueryAsync<GenreDto>(
            "sp_GetMovieGenres", new { MovieId = id }, commandType: CommandType.StoredProcedure);
        foreach (var g in existingGenres)
            await _db.ExecuteAsync("sp_RemoveMovieGenre",
                new { MovieId = id, GenreId = g.Id }, commandType: CommandType.StoredProcedure);
        foreach (var gid in dto.GenreIds)
            await _db.ExecuteAsync("sp_AddMovieGenre",
                new { MovieId = id, GenreId = gid }, commandType: CommandType.StoredProcedure);

        // Oyuncuları güncelle
        var existingActors = await _db.QueryAsync<ActorDto>(
            "sp_GetMovieActors", new { MovieId = id }, commandType: CommandType.StoredProcedure);
        foreach (var a in existingActors)
            await _db.ExecuteAsync("sp_RemoveMovieActor",
                new { MovieId = id, ActorId = a.Id }, commandType: CommandType.StoredProcedure);
        foreach (var aid in dto.ActorIds)
            await _db.ExecuteAsync("sp_AddMovieActor",
                new { MovieId = id, ActorId = aid }, commandType: CommandType.StoredProcedure);

        return NoContent();
    }

    // DELETE api/movies/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_DeleteMovie", new { Id = id }, commandType: CommandType.StoredProcedure);

        if ((int)result!.AffectedRows == 0) return NotFound();
        return NoContent();
    }

    // GET api/movies/by-genre/3
    [HttpGet("by-genre/{genreId}")]
    public async Task<IActionResult> GetByGenre(int genreId)
    {
        var movies = await _db.QueryAsync<Movie>(
            "sp_GetMoviesByGenre", new { GenreId = genreId }, commandType: CommandType.StoredProcedure);
        return Ok(movies);
    }

    // GET api/movies/by-actor/2
    [HttpGet("by-actor/{actorId}")]
    public async Task<IActionResult> GetByActor(int actorId)
    {
        var movies = await _db.QueryAsync<Movie>(
            "sp_GetMoviesByActor", new { ActorId = actorId }, commandType: CommandType.StoredProcedure);
        return Ok(movies);
    }
}
