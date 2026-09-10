namespace Control.Application.Abstractions;

public interface ISeaweedFsClient
{
    Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken);
    Task<Stream> DownloadAsync(string rutaArchivo, CancellationToken cancellationToken);
}
