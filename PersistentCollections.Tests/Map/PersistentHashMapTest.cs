using System;
using NUnit.Framework;
using PersistentCollections.Map;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Legacy;
using PersistentCollections.Tests.Utils;

namespace PersistentCollections.Tests.Map;

[TestFixture]
[TestOf(typeof(PersistentHashMap<,>))]
public class PersistentHashMapTest
{
    [Test]
    public void Empty_HasCorrectCount()
    {
        var map = PersistentHashMap<int, string>.Empty;
        Assert.That(map.Count, Is.EqualTo(0));
    }

    [Test]
    public void Add_AddsKeyValuePair()
    {
        var map = PersistentHashMap<int, string>.Empty;
        var updatedMap = map.Add(1, "Value1");

        Assert.That(updatedMap.Count, Is.EqualTo(1));
        Assert.That(updatedMap.ContainsKey(1), Is.True);
        Assert.That(updatedMap[1], Is.EqualTo("Value1"));
    }

    [Test]
    public void Add_DoesNotMutateOriginalInstance()
    {
        var map = PersistentHashMap<int, string>.Empty;
        var updatedMap = map.Add(1, "Value1");

        Assert.That(map.ContainsKey(1), Is.False);
        Assert.That(map.Count, Is.Not.EqualTo(updatedMap.Count));
    }

    [Test]
    public void SetItem_UpdatesValueForExistingKey()
    {
        var map = PersistentHashMap<int, string>.Empty.Add(1, "Value1");
        var updatedMap = map.SetItem(1, "UpdatedValue");

        Assert.That(updatedMap.Count, Is.EqualTo(1));
        Assert.That(updatedMap[1], Is.EqualTo("UpdatedValue"));
    }

    [Test]
    public void SetItem_AddsNewKeyIfNotExist()
    {
        var map = PersistentHashMap<int, string>.Empty.Add(1, "Value1");
        var updatedMap = map.SetItem(2, "Value2");

        Assert.That(updatedMap.Count, Is.EqualTo(2));
        Assert.That(updatedMap[2], Is.EqualTo("Value2"));
    }

    [Test]
    public void Remove_RemovesKeyValuePair()
    {
        var map = PersistentHashMap<int, string>.Empty
            .Add(1, "Value1")
            .Add(2, "Value2");
        var updatedMap = map.Remove(1);

        Assert.That(updatedMap.Count, Is.EqualTo(1));
        Assert.That(updatedMap.ContainsKey(1), Is.False);
    }

    [Test]
    public void Remove_DoesNotMutateOriginalInstance()
    {
        var map = PersistentHashMap<int, string>.Empty.Add(1, "Value1");
        var updatedMap = map.Remove(1);

        Assert.That(map.ContainsKey(1), Is.True);
        Assert.That(map.Count, Is.Not.EqualTo(updatedMap.Count));
    }

    [Test]
    public void TryGetValue_ReturnsTrueIfKeyExists()
    {
        var map = PersistentHashMap<int, string>.Empty.Add(1, "Value1");
        var result = map.TryGetValue(1, out var value);

        Assert.That(result, Is.True);
        Assert.That(value, Is.EqualTo("Value1"));
    }

    [Test]
    public void TryGetValue_ReturnsFalseIfKeyNotExist()
    {
        var map = PersistentHashMap<int, string>.Empty;
        var result = map.TryGetValue(1, out var value);

        Assert.That(result, Is.False);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void ContainsKey_ReturnsTrueIfKeyExists()
    {
        var map = PersistentHashMap<int, string>.Empty.Add(1, "Value1");
        Assert.That(map.ContainsKey(1), Is.True);
    }

    [Test]
    public void ContainsKey_ReturnsFalseIfKeyNotExist()
    {
        var map = PersistentHashMap<int, string>.Empty;
        Assert.That(map.ContainsKey(1), Is.False);
    }

    [Test]
    public void Clear_RemovesAllItems()
    {
        var map = PersistentHashMap<int, string>.Empty.Add(1, "Value1").Add(2, "Value2");
        var clearedMap = map.Clear();

        Assert.That(clearedMap.Count, Is.EqualTo(0));
    }

    [Test]
    public void Keys_ReturnsAllKeys()
    {
        var map = PersistentHashMap<int, string>.Empty
            .Add(1, "Value1")
            .Add(2, "Value2")
            .Add(3, "Value3");

        var keys = map.Keys.ToList();
        Assert.That(keys.Count, Is.EqualTo(3));
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, keys);
    }

