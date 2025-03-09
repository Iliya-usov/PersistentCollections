using System.Collections;
using System.Runtime.CompilerServices;

namespace PersistentCollections.Set;

public struct PersistentHashSetEnumerator<T> : IEnumerator<T>
{
  private IndexNodesLocalStack<IndexNode<T>> myIndexNodesStack;
  private ArrayEnumerator<T> myCurrentArrayEnumerator;

  internal PersistentHashSetEnumerator(IndexNode<T> node)
  {
    myIndexNodesStack = new IndexNodesLocalStack<IndexNode<T>>(node);
    myCurrentArrayEnumerator = default;
    Current = default!;
  }

  public T Current { get; private set; }
  
  public bool MoveNext()
  {
    while (true)
    {
      if (myCurrentArrayEnumerator.MoveNext())
      {
        Current = myCurrentArrayEnumerator.Current;
        return true;
      }

      while (true)
      {
        if (myIndexNodesStack.IsEmpty) return false;
        
        ref var nodeWithIndexRef = ref myIndexNodesStack.PeekRef();
        
        var index = nodeWithIndexRef.Index;
        if (index.Value >= nodeWithIndexRef.Value.Nodes.Length)
        {
          myIndexNodesStack.Pop();
          continue;
        }

        nodeWithIndexRef.IncrementIndex();
        
        var node = nodeWithIndexRef.Value.Nodes[index];
        switch (node)
        {
          case IndexNode<T> mapIndexNode:
            myIndexNodesStack.Push(mapIndexNode);
            continue;
          
          case SingleValueNode<T> mapSingleValueNode:
            Current = mapSingleValueNode.Value;
            return true;
        }
        
        myCurrentArrayEnumerator = Unsafe.As<CollisionNode<T>>(node).CreateValueEnumerator();
        break; // go to the upper loop
      }
    }
  }

  public void Reset()
  {
    throw new NotImplementedException();
  }

  object IEnumerator.Current => Current;

  public void Dispose()
  {
  }
}