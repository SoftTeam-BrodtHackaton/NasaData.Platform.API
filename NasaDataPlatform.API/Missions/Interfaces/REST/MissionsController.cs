using Microsoft.AspNetCore.Mvc;
using NasaData.Platform.API.Missions.Application;
using NasaData.Platform.API.Missions.Domain.Model;
using NasaData.Platform.API.Missions.Interfaces.REST.Resources;
namespace NasaData.Platform.API.Missions.Interfaces.REST;
[ApiController]
[Route("api/[controller]")]
public class MissionsController(MissionService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MissionSummaryResponse>>> List([FromQuery] string? type)
    {
        MissionType? parsed = null;
        if (type != null)
        {
            if (!Enum.TryParse<MissionType>(type, out var value) || !Enum.IsDefined(value) || value.ToString() != type)
                throw new ArgumentException("type inválido.");
            parsed = value;
        }
        return Ok((await service.ListAsync(parsed)).Select(m => new MissionSummaryResponse(m.MissionId, m.Title, m.Type, m.Difficulty, $"{m.Source.Provider} {m.Source.System}")));
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<MissionDetailResponse>> Get(string id) => Ok(MissionDetailResponse.From(await service.GetAsync(id)));
    [HttpPost("generate")]
    public async Task<ActionResult<MissionDetailResponse>> Generate(GenerateMissionRequest request)
    {
        var result = MissionDetailResponse.From(await service.GenerateAsync(request.EventId, request.Type!.Value, request.Difficulty!.Value));
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}
