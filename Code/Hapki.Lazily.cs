
// Convenience utilities written by Malte Hildingsson, malte@hapki.se.
// No copyright is claimed, and you may use it for any purpose you like.
// No warranty for any purpose is expressed or implied.

using System.Collections.Generic;

namespace Hapki {

public static class Lazily {
    public static T LazyNew<T>(ref T value) where T : class, new() {
        return value ?? (value = new T());
    }
}

} // Hapki

