namespace PersistentCollections.Set;

internal readonly record struct SetArgs<T>(
  T Value,
  IEqualityComparer<T> Comparer,
  MutabilityOwner MutabilityOwner);