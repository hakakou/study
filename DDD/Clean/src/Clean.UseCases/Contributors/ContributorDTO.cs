using Clean.Core.ContributorAggregate;

namespace Clean.UseCases.Contributors;
public record ContributorDto(ContributorId Id, ContributorName Name, PhoneNumber PhoneNumber);
