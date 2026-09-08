using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Social.Domain.Blogs;

namespace Social.Infrastructure.Persistence;

internal sealed class SocialModuleInitializer : IHostedService
{
    private static readonly Guid FirstExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid SecondExplorer = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid ThirdExplorer = Guid.Parse("20000000-0000-0000-0000-000000000003");

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public SocialModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<SocialDbContext>();
        await dbContext.Database.MigrateAsync();

        if (_environment.IsDevelopment() && !await dbContext.Blogs.AnyAsync())
        {
            dbContext.Blogs.AddRange(CreateDemoBlogs());
            await dbContext.SaveChangesAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<Blog> CreateDemoBlogs()
    {
        var domain = new Blog(SecondExplorer, "Šta je Explorer",
            "Explorer je platforma za istraživanje turističkih tura. Autori sastavljaju ture od ključnih tačaka, a istraživači ih prolaze, igraju igre na mapi i dele utiske u zajednici.",
            []);
        domain.Publish();
        domain.AddComment(FirstExplorer, "Savet: pre kodiranja pročitajte opis svog modula i njegovih veza sa drugim modulima.");
        domain.AddComment(SecondExplorer, "Sinergije između modula su najzanimljiviji deo projekta. Srećno!");
        domain.AddComment(ThirdExplorer, "Razgovarajte sa timom koji radi modul od kog zavisite što ranije.");

        var server = new Blog(SecondExplorer, "Kako je organizovan server",
            "Server je modularni monolit na ASP.NET Core platformi. Svaki modul ima svoju šemu u bazi, svoje slojeve i svoj javni ugovor prema drugim modulima.",
            []);
        server.Publish();
        server.AddComment(FirstExplorer, "Savet: poslovna pravila idu u agregat, a ne u servis ili kontroler.");
        server.AddComment(SecondExplorer, "Pokrenite integracione testove pre svakog slanja izmena. Srećno sa migracijama!");
        server.AddComment(ThirdExplorer, "Kad kompajler odbije referencu između projekata, to je namerno.");

        var client = new Blog(ThirdExplorer, "Kako je organizovan klijent",
            "Klijent je Angular aplikacija čiji moduli prate module servera. Tipovi za komunikaciju sa serverom se generišu iz OpenAPI dokumenta, pa ih ne pišemo ručno.",
            []);
        client.Publish();
        client.AddComment(FirstExplorer, "Savet: držite se strukture referentnog modula, čak i kad deluje jednostavno.");
        client.AddComment(SecondExplorer, "Frontend nije strašan koliko izgleda. Srećno!");
        client.AddComment(ThirdExplorer, "Regenerišite tipove svaki put kad se promeni DTO na serveru.");

        var knowledgeBase = new Blog(ThirdExplorer, "Baza znanja kursa",
            "U direktorijumu docs/knowledge-base se nalaze lekcije koje objašnjavaju svaki koncept koji projekat koristi, od arhitekture modula do migracija baze.",
            []);
        knowledgeBase.Publish();
        knowledgeBase.AddComment(FirstExplorer, "Savet: krenite od INDEX.md i pratite preduslove svake lekcije.");
        knowledgeBase.AddComment(SecondExplorer, "Primeri u lekcijama su pojednostavljeni i namerno se razlikuju od projekta.");
        knowledgeBase.AddComment(ThirdExplorer, "Pročitajte lekciju pre nego što pitate. Srećno sa učenjem!");

        return [domain, server, client, knowledgeBase];
    }
}
