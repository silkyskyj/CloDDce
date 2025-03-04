// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
// Copyright (C) 2025 Stefan Rothdach & 2025 silkyskyj
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

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class CampaignIntroPage : PageDefImpl
        {
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

            public CampaignIntroPage()
                : base("Campaign Into", new CampaignIntro())
            {
                FrameworkElement.Start.Click += new RoutedEventHandler(Start_Click);
                FrameworkElement.Start.IsEnabled = false;
                FrameworkElement.Back.Click += new RoutedEventHandler(Back_Click);
                FrameworkElement.comboBoxSelectAirGroup.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirGroup_SelectionChanged);
                FrameworkElement.datePickerStart.SelectedDateChanged += new System.EventHandler<SelectionChangedEventArgs>(datePickerStart_SelectedDateChanged);
                FrameworkElement.datePickerEnd.SelectedDateChanged += new System.EventHandler<SelectionChangedEventArgs>(datePickerEnd_SelectedDateChanged);
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                _game = play as IGame;

                Career career = Game.Core.CurrentCareer;
                CampaignInfo campaignInfo = career.CampaignInfo;

                MissionFile campaignTemplate = new MissionFile(Game, campaignInfo.InitialMissionTemplateFiles, campaignInfo.AirGroupInfos);

                ComboBox comboBoxAirGroup = FrameworkElement.comboBoxSelectAirGroup;
                comboBoxAirGroup.Items.Clear();
                foreach (AirGroup airGroup in campaignTemplate.AirGroups)
                {
                    AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                    AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                    if (airGroupInfo.ArmyIndex == career.ArmyIndex && airGroupInfo.AirForceIndex == career.AirForceIndex && aircraftInfo.IsFlyable)
                    {
                        ComboBoxItem itemAirGroup = new ComboBoxItem();
                        itemAirGroup.Content = airGroup.DisplayName + " (" + aircraftInfo.DisplayName + ")";
                        if (airGroup.Airstart == true)
                        {
                            itemAirGroup.Content += " [AIRSTART]";
                        }

                        itemAirGroup.Tag = airGroup;
                        comboBoxAirGroup.Items.Add(itemAirGroup);
                    }
                }
                if (comboBoxAirGroup.Items.Count > 0)
                {
                    comboBoxAirGroup.SelectedIndex = 0;
                }

                DatePicker datePickerStart = FrameworkElement.datePickerStart;
                datePickerStart.SelectedDate = campaignInfo.StartDate;
                DatePicker datePickerEnd = FrameworkElement.datePickerEnd;
                datePickerEnd.SelectedDate = campaignInfo.EndDate;

                UpdateButtonStatus();
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            private void comboBoxSelectAirGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                ComboBoxItem itemAirGroup = FrameworkElement.comboBoxSelectAirGroup.SelectedItem as ComboBoxItem;
                if (itemAirGroup != null)
                {
                    AirGroup airGroup = itemAirGroup.Tag as AirGroup;
                    if (airGroup != null)
                    {
                        DisplayAircraftImage(airGroup.Class);
                    }
                }
            }

            private void DisplayAircraftImage(string aircraftClass)
            {
                string path;
                ;
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

            private void datePickerStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
            {
                UpdateButtonStatus();
            }

            private void datePickerEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
            {
                UpdateButtonStatus();
            }

            private void UpdateButtonStatus()
            {
                DatePicker datePickerStart = FrameworkElement.datePickerStart;
                DatePicker datePickerEnd = FrameworkElement.datePickerEnd;
                FrameworkElement.Start.IsEnabled = FrameworkElement.comboBoxSelectAirGroup.Items.Count > 0 &&
                                                    datePickerStart.SelectedDate.HasValue && datePickerEnd.SelectedDate.HasValue &&
                                                    datePickerStart.SelectedDate.Value <= datePickerEnd.SelectedDate.Value &&
                                                    (datePickerEnd.SelectedDate.Value - datePickerStart.SelectedDate.Value).TotalDays <= CampaignInfo.MaxCampaignPeriod;
            }

            private void Back_Click(object sender, RoutedEventArgs e)
            {
                if (Game.gameInterface.BattleIsRun())
                {
                    Game.gameInterface.BattleStop();
                }

                Game.gameInterface.PageChange(new SelectCampaignPage(), null);
            }

            private void Start_Click(object sender, RoutedEventArgs e)
            {
                ComboBoxItem item = FrameworkElement.comboBoxSelectAirGroup.SelectedItem as ComboBoxItem;
                AirGroup airGroup = (AirGroup)item.Tag;
                Career career = Game.Core.CurrentCareer;
                career.AirGroup = airGroup.ToString();
                CampaignInfo campaignInfo = career.CampaignInfo;
                AircraftInfo aircraftInfo = career.CampaignInfo.GetAircraftInfo(airGroup.Class);
                career.Aircraft = aircraftInfo.DisplayName;
                campaignInfo.StartDate = FrameworkElement.datePickerStart.SelectedDate.Value;
                campaignInfo.EndDate = FrameworkElement.datePickerEnd.SelectedDate.Value;

                Game.Core.ResetCampaign(Game);

                Game.gameInterface.PageChange(new BattleIntroPage(), null);
            }
        }
    }
}