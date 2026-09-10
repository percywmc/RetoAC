using Auth.Application.Dtos;
using MediatR;

namespace Auth.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDto>;
