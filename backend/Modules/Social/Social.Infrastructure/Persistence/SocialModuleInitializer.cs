using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Social.Domain.Blogs;

namespace Social.Infrastructure.Persistence;

internal sealed class SocialModuleInitializer : IHostedService
{
    private static readonly Guid DemoAuthor = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid DemoCommenter = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

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
        var impressions = new Blog(DemoAuthor, "Utisci sa Petrovaradinske tvrđave",
            "Obilazak tvrđave ostavio je snažan utisak: podzemne galerije, Sahat kula i pogled na Dunav u kasno popodne.",
            ["https://example.com/tvrdjava.jpg"]);
        impressions.Publish();
        impressions.AddComment(DemoCommenter, "Bio sam prošlog meseca, galerije su zaista vredne obilaska.");

        var notes = new Blog(DemoAuthor, "Beleške sa Fruške gore",
            "Skica teksta o biciklističkoj ruti preko Fruške gore, još uvek bez fotografija.",
            []);

        return [impressions, notes];
    }
}
