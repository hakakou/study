namespace SpecificationPattern.Database.Entities;

public class Like
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
}
