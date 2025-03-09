namespace PersistentCollections.Set;

internal readonly record struct RemoveArgs<T>(
  T Value,
  IEqualityComparer<T> Comparer,
  MutabilityOwner MutabilityOwner);