using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.Blogs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogReading;

public class BlogReadingQueryTests : BaseIntegrationTest
{
    public BlogReadingQueryTests(SocialApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetPublished_returns_only_published_blogs()
    {
        var author = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        await author.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var blogs = await client.GetFromJsonAsync<PageResult<BlogDto>>("/api/social/blogs/published", JsonOptions);

        blogs!.Items.Should().ContainSingle(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        blogs.TotalCount.Should().Be(1);
    }
}
