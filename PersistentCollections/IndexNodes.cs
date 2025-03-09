using System.Runtime.CompilerServices;

namespace PersistentCollections;

internal readonly struct IndexNodes<T>(T[] raw)
{
  public readonly T[] Raw = raw;
  
  public int Length => Raw.Length;

  public T this[NodeIndex index]
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => Raw[index.Value];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    set => Raw[index.Value] = value;
  }

  public IndexNodes<T> ImmutableUpdate(ArrayPool<T> pool, NodeIndex index, T node)
  {
    var newArray = pool.Rent(Raw.Length);
    Raw.CopyAndSetTo(newArray, index.Value, node);
    return new IndexNodes<T>(newArray);
  }

  public IndexNodes<T> ImmutableInsert(ArrayPool<T> pool, NodeIndex index, T node)
  {
    var newArray = pool.Rent(Raw.Length + 1);
    Raw.CopeAndInsertTo(newArray, index.Value, node);
    return new IndexNodes<T>(newArray);
  }
  
  public IndexNodes<T> ImmutableRemove(ArrayPool<T> pool, NodeIndex index)
  {
    var newArray = pool.Rent(Raw.Length - 1);
    Raw.CopyAndRemoveTo(newArray, index.Value);
    return new IndexNodes<T>(newArray);
  }
}