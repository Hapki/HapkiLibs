
// Convenience utilities written by Malte Hildingsson, malte@hapki.se.
// No copyright is claimed, and you may use it for any purpose you like.
// No warranty for any purpose is expressed or implied.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Hapki.Syntax;

namespace Hapki.UIElements {

public struct VisualElementSyntax : IReductionSyntax<VisualElementSyntax> {
    public static implicit operator VisualElementSyntax(VisualElement element) =>
        new VisualElementSyntax {_element = element};

    public static implicit operator VisualElement(VisualElementSyntax syntax) =>
        syntax._element;

    VisualElement _element;

    public void OnReduce(List<ReductionSyntax<VisualElementSyntax>> nodes, int index) {
        for (int i = 0, n = nodes.Count - index; i < n; ++i)
            _element.Add(((VisualElementSyntax) nodes[i + index])._element);
    }
}

public class VisualElementBuilder {
    public static ReductionSyntax<VisualElementSyntax> Add<T>()
            where T : VisualElement, new() =>
        (VisualElementSyntax) new T();

    public static ReductionSyntax<VisualElementSyntax> Add<T>(Action<T> action)
            where T : VisualElement, new() {
        var element = new T();
        action(element);
        return (VisualElementSyntax) element;
    }
}

} // Hapki.UIElements

