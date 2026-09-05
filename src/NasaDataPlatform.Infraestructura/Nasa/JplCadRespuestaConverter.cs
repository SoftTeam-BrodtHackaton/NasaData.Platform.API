using System.Globalization;
using System.Text.Json;
using NasaDataPlatform.Dominio.Entidades;
using NasaDataPlatform.Dominio.Errores;
using NasaDataPlatform.Dominio.Valores;

namespace NasaDataPlatform.Infraestructura.Nasa;

/// <summary>
/// JPL CAD no devuelve un array de objetos: devuelve { fields: [...], data: [[...], ...] },
/// filas de puros strings. Se deserializa a un DTO plano (Fields/Data como arrays de string)
/// y se proyecta por índice de columna en vez de escribir un JsonConverter&lt;T&gt; manual:
/// la lógica interesante aquí es la proyección por nombre de columna y el parseo de cada
/// valor, no el recorrido de tokens JSON de bajo nivel — un converter personalizado solo
/// movería esa misma lógica dentro de Read()/Write() sin ganar nada en claridad.
/// </summary>
internal static class JplCadRespuestaConverter
{
    private static readonly JsonSerializerOptions JsonOpciones = new() { PropertyNameCaseInsensitive = true };

    public static IReadOnlyList<Acercamiento> ConvertirDesdeJson(string json)
    {
        JplCadRespuestaDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<JplCadRespuestaDto>(json, JsonOpciones);
        }
        catch (JsonException ex)
        {
            throw new FuenteNoDisponibleException("JPL CAD devolvió un formato inesperado.", ex);
        }

        return dto is null ? [] : Convertir(dto);
    }

    private static IReadOnlyList<Acercamiento> Convertir(JplCadRespuestaDto respuesta)
    {
        if (respuesta.Fields is not { Length: > 0 } campos || respuesta.Data is not { } filas)
        {
            return [];
        }

        var indice = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < campos.Length; i++)
        {
            indice[campos[i]] = i;
        }

        if (!indice.TryGetValue("des", out var iDes) ||
            !indice.TryGetValue("cd", out var iCd) ||
            !indice.TryGetValue("dist", out var iDist) ||
            !indice.TryGetValue("v_rel", out var iVRel) ||
            !indice.TryGetValue("h", out var iH))
        {
            throw new FuenteNoDisponibleException("JPL CAD no incluyó las columnas esperadas en 'fields'.");
        }

        var acercamientos = new List<Acercamiento>();
        foreach (var fila in filas)
        {
            if (TryConvertirFila(fila, iDes, iCd, iDist, iVRel, iH, out var acercamiento))
            {
                acercamientos.Add(acercamiento);
            }
        }

        return acercamientos;
    }

    private static bool TryConvertirFila(
        string[] fila, int iDes, int iCd, int iDist, int iVRel, int iH, out Acercamiento acercamiento)
    {
        acercamiento = null!;

        var indiceMaximo = Math.Max(iH, Math.Max(iVRel, Math.Max(iDist, Math.Max(iDes, iCd))));
        if (fila.Length <= indiceMaximo)
        {
            return false;
        }

        if (fila[iDes] is not { Length: > 0 } designacion)
        {
            return false;
        }

        if (!DateTime.TryParseExact(
                fila[iCd],
                "yyyy-MMM-dd HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var momento))
        {
            return false;
        }

        if (!double.TryParse(fila[iDist], NumberStyles.Float, CultureInfo.InvariantCulture, out var distUa) ||
            !double.TryParse(fila[iVRel], NumberStyles.Float, CultureInfo.InvariantCulture, out var vRelKmS) ||
            !double.TryParse(fila[iH], NumberStyles.Float, CultureInfo.InvariantCulture, out var magnitud))
        {
            return false;
        }

        Distancia distancia;
        Velocidad velocidad;
        try
        {
            distancia = Distancia.DesdeUnidadesAstronomicas(distUa);
            velocidad = Velocidad.DesdeKilometrosPorSegundo(vRelKmS);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        acercamiento = new Acercamiento
        {
            Designacion = designacion,
            Momento = momento,
            Distancia = distancia,
            VelocidadRelativa = velocidad,
            MagnitudAbsoluta = magnitud,
        };
        return true;
    }

    internal sealed class JplCadRespuestaDto
    {
        public string[]? Fields { get; set; }
        public string[][]? Data { get; set; }
    }
}
