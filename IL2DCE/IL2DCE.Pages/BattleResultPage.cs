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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using maddox.game;
using maddox.game.play;

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

        public BattleResultPage(string name, FrameworkElement fe)
            : base(name, fe)
        {
        }

        public override void _enter(maddox.game.IGame play, object arg)
        {
            base._enter(play, arg);

            _game = play as IGame;

            UpdateTotalPlayerStat(play.gameInterface.Player());
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

        protected void UpdateTotalPlayerStat(IPlayer player)
        {
            IGameSingle game = (Game as IGameSingle);
            Career career = game.Core.CurrentCareer;

            IPlayerStat st = player.GetBattleStat();

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
            return string.Join(", ", dic.Select(x => string.Format("[{0}]={1}", x.Key, x.Value)));
        }

        protected string ToStringkillsTypes(Dictionary<string, double> dic)
        {
            return string.Join(", ", dic.Select(x => string.Format("{0} {1}", AircraftInfo.CreateDisplayName(x.Key), x.Value.ToString(Career.KillsFormat, Career.Culture))));
        }

        protected string ToStringTimeSpan(Dictionary<string, float> dic)
        {
            return string.Join(", ", dic.Select(x => string.Format("{0} {1}", 
                                                    AircraftInfo.CreateDisplayName(x.Key), new TimeSpan((long)(x.Value * 10000000)).ToString("hh\\:mm\\:ss"))));
        }

        protected virtual string GetResultSummary(IGameSingle game)
        {
            Career career = game.Core.CurrentCareer;
            int exp = career.Experience;
            int exp2 = game.BattleSuccess == EBattleResult.DRAW ? 100 : game.BattleSuccess == EBattleResult.SUCCESS ? 200: 0;
            int rank = career.RankIndex;
            return string.Format("Date: {0} - {1}\nExp: {2} + {3}/{4}\n{5}\n",
                                    career.Date.Value.ToString("d", DateTimeFormatInfo.InvariantInfo),
                                    game.BattleSuccess.ToString(),
                                    exp,                       // Before 
                                    exp2,                      // Add Now
                                    ((rank + 1) * 1000),       // Next Rank
                                    (exp + exp2 >= (rank + 1) * 1000) ? "Promition!" : string.Empty); // Rank Up
        }

        protected virtual string GetPlayerStat(IPlayer player)
        {
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
                                    st.kills.ToString(Career.KillsFormat, Career.Culture),
                                    st.fkills.ToString(Career.KillsFormat, Career.Culture),
                                    ToStringkillsTypes(st.killsTypes));
        }

        protected virtual string GetTotalPlayerStat()
        {
            IGameSingle game = (Game as IGameSingle);
            return game.Core.CurrentCareer.ToTotalResultString();
        }
    }
}