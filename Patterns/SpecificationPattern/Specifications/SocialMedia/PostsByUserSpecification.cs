using SpecificationPattern.Database.Entities;
using SpecificationPattern.Specifications.Base;

namespace SpecificationPattern.Specifications.SocialMedia;

/// <summary>
/// Specification for filtering posts by a specific user
/// </summary>
public class PostsByUserSpecification : Specification<Post>
{
    public PostsByUserSpecification(int userId)
    {
        AddFilteringQuery(post => post.UserId == userId);
        AddIncludeQuery(post => post.User);
        AddIncludeQuery(post => post.Category);
        AddIncludeQuery(post => post.Likes);
        AddIncludeQuery(post => post.Comments);
        AddOrderByDescendingQuery(post => post.Id);
    }

    public PostsByUserSpecification(string username)
    {
        AddFilteringQuery(post => post.User.Username == username);
        AddIncludeQuery(post => post.User);
        AddIncludeQuery(post => post.Category);
        AddIncludeQuery(post => post.Likes);
        AddIncludeQuery(post => post.Comments);
        AddOrderByDescendingQuery(post => post.Id);
    }
}
