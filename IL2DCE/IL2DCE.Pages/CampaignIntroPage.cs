// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2016 Stefan Rothdach
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

using System.Windows;
using System.Windows.Controls;
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class CampaignIntroPage : PageDefImpl
        {
            public CampaignIntroPage()
                : base("Campaign Into", new CampaignIntro())
            {
                FrameworkElement.Start.Click += new RoutedEventHandler(Start_Click);
                FrameworkElement.Start.IsEnabled = false;
                FrameworkElement.Back.Click += new RoutedEventHandler(Back_Click);
                FrameworkElement.comboBoxSelectAirGroup.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirGroup_SelectionChanged);
            }

            void comboBoxSelectAirGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count == 1)
                {
                    ComboBoxItem itemAirGroup = e.AddedItems[0] as ComboBoxItem;
                    AirGroup airGroup = (AirGroup)itemAirGroup.Tag;

                    Game.Core.CurrentCareer.AirGroup = airGroup.AirGroupKey + "." + airGroup.SquadronIndex;
                }
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                FrameworkElement.comboBoxSelectAirGroup.Items.Clear();

                _game = play as IGame;

                Career career = Game.Core.CurrentCareer;
                CampaignInfo campaignInfo = career.CampaignInfo;

                MissionFile campaignTemplate = new MissionFile(Game, campaignInfo.InitialMissionTemplateFiles, campaignInfo.AirGroupInfos);

                foreach (AirGroup airGroup in campaignTemplate.AirGroups)
                {
                    AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                    if (airGroupInfo.ArmyIndex == career.ArmyIndex && airGroupInfo.AirForceIndex == career.AirForceIndex 
                        && campaignInfo.GetAircraftInfo(airGroup.Class).IsFlyable)
                    {
                        ComboBoxItem itemAirGroup = new ComboBoxItem();

                        itemAirGroup.Content = airGroup.DisplayName + " (" + campaignInfo.GetAircraftInfo(airGroup.Class).DisplayName + ")";
                        if (airGroup.Airstart == true)
                        {
                            itemAirGroup.Content += " [AIRSTART]";
                        }

                        itemAirGroup.Tag = airGroup;
                        FrameworkElement.comboBoxSelectAirGroup.Items.Add(itemAirGroup);
                    }
                }

                if (FrameworkElement.comboBoxSelectAirGroup.Items.Count > 0)
                {
                    FrameworkElement.comboBoxSelectAirGroup.SelectedIndex = 0;
                    FrameworkElement.Start.IsEnabled = true;
                }
                else
                {
                    FrameworkElement.Start.IsEnabled = false;
                }
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            private CampaignIntro FrameworkElement
            {
                get
                {
                    return FE as CampaignIntro;
                }
            }

            private IGame Game
            {
                get
                {
                    return _game;
                }
            }
            private IGame _game;

            private void Back_Click(object sender, RoutedEventArgs e)
            {
                if (Game.gameInterface.BattleIsRun())
                {
                    Game.gameInterface.BattleStop();
                }

                Game.gameInterface.PagePop(null);
            }

            private void Start_Click(object sender, RoutedEventArgs e)
            {
                Game.Core.ResetCampaign(Game);

                Game.gameInterface.PageChange(new BattleIntroPage(), null);
            }
        }
    }
}