using WatchLog.API.Models;
using WatchLog.API.Models.DTOs;

namespace WatchLog.API.Repositories.Interfaces;

public interface IGenreRepository
{
    Task<IEnumerable<Genre>> GetAllAsync();
    Task<Genre?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateGenreDto dto);
    Task<bool> UpdateAsync(UpdateGenreDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<GenreStatsDto>> GetStatsAsync();
}
