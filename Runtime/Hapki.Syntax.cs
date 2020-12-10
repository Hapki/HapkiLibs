
// Convenience utilities written by Malte Hildingsson, malte@hapki.se.
// No copyright is claimed, and you may use it for any purpose you like.
// No warranty for any purpose is expressed or implied.

using System.Collections.Generic;

namespace Hapki.Syntax {

public interface IReductionSyntax<T> where T : IReductionSyntax<T> {
    void OnReduce(List<ReductionSyntax<T>> nodes, int index);
}

public struct ReductionSyntax<T> where T : IReductionSyntax<T> {
    static readonly List<ReductionSyntax<T>> nodes = new List<ReductionSyntax<T>>();

    public T Data { get; private set; }
    public int Index { get; private set; }

    ReductionSyntax(T data) {
        Data = data;
        Index = nodes.Count;
        nodes.Add(this);
    }

    void Reduce() {
        if (nodes.Count > Index + 1) {
            Data.OnReduce(nodes, Index + 1);

            if (Index > 0)
                nodes.RemoveRange(Index + 1, nodes.Count - (Index + 1));
            else
                nodes.Clear();
        }
    }

    public ReductionSyntax<T> this[T _] => this;

    public static T operator-(ReductionSyntax<T> node) {
        node.Reduce();
        return node;
    }

    public static T operator-(T _, ReductionSyntax<T> node) => -node;

    public static implicit operator ReductionSyntax<T>(T data) => new ReductionSyntax<T>(data);

    public static implicit operator T(ReductionSyntax<T> node) => node.Data;
}

} // Hapki.Syntax

