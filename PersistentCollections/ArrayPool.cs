namespace PersistentCollections;

internal sealed class ArrayPool<T>
{
  private readonly Stack<T[]>[] myStacks;

  private readonly int myMaxLength;

  public ArrayPool(int maxLength)
  {
    myStacks = new Stack<T[]>[Constants.MaxIndexArrayLength + 1];
    for (var i = 0; i < myStacks.Length; i++)
    {
      myStacks[i] = new Stack<T[]>();
    }
    myMaxLength = maxLength;
  }

  public T[] Rent(int length)
  {
    var stacks = myStacks;
    var stack = stacks[length];
    if (stack.TryPop(out var array))
    {
      return array;
    }
    
    return new T[length];
  }

  public void Return(T[] array)
  {
    var stack = myStacks[array.Length];
    if (stack.Count < myMaxLength)
    {
      Array.Clear(array);
      stack.Push(array);
    }
  }
}