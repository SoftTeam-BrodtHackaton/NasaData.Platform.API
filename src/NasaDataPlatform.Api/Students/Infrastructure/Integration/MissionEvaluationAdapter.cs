using NasaData.Platform.API.Missions.Application;
using NasaData.Platform.API.Students.Application.Ports;
namespace NasaData.Platform.API.Students.Infrastructure.Integration;
public class MissionEvaluationAdapter(MissionService missions) : IMissionEvaluationPort
{
    public async Task<MissionEvaluation> EvaluateAsync(string missionId, string optionId)
    {
        var result = await missions.EvaluateAsync(missionId, optionId);
        return new(result.Correct, result.Feedback,
            new(result.Astronomy, result.Physics, result.DataAnalysis, result.Programming));
    }
}
