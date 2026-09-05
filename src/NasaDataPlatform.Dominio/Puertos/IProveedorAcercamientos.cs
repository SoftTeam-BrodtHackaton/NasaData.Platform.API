using NasaDataPlatform.Dominio.Entidades;

namespace NasaDataPlatform.Dominio.Puertos;

public interface IProveedorAcercamientos
{
    Task<IReadOnlyList<Acercamiento>> BuscarAsync(
        DateOnly desde, DateOnly hasta, double distMaxUa, CancellationToken ct);
}
