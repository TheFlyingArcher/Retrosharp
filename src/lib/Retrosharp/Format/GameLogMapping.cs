using System;
using System.Collections.Generic;
using System.Text;

using CsvHelper.Configuration;

namespace Retrosharp.Format
{
    public sealed class VisitorGameLogHittingStatisticsMap : ClassMap<GameLogHittingStatistics>
    {
        public VisitorGameLogHittingStatisticsMap()
        {
            Map(m => m.AtBats).Index(21);
            Map(m => m.Hits).Index(22);
            Map(m => m.Doubles).Index(23);
            Map(m => m.Triples).Index(24);
            Map(m => m.Homeruns).Index(25);
            Map(m => m.RunsBattedIn).Index(26);
            Map(m => m.SacrificeHits).Index(27);
            Map(m => m.SacrificeFlies).Index(28);
            Map(m => m.TimesHitByPitch).Index(29);
            Map(m => m.BasesOnBalls).Index(30);
            Map(m => m.IntentionalBasesOnBalls).Index(31);
            Map(m => m.Strikeouts).Index(32);
            Map(m => m.StolenBases).Index(33);
            Map(m => m.TimesCaughtStealing).Index(34);
            Map(m => m.TimesGidp).Index(35);
            Map(m => m.TimesCatchersInterference).Index(36);
            Map(m => m.LeftOnBase).Index(37);
        }
    }

    public sealed class VisitorGameLogPitchingStatisticsMap : ClassMap<GameLogPitchingStatistics>
    {
        public VisitorGameLogPitchingStatisticsMap()
        {
            Map(m => m.PitchersUsed).Index(38);
            Map(m => m.EarnedRuns).Index(39);
            Map(m => m.TeamEarnedRuns).Index(40);
            Map(m => m.WildPitches).Index(41);
            Map(m => m.Balks).Index(42);
        }
    }

    public sealed class VisitorGameLogFieldingStatisticsMap : ClassMap<GameLogFieldingStatistics>
    {
        public VisitorGameLogFieldingStatisticsMap()
        {
            Map(m => m.Putouts).Index(43);
            Map(m => m.Assists).Index(44);
            Map(m => m.Errors).Index(45);
            Map(m => m.PassedBalls).Index(46);
            Map(m => m.DoublePlays).Index(47);
            Map(m => m.TriplePlays).Index(48);
        }
    }

    #region Visitor Lineup Mapping
    public sealed class VisitorGameLineupLeadoffBatterMap : ClassMap<GameLineupBatter>
    {
        public VisitorGameLineupLeadoffBatterMap()
        {
            Map(m => m.PlayerId).Index(105);
            Map(m => m.PlayerName).Index(106);
            Map(m => m.PlayerPosition).Index(107);
        }
    }

    public sealed class VisitorGameLineupSecondBatterMap : ClassMap<GameLineupBatter>
    {
        public VisitorGameLineupSecondBatterMap()
        {
            Map(m => m.PlayerId).Index(108);
            Map(m => m.PlayerName).Index(109);
            Map(m => m.PlayerPosition).Index(110);
        }
    }

    public sealed class VisitorGameLineupThirdBatterMap : ClassMap<GameLineupBatter>
    {
        public VisitorGameLineupThirdBatterMap()
        {
            Map(m => m.PlayerId).Index(111);
            Map(m => m.PlayerName).Index(112);
            Map(m => m.PlayerPosition).Index(113);
        }
    }

    public sealed class VisitorGameLineupCleanupBatterMap : ClassMap<GameLineupBatter>
    {
        public VisitorGameLineupCleanupBatterMap()
        {
            Map(m => m.PlayerId).Index(114);
            Map(m => m.PlayerName).Index(115);
            Map(m => m.PlayerPosition).Index(116);
        }
    }

    public sealed class VisitorGameLineupFifthBatterMap : ClassMap<GameLineupBatter>
    {
        public VisitorGameLineupFifthBatterMap()
        {
            Map(m => m.PlayerId).Index(117);
            Map(m => m.PlayerName).Index(118);
            Map(m => m.PlayerPosition).Index(119);
        }
    }

    public sealed class VisitorGameLineupSixthBatterMap : ClassMap<GameLineupBatter>
    {
        public VisitorGameLineupSixthBatterMap()
        {
            Map(m => m.PlayerId).Index(120);
            Map(m => m.PlayerName).Index(121);
            Map(m => m.PlayerPosition).Index(122);
        }
    }

