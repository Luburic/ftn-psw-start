using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace Shared.Tests;

/// <summary>
/// Base factory for a module's integration tests. Each test assembly gets its own
/// database, dropped and recreated once per test run; migrations are applied by the
/// module initializers when the host boots. Modules subclass this with an empty class
/// and share one instance across all integration test classes via a collection fixture.
/// </summary>
public abstract class ExplorerApiFactory : WebApplicationFactory<Program>
{
    private readonly NpgsqlConnectionStringBuilder _connection;

    protected ExplorerApiFactory()
    {
        _connection = new NpgsqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("EXPLORER_TEST_DATABASE")
                ?? "Host=localhost;Port=5432;Database=explorer-test;Username=postgres;Password=admin");
        _connection.Database += $"-{ModuleName()}";
        RecreateDatabase();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Database", _connection.ConnectionString);
    }

    /// <summary>
    /// Restores the database to the seeded baseline: truncates every table mapped by
    /// <typeparamref name="TContext"/> and inserts the given entities. Called from the
    /// module's test base class before every test.
    /// </summary>
    public void Reseed<TContext>(IEnumerable<object> entities) where TContext : DbContext
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        var tables = context.Model.GetEntityTypes()
            .Select(entityType => (Schema: entityType.GetSchema() ?? "public", Table: entityType.GetTableName()))
            .Where(table => table.Table is not null)
            .Distinct()
            .Select(table => $"\"{table.Schema}\".\"{table.Table}\"");

        var truncate = $"TRUNCATE TABLE {string.Join(", ", tables)} RESTART IDENTITY CASCADE";
        context.Database.ExecuteSqlRaw(truncate);

        context.AddRange(entities);
        context.SaveChanges();
    }

    /// <summary>
    /// Creates a client whose requests are authenticated as the given user. The token is
    /// signed with the development key, so no Identity registration is needed; feature
    /// modules only ever see the user ID.
    /// </summary>
    public HttpClient CreateClientFor(Guid userId, string role)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateAccessToken(userId, role));
        return client;
    }

    public string CreateAccessToken(Guid userId, string role)
    {
        var jwt = Services.GetRequiredService<IConfiguration>().GetSection("Jwt");

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string ModuleName() =>
        GetType().Assembly.GetName().Name!.Replace(".Tests", "").ToLowerInvariant();

    private void RecreateDatabase()
    {
        var admin = new NpgsqlConnectionStringBuilder(_connection.ConnectionString) { Database = "postgres" };
        using var connection = new NpgsqlConnection(admin.ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS \"{_connection.Database}\" WITH (FORCE)";
        command.ExecuteNonQuery();
        command.CommandText = $"CREATE DATABASE \"{_connection.Database}\"";
        command.ExecuteNonQuery();
    }
}
