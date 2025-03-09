namespace PersistentCollections.Map;

internal abstract class Node<TKey, TValue>(bool isFrozen)
{
  [ThreadStatic]
  private static ArrayPool<Node<TKey, TValue>>? ourThreadStaticPool;
  internal static ArrayPool<Node<TKey, TValue>> SharedPool => ourThreadStaticPool ??= new(8);

  public bool IsFrozen = isFrozen;
  public bool IsMutable => !IsFrozen;

  public abstract bool TryGetValue(Hash hash, TKey key, Shift shift, PersistentHashMap<TKey, TValue>.Comparers comparers, out TValue value);
  public abstract bool TryGetKey(Hash hash, TKey key, Shift shift, PersistentHashMap<TKey, TValue>.Comparers comparers, out TKey actualKey);

  public abstract Node<TKey, TValue> Set(Hash hash, Shift shift, in SetArgs<TKey, TValue> args, out int countDiff);
  public abstract Node<TKey, TValue>? Remove(Hash hash, Shift shift, in RemoveArgs<TKey, TValue> args, out int countDiff);

  public abstract int CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex);
  
  public abstract void Freeze();
}