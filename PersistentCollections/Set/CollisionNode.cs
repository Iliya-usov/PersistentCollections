namespace PersistentCollections.Set;

internal sealed class CollisionNode<T>(Hash myHash, T[] values, MutabilityOwner myOwner) : ValueNodeBase<T>(myHash)
{
  private T[] myValues = values;

  public override bool Contains(Hash hash, T value, Shift shift, IEqualityComparer<T> comparer)
  {
    if (Hash == hash)
    {
      foreach (var existingValue in myValues)
      {
        if (comparer.Equals(value, existingValue))
        {
          return true;
        }
      }
    }

    return false;
  }

  public override bool TryGetActualValue(Hash hash, T value, Shift shift, IEqualityComparer<T> comparer, out T actualValue)
  {
    if (Hash == hash)
    {
      foreach (var existingValue in myValues)
      {
        if (comparer.Equals(value, existingValue))
        {
          actualValue = existingValue;
          return true;
        }
      }
    }

    actualValue = default!;
    return false;
  }

  public override Node<T> Set(Hash hash, Shift shift, in SetArgs<T> args, out int countDiff)
  {
    if (Hash == hash)
    {
      var values = myValues;
      for (var i = 0; i < values.Length; i++)
      {
        var value = values[i];
        if (args.Comparer.Equals(args.Value, value))
        {
          countDiff = 0;

          if (myOwner.IsTheSame(args.MutabilityOwner))
          {
            values[i] = args.Value;
            return this;
          }

          return new CollisionNode<T>(hash, values.ImmutableUpdate(i, args.Value), args.MutabilityOwner);
        }
      }

      countDiff = 1;
      var newValues = myValues.ImmutableAdd(args.Value);
      
      if (myOwner.IsTheSame(args.MutabilityOwner))
      {
        myValues = newValues;
        return this;
      }

      return new CollisionNode<T>(hash, newValues, args.MutabilityOwner);
    }

    countDiff = 1;

    var newNode = new SingleValueNode<T>(hash, args.Value);
    return SetDifferentHashesRecursively(this, newNode, shift, args);
  }

  public override Node<T>? Remove(Hash hash, Shift shift, in RemoveArgs<T> args, out int countDiff)
  {
    if (Hash != hash)
    {
      countDiff = 0;
      return this; 
    }

    var values = myValues;
    for (var index = 0; index < values.Length; index++)
    {
      var value = values[index];
      if (args.Comparer.Equals(args.Value, value))
      {
        countDiff = -1;

        if (values.Length == 1) return null; 
        
        var newValues = values.ImmutableRemove(index);
        if (myOwner.IsTheSame(args.MutabilityOwner))
        {
          myValues = newValues;
          return this;
        }
        
        return new CollisionNode<T>(hash, newValues, args.MutabilityOwner);
      }
    }

    countDiff = 0;
    return this;
  }

  public override int CopyTo(T[] array, int arrayIndex)
  {
    var values = myValues;
    values.CopyTo(array, arrayIndex);;
    return values.Length;
  }

  public ArrayEnumerator<T> CreateValueEnumerator()
  {
    return new ArrayEnumerator<T>(myValues);
  }
}