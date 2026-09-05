namespace NasaDataPlatform.Infraestructura.Tests.Soporte;

/// <summary>Fixtures capturados en tests/fixtures, copiados a bin/Fixtures por el csproj.</summary>
internal static class RutaFixtures
{
    public static string Carpeta => Path.Combine(AppContext.BaseDirectory, "Fixtures");

    public static string LeerJson(string archivo) => File.ReadAllText(Path.Combine(Carpeta, archivo));
}
