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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using maddox.game.play;
using maddox.game.world;

namespace IL2DCE.Pages
{
    public class CareerIntroPage : PageDefImpl
    {
        #region Definition

        public class PageArgs
        {
            public int Army
            {
                get;
                set;
            }

            public int AirForce
            {
                get;
                set;
            }
        }

        class AircraftSummary
        {
            public char Army
            {
                get;
                set;
            }

            public char AirForce
            {
                get;
                set;
            }

            public char Flyable
            {
                get;
                set;
            }
        }

        #endregion

        #region Property

        private CareerIntro FrameworkElement
        {
            get
            {
                return FE as CareerIntro;
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

        private int SelectedRank
        {
            get
            {
                ComboBoxItem selected = FrameworkElement.comboBoxSelectRank.SelectedItem as ComboBoxItem;
                if (selected != null)
                {
                    return (int)selected.Tag;
                }

                return -1;
            }
        }

        private CampaignInfo SelectedCampaign
        {
            get
            {
                return FrameworkElement.comboBoxSelectCampaign.SelectedItem as CampaignInfo;
            }
        }

        private AirGroup SelectedAirGroup
        {
            get
            {
                ComboBoxItem selected = FrameworkElement.comboBoxSelectAirGroup.SelectedItem as ComboBoxItem;
                if (selected != null)
                {
                    return (AirGroup)selected.Tag;
                }

                return null;
            }
        }

        private int SelectedAdditionalAirOperationsComboBox
        {
            get
            {
                int? selected = FrameworkElement.comboBoxSelectAdditionalAirOperations.SelectedItem as int?;
                if (selected != null)
                {
                    return selected.Value;
                }

                return -1;
            }
        }

        private int SelectedAdditionalGroundOperationsComboBox
        {
            get
            {
                int? selected = FrameworkElement.comboBoxSelectAdditionalGroundOperations.SelectedItem as int?;
                if (selected != null)
                {
                    return selected.Value;
                }
                else
                {
                    string text = FrameworkElement.comboBoxSelectAdditionalGroundOperations.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        int num;
                        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out num))
                        {
                            return num;
                        }
                    }
                }
                return -1;
            }
        }

        #endregion

        #region Variable

        private bool hookComboSelectionChanged = false;
        private MissionFile currentMissionFile = null;

        #endregion

