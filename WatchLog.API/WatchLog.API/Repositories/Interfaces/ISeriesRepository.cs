using WatchLog.API.Models;
using WatchLog.API.Models.DTOs;

namespace WatchLog.API.Repositories.Interfaces;

public interface ISeriesRepository
{
    Task<IEnumerable<Series>> GetAllAsync();
    Task<SeriesDetailDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateSeriesDto dto);
    Task<bool> UpdateAsync(UpdateSeriesDto dto);
    Task<bool> DeleteAsync(int id);
    Task AddActorAsync(int seriesId, int actorId);
    Task RemoveActorAsync(int seriesId, int actorId);
    Task AddGenreAsync(int seriesId, int genreId);
    Task RemoveGenreAsync(int seriesId, int genreId);
    Task<IEnumerable<Series>> GetByGenreAsync(int genreId);
    Task<IEnumerable<SeriesReportDto>> GetWithDetailsAsync();
}
