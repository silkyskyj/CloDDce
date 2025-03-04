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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class SelectCareerPage : PageDefImpl
        {
            private SelectCareer FrameworkElement
            {
                get
                {
                    return FE as SelectCareer;
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

            private CampaignInfo SelectedCampaign
            {
                get
                {
                    return FrameworkElement.comboBoxSelectCampaign.SelectedItem as CampaignInfo;
                }
            }

            public SelectCareerPage()
                : base("Select Career", new SelectCareer())
            {
                FrameworkElement.Back.Click += new System.Windows.RoutedEventHandler(bBack_Click);
                FrameworkElement.New.Click += new System.Windows.RoutedEventHandler(bNew_Click);
                FrameworkElement.Delete.Click += new System.Windows.RoutedEventHandler(Delete_Click);
                FrameworkElement.Continue.Click += new System.Windows.RoutedEventHandler(bContinue_Click);

                FrameworkElement.comboBoxSelectCampaign.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectCampaign_SelectionChanged);
                FrameworkElement.ListCareer.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(listCampaign_SelectionChanged);

                FrameworkElement.Continue.IsEnabled = false;
                FrameworkElement.Delete.IsEnabled = false;
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                _game = play as IGame;

                ComboBox comboBox = FrameworkElement.comboBoxSelectCampaign;
                comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = "[All]" });
                foreach (CampaignInfo campaignInfo in Game.Core.CampaignInfos)
                {
                    comboBox.Items.Add(campaignInfo);
                }
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0: -1;

                ListBox listBox = FrameworkElement.ListCareer;
                listBox.Items.Clear();
                foreach (Career career in Game.Core.AvailableCareers)
                {
                    listBox.Items.Add(career);
                }
                listBox.SelectedIndex = FrameworkElement.ListCareer.Items.Count > 0 ? 0 : -1;
                listBox.Items.Refresh();
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            private void comboBoxSelectCampaign_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    CampaignInfo campaignInfo = SelectedCampaign;
                    if (campaignInfo != null)
                    {
                        FrameworkElement.ListCareer.Items.Filter = new Predicate<object>(ContainsCareer);
                    }
                    else
                    {
                        FrameworkElement.ListCareer.Items.Filter = null;
                    }
                }
            }

            private void bBack_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.gameInterface.PagePop(null);
            }

            private void bNew_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.gameInterface.PageChange(new CareerIntroPage(), null);
            }

            private void bContinue_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                if (Game.Core.CurrentCareer.CampaignInfo != null)
                {
                    Game.Core.InitCampaign();
                    Game.gameInterface.PageChange(new BattleIntroPage(), null);
                }
                else
                {
                    Game.gameInterface.PageChange(new SelectCampaignPage(), null);
                }
            }

            void Delete_Click(object sender, System.Windows.RoutedEventArgs e)
            {
                Game.Core.DeleteCareer(Game.Core.CurrentCareer);

                ListBox listBox = FrameworkElement.ListCareer;
                listBox.Items.Clear();
                foreach (Career career in Game.Core.AvailableCareers)
                {
                    listBox.Items.Add(career);
                }
                listBox.SelectedIndex = FrameworkElement.ListCareer.Items.Count > 0 ? 0 : -1;
                listBox.Items.Refresh();
            }

            private void listCampaign_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    Career careerSelected = e.AddedItems[0] as Career;
                    Game.Core.CurrentCareer = careerSelected;
                }

                Career career = Game.Core.CurrentCareer;
                if (career != null && career.CampaignInfo != null)
                {
                    CampaignInfo campaignInfo = career.CampaignInfo;
                    FrameworkElement.Continue.IsEnabled = career.Date < campaignInfo.EndDate;
                    FrameworkElement.Delete.IsEnabled = true;
                    FrameworkElement.textBoxStatus.Text = string.Format("{0}\n{1}\n{2}\n",
                                                                        campaignInfo.ToSummaryString(),
                                                                        career.ToCurrestStatusString(),
                                                                        career.ToTotalResultString());
                }
                else if (career != null)
                {
                    FrameworkElement.Continue.IsEnabled = false;
                    FrameworkElement.Delete.IsEnabled = true;
                    FrameworkElement.textBoxStatus.Text = string.Format("{0}\n{1}\n{2}\n",
                                                                        "Campaign [no file]\n",
                                                                        career.ToCurrestStatusString(),
                                                                        career.ToTotalResultString());
                }
                else
                {
                    FrameworkElement.Continue.IsEnabled = false;
                    FrameworkElement.Delete.IsEnabled = true;
                    FrameworkElement.textBoxStatus.Text = string.Format("{0}\n{1}\n",
                                                                        "Campaign [no file]\n",
                                                                        "Career [no file]\n");
                }

                UpdateAircraftImage(career);
            }

            public bool ContainsCareer(object obj)
            {
                Career career = obj as Career;
                return (career.CampaignInfo == SelectedCampaign);
            }

            private void UpdateAircraftImage(Career career)
            {
                AirGroup airGroup;
                if (career != null && career.CampaignInfo != null)
                {
                    MissionFile missionFile = new MissionFile(Game, career.CampaignInfo.InitialMissionTemplateFiles, career.CampaignInfo.AirGroupInfos);
                    airGroup = missionFile.AirGroups.Where(x => x.ArmyIndex == career.ArmyIndex && string.Compare(x.ToString(), career.AirGroup) == 0).FirstOrDefault();
                }
                else
                {
                    airGroup = null;
                }

                DisplayAircraftImage(airGroup != null ? airGroup.Class : string.Empty);
            }

            private void DisplayAircraftImage(string aircraftClass)
            {
                string path;;
                if (!string.IsNullOrEmpty(aircraftClass) && 
                    !string.IsNullOrEmpty(path = new AircraftImage(Game.gameInterface.ToFileSystemPath(Config.PartsFolder)).GetImagePath(aircraftClass)))
                {
                    // using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                        var decoder = new TiffBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        BitmapSource source = decoder.Frames[0];
                        FrameworkElement.imageAircraft.Source = source;
                        FrameworkElement.borderImage.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    FrameworkElement.imageAircraft.Source = null;
                    FrameworkElement.borderImage.Visibility = Visibility.Hidden;
                }
            }
        }
    }
}