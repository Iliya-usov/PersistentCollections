using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace PersistentCollections.Map2;

public sealed class PersistentHashMap2<TKey, TValue> : IImmutableDictionary<TKey, TValue>
{
  private IndexNode<TKey, TValue> myRoot;
  private readonly Comparers myComparers;
  
  public int Count { get; }

  public TValue this[TKey key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

  public IEnumerable<TKey> Keys => this.Select(static x => x.Key);
  public IEnumerable<TValue> Values => this.Select(static x => x.Value);

  public static PersistentHashMap2<TKey, TValue> Empty { get; } = new(IndexNode<TKey, TValue>.Empty,0, Comparers.Default);

  internal PersistentHashMap2(IndexNode<TKey, TValue> root, int count, Comparers comparers)
  {
    myRoot = root.Freeze();;
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
    
  public PersistentHashMap2<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
  {
    var builder = ToBuilder();
    foreach (var (key, value) in pairs) // todo fast paths 
      builder.Set(key, value, CollisionBehavior.ThrowIfValueDifferent);
    
    return builder.Build();
  }
  
  public PersistentHashMap2<TKey, TValue> Clear() => Empty;

  public bool Contains(KeyValuePair<TKey, TValue> pair)
  {
    return TryGetValue(pair.Key, out var value) && myComparers.ValueComparer.Equals(value, pair.Value);
  }
  
  public PersistentHashMap2<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
  {
    var builder = ToBuilder();
    foreach (var key in keys) // todo fast paths 
      builder.Remove(key);
    
    return builder.Build();
  }

  public PersistentHashMap2<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
  {
    var builder = ToBuilder();
    foreach (var (key, value) in items) // todo fast paths 
      builder[key] = value;
    
    return builder.Build();
  }
  
  public PersistentHashMap2<TKey, TValue> Add(TKey key, TValue value) => Set(key, value, CollisionBehavior.ThrowIfValueDifferent);
  public PersistentHashMap2<TKey, TValue> SetItem(TKey key, TValue value) => Set(key, value, CollisionBehavior.SetValue);

  private PersistentHashMap2<TKey, TValue> Set(TKey key, TValue value, CollisionBehavior collisionBehavior)
  {
    var hashCode = myComparers.KeyComparer.GetHashCode(key);
    var addArgs = new SetArgs<TKey, TValue>(key, value, myComparers, collisionBehavior);
    var newRoot = myRoot.Set(new Hash(hashCode), Shift.Zero, in addArgs, out var countDiff);
    if (newRoot.Value == myRoot.Value)
      return this;

    return new PersistentHashMap2<TKey, TValue>(newRoot, Count + countDiff, myComparers);
  }
  
  public PersistentHashMap2<TKey, TValue> Remove(TKey key) => Remove(key, Maybe<TValue>.Null);
  public PersistentHashMap2<TKey, TValue> Remove(TKey key, TValue value) => Remove(key, new Maybe<TValue>(value));

  private PersistentHashMap2<TKey, TValue> Remove(TKey key, Maybe<TValue> maybe)
  {
    var hashCode = myComparers.KeyComparer.GetHashCode(key);
    var removeArg = new RemoveArgs<TKey, TValue>(key, maybe, myComparers);
    var newRoot = myRoot.Remove(new Hash(hashCode), Shift.Zero, in removeArg, out var countDiff);
    if (newRoot.HasValue && newRoot.Value.Value == myRoot.Value)
      return this;
    
    return new PersistentHashMap2<TKey, TValue>(newRoot ?? IndexNode<TKey, TValue>.Empty, Count + countDiff, myComparers);
  }
  
  public PersistentHashMap2<TKey, TValue> WithComparers(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
  {
    keyComparer ??= EqualityComparer<TKey>.Default;
    valueComparer ??= EqualityComparer<TValue>.Default;
    
    if (keyComparer.Equals(myComparers.KeyComparer))
    {
      if (valueComparer.Equals(myComparers.ValueComparer))
        return this;
      
      return new PersistentHashMap2<TKey, TValue>(myRoot, Count, new Comparers(keyComparer, valueComparer));
    }
    
    var builder = new PersistentHashMapBuilder2<TKey, TValue>(IndexNode<TKey, TValue>.Empty, 0, new Comparers(keyComparer, valueComparer));
    foreach (var keyValuePair in this)
    {
      builder.Add(keyValuePair.Key, keyValuePair.Value);
    }
    
    return builder.Build();
  }
  
  public PersistentHashMapBuilder2<TKey, TValue> ToBuilder()
  {
    return new PersistentHashMapBuilder2<TKey, TValue>(myRoot, Count, myComparers);
  }

  // public PersistentHashMapEnumerator<TKey, TValue> GetEnumerator()
  // {
  //   return new PersistentHashMapEnumerator<TKey, TValue>(myRoot);
  // }

  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
  {
    yield break;
  }
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

internal enum Kind : byte
{
  IndexNode = 1,
  CollisionNode = 2,
  SingleValueNode = 3
}

[StructLayout(LayoutKind.Explicit)]
internal struct State
{
  [FieldOffset(0)]
  public long Value;
  
  [FieldOffset(0)] public Bitmap Bitmap;
  [FieldOffset(0)] public Hash Hash;
  
  [FieldOffset(4)] public bool IsFrozen; // can be a part of a hash (first/last bit)
  [FieldOffset(5)] public Kind Kind; // use type check instead of this?
  
  [FieldOffset(6)] public short Dummy;

  public State(Bitmap bitmap, bool isFrozen)
  {
    Bitmap = bitmap;
    IsFrozen = isFrozen;
    Kind = Kind.IndexNode;
  }

  public State(Hash hash, Kind kind, bool isFrozen)
  {
    Hash = hash;
    IsFrozen = isFrozen;
    Kind = kind;
  }
}

internal struct Entry<TKey, TValue>
{
  [ThreadStatic]
  private static ArrayPool<Entry<TKey, TValue>>? ourThreadStaticPool;
  internal static ArrayPool<Entry<TKey, TValue>> SharedPool => ourThreadStaticPool ??= new(8);
  
  public State State;
  public object Value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Entry(Bitmap bitmap, Array nodes, bool isFrozen)
  {
    State = new State(bitmap, isFrozen);
    Value = nodes;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Entry(Hash hash, KeyValuePair<TKey, TValue>[] values, bool isFrozen)
  {
    State = new State(hash, Kind.CollisionNode, isFrozen);
    Value = values;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Entry(Hash hash, object boxedPair, bool isFrozen)
  {
    State = new State(hash, Kind.SingleValueNode, isFrozen);
    Value = boxedPair;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IndexNode<TKey, TValue> AsIndexNode() => new (State.Bitmap, GetValueAsIndexArray(), State.IsFrozen);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private Entry<TKey, TValue>[] GetValueAsIndexArray() => (Entry<TKey, TValue>[])Value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public CollisionNode<TKey, TValue> AsCollisionNode() => new (State.Hash, (KeyValuePair<TKey, TValue>[])Value, State.IsFrozen);
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public SingleValueNode<TKey, TValue> AsSingleValueNode() => new (State.Hash, (KeyValuePair<TKey, TValue>)Value, Value, State.IsFrozen);

  public void Freeze()
  {
    if (State.IsFrozen) return;

    if (State.Kind == Kind.IndexNode)
    {
      var array = GetValueAsIndexArray();
      for (var index = 0; index < array.Length; index++)
      {
        array[index].Freeze();
      }
    }
    
    State.IsFrozen = true;
  }
}

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal readonly struct CollisionNode<TKey, TValue>(Hash hash, KeyValuePair<TKey, TValue>[] pairs, bool isFrozen)
{
  private readonly Hash myHash = hash;
  private readonly KeyValuePair<TKey, TValue>[] myPairs = pairs;
  private readonly bool myIsFrozen = isFrozen;
  private bool IsMutable => !myIsFrozen;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Entry<TKey, TValue> AsEntry() => new(myHash, myPairs, myIsFrozen);

  public bool TryGetValue(Hash hash, TKey key, PersistentHashMap2<TKey, TValue>.Comparers comparers, out TValue value)
  {
    if (myHash == hash)
    {
      foreach (var pair in myPairs)
      {
        if (comparers.KeyComparer.Equals(key, pair.Key))
        {
          value = pair.Value;
          return true;
        }
      }
    }

    value = default!;
    return false;
  }
  
  public bool TryGetKey(Hash hash, TKey key, PersistentHashMap2<TKey, TValue>.Comparers comparers, out TKey actualKey)
  {
    if (myHash == hash)
    {
      foreach (var pair in myPairs)
      {
        if (comparers.KeyComparer.Equals(key, pair.Key))
        {
          actualKey = pair.Key;
          return true;
        }
      }
    }

    actualKey = default!;
    return false;
  }

  public Entry<TKey, TValue> Set(Hash hash, Shift shift, in SetArgs<TKey, TValue> args, out int countDiff)
  {
    if (myHash == hash)
    {
      var pairs = myPairs;
      for (var i = 0; i < pairs.Length; i++)
      {
        var pair = pairs[i];
        if (args.Comparers.KeyComparer.Equals(args.Key, pair.Key))
        {
          countDiff = 0;

          switch (args.CollisionBehavior)
          {
            case CollisionBehavior.ThrowIfValueDifferent:
            {
              if (args.Comparers.ValueComparer.Equals(args.Value, pair.Value))
                goto case CollisionBehavior.Skip;

              goto case CollisionBehavior.ThrowAlways;
            }
            case CollisionBehavior.SetValue:
            {
              if (args.Comparers.ValueComparer.Equals(args.Value, pair.Value))
                goto case CollisionBehavior.Skip;

              var newPair = new KeyValuePair<TKey, TValue>(args.Key, args.Value);
              if (IsMutable)
              {
                pairs[i] = newPair;
                return AsEntry();
              }

              return new CollisionNode<TKey, TValue>(hash, pairs.ImmutableUpdate(i, newPair), false).AsEntry();
            }
            case CollisionBehavior.Skip:
              return AsEntry();
            case CollisionBehavior.ThrowAlways:
              throw new ArgumentException("");
            default:
              throw new ArgumentOutOfRangeException();
          }
        }
      }

      countDiff = 1;
      var newPairs = myPairs.ImmutableAdd(new KeyValuePair<TKey, TValue>(args.Key, args.Value));

      return new CollisionNode<TKey, TValue>(hash, newPairs, false).AsEntry();
    }

    countDiff = 1;
    
    var newNode = new CollisionNode<TKey, TValue>(hash, [new KeyValuePair<TKey, TValue>(args.Key, args.Value)], false);
    return SetDifferentHashesRecursively(this, newNode, shift, args).AsEntry();
  }
  
  private static IndexNode<TKey, TValue> SetDifferentHashesRecursively(CollisionNode<TKey, TValue> left, CollisionNode<TKey, TValue> right, Shift shift, SetArgs<TKey, TValue> args)
  {
    var leftBitmapIndex = left.myHash.GetBitmapIndex(shift);
    var rightBitmapIndex = right.myHash.GetBitmapIndex(shift);

    if (leftBitmapIndex.Value == rightBitmapIndex.Value)
    {
      var node = SetDifferentHashesRecursively(left, right, shift.Next(), args);
      var array = Entry<TKey, TValue>.SharedPool.Rent(1);
      array[0] = node.AsEntry();
      return new IndexNode<TKey, TValue>(Bitmap.From(leftBitmapIndex), array, false);
    }

    var nodes = Entry<TKey, TValue>.SharedPool.Rent(2);
    if (leftBitmapIndex.Value < rightBitmapIndex.Value)
    {
      nodes[0] = left.AsEntry();
      nodes[1] = right.AsEntry();
    }
    else
    {
      nodes[0] = right.AsEntry();
      nodes[1] = left.AsEntry();
    }
    
    return new IndexNode<TKey, TValue>(Bitmap.From(leftBitmapIndex, rightBitmapIndex), nodes, false);
  }

  public Entry<TKey, TValue>? Remove(Hash hash, in RemoveArgs<TKey, TValue> args, out int countDiff)
  {
    if (myHash != hash)
    {
      countDiff = 0;
      return AsEntry(); 
    }

    var pairs = myPairs;
    for (var index = 0; index < pairs.Length; index++)
    {
      var pair = pairs[index];
      if (args.Comparers.KeyComparer.Equals(args.Key, pair.Key))
      {
        if (args.Value.HasValue && !args.Comparers.ValueComparer.Equals(args.Value.Value, pair.Value))
        {
          countDiff = 0;
          return AsEntry();
        }

        countDiff = -1;
        if (pairs.Length == 1) return null; // todo return to pool
        
        var newPairs = pairs.ImmutableRemove(index);
        return new CollisionNode<TKey, TValue>(hash, newPairs, false).AsEntry();
      }
    }

    countDiff = 0;
    return AsEntry();
  }
}

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal readonly struct SingleValueNode<TKey, TValue>(Hash hash, KeyValuePair<TKey, TValue> pairs, object boxedPair, bool isFrozen)
{
  private readonly Hash myHash = hash;
  private readonly KeyValuePair<TKey, TValue> myPair = pairs;
  private readonly object myBoxedPair = boxedPair;
  private readonly bool myIsFrozen = isFrozen;
  private bool IsMutable => !myIsFrozen;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Entry<TKey, TValue> AsEntry() => new(myHash, myBoxedPair, myIsFrozen);

  public bool TryGetValue(Hash hash, TKey key, PersistentHashMap2<TKey, TValue>.Comparers comparers, out TValue value)
  {
    if (myHash == hash)
    {
      var pair = myPair;
      if (comparers.KeyComparer.Equals(key, pair.Key))
      {
        value = pair.Value;
        return true;
      }
    }

    value = default!;
    return false;
  }
  
  public bool TryGetKey(Hash hash, TKey key, PersistentHashMap2<TKey, TValue>.Comparers comparers, out TKey actualKey)
  {
    if (myHash == hash)
    {
      var pair = myPair;
      if (comparers.KeyComparer.Equals(key, pair.Key))
      {
        actualKey = pair.Key;
        return true;
      }
    }

    actualKey = default!;
    return false;
  }

  public Entry<TKey, TValue> Set(Hash hash, Shift shift, in SetArgs<TKey, TValue> args, out int countDiff)
  {
    if (myHash == hash)
    {
      var pair = myPair;
      if (args.Comparers.KeyComparer.Equals(args.Key, pair.Key))
      {
        countDiff = 0;

        switch (args.CollisionBehavior)
        {
          case CollisionBehavior.ThrowIfValueDifferent:
          {
            if (args.Comparers.ValueComparer.Equals(args.Value, pair.Value))
              goto case CollisionBehavior.Skip;

            goto case CollisionBehavior.ThrowAlways;
          }
          case CollisionBehavior.SetValue:
          {
            if (args.Comparers.ValueComparer.Equals(args.Value, pair.Value))
              goto case CollisionBehavior.Skip;

            var newPair = new KeyValuePair<TKey, TValue>(args.Key, args.Value);
            return new SingleValueNode<TKey, TValue>(hash, newPair, myBoxedPair, false).AsEntry();
          }
          case CollisionBehavior.Skip:
            return AsEntry();
          case CollisionBehavior.ThrowAlways:
            throw new ArgumentException("");
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      countDiff = 1;
      
      KeyValuePair<TKey, TValue>[] newPairs = [myPair, new(args.Key, args.Value)];
      return new CollisionNode<TKey, TValue>(hash, newPairs, false).AsEntry();
    }

    countDiff = 1;

    var keyValuePair = new KeyValuePair<TKey, TValue>(args.Key, args.Value);
    var newNode = new SingleValueNode<TKey, TValue>(hash, keyValuePair, keyValuePair, false);
    return SetDifferentHashesRecursively(this, newNode, shift, args).AsEntry();
  }
  
  private static IndexNode<TKey, TValue> SetDifferentHashesRecursively(SingleValueNode<TKey, TValue> left, SingleValueNode<TKey, TValue> right, Shift shift, SetArgs<TKey, TValue> args)
  {
    var leftBitmapIndex = left.myHash.GetBitmapIndex(shift);
    var rightBitmapIndex = right.myHash.GetBitmapIndex(shift);

    if (leftBitmapIndex.Value == rightBitmapIndex.Value)
    {
      var node = SetDifferentHashesRecursively(left, right, shift.Next(), args);
      var array = Entry<TKey, TValue>.SharedPool.Rent(1);
      array[0] = node.AsEntry();
      return new IndexNode<TKey, TValue>(Bitmap.From(leftBitmapIndex), array, false);
    }

    var nodes = Entry<TKey, TValue>.SharedPool.Rent(2);
    if (leftBitmapIndex.Value < rightBitmapIndex.Value)
    {
      nodes[0] = left.AsEntry();
      nodes[1] = right.AsEntry();
    }
    else
    {
      nodes[0] = right.AsEntry();
      nodes[1] = left.AsEntry();
    }
    
    return new IndexNode<TKey, TValue>(Bitmap.From(leftBitmapIndex, rightBitmapIndex), nodes, false);
  }
  
  public Entry<TKey, TValue>? Remove(Hash hash, in RemoveArgs<TKey, TValue> args, out int countDiff)
  {
    if (myHash != hash)
    {
      countDiff = 0;
      return AsEntry(); 
    }
    
    var pair = myPair;
    if (args.Comparers.KeyComparer.Equals(args.Key, pair.Key))
    {
      if (args.Value.HasValue && !args.Comparers.ValueComparer.Equals(args.Value.Value, pair.Value))
      {
        countDiff = 0;
        return AsEntry();
      }

      countDiff = -1;
      return null;
    }

    countDiff = 0;
    return AsEntry();
  }
}

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal readonly struct IndexNode<TKey, TValue>(Bitmap bitmap, Entry<TKey, TValue>[] nodes, bool isFrozen)
{
  public static readonly IndexNode<TKey, TValue> Empty = new(default, [], true);
  
  private readonly Bitmap myBitmap = bitmap;
  private readonly IndexNodes<Entry<TKey, TValue>> myNodes = new(nodes);

  private bool IsMutable => !isFrozen;

  public object Value => myNodes;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Entry<TKey, TValue> AsEntry() => new(myBitmap, myNodes.Raw, isFrozen);
  
  public bool TryGetValue(Hash hash, TKey key, Shift shift, PersistentHashMap2<TKey, TValue>.Comparers comparers, out TValue value)
  {
    return TryGetValue(this, hash, key, shift, comparers, out value);
  }

  public bool TryGetKey(Hash hash, TKey key, Shift shift, PersistentHashMap2<TKey, TValue>.Comparers comparers, out TKey actualKey)
  {
    return TryGetKey(this, hash, key, shift, comparers, out actualKey);
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool TryGetValue(IndexNode<TKey, TValue> indexNode, Hash hash, TKey key, Shift shift, PersistentHashMap2<TKey, TValue>.Comparers comparers, out TValue value)
  {
    while (true)
    {
      var bitmapIndex = hash.GetBitmapIndex(shift);
      if (indexNode.myBitmap.IsEmptyAt(bitmapIndex))
      {
        value = default!;
        return false;
      }

      var nodeIndex = indexNode.myBitmap.GetNodePositon(bitmapIndex);
      var node = indexNode.myNodes[nodeIndex];
      switch (node.State.Kind)
      {
        case Kind.IndexNode:
          indexNode = node.AsIndexNode();
          shift = shift.Next();
          break;
        case Kind.SingleValueNode:
          return node.AsSingleValueNode().TryGetValue(hash, key, comparers, out value);
        case Kind.CollisionNode:
          return node.AsCollisionNode().TryGetValue(hash, key, comparers, out value);
        default:
          value = default!;
          return false;
      }
    }
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool TryGetKey(IndexNode<TKey, TValue> indexNode, Hash hash, TKey key, Shift shift, PersistentHashMap2<TKey, TValue>.Comparers comparers, out TKey actualKey)
  {
    while (true)
    {
      var bitmapIndex = hash.GetBitmapIndex(shift);
      if (indexNode.myBitmap.IsEmptyAt(bitmapIndex))
      {
        actualKey = default!;
        return false;
      }

      var nodeIndex = indexNode.myBitmap.GetNodePositon(bitmapIndex);
      var node = indexNode.myNodes[nodeIndex];
      switch (node.State.Kind)
      {
        case Kind.IndexNode:
          indexNode = node.AsIndexNode();
          shift.Next();
          break;
        case Kind.SingleValueNode:
          return node.AsSingleValueNode().TryGetKey(hash, key, comparers, out actualKey);
        case Kind.CollisionNode:
          return node.AsCollisionNode().TryGetKey(hash, key, comparers, out actualKey);
        default:
          actualKey = default!;
          return false;
      }
    }
  }
  
  public IndexNode<TKey, TValue> Set(Hash hash, Shift shift, in SetArgs<TKey, TValue> args, out int countDiff)
  {
    var bitmapIndex = hash.GetBitmapIndex(shift);
    var nodeIndex = myBitmap.GetNodePositon(bitmapIndex);
    if (myBitmap.HasNodeAt(bitmapIndex))
    {
      var node = myNodes[nodeIndex];
      var newNode = node.State.Kind switch
      {
        Kind.IndexNode => node.AsIndexNode().Set(hash, shift.Next(), in args, out countDiff).AsEntry(),
        Kind.SingleValueNode => node.AsSingleValueNode().Set(hash, shift.Next(), in args, out countDiff),
        Kind.CollisionNode => node.AsCollisionNode().Set(hash, shift.Next(), in args, out countDiff),
        _ => throw new ArgumentOutOfRangeException()
      };
      
      if (node.Value == newNode.Value)
        return this;

      if (IsMutable)
      {
        myNodes[nodeIndex] = newNode;
        return this;
      }
      
      return new IndexNode<TKey, TValue>(myBitmap, myNodes.ImmutableUpdate(Entry<TKey, TValue>.SharedPool, nodeIndex, newNode).Raw, false);
    }

    countDiff = 1;

    var keyValuePair = new KeyValuePair<TKey, TValue>(args.Key, args.Value);
    var newNodes = myNodes.ImmutableInsert(Entry<TKey, TValue>.SharedPool, nodeIndex, new SingleValueNode<TKey, TValue>(hash, keyValuePair, keyValuePair,false).AsEntry());
    var newBitmap = myBitmap.MarkNodePresent(bitmapIndex);
    if (IsMutable) Entry<TKey, TValue>.SharedPool.Return(myNodes.Raw);

    return new IndexNode<TKey, TValue>(newBitmap, newNodes.Raw, false);
  }
  
  public IndexNode<TKey, TValue>? Remove(Hash hash, Shift shift, in RemoveArgs<TKey, TValue> args, out int countDiff)
  {
    var bitmapIndex = hash.GetBitmapIndex(shift);
    var nodeIndex = myBitmap.GetNodePositon(bitmapIndex);

    if (myBitmap.HasNodeAt(bitmapIndex))
    {
      var node = myNodes[nodeIndex];
      var nullableNewNode = node.State.Kind switch
      {
        Kind.IndexNode => node.AsIndexNode().Remove(hash, shift.Next(), in args, out countDiff)?.AsEntry(),
        Kind.SingleValueNode => node.AsSingleValueNode().Remove(hash, in args, out countDiff),
        Kind.CollisionNode => node.AsCollisionNode().Remove(hash, in args, out countDiff),
        _ => throw new ArgumentOutOfRangeException()
      };
      if (nullableNewNode.HasValue && nullableNewNode.Value.Value == node.Value)
        return this;

      if (nullableNewNode == null)
      {
        var newBitmap = myBitmap.MarkNodeAbsent(bitmapIndex);
        if (newBitmap.IsEmpty) return null; // todo return to the pool
        
        var newNodes = myNodes.ImmutableRemove(Entry<TKey, TValue>.SharedPool, nodeIndex);
        if (IsMutable) Entry<TKey, TValue>.SharedPool.Return(myNodes.Raw);
        
        return new IndexNode<TKey, TValue>(newBitmap, newNodes.Raw, false);
      }

      var newNode = nullableNewNode.Value;
      if (IsMutable)
      {
        myNodes[nodeIndex] = newNode;
        return this;
      }

      return new IndexNode<TKey, TValue>(myBitmap, myNodes.ImmutableUpdate(Entry<TKey, TValue>.SharedPool, nodeIndex, newNode).Raw, false);
    }

    countDiff = 0;
    return this;
  }

  public IndexNode<TKey, TValue> Freeze()
  {
    if (isFrozen) return this;

    for (var i = 0; i < nodes.Length; i++)
    {
      nodes[i].Freeze();
    }

    return new IndexNode<TKey, TValue>(myBitmap, nodes, true);
  }
}

internal readonly record struct RemoveArgs<TKey, TValue>(
  TKey Key,
  Maybe<TValue> Value,
  PersistentHashMap2<TKey, TValue>.Comparers Comparers);
  
internal readonly record struct SetArgs<TKey, TValue>(
  TKey Key,
  TValue Value,
  PersistentHashMap2<TKey, TValue>.Comparers Comparers,
  CollisionBehavior CollisionBehavior);