    public sealed class VisitorGameLineupSeventhBatterMap : ClassMap<GameLineupBatter>
    {
        public VisitorGameLineupSeventhBatterMap()
        {
            Map(m => m.PlayerId).Index(123);
            Map(m => m.PlayerName).Index(124);
            Map(m => m.PlayerPosition).Index(125);
        }
    }

    public sealed class VisitorGameLineupEighthBatterMap : ClassMap<GameLineupBatter>
    {
        public VisitorGameLineupEighthBatterMap()
        {
            Map(m => m.PlayerId).Index(126);
            Map(m => m.PlayerName).Index(127);
            Map(m => m.PlayerPosition).Index(128);
        }
    }

    public sealed class VisitorGameLineupNinthBatterMap : ClassMap<GameLineupBatter>
    {
        public VisitorGameLineupNinthBatterMap()
        {
            Map(m => m.PlayerId).Index(129);
            Map(m => m.PlayerName).Index(130);
            Map(m => m.PlayerPosition).Index(131);
        }
    }

    public sealed class VisitorGameLineupMap : ClassMap<GameLineup>
    {
        public VisitorGameLineupMap()
        {
            References<VisitorGameLineupLeadoffBatterMap>(r => r.LeadoffBatter);
            References<VisitorGameLineupSecondBatterMap>(r => r.SecondBatter);
            References<VisitorGameLineupThirdBatterMap>(r => r.ThirdBatter);
            References<VisitorGameLineupCleanupBatterMap>(r => r.CleanupBatter);
            References<VisitorGameLineupFifthBatterMap>(r => r.FifthBatter);
            References<VisitorGameLineupSixthBatterMap>(r => r.SixthBatter);
            References<VisitorGameLineupSeventhBatterMap>(r => r.SeventhBatter);
            References<VisitorGameLineupEighthBatterMap>(r => r.EighthBatter);
            References<VisitorGameLineupNinthBatterMap>(r => r.NinthBatter);
        }
    }
    #endregion

    public sealed class HomeGameLogHittingStatisticsMap : ClassMap<GameLogHittingStatistics>
    {
        public HomeGameLogHittingStatisticsMap()
        {
            Map(m => m.AtBats).Index(49);
            Map(m => m.Hits).Index(50);
            Map(m => m.Doubles).Index(51);
            Map(m => m.Triples).Index(52);
            Map(m => m.Homeruns).Index(53);
            Map(m => m.RunsBattedIn).Index(54);
            Map(m => m.SacrificeHits).Index(55);
            Map(m => m.SacrificeFlies).Index(56);
            Map(m => m.TimesHitByPitch).Index(57);
            Map(m => m.BasesOnBalls).Index(58);
            Map(m => m.IntentionalBasesOnBalls).Index(59);
            Map(m => m.Strikeouts).Index(60);
            Map(m => m.StolenBases).Index(61);
            Map(m => m.TimesCaughtStealing).Index(62);
            Map(m => m.TimesGidp).Index(63);
            Map(m => m.TimesCatchersInterference).Index(64);
            Map(m => m.LeftOnBase).Index(65);
        }
    }

    public sealed class HomeGameLogPitchingStatisticsMap : ClassMap<GameLogPitchingStatistics>
    {
        public HomeGameLogPitchingStatisticsMap()
        {
            Map(m => m.PitchersUsed).Index(66);
            Map(m => m.EarnedRuns).Index(67);
            Map(m => m.TeamEarnedRuns).Index(68);
            Map(m => m.WildPitches).Index(69);
            Map(m => m.Balks).Index(70);
        }
    }
    public sealed class HomeGameLogFieldingStatisticsMap : ClassMap<GameLogFieldingStatistics>
    {
        public HomeGameLogFieldingStatisticsMap()
        {
            Map(m => m.Putouts).Index(71);
            Map(m => m.Assists).Index(72);
            Map(m => m.Errors).Index(73);
            Map(m => m.PassedBalls).Index(74);
            Map(m => m.DoublePlays).Index(75);
            Map(m => m.TriplePlays).Index(76);
        }
    }

    #region Home Lineup Mapping
    public sealed class HomeGameLineupLeadoffBatterMap : ClassMap<GameLineupBatter>
    {
        public HomeGameLineupLeadoffBatterMap()
        {
            Map(m => m.PlayerId).Index(132);
            Map(m => m.PlayerName).Index(133);
            Map(m => m.PlayerPosition).Index(134);
        }
    }

