using NasaData.Platform.API.Missions.Domain.Model;
using NasaData.Platform.API.Shared.Domain.Repositories;
namespace NasaData.Platform.API.Missions.Domain.Repositories;
public interface IMissionRepository : IBaseRepository<Mission>
{
    Task<Mission?> FindByMissionIdAsync(string id);
    Task<IEnumerable<Mission>> ListByTypeAsync(MissionType? type);
}
