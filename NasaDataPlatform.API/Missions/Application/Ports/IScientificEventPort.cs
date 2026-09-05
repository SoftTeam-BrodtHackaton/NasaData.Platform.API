using NasaData.Platform.API.Missions.Domain.Model;
namespace NasaData.Platform.API.Missions.Application.Ports;
public sealed record ScientificEvent(ScientificData Data, MissionSource Source);
public interface IScientificEventPort
{
    Task<ScientificEvent?> FindAsync(string eventId, MissionType type);
}
