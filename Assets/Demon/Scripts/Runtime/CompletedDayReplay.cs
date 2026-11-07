using System;
using Demon.Slice.Domain;

namespace Demon.Slice
{
    // Debug navigation runs the existing public interactions against a fresh gameplay session.
    // It never restores a clock backwards or sets discoveries/seals through private fields.
    public static class CompletedDayReplay
    {
        public static void Run(GameSceneController world, int targetDay, bool finalNight)
        {
            if (targetDay < 1 || targetDay > 9 || (finalNight && targetDay != 8))
                throw new ArgumentOutOfRangeException(nameof(targetDay));
            if (!world || world.Session == null || !world.Exploration || !world.Exploration.HasFinalPreparation ||
                world.Session.Clock.Day != 1 || world.Campaign.Progress.FoundCount != 0)
                throw new InvalidOperationException("Complete-day replay requires a fresh D1 campaign with all authored exploration tools.");
            while (world.Session.Clock.Day < targetDay)
            {
                int day = world.Session.Clock.Day;
                Complete(world, day);
                if (day < 8) world.Sleep();
                Require(world.Session.Clock.Day == day + 1, "Day advancement did not complete D" + day);
            }
            if (finalNight)
            {
                world.Exploration.HandoverRecord.Use(world.Player);
                world.Player.ExitCloseView();
                Advance(world, 7 * GameClock.MinutesPerDay + world.Campaign.Definition.FinalTabletHour * 60);
                world.Retrieval.RefreshPresentation();
            }
            if (!world.Session.Descended) world.Player.ReturnToExploration();
        }

        static void Complete(GameSceneController world, int day)
        {
            var guide = world.Exploration;
            switch (day)
            {
                case 1:
                    foreach (var observation in guide.Observations) { observation.Use(world.Player); world.Player.ExitCloseView(); }
                    Advance(world, world.Campaign.Definition.FirstNightOpeningHour * 60);
                    Require(world.Session.LakeSeal == 0 && guide.Log.Count == 3, "First-night opening/observations missing.");
                    break;
                case 2: Read(world, guide.FirstTablet); break;
                case 3: Read(world, guide.SecondTablet); break;
                case 4: Read(world, guide.ThirdTablet); break;
                case 5: Read(world, guide.FourthTablet); break;
                case 6:
                    guide.FifthTablet.Use(world.Player);
                    Fill(world.Listening.Board, world.Listening.Definition.Target);
                    Require(world.Listening.TryCompile(), "T05 could not activate.");
                    world.Player.ExitCloseView();
                    break;
                case 7:
                    guide.SolarRecord.Use(world.Player); world.Player.ExitCloseView();
                    var box = world.Cipher;
                    var answer = box.Wheel.ReadCode();
                    for (int i = 0; i < 3; i++)
                        for (int turn = 0; box.Model.At(i) != answer[i] && turn < 10; turn++) box.Model.Cycle(i);
                    Require(box.TryConfirm(), "Box A could not unlock using the authored bearing code.");
                    box.Use(world.Player); world.Player.ExitCloseView();
                    Require(box.Acquired && world.Campaign.Progress.Found(5), "T06 was not acquired.");
                    world.Tether.Rule.Advance(true, world.Tether.Rule.RequiredSeconds);
                    guide.RefreshPracticeProgress();
                    world.Tether.Rule.Release(); guide.RefreshPracticeProgress();
                    Require(guide.PracticedBeam, "T06 lock/release practice did not complete.");
                    break;
                case 8:
                    guide.HandoverRecord.Use(world.Player); world.Player.ExitCloseView();
                    Advance(world, 7 * GameClock.MinutesPerDay + world.Campaign.Definition.FinalTabletHour * 60);
                    world.Retrieval.RefreshPresentation();
                    var pickup = world.Retrieval.Pickup;
                    pickup.Use(world.Player); pickup.TickHeld(true, pickup.HoldSeconds + .01f);
                    Require(world.Retrieval.Place(world.Player), "T07 pickup/placement did not complete.");
                    Fill(world.Session.Board, world.Settings.Target);
                    Require(world.TryCompile(), "T07 did not compile.");
                    world.ReadBox();
                    Require(world.HasReadBox && world.Player.Reading, "Box B record was not read.");
                    world.Player.ExitCloseView();
                    // Use the same sequence and configured timings, skipping its wall-clock wait for this debug jump.
                    for (int tick = 0; tick < 1000 && !world.EndingPresented; tick++) world.Finale.Tick(.25f);
                    Require(world.EndingPresented && world.Session.Descended && world.Session.ExchangeCount == 2,
                        "D9 must follow both SEAL exchanges and the normal ending sequence.");
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(day));
            }
        }

        static void Read(GameSceneController world, NineDayTablet tablet)
        { tablet.Use(world.Player); world.Player.ExitCloseView(); Require(world.Campaign.Progress.Found(tablet.Index), "Missing tablet discovery."); }

        static void Advance(GameSceneController world, double minute)
        {
            world.Session.Clock.AdvanceTo(Math.Max(world.Session.Clock.AbsoluteMinutes, minute));
            world.Campaign.RefreshClockState();
        }

        static void Fill(RuneBoardModel board, RuneKind[] target)
        {
            // Route the authored answer through the same tray/slot operations as manual play.
            for (int i = 0; i < board.Count; i++)
            {
                if (board.At(i) == target[i]) continue;
                if (board.Held != target[i] && !board.PickFromTray(target[i]))
                {
                    int source = -1;
                    for (int j = i + 1; j < board.Count; j++)
                        if (board.At(j) == target[i]) { source = j; break; }
                    Require(source >= 0, "Authored rune supply cannot complete slot " + i);
                    board.ClickSlot(source);
                }
                Require(board.Held == target[i] && board.ClickSlot(i), "Rune placement failed.");
            }
            Require(board.MatchesTarget(), "Completed layout does not match its authored target.");
        }

        static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }
    }
}
