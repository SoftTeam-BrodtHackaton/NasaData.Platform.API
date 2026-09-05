namespace NasaDataPlatform.Dominio.Errores;

/// <summary>
/// Una fuente de datos externa (DONKI, JPL CAD) no respondió o respondió con error.
/// Los adaptadores traducen cualquier falla de red, HTTP o de parseo a esta
/// excepción: el dominio no debe conocer HttpRequestException ni JsonException.
/// </summary>
public sealed class FuenteNoDisponibleException : Exception
{
    public FuenteNoDisponibleException(string mensaje)
        : base(mensaje)
    {
    }

    public FuenteNoDisponibleException(string mensaje, Exception innerException)
        : base(mensaje, innerException)
    {
    }
}
