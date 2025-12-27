using Vogen;

[assembly: VogenDefaults(
        staticAbstractsGeneration: StaticAbstractsGeneration.MostCommon 
        | StaticAbstractsGeneration.InstanceMethodsAndProperties)]


namespace Clean.Core.ContributorAggregate;

[ValueObject<int>()]
public readonly partial struct ContributorId
{
  private static Validation Validate(int value)
      => value > 0 ? Validation.Ok : Validation.Invalid("ContributorId must be positive.");
}

[ValueObject<int>]
public readonly partial struct IssueId
{
  private static Validation Validate(int value)
      => value > 0 ? Validation.Ok : Validation.Invalid("IssueId must be positive.");
}

//public static class GuidFactory<TSelf>
//   where TSelf : IVogen<TSelf, Guid>
//{
//  // ReSharper disable once StaticMemberInGenericType
//  private static long _counter = DateTime.UtcNow.Ticks;

//  static TSelf NewSequential()
//  {
//    var guidBytes = Guid.NewGuid().ToByteArray();

//    var counterBytes = BitConverter.GetBytes(
//       Interlocked.Increment(ref _counter));

//    if (!BitConverter.IsLittleEndian)
//    {
//      Array.Reverse(counterBytes);
//    }

//    guidBytes[08] = counterBytes[1];
//    guidBytes[09] = counterBytes[0];
//    guidBytes[10] = counterBytes[7];
//    guidBytes[11] = counterBytes[6];
//    guidBytes[12] = counterBytes[5];
//    guidBytes[13] = counterBytes[4];
//    guidBytes[14] = counterBytes[3];
//    guidBytes[15] = counterBytes[2];

//    return TSelf.From(new Guid(guidBytes));
//  }
//}
