using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecificationPattern.Database;
using SpecificationPattern.Features.Shared;
using SpecificationPattern.Features.SocialMedia.DTOs;
using SpecificationPattern.Repositories;
using SpecificationPattern.Specifications.SocialMedia;

namespace SpecificationPattern.Features.SocialMedia.GetPopularAuthors;

public class GetPopularAuthorsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/social-media/popular-authors", Handle);
    }

    private static async Task<IResult> Handle(
        [FromQuery] int? minAverageLikes,
        ApplicationDbContext dbContext,
        ILogger<GetPopularAuthorsEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var specification = new PopularAuthorSpecification(minAverageLikes ?? 120);

        var response = await dbContext
            .ApplySpecification(specification)
            .Select(author => author.ToDto())
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} popular authors with minimum average likes of {MinLikes}",
            response.Count, minAverageLikes ?? 120);

        return Results.Ok(response);
    }
}
