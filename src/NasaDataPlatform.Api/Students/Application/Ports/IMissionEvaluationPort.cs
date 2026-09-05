using NasaData.Platform.API.Students.Domain.Model;
namespace NasaData.Platform.API.Students.Application.Ports;
public sealed record MissionEvaluation(bool Correct, string Feedback, SkillContribution Skills);
public interface IMissionEvaluationPort
{
    Task<MissionEvaluation> EvaluateAsync(string missionId, string optionId);
}
