namespace CargaMasiva.Infrastructure.Options;

public class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    public string ControlDatabase { get; set; } = default!;
}
