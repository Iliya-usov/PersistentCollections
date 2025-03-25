using System.Collections;

namespace PersistentCollections.Map2;

public class PersistentHashMapBuilder2<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
{
  private IndexNode<TKey, TValue> myRoot;
  private readonly PersistentHashMap2<TKey, TValue>.Comparers myComparers;
  
  public int Count { get; private set; }
  public bool IsReadOnly => false;

  public TValue this[TKey key]
  {
    get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
    set => Set(key, value, CollisionBehavior.SetValue);
  }

  IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
  IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

  public ICollection<TKey> Keys => this.Select(static x => x.Key).ToList(); // todo optimize
  public ICollection<TValue> Values => this.Select(static x => x.Value).ToList();// todo optimize

  internal PersistentHashMapBuilder2(IndexNode<TKey, TValue> root, int count, PersistentHashMap2<TKey, TValue>.Comparers comparers)
  {
    myRoot = root;
    Count = count;
    myComparers = comparers;
  }

  public bool Contains(KeyValuePair<TKey, TValue> item)
  {
    return TryGetValue(item.Key, out var value) && myComparers.ValueComparer.Equals(value, item.Value);
  }

  public bool TryGetValue(TKey key, out TValue value)
  {
    return myRoot.TryGetValue(new Hash(myComparers.KeyComparer.GetHashCode(key)), key, Shift.Zero, myComparers, out value);
  }

  public bool ContainsKey(TKey key)
  {
    return TryGetValue(key, out _);
  }

  public void Add(TKey key, TValue value)
  {
    Set(key, value, CollisionBehavior.ThrowIfValueDifferent);;
  }

  public void Add(KeyValuePair<TKey, TValue> item)
  {
    Add(item.Key, item.Value);
  }

  internal void Set(TKey key, TValue value, CollisionBehavior collisionBehavior)
  {
    var hashCode = myComparers.KeyComparer.GetHashCode(key);
    var addArgs = new SetArgs<TKey, TValue>(key, value, myComparers, collisionBehavior);
    myRoot = myRoot.Set(new Hash(hashCode), Shift.Zero, in addArgs, out var countDiff);
    Count += countDiff;
  }

  public bool Remove(KeyValuePair<TKey, TValue> item)
  {
    return Remove(item.Key, item.Value);
  }

  public bool Remove(TKey key)
  {
    return Remove(key, Maybe<TValue>.Null);
  }
  
  public bool Remove(TKey key, TValue value)
  {
    return Remove(key, new Maybe<TValue>(value));
  }
  
  private bool Remove(TKey key, Maybe<TValue> maybe)
  {
    var hashCode = myComparers.KeyComparer.GetHashCode(key);
    var removeArgs = new RemoveArgs<TKey, TValue>(key, maybe, myComparers);
    myRoot = myRoot.Remove(new Hash(hashCode), Shift.Zero, in removeArgs, out var countDiff) ?? IndexNode<TKey, TValue>.Empty;
    Count += countDiff;
    return countDiff != 0;
  }

  public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
  {
    throw new NotImplementedException();
    // if (array.Length - arrayIndex < Count)
    //   throw new ArgumentException("Not enough space in the array");
    //
    // myRoot.CopyTo(array, arrayIndex);
  }
  
  public void Clear()
  {
    Count = 0;
    myRoot = IndexNode<TKey, TValue>.Empty;
  }

  public PersistentHashMap2<TKey, TValue> Build()
  {
    var root = myRoot;
    return new PersistentHashMap2<TKey, TValue>(root, Count, myComparers);
  }

  // public PersistentHashMapEnumerator<TKey, TValue> GetEnumerator()
  // {
  //   return new PersistentHashMapEnumerator<TKey, TValue>(myRoot);
  // }

  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
  {
    yield break;
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}