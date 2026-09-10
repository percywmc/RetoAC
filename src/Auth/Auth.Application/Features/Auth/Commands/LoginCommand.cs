using Auth.Application.Dtos;
using MediatR;

namespace Auth.Application.Features.Auth.Commands;

public record LoginCommand(string Username, string Password) : IRequest<AuthResultDto>;
