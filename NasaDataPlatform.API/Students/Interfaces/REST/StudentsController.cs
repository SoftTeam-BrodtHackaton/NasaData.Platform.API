using Microsoft.AspNetCore.Mvc;
using NasaData.Platform.API.Students.Application;
using NasaData.Platform.API.Students.Interfaces.REST.Resources;
namespace NasaData.Platform.API.Students.Interfaces.REST;
[ApiController]
[Route("api/[controller]/{studentId}")]
public class StudentsController(StudentService service) : ControllerBase
{
    [HttpPost("missions/{missionId}/answer")]
    public async Task<ActionResult<SubmitAnswerResponse>> Answer(string studentId, string missionId, SubmitAnswerRequest request)
    {
        var result = await service.AnswerAsync(studentId, missionId, request.OptionId);
        return Ok(new SubmitAnswerResponse(result.Correct, result.PointsEarned, result.Feedback));
    }
    [HttpGet("progress")]
    public async Task<ActionResult<StudentProgressResponse>> Progress(string studentId)
    {
        var progress = (await service.GetAsync(studentId)).GetProgress();
        return Ok(new StudentProgressResponse(studentId, progress.CompletedMissions, progress.TotalPoints, progress.Level));
    }
    [HttpGet("profile")]
    public async Task<ActionResult<StudentProfileResponse>> Profile(string studentId)
    {
        var student = await service.GetAsync(studentId);
        var profile = student.GetProfile();
        return Ok(new StudentProfileResponse(studentId, new(profile.Astronomy, profile.Physics, profile.DataAnalysis, profile.Programming), student.RecommendedAreas()));
    }
}
