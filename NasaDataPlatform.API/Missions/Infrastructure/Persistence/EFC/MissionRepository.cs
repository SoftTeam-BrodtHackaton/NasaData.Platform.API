using Microsoft.EntityFrameworkCore;
using NasaData.Platform.API.Missions.Domain.Model;
using NasaData.Platform.API.Missions.Domain.Repositories;
using NasaData.Platform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using NasaData.Platform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
namespace NasaData.Platform.API.Missions.Infrastructure.Persistence.EFC;
public class MissionRepository(AppDbContext context) : BaseRepository<Mission>(context), IMissionRepository
{
    public Task<Mission?> FindByMissionIdAsync(string id) => Context.Set<Mission>().SingleOrDefaultAsync(x => x.MissionId == id);
    public async Task<IEnumerable<Mission>> ListByTypeAsync(MissionType? type) => await Context.Set<Mission>().AsNoTracking()
        .Where(x => type == null || x.Type == type).OrderBy(x => x.Id).ToListAsync();
}
