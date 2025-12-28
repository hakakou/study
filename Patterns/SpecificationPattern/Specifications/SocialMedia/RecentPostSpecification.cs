using SpecificationPattern.Database.Entities;
using SpecificationPattern.Specifications.Base;

namespace SpecificationPattern.Specifications.SocialMedia;

/// <summary>
/// Specification for filtering recent posts within specified time period
/// </summary>
public class RecentPostSpecification : Specification<Post>
{
    public RecentPostSpecification(int daysBack = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);
        AddFilteringQuery(post => post.CreatedAtUtc >= cutoffDate);
        AddIncludeQuery(post => post.User);
        AddIncludeQuery(post => post.Category);
        AddOrderByDescendingQuery(post => post.CreatedAtUtc);
    }
}
