namespace PersistentCollections.Set;

internal abstract class Node<T>
{
  [ThreadStatic]
  private static ArrayPool<Node<T>>? ourThreadStaticPool;
  internal static ArrayPool<Node<T>> SharedPool => ourThreadStaticPool ??= new(8);
  
  public abstract bool Contains(Hash hash, T value, Shift shift, IEqualityComparer<T> comparer);
  public abstract bool TryGetActualValue(Hash hash, T value, Shift shift, IEqualityComparer<T> comparer, out T actualValue);
  
  public abstract Node<T> Set(Hash hash, Shift shift, in SetArgs<T> args, out int countDiff);
  public abstract Node<T>? Remove(Hash hash, Shift shift, in RemoveArgs<T> args, out int countDiff);

  public abstract int CopyTo(T[] array, int arrayIndex);
}