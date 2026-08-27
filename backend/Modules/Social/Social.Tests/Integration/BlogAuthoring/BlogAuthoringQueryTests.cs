using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.BlogAuthoring;
using Social.Application.Blogs;
using Xunit;

namespace Social.Tests.Integration.BlogAuthoring;

public class BlogAuthoringQueryTests : BaseIntegrationTest
{
    public BlogAuthoringQueryTests(SocialApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetMine_returns_only_the_callers_blogs()
    {
        var otherAuthor = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        await otherAuthor.PostAsJsonAsync("/api/social/blogs",
            new CreateBlogDto("Tuđi blog", "Opis tuđeg bloga.", []));
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var blogs = await client.GetFromJsonAsync<List<BlogDto>>("/api/social/blogs/mine", JsonOptions);

        blogs.Should().HaveCount(2);
        blogs.Should().OnlyContain(blog => blog.AuthorId == WellKnownUsers.Explorer);
    }
}
