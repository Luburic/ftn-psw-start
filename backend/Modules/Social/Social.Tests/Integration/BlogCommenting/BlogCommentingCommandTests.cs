using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.BlogCommenting;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogCommenting;

public class BlogCommentingCommandTests : BaseIntegrationTest
{
    public BlogCommentingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task AddComment_stores_the_comment_on_a_published_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateCommentDto("Odličan blog!");

        var response = await client.PostAsJsonAsync($"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/comments",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.PublishedRiverbank.Id);
        stored.Comments.Should().ContainSingle(comment =>
            comment.UserId == WellKnownUsers.Administrator && comment.Text == "Odličan blog!");
    }

    [Fact]
    public async Task UpdateComment_changes_the_callers_own_comment()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var commentId = BlogSeed.CommentedMuseumVisit.Comments.Single().Id;
        var request = new UpdateCommentDto("Ipak prosečan blog.");

        var response = await client.PutAsJsonAsync(
            $"/api/social/blogs/{BlogSeed.CommentedMuseumVisit.Id}/comments/{commentId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.CommentedMuseumVisit.Id);
        stored.Comments.Single().Text.Should().Be("Ipak prosečan blog.");
    }

    [Fact]
    public async Task UpdateComment_rejects_another_users_comment()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        var commentId = BlogSeed.CommentedMuseumVisit.Comments.Single().Id;
        var request = new UpdateCommentDto("Izmena tuđeg komentara.");

        var response = await client.PutAsJsonAsync(
            $"/api/social/blogs/{BlogSeed.CommentedMuseumVisit.Id}/comments/{commentId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.CommentedMuseumVisit.Id);
        stored.Comments.Single().Text.Should().Be("Odlična preporuka!");
    }

    [Fact]
    public async Task DeleteComment_removes_the_callers_own_comment()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var commentId = BlogSeed.CommentedMuseumVisit.Comments.Single().Id;

        var response = await client.DeleteAsync(
            $"/api/social/blogs/{BlogSeed.CommentedMuseumVisit.Id}/comments/{commentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.CommentedMuseumVisit.Id);
        stored.Comments.Should().BeEmpty();
    }
}
