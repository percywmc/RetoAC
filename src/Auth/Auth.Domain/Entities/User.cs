namespace Auth.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Role { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public User(Guid id, string username, string email, string passwordHash, string role)
    {
        Id = id;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public RefreshToken AddRefreshToken(string token, DateTime expiresAtUtc)
    {
        var refreshToken = new RefreshToken(Guid.NewGuid(), Id, token, expiresAtUtc);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public bool HasActiveRefreshToken(string token)
    {
        return _refreshTokens.Any(rt => rt.Token == token && rt.IsActive);
    }

    public void RevokeRefreshToken(string token)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Token == token);
        refreshToken?.Revoke();
    }
}
