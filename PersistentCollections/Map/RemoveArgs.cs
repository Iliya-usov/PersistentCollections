namespace PersistentCollections.Map;

internal readonly record struct RemoveArgs<TKey, TValue>(
  TKey Key,
  Maybe<TValue> Value,
  PersistentHashMap<TKey, TValue>.Comparers Comparers);