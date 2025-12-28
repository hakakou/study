using SpecificationPattern.Database.Entities;
using SpecificationPattern.Specifications.Base;

namespace SpecificationPattern.Specifications.SocialMedia;

/// <summary>
/// Specification for filtering posts with high engagement based on comment count
/// </summary>
public class MostEngagedPostSpecification : Specification<Post>
{
    public MostEngagedPostSpecification(int minCommentsCount = 75)
    {
        AddFilteringQuery(post => post.Comments.Count >= minCommentsCount);
        AddIncludeQuery(post => post.Comments);
        AddIncludeQuery(post => post.User);
        AddIncludeQuery(post => post.Category);
        AddOrderByDescendingQuery(post => post.Comments.Count);
    }
}
