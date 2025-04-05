// IL2DCE: A dynamic campaign engine & dynamic mission for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
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

        protected PlayerStats PlayerStat
        {
            get;
            set;
        }

        protected EPlayerStatsType StatType
        {
            get;
            set;
        }

        public BattleResultPage(string name, FrameworkElement fe)
            : base(name, fe)
        {
        }

        public override void _enter(maddox.game.IGame play, object arg)
        {
            base._enter(play, arg);

            _game = play as IGame;

            Config config = Game.Core.Config;
            Career career = Game.Core.CurrentCareer;
            Mission.Mission mission = Game.Core.Mission as Mission.Mission;

            try
            {
                (FE.FindName("Back") as Button).Click += new RoutedEventHandler(Back_Click);
                (FE.FindName("ReFly") as Button).Click += new RoutedEventHandler(ReFly_Click);
                (FE.FindName("Fly") as Button).Click += new RoutedEventHandler(Fly_Click);

                TextBox textBoxDescription = FE.FindName("textBoxDescription") as TextBox;
                TextBox textBoxSlide = FE.FindName("textBoxSlide") as TextBox;
                FontFamily fontFamiry = new FontFamily(Config.DefaultFixedFontName);
                textBoxDescription.FontFamily = fontFamiry;
                textBoxDescription.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                textBoxDescription.MinHeight = 800;
                textBoxDescription.UpdateLayout();
                textBoxSlide.FontFamily = fontFamiry;
                textBoxSlide.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                textBoxSlide.MinWidth = 940;
                textBoxSlide.MinHeight = 940;
                textBoxSlide.UpdateLayout();

                PlayerStat = new PlayerStats(Game, career.ArmyIndex, mission != null ? mission.PlayerActorName : string.Empty, config.StatKillsOver);
                EPlayerStatsType type = Enum.IsDefined(typeof(EPlayerStatsType), config.StatType) ? (EPlayerStatsType)config.StatType : EPlayerStatsType.Api;
                StatType = PlayerStat.Create(type, mission != null ? mission.ActorDead : null);

                string result = GetResultSummary() + GetPlayerStat();
                textBoxDescription.Text = result;
                textBoxSlide.Text = GetTotalPlayerStat();
            }
            catch (Exception ex)
            {
                string message = string.Format("{0} - {1} {2} {3} {4} {5}", "BattleResultPage._enter", 
                    ex.Message, career.PilotName, career.CampaignInfo.Id, mission != null && mission.ActorDead != null ? mission.ActorDead.Count: -1, ex.StackTrace);
                Core.WriteLog(message);
                MessageBox.Show(string.Format("{0}", ex.Message), Config.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            else if (career.BattleType == EBattleType.Campaign)
            {
                Game.gameInterface.PageChange(new SelectCareerPage(), null);
            }
            else
            {
                Game.gameInterface.PagePop(null);
            }
        }

        protected void Fly_Click(object sender, RoutedEventArgs e)
        {
            Career career = Game.Core.CurrentCareer;
            try
            {
                if (career.BattleType == EBattleType.QuickMission)
                {
                    string valueSummary = string.Format("{0}|{1}|{2}",
                        career.CampaignInfo.Id, string.IsNullOrEmpty(career.AirGroupDisplay) ? AirGroup.CreateDisplayName(career.AirGroup) : career.AirGroupDisplay, career.Aircraft);
                    PlayerStat.UpdatePlayerStat(StatType, career, DateTime.Now, valueSummary);

                    Game.Core.UpdateResult(career);
                    Game.gameInterface.PageChange(new QuickMissionPage(), null);
                }
                else
                {
                    PlayerStat.UpdatePlayerStat(StatType, career, career.Date.Value);

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
            catch (Exception ex)
            {
                string message = string.Format("{0} - {1} {2} {3} {4}", "BattleResultPage.Fly_Click", ex.Message, career.PilotName, career.CampaignInfo.Id, ex.StackTrace);
                Core.WriteLog(message);
                MessageBox.Show(string.Format("{0}", ex.Message), Config.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected string ToStringTimeSpan(Dictionary<string, float> dic, string separator = Config.CommaStr)
        {
            return string.Join(separator, dic.Select(x => string.Format("{0} {1}", 
                                                    AircraftInfo.CreateDisplayName(x.Key), new TimeSpan((long)(x.Value * 10000000)).ToString("hh\\:mm\\:ss"))));
        }

        protected virtual string GetResultSummary()
        {
            IGameSingle game = Game as IGameSingle;
            Career career = game.Core.CurrentCareer;
            int exp = career.Experience;
            int exp2 = game.BattleSuccess == EBattleResult.DRAW ? Config.ExpDraw : game.BattleSuccess == EBattleResult.SUCCESS ? Config.ExpSuccess: Config.ExpFail;
            int rank = career.RankIndex;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(DateTimeFormatInfo.InvariantInfo, "Date: {0:d} - {1}", career.Date.Value, game.BattleSuccess.ToString());
            sb.AppendLine();
            // Before + Add Now [Next Rank]
            sb.AppendFormat(DateTimeFormatInfo.InvariantInfo, "Exp: {0} + {1} [Next Rank {2}]", 
                            exp, exp2, rank < Rank.RankMax ? ((rank + 1) * Config.RankupExp).ToString(CultureInfo.InvariantCulture.NumberFormat) : " - ");
            sb.AppendLine();
            // Rank Up
            sb.AppendFormat(rank < Rank.RankMax && (exp + exp2 >= (rank + 1) * Config.RankupExp) ? "Promition!" : string.Empty);
            sb.AppendLine();
            return sb.ToString();
        }

        protected virtual string GetPlayerStat()
        {
            if (StatType == EPlayerStatsType.Api)
            {
                return GetPlayerStatDefaultAPI();
            }
            else
            {
                IGameSingle game = Game as IGameSingle;
                IPlayer player = game.gameInterface.Player();
                IPlayerStat st = player.GetBattleStat();

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("PlayerStat [{0}]", player?.Name() ?? string.Empty);
                sb.AppendLine();
                sb.AppendFormat("Pilot Name : {0}", Game.Core.CurrentCareer.ToString());
                sb.AppendLine();
                sb.AppendFormat("Flying Time: {0}", ToStringTimeSpan(st.tTotalTypes));
                sb.AppendLine();
                sb.AppendLine();
                sb.Append(PlayerStats.ToStringSummary(st, false));
                sb.AppendLine(PlayerStat.ToStringTotal());
                sb.AppendLine("[Kills Type]");
                sb.Append(PlayerStat.ToStringKillsType());
                return sb.ToString();
            }
        }

        protected virtual string GetPlayerStatDefaultAPI()
        {
            IGameSingle game = Game as IGameSingle;
            IPlayer player = game.gameInterface.Player();
            IPlayerStat st = player.GetBattleStat();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("PlayerStat [{0}]", player?.Name() ?? string.Empty);
            sb.AppendLine();
            sb.AppendFormat("Pilot Name : {0}", Game.Core.CurrentCareer.ToString());
            sb.AppendLine();
            sb.AppendFormat("Flying Time: {0}", ToStringTimeSpan(st.tTotalTypes));
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(PlayerStats.ToStringSummary(st));
            sb.AppendLine("[Kills Type]");
            sb.Append(PlayerStats.ToStringkillsTypes(st.killsTypes));
            return sb.ToString();
        }

        protected virtual string GetTotalPlayerStat()
        {
            return string.Format("Total Status [up to previous time]\n{0}", Game.Core.CurrentCareer.ToStringTotalResult());
        }
    }
}