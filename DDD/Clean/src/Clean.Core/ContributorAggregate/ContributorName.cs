using Vogen;

namespace Clean.Core.ContributorAggregate;

[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public partial struct ContributorName
{
  public const int MaxLength = 100;
  private static Validation Validate(in string name) =>
    string.IsNullOrEmpty(name)
      ? Validation.Invalid("Name cannot be empty")
      : name.Length > MaxLength
        ? Validation.Invalid($"Name cannot be longer than {MaxLength} characters")
        : Validation.Ok;
}


[ValueObject<string>(conversions: Conversions.SystemTextJson)]
public readonly partial struct IssueName
{
  public const int MaxLength = 200;
  private static Validation Validate(in string name) =>
    string.IsNullOrEmpty(name)
      ? Validation.Invalid("Name cannot be empty")
      : name.Length > MaxLength
        ? Validation.Invalid($"Name cannot be longer than {MaxLength} characters")
        : Validation.Ok;

  private static string NormalizeInput(string input)
  {
    return input.Trim();
  }
}
