using System.Text;
using System.Text.Json.Serialization;
using Exploration.Api;
using Exploration.Infrastructure;
using Games.Api;
using Games.Infrastructure;
using Host.Api;
using Identity.Api;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Payment.Api;
using Payment.Infrastructure;
using Scalar.AspNetCore;
using Social.Api;
using Social.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddIdentityControllers()
    .AddExplorationControllers()
    .AddGamesControllers()
    .AddSocialControllers()
    .AddPaymentControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });
builder.Services.AddAuthorization();

builder.Services
    .AddIdentityModule(builder.Configuration)
    .AddExplorationModule(builder.Configuration)
    .AddGamesModule(builder.Configuration)
    .AddSocialModule(builder.Configuration)
    .AddPaymentModule(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
