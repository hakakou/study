using Haka.Patterns.DDD;
using Test.Domain.AggregateModel;
using Test.Domain.Exceptions;
using Test.Domain.Specifications;

namespace Test.Domain.DomainServices;

public class IssueManager(IRepository<Issue> issueRepository) : IDomainService
{
    public async Task AssignToUser(AppUser user, Issue issue)
    {
        var issues = await issueRepository.CountAsync();
        if (issues >= 3)
            throw new BusinessException("...");

        issue.AssignedUserId = user.Id;
    }

    public async Task<Issue> CreateAsync(Guid repoId, IssueName name, DateTime createdDate)
    {
        var q = new DuplicateNameSpecification(repoId, name);

        var list = await issueRepository.ListAsync(q);
        bool any = await issueRepository.AnyAsync(q);

        if (any)
            throw new BusinessException("...");

        var issue = new Issue(repoId, name, createdDate);

        return issue;
    }
}
