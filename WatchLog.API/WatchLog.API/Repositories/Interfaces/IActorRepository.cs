using WatchLog.API.Models;
using WatchLog.API.Models.DTOs;

namespace WatchLog.API.Repositories.Interfaces;

public interface IActorRepository
{
    Task<IEnumerable<Actor>> GetAllAsync();
    Task<Actor?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateActorDto dto);
    Task<bool> UpdateAsync(UpdateActorDto dto);
    Task<bool> DeleteAsync(int id);
}
