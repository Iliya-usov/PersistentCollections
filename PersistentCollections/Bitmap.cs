using System.Numerics;
using System.Runtime.CompilerServices;

namespace PersistentCollections;

internal readonly struct Bitmap(int myValue)
{
  public int Value => myValue;
  
  public bool IsEmpty => myValue == 0;
  
  public NodeIndex GetNodePositon(BitmapIndex bitmapIndex)
  {
    var mask = (1 << bitmapIndex.Value) - 1;
    return new(BitOperations.PopCount((uint)(myValue & mask)));
  }

  public bool HasNodeAt(BitmapIndex bitmapIndex) => (myValue & (1 << bitmapIndex.Value)) != 0;
  public bool IsEmptyAt(BitmapIndex bitmapIndex) => !HasNodeAt(bitmapIndex);
  
  public Bitmap MarkNodePresent(BitmapIndex bitmapIndex) => new(myValue | (1 << bitmapIndex.Value));
  public Bitmap MarkNodeAbsent(BitmapIndex bitmapIndex) => new(myValue & ~(1 << bitmapIndex.Value));

  public static Bitmap From(BitmapIndex index1, BitmapIndex index2) => new(1 << index1.Value | 1 << index2.Value);
  public static Bitmap From(BitmapIndex index) => new(1 << index.Value);
}