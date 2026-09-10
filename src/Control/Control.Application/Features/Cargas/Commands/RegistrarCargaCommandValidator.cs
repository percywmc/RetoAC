using Control.Application.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Control.Application.Features.Cargas.Commands;

public class RegistrarCargaCommandValidator : AbstractValidator<RegistrarCargaCommand>
{
    private readonly long _tamanoMaximoBytes;
    private static readonly string[] ExtensionesPermitidas = [".xlsx"];

    public RegistrarCargaCommandValidator(IOptions<CargaOptions> options)
    {
        _tamanoMaximoBytes = options.Value.TamanoMaximoMB * 1024L * 1024L;

        RuleFor(x => x.NombreArchivo)
            .NotEmpty().WithMessage("El nombre del archivo es obligatorio.")
            .Must(TenerExtensionValida).WithMessage("Solo se permiten archivos con extensión .xlsx.");

        RuleFor(x => x.TamanoBytes)
            .GreaterThan(0).WithMessage("El archivo está vacío.")
            .LessThanOrEqualTo(_tamanoMaximoBytes).WithMessage($"El archivo excede el tamaño máximo permitido de {options.Value.TamanoMaximoMB} MB.");

        RuleFor(x => x.Usuario)
            .NotEmpty().WithMessage("El usuario es obligatorio.");
    }

    private static bool TenerExtensionValida(string nombreArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo);
        return ExtensionesPermitidas.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }
}
