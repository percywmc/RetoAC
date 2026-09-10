using Control.Application.Abstractions;
using Control.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Control.Infrastructure.ExternalServices;

public class SeaweedFsClient : ISeaweedFsClient
{
    private readonly HttpClient _httpClient;
    private readonly SeaweedFsOptions _options;

    public SeaweedFsClient(HttpClient httpClient, IOptions<SeaweedFsOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken)
    {
        using var formData = new MultipartFormDataContent();
        using var streamContent = new StreamContent(content);
        formData.Add(streamContent, "file", fileName);

        var relativePath = $"/cargas/{Guid.NewGuid()}/{fileName}";
        var response = await _httpClient.PostAsync(relativePath, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        return $"{_options.FilerUrl.TrimEnd('/')}{relativePath}";
    }

    public async Task<Stream> DownloadAsync(string rutaArchivo, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(rutaArchivo, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
