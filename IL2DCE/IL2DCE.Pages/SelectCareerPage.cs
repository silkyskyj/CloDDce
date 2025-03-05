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
using maddox.game.page;
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class SelectCareerPage : PageDefImpl
        {

            #region Property

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

            private int SelectedArmyIndex
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectArmy.SelectedItem as ComboBoxItem;
                    if (selected != null)
                    {
                        return (int)selected.Tag;
                    }

                    return -1;
                }
            }

            private int SelectedAirForceIndex
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectAirForce.SelectedItem as ComboBoxItem;
                    if (selected != null)
                    {
                        return (int)selected.Tag;
                    }

                    return -1;
                }
            }

            private Career SelectedCareer
            {
                get
                {
                    return FrameworkElement.ListCareer.SelectedItem as Career;
                }
            }

            #endregion

            #region Variable

            private bool hookComboSelectionChanged = false;

            #endregion

            public SelectCareerPage()
                : base("Select Career", new SelectCareer())
            {
                FrameworkElement.comboBoxSelectCampaign.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectCampaign_SelectionChanged);
                FrameworkElement.comboBoxSelectArmy.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectArmy_SelectionChanged);
                FrameworkElement.comboBoxSelectAirForce.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirForce_SelectionChanged);
                FrameworkElement.ListCareer.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(listCampaign_SelectionChanged);

                FrameworkElement.Back.Click += new System.Windows.RoutedEventHandler(bBack_Click);
                FrameworkElement.New.Click += new System.Windows.RoutedEventHandler(bNew_Click);
                FrameworkElement.Delete.Click += new System.Windows.RoutedEventHandler(Delete_Click);
                FrameworkElement.Continue.Click += new System.Windows.RoutedEventHandler(bContinue_Click);

                FrameworkElement.Continue.IsEnabled = false;
                FrameworkElement.Delete.IsEnabled = false;
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                _game = play as IGame;

                UpdateCampaignComboBoxInfo();
                UpdateArmyComboBoxInfo();
                UpdateAirForceComboBoxInfo();

                ListBox listBox = FrameworkElement.ListCareer;
                listBox.Items.Clear();
                foreach (Career career in Game.Core.AvailableCareers)
                {
                    listBox.Items.Add(career);
                }
                // listBox.Items.Refresh();
                listBox.SelectedIndex = FrameworkElement.ListCareer.Items.Count > 0 ? 0 : -1;
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            #region Button Click

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
                Career career = SelectedCareer;
                if (career != null && career.CampaignInfo != null)
                {
                    Game.Core.CurrentCareer = career;
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
                Career career = SelectedCareer;
                if (career != null)
                {
                    ListBox listBox = FrameworkElement.ListCareer;
                    int old = listBox.SelectedIndex;
                    listBox.Items.Remove(career);
                    Game.Core.DeleteCareer(career);
                    //listBox.Items.Refresh();
                    listBox.SelectedIndex = listBox.Items.Count > 0 && old < listBox.Items.Count ? old : -1;
                }
            }

            #endregion

            #region ComboBox & ListBox SelectionChanged

            private void comboBoxSelectCampaign_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {

                }

                UpdateCareerList();
                UpdateButtonStatus();
            }

            private void comboBoxSelectArmy_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    UpdateAirForceComboBoxInfo();
                }

                UpdateCareerList();
                UpdateButtonStatus();
            }

            private void comboBoxSelectAirForce_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                }

                UpdateCareerList();
                UpdateButtonStatus();
            }

            private void listCampaign_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            {
                Career career = SelectedCareer;
                if (career != null && career.CampaignInfo != null)
                {
                    CampaignInfo campaignInfo = career.CampaignInfo;
                    FrameworkElement.textBoxStatus.Text = string.Format("{0}\n{1}\n{2}\n",
                                                                        campaignInfo.ToSummaryString(),
                                                                        career.ToCurrestStatusString(),
                                                                        career.ToTotalResultString());
                }
                else if (career != null)
                {
                    FrameworkElement.textBoxStatus.Text = string.Format("{0}\n{1}\n{2}\n",
                                                                        "Campaign [no file]\n",
                                                                        career.ToCurrestStatusString(),
                                                                        career.ToTotalResultString());
                }
                else
                {
                    FrameworkElement.textBoxStatus.Text = string.Format("{0}\n{1}\n",
                                                                        "Campaign [no file]\n",
                                                                        "Career [no file]\n");
                }

                UpdateAircraftImage(career);
                UpdateButtonStatus();
            }

            #endregion

            private void UpdateCampaignComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectCampaign;
                comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = "[All]" });
                foreach (CampaignInfo campaignInfo in Game.Core.CampaignInfos)
                {
                    comboBox.Items.Add(campaignInfo);
                }
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateArmyComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectArmy;
                comboBox.Items.Add(new ComboBoxItem() { Tag = -1, Content = "[All]" });
                for (int i = 0; i < (int)EArmy.Count; i++)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = i + 1, Content = Career.Army[i] });
                }
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }
            private void UpdateAirForceComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectAirForce;
                comboBox.Items.Clear();

                comboBox.Items.Add(new ComboBoxItem() { Tag = -1, Content = "[All]" });
                int armyIndex = SelectedArmyIndex;
                if (armyIndex != -1)
                {
                    if (armyIndex == (int)EArmy.Red)
                    {
                        for (int i = 0; i < (int)AirForceRed.Count; i++)
                        {
                            comboBox.Items.Add(new ComboBoxItem() { Tag = i + 1, Content = Career.AirForce[i] });
                        }
                    }
                    else if (armyIndex == (int)EArmy.Blue)
                    {
                        int diff = (int)AirForceRed.Count;
                        for (int i = 0; i < (int)AirForceBlue.Count; i++)
                        {
                            comboBox.Items.Add(new ComboBoxItem() { Tag = i + 1, Content = Career.AirForce[i + diff] });
                        }
                    }
                }
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateCareerList()
            {
                ListBox listBox = FrameworkElement.ListCareer;
                listBox.Items.Filter = new Predicate<object>(ContainsCareer);
                listBox.SelectedIndex = listBox.Items.Count > 0 ? 0 : -1;
            }

            private bool ContainsCareer(object obj)
            {
                CampaignInfo campaignInfo = SelectedCampaign;
                int armyIndex = SelectedArmyIndex;
                int airForceIndex = SelectedAirForceIndex;
                Career career = obj as Career;
                return ((campaignInfo == null || career.CampaignInfo == campaignInfo) &&
                        (armyIndex == -1 || career.ArmyIndex == armyIndex) &&
                        (airForceIndex == -1 || career.AirForceIndex == airForceIndex));
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

            private void UpdateButtonStatus()
            {
                Career careea = SelectedCareer;
                FrameworkElement.Continue.IsEnabled = careea != null && careea.CampaignInfo != null && careea.Date < careea.CampaignInfo.EndDate;
                FrameworkElement.Delete.IsEnabled = careea != null;
            }
        }
    }
}