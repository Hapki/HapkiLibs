
// Convenience utilities written by Malte Hildingsson, malte@hapki.se.
// No copyright is claimed, and you may use it for any purpose you like.
// No warranty for any purpose is expressed or implied.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
using Hapki.Syntax;

namespace Hapki.Playables {

public struct PlayableSyntax : IPlayable, IReductionSyntax<PlayableSyntax> {
    public static PlayableSyntax Create<T>(PlayableGraph graph, T playable)
            where T : struct, IPlayable =>
        new PlayableSyntax(graph, playable.GetHandle());

    readonly PlayableGraph _graph;
    readonly PlayableHandle _handle;

    PlayableSyntax(PlayableGraph graph, PlayableHandle handle) {
        _graph = graph;
        _handle = handle;
    }

    public PlayableHandle GetHandle() => _handle;

    public void OnReduce(List<ReductionSyntax<PlayableSyntax>> nodes, int index) {
        for (int i = 0, n = nodes.Count - index; i < n; ++i)
            _graph.Connect((PlayableSyntax) nodes[i + index], 0, this, i);
    }
}

public static class PlayableSyntaxExtensions {
    public static ReductionSyntax<PlayableSyntax> Add(
            this PlayableGraph graph, ref AnimationClipPlayable playable, AnimationClip clip) =>
        PlayableSyntax.Create(graph, playable = AnimationClipPlayable.Create(graph, clip));

    public static ReductionSyntax<PlayableSyntax> Add(
            this PlayableGraph graph, ref AnimationMixerPlayable playable, int count) =>
        PlayableSyntax.Create(graph, playable = AnimationMixerPlayable.Create(graph, count));

    public static ReductionSyntax<PlayableSyntax> Add(
            this PlayableGraph graph, ref AudioClipPlayable playable, AudioClip clip, bool looping) =>
        PlayableSyntax.Create(graph, playable = AudioClipPlayable.Create(graph, clip, looping));

    public static ReductionSyntax<PlayableSyntax> Add(
            this PlayableGraph graph, ref AudioMixerPlayable playable, int count, bool normalize = false) =>
        PlayableSyntax.Create(graph, playable = AudioMixerPlayable.Create(graph, count, normalize));
}

} // Hapki.Playables

