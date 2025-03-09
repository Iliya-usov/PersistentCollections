using System.Collections;
using System.Runtime.CompilerServices;

namespace PersistentCollections.Map;

public struct PersistentHashMapEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
{
  private IndexNodesLocalStack<IndexNode<TKey, TValue>> myIndexNodesStack;
  private ArrayEnumerator<KeyValuePair<TKey, TValue>> myCurrentArrayEnumerator;

  internal PersistentHashMapEnumerator(IndexNode<TKey, TValue> node)
  {
    myIndexNodesStack = new IndexNodesLocalStack<IndexNode<TKey, TValue>>(node);
    myCurrentArrayEnumerator = default;
    Current = default!; 
  }

  public KeyValuePair<TKey, TValue> Current { get; private set; }
  
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
          case IndexNode<TKey, TValue> mapIndexNode:
            myIndexNodesStack.Push(mapIndexNode);
            continue;
          
          case SingleValueNode<TKey, TValue> mapSingleValueNode:
            Current = new KeyValuePair<TKey, TValue>(mapSingleValueNode.Key, mapSingleValueNode.Value);
            return true;
        }
        
        myCurrentArrayEnumerator = Unsafe.As<CollisionNode<TKey, TValue>>(node).CreateValueEnumerator();
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