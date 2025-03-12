/// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
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

using System.Windows;
using System.Windows.Controls;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using maddox.game.page;
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class SelectCampaignPage : PageDefImpl
        {
            private SelectCampaign FrameworkElement
            {
                get
                {
                    return FE as SelectCampaign;
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

            public SelectCampaignPage()
                : base("Select Campaign", new SelectCampaign())
            {
                FrameworkElement.bBack.Click += new RoutedEventHandler(bBack_Click);
                FrameworkElement.bNew.Click += new RoutedEventHandler(bNew_Click);

                FrameworkElement.ListCampaign.SelectionChanged += new SelectionChangedEventHandler(listCampaign_SelectionChanged);

                // TODO: Make button visible when it is possible to continue a campaign.
                FrameworkElement.bNew.IsEnabled = false;
                FrameworkElement.bContinue.Visibility = System.Windows.Visibility.Hidden;
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                _game = play as IGame;

                FrameworkElement.ListCampaign.Items.Clear();

                foreach (CampaignInfo campaignInfo in Game.Core.CampaignInfos)
                {
                    FrameworkElement.ListCampaign.Items.Add(campaignInfo);
                }

                if (FrameworkElement.ListCampaign.Items.Count > 0)
                {
                    FrameworkElement.ListCampaign.SelectedIndex = 0;
                }
                else
                {
                    FrameworkElement.ListCampaign.SelectedIndex = -1;
                }
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            private void bBack_Click(object sender, RoutedEventArgs e)
            {
                // Remove the selection
                Game.Core.CurrentCareer.CampaignInfo = null;

                Game.gameInterface.PagePop(null);
            }

            private void bNew_Click(object sender, RoutedEventArgs e)
            {
                Game.Core.InitCampaign();

                Game.gameInterface.PageChange(new CampaignIntroPage(), null);
            }

            private void listCampaign_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                Career career = Game.Core.CurrentCareer;

                if (e.AddedItems.Count > 0)
                {
                    CampaignInfo campaignInfo = e.AddedItems[0] as CampaignInfo;

                    MissionFile campaignTemplate = new MissionFile(Game, campaignInfo.InitialMissionTemplateFiles, campaignInfo.AirGroupInfos);

                    string description = "Available AirGroups:\n";

                    int availableAirGroups = 0;
                    foreach (AirGroup airGroup in campaignTemplate.AirGroups)
                    {
                        AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                        if (airGroupInfo.ArmyIndex == career.ArmyIndex && airGroupInfo.AirForceIndex == career.AirForceIndex 
                            && campaignInfo.GetAircraftInfo(airGroup.Class).IsFlyable)
                        {
                            description += airGroup.DisplayName + "\n";
                            availableAirGroups++;
                        }
                    }

                    if (availableAirGroups > 0)
                    {
                        career.CampaignInfo = campaignInfo;
                    }
                    else
                    {
                        description += "None for your AirForce! Please select a different campaign.\n";
                        career.CampaignInfo = null;
                    }

                    FrameworkElement.txtDesc.Text = description;
                }

                FrameworkElement.bNew.IsEnabled = career.CampaignInfo != null;
            }
        }
    }
}