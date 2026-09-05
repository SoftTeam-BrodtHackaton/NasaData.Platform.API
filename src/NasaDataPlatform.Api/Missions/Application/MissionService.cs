using System.Security.Cryptography;
using System.Text;
using NasaData.Platform.API.Missions.Application.Ports;
using NasaData.Platform.API.Missions.Domain.Model;
using NasaData.Platform.API.Missions.Domain.Repositories;
using NasaData.Platform.API.Shared.Domain.Repositories;
namespace NasaData.Platform.API.Missions.Application;
public sealed record MissionEvaluationResult(bool Correct, string Feedback, int Astronomy, int Physics, int DataAnalysis, int Programming);
public class MissionService(IMissionRepository repository, IScientificEventPort events, IUnitOfWork unitOfWork)
{
    public Task<IEnumerable<Mission>> ListAsync(MissionType? type) => repository.ListByTypeAsync(type);
    public async Task<Mission> GetAsync(string id) => await repository.FindByMissionIdAsync(id) ?? throw new KeyNotFoundException("Misión no encontrada.");
    public async Task<MissionEvaluationResult> EvaluateAsync(string id, string optionId)
    {
        var mission = await GetAsync(id);
        if (!mission.Question.Options.Any(x => x.Id == optionId)) throw new ArgumentException("La opción no existe en esta misión.");
        var correct = mission.Question.CorrectOptionId == optionId;
        var weights = mission.SkillWeights;
        return new(correct, correct ? "Correcto. El evento fue identificado correctamente." : "Revisa nuevamente los datos científicos.",
            weights.Astronomy, weights.Physics, weights.DataAnalysis, weights.Programming);
    }
    public async Task<Mission> GenerateAsync(string eventId, MissionType type, Difficulty difficulty)
    {
        if (string.IsNullOrWhiteSpace(eventId) || eventId.Length > 160 || !Enum.IsDefined(type) || !Enum.IsDefined(difficulty))
            throw new ArgumentException("eventId, type o difficulty inválidos.");
        var scientificEvent = await events.FindAsync(eventId, type) ?? throw new KeyNotFoundException("Evento científico no encontrado.");
        // The same event/type/difficulty yields the same mission, preventing duplicate rewards through generation.
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{type}|{difficulty}|{scientificEvent.Source.Provider}|{scientificEvent.Source.System}|{eventId}")));
        var id = $"MISSION-{hash[..24]}";
        var existing = await repository.FindByMissionIdAsync(id);
        if (existing != null) return existing;
        var mission = Mission.Generate(id, type, difficulty, scientificEvent.Data, scientificEvent.Source);
        await repository.AddAsync(mission);
        await unitOfWork.CompleteAsync();
        return mission;
    }
}
