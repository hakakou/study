using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecificationPattern.Database;
using SpecificationPattern.Features.Shared;
using SpecificationPattern.Features.SocialMedia.DTOs;
using SpecificationPattern.Repositories;
using SpecificationPattern.Specifications.SocialMedia;

namespace SpecificationPattern.Features.SocialMedia.GetEngagedPosts;

public class GetEngagedPostsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/social-media/engaged-posts", Handle);
    }

    private static async Task<IResult> Handle(
        [FromQuery] int? minCommentsCount,
        ApplicationDbContext dbContext,
        ILogger<GetEngagedPostsEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var specification = new MostEngagedPostSpecification(minCommentsCount ?? 75);

        var response = await dbContext
            .ApplySpecification(specification)
            .Select(post => post.ToDto())
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} engaged posts with minimum {MinComments} comments",
            response.Count, minCommentsCount ?? 75);

        return Results.Ok(response);
    }
}
