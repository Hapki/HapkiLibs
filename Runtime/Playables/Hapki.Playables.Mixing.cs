
// Convenience utilities written by Malte Hildingsson, malte@hapki.se.
// No copyright is claimed, and you may use it for any purpose you like.
// No warranty for any purpose is expressed or implied.

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hapki.Playables {

using IndexWeightPair = ValueTuple<(int index, float weight), (int index, float weight)>;

public static partial class PlayableExtensions {
    public static double GetMixerTime(this Playable mixer) =>
        mixer.GetOutput(0).GetTime();

    public static IndexWeightPair GetActiveMixerInputs(this Playable mixer) {
        for (int i = 0, n = mixer.GetInputCount(); i < n; ++i) {
            float u = mixer.GetInputWeight(i);

            if (u > 0f) {
                if (i < n - 1) {
                    float v = mixer.GetInputWeight(i + 1);

                    if (v > 0f)
                        return ((i, u), (i + 1, v));
                }

                return ((i, u), (-1, 0f));
            }
        }

        return ((-1, 0f), (-1, 0f));
    }

    public static (TimelineClip clip, int index) GetActiveMixerInput(this Playable mixer, TimelineClip[] clips) =>
        mixer.GetActiveMixerInput(clips, mixer.GetMixerTime());

    public static (TimelineClip clip, int index) GetActiveMixerInput(
            this Playable mixer, TimelineClip[] clips, double time) {
        int count = clips.Length;
        int upper = count - 1;
        int lower = 0;
        int index = 0;

        while (lower <= upper) {
            index = (lower + upper) >> 1;
            var clip = clips[index];

            if (clip.start > time)
                upper = index - 1;
            else if (clip.end <= time)
                lower = index + 1;
            else
                return (clip, index);
        }

        return (null, upper);
    }

    public static (int first, int second) GetActiveMixerInputs(this Playable mixer, TimelineClip[] clips) =>
        mixer.GetActiveMixerInputs(clips, mixer.GetMixerTime());

    public static (int first, int second) GetActiveMixerInputs(
            this Playable mixer, TimelineClip[] clips, double time) {
        var active = mixer.GetActiveMixerInput(clips, time);

        if (active.clip != null) {
            int count = clips.Length;
            int index = active.index;

            if (index < count - 1) {
                var next = clips[index + 1];

                if (next.start <= time)
                    return (index, index + 1);
            }

            if (index > 0) {
                var prev = clips[index - 1];

                if (prev.end >= time)
                    return (index - 1, index);
            }

            return (index, -1);
        }

        return (-1, -1);
    }
}

} // Hapki.Playables

