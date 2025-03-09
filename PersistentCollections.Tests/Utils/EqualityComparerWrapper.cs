using System;
using System.Collections.Generic;

namespace PersistentCollections.Tests.Utils;

public class EqualityComparerWrapper<T>(Func<T, T, bool> equals, Func<T, int> hash) : IEqualityComparer<T>
{
  public bool Equals(T x, T y) => equals(x, y);
  public int GetHashCode(T obj) => hash(obj);
}