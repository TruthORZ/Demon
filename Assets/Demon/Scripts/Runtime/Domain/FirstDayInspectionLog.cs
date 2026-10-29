using System;
namespace Demon.Slice.Domain
{
    public enum ObservationSite { Radio, WindVane, Lake }
    public sealed class FirstDayInspectionLog
    {
        readonly bool[] observed = new bool[3];
        public int Count { get; private set; }
        public bool Contains(ObservationSite site) => observed[Index(site)];
        static int Index(ObservationSite site) { int index = (int)site; if (index < 0 || index >= 3) throw new ArgumentOutOfRangeException(nameof(site)); return index; }
        public bool Record(int day, ObservationSite site)
        {
            if (day < 1 || day > 9) throw new ArgumentOutOfRangeException(nameof(day));
            int index = Index(site); if (day != 1 || observed[index]) return false;
            observed[index] = true; Count++; return true;
        }
    }
}
