using System;
using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    public sealed class NineDayFlow : MonoBehaviour
    {
        [SerializeField] NineDayDefinition definition;
        [SerializeField] NineDayTablet[] fixedTablets;
        [SerializeField] GameObject finalTablet;
        SliceSceneController world;
        int currentDay;
        bool openedFirstNight;
        public DiscoveryProgress Progress { get; private set; }
        public NineDayDefinition Definition => definition;
        public bool FinalAvailable => world && (world.Session.Clock.Day > definition.FinalTabletDay ||
            (world.Session.Clock.Day == definition.FinalTabletDay && world.Session.Clock.MinuteOfDay >= definition.FinalTabletHour * 60));

        public void Configure(NineDayDefinition data, NineDayTablet[] tablets, GameObject editableTablet)
        { definition = data; fixedTablets = tablets; finalTablet = editableTablet; }

        public void Initialize(SliceSceneController controller)
        {
            if (!definition || definition.Days.Length != 9 || definition.Tablets.Length != 6 ||
                fixedTablets == null || fixedTablets.Length != 6 || !finalTablet)
                throw new InvalidOperationException("Nine-day flow requires nine days, six fixed tablets and T07.");
            var days = new int[6];
            for (int i = 0; i < 6; i++)
            {
                if (definition.Tablets[i] == null || !fixedTablets[i]) throw new InvalidOperationException("Missing tablet plan/reference.");
                days[i] = definition.Tablets[i].DiscoveryDay;
            }
            foreach (var day in definition.Days) if (day == null) throw new InvalidOperationException("Missing day plan.");
            world = controller;
            Progress = new DiscoveryProgress(days);
            world.Session.Clock.Midnight += OnMidnight;
            RefreshDay();
            EvaluateFirstNight();
        }
        void Update()
        {
            if (!world) return;
            EvaluateFirstNight();
            if (finalTablet.activeSelf != FinalAvailable) finalTablet.SetActive(FinalAvailable);
        }
        void OnMidnight(int day) { RefreshDay(); EvaluateFirstNight(); }
        void RefreshDay()
        {
            currentDay = world.Session.Clock.Day;
            for (int i = 0; i < 6; i++) fixedTablets[i].gameObject.SetActive(Progress.Available(i, currentDay));
            finalTablet.SetActive(FinalAvailable);
            if (!world.Session.Descended) world.SetMessage(definition.Days[Mathf.Clamp(currentDay - 1, 0, 8)].Objective);
        }
        void EvaluateFirstNight()
        {
            if (openedFirstNight || (world.Session.Clock.Day == 1 && world.Session.Clock.MinuteOfDay < definition.FirstNightOpeningHour * 60)) return;
            openedFirstNight = true;
            world.Session.OpenLake();
            if (world.Session.Clock.Day == 1) world.SetMessage(definition.FirstNightMessage);
        }
        public float WakeHourFor(int day) => definition.Days[Mathf.Clamp(day - 1, 0, 8)].WakeHour;
        public string Title => definition.Days[Mathf.Clamp(currentDay - 1, 0, 8)].Title;
        public void ReadTablet(int index, Transform view)
        {
            if (!Progress.Available(index, world.Session.Clock.Day) || world.Session.Descended) return;
            Progress.Discover(index, world.Session.Clock.Day);
            var tablet = definition.Tablets[index];
            string node = tablet.DateNodeDay > 0 ? "日期节点：D" + tablet.DateNodeDay + " 00:00（" +
                (world.Session.Clock.Day >= tablet.DateNodeDay ? "已到达" : "尚未到达") + "）" : "工具机制记录（当前灰盒尚未接入工具操作）";
            world.ReadDocument(tablet.Title + "\n" + node + "\n\n" + tablet.Record + "\n\nEsc 放下石板记录。", view);
        }
        void OnDestroy() { if (world && world.Session != null) world.Session.Clock.Midnight -= OnMidnight; }
    }
}
