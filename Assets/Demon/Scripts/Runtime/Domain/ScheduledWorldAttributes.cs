using System;
namespace Demon.Slice.Domain
{
    public sealed class ScheduledWorldAttributes
    {
        readonly int soundDay, lightDay;
        public int Day { get; private set; }
        public int RadioSound { get; private set; } = 1;
        public int DemonSound { get; private set; }
        public int SunLight { get; private set; } = 1;
        public int DemonLight { get; private set; }
        public int SoundExchangeCount { get; private set; }
        public int LightTransferCount { get; private set; }
        public ScheduledWorldAttributes(int soundDay, int lightDay)
        {
            if (soundDay < 1 || soundDay > 9 || lightDay < 1 || lightDay > 9) throw new ArgumentOutOfRangeException(nameof(soundDay));
            this.soundDay = soundDay; this.lightDay = lightDay;
        }
        public void AdvanceToDay(int day)
        {
            if (day < 1 || day > 9 || day < Day) throw new ArgumentOutOfRangeException(nameof(day));
            if (day >= soundDay && SoundExchangeCount == 0)
            {
                if (!AttributeOperations.TryExchange(RadioSound, 1, DemonSound, 1, out int radio, out int demon)) throw new InvalidOperationException("SOUND capacity violated.");
                RadioSound = radio; DemonSound = demon; SoundExchangeCount++;
            }
            if (day >= lightDay && LightTransferCount == 0)
            {
                if (!AttributeOperations.TryTransfer(SunLight, DemonLight, 1, out int sun, out int demon)) throw new InvalidOperationException("LIGHT capacity violated.");
                SunLight = sun; DemonLight = demon; LightTransferCount++;
            }
            Day = day;
        }
    }
}
