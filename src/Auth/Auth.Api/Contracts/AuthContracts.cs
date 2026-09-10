namespace Auth.Api.Contracts;

public record LoginRequest(string Username, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc, string Username, string Role);
