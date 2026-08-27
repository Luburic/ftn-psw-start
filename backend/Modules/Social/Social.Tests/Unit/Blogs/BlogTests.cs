using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Blogs;
using Xunit;

namespace Social.Tests.Unit.Blogs;

public class BlogTests
{
    private static Blog CreateBlog() =>
        new(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", ["https://example.com/tvrdjava.jpg"]);

    private static Blog CreatePublishedBlog()
    {
        var blog = CreateBlog();
        blog.Publish();
        return blog;
    }

    [Fact]
    public void Creation_produces_a_draft_blog()
    {
        var blog = CreateBlog();

        blog.Status.Should().Be(BlogStatus.Draft);
        blog.AuthorId.Should().Be(WellKnownUsers.Explorer);
        blog.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        blog.Comments.Should().BeEmpty();
    }

    [Fact]
    public void Creation_without_images_produces_an_empty_list()
    {
        var blog = new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", null);

        blog.Images.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_blank_title(string title)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, title, "Opis obilaska tvrđave.", []);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_blank_description(string description)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", description, []);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Publish_publishes_a_draft_blog()
    {
        var blog = CreateBlog();

        blog.Publish();

        blog.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public void Publish_rejects_an_already_published_blog()
    {
        var blog = CreatePublishedBlog();

        var publishing = () => blog.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Close_closes_a_published_blog()
    {
        var blog = CreatePublishedBlog();

        blog.Close();

        blog.Status.Should().Be(BlogStatus.Closed);
    }

    [Fact]
    public void Close_rejects_a_draft_blog()
    {
        var blog = CreateBlog();

        var closing = () => blog.Close();

        closing.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddComment_stores_the_comment_on_a_published_blog()
    {
        var blog = CreatePublishedBlog();

        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        blog.Comments.Should().ContainSingle(comment =>
            comment.UserId == WellKnownUsers.Administrator && comment.Text == "Odlična tura!");
    }

    [Fact]
    public void AddComment_rejects_a_closed_blog()
    {
        var blog = CreatePublishedBlog();
        blog.Close();

        var addition = () => blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        addition.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddComment_rejects_blank_text(string text)
    {
        var blog = CreatePublishedBlog();

        var addition = () => blog.AddComment(WellKnownUsers.Administrator, text);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateComment_changes_the_authors_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.");

        blog.Comments.Single().Text.Should().Be("Ipak prosečna tura.");
    }

    [Fact]
    public void UpdateComment_rejects_another_users_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var update = () => blog.UpdateComment(comment.Id, WellKnownUsers.Explorer, "Izmena tuđeg komentara.");

        update.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void DeleteComment_removes_the_authors_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.DeleteComment(comment.Id, WellKnownUsers.Administrator);

        blog.Comments.Should().BeEmpty();
    }

}
