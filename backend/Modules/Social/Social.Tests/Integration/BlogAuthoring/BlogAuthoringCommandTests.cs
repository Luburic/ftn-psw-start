using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.BlogAuthoring;
using Social.Application.Blogs;
using Social.Domain.Blogs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogAuthoring;

public class BlogAuthoringCommandTests : BaseIntegrationTest
{
    public BlogAuthoringCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Create_stores_a_draft_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", ["https://example.com/slika.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<BlogDto>(JsonOptions);
        created!.AuthorId.Should().Be(WellKnownUsers.Explorer);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore + 1);
        var stored = assertContext.Blogs.Single(blog => blog.Id == created.Id);
        stored.Status.Should().Be(BlogStatus.Draft);
        stored.Images.Should().ContainSingle().Which.Should().Be("https://example.com/slika.jpg");
        stored.Comments.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_rejects_a_blank_title()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateBlogDto("   ", "Opis novog bloga.", []);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore);
    }

    [Fact]
    public async Task Create_requires_authentication()
    {
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", []);

        var response = await Client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_requires_the_explorer_role()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", []);

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Publish_publishes_a_draft_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public async Task Publish_rejects_another_authors_blog()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Status.Should().Be(BlogStatus.Draft);
    }
}
