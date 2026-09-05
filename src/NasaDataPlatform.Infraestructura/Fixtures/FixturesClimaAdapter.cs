using NasaDataPlatform.Dominio.Entidades;
using NasaDataPlatform.Dominio.Errores;
using NasaDataPlatform.Dominio.Puertos;
using NasaDataPlatform.Infraestructura.Nasa;

namespace NasaDataPlatform.Infraestructura.Fixtures;

/// <summary>
/// Respaldo de demo: lee las mismas respuestas de DONKI capturadas en disco, sin
/// tocar la red, y reutiliza el parseo del adaptador real para garantizar la misma
/// política de campos faltantes/nulos.
/// </summary>
internal sealed class FixturesClimaAdapter : IProveedorClimaEspacial
{
    private readonly string _carpeta;

    public FixturesClimaAdapter(FixturesConfiguracion configuracion)
    {
        _carpeta = configuracion.Carpeta;
    }

    public async Task<IReadOnlyList<EventoClimaEspacial>> BuscarAsync(
        TipoClima tipo, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var archivo = tipo switch
        {
            TipoClima.Cme => "donki-cme.json",
            TipoClima.Flr => "donki-flr.json",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de clima espacial no soportado."),
        };

        var json = await LeerArchivoAsync(archivo, ct);

        var eventos = tipo switch
        {
            TipoClima.Cme => DonkiRespuestaConverter.AnalizarCme(json),
            TipoClima.Flr => DonkiRespuestaConverter.AnalizarFlr(json),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };

        var desdeUtc = desde.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var hastaUtc = hasta.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return eventos.Where(e => e.Momento >= desdeUtc && e.Momento <= hastaUtc).ToList();
    }

    private async Task<string> LeerArchivoAsync(string archivo, CancellationToken ct)
    {
        var ruta = Path.Combine(_carpeta, archivo);
        try
        {
            return await File.ReadAllTextAsync(ruta, ct);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new FuenteNoDisponibleException($"No se pudo leer el fixture '{archivo}' en '{_carpeta}'.", ex);
        }
    }
}
