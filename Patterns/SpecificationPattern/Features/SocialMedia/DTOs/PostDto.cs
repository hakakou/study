namespace SpecificationPattern.Features.SocialMedia.DTOs;

public class PostDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public string? Category { get; set; }
    public AuthorDto? Author { get; set; }
}

public class AuthorDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
}
