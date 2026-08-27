using AutoMapper;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Application;

public sealed class SocialMapperProfile : Profile
{
    public SocialMapperProfile()
    {
        CreateMap<Blog, BlogDto>();
        CreateMap<Comment, CommentDto>();
    }
}
