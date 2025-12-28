using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecificationPattern.Database;
using SpecificationPattern.Features.Shared;
using SpecificationPattern.Features.SocialMedia.DTOs;
using SpecificationPattern.Repositories;
using SpecificationPattern.Specifications.SocialMedia;

namespace SpecificationPattern.Features.SocialMedia.GetDotNetAndArchitecturePosts;

public class GetDotNetAndArchitecturePostsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/social-media/dotnet-architecture-posts", Handle);
    }

    private static async Task<IResult> Handle(
        ApplicationDbContext dbContext,
        ILogger<GetDotNetAndArchitecturePostsEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var specification = new DotNetAndArchitecturePostSpecification();

        var response = await dbContext
            .ApplySpecification(specification)
            .Select(post => post.ToDto())
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} posts from .NET and Architecture categories", response.Count);

        return Results.Ok(response);
    }
}
