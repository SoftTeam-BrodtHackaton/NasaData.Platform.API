using System.Globalization;
using System.Net;
using NasaDataPlatform.Dominio.Entidades;
using NasaDataPlatform.Dominio.Errores;
using NasaDataPlatform.Dominio.Puertos;

namespace NasaDataPlatform.Infraestructura.Nasa;

/// <summary>
/// Adaptador real de JPL Close-Approach Data. No consume la clave de NASA.
/// </summary>
internal sealed class JplCadAdapter : IProveedorAcercamientos
{
    private readonly HttpClient _httpClient;

    public JplCadAdapter(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Acercamiento>> BuscarAsync(
        DateOnly desde, DateOnly hasta, double distMaxUa, CancellationToken ct)
    {
        var url =
            $"cad.api?date-min={desde:yyyy-MM-dd}&date-max={hasta:yyyy-MM-dd}" +
            $"&dist-max={distMaxUa.ToString(CultureInfo.InvariantCulture)}";

        string cuerpo;
        try
        {
            using var respuesta = await _httpClient.GetAsync(url, ct);

            if (respuesta.StatusCode == HttpStatusCode.TooManyRequests || (int)respuesta.StatusCode >= 500)
            {
                throw new FuenteNoDisponibleException(
                    $"JPL CAD respondió {(int)respuesta.StatusCode} ({respuesta.StatusCode}).");
            }

            respuesta.EnsureSuccessStatusCode();
            cuerpo = await respuesta.Content.ReadAsStringAsync(ct);
        }
        catch (HttpRequestException ex)
        {
            throw new FuenteNoDisponibleException("No se pudo contactar JPL CAD.", ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new FuenteNoDisponibleException("JPL CAD no respondió a tiempo.", ex);
        }

        return JplCadRespuestaConverter.ConvertirDesdeJson(cuerpo);
    }
}