    public sealed class HomeGameLineupSecondBatterMap : ClassMap<GameLineupBatter>
    {
        public HomeGameLineupSecondBatterMap()
        {
            Map(m => m.PlayerId).Index(135);
            Map(m => m.PlayerName).Index(136);
            Map(m => m.PlayerPosition).Index(137);
        }
    }

    public sealed class HomeGameLineupThirdBatterMap : ClassMap<GameLineupBatter>
    {
        public HomeGameLineupThirdBatterMap()
        {
            Map(m => m.PlayerId).Index(138);
            Map(m => m.PlayerName).Index(139);
            Map(m => m.PlayerPosition).Index(140);
        }
    }

    public sealed class HomeGameLineupCleanupBatterMap : ClassMap<GameLineupBatter>
    {
        public HomeGameLineupCleanupBatterMap()
        {
            Map(m => m.PlayerId).Index(141);
            Map(m => m.PlayerName).Index(142);
            Map(m => m.PlayerPosition).Index(143);
        }
    }

    public sealed class HomeGameLineupFifthBatterMap : ClassMap<GameLineupBatter>
    {
        public HomeGameLineupFifthBatterMap()
        {
            Map(m => m.PlayerId).Index(144);
            Map(m => m.PlayerName).Index(145);
            Map(m => m.PlayerPosition).Index(146);
        }
    }

    public sealed class HomeGameLineupSixthBatterMap : ClassMap<GameLineupBatter>
    {
        public HomeGameLineupSixthBatterMap()
        {
            Map(m => m.PlayerId).Index(147);
            Map(m => m.PlayerName).Index(148);
            Map(m => m.PlayerPosition).Index(149);
        }
    }

    public sealed class HomeGameLineupSeventhBatterMap : ClassMap<GameLineupBatter>
    {
        public HomeGameLineupSeventhBatterMap()
        {
            Map(m => m.PlayerId).Index(150);
            Map(m => m.PlayerName).Index(151);
            Map(m => m.PlayerPosition).Index(152);
        }
    }

    public sealed class HomeGameLineupEighthBatterMap : ClassMap<GameLineupBatter>
    {
        public HomeGameLineupEighthBatterMap()
        {
            Map(m => m.PlayerId).Index(153);
            Map(m => m.PlayerName).Index(154);
            Map(m => m.PlayerPosition).Index(155);
        }
    }

    public sealed class HomeGameLineupNinthBatterMap : ClassMap<GameLineupBatter>
    {
        public HomeGameLineupNinthBatterMap()
        {
            Map(m => m.PlayerId).Index(156);
            Map(m => m.PlayerName).Index(157);
            Map(m => m.PlayerPosition).Index(158);
        }
    }

    public sealed class HomeGameLineupMap : ClassMap<GameLineup>
    {
        public HomeGameLineupMap()
        {
            References<HomeGameLineupLeadoffBatterMap>(r => r.LeadoffBatter);
            References<HomeGameLineupSecondBatterMap>(r => r.SecondBatter);
            References<HomeGameLineupThirdBatterMap>(r => r.ThirdBatter);
            References<HomeGameLineupCleanupBatterMap>(r => r.CleanupBatter);
            References<HomeGameLineupFifthBatterMap>(r => r.FifthBatter);
            References<HomeGameLineupSixthBatterMap>(r => r.SixthBatter);
            References<HomeGameLineupSeventhBatterMap>(r => r.SeventhBatter);
            References<HomeGameLineupEighthBatterMap>(r => r.EighthBatter);
            References<HomeGameLineupNinthBatterMap>(r => r.NinthBatter);
        }
    }
    #endregion

