using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecificationPattern.Database;
using SpecificationPattern.Features.Shared;
using SpecificationPattern.Features.SocialMedia.DTOs;
using SpecificationPattern.Repositories;
using SpecificationPattern.Specifications.SocialMedia;

namespace SpecificationPattern.Features.SocialMedia.GetRecentPosts;

public class GetRecentPostsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/social-media/recent-posts", Handle);
    }

    private static async Task<IResult> Handle(
        [FromQuery] int? daysBack,
        ApplicationDbContext dbContext,
        ILogger<GetRecentPostsEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var specification = new RecentPostSpecification(daysBack ?? 7);

        var response = await dbContext
            .ApplySpecification(specification)
            .Select(post => post.ToDto())
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} recent posts from the last {DaysBack} days",
            response.Count, daysBack ?? 7);

        return Results.Ok(response);
    }
}
