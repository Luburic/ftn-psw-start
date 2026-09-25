using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Identity.Core.DTOs;
using Identity.Tests.Integration.Seeds;
using Xunit;

namespace Identity.Tests.Integration;

public sealed class IdentityEndpointTests : BaseIntegrationTest
{
    public IdentityEndpointTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task New_user_registers_as_an_explorer()
    {
        var client = Factory.CreateClient();
        var request = new RegisterDto("new-explorer@test.com", "BrandNewSecret1!");

        var response = await client.PostAsJsonAsync("/api/identity/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccessTokenDto>();
        var token = new JwtSecurityTokenHandler().ReadJwtToken(body!.AccessToken);
        token.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Sub);
        token.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == "explorer");
    }

    [Fact]
    public async Task Email_can_be_registered_only_once()
    {
        var client = Factory.CreateClient();
        var request = new RegisterDto(UserSeed.Explorer.Email!, "BrandNewSecret1!");

        var response = await client.PostAsJsonAsync("/api/identity/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task User_logs_in_with_valid_credentials()
    {
        var client = Factory.CreateClient();
        var request = new LoginDto(UserSeed.Explorer.Email!, UserSeed.Password);

        var response = await client.PostAsJsonAsync("/api/identity/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccessTokenDto>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task User_cannot_log_in_with_a_wrong_password()
    {
        var client = Factory.CreateClient();
        var request = new LoginDto(UserSeed.Explorer.Email!, "WrongPassword1!");

        var response = await client.PostAsJsonAsync("/api/identity/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
