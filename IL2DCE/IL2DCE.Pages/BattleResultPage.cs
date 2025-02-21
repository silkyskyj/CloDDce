// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
// Copyright (C) 2016 Stefan Rothdach & 2025 silkyskyj
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using maddox.game;
using maddox.game.play;
using maddox.game.world;
using static IL2DCE.Mission.Mission;

namespace IL2DCE.Pages
{
    public class BattleResultPage : PageDefImpl
    {
        protected IGame Game
        {
            get
            {
                return _game;
            }
        }
        protected IGame _game;

        private Dictionary<string, int> killsAircraft = new Dictionary<string, int>();
        private Dictionary<string, int> killsFliendlyAircraft = new Dictionary<string, int>();
        private Dictionary<string, int> killsGroundUnit = new Dictionary<string, int>();
        private Dictionary<string, int> killsFliendlyGroundUnit = new Dictionary<string, int>();

        public BattleResultPage(string name, FrameworkElement fe)
            : base(name, fe)
        {
        }

        public override void _enter(maddox.game.IGame play, object arg)
        {
            base._enter(play, arg);

            _game = play as IGame;

            CreatePlayerStat();
        }

        public override void _leave(maddox.game.IGame play, object arg)
        {
            base._leave(play, arg);

            _game = null;
        }

        protected void ReFly_Click(object sender, RoutedEventArgs e)
        {
            Game.gameInterface.PageChange(new BattleIntroPage(), null);
        }

        protected void Back_Click(object sender, RoutedEventArgs e)
        {
            Career career = Game.Core.CurrentCareer;
            if (career.BattleType == EBattleType.QuickMission)
            {
                Game.gameInterface.PageChange(new QuickMissionPage(), null);
            }
            else
            {
                Game.gameInterface.PagePop(null);
            }
        }

        protected void Fly_Click(object sender, RoutedEventArgs e)
        {
            UpdateTotalPlayerStat();

            Career career = Game.Core.CurrentCareer;
            if (career.BattleType == EBattleType.QuickMission)
            {
                Game.gameInterface.PageChange(new QuickMissionPage(), null);
            }
            else
            {
                CampaignStatus status = Game.Core.AdvanceCampaign(Game);
                if (status != CampaignStatus.DateEnd)
                {
                    Game.gameInterface.PageChange(new BattleIntroPage(), null);
                }
                else
                {
                    Game.gameInterface.PageChange(new CampaignCompletionPage(), null);
                }
            }
        }

