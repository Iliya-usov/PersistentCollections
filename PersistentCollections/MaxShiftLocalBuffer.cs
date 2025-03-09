using System.Runtime.CompilerServices;

namespace PersistentCollections;

[InlineArray(Constants.MaxShiftsCount)]
internal struct MaxShiftLocalBuffer<T>
{
  private T myValue;
}