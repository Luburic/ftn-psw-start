using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.BlogCommenting;
using Social.Application.Blogs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogCommenting;

public class BlogCommentingCommandTests : BaseIntegrationTest
{
    public BlogCommentingCommandTests(SocialApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AddComment_stores_the_comment_on_a_published_blog()
    {
        var author = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        await author.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/comments",
            new CreateCommentDto("Odličan blog!"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var published = await client.GetFromJsonAsync<PageResult<BlogDto>>("/api/social/blogs/published", JsonOptions);
        var blog = published!.Items.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        blog.Comments.Should().ContainSingle(comment =>
            comment.UserId == WellKnownUsers.Administrator && comment.Text == "Odličan blog!");
    }

    [Fact]
    public async Task UpdateComment_changes_the_callers_own_comment()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var commentId = await PublishAndCommentAsync(client);

        var response = await client.PutAsJsonAsync(
            $"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/comments/{commentId}",
            new UpdateCommentDto("Ipak prosečan blog."));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var published = await client.GetFromJsonAsync<PageResult<BlogDto>>("/api/social/blogs/published", JsonOptions);
        var blog = published!.Items.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        blog.Comments.Single().Text.Should().Be("Ipak prosečan blog.");
    }

    [Fact]
    public async Task UpdateComment_rejects_another_users_comment()
    {
        var commenter = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var commentId = await PublishAndCommentAsync(commenter);
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PutAsJsonAsync(
            $"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/comments/{commentId}",
            new UpdateCommentDto("Izmena tuđeg komentara."));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteComment_removes_the_callers_own_comment()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var commentId = await PublishAndCommentAsync(client);

        var response = await client.DeleteAsync(
            $"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/comments/{commentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var published = await client.GetFromJsonAsync<PageResult<BlogDto>>("/api/social/blogs/published", JsonOptions);
        var blog = published!.Items.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        blog.Comments.Should().BeEmpty();
    }

    private async Task<Guid> PublishAndCommentAsync(HttpClient commenter)
    {
        var author = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        await author.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        await commenter.PostAsJsonAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/comments",
            new CreateCommentDto("Odličan blog!"));

        var published = await commenter.GetFromJsonAsync<PageResult<BlogDto>>("/api/social/blogs/published", JsonOptions);
        var blog = published!.Items.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        return blog.Comments.Single().Id;
    }
}
