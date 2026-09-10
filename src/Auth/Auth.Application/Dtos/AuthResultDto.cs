namespace Auth.Application.Dtos;

public record AuthResultDto(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc, string Username, string Role);