    public sealed class GameLogMap : ClassMap<GameLog>
    {
        public GameLogMap()
        {
            Map(m => m.GameDate)
                .Index(0)
                .Convert(c => RetrosheetDateParser.Parse(c.Row[0])
                    ?? throw new FormatException($"Game log record has an unparseable or missing GameDate: '{c.Row[0]}'."));
            Map(m => m.GameNumber).Index(1);
            Map(m => m.DayOfWeek).Index(2);
            Map(m => m.VisitorTeamCode).Index(3);
            Map(m => m.VisitorLeague).Index(4);
            Map(m => m.VisitorGameNumber).Index(5);
            Map(m => m.HomeTeamCode).Index(6);
            Map(m => m.HomeLeague).Index(7);
            Map(m => m.HomeGameNumber).Index(8);
            Map(m => m.VisitorScore).Index(9);
            Map(m => m.HomeScore).Index(10);
            Map(m => m.GameLengthOuts).Index(11);
            Map(m => m.DayOrNight).Index(12);
            Map(m => m.CompletionInfo).Index(13);
            Map(m => m.ForfeitInfo).Index(14);
            Map(m => m.ProtestInfo).Index(15);
            Map(m => m.ParkCode).Index(16);
            Map(m => m.GameAttendance)
                .Index(17)
                .Convert(c => string.IsNullOrWhiteSpace(c.Row[17]) ? (int?)null : int.Parse(c.Row[17]));
            Map(m => m.GameLengthMinutes).Index(18);
            Map(m => m.VisitorScoreLine).Index(19);
            Map(m => m.HomeScoreLine).Index(20);
            References<VisitorGameLogHittingStatisticsMap>(v => v.VisitorHitting); //21-37
            References<VisitorGameLogPitchingStatisticsMap>(v => v.VisitorPitching); //38-42
            References<VisitorGameLogFieldingStatisticsMap>(v => v.VisitorFielding); //43-48
            References<HomeGameLogHittingStatisticsMap>(h => h.HomeHitting); //49-65
            References<HomeGameLogPitchingStatisticsMap>(h => h.HomePitching); //66-70
            References<HomeGameLogFieldingStatisticsMap>(h => h.HomeFielding); //71-76
            Map(m => m.UmpireHomeId).Index(77);
            Map(m => m.UmpireHomeName).Index(78);
            Map(m => m.UmpireFirstId).Index(79);
            Map(m => m.UmpireFirstName).Index(80);
            Map(m => m.UmpireSecondId).Index(81);
            Map(m => m.UmpireSecondName).Index(82);
            Map(m => m.UmpireThirdId).Index(83);
            Map(m => m.UmpireThirdName).Index(84);
            Map(m => m.UmpireLeftId)
                .Index(85)
                .Convert(c =>
                {
                    return c.Value.UmpireLeftId == "(none)" || string.IsNullOrEmpty(c.Value.UmpireLeftId)
                        ? null
                        : c.Value.UmpireLeftId;
                });
            Map(m => m.UmpireLeftName).Index(86);
            Map(m => m.UmpireRightId)
                .Index(87)
                .Convert(c =>
                {
                    return c.Value.UmpireRightId == "(none)" || string.IsNullOrEmpty(c.Value.UmpireRightId)
                        ? null
                        : c.Value.UmpireRightId;
                });
            Map(m => m.UmpireRightName).Index(88);

            Map(m => m.VisitorManagerId).Index(89);
            Map(m => m.VisitorManagerName).Index(90);
            Map(m => m.HomeManagerId).Index(91);
            Map(m => m.HomeManagerName).Index(92);
            Map(m => m.WinningPitcherId).Index(93);
            Map(m => m.WinningPitcherName).Index(94);
            Map(m => m.LosingPitcherId).Index(95);
            Map(m => m.LosingPitcherName).Index(96);
            Map(m => m.SavingPitcherId)
                .Index(97)
                .Convert(c =>
                {
                    return c.Value.SavingPitcherId == "(none)" || string.IsNullOrEmpty(c.Value.SavingPitcherId)
                        ? null
                        : c.Value.SavingPitcherId;
                });
            Map(m => m.SavingPitcherName).Index(98);
            Map(m => m.GameWinningPlayerId)
                .Index(99)
                .Convert(c =>
                {
                    return c.Value.GameWinningPlayerId == "(none)" || string.IsNullOrEmpty(c.Value.GameWinningPlayerId)
                        ? null
                        : c.Value.GameWinningPlayerId;
                });
            Map(m => m.GameWinningPlayerName).Index(100);
            Map(m => m.VisitorStartingPitcherId).Index(101);
            Map(m => m.VisitorStartingPitcherName).Index(102);
            Map(m => m.HomeStartingPitcherId).Index(103);
            Map(m => m.HomeStartingPitcherName).Index(104);
            References<VisitorGameLineupMap>(r => r.VisitorStartingLineup);
            References<HomeGameLineupMap>(r => r.HomeStartingLineup);
            Map(m => m.AdditionalInformation).Index(159);
            Map(m => m.AcquisitionInfo).Index(160);
        }
    }
}
