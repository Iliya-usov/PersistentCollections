namespace PersistentCollections.Set;

internal abstract class ValueNodeBase<T>(Hash hash) : Node<T>
{
  public readonly Hash Hash = hash;
  
  protected static Node<T> SetDifferentHashesRecursively(ValueNodeBase<T> left, ValueNodeBase<T> right, Shift shift, SetArgs<T> args)
  {
    var leftBitmapIndex = left.Hash.GetBitmapIndex(shift);
    var rightBitmapIndex = right.Hash.GetBitmapIndex(shift);

    if (leftBitmapIndex.Value == rightBitmapIndex.Value)
    {
      var node = SetDifferentHashesRecursively(left, right, shift.Next(), args);
      var array = SharedPool.Rent(1);
      array[0] = node;
      return new IndexNode<T>(Bitmap.From(leftBitmapIndex), new IndexNodes<Node<T>>(array), false);
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
    
    return new IndexNode<T>(Bitmap.From(leftBitmapIndex, rightBitmapIndex), new IndexNodes<Node<T>>(nodes), false);
  }
}