        private void CreatePlayerStat()
        {
            Career career = Game.Core.CurrentCareer;
            Config config = Game.Core.Config;
            double killsScoreOver = config.StatKillsOver;
            int statType = config.StatType;

            switch (statType)
            {
                case 1: // DefaultAPI
                    ;
                    break;

                case 2: // Battle's DamageInfo 
                    {
                        int army = career.ArmyIndex;
                        ArrayList battleDamageVictims = Game.battleGetDamageVictims();
                        foreach (var item in battleDamageVictims)
                        {
                            if (item is AiActor)
                            {
                                AiActor actor = item as AiActor;
                                ArrayList damageInitiatorsArray = Game.battleGetDamageInitiators(actor);
                                foreach (var initiator in damageInitiatorsArray)
                                {
                                    if (initiator is DamagerScore)
                                    {
                                        DamagerScore score = initiator as DamagerScore;
                                        if (score.initiator.Actor != null)
                                        {
                                            Debug.WriteLine("Actor.IsValid: {0}={1}", score.initiator.Actor.Name(), score.initiator.Actor.IsValid());
                                            if (score.score > killsScoreOver && score.initiator != null && score.initiator.Player != null)
                                            {
                                                int armyActor = actor.Army();
                                                if (actor is AiAircraft)
                                                {
                                                    AiAircraft aiAircraft = item as AiAircraft;
                                                    Debug.WriteLine("AiAircraft: {0}={1}", aiAircraft.InternalTypeName(), aiAircraft.Group().Name());
                                                    if (armyActor != army)
                                                    {
                                                        AddKillsCount(killsAircraft, aiAircraft.InternalTypeName());
                                                    }
                                                    else
                                                    {
                                                        AddKillsCount(killsFliendlyAircraft, aiAircraft.InternalTypeName());
                                                    }
                                                }
                                                else if (actor is AiGroundActor)
                                                {
                                                    AiGroundActor aiGroundActor = item as AiGroundActor;
                                                    Debug.WriteLine("AiGroundActor: {0}={1}", aiGroundActor.InternalTypeName(), aiGroundActor.Group().Name());
                                                    if (armyActor != army)
                                                    {
                                                        AddKillsCount(killsAircraft, aiGroundActor.InternalTypeName());
                                                    }
                                                    else
                                                    {
                                                        AddKillsCount(killsFliendlyAircraft, aiGroundActor.InternalTypeName());
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;

                case 3: // Battle's DamageInfo (Linq)
                    {
                        ArrayList battleDamageVictims = Game.battleGetDamageVictims();
                        var damageInitiatorsArray = battleDamageVictims.ToArray().Where(x => x is AiActor).Select(x => Game.battleGetDamageInitiators(x as AiActor));
                        var playerDamageScores = damageInitiatorsArray.Select(x => x.ToArray().Where(y => y is DamagerScore && (y as DamagerScore).initiator != null && (y as DamagerScore).initiator.Player != null)).Select(z => z as DamagerScore);
                        var playerKillsAircraft = playerDamageScores.Where(x => x.initiator.Actor != null && x.initiator.Actor is AiAircraft && x.score > killsScoreOver);
                        var playerKillsGround = playerDamageScores.Where(x => x.initiator.Actor != null && x.initiator.Actor is AiGroundActor && x.score > killsScoreOver);
                        var playerDamageAircraft = playerDamageScores.Where(x => x.initiator.Actor != null && x.initiator.Actor is AiAircraft && x.score < killsScoreOver);
                        var playerDamageGround = playerDamageScores.Where(x => x.initiator.Actor != null && x.initiator.Actor is AiGroundActor && x.score < killsScoreOver);

                        // .... 

                    }
                    break;

                case 0: // Check Mission OnActorDead methos score value
                default:
                    {
                        // Check Type 2: 
                        char[] split = new char[] { Mission.Mission.ActorDeadInfoSplitChar };
                        int army = career.ArmyIndex;
                        Mission.Mission mission = Game.Core.Mission as Mission.Mission;
                        string playerActorName = mission.PlayerActorName;
                        var playerDamegedActor = mission.ActorDead.Where(x => x.Value.ToArray().Any(y => y.initiator != null && y.initiator.Player != null));
                        List<string> playerKillsActor = new List<string>();
                        foreach (var item in playerDamegedActor)
                        {
                            string[] keys = item.Key.Split(split);
                            if (keys.Length >= (int)ActorDeadInfoKey.Count)
                            {
                                int armyActor;
                                int actorType;
                                ;
                                if (int.TryParse(keys[(int)ActorDeadInfoKey.Army], out armyActor) && int.TryParse(keys[(int)ActorDeadInfoKey.ActorType], out actorType))
                                {
                                    if (string.Compare(keys[(int)ActorDeadInfoKey.ActorName], playerActorName, true) != 0)
                                    {
                                        double totalScore = item.Value.Sum(x => x.score);
                                        double playerScore = item.Value.Where(x => x.initiator != null && x.initiator.Player != null).Sum(x => x.score);
                                        if (playerScore > totalScore * killsScoreOver)
                                        {
                                            Debug.WriteLine("Paler Kill: {0}={1}/{2}", item.Key, playerScore, totalScore);
                                            string typeName = keys[(int)ActorDeadInfoKey.ActorTypeName];
                                            if (armyActor != army)
                                            {   // Enemy kill
                                                if (actorType == 0)
                                                {
                                                    AddKillsCount(killsAircraft, typeName);
                                                }
                                                else
                                                {
                                                    AddKillsCount(killsGroundUnit, typeName);
                                                }
                                            }
                                            else
                                            {   // Friendly kill
                                                if (actorType == 0)
                                                {
                                                    AddKillsCount(killsFliendlyAircraft, typeName);
                                                }
                                                else
                                                {
                                                    AddKillsCount(killsFliendlyGroundUnit, typeName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        protected void UpdateTotalPlayerStat()
        {
            Config config = Game.Core.Config;
            int statType = config.StatType;
            if (statType == 1)
            {
                UpdateTotalPlayerStatDefaultAPI();
            }
            else
            {
                Career career = Game.Core.CurrentCareer;
                IPlayerStat st = Game.gameInterface.Player().GetBattleStat();

                career.Takeoffs += st.takeoffs;
                career.Landings += st.landings;
                career.Bails += st.bails;
                career.Deaths += st.deaths;
                career.Kills += killsAircraft.Sum(x => x.Value);
                string killsTypes = ToString(killsAircraft);
                if (!string.IsNullOrEmpty(killsTypes))
                {
                    if (career.KillsHistory.ContainsKey(career.Date.Value.Date))
                    {
                        career.KillsHistory[career.Date.Value.Date] += ", " + killsTypes;
                    }
                    else
                    {
                        career.KillsHistory.Add(career.Date.Value.Date, killsTypes);
                    }
                }

                career.KillsGround += killsGroundUnit.Sum(x => x.Value);
                killsTypes = ToString(killsGroundUnit);
                if (!string.IsNullOrEmpty(killsTypes))
                {
                    if (career.KillsGroundHistory.ContainsKey(career.Date.Value.Date))
                    {
                        career.KillsGroundHistory[career.Date.Value.Date] += ", " + killsTypes;
                    }
                    else
                    {
                        career.KillsGroundHistory.Add(career.Date.Value.Date, killsTypes);
                    }
                }
            }
        }

        protected void UpdateTotalPlayerStatDefaultAPI()
        {
            IGameSingle game = (Game as IGameSingle);
            IPlayer player = game.gameInterface.Player();
            IPlayerStat st = player.GetBattleStat();

            Career career = game.Core.CurrentCareer;

            career.Takeoffs += st.takeoffs;
            career.Landings += st.landings;
            career.Bails += st.bails;
            career.Deaths += st.deaths;
            career.Kills += ((int)(st.kills * 100)) / 100.0;
            string killsTypes = ToStringkillsTypes(st.killsTypes);
            if (!string.IsNullOrEmpty(killsTypes))
            {
                if (career.KillsHistory.ContainsKey(career.Date.Value.Date))
                {
                    career.KillsHistory[career.Date.Value.Date] += ", " + killsTypes;
                }
                else
                {
                    career.KillsHistory.Add(career.Date.Value.Date, killsTypes);
                }
            }
        }

        protected string ToString<T>(Dictionary<string, T> dic)
        {
            return string.Join(", ", dic.Select(x => string.Format("{0} {1}", x.Key, x.Value)));
        }

        protected string ToStringkillsTypes(Dictionary<string, double> dic)
        {
            return string.Join(", ", dic.Select(x => string.Format("{0} {1}", AircraftInfo.CreateDisplayName(x.Key), x.Value.ToString(Career.KillsFormat, Config.Culture))));
        }

        protected string ToStringTimeSpan(Dictionary<string, float> dic)
        {
            return string.Join(", ", dic.Select(x => string.Format("{0} {1}", 
                                                    AircraftInfo.CreateDisplayName(x.Key), new TimeSpan((long)(x.Value * 10000000)).ToString("hh\\:mm\\:ss"))));
        }

        protected virtual string GetResultSummary()
        {
            IGameSingle game = Game as IGameSingle;
            Career career = game.Core.CurrentCareer;
            int exp = career.Experience;
            int exp2 = game.BattleSuccess == EBattleResult.DRAW ? 100 : game.BattleSuccess == EBattleResult.SUCCESS ? 200: 0;
            int rank = career.RankIndex;
            return string.Format("Date: {0} - {1}\nExp: {2} + {3} [Next Rank {4}]\n{5}\n",
                                    career.Date.Value.ToString("d", DateTimeFormatInfo.InvariantInfo),
                                    game.BattleSuccess.ToString(),
                                    exp,                       // Before 
                                    exp2,                      // Add Now
                                    rank < Career.RankMax ? ((rank + 1) * 1000).ToString():  " - ",       // Next Rank
                                    rank < Career.RankMax && (exp + exp2 >= (rank + 1) * 1000) ? "Promition!" : string.Empty); // Rank Up
        }

        protected virtual string GetPlayerStat()
        {
            IGameSingle game = Game as IGameSingle;
            IPlayer player = game.gameInterface.Player();
            IPlayerStat st = player.GetBattleStat();

            return String.Format("PlayerStat [{0}] {1}\n Flying Time: {2}\n Takeoffs: {3}\n Landings: {4}\n Deaths: {5}\n Bails: {6}\n Ditches: {7}\n PlanesWrittenOff: {8}\n" +
                                    " Kills[Aircraft]: {9}\n Kills[GroundUnit]: {10}\n Friendly Kills[Aircraft]: {11}\n Friendly Kills[GroundUnit]: {12}\n" +
                                    "\n KillsTypes\n  Aircraft: {13}\n  GroundUnit: {14}\n  Aircraft[Friendly]: {15}\n  GroundUnit[Friendly]: {16}\n",
                                    player?.Name() ?? string.Empty,
                                    Game.Core.CurrentCareer.ToString(),
                                    ToStringTimeSpan(st.tTotalTypes),
                                    st.takeoffs,
                                    st.landings,
                                    st.deaths,
                                    st.bails,
                                    st.ditches,
                                    st.planesWrittenOff,
                                    killsAircraft.Sum(x => x.Value),
                                    killsGroundUnit.Sum(x => x.Value),
                                    killsFliendlyAircraft.Sum(x => x.Value),
                                    killsFliendlyGroundUnit.Sum(x => x.Value),
                                    ToString(killsAircraft),
                                    ToString(killsGroundUnit),
                                    ToString(killsFliendlyAircraft),
                                    ToString(killsFliendlyGroundUnit));
        }

        protected virtual string GetPlayerStatDefaultAPI()
        {
            IGameSingle game = Game as IGameSingle;
            IPlayer player = game.gameInterface.Player();
            IPlayerStat st = player.GetBattleStat();
            return String.Format("PlayerStat [{0}] {1}\n Flying Time: {2}\n Takeoffs: {3}\n Landings: {4}\n Deaths: {5}\n Bails: {6}\n Ditches: {7}\n PlanesWrittenOff: {8}\n" +
                                    " Kills: {9}\n Friendly Kills: {10}\n KillsTypes: {11}\n",
                                    player?.Name() ?? string.Empty,
                                    Game.Core.CurrentCareer.ToString(),
                                    ToStringTimeSpan(st.tTotalTypes),
                                    st.takeoffs,
                                    st.landings,
                                    st.deaths,
                                    st.bails,
                                    st.ditches,
                                    st.planesWrittenOff,
                                    st.kills.ToString(Career.KillsFormat, Config.Culture),
                                    st.fkills.ToString(Career.KillsFormat, Config.Culture),
                                    ToStringkillsTypes(st.killsTypes));
        }


        private void AddKillsCount(Dictionary<string, int> dic, string name, int count = 1)
        {
            const string delStart = ".";
            int idx = name.IndexOf(delStart, StringComparison.CurrentCultureIgnoreCase);
            if (idx != -1)
            {
                name = name.Substring(idx + delStart.Length);
            }
            if (dic.ContainsKey(name))
            {
                dic[name] += 1;
            }
            else
            {
                dic.Add(name, 1);
            }
        }

        private DamagerScore[] GetPlayerDamagerScore(ArrayList listDamage)
        {
            return listDamage.ToArray().Where(x => x is DamagerScore && (x as DamagerScore).initiator.Player != null).ToArray() as DamagerScore[];
        }

        protected virtual string GetTotalPlayerStat()
        {
            return Game.Core.CurrentCareer.ToTotalResultString();
        }
    }
}