
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Collections;

namespace Hapki.Diagnostics {

public struct TraceEventData {
    public const byte Begin = 0x42;
    public const byte End = 0x45;
    public const byte Complete = 0x58;
    public const byte Instant = 0x69;
    public const byte Metadata = 0x4d;

    public FixedString128 name;
    public (FixedString128 name, object value)[] args;
    public long timestamp;
    public long duration;
    public int threadId;
    public byte phase;
}

public struct TraceEvent : IDisposable {
    Trace _trace;
    FixedString128 _name;

    public TraceEvent(Trace trace, FixedString128 name, params (FixedString128 name, object value)[] args) {
        trace.BeginEvent(name, args);
        _trace = trace;
        _name = name;
    }

    public void Dispose() =>
        _trace.EndEvent(_name);
}

public struct Trace : IDisposable {
    public static Trace Create() => Create(DateTime.Now);

    public static Trace Create(DateTime startTime) => new Trace {
        _eventData = new List<TraceEventData>(2048),
        _threadNames = new Dictionary<int, FixedString128>(),
        _startTime = startTime,
        _isTracing = true
    };

    public static Trace Combine(Trace a, Trace b) {
        var trace = Create(b._startTime);
        var deltaTime = (a._startTime - b._startTime).Ticks / 10;

        lock (a._eventData) {
            for (int i = 0, n = a._eventData.Count; i < n; ++i) {
                var e = a._eventData[i];
                e.timestamp -= deltaTime;
                trace._eventData.Add(e);
            }

            foreach (var i in a._threadNames)
                trace._threadNames[i.Key] = i.Value;
        }

        lock (b._eventData) {
            for (int i = 0, n = b._eventData.Count; i < n; ++i)
                trace._eventData.Add(b._eventData[i]);

            foreach (var i in b._threadNames)
                trace._threadNames[i.Key] = i.Value;
        }

        trace._eventData.Sort((x, y) => (x.timestamp.CompareTo(y.timestamp)));
        return trace;
    }

    List<TraceEventData> _eventData;
    Dictionary<int, FixedString128> _threadNames;
    DateTime _startTime;
    bool _isTracing;

    internal List<TraceEventData> EventData => _eventData;
    internal Dictionary<int, FixedString128> ThreadNames => _threadNames;

    public long Timestamp => (DateTime.Now - _startTime).Ticks / 10;

    public bool IsTracing => _isTracing;

    public void Dispose() => this = default;

    public void BeginEvent(
            FixedString128 name, params (FixedString128 name, object value)[] args) =>
        AddEvent(TraceEventData.Begin, name, Timestamp, args: args);

    public void BeginEvent(
            int threadId, FixedString128 name, params (FixedString128 name, object value)[] args) =>
        AddEvent(TraceEventData.Begin, threadId, name, Timestamp, args: args);

    public void EndEvent(FixedString128 name, params (FixedString128 name, object value)[] args) =>
        AddEvent(TraceEventData.End, name, Timestamp, args: args);

    public void EndEvent(int threadId, FixedString128 name, params (FixedString128 name, object value)[] args) =>
        AddEvent(TraceEventData.End, threadId, name, Timestamp, args: args);

    public void CompleteEvent(
            FixedString128 name, long startTimestamp, params (FixedString128 name, object value)[] args) =>
        AddEvent(TraceEventData.Complete, name, startTimestamp, Timestamp - startTimestamp, args: args);

    public void CompleteEvent(
            int threadId, FixedString128 name, long startTimestamp, params (FixedString128 name, object value)[] args) =>
        AddEvent(TraceEventData.Complete, threadId, name, startTimestamp, Timestamp - startTimestamp, args: args);

    public void InstantEvent(
            FixedString128 name, params (FixedString128 name, object value)[] args) =>
        AddEvent(TraceEventData.Instant, name, Timestamp, args: args);

    public void InstantEvent(
            int threadId, FixedString128 name, params (FixedString128 name, object value)[] args) =>
        AddEvent(TraceEventData.Instant, threadId, name, Timestamp, args: args);

    public void AddEvent(
            byte phase, FixedString128 name, long timestamp, long duration = 0,
            params (FixedString128 name, object value)[] args) {
        var thread = Thread.CurrentThread;

        lock (_eventData) {
            if (!_threadNames.ContainsKey(thread.ManagedThreadId))
                _threadNames.Add(
                    thread.ManagedThreadId,
                    thread.Name ?? $"Managed Thread {thread.ManagedThreadId}");

            _eventData.Add(new TraceEventData {
                name = name, args = args, timestamp = timestamp, duration = duration,
                threadId = thread.ManagedThreadId, phase = phase
            });
        }
    }

