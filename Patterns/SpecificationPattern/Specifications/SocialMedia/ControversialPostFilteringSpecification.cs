using SpecificationPattern.Database.Entities;
using SpecificationPattern.Specifications.Base;

namespace SpecificationPattern.Specifications.SocialMedia;

/// <summary>
/// Specification for filtering controversial posts with high comment-to-like ratio
/// </summary>
public class ControversialPostSpecification : Specification<Post>
{
    public ControversialPostSpecification(double minCommentToLikeRatio = 0.6, int minTotalEngagement = 50)
    {
        AddFilteringQuery(post =>
            post.Likes.Count > 0 &&
            post.Comments.Count > 0 &&
            (post.Likes.Count + post.Comments.Count) >= minTotalEngagement &&
            ((double)post.Comments.Count / post.Likes.Count) >= minCommentToLikeRatio);

        AddIncludeQuery(post => post.Likes);
        AddIncludeQuery(post => post.Comments);
        AddIncludeQuery(post => post.User);
        AddIncludeQuery(post => post.Category);
        AddOrderByDescendingQuery(post => (double)post.Comments.Count / post.Likes.Count);
    }
}
