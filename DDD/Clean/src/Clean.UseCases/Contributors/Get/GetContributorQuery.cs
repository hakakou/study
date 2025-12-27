using Clean.Core.ContributorAggregate;

namespace Clean.UseCases.Contributors.Get;

public record GetContributorQuery(ContributorId ContributorId) : IQuery<Result<ContributorDto>>;
