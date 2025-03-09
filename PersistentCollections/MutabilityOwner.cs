namespace PersistentCollections;

internal sealed class MutabilityOwner
{
  public static MutabilityOwner Null { get; } = new();
  
  public bool IsTheSame(MutabilityOwner other) =>
    ReferenceEquals(other, this) && !ReferenceEquals(Null, other);
}