using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecificationPattern.Database;
using SpecificationPattern.Features.Shared;
using SpecificationPattern.Features.SocialMedia.DTOs;
using SpecificationPattern.Repositories;
using SpecificationPattern.Specifications.SocialMedia;

namespace SpecificationPattern.Features.SocialMedia.GetUserPosts;

public class GetUserPostsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/social-media/users/{userIdentifier}/posts", Handle);
    }

    private static async Task<IResult> Handle(
        [FromRoute] string userIdentifier,
        ApplicationDbContext dbContext,
        ILogger<GetUserPostsEndpoint> logger,
        CancellationToken cancellationToken)
    {
	    var specification = int.TryParse(userIdentifier, out var userId)
		    ? new PostsByUserSpecification(userId)
		    : new PostsByUserSpecification(userIdentifier);

	    var query = dbContext.ApplySpecification(specification);

        if (!await query.AnyAsync(cancellationToken))
        {
            logger.LogInformation("No posts found for user {UserIdentifier}", userIdentifier);
            return Results.NotFound($"No posts found for user '{userIdentifier}'");
        }

        var response = await query
            .Select(post => post.ToDto())
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} posts for user {UserIdentifier}",
            response.Count, userIdentifier);

        return Results.Ok(response);
    }
}
