using System.Net;
using NasaDataPlatform.Dominio.Entidades;
using NasaDataPlatform.Dominio.Errores;
using NasaDataPlatform.Dominio.Puertos;

namespace NasaDataPlatform.Infraestructura.Nasa;

/// <summary>
/// Adaptador real de DONKI para CME y FLR (GST queda fuera de alcance). Inyectado
/// como cliente tipado vía IHttpClientFactory: nunca crea su propio HttpClient.
/// </summary>
internal sealed class DonkiAdapter : IProveedorClimaEspacial
{
    private readonly HttpClient _httpClient;
    private readonly NasaConfiguracion _configuracion;

    public DonkiAdapter(HttpClient httpClient, NasaConfiguracion configuracion)
    {
        _httpClient = httpClient;
        _configuracion = configuracion;
    }

    public async Task<IReadOnlyList<EventoClimaEspacial>> BuscarAsync(
        TipoClima tipo, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var endpoint = tipo switch
        {
            TipoClima.Cme => "DONKI/CME",
            TipoClima.Flr => "DONKI/FLR",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de clima espacial no soportado."),
        };

        var url =
            $"{endpoint}?startDate={desde:yyyy-MM-dd}&endDate={hasta:yyyy-MM-dd}" +
            $"&api_key={Uri.EscapeDataString(_configuracion.ApiKey)}";

        var cuerpo = await ObtenerCuerpoAsync(url, endpoint, ct);

        return tipo switch
        {
            TipoClima.Cme => DonkiRespuestaConverter.AnalizarCme(cuerpo),
            TipoClima.Flr => DonkiRespuestaConverter.AnalizarFlr(cuerpo),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };
    }

    private async Task<string> ObtenerCuerpoAsync(string url, string nombreFuente, CancellationToken ct)
    {
        try
        {
            using var respuesta = await _httpClient.GetAsync(url, ct);

            if (respuesta.StatusCode == HttpStatusCode.TooManyRequests || (int)respuesta.StatusCode >= 500)
            {
                throw new FuenteNoDisponibleException(
                    $"{nombreFuente} respondió {(int)respuesta.StatusCode} ({respuesta.StatusCode}).");
            }

            respuesta.EnsureSuccessStatusCode();
            return await respuesta.Content.ReadAsStringAsync(ct);
        }
        catch (HttpRequestException ex)
        {
            throw new FuenteNoDisponibleException($"No se pudo contactar {nombreFuente}.", ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new FuenteNoDisponibleException($"{nombreFuente} no respondió a tiempo.", ex);
        }
    }
}

/// <summary>Clave de NASA usada por los adaptadores DONKI. NeoWs no está en alcance.</summary>
internal sealed record NasaConfiguracion(string ApiKey);
