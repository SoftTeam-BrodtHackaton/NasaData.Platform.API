using NasaDataPlatform.Dominio.Entidades;
using NasaDataPlatform.Dominio.Errores;
using NasaDataPlatform.Dominio.Puertos;
using NasaDataPlatform.Infraestructura.Nasa;

namespace NasaDataPlatform.Infraestructura.Fixtures;

/// <summary>
/// Respaldo de demo: lee la respuesta de JPL CAD capturada en disco, sin tocar la
/// red, y reutiliza el mismo conversor que el adaptador real.
/// </summary>
internal sealed class FixturesAcercamientosAdapter : IProveedorAcercamientos
{
    private const string Archivo = "jpl-cad.json";

    private readonly string _carpeta;

    public FixturesAcercamientosAdapter(FixturesConfiguracion configuracion)
    {
        _carpeta = configuracion.Carpeta;
    }

    public async Task<IReadOnlyList<Acercamiento>> BuscarAsync(
        DateOnly desde, DateOnly hasta, double distMaxUa, CancellationToken ct)
    {
        var ruta = Path.Combine(_carpeta, Archivo);

        string json;
        try
        {
            json = await File.ReadAllTextAsync(ruta, ct);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new FuenteNoDisponibleException($"No se pudo leer el fixture '{Archivo}' en '{_carpeta}'.", ex);
        }

        var acercamientos = JplCadRespuestaConverter.ConvertirDesdeJson(json);

        var desdeUtc = desde.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var hastaUtc = hasta.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return acercamientos
            .Where(a => a.Momento >= desdeUtc && a.Momento <= hastaUtc && a.Distancia.UnidadesAstronomicas <= distMaxUa)
            .ToList();
    }
}

/// <summary>Carpeta donde viven los JSON de respaldo de demo.</summary>
internal sealed record FixturesConfiguracion(string Carpeta);
