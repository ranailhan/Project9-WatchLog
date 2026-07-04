using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WatchLog.API.Models;
using WatchLog.API.Models.DTOs;

namespace WatchLog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeriesController : ControllerBase
{
    private readonly IDbConnection _db;
    public SeriesController(IDbConnection db) => _db = db;

    // GET api/series
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var series = await _db.QueryAsync<Series>(
            "sp_GetAllSeries", commandType: CommandType.StoredProcedure);
        return Ok(series);
    }

    // GET api/series/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var series = await _db.QueryFirstOrDefaultAsync<Series>(
            "sp_GetSeriesById", new { Id = id }, commandType: CommandType.StoredProcedure);

        if (series == null) return NotFound();

        var actors = await _db.QueryAsync<ActorDto>(
            "sp_GetSeriesActors", new { SeriesId = id }, commandType: CommandType.StoredProcedure);

        var genres = await _db.QueryAsync<GenreDto>(
            "sp_GetSeriesGenres", new { SeriesId = id }, commandType: CommandType.StoredProcedure);

        var detail = new SeriesDetailDto
        {
            Id           = series.Id,
            Title        = series.Title,
            Description  = series.Description,
            StartYear    = series.StartYear,
            EndYear      = series.EndYear,
            SeasonCount  = series.SeasonCount,
            EpisodeCount = series.EpisodeCount,
            PosterUrl    = series.PosterUrl,
            TrailerUrl   = series.TrailerUrl,
            Director     = series.Director,
            Country      = series.Country,
            Language     = series.Language,
            CreatedAt    = series.CreatedAt,
            Actors       = actors.ToList(),
            Genres       = genres.ToList()
        };

        return Ok(detail);
    }

    // POST api/series
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSeriesDto dto)
    {
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_CreateSeries",
            new
            {
                dto.Title, dto.Description, dto.StartYear, dto.EndYear,
                dto.SeasonCount, dto.EpisodeCount, dto.PosterUrl, dto.TrailerUrl,
                dto.Director, dto.Country, dto.Language
            },
            commandType: CommandType.StoredProcedure);

        int newId = (int)result!.NewId;

        foreach (var gid in dto.GenreIds)
            await _db.ExecuteAsync("sp_AddSeriesGenre",
                new { SeriesId = newId, GenreId = gid }, commandType: CommandType.StoredProcedure);

        foreach (var aid in dto.ActorIds)
            await _db.ExecuteAsync("sp_AddSeriesActor",
                new { SeriesId = newId, ActorId = aid }, commandType: CommandType.StoredProcedure);

        return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
    }

    // PUT api/series/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSeriesDto dto)
    {
        dto.Id = id;
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_UpdateSeries",
            new
            {
                dto.Id, dto.Title, dto.Description, dto.StartYear, dto.EndYear,
                dto.SeasonCount, dto.EpisodeCount, dto.PosterUrl, dto.TrailerUrl,
                dto.Director, dto.Country, dto.Language
            },
            commandType: CommandType.StoredProcedure);

        if ((int)result!.AffectedRows == 0) return NotFound();

        // Türleri güncelle
        var existingGenres = await _db.QueryAsync<GenreDto>(
            "sp_GetSeriesGenres", new { SeriesId = id }, commandType: CommandType.StoredProcedure);
        foreach (var g in existingGenres)
            await _db.ExecuteAsync("sp_RemoveSeriesGenre",
                new { SeriesId = id, GenreId = g.Id }, commandType: CommandType.StoredProcedure);
        foreach (var gid in dto.GenreIds)
            await _db.ExecuteAsync("sp_AddSeriesGenre",
                new { SeriesId = id, GenreId = gid }, commandType: CommandType.StoredProcedure);

        // Oyuncuları güncelle
        var existingActors = await _db.QueryAsync<ActorDto>(
            "sp_GetSeriesActors", new { SeriesId = id }, commandType: CommandType.StoredProcedure);
        foreach (var a in existingActors)
            await _db.ExecuteAsync("sp_RemoveSeriesActor",
                new { SeriesId = id, ActorId = a.Id }, commandType: CommandType.StoredProcedure);
        foreach (var aid in dto.ActorIds)
            await _db.ExecuteAsync("sp_AddSeriesActor",
                new { SeriesId = id, ActorId = aid }, commandType: CommandType.StoredProcedure);

        return NoContent();
    }

    // DELETE api/series/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_DeleteSeries", new { Id = id }, commandType: CommandType.StoredProcedure);

        if ((int)result!.AffectedRows == 0) return NotFound();
        return NoContent();
    }

    // GET api/series/by-genre/3
    [HttpGet("by-genre/{genreId}")]
    public async Task<IActionResult> GetByGenre(int genreId)
    {
        var series = await _db.QueryAsync<Series>(
            "sp_GetSeriesByGenre", new { GenreId = genreId }, commandType: CommandType.StoredProcedure);
        return Ok(series);
    }
}
