namespace SpecificationPattern.Features.SocialMedia.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int PostsCount { get; set; }
    public int TotalLikes { get; set; }
    public double AverageLikes { get; set; }
}
