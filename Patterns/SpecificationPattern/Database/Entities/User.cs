namespace SpecificationPattern.Database.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
