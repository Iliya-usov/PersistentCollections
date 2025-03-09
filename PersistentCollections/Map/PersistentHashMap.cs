using System.Collections;
using System.Collections.Immutable;

namespace PersistentCollections.Map;

public sealed class PersistentHashMap<TKey, TValue> : IImmutableDictionary<TKey, TValue>
{
  private readonly IndexNode<TKey, TValue> myRoot;
  private readonly Comparers myComparers;
  
  public int Count { get; }

  public TValue this[TKey key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

  public IEnumerable<TKey> Keys => this.Select(static x => x.Key);
  public IEnumerable<TValue> Values => this.Select(static x => x.Value);

  public static PersistentHashMap<TKey, TValue> Empty { get; } = new(IndexNode<TKey, TValue>.Empty,0, Comparers.Default);

  internal PersistentHashMap(IndexNode<TKey, TValue> root, int count, Comparers comparers)
  {
    root.Freeze();
    
    myRoot = root;
    Count = count;
    myComparers = comparers;
  }

  public bool ContainsKey(TKey key)
  {
    return TryGetValue(key, out _);
  }

  public bool TryGetValue(TKey key, out TValue value)
  {
    var hashCode = myComparers.KeyComparer.GetHashCode(key);
    return myRoot.TryGetValue(new Hash(hashCode), key, Shift.Zero, myComparers, out value);
  }

  public bool TryGetKey(TKey equalKey, out TKey actualKey)
  {
    var hashCode = myComparers.KeyComparer.GetHashCode(equalKey);
    return myRoot.TryGetKey(new Hash(hashCode), equalKey, Shift.Zero, myComparers, out actualKey);
  }
    
  public PersistentHashMap<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
  {
    var builder = ToBuilder();
    foreach (var (key, value) in pairs) // todo fast paths 
      builder.Set(key, value, CollisionBehavior.ThrowIfValueDifferent);
    
    return builder.Build();
  }
  
  public PersistentHashMap<TKey, TValue> Clear() => Empty;

  public bool Contains(KeyValuePair<TKey, TValue> pair)
  {
    return TryGetValue(pair.Key, out var value) && myComparers.ValueComparer.Equals(value, pair.Value);
  }
  
  public PersistentHashMap<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
  {
    var builder = ToBuilder();
    foreach (var key in keys) // todo fast paths 
      builder.Remove(key);
    
    return builder.Build();
  }

  public PersistentHashMap<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
  {
    var builder = ToBuilder();
    foreach (var (key, value) in items) // todo fast paths 
      builder[key] = value;
    
    return builder.Build();
  }
  
  public PersistentHashMap<TKey, TValue> Add(TKey key, TValue value) => Set(key, value, CollisionBehavior.ThrowIfValueDifferent);
  public PersistentHashMap<TKey, TValue> SetItem(TKey key, TValue value) => Set(key, value, CollisionBehavior.SetValue);

  private PersistentHashMap<TKey, TValue> Set(TKey key, TValue value, CollisionBehavior collisionBehavior)
  {
    var hashCode = myComparers.KeyComparer.GetHashCode(key);
    var addArgs = new SetArgs<TKey, TValue>(key, value, myComparers, collisionBehavior);
    var newRoot = myRoot.Set(new Hash(hashCode), Shift.Zero, in addArgs, out var countDiff);
    if (newRoot == myRoot)
      return this;

    return new PersistentHashMap<TKey, TValue>(newRoot, Count + countDiff, myComparers);
  }
  
  public PersistentHashMap<TKey, TValue> Remove(TKey key) => Remove(key, Maybe<TValue>.Null);
  public PersistentHashMap<TKey, TValue> Remove(TKey key, TValue value) => Remove(key, new Maybe<TValue>(value));

  private PersistentHashMap<TKey, TValue> Remove(TKey key, Maybe<TValue> maybe)
  {
    var hashCode = myComparers.KeyComparer.GetHashCode(key);
    var removeArg = new RemoveArgs<TKey, TValue>(key, maybe, myComparers);
    var newRoot = myRoot.Remove(new Hash(hashCode), Shift.Zero, in removeArg, out var countDiff);
    if (newRoot == myRoot)
      return this;
    
    return new PersistentHashMap<TKey, TValue>(newRoot ?? IndexNode<TKey, TValue>.Empty, Count + countDiff, myComparers);
  }
  
  public PersistentHashMap<TKey, TValue> WithComparers(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
  {
    keyComparer ??= EqualityComparer<TKey>.Default;
    valueComparer ??= EqualityComparer<TValue>.Default;
    
    if (keyComparer.Equals(myComparers.KeyComparer))
    {
      if (valueComparer.Equals(myComparers.ValueComparer))
        return this;
      
      return new PersistentHashMap<TKey, TValue>(myRoot, Count, new Comparers(keyComparer, valueComparer));
    }
    
    var builder = new PersistentHashMapBuilder<TKey, TValue>(IndexNode<TKey, TValue>.Empty, 0, new Comparers(keyComparer, valueComparer));
    foreach (var keyValuePair in this)
    {
      builder.Add(keyValuePair.Key, keyValuePair.Value);
    }
    
    return builder.Build();
  }
  
  public PersistentHashMapBuilder<TKey, TValue> ToBuilder()
  {
    return new PersistentHashMapBuilder<TKey, TValue>(myRoot, Count, myComparers);
  }

  public PersistentHashMapEnumerator<TKey, TValue> GetEnumerator()
  {
    return new PersistentHashMapEnumerator<TKey, TValue>(myRoot);
  }

  IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  
  
  #region explicit IImmutableDictionary

  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value) => Add(key, value);
  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key) => Remove(key);
  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value) => Set(key, value, CollisionBehavior.SetValue);
  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear() => Clear();

  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items) => SetItems(items);
  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys) => RemoveRange(keys);
  IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs) => AddRange(pairs);

  #endregion

  internal class Comparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
  {
    public static readonly Comparers Default = new(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
    
    public readonly IEqualityComparer<TKey> KeyComparer = keyComparer;
    public readonly IEqualityComparer<TValue> ValueComparer = valueComparer;
  }
}