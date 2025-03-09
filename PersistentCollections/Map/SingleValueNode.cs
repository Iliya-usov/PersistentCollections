namespace PersistentCollections.Map;

internal sealed class SingleValueNode<TKey, TValue>(Hash myHash, TKey key, TValue value) : ValueNodeBase<TKey, TValue>(myHash)
{
  public TKey Key = key;
  public TValue Value = value;

  public override bool TryGetValue(Hash hash, TKey key, Shift shift, PersistentHashMap<TKey, TValue>.Comparers comparers, out TValue value)
  {
    if (Hash == hash && comparers.KeyComparer.Equals(key, Key))
    {
      value = Value;
      return true;
    }
    
    value = default!;
    return false;
  }

  public override bool TryGetKey(Hash hash, TKey key, Shift shift, PersistentHashMap<TKey, TValue>.Comparers comparers, out TKey actualKey)
  {
    if (Hash == hash && comparers.KeyComparer.Equals(key, Key))
    {
      actualKey = Key;
      return true;
    }
    
    actualKey = default!;
    return false;
  }

  public override Node<TKey, TValue> Set(Hash hash, Shift shift, in SetArgs<TKey, TValue> args, out int countDiff)
  {
    if (Hash == hash)
    {
      if (args.Comparers.KeyComparer.Equals(args.Key, Key))
      {
        countDiff = 0;

        switch (args.CollisionBehavior)
        {
          case CollisionBehavior.ThrowIfValueDifferent:
          {
            if (args.Comparers.ValueComparer.Equals(args.Value, Value))
              goto case CollisionBehavior.Skip;

            goto case CollisionBehavior.ThrowAlways;
          }
          case CollisionBehavior.SetValue:
          {
            if (args.Comparers.ValueComparer.Equals(args.Value, Value))
              goto case CollisionBehavior.Skip;

            var newPair = new KeyValuePair<TKey, TValue>(args.Key, args.Value);
            if (IsMutable)
            {
              (Key, Value) = newPair;
              return this;
            }

            return new SingleValueNode<TKey, TValue>(hash, args.Key, args.Value);
          }
          case CollisionBehavior.Skip:
            return this;
          case CollisionBehavior.ThrowAlways:
            throw new ArgumentException("");
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      countDiff = 1;
      return new CollisionNode<TKey, TValue>(hash, [new KeyValuePair<TKey, TValue>(Key, Value), new KeyValuePair<TKey, TValue>(args.Key, args.Value)]);
    }
    
    countDiff = 1;
    return SetDifferentHashesRecursively(this, new SingleValueNode<TKey, TValue>(hash, args.Key, args.Value), shift, args);
  }

  public override Node<TKey, TValue>? Remove(Hash hash, Shift shift, in RemoveArgs<TKey, TValue> args, out int countDiff)
  {
    if (Hash == hash && args.Comparers.KeyComparer.Equals(args.Key, Key))
    {
      if (args.Value.HasValue && !args.Comparers.ValueComparer.Equals(args.Value.Value, Value))
      {
        countDiff = 0;
        return this;
      }

      countDiff = -1;
      return null;
    }
    
    countDiff = 0;
    return this;
  }

  public override int CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
  {
    array[arrayIndex] = new KeyValuePair<TKey, TValue>(Key, Value);
    return 1;
  }
}