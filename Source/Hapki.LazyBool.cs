
using System;

[Serializable]
public struct LazyBool {
    public static implicit operator LazyBool(bool value) {
        return new LazyBool {_value = value ? 1 : -1};
    }

    public static bool operator==(LazyBool a, bool b) { return a.ToBool(b); }
    public static bool operator!=(LazyBool a, bool b) { return !a.ToBool(b); }

    int _value;

    public bool ToBool(bool polarity) {
        return polarity ? _value > 0 : _value < 0;
    }

    public override bool Equals(object o) {
        if (o is LazyBool)
            return ((LazyBool) o)._value == _value;

        if (o is bool)
            return this == (bool) o;

        return false;
    }

    public override int GetHashCode() {
        return _value.GetHashCode();
    }
}

