using Auth.Application.Abstractions;
using Auth.Application.Dtos;
using Auth.Application.Exceptions;
using Auth.Domain.Repositories;
using MediatR;

namespace Auth.Application.Features.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user is null || !user.IsActive || !user.HasActiveRefreshToken(request.RefreshToken))
        {
            throw new InvalidCredentialsException();
        }

        user.RevokeRefreshToken(request.RefreshToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtTokenGenerator.RefreshTokenExpirationDays);

        var newRefreshToken = user.AddRefreshToken(newRefreshTokenValue, refreshTokenExpiration);
        await _userRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtTokenGenerator.AccessTokenExpirationMinutes);

        return new AuthResultDto(accessToken, newRefreshTokenValue, accessTokenExpiration, user.Username, user.Role);
    }
}
