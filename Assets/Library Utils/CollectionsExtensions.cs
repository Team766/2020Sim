using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

public static class CollectionsExtensions
{
    // For Reference Types (or Nullable Reference Types)
    public static TResult? Map<TInput, TResult>(this TInput? input, Func<TInput, TResult> function)
        where TInput : class
        where TResult : class
    {
        return input is not null ? function(input) : null;
    }

    // For Nullable Value Types (Structs like int?, DateTime?)
    public static TResult? MapValue<TInput, TResult>(this TInput? input, Func<TInput, TResult> function)
        where TInput : struct
        where TResult : struct
    {
        return input.HasValue ? function(input.Value) : null;
    }

    public static V ComputeIfAbsent<K, V>(this Dictionary<K, V> dict, K key, Func<K, V> generator)
    {
        if (dict.TryGetValue(key, out var value))
        {
            return value;
        }
        var generated = generator(key);
        dict.Add(key, generated);
        return generated;
    }
}

public class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
{
    protected readonly Func<TKey, TValue> factory;

    public DefaultDictionary(Func<TKey, TValue> factory)
    {
        this.factory = factory;
    }

    public new TValue this[TKey key]
    {
        get
        {
            if (!TryGetValue(key, out TValue val))
            {
                val = factory(key);
                Add(key, val);
            }
            return val;
        }
        set { base[key] = value; }
    }
}