using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationModuleInitializer : IHostedService
{
    private static readonly Guid DemoAuthor = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public ExplorationModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ExplorationDbContext>();
        await dbContext.Database.MigrateAsync();

        if (_environment.IsDevelopment() && !await dbContext.Tours.AnyAsync())
        {
            dbContext.Tours.AddRange(CreateDemoTours());
            await dbContext.SaveChangesAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<Tour> CreateDemoTours()
    {
        var fortress = new Tour(DemoAuthor, "Šetnja Petrovaradinskom tvrđavom",
            "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav i stari deo Novog Sada.",
            TourDifficulty.Easy, ["istorija", "priroda"]);
        fortress.AddTransportTime(TransportMode.Walking, 120);
        fortress.Publish();

        var fruskaGora = new Tour(DemoAuthor, "Biciklom preko Fruške gore",
            "Ruta vodi od Sremske Kamenice preko Popovice do Iriškog venca, pored manastira Beočin i vidikovaca sa kojih se vidi ceo Srem, uz nekoliko dužih uspona.",
            TourDifficulty.Hard, ["biciklizam", "planina"]);
        fruskaGora.AddTransportTime(TransportMode.Bicycle, 240);
        fruskaGora.Publish();

        var cityCenter = new Tour(DemoAuthor, "Obilazak centra grada",
            "Kratak obilazak Trga slobode, Dunavske ulice i Dunavskog parka.",
            TourDifficulty.Moderate, ["grad"]);

        return [fortress, fruskaGora, cityCenter];
    }
}
