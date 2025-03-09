namespace PersistentCollections.Map;

internal abstract class ValueNodeBase<TKey, TValue>(Hash hash) : Node<TKey, TValue>(false)
{
  public readonly Hash Hash = hash;

  public override void Freeze()
  {
    IsFrozen = true;
  }

  protected static Node<TKey, TValue> SetDifferentHashesRecursively(ValueNodeBase<TKey, TValue> left, ValueNodeBase<TKey, TValue> right, Shift shift, SetArgs<TKey, TValue> args)
  {
    var leftBitmapIndex = left.Hash.GetBitmapIndex(shift);
    var rightBitmapIndex = right.Hash.GetBitmapIndex(shift);

    if (leftBitmapIndex.Value == rightBitmapIndex.Value)
    {
      var node = SetDifferentHashesRecursively(left, right, shift.Next(), args);
      var array = SharedPool.Rent(1);
      array[0] = node;
      return new IndexNode<TKey, TValue>(Bitmap.From(leftBitmapIndex), new IndexNodes<Node<TKey, TValue>>(array));
    }

    var nodes = SharedPool.Rent(2);
    if (leftBitmapIndex.Value < rightBitmapIndex.Value)
    {
      nodes[0] = left;
      nodes[1] = right;
    }
    else
    {
      nodes[0] = right;
      nodes[1] = left;
    }
    
    return new IndexNode<TKey, TValue>(Bitmap.From(leftBitmapIndex, rightBitmapIndex), new IndexNodes<Node<TKey, TValue>>(nodes));
  }
}