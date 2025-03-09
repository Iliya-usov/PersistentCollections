namespace PersistentCollections.Map;

internal readonly record struct SetArgs<TKey, TValue>(
  TKey Key,
  TValue Value,
  PersistentHashMap<TKey, TValue>.Comparers Comparers,
  CollisionBehavior CollisionBehavior);