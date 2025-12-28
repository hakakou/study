using SpecificationPattern.Database.Entities;
using SpecificationPattern.Specifications.Base;

namespace SpecificationPattern.Specifications.SocialMedia;

/// <summary>
/// Specification for filtering posts from .NET OR Architecture categories
/// </summary>
public class DotNetAndArchitecturePostSpecification : Specification<Post>
{
    public DotNetAndArchitecturePostSpecification()
    {
        var dotNetSpec = new PostByCategorySpecification(".NET");
        var architectureSpec = new PostByCategorySpecification("Architecture");

        var combinedSpec = dotNetSpec.Or(architectureSpec);

        AddFilteringQuery(combinedSpec.FilterQuery!);

        AddIncludeQuery(post => post.Category);
        AddIncludeQuery(post => post.User);

        AddOrderByDescendingQuery(post => post.Id);
    }
}

/// <summary>
/// Helper specification for filtering posts by category name
/// </summary>
public class PostByCategorySpecification : Specification<Post>
{
    public PostByCategorySpecification(string categoryName)
    {
        AddFilteringQuery(post => post.Category.Name == categoryName);
    }
}
