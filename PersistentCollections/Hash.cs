namespace PersistentCollections;

internal readonly record struct Hash(int Value)
{
  public BitmapIndex GetBitmapIndex(Shift shift) => new((Value >> shift.Value) & Constants.Mask);
}