    public void AddEvent(
            byte phase, int threadId, FixedString128 name, long timestamp, long duration = 0,
            params (FixedString128 name, object value)[] args) {
        lock (_eventData) {
            _eventData.Add(new TraceEventData {
                name = name, args = args, timestamp = timestamp, duration = duration,
                threadId = threadId, phase = phase
            });
        }
    }

    public void AddThreadName(int threadId, string name) {
        lock (_eventData) {
            if (!_threadNames.ContainsKey(threadId))
                _threadNames.Add(threadId, name);
        }
    }
}

public static class TraceExtensions {
    static readonly int _processId = Process.GetCurrentProcess().Id;
    static readonly string _processName =
        $"{Application.identifier}/{Application.version} - Unity/{Application.unityVersion}";

    static StringBuilder AppendEvent(this StringBuilder builder, TraceEventData e) {
        int argCount = (e.args?.Length).GetValueOrDefault();

        builder.Append(" {\n");
        builder
            .AppendFormat("  \"ph\": \"{0}\"", (char) e.phase)
            .AppendFormat(",\n  \"name\": \"{0}\"", e.name)
            .AppendFormat(",\n  \"ts\": \"{0}\"", e.timestamp)
            .AppendFormat(",\n  \"dur\": \"{0}\"", e.duration)
            .AppendFormat(",\n  \"tid\": \"{0}\"", e.threadId)
            .AppendFormat(",\n  \"pid\": \"{0}\"", _processId);

        if (argCount > 0) {
            void AppendArg(int i) {
                var arg = e.args[i];

                if (arg.value is Array array) {
                    builder.AppendFormat("\n   \"{0}\": [", arg.name);

                    if (array.Length > 0) {
                        builder.AppendFormat("\n    \"{0}\"", array.GetValue(0));

                        for (int j = 1; j < array.Length; ++j)
                            builder.AppendFormat(",\n    \"{0}\"", array.GetValue(j));
                    }

                    builder.Append("\n   ]");
                } else
                    builder.AppendFormat("\n   \"{0}\": \"{1}\"", arg.name, arg.value);
            }

            builder.Append(",\n  \"args\": {\n");
            AppendArg(0);

            for (int i = 1; i < argCount; ++i)
            {
                builder.Append(',');
                AppendArg(i);
            }

            builder.Append("\n  }");
        }

        builder.Append("\n }");
        return builder;
    }

    static void AppendMetaData(this StringBuilder builder, Trace trace) {
        var threadIds = new NativeList<int>(10, Allocator.Temp);
        var data = trace.EventData;

        for (int i = 0, n = data.Count; i < n; ++i) {
            var e = data[i];
            bool found = false;

            for (int j = 0, m = threadIds.Length; j < m; ++j)
                if (threadIds[j] == e.threadId) {
                    found = true;
                    break;
                }

            if (!found)
                threadIds.Add(e.threadId);
        }

        builder.AppendEvent(new TraceEventData {
            phase = TraceEventData.Metadata,
            name = "process_name",
            args = new (FixedString128, object)[] {("name", _processName)}
        });
        builder.Append(",\n");
        builder.AppendEvent(new TraceEventData {
            phase = TraceEventData.Metadata,
            name = "process_labels",
            args = new (FixedString128, object)[] {("labels", SceneManager.GetActiveScene().name)}
        });

        var threads = Process.GetCurrentProcess().Threads;

        for (int i = 0, n = threadIds.Length; i < n; ++i) {
            int threadId = threadIds[i];

            builder.Append(",\n");
            builder.AppendEvent(new TraceEventData {
                phase = TraceEventData.Metadata,
                name = "thread_name",
                threadId = threadId,
                args = new (FixedString128, object)[] {("name", trace.ThreadNames[threadId])}
            });
        }

        threadIds.Dispose();
    }

    public static string FormatJson(this Trace trace) {
        var builder = new StringBuilder(4096);
        var data = trace.EventData;

        lock (data) {
            builder.Append("{\n");
            builder.Append("\"displayTimeUnit\": \"ms\",\n");
            builder.Append("\"traceEvents\": [\n");
            builder.AppendMetaData(trace);

            for (int i = 0, n = data.Count; i < n; ++i) {
                builder.Append(",\n");
                builder.AppendEvent(data[i]);
            }

            builder.Append("\n]}\n");
        }

        return builder.ToString();
    }

    public static void WriteJson(this Trace trace, string path) =>
        File.WriteAllText(path, trace.FormatJson());
}

} // Hapki.Diagnostics