    [Test]
    public void Values_ReturnsAllValues()
    {
        var map = PersistentHashMap<int, string>.Empty
            .Add(1, "Value1")
            .Add(2, "Value2")
            .Add(3, "Value3");

        var values = map.Values.ToList();
        Assert.That(values.Count, Is.EqualTo(3));
        CollectionAssert.AreEquivalent(new[] { "Value1", "Value2", "Value3" }, values);
    }

    [Test]
    public void AddRange_AddsMultipleKeyValuePairs()
    {
        var map = PersistentHashMap<int, string>.Empty;
        var updatedMap = map.AddRange([
            new KeyValuePair<int, string>(1, "Value1"),
            new KeyValuePair<int, string>(2, "Value2")
        ]);

        Assert.That(updatedMap.Count, Is.EqualTo(2));
        Assert.That(updatedMap[1], Is.EqualTo("Value1"));
        Assert.That(updatedMap[2], Is.EqualTo("Value2"));
    }

    [Test]
    public void RemoveRange_RemovesSpecifiedKeys()
    {
        var map = PersistentHashMap<int, string>.Empty
            .Add(1, "Value1")
            .Add(2, "Value2")
            .Add(3, "Value3");
        var updatedMap = map.RemoveRange([1, 2]);

        Assert.That(updatedMap.Count, Is.EqualTo(1));
        Assert.That(updatedMap.ContainsKey(3), Is.True);
    }

    [Test]
    public void SetItems_ReplacesExistingKeyAndAddsNewKeys()
    {
        var map = PersistentHashMap<int, string>.Empty
            .Add(1, "Value1")
            .Add(2, "Value2");
        var updatedMap = map.SetItems([
            new KeyValuePair<int, string>(1, "UpdatedValue1"),
            new KeyValuePair<int, string>(3, "Value3")
        ]);

        Assert.That(updatedMap.Count, Is.EqualTo(3));
        Assert.That(updatedMap[1], Is.EqualTo("UpdatedValue1"));
        Assert.That(updatedMap[3], Is.EqualTo("Value3"));
    }

    [Test]
    public void WithComparers_ChangesComparers()
    {
        var customKeyComparer = StringComparer.OrdinalIgnoreCase;
        var customValueComparer = EqualityComparer<string>.Default;

        var map = PersistentHashMap<string, string>.Empty.WithComparers(customKeyComparer, customValueComparer);
        var updatedMap = map.Add("key", "value");

        Assert.That(updatedMap.ContainsKey("KEY"), Is.True);
    }
    
    [Test]
    public void Remove_WithValue_RemovesOnlyIfValueMatches()
    {
        var map = PersistentHashMap<int, string>.Empty.Add(1, "Value1");
        var updatedMap = map.Remove(1, "WrongValue");
    
        Assert.That(map.Count, Is.EqualTo(updatedMap.Count));
        Assert.That(updatedMap.ContainsKey(1), Is.True);
    
        updatedMap = map.Remove(1, "Value1");
        Assert.That(updatedMap.ContainsKey(1), Is.False);
    }
    
    [Test]
    public void Contains_ChecksKeyValuePair()
    {
        var map = PersistentHashMap<int, string>.Empty.Add(1, "Value1");
        
        Assert.That(map.Contains(new KeyValuePair<int, string>(1, "Value1")), Is.True);
        Assert.That(map.Contains(new KeyValuePair<int, string>(1, "WrongValue")), Is.False);
        Assert.That(map.Contains(new KeyValuePair<int, string>(2, "Value1")), Is.False);
    }
    
    [Test]
    public void Indexer_ThrowsOnMissingKey()
    {
        var map = PersistentHashMap<int, string>.Empty;
        
        Assert.Throws<KeyNotFoundException>(() => _ = map[1]);
    }
    
