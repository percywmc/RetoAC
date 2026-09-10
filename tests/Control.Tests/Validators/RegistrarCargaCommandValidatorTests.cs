using Control.Application.Features.Cargas.Commands;
using Control.Application.Options;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;

namespace Control.Tests.Validators;

public class RegistrarCargaCommandValidatorTests
{
    private readonly RegistrarCargaCommandValidator _validator = new(Options.Create(new CargaOptions { TamanoMaximoMB = 10 }));

    private static RegistrarCargaCommand CrearComando(
        string nombreArchivo = "archivo.xlsx",
        long tamanoBytes = 1024,
        string usuario = "usuario@example.com")
    {
        return new RegistrarCargaCommand(nombreArchivo, tamanoBytes, Stream.Null, usuario);
    }

    [Fact]
    public void Debe_ser_valido_cuando_todos_los_campos_son_correctos()
    {
        var comando = CrearComando();

        var resultado = _validator.TestValidate(comando);

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("archivo.csv")]
    [InlineData("archivo.docx")]
    [InlineData("archivo")]
    public void Debe_fallar_cuando_la_extension_no_es_xlsx(string nombreArchivo)
    {
        var comando = CrearComando(nombreArchivo: nombreArchivo);

        var resultado = _validator.TestValidate(comando);

        resultado.ShouldHaveValidationErrorFor(x => x.NombreArchivo);
    }

    [Fact]
    public void Debe_fallar_cuando_el_archivo_esta_vacio()
    {
        var comando = CrearComando(tamanoBytes: 0);

        var resultado = _validator.TestValidate(comando);

        resultado.ShouldHaveValidationErrorFor(x => x.TamanoBytes);
    }

    [Fact]
    public void Debe_fallar_cuando_el_archivo_excede_el_tamano_maximo()
    {
        var comando = CrearComando(tamanoBytes: 11 * 1024 * 1024);

        var resultado = _validator.TestValidate(comando);

        resultado.ShouldHaveValidationErrorFor(x => x.TamanoBytes);
    }

    [Fact]
    public void Debe_fallar_cuando_el_usuario_esta_vacio()
    {
        var comando = CrearComando(usuario: string.Empty);

        var resultado = _validator.TestValidate(comando);

        resultado.ShouldHaveValidationErrorFor(x => x.Usuario);
    }
}
