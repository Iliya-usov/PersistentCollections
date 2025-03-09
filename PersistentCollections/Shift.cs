namespace PersistentCollections;

internal readonly record struct Shift(int Value)
{
  public static Shift Zero { get; } = new(0);
  
  public Shift Next() => new(Value + Constants.Shift);
}