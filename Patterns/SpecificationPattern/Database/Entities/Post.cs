namespace SpecificationPattern.Database.Entities;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public DateTime CreatedAtUtc { get; set; }
}
