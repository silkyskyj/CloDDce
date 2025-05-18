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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using IL2DCE.Pages.Controls;
using maddox.game;
using maddox.GP;
using static IL2DCE.MissionObjectModel.Spawn;

namespace IL2DCE.Pages
{
    public class CareerIntroPage : MissionPage
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

        protected CampaignInfo SelectedCampaign
        {
            get
            {
                return FrameworkElement.comboBoxSelectCampaign.SelectedItem as CampaignInfo;
            }
        }

        protected override string SelectedCampaignMission
        {
            get
            {
                CampaignInfo campaignInfo = SelectedCampaign;
                if (campaignInfo != null)
                {
                    return campaignInfo.InitialMissionTemplateFile;
                }

                return string.Empty;
            }
        }

        protected ECampaignMode SelectedCampaignMode
        {
            get
            {
                ComboBoxItem selected = FrameworkElement.comboBoxSelectCampaignMode.SelectedItem as ComboBoxItem;
                if (selected != null)
                {
                    return (ECampaignMode)selected.Tag;
                }

                return ECampaignMode.Default;
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

        public bool SelectedSpawnParked
        {
            get
            {
                bool? isCheckd = FrameworkElement.checkBoxSpawnParked.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        private string SelectedPilotName
        {
            get
            {
                return FrameworkElement.textBoxPilotName.Text;
            }
        }

        private bool IsErrorPilotName
        {
            get;
            set;
        }

        private Skill SelectedSkill
        {
            get
            {
                ComboBoxItem selected = FrameworkElement.comboBoxSelectSkill.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (Skill)selected.Tag;
                }

                return null;
            }
        }

        private ECampaignProgress SelectedProgress
        {
            get
            {
                ComboBoxItem selected = FrameworkElement.comboBoxSelectProgress.SelectedItem as ComboBoxItem;
                if (selected != null)
                {
                    return (ECampaignProgress)selected.Tag;
                }

                return ECampaignProgress.Daily;
            }
        }

        private bool IsProgressShortType
        {
            get
            {
                return SelectedProgress == ECampaignProgress.AnyTime || SelectedProgress == ECampaignProgress.AnyDayAnyTime;
            }
        }

        public bool SelectedStrictMode
        {
            get
            {
                bool? isCheckd = FrameworkElement.checkBoxStrictMode.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        #endregion

        #region Variable

        private bool hookComboSelectionChanged = false;

        #region StrictModeSaveSelectedInfo

        private string comboBoxSelectUnitNumsArmorText;
        private string comboBoxSelectUnitNumsShipText;
        private bool checkBoxSpawnRandomLocationEnemyChecked;
        private bool checkBoxSpawnRandomLocationFriendlyChecked;
        private bool checkBoxSpawnRandomLocationPlayerChecked;

        #endregion

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
            FrameworkElement.comboBoxSelectSkill.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectSkill_SelectionChanged);
            FrameworkElement.datePickerStart.SelectedDateChanged += new System.EventHandler<SelectionChangedEventArgs>(datePickerStart_SelectedDateChanged);
            FrameworkElement.datePickerEnd.SelectedDateChanged += new System.EventHandler<SelectionChangedEventArgs>(datePickerEnd_SelectedDateChanged);
            FrameworkElement.comboBoxSelectProgress.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectProgress_SelectionChanged);
            FrameworkElement.GeneralSettingsGroupBox.ComboBoxSelectionChangedEvent += new SelectionChangedEventHandler(GeneralSettingsGroupBox_ComboBoxSelectionChangedEvent);
            FrameworkElement.GeneralSettingsGroupBox.ComboBoxTextChangedEvent += new TextChangedEventHandler(GeneralSettingsGroupBox_ComboBoxTextChangedEvent);
            FrameworkElement.checkBoxStrictMode.Checked += new RoutedEventHandler(checkBoxStrictMode_CheckedChange);
            FrameworkElement.checkBoxStrictMode.Unchecked += new RoutedEventHandler(checkBoxStrictMode_CheckedChange);

            FrameworkElement.buttonImportMission.Click += new RoutedEventHandler(ImportMission);
            FrameworkElement.buttonImportMissionFile.Click += new RoutedEventHandler(ImportMissionFile);
            FrameworkElement.buttonImportMissionFolder.Click += new RoutedEventHandler(ImportMissionFolder);
            FrameworkElement.buttonImportCampaignFile.Click += new RoutedEventHandler(ImportCampaignFile);
            FrameworkElement.buttonImportCampaignFolder.Click += new RoutedEventHandler(ImportCampaignFolder);
            FrameworkElement.buttonMissionLoad.Click += new RoutedEventHandler(MissionLoad);
        }

        public override void _enter(maddox.game.IGame play, object arg)
        {
            base._enter(play, arg);

            // FrameworkElement.comboBoxSelectArmy.Items.Clear();

            FrameworkElement.GeneralSettingsGroupBox.SetRelationInfo(play.gameInterface, Game.Core.Config);

            PageArgs pageArgs = arg as PageArgs;

            hookComboSelectionChanged = true;
            UpdateArmyComboBoxInfo(pageArgs != null ? pageArgs.Army: -1);
            hookComboSelectionChanged = false;
            UpdateAirForceComboBoxInfo(pageArgs != null ? pageArgs.AirForce : -1);
            UpdateCampaignComboBoxInfo();
            UpdateCampaignComboBoxFilter();
            UpdateProgressComboBoxInfo();

#if false
            DifficultySetting d = Game.gpDifficultyGet();
            FrameworkElement.checkBoxStrictMode.IsChecked = 
                FrameworkElement.checkBoxStrictMode.IsEnabled = d.Vulnerability && d.Realistic_Landings && d.Realistic_Gunnery && d.Realistic_Bombing && d.Limited_Ammo && 
                                                            d.Limited_Fuel && d.NoAutopilot && d.Takeoff_N_Landing && d.Cockpit_Always_On && d.Head_Shake && d.Blackouts_N_Redouts && 
                                                            d.No_Padlock &&  d.ComplexEManagement && d.Engine_Temperature_Effects && d.Wind_N_Turbulence && d.Flutter_Effect && 
                                                            d.Torque_N_Gyro_Effects && d.Stalls_N_Spins && d.Clouds && d.No_Icons;
#endif
        }

        public override void _leave(maddox.game.IGame play, object arg)
        {
            base._leave(play, arg);
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
            UpdateAirGroupComboBoxInfo();
            UpdateButtonStatus();
        }

        private void comboBoxSelectCampaign_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
            {
                CampaignInfo campaignInfo = SelectedCampaign;
                if (campaignInfo != null)
                {
                    string missionTemplateFileName = campaignInfo.InitialMissionTemplateFile;
                    ISectionFile missionTemplateSectionFile = Game.gpLoadSectionFile(missionTemplateFileName);
                    currentMissionFile = new MissionFile(missionTemplateSectionFile, campaignInfo.AirGroupInfos, MissionFile.LoadLevel.AirGroup);
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
            missionLoaded = false;

            UpdateMapNameInfo();
            UpdateCampaignModeComboBoxInfo();
            UpdateDataPicker();
            UpdateAirGroupComboBoxInfo();
            UpdateButtonStatus();
        }

        private void comboBoxSelectAirGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSpawnParkedInfo();
            UpdateSkillComboBoxInfo();
            GeneralSettingsGroupBox.UpdateSkillComboBoxSkillValueInfo(FrameworkElement.comboBoxSelectSkill, SelectedSkill, SelectedAirGroup, Game.Core.Config.Skills);
            UpdateAircraftImage();
            UpdateButtonStatus();
        }


        private void comboBoxSelectSkill_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // if (e.AddedItems.Count > 0)
            {
                GeneralSettingsGroupBox.UpdateSkillComboBoxSkillValueInfo(FrameworkElement.comboBoxSelectSkill, SelectedSkill, SelectedAirGroup, Game.Core.Config.Skills);
            }
        }

        private void GeneralSettingsGroupBox_ComboBoxSelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            if (Game != null)
            {
                UpdateButtonStatus();
            }
        }

