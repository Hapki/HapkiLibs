
// Convenience utilities written by Malte Hildingsson, malte@hapki.se.
// No copyright is claimed, and you may use it for any purpose you like.
// No warranty for any purpose is expressed or implied.

using System.Collections;
using System.Collections.Generic;

namespace Hapki {

public struct Unum<T> : IEnumerable<T> {
    T value;

    Unum(T value) {
        this.value = value;
    }

    public IEnumerator<T> GetEnumerator() {
        yield return value;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
    }

    public static explicit operator Unum<T>(T value) {
        return new Unum<T>(value);
    }
}

} // Hapki

