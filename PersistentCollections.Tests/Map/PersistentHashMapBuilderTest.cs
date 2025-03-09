using NUnit.Framework;
using PersistentCollections.Map;
using System;
using System.Collections.Generic;

namespace PersistentCollections.Tests.Map;

[TestFixture]
[TestOf(typeof(PersistentHashMapBuilder<,>))]
public class PersistentHashMapBuilderTest
{
    [Test]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        var map = PersistentHashMap<string, string>.Empty;
        var builder = map.ToBuilder();

        Assert.That(builder.Count, Is.EqualTo(0));
        Assert.That(builder.IsReadOnly, Is.False);
    }

    [Test]
    public void Indexer_Get_ShouldReturnExistingValue()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        Assert.That(builder["key1"], Is.EqualTo("value1"));
    }

    [Test]
    public void Indexer_Set_ShouldAddOrUpdateValue()
    {
        var builder = CreateBuilder();
        builder["key1"] = "value1";
        Assert.That(builder["key1"], Is.EqualTo("value1"));

        builder["key1"] = "value2";
        Assert.That(builder["key1"], Is.EqualTo("value2"));
    }

    [Test]
    public void Indexer_Get_ShouldThrowKeyNotFoundExceptionForNonExistentKey()
    {
        var builder = CreateBuilder();
        Assert.That(() =>
        {
            var _ = builder["nonexistent"];
        }, Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void Contains_ShouldReturnTrueForExistingKeyValuePair()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        Assert.That(builder.Contains(new KeyValuePair<string, string>("key1", "value1")), Is.True);
    }

    [Test]
    public void Contains_ShouldReturnFalseForNonExistingKeyValuePair()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        Assert.That(builder.Contains(new KeyValuePair<string, string>("key1", "value2")), Is.False);
    }

    [Test]
    public void TryGetValue_ShouldReturnTrueAndOutValueForExistingKey()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        Assert.That(builder.TryGetValue("key1", out var value), Is.True);
        Assert.That(value, Is.EqualTo("value1"));
    }

    [Test]
    public void TryGetValue_ShouldReturnFalseForNonExistingKey()
    {
        var builder = CreateBuilder();
        Assert.That(builder.TryGetValue("nonexistent", out var _), Is.False);
    }

    [Test]
    public void ContainsKey_ShouldReturnTrueForExistingKey()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        Assert.That(builder.ContainsKey("key1"), Is.True);
    }

    [Test]
    public void ContainsKey_ShouldReturnFalseForNonExistingKey()
    {
        var builder = CreateBuilder();
        Assert.That(builder.ContainsKey("nonexistent"), Is.False);
    }

    [Test]
    public void Add_ShouldAddNewItem()
    {
        var builder = CreateBuilder();
        builder.Add("key1", "value1");
        Assert.That("value1", Is.EqualTo(builder["key1"]));
    }

    [Test]
    public void Add_ShouldThrowExceptionForDuplicateKey()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        Assert.That(() => builder.Add("key1", "value2"), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Remove_ShouldRemoveExistingKey()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        Assert.That(builder.Remove("key1"), Is.True);
        Assert.That(builder.Count, Is.EqualTo(0));
    }

    [Test]
    public void Remove_ShouldReturnFalseForNonExistingKey()
    {
        var builder = CreateBuilder();
        Assert.That(builder.Remove("nonexistent"), Is.False);
    }

    [Test]
    public void CopyTo_ShouldCopyItemsToArray()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        var array = new KeyValuePair<string, string>[1];
        builder.CopyTo(array, 0);

        Assert.That(array[0], Is.EqualTo(new KeyValuePair<string, string>("key1", "value1")));
    }

    [Test]
    public void CopyTo_ShouldThrowExceptionForInsufficientArraySpace()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        var array = new KeyValuePair<string, string>[0];
        Assert.That(() => builder.CopyTo(array, 0), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Clear_ShouldRemoveAllItems()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        builder.Clear();

        Assert.That(builder.Count, Is.EqualTo(0));
        Assert.That(builder.ContainsKey("key1"), Is.False);
    }

    [Test]
    public void Build_ShouldReturnPersistentHashMap()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        var map = builder.Build();
    
        Assert.That(map, Is.InstanceOf<PersistentHashMap<string, string>>());
        Assert.That(map["key1"], Is.EqualTo("value1"));
    }
    
    [Test]
    public void Build_ShouldAllowBuilderModificationAfterBuild()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        var map = builder.Build();
    
        builder.Add("key2", "value2");
        Assert.That(builder.Count, Is.EqualTo(2));
        Assert.That(builder["key2"], Is.EqualTo("value2"));
    }
    
    [Test]
    public void Build_ShouldNotModifyBuiltMapWhenBuilderIsModified()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        var map = builder.Build();
    
        builder.Add("key2", "value2");
        builder["key1"] = "modified";
    
        Assert.That(map.Count, Is.EqualTo(1));
        Assert.That(map["key1"], Is.EqualTo("value1"));
        Assert.That(() => map["key2"], Throws.TypeOf<KeyNotFoundException>());
    }
    
    [Test]
    public void Build_ShouldCreateIndependentSnapshots()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        var map1 = builder.Build();
    
        builder.Add("key2", "value2");
        var map2 = builder.Build();
    
        Assert.That(map1.Count, Is.EqualTo(1));
        Assert.That(map2.Count, Is.EqualTo(2));
        Assert.That(() => map1["key2"], Throws.TypeOf<KeyNotFoundException>());
        Assert.That(map2["key2"], Is.EqualTo("value2"));
    }

    [Test]
    public void GetEnumerator_ShouldReturnEmptyEnumerator_WhenBuilderIsEmpty()
    {
        var builder = CreateBuilder();
        var enumerator = builder.GetEnumerator();
        
        Assert.That(enumerator.MoveNext(), Is.False);
    }
    
    [Test]
    public void GetEnumerator_ShouldEnumerateSingleItem()
    {
        var builder = CreateBuilderWithItem("key1", "value1");
        var enumerator = builder.GetEnumerator();
        
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current.Key, Is.EqualTo("key1"));
        Assert.That(enumerator.Current.Value, Is.EqualTo("value1"));
        Assert.That(enumerator.MoveNext(), Is.False);
    }
    
    [Test]
    public void GetEnumerator_ShouldEnumerateMultipleItems()
    {
        var builder = CreateBuilder();
        builder["key1"] = "value1";
        builder["key2"] = "value2";
        
        var items = new List<KeyValuePair<string, string>>();
        foreach (var item in builder)
        {
            items.Add(item);
        }
        
        Assert.That(items, Has.Count.EqualTo(2));
        Assert.That(items, Has.Member(new KeyValuePair<string, string>("key1", "value1")));
        Assert.That(items, Has.Member(new KeyValuePair<string, string>("key2", "value2")));
    }
    
    private static PersistentHashMapBuilder<string, string> CreateBuilder() => PersistentHashMap<string, string>.Empty.ToBuilder();

    private static PersistentHashMapBuilder<string, string> CreateBuilderWithItem(string key, string value)
    {
        var builder = CreateBuilder();
        builder[key] = value;
        return builder;
    }
}