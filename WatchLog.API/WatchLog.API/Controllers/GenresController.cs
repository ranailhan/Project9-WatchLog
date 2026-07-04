using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WatchLog.API.Models;
using WatchLog.API.Models.DTOs;

namespace WatchLog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController : ControllerBase
{
    private readonly IDbConnection _db;
    public GenresController(IDbConnection db) => _db = db;

    // GET api/genres
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var genres = await _db.QueryAsync<Genre>(
            "sp_GetAllGenres", commandType: CommandType.StoredProcedure);
        return Ok(genres);
    }

    // GET api/genres/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var genre = await _db.QueryFirstOrDefaultAsync<Genre>(
            "sp_GetGenreById", new { Id = id }, commandType: CommandType.StoredProcedure);
        if (genre == null) return NotFound();
        return Ok(genre);
    }

    // GET api/genres/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _db.QueryAsync<GenreStatsDto>(
            "sp_GetGenreStats", commandType: CommandType.StoredProcedure);
        return Ok(stats);
    }

    // POST api/genres
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGenreDto dto)
    {
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_CreateGenre", new { dto.Name }, commandType: CommandType.StoredProcedure);

        int newId = (int)result!.NewId;
        return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
    }

    // PUT api/genres/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGenreDto dto)
    {
        dto.Id = id;
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_UpdateGenre", new { dto.Id, dto.Name }, commandType: CommandType.StoredProcedure);

        if ((int)result!.AffectedRows == 0) return NotFound();
        return NoContent();
    }

    // DELETE api/genres/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_DeleteGenre", new { Id = id }, commandType: CommandType.StoredProcedure);

        if ((int)result!.AffectedRows == 0) return NotFound();
        return NoContent();
    }
}
