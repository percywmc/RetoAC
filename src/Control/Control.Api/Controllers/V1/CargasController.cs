using Control.Application.Features.Cargas.Commands;
using Control.Application.Features.Cargas.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Control.Api.Controllers.V1;

[ApiController]
[Route("api/v1/cargas")]
[Authorize]
public class CargasController : ControllerBase
{
    private readonly ISender _sender;

    public CargasController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(Roles = "CargaMasiva.Ejecutor")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarCarga(IFormFile archivo, CancellationToken cancellationToken)
    {
        var usuario = User.Identity?.Name ?? User.FindFirst("unique_name")?.Value ?? "desconocido";
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
            ?? User.FindFirst("email")?.Value 
            ?? "desconocido@example.com";

        await using var stream = archivo.OpenReadStream();
        var command = new RegistrarCargaCommand(archivo.FileName, archivo.Length, stream, usuario, email);
        var resultado = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(ObtenerCarga), new { id = resultado.Id }, resultado);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarCargas(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var esAdmin = User.IsInRole("Admin");
        var usuario = esAdmin ? null : (User.Identity?.Name ?? User.FindFirst("unique_name")?.Value);

        var resultado = await _sender.Send(new ListarCargasQuery(usuario, pageNumber, pageSize), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerCarga(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _sender.Send(new ObtenerCargaQuery(id), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}/contenido")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerContenido(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _sender.Send(new ObtenerContenidoQuery(id), cancellationToken);
        return File(resultado.Contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", resultado.NombreArchivo);
    }
}