        public CareerIntroPage()
            : base("Career Intro", new CareerIntro())
        {
            FrameworkElement.Start.Click += new RoutedEventHandler(Start_Click);
            FrameworkElement.Back.Click += new RoutedEventHandler(Back_Click);
            FrameworkElement.comboBoxSelectArmy.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectArmy_SelectionChanged);
            FrameworkElement.comboBoxSelectAirForce.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirForce_SelectionChanged);
            FrameworkElement.textBoxPilotName.TextChanged += new TextChangedEventHandler(textBoxPilotName_TextChanged);
            FrameworkElement.comboBoxSelectCampaign.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectCampaign_SelectionChanged);
            FrameworkElement.comboBoxSelectAirGroup.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirGroup_SelectionChanged);
            FrameworkElement.comboBoxSelectAdditionalAirOperations.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAdditionalAirOperations_SelectionChanged);
            FrameworkElement.comboBoxSelectAdditionalGroundOperations.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAdditionalGroundOperations_SelectionChanged);

            FrameworkElement.comboBoxSelectAirGroup.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirGroup_SelectionChanged);
            FrameworkElement.datePickerStart.SelectedDateChanged += new System.EventHandler<SelectionChangedEventArgs>(datePickerStart_SelectedDateChanged);
            FrameworkElement.datePickerEnd.SelectedDateChanged += new System.EventHandler<SelectionChangedEventArgs>(datePickerEnd_SelectedDateChanged);

            FrameworkElement.comboBoxSelectAdditionalGroundOperations.Loaded += new RoutedEventHandler(comboBoxSelectAdditionalGroundOperations_Loaded);
            
            FrameworkElement.labelVersion.Content = Config.CreateVersionString(Assembly.GetExecutingAssembly().GetName().Version);
        }

        public override void _enter(maddox.game.IGame play, object arg)
        {
            base._enter(play, arg);

            // FrameworkElement.comboBoxSelectArmy.Items.Clear();

            _game = play as IGame;
            PageArgs pageArgs = arg as PageArgs;

            hookComboSelectionChanged = true;
            UpdateArmyComboBoxInfo(pageArgs != null ? pageArgs.Army: -1);
            hookComboSelectionChanged = false;
            UpdateAirForceComboBoxInfo(pageArgs != null ? pageArgs.AirForce : -1);
            UpdateCampaignComboBoxInfo();
            UpdateCampaignComboBoxFilter();
            UpdateSelectedAdditionalAirOperationsComboBox();
            UpdateSelectedAdditionalGroundOperationsComboBox();
        }

        public override void _leave(maddox.game.IGame play, object arg)
        {
            base._leave(play, arg);

            _game = null;
        }

        #region EventHandler

        #region ComboBox SelectionChanged

        void comboBoxSelectArmy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
            {
                UpdateAirForceComboBoxInfo();
            }

            UpdateButtonStatus();
        }

        void comboBoxSelectAirForce_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
            {
                UpdateRankComboBoxInfo();
            }

            UpdateTextBoxPilotName();
            UpdateCampaignComboBoxFilter();
            UpdateButtonStatus();
        }

        private void comboBoxSelectCampaign_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
            {
                CampaignInfo campaignInfo = SelectedCampaign;
                if (campaignInfo != null)
                {
                    currentMissionFile = new MissionFile(Game, campaignInfo.InitialMissionTemplateFiles, campaignInfo.AirGroupInfos);
                }
                else
                {
                    currentMissionFile = null;
                }
            }
            else
            {
                currentMissionFile = null;
            }

            UpdateDataPicker();
            UpdateAirGroupComboBoxInfo();
            UpdateButtonStatus();
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

            UpdateButtonStatus();
        }

        private void comboBoxSelectAdditionalAirOperations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStatus();
        }

        private void comboBoxSelectAdditionalGroundOperations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStatus();
        }

        private void comboBoxSelectAdditionalGroundOperations_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            FrameworkElement.comboBoxSelectAdditionalGroundOperations.TextBox.TextChanged += new TextChangedEventHandler(comboBoxSelectAdditionalGroundOperations_TextChanged);
        }

        #endregion

        #region TextBox

        void textBoxPilotName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Game != null)
            {
                UpdateButtonStatus();
            }
        }

        protected virtual void comboBoxSelectAdditionalGroundOperations_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Game != null)
            {
                UpdateButtonStatus();
            }
        }

        #endregion

        #region DatePicker

        private void datePickerStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStatus();
        }

        private void datePickerEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStatus();
        }

        #endregion

        #region Button Click

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Game.gameInterface.BattleIsRun())
            {
                Game.gameInterface.BattleStop();
            }

            Game.gameInterface.PageChange(new SelectCareerPage(), null);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            string pilotName = FrameworkElement.textBoxPilotName.Text;
            int armyIndex = SelectedArmyIndex;
            int airForceIndex = SelectedAirForceIndex;
            int rankIndex = SelectedRank;
            CampaignInfo campaign = SelectedCampaign;
            AirGroup airGroup = SelectedAirGroup;

            try
            {
                Career career = new Career(pilotName, armyIndex, airForceIndex, rankIndex);
                career.BattleType = EBattleType.Campaign;
                Game.Core.AvailableCareers.Add(career);
                Game.Core.CurrentCareer = career;
                career.CampaignInfo = campaign;
                career.AirGroup = airGroup.ToString();
                AircraftInfo aircraftInfo = career.CampaignInfo.GetAircraftInfo(airGroup.Class);
                career.Aircraft = aircraftInfo.DisplayName;
                campaign.StartDate = FrameworkElement.datePickerStart.SelectedDate.Value;
                campaign.EndDate = FrameworkElement.datePickerEnd.SelectedDate.Value;
                career.AdditionalAirOperations = SelectedAdditionalAirOperationsComboBox;
                career.AdditionalGroundOperations = SelectedAdditionalGroundOperationsComboBox;

                Game.Core.ResetCampaign(Game);

                Game.gameInterface.PageChange(new BattleIntroPage(), null);

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}", ex.Message), "IL2DCE", MessageBoxButton.OK, MessageBoxImage.Error);
                Game.gameInterface.LogErrorToConsole(string.Format("{0} - {1}", "CareerIntroPage.Start_Click", ex.Message));
            }
        }

        #endregion

        #endregion

        private void UpdateArmyComboBoxInfo(int armyIndex = -1)
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectArmy;
            for (EArmy army = EArmy.Red; army <= EArmy.Blue; army++)
            {
                comboBox.Items.Add(new ComboBoxItem() { Tag = (int)army, Content = army.ToString() });
            }
            if (armyIndex == -1)
            {
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }
            else
            {
                comboBox.SelectedIndex = armyIndex - 1;
            }
        }

        private void UpdateAirForceComboBoxInfo(int airForceIndex = -1)
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectAirForce;
            comboBox.Items.Clear();

            int armyIndex = SelectedArmyIndex;
            if (armyIndex != -1)
            {
                if (armyIndex == (int)EArmy.Red)
                {
                    foreach (var item in Enum.GetValues(typeof(EAirForceRed)))
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = (int)item, Content = ((EAirForceRed)item).ToDescription() });
                    }
                }
                else if (armyIndex == (int)EArmy.Blue)
                {
                    foreach (var item in Enum.GetValues(typeof(EAirForceBlue)))
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = (int)item, Content = ((EAirForceBlue)item).ToDescription() });
                    }
                }
            }
            if (airForceIndex == -1)
            {
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }
            else
            {
                comboBox.SelectedIndex = airForceIndex - 1;
            }
        }

        private void UpdateTextBoxPilotName()
        {
            int armyIndex = SelectedArmyIndex;
            int airForceIndex = SelectedAirForceIndex;
            if (armyIndex != -1 && airForceIndex != -1)
            {
                AirForce airForce = AirForces.Default.Where(x => x.ArmyIndex == armyIndex && x.AirForceIndex == airForceIndex).FirstOrDefault();
                FrameworkElement.textBoxPilotName.Text = airForce.PilotNameDefault;
            }
            else
            {
                FrameworkElement.textBoxPilotName.Text = string.Empty;
            }
        }

        private void UpdateRankComboBoxInfo()
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectRank;
            int selected = comboBox.SelectedIndex;
            comboBox.Items.Clear();

            int armyIndex = SelectedArmyIndex;
            int airForceIndex = SelectedAirForceIndex;

            if (armyIndex != -1 && airForceIndex != -1)
            {
                AirForce airForce = AirForces.Default.Where(x => x.ArmyIndex == armyIndex && x.AirForceIndex == airForceIndex).FirstOrDefault();
                for (int i = 0; i <= Rank.RankMax; i++)
                {
                    comboBox.Items.Add(
                        new ComboBoxItem()
                        {
                            Content = airForce.Ranks[i],
                            Tag = i,
                        });
                }
            }

            if (comboBox.Items.Count > 0)
            {
                comboBox.IsEnabled = true;
                comboBox.SelectedIndex = selected != -1 ? selected : 0;
            }
            else
            {
                comboBox.IsEnabled = false;
                comboBox.SelectedIndex = -1;
            }
        }

        private void UpdateCampaignComboBoxInfo(CampaignInfo campaignInfoSelect = null)
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectCampaign;
            comboBox.Items.Clear();

            foreach (CampaignInfo campaignInfo in Game.Core.CampaignInfos)
            {
                MissionFile missionFile = new MissionFile(Game, campaignInfo.InitialMissionTemplateFiles, campaignInfo.AirGroupInfos);
                campaignInfo.Data = missionFile.AirGroups.Select(x => new AircraftSummary()
                {
                    Army = (char)x.ArmyIndex,
                    AirForce = (char)x.AirGroupInfo.AirForceIndex,
                    Flyable = (char)(campaignInfo.GetAircraftInfo(x.Class).IsFlyable ? 1 : 0)
                }).ToArray();
                comboBox.Items.Add(campaignInfo);
            }

            if (campaignInfoSelect == null)
            {
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }
            else
            {
                comboBox.SelectedItem =campaignInfoSelect;
            }
        }

        private void UpdateCampaignComboBoxFilter()
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectCampaign;
            comboBox.Items.Filter = new Predicate<object>(ContainsCampaign);
            comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
        }

        private bool ContainsCampaign(object obj)
        {
            CampaignInfo campaignInfo = obj as CampaignInfo;
            AircraftSummary[] aircraftSummary = campaignInfo.Data as AircraftSummary[];
            return aircraftSummary != null && aircraftSummary.Any(x => x.Army == SelectedArmyIndex && x.AirForce == SelectedAirForceIndex && x.Flyable == 1);
        }

        private void UpdateAirGroupComboBoxInfo()
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectAirGroup;
            comboBox.Items.Clear();

            CampaignInfo campaignInfo = SelectedCampaign;
            if (campaignInfo != null && currentMissionFile != null)
            {
                int armyIndex = SelectedArmyIndex;
                int airForceIndex = SelectedAirForceIndex;
                if (armyIndex != -1 && airForceIndex != -1)
                {
                    foreach (AirGroup airGroup in currentMissionFile.AirGroups.OrderBy(x => x.Class))
                    {
                        AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                        AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                        if (airGroupInfo.ArmyIndex == armyIndex && airGroupInfo.AirForceIndex == airForceIndex && aircraftInfo.IsFlyable)
                        {
                            comboBox.Items.Add(new ComboBoxItem() { Tag = airGroup, Content = CreateAirGroupContent(airGroup, campaignInfo, aircraftInfo) });
                        }
                    }
                }
            }

            comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
        }

        private void UpdateSelectedAdditionalAirOperationsComboBox()
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectAdditionalAirOperations;
            foreach (var item in Enumerable.Range(Config.MinAdditionalAirOperations, Config.MaxAdditionalAirOperations))
            {
                comboBox.Items.Add(item);
            }
            comboBox.SelectedItem = Config.DefaultAdditionalAirOperations;
        }

        private void UpdateSelectedAdditionalGroundOperationsComboBox()
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectAdditionalGroundOperations;
            for (int i = Config.MinAdditionalGroundOperations; i <= Config.MaxAdditionalGroundOperations; i += 10)
            {
                comboBox.Items.Add(i);
            }
            comboBox.SelectedItem = Config.DefaultAdditionalGroundOperations;
        }

        private string CreateAirGroupContent(AirGroup airGroup, CampaignInfo campaignInfo, AircraftInfo aircraftInfo = null)
        {
            if (aircraftInfo == null)
            {
                aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
            }
            return string.Format("{0} ({1}){2}", airGroup.DisplayName, aircraftInfo.DisplayName, airGroup.Airstart ? " [AIRSTART]": string.Empty);
        }

        private void UpdateDataPicker()
        {
            CampaignInfo campaignInfo = SelectedCampaign;
            if (campaignInfo != null)
            {
                FrameworkElement.datePickerStart.SelectedDate = campaignInfo.StartDate;
                FrameworkElement.datePickerEnd.SelectedDate = campaignInfo.EndDate;
            }
        }

        private void DisplayAircraftImage(string aircraftClass)
        {
            //string path;
            //;
            //if (!string.IsNullOrEmpty(aircraftClass) &&
            //    !string.IsNullOrEmpty(path = new AircraftImage(Game.gameInterface.ToFileSystemPath(Config.PartsFolder)).GetImagePath(aircraftClass)))
            //{
            //    // using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            //    {
            //        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            //        var decoder = new TiffBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            //        BitmapSource source = decoder.Frames[0];
            //        FrameworkElement.imageAircraft.Source = source;
            //        FrameworkElement.borderImage.Visibility = Visibility.Visible;
            //    }
            //}
            //else
            //{
            //    FrameworkElement.imageAircraft.Source = null;
            //    FrameworkElement.borderImage.Visibility = Visibility.Hidden;
            //}
        }

        private void UpdateButtonStatus()
        {
            string pilotName = FrameworkElement.textBoxPilotName.Text;
            DatePicker datePickerStart = FrameworkElement.datePickerStart;
            DatePicker datePickerEnd = FrameworkElement.datePickerEnd;
            int addGroundOpe = SelectedAdditionalGroundOperationsComboBox;
            FrameworkElement.Start.IsEnabled = SelectedArmyIndex != -1 && SelectedAirForceIndex != -1 && SelectedRank != -1 && 
                                                !Game.Core.AvailableCareers.Any(x => string.Compare(x.PilotName, pilotName) == 0) &&
                                                SelectedCampaign != null && SelectedAirGroup != null &&
                                                datePickerStart.SelectedDate.HasValue && datePickerEnd.SelectedDate.HasValue &&
                                                datePickerStart.SelectedDate.Value <= datePickerEnd.SelectedDate.Value &&
                                                (datePickerEnd.SelectedDate.Value - datePickerStart.SelectedDate.Value).TotalDays <= CampaignInfo.MaxCampaignPeriod &&
                                                addGroundOpe >= Config.MinAdditionalGroundOperations && addGroundOpe <= Config.MaxAdditionalGroundOperations;
        }
    }
}