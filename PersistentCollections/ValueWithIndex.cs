namespace PersistentCollections;

internal struct ValueWithIndex<T>(T value)
{
  public T Value = value;
  public int RawIndex;
  public NodeIndex Index => new(RawIndex);
  
  public void IncrementIndex() => RawIndex++;
}