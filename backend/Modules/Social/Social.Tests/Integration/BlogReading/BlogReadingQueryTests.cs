using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.Blogs;
using Social.Domain.Blogs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogReading;

public class BlogReadingQueryTests : BaseIntegrationTest
{
    public BlogReadingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task GetPublished_returns_only_published_blogs()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var blogs = await client.GetFromJsonAsync<PageResult<BlogDto>>("/api/social/blogs/published", JsonOptions);

        blogs!.Items.Should().OnlyContain(blog => blog.Status == BlogStatus.Published);
        blogs.Items.Should().Contain(blog => blog.Id == BlogSeed.PublishedRiverbank.Id);
        blogs.Items.Should().NotContain(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        blogs.TotalCount.Should().Be(BlogSeed.All.Count(blog => blog.Status == BlogStatus.Published));
    }

    [Fact]
    public async Task GetById_returns_a_published_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var blog = await client.GetFromJsonAsync<BlogDto>(
            $"/api/social/blogs/{BlogSeed.CommentedMuseumVisit.Id}", JsonOptions);

        blog!.Id.Should().Be(BlogSeed.CommentedMuseumVisit.Id);
        blog.Title.Should().Be(BlogSeed.CommentedMuseumVisit.Title);
        blog.Status.Should().Be(BlogStatus.Published);
        blog.Comments.Should().ContainSingle(comment => comment.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task GetById_rejects_a_draft_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_rejects_an_unknown_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/social/blogs/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
