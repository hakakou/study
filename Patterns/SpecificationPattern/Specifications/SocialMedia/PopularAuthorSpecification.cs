using SpecificationPattern.Database.Entities;
using SpecificationPattern.Specifications.Base;

namespace SpecificationPattern.Specifications.SocialMedia;

/// <summary>
/// Specification for filtering popular authors based on their posts' like counts
/// </summary>
public class PopularAuthorSpecification : Specification<User>
{
    public PopularAuthorSpecification(int minAverageLikes = 120)
    {
        AddFilteringQuery(user => user.Posts.Any() &&
            user.Posts.SelectMany(p => p.Likes).Count() / (double)user.Posts.Count >= minAverageLikes);

        AddIncludeQuery(user => user.Posts);

        AddOrderByDescendingQuery(user => user.Posts.SelectMany(p => p.Likes).Count());
    }
}
