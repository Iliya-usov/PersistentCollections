using System.Collections;
using System.Collections.Immutable;

namespace PersistentCollections.Set;

public class PersistentHashSet<T> : IImmutableSet<T>
{
  public int Count { get; }
  
  public IImmutableSet<T> Add(T value)
  {
    throw new NotImplementedException();
  }

  public IImmutableSet<T> Clear()
  {
    throw new NotImplementedException();
  }

  public bool Contains(T value)
  {
    throw new NotImplementedException();
  }

  public IImmutableSet<T> Except(IEnumerable<T> other)
  {
    throw new NotImplementedException();
  }

  public IImmutableSet<T> Intersect(IEnumerable<T> other)
  {
    throw new NotImplementedException();
  }

  public bool IsProperSubsetOf(IEnumerable<T> other)
  {
    throw new NotImplementedException();
  }

  public bool IsProperSupersetOf(IEnumerable<T> other)
  {
    throw new NotImplementedException();
  }

  public bool IsSubsetOf(IEnumerable<T> other)
  {
    throw new NotImplementedException();
  }

  public bool IsSupersetOf(IEnumerable<T> other)
  {
    throw new NotImplementedException();
  }

  public bool Overlaps(IEnumerable<T> other)
  {
    throw new NotImplementedException();
  }

  public IImmutableSet<T> Remove(T value)
  {
    throw new NotImplementedException();
  }

  public bool SetEquals(IEnumerable<T> other)
  {
    throw new NotImplementedException();
  }

  public IImmutableSet<T> SymmetricExcept(IEnumerable<T> other)
  {
    throw new NotImplementedException();
  }

  public bool TryGetValue(T equalValue, out T actualValue)
  {
    throw new NotImplementedException();
  }

  public IImmutableSet<T> Union(IEnumerable<T> other)
  {
    throw new NotImplementedException();
  }

  public PersistentHashSetEnumerator<T> GetEnumerator() => new PersistentHashSetEnumerator<T>();

  IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}