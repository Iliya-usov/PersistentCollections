namespace PersistentCollections.Map;

internal sealed class CollisionNode<TKey, TValue>(Hash myHash, KeyValuePair<TKey, TValue>[] pairs) : ValueNodeBase<TKey, TValue>(myHash)
{
  private KeyValuePair<TKey, TValue>[] myPairs = pairs;

  public override bool TryGetValue(Hash hash, TKey key, Shift shift, PersistentHashMap<TKey, TValue>.Comparers comparers, out TValue value)
  {
    if (Hash == hash)
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

  public override bool TryGetKey(Hash hash, TKey key, Shift shift, PersistentHashMap<TKey, TValue>.Comparers comparers, out TKey actualKey)
  {
    if (Hash == hash)
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

  public override Node<TKey, TValue> Set(Hash hash, Shift shift, in SetArgs<TKey, TValue> args, out int countDiff)
  {
    if (Hash == hash)
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
                return this;
              }

              return new CollisionNode<TKey, TValue>(hash, pairs.ImmutableUpdate(i, newPair));
            }
            case CollisionBehavior.Skip:
              return this;
            case CollisionBehavior.ThrowAlways:
              throw new ArgumentException("");
            default:
              throw new ArgumentOutOfRangeException();
          }
        }
      }

      countDiff = 1;
      var newPairs = myPairs.ImmutableAdd(new KeyValuePair<TKey, TValue>(args.Key, args.Value));
      
      if (IsMutable)
      {
        myPairs = newPairs;
        return this;
      }

      return new CollisionNode<TKey, TValue>(hash, newPairs);
    }

    countDiff = 1;

    var newNode = new SingleValueNode<TKey, TValue>(hash, args.Key, args.Value);
    return SetDifferentHashesRecursively(this, newNode, shift, args);
  }

  public override Node<TKey, TValue>? Remove(Hash hash, Shift shift, in RemoveArgs<TKey, TValue> args, out int countDiff)
  {
    if (Hash != hash)
    {
      countDiff = 0;
      return this; 
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
          return this;
        }

        countDiff = -1;
        if (pairs.Length == 1) return null; // todo return to pool
        
        var newPairs = pairs.ImmutableRemove(index);
        if (IsMutable)
        {
          myPairs = newPairs;
          return this;
        }
        
        return new CollisionNode<TKey, TValue>(hash, newPairs);
      }
    }

    countDiff = 0;
    return this;
  }

  public override int CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
  {
    var pairs = myPairs;
    pairs.CopyTo(array, arrayIndex);;
    return pairs.Length;
  }

  public ArrayEnumerator<KeyValuePair<TKey, TValue>> CreateValueEnumerator()
  {
    return new ArrayEnumerator<KeyValuePair<TKey, TValue>>(myPairs);
  }
}