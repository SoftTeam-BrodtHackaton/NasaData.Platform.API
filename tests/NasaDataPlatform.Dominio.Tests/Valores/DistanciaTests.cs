using FluentAssertions;
using NasaDataPlatform.Dominio.Valores;

namespace NasaDataPlatform.Dominio.Tests.Valores;

public class DistanciaTests
{
    [Fact]
    public void DesdeUnidadesAstronomicas_ConValorValido_LoExpone()
    {
        var distancia = Distancia.DesdeUnidadesAstronomicas(0.0163724048208528);

        distancia.UnidadesAstronomicas.Should().Be(0.0163724048208528);
    }

    [Fact]
    public void DesdeDistanciasLunares_ConvierteAUnidadesAstronomicas()
    {
        // 1 UA = 389.17... LD, así que 389.17 LD debe ser ~1 UA.
        var distancia = Distancia.DesdeDistanciasLunares(389.17);

        distancia.UnidadesAstronomicas.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void DistanciasLunares_EsInversaDeDesdeDistanciasLunares()
    {
        var distancia = Distancia.DesdeDistanciasLunares(3.0);

        distancia.DistanciasLunares.Should().BeApproximately(3.0, 1e-9);
    }

    [Fact]
    public void DesdeUnidadesAstronomicas_Negativo_Lanza()
    {
        var accion = () => Distancia.DesdeUnidadesAstronomicas(-0.01);

        accion.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void DesdeUnidadesAstronomicas_NoFinito_Lanza(double valor)
    {
        var accion = () => Distancia.DesdeUnidadesAstronomicas(valor);

        accion.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DesdeUnidadesAstronomicas_Cero_EsValido()
    {
        var distancia = Distancia.DesdeUnidadesAstronomicas(0);

        distancia.UnidadesAstronomicas.Should().Be(0);
    }
}
