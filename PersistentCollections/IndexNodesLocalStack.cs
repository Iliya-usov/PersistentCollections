using System.Diagnostics.CodeAnalysis;

namespace PersistentCollections;

internal struct IndexNodesLocalStack<T>
{
  private MaxShiftLocalBuffer<ValueWithIndex<T>> myBuffer;
  private int myIndex;
  
  public bool IsEmpty => myIndex < 0;

  public IndexNodesLocalStack(T value)
  {
    myBuffer[0] = new ValueWithIndex<T>(value);
    myIndex = 0;
  }

  public void Push(T value)
  {
    if (myIndex >= Constants.MaxShiftsCount) throw new InvalidOperationException();

    myBuffer[++myIndex] = new ValueWithIndex<T>(value);
  }

  [UnscopedRef]
  public ref ValueWithIndex<T> PeekRef()
  {
    return ref myBuffer[myIndex];
  }

  public void Pop()
  {
    if (IsEmpty) 
      throw new InvalidOperationException();

    --myIndex;
  }
}