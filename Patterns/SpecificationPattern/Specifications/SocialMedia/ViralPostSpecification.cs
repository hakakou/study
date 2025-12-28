using SpecificationPattern.Database.Entities;
using SpecificationPattern.Specifications.Base;

namespace SpecificationPattern.Specifications.SocialMedia;

/// <summary>
/// Specification for filtering viral posts based on like count threshold
/// </summary>
public class ViralPostSpecification : Specification<Post>
{
    public ViralPostSpecification(int minLikesCount = 150)
    {
        AddFilteringQuery(post => post.Likes.Count >= minLikesCount);
        AddIncludeQuery(post => post.Likes);
        AddIncludeQuery(post => post.User);
        AddIncludeQuery(post => post.Category);
        AddOrderByDescendingQuery(post => post.Likes.Count);
    }
}
