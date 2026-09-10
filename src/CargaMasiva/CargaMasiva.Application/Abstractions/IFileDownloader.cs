namespace CargaMasiva.Application.Abstractions;

public interface IFileDownloader
{
    Task<Stream> DownloadAsync(string rutaArchivo, CancellationToken cancellationToken);
}
