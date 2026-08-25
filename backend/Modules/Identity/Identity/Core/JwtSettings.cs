namespace Identity.Core;

public sealed class JwtSettings
{
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public string Key { get; init; } = "";
    public int ExpirationMinutes { get; init; } = 60;
}
