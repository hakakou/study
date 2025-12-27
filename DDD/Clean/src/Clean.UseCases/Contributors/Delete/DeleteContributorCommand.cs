using Clean.Core.ContributorAggregate;

namespace Clean.UseCases.Contributors.Delete;

public record DeleteContributorCommand(ContributorId ContributorId) : ICommand<Result>;