    [Test]
    public void LargeScale_Add_Operations()
    {
        const int itemCount = 1000;
        var random = new Random(42);
        var map = PersistentHashMap<int, string>.Empty;
        var referenceDict = new Dictionary<int, string>();
        
        for (var i = 0; i < itemCount; i++)
        {
            var key = random.Next();
            var value = $"Value{i}";
            if (!referenceDict.ContainsKey(key))
            {
                map = map.Add(key, value);
                referenceDict.Add(key, value);
            }
        }
        
        Assert.That(map.Count, Is.EqualTo(referenceDict.Count));
        foreach (var (key, value) in referenceDict)
        {
            Assert.That(map[key], Is.EqualTo(value));
        }
    }
    
    [Test]
    public void LargeScale_Remove_Operations()
    {
        const int itemCount = 1000;
        var random = new Random(42);
        var map = PersistentHashMap<int, string>.Empty;
        var referenceDict = new Dictionary<int, string>();
        var keys = new List<int>();
        
        for (var i = 0; i < itemCount; i++)
        {
            var key = random.Next();
            var value = $"Value{i}";
            if (!referenceDict.ContainsKey(key))
            {
                map = map.Add(key, value);
                referenceDict.Add(key, value);
                keys.Add(key);
            }
        }
        
        foreach (var key in keys.Take(itemCount / 2))
        {
            map = map.Remove(key);
            referenceDict.Remove(key);
        }
        
        Assert.That(map.Count, Is.EqualTo(referenceDict.Count));
        foreach (var (key, value) in referenceDict)
        {
            Assert.That(map[key], Is.EqualTo(value));
        }
    }
    
    [Test]
    public void LargeScale_SetItem_Operations()
    {
        const int itemCount = 1000;
        var random = new Random(42);
        var map = PersistentHashMap<int, string>.Empty;
        var referenceDict = new Dictionary<int, string>();
        
        for (var i = 0; i < itemCount; i++)
        {
            var key = random.Next(itemCount / 2); // Ensure some key collisions
            var value = $"Value{i}";
            map = map.SetItem(key, value);
            referenceDict[key] = value;
        }
        
        Assert.That(map.Count, Is.EqualTo(referenceDict.Count));
        foreach (var (key, value) in referenceDict)
        {
            Assert.That(map[key], Is.EqualTo(value));
        }
    }
    
    [Test]
    public void WithComparers_HandlesCollisions()
    {
        var customKeyComparer = StringComparer.OrdinalIgnoreCase;
        var customValueComparer = EqualityComparer<string>.Default;
    
        var map = PersistentHashMap<string, string>.Empty
            .WithComparers(customKeyComparer, customValueComparer)
            .Add("key", "value1");
    
        Assert.Throws<ArgumentException>(() => map.Add("KEY", "value2"));
        Assert.That(map["KEY"], Is.EqualTo("value1"));
    }

    [Test]
    public void Enumerator_IteratesOverKeyValuePairs()
    {
        var map = PersistentHashMap<int, string>.Empty
            .Add(1, "Value1")
            .Add(2, "Value2");
        var elements = map.ToList();

        Assert.That(elements.Count, Is.EqualTo(2));
        CollectionAssert.Contains(elements, new KeyValuePair<int, string>(1, "Value1"));
        CollectionAssert.Contains(elements, new KeyValuePair<int, string>(2, "Value2"));
    }

    [Test]
    public void Enumeration_Collision()
    {
        var comparer = new EqualityComparerWrapper<int>(
            (l, r) => l == r,
            o =>
            {
                o = (o & int.MaxValue) % 32;
                return 1 << o;
            });

        var map = PersistentHashMap<int, int>.Empty
            .WithComparers(comparer, comparer);

        for (int i = 0; i < 32; i++)
        {
           map = map.Add(i, i);
        }

        var list = map.Select(x => x.Key).ToList();
        list.Sort();
        Assert.That(list, Is.EqualTo(Enumerable.Range(0, 32).ToList()));
    }
}