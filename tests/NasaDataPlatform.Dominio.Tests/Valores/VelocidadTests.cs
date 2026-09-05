using FluentAssertions;
using NasaDataPlatform.Dominio.Valores;

namespace NasaDataPlatform.Dominio.Tests.Valores;

public class VelocidadTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(609.0)]
    [InlineData(1033.0)]
    public void DesdeKilometrosPorSegundo_ConValorValido_LoExpone(double valor)
    {
        var velocidad = Velocidad.DesdeKilometrosPorSegundo(valor);

        velocidad.KilometrosPorSegundo.Should().Be(valor);
    }

    [Fact]
    public void DesdeKilometrosPorSegundo_Negativo_Lanza()
    {
        var accion = () => Velocidad.DesdeKilometrosPorSegundo(-1);

        accion.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void DesdeKilometrosPorSegundo_NoFinito_Lanza(double valor)
    {
        var accion = () => Velocidad.DesdeKilometrosPorSegundo(valor);

        accion.Should().Throw<ArgumentOutOfRangeException>();
    }
}
