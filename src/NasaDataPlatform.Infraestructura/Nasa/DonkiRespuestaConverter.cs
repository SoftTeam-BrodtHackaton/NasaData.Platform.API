using System.Globalization;
using System.Text.Json;
using NasaDataPlatform.Dominio.Entidades;
using NasaDataPlatform.Dominio.Errores;
using NasaDataPlatform.Dominio.Valores;

namespace NasaDataPlatform.Infraestructura.Nasa;

/// <summary>
/// DTOs y parseo de las respuestas de DONKI (CME, FLR). Separado del adaptador HTTP
/// para que el adaptador de fixtures aplique exactamente la misma política de campos
/// faltantes/nulos sin duplicar la lógica: un campo requerido ausente o inválido
/// descarta ese evento; un campo opcional ausente queda en null.
/// </summary>
internal static class DonkiRespuestaConverter
{
    private static readonly JsonSerializerOptions JsonOpciones = new() { PropertyNameCaseInsensitive = true };

    private static readonly string[] FormatosFechaDonki =
    [
        "yyyy-MM-dd'T'HH:mm'Z'",
        "yyyy-MM-dd'T'HH:mm:ss'Z'",
    ];

    public static IReadOnlyList<EventoClimaEspacial> AnalizarCme(string json)
    {
        var dtos = Deserializar<List<DonkiCmeDto>>(json, "DONKI CME");

        var eventos = new List<EventoClimaEspacial>();
        foreach (var dto in dtos ?? [])
        {
            if (dto.ActivityID is not { Length: > 0 } id) continue;
            if (dto.Link is not { Length: > 0 } enlace) continue;
            if (!TryParseFecha(dto.StartTime, out var momento)) continue;

            var analisis = dto.CmeAnalyses?.FirstOrDefault(a => a.IsMostAccurate == true)
                           ?? dto.CmeAnalyses?.FirstOrDefault();
            if (analisis?.Speed is not { } velocidadKmS) continue;

            Velocidad velocidad;
            try
            {
                velocidad = Velocidad.DesdeKilometrosPorSegundo(velocidadKmS);
            }
            catch (ArgumentOutOfRangeException)
            {
                continue;
            }

            eventos.Add(new EventoClimaEspacial.Cme
            {
                Id = id,
                Tipo = TipoClima.Cme,
                Momento = momento,
                Enlace = enlace,
                Velocidad = velocidad,
                RegionOrigen = string.IsNullOrWhiteSpace(dto.SourceLocation) ? null : dto.SourceLocation,
            });
        }

        return eventos;
    }

    public static IReadOnlyList<EventoClimaEspacial> AnalizarFlr(string json)
    {
        var dtos = Deserializar<List<DonkiFlrDto>>(json, "DONKI FLR");

        var eventos = new List<EventoClimaEspacial>();
        foreach (var dto in dtos ?? [])
        {
            if (dto.FlrID is not { Length: > 0 } id) continue;
            if (dto.Link is not { Length: > 0 } enlace) continue;
            if (dto.ClassType is not { Length: > 0 } clase) continue;
            if (!TryParseFecha(dto.PeakTime, out var momento)) continue;

            eventos.Add(new EventoClimaEspacial.Flr
            {
                Id = id,
                Tipo = TipoClima.Flr,
                Momento = momento,
                Enlace = enlace,
                ClaseDestello = clase,
                RegionActiva = dto.ActiveRegionNum,
            });
        }

        return eventos;
    }

    private static T? Deserializar<T>(string json, string fuente)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOpciones);
        }
        catch (JsonException ex)
        {
            throw new FuenteNoDisponibleException($"{fuente} devolvió un formato inesperado.", ex);
        }
    }

    private static bool TryParseFecha(string? valor, out DateTime momento)
    {
        if (valor is { Length: > 0 } &&
            DateTime.TryParseExact(
                valor,
                FormatosFechaDonki,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out momento))
        {
            return true;
        }

        momento = default;
        return false;
    }

    internal sealed class DonkiCmeDto
    {
        public string? ActivityID { get; set; }
        public string? StartTime { get; set; }
        public string? SourceLocation { get; set; }
        public string? Link { get; set; }
        public List<DonkiCmeAnalysisDto>? CmeAnalyses { get; set; }
    }

    internal sealed class DonkiCmeAnalysisDto
    {
        public bool? IsMostAccurate { get; set; }
        public double? Speed { get; set; }
    }

    internal sealed class DonkiFlrDto
    {
        public string? FlrID { get; set; }
        public string? PeakTime { get; set; }
        public string? ClassType { get; set; }
        public string? Link { get; set; }
        public int? ActiveRegionNum { get; set; }
    }
}
