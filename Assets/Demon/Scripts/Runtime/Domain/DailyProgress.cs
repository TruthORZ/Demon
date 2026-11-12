using System;
using System.Collections.Generic;

namespace Demon.Slice.Domain
{
    public sealed class DailyProgress
    {
        readonly HashSet<string> completed = new HashSet<string>(StringComparer.Ordinal);
        public int Day { get; private set; }
        public DailyProgress(int day) { Begin(day); }
        public void Begin(int day)
        {
            if (day < 1 || day > 9) throw new ArgumentOutOfRangeException(nameof(day));
            Day = day; completed.Clear();
        }
        public bool Record(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Task ID required.");
            return completed.Add(id);
        }
        public bool Contains(string id) => completed.Contains(id);
        public bool Complete(string[] required)
        {
            if (required == null) throw new ArgumentNullException(nameof(required));
            foreach (string id in required) if (!completed.Contains(id)) return false;
            return true;
        }
        public string[] Capture() { var copy = new string[completed.Count]; completed.CopyTo(copy); return copy; }
        public void Restore(int day, string[] ids)
        { Begin(day); foreach (string id in ids ?? throw new ArgumentNullException(nameof(ids))) Record(id); }
        public bool CanSleep(string[] required, double hour, double nightHour) =>
            Day < 8 && hour >= nightHour && Complete(required);
    }

    public sealed class RecordCollection
    {
        readonly HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
        public bool Add(string id)
        { if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Record ID required."); return ids.Add(id); }
        public bool Contains(string id) => ids.Contains(id);
        public string[] Capture() { var copy = new string[ids.Count]; ids.CopyTo(copy); Array.Sort(copy, StringComparer.Ordinal); return copy; }
        public void Restore(string[] snapshot)
        { ids.Clear(); foreach (string id in snapshot ?? throw new ArgumentNullException(nameof(snapshot))) Add(id); }
    }
}
