using SpecificationPattern.Database.Entities;
using SpecificationPattern.Specifications.Base;

namespace SpecificationPattern.Specifications.SocialMedia;

/// <summary>
/// Specification for filtering posts that are both recent and have high engagement
/// </summary>
public class HighEngagementRecentPostSpecification : Specification<Post>
{
    public HighEngagementRecentPostSpecification(int daysBack = 7, int minLikes = 100, int minComments = 30)
    {
        var recentSpec = new RecentPostSpecification(daysBack);
        var highEngagementSpec = new HighEngagementPostSpecification(minLikes, minComments);

        var combinedSpec = recentSpec.And(highEngagementSpec);

        AddFilteringQuery(combinedSpec.FilterQuery!);

        AddIncludeQuery(post => post.User);
        AddIncludeQuery(post => post.Category);
        AddIncludeQuery(post => post.Likes);
        AddIncludeQuery(post => post.Comments);

        AddOrderByDescendingQuery(post => post.Likes.Count + post.Comments.Count);
    }
}

/// <summary>
/// Helper specification for high engagement posts
/// </summary>
public class HighEngagementPostSpecification : Specification<Post>
{
    public HighEngagementPostSpecification(int minLikes, int minComments)
    {
        AddFilteringQuery(post => post.Likes.Count >= minLikes && post.Comments.Count >= minComments);
    }
}
