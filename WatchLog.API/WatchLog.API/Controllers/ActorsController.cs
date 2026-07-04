using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WatchLog.API.Models;
using WatchLog.API.Models.DTOs;

namespace WatchLog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActorsController : ControllerBase
{
    private readonly IDbConnection _db;
    public ActorsController(IDbConnection db) => _db = db;

    // GET api/actors
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var actors = await _db.QueryAsync<Actor>(
            "sp_GetAllActors", commandType: CommandType.StoredProcedure);
        return Ok(actors);
    }

    // GET api/actors/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var actor = await _db.QueryFirstOrDefaultAsync<Actor>(
            "sp_GetActorById", new { Id = id }, commandType: CommandType.StoredProcedure);
        if (actor == null) return NotFound();
        return Ok(actor);
    }

    // POST api/actors
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActorDto dto)
    {
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_CreateActor",
            new { dto.FullName, dto.BirthDate, dto.Biography, dto.PhotoUrl, dto.Nationality },
            commandType: CommandType.StoredProcedure);

        int newId = (int)result!.NewId;
        return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
    }

    // PUT api/actors/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateActorDto dto)
    {
        dto.Id = id;
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_UpdateActor",
            new { dto.Id, dto.FullName, dto.BirthDate, dto.Biography, dto.PhotoUrl, dto.Nationality },
            commandType: CommandType.StoredProcedure);

        if ((int)result!.AffectedRows == 0) return NotFound();
        return NoContent();
    }

    // DELETE api/actors/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _db.QueryFirstOrDefaultAsync<dynamic>(
            "sp_DeleteActor", new { Id = id }, commandType: CommandType.StoredProcedure);

        if ((int)result!.AffectedRows == 0) return NotFound();
        return NoContent();
    }
}
