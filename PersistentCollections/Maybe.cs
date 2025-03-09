namespace PersistentCollections;

internal readonly struct Maybe<T>
{
  public T Value { get; }
  public bool HasValue { get; }

  public Maybe(T value)
  {
    Value = value;
    HasValue = true;
  }

  public static Maybe<T> Null => default;
}