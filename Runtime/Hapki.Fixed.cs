
// Convenience utilities written by Malte Hildingsson, malte@hapki.se.
// No copyright is claimed, and you may use it for any purpose you like.
// No warranty for any purpose is expressed or implied.

using UnityEngine;

namespace Hapki {

// Q10.6 signed fixed point type

public struct Fixed16 {
    public static Fixed16 zero => FromBits(0x0000);
    public static Fixed16 zeroPointZeroOne => FromBits(0x0001);
    public static Fixed16 zeroPointOne => FromBits(0x0006);
    public static Fixed16 zeroPointFive => FromBits(0x0020);
    public static Fixed16 one => FromBits(0x0040);
    public static Fixed16 onePointFive => FromBits(0x0060);
    public static Fixed16 two => FromBits(0x0080);
    public static Fixed16 twoPointFive => FromBits(0x00a0);

    public readonly short bits;

    public static Fixed16 FromBits(short v) =>
        new Fixed16(v);

    public static explicit operator Fixed16(int v) =>
        new Fixed16((short) (v << 6));

    public static explicit operator Fixed16(float v) {
        float x = Mathf.Floor(v);
        return new Fixed16((short) (((int) x << 6) + Mathf.RoundToInt((v - x) * 64f)));
    }

    public static explicit operator float(Fixed16 v) =>
        (float) (v.bits >> 6) + ((v.bits & 0x3f) * 0.015625f);

    public static explicit operator int(Fixed16 v) =>
        v.bits >> 6;

    public static Fixed16 operator+(Fixed16 x, Fixed16 y) =>
        new Fixed16((short) (x.bits + y.bits));

    public static Fixed16 operator-(Fixed16 x, Fixed16 y) =>
        new Fixed16((short) (x.bits - y.bits));

    public static Fixed16 operator*(Fixed16 x, Fixed16 y) =>
        new Fixed16((short) ((x.bits * y.bits) >> 6));

    public static Fixed16 operator/(Fixed16 x, Fixed16 y) =>
        new Fixed16((short) ((x.bits << 6) / y.bits));

    public static bool operator<(Fixed16 x, Fixed16 y) => x.bits < y.bits;
    public static bool operator>(Fixed16 x, Fixed16 y) => x.bits > y.bits;

    public static bool operator<=(Fixed16 x, Fixed16 y) => x.bits <= y.bits;
    public static bool operator>=(Fixed16 x, Fixed16 y) => x.bits >= y.bits;

    public static Fixed16 Abs(Fixed16 v) {
        int x = v.bits >> 6;
        return new Fixed16((short) (((x >= 0 ? x : -x) << 6) + (v.bits & 0x3f)));
    }

    public static int RoundToInt(Fixed16 v) =>
        (int) (v + zeroPointFive);

    Fixed16(short bits) =>
        this.bits = bits;

    public override int GetHashCode() =>
        bits ^ ((bits - 1) << 16);

    public override string ToString() =>
        ((float) this).ToString();
}

// Q22.10 signed fixed point type

public struct Fixed32 {
    public static Fixed32 zero => FromBits(0x000000000);
    public static Fixed32 zeroPointZeroOne => FromBits(0x00000000a);
    public static Fixed32 zeroPointOne => FromBits(0x000000066);
    public static Fixed32 zeroPointFive => FromBits(0x000000200);
    public static Fixed32 one => FromBits(0x000000400);
    public static Fixed32 onePointFive => FromBits(0x000000600);
    public static Fixed32 two => FromBits(0x000000800);
    public static Fixed32 twoPointFive => FromBits(0x000000a00);

    public readonly int bits;

    public static Fixed32 FromBits(int v) =>
        new Fixed32(v);

    public static explicit operator Fixed32(int v) =>
        new Fixed32(v << 10);

    public static explicit operator Fixed32(float v) {
        float x = Mathf.Floor(v);
        return new Fixed32(((int) x << 10) + Mathf.RoundToInt((v - x) * 1024f));
    }

    public static explicit operator float(Fixed32 v) =>
        (float) (v.bits >> 10) + ((v.bits & 0x3ff) * 0.0009765625f);

    public static explicit operator int(Fixed32 v) =>
        v.bits >> 10;

    public static Fixed32 operator+(Fixed32 x, Fixed32 y) =>
        new Fixed32(x.bits + y.bits);

    public static Fixed32 operator-(Fixed32 x, Fixed32 y) =>
        new Fixed32(x.bits - y.bits);

    public static Fixed32 operator*(Fixed32 x, Fixed32 y) =>
        new Fixed32((int) (((long) x.bits * (long) y.bits) >> 10));

    public static Fixed32 operator/(Fixed32 x, Fixed32 y) =>
        new Fixed32((int) (((long) x.bits << 10) / (long) y.bits));

    public static bool operator<(Fixed32 x, Fixed32 y) => x.bits < y.bits;
    public static bool operator>(Fixed32 x, Fixed32 y) => x.bits > y.bits;

    public static bool operator<=(Fixed32 x, Fixed32 y) => x.bits <= y.bits;
    public static bool operator>=(Fixed32 x, Fixed32 y) => x.bits >= y.bits;

    public static Fixed32 Abs(Fixed32 v) {
        int x = v.bits >> 10;
        return new Fixed32(((x >= 0 ? x : -x) << 10) + (v.bits & 0x3ff));
    }

    public static int RoundToInt(Fixed32 v) =>
        (int) (v + zeroPointFive);

    Fixed32(int bits) =>
        this.bits = bits;

    public override int GetHashCode() =>
        bits ^ ((bits - 1) << 16);

    public override string ToString() =>
        ((float) this).ToString();
}

} // Hapki

