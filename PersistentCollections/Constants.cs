namespace PersistentCollections;

internal static class Constants
{
  public const int MaxShiftsCount = 7;
  
  public const int Shift = 5;
  public const int MaxIndexArrayLength = 1 << Shift;
  public const int Mask = MaxIndexArrayLength - 1;
}