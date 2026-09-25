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
    public async Task Only_published_blogs_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var blogs = await client.GetFromJsonAsync<PageResult<BlogDto>>("/api/social/blogs/published", JsonOptions);

        blogs!.Items.Should().OnlyContain(blog => blog.Status == BlogStatus.Published);
        blogs.Items.Should().Contain(blog => blog.Id == BlogSeed.PublishedRiverbank.Id);
        blogs.Items.Should().NotContain(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        blogs.TotalCount.Should().Be(BlogSeed.All.Count(blog => blog.Status == BlogStatus.Published));
    }

    [Fact]
    public async Task Published_blog_is_shown_with_its_comments()
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
    public async Task Draft_blog_cannot_be_read()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Unknown_blog_cannot_be_read()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/social/blogs/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