        private void GeneralSettingsGroupBox_ComboBoxTextChangedEvent(object sender, TextChangedEventArgs e)
        {
            if (Game != null)
            {
                UpdateButtonStatus();
            }
        }

        private void comboBoxSelectProgress_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool disable = IsProgressShortType;
            FrameworkElement.GeneralSettingsGroupBox.checkBoxSpawnRandomLocationPlayer.IsEnabled = !disable;
            FrameworkElement.GeneralSettingsGroupBox.checkBoxSpawnRandomLocationPlayer.Visibility = disable || SelectedStrictMode ? Visibility.Hidden: Visibility.Visible;
            UpdateButtonStatus();
        }

        #endregion

        #region TextBox

        void textBoxPilotName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Game != null)
            {
                string pilotName = SelectedPilotName;
                ToolTip toolTip = FrameworkElement.textBoxPilotName.ToolTip as ToolTip;
                if (IsErrorPilotName = string.IsNullOrEmpty(pilotName) || Game.Core.AvailableCareers.Any(x => string.Compare(x.PilotName, pilotName) == 0))
                {
                    FrameworkElement.textBoxPilotName.BorderBrush = Brushes.Red;
                    if (toolTip == null)
                    {
                        toolTip = new ToolTip();
                    }
                    FrameworkElement.textBoxPilotName.ToolTip = toolTip;
                    toolTip.Foreground = Brushes.Red;
                    toolTip.Content = "This pilot name is already in use.";
                }
                else
                {
                    FrameworkElement.textBoxPilotName.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 171, 173, 179));
                    if (toolTip != null)
                    {
                        toolTip.Content = string.Empty;
                    }
                }
            }
            UpdateButtonStatus();
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

        #region CheckBox

        private void checkBoxStrictMode_CheckedChange(object sender, RoutedEventArgs e)
        {
            GeneralSettingsGroupBox general = FrameworkElement.GeneralSettingsGroupBox;
            bool strictMode = SelectedStrictMode;
            if (strictMode)
            {
                general.labelSelectUnitNumsArmor.Visibility = Visibility.Hidden;
                comboBoxSelectUnitNumsArmorText = general.comboBoxSelectUnitNumsArmor.Text;
                general.comboBoxSelectUnitNumsArmor.Text = EArmorUnitNumsSet.Default.ToDescription();
                general.comboBoxSelectUnitNumsArmor.IsEnabled = false;
                general.comboBoxSelectUnitNumsArmor.Visibility = Visibility.Hidden;

                general.labelSelectUnitNumsShip.Visibility = Visibility.Hidden;
                comboBoxSelectUnitNumsShipText = general.comboBoxSelectUnitNumsShip.Text;
                general.comboBoxSelectUnitNumsShip.Text = EArmorUnitNumsSet.Default.ToDescription();
                general.comboBoxSelectUnitNumsShip.IsEnabled = false;
                general.comboBoxSelectUnitNumsShip.Visibility = Visibility.Hidden;

                general.labelSelectSpawnRandomLocation.Visibility = Visibility.Hidden;
                checkBoxSpawnRandomLocationEnemyChecked = general.checkBoxSpawnRandomLocationEnemy.IsChecked != null && general.checkBoxSpawnRandomLocationEnemy.IsChecked.Value;
                general.checkBoxSpawnRandomLocationEnemy.IsChecked = false;
                general.checkBoxSpawnRandomLocationEnemy.IsEnabled = false;
                general.checkBoxSpawnRandomLocationEnemy.Visibility = Visibility.Hidden;

                checkBoxSpawnRandomLocationFriendlyChecked = general.checkBoxSpawnRandomLocationFriendly.IsChecked != null && general.checkBoxSpawnRandomLocationFriendly.IsChecked.Value;
                general.checkBoxSpawnRandomLocationFriendly.IsChecked = false;
                general.checkBoxSpawnRandomLocationFriendly.IsEnabled = false;
                general.checkBoxSpawnRandomLocationFriendly.Visibility = Visibility.Hidden;

                checkBoxSpawnRandomLocationPlayerChecked = general.checkBoxSpawnRandomLocationPlayer.IsChecked != null && general.checkBoxSpawnRandomLocationPlayer.IsChecked.Value;
                general.checkBoxSpawnRandomLocationPlayer.IsChecked = false;
                general.checkBoxSpawnRandomLocationPlayer.IsEnabled = false;
                general.checkBoxSpawnRandomLocationPlayer.Visibility = Visibility.Hidden;
            }
            else
            {
                general.labelSelectUnitNumsArmor.Visibility = Visibility.Visible;
                general.comboBoxSelectUnitNumsArmor.IsEnabled = true;
                general.comboBoxSelectUnitNumsArmor.Visibility = Visibility.Visible;
                general.comboBoxSelectUnitNumsArmor.Text = comboBoxSelectUnitNumsArmorText;

                general.labelSelectUnitNumsShip.Visibility = Visibility.Visible;
                general.comboBoxSelectUnitNumsShip.IsEnabled = true;
                general.comboBoxSelectUnitNumsShip.Visibility = Visibility.Visible;
                general.comboBoxSelectUnitNumsShip.Text = comboBoxSelectUnitNumsShipText;

                general.labelSelectSpawnRandomLocation.Visibility = Visibility.Visible;
                general.checkBoxSpawnRandomLocationEnemy.IsEnabled = true;
                general.checkBoxSpawnRandomLocationEnemy.Visibility = Visibility.Visible;
                general.checkBoxSpawnRandomLocationEnemy.IsChecked = checkBoxSpawnRandomLocationEnemyChecked;

                general.checkBoxSpawnRandomLocationFriendly.IsEnabled = true;
                general.checkBoxSpawnRandomLocationFriendly.Visibility = Visibility.Visible;
                general.checkBoxSpawnRandomLocationFriendly.IsChecked = checkBoxSpawnRandomLocationFriendlyChecked;

                bool disable = IsProgressShortType;
                general.checkBoxSpawnRandomLocationPlayer.IsEnabled = disable;
                general.checkBoxSpawnRandomLocationPlayer.Visibility = disable ? Visibility.Hidden: Visibility.Visible;
                general.checkBoxSpawnRandomLocationPlayer.IsChecked = disable ? false: checkBoxSpawnRandomLocationPlayerChecked;
            }
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
            Config config = Game.Core.Config;
            string pilotName = SelectedPilotName;
            int armyIndex = SelectedArmyIndex;
            int airForceIndex = SelectedAirForceIndex;
            int rankIndex = SelectedRank;
            CampaignInfo campaign = SelectedCampaign;
            AirGroup airGroup = SelectedAirGroup;
            GeneralSettingsGroupBox generalSettings = FrameworkElement.GeneralSettingsGroupBox;

            Career career = null;
            try
            {
                career = new Career(pilotName, armyIndex, airForceIndex, rankIndex);
                career.BattleType = EBattleType.Campaign;
                career.CampaignInfo = campaign;
                career.CampaignMode = SelectedCampaignMode;
                career.MissionIndex = 0;
                career.AirGroup = airGroup.ToString();
                career.AirGroupDisplay = airGroup.VirtualAirGroupKey;
                career.PlayerAirGroup = airGroup;
                career.Aircraft = AircraftInfo.CreateDisplayName(airGroup.Class);
                career.Spawn = (int)(FrameworkElement.checkBoxSpawnParked.IsEnabled ? SelectedSpawnParked ? ESpawn.Parked : ESpawn.Idle: ESpawn.Default);
                career.PlayerAirGroupSkill = SelectedSkill != null && SelectedSkill != Skill.Random ? SelectedSkill != Skill.Default ? new Skill[] { SelectedSkill }: career.UpdatePlayerAirGroupSkill() : null;
                campaign.StartDate = FrameworkElement.datePickerStart.SelectedDate.Value;
                campaign.EndDate = FrameworkElement.datePickerEnd.SelectedDate.Value;
                career.CampaignProgress = SelectedProgress;
                career.AdditionalAirOperations = generalSettings.SelectedAdditionalAirOperations;
                career.AdditionalGroundOperations = generalSettings.SelectedAdditionalGroundOperations;
                career.AdditionalAirGroups = generalSettings.SelectedAdditionalAirGroups;
                career.AdditionalGroundGroups = generalSettings.SelectedAdditionalGroundGroups;
                career.AdditionalStationaries = generalSettings.SelectedAdditionalStasionaries;
                career.SpawnDynamicAirGroups = generalSettings.SelectedKeepTotalAirGroups;
                career.SpawnDynamicGroundGroups = generalSettings.SelectedKeepTotalGroundGroups;
                career.SpawnDynamicStationaries = generalSettings.SelectedKeepTotalStationaries;
                career.ArmorUnitNumsSet = generalSettings.SelectedUnitNumsArmor;
                career.ShipUnitNumsSet = generalSettings.SelectedUnitNumsShip;
                career.GroundGroupGenerateType = generalSettings.SelectedGroundGroupGeneric ? EGroundGroupGenerateType.Generic : EGroundGroupGenerateType.Default;
                career.StationaryGenerateType = generalSettings.SelectedStationaryGeneric ? EStationaryGenerateType.Generic : EStationaryGenerateType.Default;
                career.SpawnRandomLocationFriendly = generalSettings.SelectedSpawnRandomLocationFriendly;
                career.SpawnRandomLocationEnemy = generalSettings.SelectedSpawnRandomLocationEnemy;
                career.SpawnRandomLocationPlayer = generalSettings.checkBoxSpawnRandomLocationPlayer.Visibility == Visibility.Visible ? generalSettings.SelectedSpawnRandomLocationPlayer: false;
                career.SpawnRandomAltitudeFriendly = generalSettings.SelectedSpawnRandomAltitudeFriendly;
                career.SpawnRandomAltitudeEnemy = generalSettings.SelectedSpawnRandomAltitudeEnemy;
                career.SpawnRandomTimeFriendly = generalSettings.SelectedSpawnRandomTimeFriendly;
                career.SpawnRandomTimeEnemy = generalSettings.SelectedSpawnRandomTimeEnemy;
                career.SpawnRandomTimeBeginSec = generalSettings.SelectedRandomTimeBegin;
                career.SpawnRandomTimeEndSec = generalSettings.SelectedRandomTimeEnd;
                career.AISkill = generalSettings.SelecteAISkill;
                career.ArtilleryTimeout = generalSettings.SelectedArtilleryTimeout;
                career.ArtilleryRHide = generalSettings.SelectedArtilleryRHide;
                career.ArtilleryZOffset = generalSettings.SelectedArtilleryZOffset;
                career.ShipSleep = generalSettings.SelectedShipSleep;
                career.ShipSkill = generalSettings.SelectedShipSkill;
                career.ShipSlowfire = generalSettings.SelectedShipSlowfire;
                career.ReArmTime = generalSettings.SelectedAutoReArm ? config.ProcessTimeReArm : -1;
                career.ReFuelTime = generalSettings.SelectedAutoReFuel ? config.ProcessTimeReFuel : -1;
                career.TrackRecording = generalSettings.SelectedTrackRecoding;
                career.RandomTimeBegin = generalSettings.SelectedBattleTimeBegin;
                career.RandomTimeEnd = generalSettings.SelectedBattleTimeEnd;

                career.StrictMode = SelectedStrictMode;

                Career added = Game.Core.AvailableCareers.Where(x => string.Compare(x.PilotName, pilotName, true) == 0).FirstOrDefault();
                if (added != null)
                {
                    Game.Core.AvailableCareers.Remove(career);
                }
                Game.Core.AvailableCareers.Add(career);
                Game.Core.CurrentCareer = career;

                Game.Core.ResetCampaign(Game);

                Game.gameInterface.PageChange(new BattleIntroPage(), null);
            }
            catch (Exception ex)
            {
                string message = string.Format("{0} - {1} {2} {3} {4}", "CareerIntroPage.Start_Click", ex.Message, campaign.Name, airGroup.DisplayDetailName, ex.StackTrace);
                Core.WriteLog(message);
                MessageBox.Show(string.Format("{0}", ex.Message), Config.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                if (career != null)
                {
                    Game.Core.AvailableCareers.Remove(career);
                }
            }
        }

        #endregion

        #endregion

        #region Import and Convert Mission File

        protected override void ImportMissionProgressWindow(ProgressWindowModel model, bool isCampaign)
        {
            base.ImportMissionProgressWindow(model, isCampaign);
            Game.gameInterface.PageChange(new CareerIntroPage(), null);
        }

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
                string missionTemplateFileName = campaignInfo.InitialMissionTemplateFile;
                ISectionFile missionTemplateSectionFile = Game.gpLoadSectionFile(missionTemplateFileName);
                MissionFile missionFile = new MissionFile(missionTemplateSectionFile, campaignInfo.AirGroupInfos, MissionFile.LoadLevel.AirGroup);
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

        private void UpdateMapNameInfo()
        {
            string mapName;
            if (currentMissionFile != null && !string.IsNullOrEmpty(currentMissionFile.Map))
            {
                int idx = currentMissionFile.Map.IndexOf("$");
                mapName = currentMissionFile.Map.Substring(idx != -1 ? idx + 1 : 0);
            }
            else
            {
                mapName = string.Empty;
            }
            FrameworkElement.labelMapName.Content = string.Format(MapFormat, mapName);
        }

        private void UpdateCampaignModeComboBoxInfo()
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectCampaignMode;
            comboBox.Items.Clear();

            CampaignInfo campaignInfo = SelectedCampaign;
            if (campaignInfo == null)
            {
                comboBox.SelectedIndex = -1;
            }
            else
            {
                if (campaignInfo.InitialMissionTemplateFileCount == 1)
                {
                    comboBox.Items.Add(new ComboBoxItem()
                    {
                        Tag = ECampaignMode.Default,
                        Content = ECampaignMode.Default.ToDescription(),
                    });
                }
                if (campaignInfo.InitialMissionTemplateFileCount > 1)
                {
                    for (ECampaignMode mode = ECampaignMode.Default + 1; mode < ECampaignMode.Count; mode++)
                    {
                        comboBox.Items.Add(
                            new ComboBoxItem()
                            {
                                Tag = mode,
                                Content = mode.ToDescription(),
                            });
                    }
                }

                comboBox.Text = campaignInfo.CampaignMode.ToDescription();
            }
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
                            comboBox.Items.Add(new ComboBoxItem()
                            {
                                Tag = airGroup,
                                Content = missionLoaded ? CreateAirGroupContent(airGroup, campaignInfo) : CreateAirGroupContent(airGroup, campaignInfo, string.Empty, aircraftInfo)
                            });
                        }
                    }
                }
            }

            comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
        }

        protected override void UpdateAirGroupContent()
        {
            CampaignInfo campaignInfo = SelectedCampaign;
            foreach (ComboBoxItem item in FrameworkElement.comboBoxSelectAirGroup.Items)
            {
                AirGroup airGroup = item.Tag as AirGroup;
                item.Content = CreateAirGroupContent(airGroup, campaignInfo);
            }
        }

        private string CreateAirGroupContent(AirGroup airGroup, CampaignInfo campaignInfo)
        {
            string content = string.Empty;
            AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
            Point3d pos = airGroup.Position;
            double distance = Game.gpFrontDistance(airGroup.ArmyIndex, pos.x, pos.y);
            if (distance == 0)
            {
                distance = Game.gpFrontDistance(airGroup.ArmyIndex == (int)EArmy.Red ? (int)EArmy.Blue : (int)EArmy.Red, pos.x, pos.y);
            }
            if (airGroup.Airstart)
            {
                content = CreateAirGroupContent(airGroup, campaignInfo, string.Empty, aircraftInfo, distance);
            }
            else
            {
                string airportName = string.Empty;
                var airports = Game.gpAirports();
                var airport = airports.Where(x => x.Pos().distance(ref pos) <= x.FieldR());
                if (!airport.Any())
                {
                    airport = airports.Where(x => x.Pos().distance(ref pos) <= x.FieldR() * 1.5);
                }
                if (airport.Any())
                {
                    airportName = airport.First().Name();
                }
                content = CreateAirGroupContent(airGroup, campaignInfo, airportName, aircraftInfo, distance);
            }

            return content;
        }

        private string CreateAirGroupContent(AirGroup airGroup, CampaignInfo campaignInfo, string airportName, AircraftInfo aircraftInfo = null, double distance = -1)
        {
            if (aircraftInfo == null)
            {
                aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
            }
            return string.Format(Config.NumberFormat, "{0} ({1}){2}{3}",
                    airGroup.DisplayName, aircraftInfo.DisplayName, airGroup.Airstart ? " [AIRSTART]" : string.IsNullOrEmpty(airportName) ? string.Empty : string.Format(" [{0}]", airportName),
                    distance >= 0 ? string.Format(Config.NumberFormat, " {0:F2}km", distance / 1000) : string.Empty);
        }


        private void UpdateSpawnParkedInfo()
        {
            AirGroup airGroup = SelectedAirGroup;
            if (airGroup != null && !airGroup.Airstart)
            {
                FrameworkElement.checkBoxSpawnParked.IsChecked = airGroup.SetOnParked;
                FrameworkElement.checkBoxSpawnParked.Visibility = Visibility.Visible;
                FrameworkElement.checkBoxSpawnParked.IsEnabled = true;
            }
            else
            {
                FrameworkElement.checkBoxSpawnParked.IsChecked = false;
                FrameworkElement.checkBoxSpawnParked.Visibility = Visibility.Hidden;
                FrameworkElement.checkBoxSpawnParked.IsEnabled = false;
            }
        }

        private void UpdateSkillComboBoxInfo()
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectSkill;
            string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;

            if (comboBox.Items.Count == 0)
            {
                comboBox.Items.Add(new ComboBoxItem() { Tag = Skill.Default, Content = Skill.Default.Name });
                comboBox.Items.Add(new ComboBoxItem() { Tag = Skill.Random, Content = Skill.Random.Name });
                Config config = Game.Core.Config;
                config.Skills.ForEach(x => comboBox.Items.Add(new ComboBoxItem() { Tag = x, Content = x.Name }));
            }

            string defaultString = string.Empty;
            if (SelectedAirGroup != null)
            {
                AirGroup airGroup = SelectedAirGroup;
                Skill skill = string.IsNullOrEmpty(airGroup.Skill) ? null : Skill.Parse(airGroup.Skill);
                defaultString = string.Format(MissionDefaultFormat, airGroup.Skills != null && airGroup.Skills.Count > 0 ?
                                Skill.SkillNameMulti : skill != null ? skill.IsTyped() ? skill.GetTypedName() : Skill.SkillNameCustom : string.Empty);
            }
            FrameworkElement.labelDefaultSkill.Content = defaultString;

            EnableSelectItem(comboBox, selected, SelectedAirGroup == null);
        }

        private void UpdateDataPicker()
        {
            CampaignInfo campaignInfo = SelectedCampaign;
            if (campaignInfo != null)
            {
                FrameworkElement.datePickerStart.SelectedDate = campaignInfo.StartDate;
                FrameworkElement.datePickerEnd.SelectedDate = campaignInfo.EndDate;
                FrameworkElement.labelDefaultSelectDate.Content = string.Format(Config.DateTimeFormat, MissionDefaultDateFormat, campaignInfo.StartDate, campaignInfo.EndDate);
            }
        }

        private void UpdateProgressComboBoxInfo()
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectProgress;
            for (ECampaignProgress i = ECampaignProgress.Daily; i < ECampaignProgress.Count; i++)
            {
                comboBox.Items.Add(
                    new ComboBoxItem()
                    {
                        Tag = i,
                        Content = i.ToDescription(),
                    });
            }
            comboBox.Text = ECampaignProgress.AnyDayAnyTime.ToDescription();
        }

        private void UpdateAircraftImage()
        {
            AirGroup airGroup = SelectedAirGroup;
            FrameworkElement.borderImage.DisplayImage(Game.gameInterface, airGroup != null ? airGroup.Class : string.Empty);
        }

        private void EnableSelectItem(ComboBox comboBox, string selected, bool forceDisable = false)
        {
            if (!forceDisable && comboBox.Items.Count > 0)
            {
                comboBox.IsEnabled = true;
                comboBox.Text = selected;
                if (!comboBox.IsEditable && comboBox.SelectedIndex == -1)
                {
                    comboBox.SelectedIndex = 0;
                }
            }
            else
            {
                comboBox.IsEnabled = false;
                comboBox.SelectedIndex = -1;
            }
        }

        private void UpdateButtonStatus()
        {
            GeneralSettingsGroupBox generalSettings = FrameworkElement.GeneralSettingsGroupBox;
            // string pilotName = FrameworkElement.textBoxPilotName.Text;
            DatePicker datePickerStart = FrameworkElement.datePickerStart;
            DatePicker datePickerEnd = FrameworkElement.datePickerEnd;
            int addGroundOpe = generalSettings.SelectedAdditionalGroundOperations;
            int timeBegin = generalSettings.SelectedRandomTimeBegin;
            int timeEnd = generalSettings.SelectedRandomTimeEnd;
            bool timeEnable = generalSettings.SelectedSpawnRandomTimeEnemy || generalSettings.SelectedSpawnRandomTimeFriendly;
            double timeBattleBegin = generalSettings.SelectedBattleTimeBegin;
            double timeBattleEnd = generalSettings.SelectedBattleTimeEnd;
            FrameworkElement.Start.IsEnabled = SelectedArmyIndex != -1 && SelectedAirForceIndex != -1 && SelectedRank != -1 && 
                                                !IsErrorPilotName/*!Game.Core.AvailableCareers.Any(x => string.Compare(x.PilotName, pilotName) == 0)*/ &&
                                                SelectedCampaign != null && SelectedAirGroup != null &&
                                                datePickerStart.SelectedDate.HasValue && datePickerEnd.SelectedDate.HasValue &&
                                                datePickerStart.SelectedDate.Value <= datePickerEnd.SelectedDate.Value &&
                                                (datePickerEnd.SelectedDate.Value - datePickerStart.SelectedDate.Value).TotalDays <= CampaignInfo.MaxCampaignPeriod &&
                                                addGroundOpe >= Config.MinAdditionalGroundOperations && addGroundOpe <= Config.MaxAdditionalGroundOperations &&
                                                (!timeEnable || timeEnable && timeBegin >= SpawnTime.MinimumBeginSec && timeEnd <= SpawnTime.MaximumEndSec && timeBegin <= timeEnd) &&
                                                timeBattleBegin != -1 && timeBattleEnd != -1 && timeBattleBegin <= timeBattleEnd;
        }
    }
}