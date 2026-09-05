using NasaDataPlatform.Dominio.Entidades;

namespace NasaDataPlatform.Dominio.Puertos;

public interface IProveedorClimaEspacial
{
    Task<IReadOnlyList<EventoClimaEspacial>> BuscarAsync(
        TipoClima tipo, DateOnly desde, DateOnly hasta, CancellationToken ct);
}
