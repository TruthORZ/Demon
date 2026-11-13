using UnityEngine;

namespace Demon.Slice
{
    // Uses the existing discovery and checkpoint state, including debug replay and retries.
    public sealed class RouteItemPresentation : MonoBehaviour
    {
        [SerializeField] GameSceneController world;
        [SerializeField] GameObject[] tabletBeacons;
        [SerializeField] Transform[] tabletCenters;
        [SerializeField] GameObject boxPaper;
        [SerializeField] GameObject[] solarPaper, handoverPaper;
        void LateUpdate()
        {
            if (world.Session == null || world.Playthrough.Records == null) return;
            var campaign = world.Campaign;
            int day = world.Session.Clock.Day;
            for (int i = 0; i < tabletBeacons.Length; i++)
            {
                bool visible = i == 6 ? world.Retrieval.Available && !world.Retrieval.Progress.Acquired
                    : campaign.Progress.Available(i, day) && !campaign.Progress.Found(i);
                if (visible) tabletBeacons[i].transform.position = tabletCenters[i].position;
                SetVisible(tabletBeacons[i], visible && !world.Session.Descended);
            }

            SetVisible(boxPaper, !world.Playthrough.Records.Contains("box-record"));
            foreach (var part in solarPaper) SetVisible(part, !world.Exploration.SolarRead || world.Player.ReadingAt(world.Exploration.SolarRecord.CloseView));
            foreach (var part in handoverPaper) SetVisible(part, !world.Exploration.HandoverRead || world.Player.ReadingAt(world.Exploration.HandoverRecord.CloseView));
        }
        static void SetVisible(GameObject target, bool value)
        {
            if (target.activeSelf != value) target.SetActive(value);
        }
    }
}
