using System.Net;
using System.Text;

namespace NasaDataPlatform.Infraestructura.Tests.Soporte;

/// <summary>Sirve una respuesta fija sin tocar la red. Registra la última solicitud para inspección.</summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string? _contenido;

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string? contenido = null)
    {
        _statusCode = statusCode;
        _contenido = contenido;
    }

    public HttpRequestMessage? UltimaSolicitud { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        UltimaSolicitud = request;

        var respuesta = new HttpResponseMessage(_statusCode)
        {
            Content = _contenido is null ? null : new StringContent(_contenido, Encoding.UTF8, "application/json"),
        };

        return Task.FromResult(respuesta);
    }

    public static HttpClient CrearCliente(HttpStatusCode statusCode, string? contenido, Uri baseAddress) =>
        new(new FakeHttpMessageHandler(statusCode, contenido)) { BaseAddress = baseAddress };
}
