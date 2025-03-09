namespace PersistentCollections.Set;

internal class SingleValueNode<T>(Hash myHash, T value) : ValueNodeBase<T>(myHash)
{
  public T Value = value;

  public override bool Contains(Hash hash, T value, Shift shift, IEqualityComparer<T> comparer)
  {
    return Hash == hash && comparer.Equals(Value, value);
  }

  public override bool TryGetActualValue(Hash hash, T value, Shift shift, IEqualityComparer<T> comparer, out T actualValue)
  {
    if (Hash == hash && comparer.Equals(Value, value))
    {
      actualValue = Value;
      return true;
    }
    
    actualValue = default!;
    return false;
  }

  public override Node<T> Set(Hash hash, Shift shift, in SetArgs<T> args, out int countDiff)
  {
    if (Hash == hash)
    {
      if (args.Comparer.Equals(args.Value, Value))
      {
        countDiff = 0;
        return this;
      }

      countDiff = 1;
      return new CollisionNode<T>(hash, [Value, args.Value], args.MutabilityOwner);
    }
    
    countDiff = 1;
    return SetDifferentHashesRecursively(this, new SingleValueNode<T>(hash, args.Value), shift, args);
  }

  public override Node<T>? Remove(Hash hash, Shift shift, in RemoveArgs<T> args, out int countDiff)
  {
    if (Hash == hash && args.Comparer.Equals(Value, args.Value))
    {
      countDiff = -1;
      return null;
    }
    
    countDiff = 0;
    return this;
  }

  public override int CopyTo(T[] array, int arrayIndex)
  {
    array[arrayIndex] = Value;
    return 1;
  }
}