using System.ComponentModel.DataAnnotations;
namespace NasaData.Platform.API.Students.Interfaces.REST.Resources;
public record SubmitAnswerRequest([Required, MaxLength(20)] string OptionId);
public record SubmitAnswerResponse(bool Correct, int PointsEarned, string Feedback);
public record StudentProgressResponse(string StudentId, int CompletedMissions, int TotalPoints, string Level);
public record SkillsResponse(int Astronomy, int Physics, int DataAnalysis, int Programming);
public record StudentProfileResponse(string StudentId, SkillsResponse Skills, IEnumerable<string> RecommendedAreas);
