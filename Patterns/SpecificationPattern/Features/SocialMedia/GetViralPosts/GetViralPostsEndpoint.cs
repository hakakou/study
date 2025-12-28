using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecificationPattern.Database;
using SpecificationPattern.Features.Shared;
using SpecificationPattern.Features.SocialMedia.DTOs;
using SpecificationPattern.Repositories;
using SpecificationPattern.Specifications.SocialMedia;

namespace SpecificationPattern.Features.SocialMedia.GetViralPosts;

public class GetViralPostsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/social-media/viral-posts", Handle);
    }

    private static async Task<IResult> Handle(
        [FromQuery] int? minLikesCount,
        ApplicationDbContext dbContext,
        ILogger<GetViralPostsEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var specification = new ViralPostSpecification(minLikesCount ?? 150);

        var response = await dbContext
	        .ApplySpecification(specification)
	        .Select(post => post.ToDto())
	        .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} viral posts with minimum {MinLikes} likes",
            response.Count, minLikesCount ?? 150);

        return Results.Ok(response);
    }
}
