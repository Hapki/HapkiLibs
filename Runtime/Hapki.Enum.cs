
// Convenience utilities written by Malte Hildingsson, malte@hapki.se.
// No copyright is claimed, and you may use it for any purpose you like.
// No warranty for any purpose is expressed or implied.

using System;
using UnityEngine;

namespace Hapki {

public static class Enum {
    public static T Parse<T>(string name, T fallback = default(T)) where T : struct, IConvertible {
#if NET_4_6
        T e;
        if (System.Enum.TryParse(name, out e))
            return e;
        return fallback;
#else
        try {
            return (T) System.Enum.Parse(typeof(T), name);
        } catch {
            return fallback;
        }
#endif
    }
}

public class EnumFlagsAttribute : PropertyAttribute {
}

public class SerializedEnumAttribute : PropertyAttribute {
    public readonly Type type;

    public SerializedEnumAttribute(Type type) {
        this.type = type;
    }
}

} // Hapki

