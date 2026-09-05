using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using NasaData.Platform.API.Missions.Infrastructure.ScientificEvents;
namespace NasaData.Platform.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            ArgumentException => 400,
            KeyNotFoundException => 404,
            DbUpdateConcurrencyException => 409,
            DbUpdateException { InnerException: MySqlException { Number: 1062 } } => 409,
            ScientificDependencyUnavailableException => 503,
            HttpRequestException => 502,
            _ => 500
        };
        if (status >= 500) logger.LogError(exception, "Error procesando {Path}", context.Request.Path);
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = status switch { 409 => "Conflicto de escritura. Reintenta la solicitud.", 500 => "Error interno del servidor.", 502 => "Error de integración externa.", _ => exception.Message },
            Instance = context.Request.Path
        }, cancellationToken);
        return true;
    }
}
