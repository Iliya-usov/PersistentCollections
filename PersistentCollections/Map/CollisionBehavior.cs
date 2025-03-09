namespace PersistentCollections.Map;

internal enum CollisionBehavior
{
  /// <summary>
  /// Sets the value for the given key, even if that overwrites an existing value.
  /// </summary>
  SetValue,

  /// <summary>
  /// Skips the mutating operation if a key conflict is detected.
  /// </summary>
  Skip,

  /// <summary>
  /// Throw an exception if the key already exists with a different key.
  /// </summary>
  ThrowIfValueDifferent,

  /// <summary>
  /// Throw an exception if the key already exists regardless of its value.
  /// </summary>
  ThrowAlways,
}