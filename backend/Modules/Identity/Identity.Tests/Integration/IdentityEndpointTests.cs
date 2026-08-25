using System.IdentityModel.Tokens.Jwt;
using Identity.Api;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Xunit;

namespace Identity.Tests.Integration;

public sealed class IdentityEndpointTests(IdentityApiFactory factory) : IClassFixture<IdentityApiFactory>
{
    private const string Password = "SuperSecret1!";

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_returns_a_token_for_a_new_user()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/register", new { email = NewEmail(), password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        var token = new JwtSecurityTokenHandler().ReadJwtToken(body!.AccessToken);
        token.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Sub);
        token.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == "explorer");
    }

    [Fact]
    public async Task Register_rejects_a_duplicate_email()
    {
        var email = NewEmail();
        await _client.PostAsJsonAsync("/api/identity/register", new { email, password = Password });

        var response = await _client.PostAsJsonAsync("/api/identity/register", new { email, password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_returns_a_token_for_valid_credentials()
    {
        var email = NewEmail();
        await _client.PostAsJsonAsync("/api/identity/register", new { email, password = Password });

        var response = await _client.PostAsJsonAsync("/api/identity/login", new { email, password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_rejects_a_wrong_password()
    {
        var email = NewEmail();
        await _client.PostAsJsonAsync("/api/identity/register", new { email, password = Password });

        var response = await _client.PostAsJsonAsync("/api/identity/login", new { email, password = "WrongPassword1!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static string NewEmail() => $"student-{Guid.NewGuid():N}@test.com";
}
