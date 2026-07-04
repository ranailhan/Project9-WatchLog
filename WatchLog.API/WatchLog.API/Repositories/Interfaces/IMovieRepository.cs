using WatchLog.API.Models;
using WatchLog.API.Models.DTOs;

namespace WatchLog.API.Repositories.Interfaces;

public interface IMovieRepository
{
    Task<IEnumerable<Movie>> GetAllAsync();
    Task<MovieDetailDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateMovieDto dto);
    Task<bool> UpdateAsync(UpdateMovieDto dto);
    Task<bool> DeleteAsync(int id);
    Task AddActorAsync(int movieId, int actorId);
    Task RemoveActorAsync(int movieId, int actorId);
    Task AddGenreAsync(int movieId, int genreId);
    Task RemoveGenreAsync(int movieId, int genreId);
    Task<IEnumerable<Movie>> GetByGenreAsync(int genreId);
    Task<IEnumerable<Movie>> GetByActorAsync(int actorId);
    Task<IEnumerable<MovieReportDto>> GetWithDetailsAsync();
}
