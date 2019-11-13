
// Convenience utilities written by Malte Hildingsson, malte@hapki.se.
// No copyright is claimed, and you may use it for any purpose you like.
// No warranty for any purpose is expressed or implied.

using UnityEngine;

namespace Hapki {

public static class Safely {
    public static void SafeDefault<T>(ref T value) =>
        value = default(T);

    public static void SafeDefault(ref Quaternion value) =>
        value = Quaternion.identity;

    public static void SafeDestroy(Object obj, bool isAsset = false) {
        if (obj) {
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj, isAsset);
        }
    }
}

} // Hapki

