using NasaData.Platform.API.Missions.Application.Ports;
using NasaData.Platform.API.Missions.Domain.Model;
namespace NasaData.Platform.API.Missions.Infrastructure.ScientificEvents;

// Local normalized input adapter. Replace this registration when the team's extraction is available.
public class ConfiguredScientificEventAdapter(IConfiguration configuration, IHostEnvironment environment) : IScientificEventPort
{
    public Task<ScientificEvent?> FindAsync(string eventId, MissionType type)
    {
        if (!environment.IsDevelopment() || !configuration.GetValue<bool>("Missions:EnableFixtures"))
            throw new ScientificDependencyUnavailableException("La integración científica aún no está configurada.");
        var item = configuration.GetSection("Missions:Fixtures").GetChildren()
            .FirstOrDefault(x => x["EventId"] == eventId && x["Type"] == type.ToString());
        return Task.FromResult(item == null ? null : new ScientificEvent(
            new ScientificData(item["ClassType"], item["Date"] ?? "", item["ObjectName"]),
            new MissionSource("NASA Data Platform", "EDUCATIONAL_FIXTURE", false, eventId)));
    }
}
public sealed class ScientificDependencyUnavailableException(string message) : Exception(message);
