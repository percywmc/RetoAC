using CargaMasiva.Application.Abstractions;
using CargaMasiva.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CargaMasiva.Infrastructure.ExternalServices;

public class SeaweedFsDownloader : IFileDownloader
{
    private readonly HttpClient _httpClient;

    public SeaweedFsDownloader(HttpClient httpClient, IOptions<SeaweedFsOptions> options)
    {
        _httpClient = httpClient;
    }

    public async Task<Stream> DownloadAsync(string rutaArchivo, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(rutaArchivo, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
