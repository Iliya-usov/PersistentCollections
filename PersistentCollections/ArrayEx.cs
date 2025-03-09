namespace PersistentCollections;

public static class ArrayEx
{
  public static T[] ImmutableAdd<T>(this T[] array, T value)
  {
    var newArray = new T[array.Length + 1];
    Array.Copy(array, newArray, array.Length);
    newArray[array.Length] = value;
    return newArray;
  }

  public static T[] ImmutableInsert<T>(this T[] array, int index, T value)
  {
    var newArray = new T[array.Length + 1];
    CopeAndInsertTo(array, newArray, index, value);
    return newArray;
  }

  public static void CopeAndInsertTo<T>(this T[] array, T[] destination, int index, T value)
  {
    Array.Copy(array, destination, index);
    destination[index] = value;
    Array.Copy(array, index, destination, index + 1, array.Length - index);
  }

  public static T[] ImmutableRemove<T>(this T[] array, int index)
  {
    var newArray = new T[array.Length - 1];
    return CopyAndRemoveTo(array, newArray, index);
  }

  public static T[] CopyAndRemoveTo<T>(this T[] array, T[] newArray, int index)
  {
    Array.Copy(array, newArray, index);
    Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
    return newArray;
  }

  public static T[] ImmutableUpdate<T>(this T[] array, int index, T value)
  {
    var newArray = new T[array.Length];
    return CopyAndSetTo(array, newArray, index, value);
  }

  public static T[] CopyAndSetTo<T>(this T[] array, T[] newArray, int index, T value)
  {
    Array.Copy(array, newArray, array.Length);
    newArray[index] = value;
    return newArray;
  }
}