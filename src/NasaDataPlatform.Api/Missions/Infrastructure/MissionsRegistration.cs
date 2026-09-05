using NasaData.Platform.API.Missions.Application;
using NasaData.Platform.API.Missions.Application.Ports;
using NasaData.Platform.API.Missions.Domain.Repositories;
using NasaData.Platform.API.Missions.Infrastructure.Persistence.EFC;
using NasaData.Platform.API.Missions.Infrastructure.ScientificEvents;
namespace NasaData.Platform.API.Missions.Infrastructure;
public static class MissionsRegistration
{
    public static IServiceCollection AddMissions(this IServiceCollection services) => services
        .AddScoped<IMissionRepository, MissionRepository>()
        .AddScoped<IScientificEventPort, ConfiguredScientificEventAdapter>()
        .AddScoped<MissionService>();
}
