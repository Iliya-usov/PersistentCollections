namespace PersistentCollections.Map;

internal sealed class IndexNode<TKey, TValue>(Bitmap bitmap, IndexNodes<Node<TKey, TValue>> nodes, bool isFrozen = false) : Node<TKey, TValue>(isFrozen)
{
  public static IndexNode<TKey, TValue> Empty { get; } = new(default, new IndexNodes<Node<TKey, TValue>>([]), true);

  private Bitmap myBitmap = bitmap;
  
  public IndexNodes<Node<TKey, TValue>> Nodes = nodes;

  public override void Freeze()
  {
    if (IsFrozen) return; // do not need to travers subtrees

    foreach (var node in Nodes.Raw)
    {
      node.Freeze();
    }

    IsFrozen = true;
  }

  public override bool TryGetValue(Hash hash, TKey key, Shift shift, PersistentHashMap<TKey, TValue>.Comparers comparers, out TValue value)
  {
    var bitmapIndex = hash.GetBitmapIndex(shift);
    if (myBitmap.IsEmptyAt(bitmapIndex))
    {
      value = default!;
      return false;
    }

    var nodeIndex = myBitmap.GetNodePositon(bitmapIndex);
    var node = Nodes[nodeIndex];
    return node.TryGetValue(hash, key, shift.Next(), comparers, out value);
  }

  public override bool TryGetKey(Hash hash, TKey key, Shift shift, PersistentHashMap<TKey, TValue>.Comparers comparers, out TKey actualKey)
  {
    var bitmapIndex = hash.GetBitmapIndex(shift);
    if (myBitmap.IsEmptyAt(bitmapIndex))
    {
      actualKey = default!;
      return false;
    }

    var nodeIndex = myBitmap.GetNodePositon(bitmapIndex);
    var node = Nodes[nodeIndex];
    return node.TryGetKey(hash, key, shift.Next(), comparers, out actualKey);
  }

  public override IndexNode<TKey, TValue> Set(Hash hash, Shift shift, in SetArgs<TKey, TValue> args, out int countDiff)
  {
    var bitmapIndex = hash.GetBitmapIndex(shift);
    var nodeIndex = myBitmap.GetNodePositon(bitmapIndex);

    if (myBitmap.HasNodeAt(bitmapIndex))
    {
      var node = Nodes[nodeIndex];
      var newNode = node.Set(hash, shift.Next(), in args, out countDiff);
      if (newNode == node)
        return this;

      if (IsMutable)
      {
        Nodes[nodeIndex] = newNode;
        return this;
      }
      
      return new IndexNode<TKey, TValue>(myBitmap, Nodes.ImmutableUpdate(SharedPool, nodeIndex, newNode));
    }

    countDiff = 1;

    var newNodes = Nodes.ImmutableInsert(SharedPool, nodeIndex, new SingleValueNode<TKey, TValue>(hash, args.Key, args.Value));
    var newBitmap = myBitmap.MarkNodePresent(bitmapIndex);
    
    if (IsMutable)
    {
      SharedPool.Return(Nodes.Raw);

      myBitmap = newBitmap;
      Nodes = newNodes;
      
      return this;
    }

    return new IndexNode<TKey, TValue>(newBitmap, newNodes);
  }
  
  public override IndexNode<TKey, TValue>? Remove(Hash hash, Shift shift, in RemoveArgs<TKey, TValue> args, out int countDiff)
  {
    var bitmapIndex = hash.GetBitmapIndex(shift);
    var nodeIndex = myBitmap.GetNodePositon(bitmapIndex);

    if (myBitmap.HasNodeAt(bitmapIndex))
    {
      var node = Nodes[nodeIndex];
      var newNode = node.Remove(hash, shift.Next(), in args, out countDiff);
      if (newNode == node)
        return this;

      if (newNode == null)
      {
        var newBitmap = myBitmap.MarkNodeAbsent(bitmapIndex);
        if (newBitmap.IsEmpty) return null; // todo return to the pool
        
        var newNodes = Nodes.ImmutableRemove(SharedPool, nodeIndex);
        if (IsMutable)
        {
          SharedPool.Return(Nodes.Raw);
          
          Nodes = newNodes;
          myBitmap = newBitmap;
          return this;
        }
        
        return new IndexNode<TKey, TValue>(newBitmap, newNodes);
      }

      if (IsMutable)
      {
        Nodes[nodeIndex] = newNode;
        return this;
      }

      return new IndexNode<TKey, TValue>(myBitmap, Nodes.ImmutableUpdate(SharedPool, nodeIndex, newNode));
    }

    countDiff = 0;
    return this;
  }

  public override int CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
  {
    var count = 0;
    // ReSharper disable once LoopCanBeConvertedToQuery
    foreach (var node in Nodes.Raw)
    {
      count += node.CopyTo(array, arrayIndex + count);
    }

    return count;
  }
}