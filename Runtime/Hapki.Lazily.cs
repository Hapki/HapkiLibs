
// Convenience utilities written by Malte Hildingsson, malte@hapki.se.
// No copyright is claimed, and you may use it for any purpose you like.
// No warranty for any purpose is expressed or implied.

using System.Collections.Generic;
using UnityEngine;

namespace Hapki {

using static Safely;

public static class Lazily {
    public static void LazyAdd<TDictionary, TKey, TValue>(this TDictionary dictionary, TKey key, out TValue value)
            where TDictionary : IDictionary<TKey, TValue>
            where TValue : new() {
        if (!dictionary.TryGetValue(key, out value))
            dictionary.Add(key, value = new TValue());
    }

    public static T LazyNew<T>(ref T value) where T : class, new() => value ?? (value = new T());

    public static void LazyDestroy<T>(ref T value, bool isAsset = false) where T : Object {
        SafeDestroy(value, isAsset);
        value = null;
    }

    public static T LazyAddComponent<T>(this GameObject gameObject) where T : Component =>
        gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();

    public static T LazyAddComponent<T>(this GameObject gameObject, ref T value) where T : Component =>
        value ? value : (value = gameObject.LazyAddComponent<T>());

    public static T LazyGetComponent<T>(this GameObject gameObject, ref T value) where T : Component =>
        value ? value : (value = gameObject.GetComponent<T>());
}

} // Hapki

