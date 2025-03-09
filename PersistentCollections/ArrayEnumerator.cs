using System.Collections;

namespace PersistentCollections;

internal struct ArrayEnumerator<T>(T[]? pairs) : IEnumerator<T>
{
  private int myIndex = -1;

  public bool MoveNext()
  {
    if (pairs == null) return false;
    
    var index = myIndex + 1;
    if (index >= pairs.Length) return false;
    
    myIndex = index;
    return true;
  }

  public void Reset()
  {
    throw new NotImplementedException();
  }

  public T Current => pairs![myIndex];

  object IEnumerator.Current => Current;

  public void Dispose()
  {
  }
}