using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Clean.Core.ContributorAggregate;
using Test.Domain.AggregateModel;
using Xunit.Abstractions;

namespace Clean.IntegrationTests.Data;

public class EfRepositoryAdd(ITestOutputHelper output) : BaseEfRepoTestFixture(output)
{
  [Fact]
  public async Task AddsContributorAndSetsId()
  {
    var testContributorName = ContributorName.From("testContributor");
    var testContributorStatus = ContributorStatus.NotSet;
    var repository = GetRepository();
    var Contributor = new Contributor(testContributorName);

    await repository.AddAsync(Contributor);
    await repository.AddAsync(new Contributor(testContributorName));
    await repository.AddAsync(new Contributor(testContributorName));

    var newContributor = (await repository.ListAsync(new FirstOrDefaultSpecification<Contributor>()))
      .FirstOrDefault();

    newContributor.ShouldNotBeNull();
    testContributorName.ShouldBe(newContributor.Name);
    testContributorStatus.ShouldBe(newContributor.Status);
    newContributor.Id.Value.ShouldBeGreaterThan(0);

    (await repository.CountAsync()).ShouldBe(3);
  }

  [Fact]
  public async Task Custom()
  {
    var repository = GetIssueRepository();

    var issue = new Issue(Guid.NewGuid(), IssueName.From(" Test"), DateTime.Now);

    await repository.AddAsync(issue);

    var issue2 = new Issue(Guid.NewGuid(), IssueName.From(" Test"), DateTime.Now.AddDays(-60));
    await repository.AddAsync(issue2);

    var newIssue = (await repository.ListAsync()).OrderBy(c=>c.Id).ToList();

    newIssue[0].Id.ShouldBeGreaterThan(0);
    newIssue[0].Name.ShouldBe(issue.Name);

    newIssue[0].IsInInactive().ShouldBeFalse();
    newIssue[1].IsInInactive().ShouldBeTrue();

    //repository.get
    var specification = new InactiveIssuesSpecification();
    var query = await repository.ListAsync(specification);
    query.Count().ShouldBe(1);

    var nameSpec = await repository.FirstOrDefaultAsync(new IssueNameSpec(1));
    nameSpec.Value.ShouldBe("Test");
  }
}

/*
     info: 25/12/2025 22:12:58.837 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command) 
      Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "i"."Id", "i"."AssignedUserId", "i"."CreatedDate", "i"."Description", "i"."GitRepositoryId", "i"."Name"
      FROM "Issues" AS "i"
info: 25/12/2025 22:12:58.890 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command) 
      Executed DbCommand (3ms) [Parameters=[@p0='?' (DbType = Int32), @p1='?' (DbType = Guid), @p2='?' (DbType = DateTime), @p3='?', @p4='?' (DbType = Guid), @p5='?' (Size = 4)], CommandType='Text', CommandTimeout='30']
      INSERT INTO "Issues" ("Id", "AssignedUserId", "CreatedDate", "Description", "GitRepositoryId", "Name")
      VALUES (@p0, @p1, @p2, @p3, @p4, @p5);
info: 25/12/2025 22:12:58.905 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command) 
      Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "i"."Id", "i"."AssignedUserId", "i"."CreatedDate", "i"."Description", "i"."GitRepositoryId", "i"."Name"
      FROM "Issues" AS "i"
*/
