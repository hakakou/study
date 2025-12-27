using Clean.Core.ContributorAggregate;
using Xunit.Abstractions;

namespace Clean.IntegrationTests.Data;

public class EfRepositoryDelete(ITestOutputHelper output) : BaseEfRepoTestFixture(output)
{
  [Fact]
  public async Task DeletesItemAfterAddingIt()
  {
    // add a Contributor
    var repository = GetRepository();
    var initialName = ContributorName.From(Guid.NewGuid().ToString());
    var Contributor = new Contributor(initialName);
    await repository.AddAsync(Contributor);

    // delete the item
    await repository.DeleteAsync(Contributor);

    // verify it's no longer there
    (await repository.ListAsync()).ShouldNotContain(Contributor => Contributor.Name == initialName);
  }
}
