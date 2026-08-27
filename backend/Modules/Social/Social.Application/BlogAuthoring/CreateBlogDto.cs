namespace Social.Application.BlogAuthoring;

public sealed record CreateBlogDto(string Title, string Description, List<string>? Images);
