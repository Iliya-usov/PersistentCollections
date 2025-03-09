namespace PersistentCollections.Set;

internal sealed class IndexNode<T>(Bitmap bitmap, IndexNodes<Node<T>> nodes, bool isFrozen) : Node<T>
{
  public static IndexNode<T> Empty { get; } = new(default, new IndexNodes<Node<T>>([]), true);

  private Bitmap myBitmap = bitmap;
  private bool myIsFrozen = isFrozen;
  
  public IndexNodes<Node<T>> Nodes = nodes;

  public override bool Contains(Hash hash, T value, Shift shift, IEqualityComparer<T> comparer)
  {
    var bitmapIndex = hash.GetBitmapIndex(shift);
    if (myBitmap.IsEmptyAt(bitmapIndex))
      return false;

    var nodeIndex = myBitmap.GetNodePositon(bitmapIndex);
    var node = Nodes[nodeIndex];
    return node.Contains(hash, value, shift.Next(), comparer);
  }

  public override bool TryGetActualValue(Hash hash, T value, Shift shift, IEqualityComparer<T> comparer, out T actualValue)
  {
    var bitmapIndex = hash.GetBitmapIndex(shift);
    if (myBitmap.IsEmptyAt(bitmapIndex))
    {
      actualValue = default!;
      return false;
    }

    var nodeIndex = myBitmap.GetNodePositon(bitmapIndex);
    var node = Nodes[nodeIndex];
    return node.TryGetActualValue(hash, value, shift.Next(), comparer, out actualValue);
  }

  public override Node<T> Set(Hash hash, Shift shift, in SetArgs<T> args, out int countDiff)
  {
    var bitmapIndex = hash.GetBitmapIndex(shift);
    var nodeIndex = myBitmap.GetNodePositon(bitmapIndex);

    if (myBitmap.HasNodeAt(bitmapIndex))
    {
      var node = Nodes[nodeIndex];
      var newNode = node.Set(hash, shift.Next(), in args, out countDiff);
      if (newNode == node)
        return this;

      // if (owner.IsTheSame(args.MutabilityOwner))
      if (!myIsFrozen)
      {
        Nodes[nodeIndex] = newNode;
        return this;
      }
      
      return new IndexNode<T>(myBitmap, Nodes.ImmutableUpdate(SharedPool, nodeIndex, newNode), false);
    }

    countDiff = 1;

    var newNodes = Nodes.ImmutableInsert(SharedPool, nodeIndex, new SingleValueNode<T>(hash, args.Value));
    var newBitmap = myBitmap.MarkNodePresent(bitmapIndex);
    
    // if (owner.IsTheSame(args.MutabilityOwner))
    if (!myIsFrozen)
    {
      SharedPool.Return(Nodes.Raw);

      myBitmap = newBitmap;
      Nodes = newNodes;
      
      return this;
    }

    return new IndexNode<T>(newBitmap, newNodes, false);
  }
  
  public override IndexNode<T>? Remove(Hash hash, Shift shift, in RemoveArgs<T> args, out int countDiff)
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
        // if (owner.IsTheSame(args.MutabilityOwner))
        if (!myIsFrozen)
        {
          Nodes = newNodes;
          myBitmap = newBitmap;
          return this;
        }
        
        return new IndexNode<T>(newBitmap, newNodes, false);
      }

      // if (owner.IsTheSame(args.MutabilityOwner))
      if (!myIsFrozen)
      {
        Nodes[nodeIndex] = newNode;
        return this;
      }

      return new IndexNode<T>(myBitmap, Nodes.ImmutableUpdate(SharedPool, nodeIndex, newNode), false);
    }

    countDiff = 0;
    return this;
  }

  public override int CopyTo(T[] array, int arrayIndex)
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