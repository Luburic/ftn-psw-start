using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.BlogAuthoring;
using Social.Application.Blogs;
using Social.Domain.Blogs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogAuthoring;

public class BlogAuthoringCommandTests : BaseIntegrationTest
{
    public BlogAuthoringCommandTests(SocialApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_stores_a_draft_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/social/blogs",
            new CreateBlogDto("Novi blog", "Opis novog bloga.", ["https://example.com/slika.jpg"]));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var blog = await response.Content.ReadFromJsonAsync<BlogDto>(JsonOptions);
        blog!.Id.Should().NotBeEmpty();
        blog.AuthorId.Should().Be(WellKnownUsers.Explorer);
        blog.Status.Should().Be(BlogStatus.Draft);
        blog.Images.Should().ContainSingle().Which.Should().Be("https://example.com/slika.jpg");
        blog.Comments.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_rejects_a_blank_title()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/social/blogs",
            new CreateBlogDto("   ", "Opis novog bloga.", []));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_requires_authentication()
    {
        var response = await Client.PostAsJsonAsync("/api/social/blogs",
            new CreateBlogDto("Novi blog", "Opis novog bloga.", []));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_requires_the_explorer_role()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync("/api/social/blogs",
            new CreateBlogDto("Novi blog", "Opis novog bloga.", []));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Publish_publishes_a_draft_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var published = await client.GetFromJsonAsync<PageResult<BlogDto>>("/api/social/blogs/published", JsonOptions);
        published!.Items.Should().ContainSingle(blog => blog.Id == BlogSeed.FortressImpressions.Id);
    }

    [Fact]
    public async Task Publish_rejects_another_authors_blog()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Close_closes_a_published_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/close", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var published = await client.GetFromJsonAsync<PageResult<BlogDto>>("/api/social/blogs/published", JsonOptions);
        published!.Items.Should().NotContain(blog => blog.Id == BlogSeed.FortressImpressions.Id);
    }
}
