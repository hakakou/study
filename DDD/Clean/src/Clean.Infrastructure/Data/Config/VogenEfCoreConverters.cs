using Clean.Core.ContributorAggregate;
using Vogen;

namespace Clean.Infrastructure.Data.Config;

[EfCoreConverter<ContributorId>]
[EfCoreConverter<ContributorName>]
[EfCoreConverter<IssueId>]
[EfCoreConverter<IssueName>]
internal partial class VogenEfCoreConverters;
