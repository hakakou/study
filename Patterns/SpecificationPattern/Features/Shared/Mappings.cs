using SpecificationPattern.Database.Entities;
using SpecificationPattern.Features.SocialMedia.DTOs;

namespace SpecificationPattern.Features.Shared;

public static class Mappings
{
    public static PostDto ToDto(this Post post)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAtUtc = post.CreatedAtUtc,
            LikesCount = post.Likes.Count,
            CommentsCount = post.Comments.Count,
            Category = post.Category.Name,
            Author = post.User is not null
                ? new AuthorDto
                {
                    Id = post.User.Id,
                    Username = post.User.Username
                }
                : null
        };
    }

    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            PostsCount = user.Posts.Count,
            TotalLikes = user.Posts.Sum(p => p.Likes.Count),
            AverageLikes = user.Posts?.Count > 0
                ? user.Posts.Sum(p => p.Likes.Count) / (double)user.Posts.Count
                : 0
        };
    }
}
