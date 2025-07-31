// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2016 Stefan Rothdach & 2025 silkysky
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
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using IL2DCE.Pages.Controls;
using IL2DCE.Util;
using maddox.game;
using static IL2DCE.MissionObjectModel.Spawn;

namespace IL2DCE
{
    namespace Pages
    {
        public class QuickMissionPage : MissionPage
        {
            #region Definition

            #region Constant

            public const string SectionDQMSetting = "DQMSetting";
            public const string DQMSettingFileFilter = "DQM Setting File(.ini)|DQMSetting*.ini";

            private const string RandomString = "[Random]";
            private const string DefaultString = "Default";
            private const string MissionTypeDefaultFormat = "Mission Type Default: {0}";

            #endregion

            #endregion

            #region Property

            private QuickMission FrameworkElement
            {
                get
                {
                    return FE as QuickMission;
                }
            }

            protected override CampaignInfo SelectedCampaign
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
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectCampaignMission.SelectedItem as ComboBoxItem;
                    if (selected != null)
                    {
                        return selected.Tag as string;
                    }

                    return string.Empty;
                }
            }

            protected int SelectedCampaignMissionIndex
            {
                get
                {
                    return FrameworkElement.comboBoxSelectCampaignMission.SelectedIndex;
                }
            }

            protected override int SelectedArmyIndex
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

            protected override int SelectedAirForceIndex
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

            protected override AirGroup SelectedAirGroup
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

            private EMissionType? SelectedMissionType
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectMissionType.SelectedItem as ComboBoxItem;
                    if (selected != null && selected.Tag != null)
                    {
                        return (EMissionType)selected.Tag;
                    }

                    return null;
                }
            }

            private int SelectedFlight
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectFlight.SelectedItem as ComboBoxItem;
                    if (selected != null && selected.Tag != null)
                    {
                        return (int)selected.Tag;
                    }

                    return -1;
                }
            }

            private EFormation SelectedFormation
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectFormation.SelectedItem as ComboBoxItem;
                    if (selected != null && selected.Tag != null)
                    {
                        return (EFormation)selected.Tag;
                    }

                    return EFormation.Default;
                }
            }

            private int SelectedSpawn
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSpawn.SelectedItem as ComboBoxItem;
                    if (selected != null && selected.Tag != null)
                    {
                        return (int)selected.Tag;
                    }

                    return (int)ESpawn.Random;
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

            private int SelectedSpeed
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSpeed.SelectedItem as ComboBoxItem;
                    if (selected != null && selected.Tag != null)
                    {
                        return (int)selected.Tag;
                    }

                    return -1;
                }
            }

            private int SelectedFuel
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxFuel.SelectedItem as ComboBoxItem;
                    if (selected != null && selected.Tag != null)
                    {
                        return (int)selected.Tag;
                    }

                    return -1;
                }
            }

            private double SelectedTime
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectTime.SelectedItem as ComboBoxItem;
                    if (selected != null && selected.Tag != null)
                    {
                        return (double)selected.Tag;
                    }

                    return -1;
                }
            }

            private int SelectedWeather
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectWeather.SelectedItem as ComboBoxItem;
                    if (selected != null && selected.Tag != null)
                    {
                        return (int)selected.Tag;
                    }

                    return -1;
                }
            }

            private int SelectedCloudAltitude
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectCloudAltitude.SelectedItem as ComboBoxItem;
                    if (selected != null && selected.Tag != null)
                    {
                        return (int)selected.Tag;
                    }

                    return -1;
                }
            }

            #endregion 

            #region Variable

            private bool hookComboSelectionChanged = false;

            private int defaultMissionAltitude = 0;

            #endregion

            public QuickMissionPage()
                : base("Quick Mission", new QuickMission())
            {
                FrameworkElement.comboBoxSelectCampaign.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectCampaign_SelectionChanged);
                FrameworkElement.comboBoxSelectCampaignMission.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectCampaignMission_SelectionChanged);
                FrameworkElement.comboBoxSelectArmy.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectArmy_SelectionChanged);
                FrameworkElement.comboBoxSelectAirForce.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirForce_SelectionChanged);
                FrameworkElement.comboBoxSelectAirGroup.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirGroup_SelectionChanged);
                FrameworkElement.comboBoxSelectSkill.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectSkill_SelectionChanged);
                FrameworkElement.comboBoxSelectMissionType.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectMissionType_SelectionChanged);
                FrameworkElement.comboBoxSpawn.SelectionChanged += new SelectionChangedEventHandler(comboBoxSpawn_SelectionChanged);
                FrameworkElement.comboBoxSelectTime.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectTime_SelectionChanged);
                FrameworkElement.comboBoxSpawn.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSpawn_IsEnabledChanged);
                FrameworkElement.comboBoxSelectCampaign.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSelectCampaign_IsEnabledChanged);
                FrameworkElement.comboBoxSelectCampaignMission.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSelectCampaignMission_IsEnabledChanged);
                FrameworkElement.comboBoxSelectArmy.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSelectArmy_IsEnabledChanged);
                FrameworkElement.comboBoxSelectAirForce.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSelectAirForce_IsEnabledChanged);
                FrameworkElement.comboBoxSelectAirGroup.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSelectAirGroup_IsEnabledChanged);
                FrameworkElement.comboBoxSelectMissionType.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSelectMissionType_IsEnabledChanged);

                FrameworkElement.datePickerStart.SelectedDateChanged += new System.EventHandler<SelectionChangedEventArgs>(datePickerStart_SelectedDateChanged);

                FrameworkElement.GeneralSettingsGroupBox.ComboBoxSelectionChangedEvent += new SelectionChangedEventHandler(GeneralSettingsGroupBox_ComboBoxSelectionChangedEvent);
                FrameworkElement.GeneralSettingsGroupBox.ComboBoxTextChangedEvent += new TextChangedEventHandler(GeneralSettingsGroupBox_ComboBoxTextChangedEvent);

                FrameworkElement.checkBoxSelectCampaignFilter.Checked += new RoutedEventHandler(checkBoxSelectCampaignFilter_CheckedChange);
                FrameworkElement.checkBoxSelectCampaignFilter.Unchecked += new RoutedEventHandler(checkBoxSelectCampaignFilter_CheckedChange);
                FrameworkElement.checkBoxSelectAirgroupFilter.Checked += new RoutedEventHandler(checkBoxSelectAirgroupFilter_CheckedChange);
                FrameworkElement.checkBoxSelectAirgroupFilter.Unchecked += new RoutedEventHandler(checkBoxSelectAirgroupFilter_CheckedChange);

                FrameworkElement.Start.Click += new System.Windows.RoutedEventHandler(Start_Click);
                FrameworkElement.Back.Click += new System.Windows.RoutedEventHandler(Back_Click);
                FrameworkElement.buttonImportMission.Click += new RoutedEventHandler(ImportMission);
                FrameworkElement.buttonImportMissionFile.Click += new RoutedEventHandler(ImportMissionFile);
                FrameworkElement.buttonImportMissionFolder.Click += new RoutedEventHandler(ImportMissionFolder);
                FrameworkElement.buttonImportCampaignFile.Click += new RoutedEventHandler(ImportCampaignFile);
                FrameworkElement.buttonImportCampaignFolder.Click += new RoutedEventHandler(ImportCampaignFolder);
                FrameworkElement.buttonReload.Click += new RoutedEventHandler(buttonReload_Click);
                FrameworkElement.buttonMissionLoad.Click += new RoutedEventHandler(MissionLoad);
                FrameworkElement.buttonStats.Click += new RoutedEventHandler(buttonStats_Click);
                FrameworkElement.buttonUserLoad.Click += new RoutedEventHandler(buttonUserLoad_Click);
                FrameworkElement.buttonUserSave.Click += new RoutedEventHandler(buttonUserSave_Click);

                // FrameworkElement.buttonMissionLoad.Visibility = Visibility.Hidden;
                FrameworkElement.checkBoxSelectCampaignFilter.Visibility = Visibility.Hidden;
                FrameworkElement.checkBoxSelectAirgroupFilter.Visibility = Visibility.Hidden;

#if !Blitz
                FrameworkElement.GeneralSettingsGroupBox.helpButtonAutoReArmRefuel.Visibility = Visibility.Hidden;
                FrameworkElement.GeneralSettingsGroupBox.checkBoxAutoReArm.Visibility = Visibility.Hidden;
                FrameworkElement.GeneralSettingsGroupBox.checkBoxAutoReFuel.Visibility = Visibility.Hidden;
#endif
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                FrameworkElement.GeneralSettingsGroupBox.SetRelationInfo(play.gameInterface, Game.Core.Config);

                if (Game.Core.CurrentCareer != null)
                {
                    hookComboSelectionChanged = true;
                    UpdateCampaignComboBoxInfo();
                    FrameworkElement.comboBoxSelectCampaign.SelectedItem = null;
                    hookComboSelectionChanged = false;
                    SelectLastInfo(Game.Core.CurrentCareer);
                }
                else
                {
                    UpdateCampaignComboBoxInfo();
                }

                UpdateButtonStatus();
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);
            }

            #region Event Handler

            #region ComboBox SelectionChanged

            private void comboBoxSelectCampaign_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    UpdateCampaignMissonComboBoxInfo();
                    UpdateSelectDateInfo();
                }

                UpdateButtonStatus();
            }

            private void comboBoxSelectCampaignMission_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    CampaignInfo campaignInfo = SelectedCampaign;
                    if (campaignInfo != null)
                    {
                        string missionFile = SelectedCampaignMission;
                        if (!string.IsNullOrEmpty(missionFile))
                        {
                            ISectionFile missionTemplateSectionFile = Game.gpLoadSectionFile(missionFile);
                            currentMissionFile = new MissionFile(missionTemplateSectionFile, campaignInfo.AirGroupInfos, MissionFile.LoadLevel.AirGroup);
                        }
                    }
                    missionLoaded = false;
                    UpdateMapNameInfo();
                    UpdateArmyComboBoxInfo(true);
                    UpdateSelectDefaultAirGroupLabel();
                    UpdateTimeComboBoxInfo();
                    UpdateWeatherComboBoxInfo();
                    UpdateCloudAltitudeComboBoxInfo();
                }

                UpdateButtonStatus();
            }

            private void comboBoxSelectArmy_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    UpdateAirForceComboBoxInfo(true);
                }

                UpdateButtonStatus();
            }

            private void comboBoxSelectAirForce_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    UpdateRankComboBoxInfo();
                    UpdateAirGroupComboBoxInfo();
                }

                UpdateButtonStatus();
            }

            private void comboBoxSelectAirGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    UpdateMissionTypeComboBoxInfo();
                    UpdateFlightComboBoxInfo();
                    UpdateFormationComboBoxInfo();
                    UpdateFormationDefaultLabel();
                    UpdateSkillComboBoxInfo();
                    GeneralSettingsGroupBox.UpdateSkillComboBoxSkillValueInfo(FrameworkElement.comboBoxSelectSkill, SelectedSkill, SelectedAirGroup, Game.Core.Config.Skills);
                    UpdateFuelComboBoxInfo();
                }

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

            private void comboBoxSelectMissionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    UpdateFlightDefaultLabel();
                    UpdateSpawnComboBoxInfo();
                }

                UpdateButtonStatus();
            }

            private void comboBoxSpawn_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    UpdateSpeedComboBoxInfo();
                }

                UpdateButtonStatus();
            }

            private void comboBoxSelectTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    FrameworkElement.GeneralSettingsGroupBox.EnableBattleTimeComboBox(SelectedTime == MissionTime.Random);
                }

                UpdateButtonStatus();
            }

            #endregion

            #region ComboBox IsEnabledChanged

            private void comboBoxSelectCampaign_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (Game != null && !(bool)e.NewValue)
                {
                    UpdateCampaignMissonComboBoxInfo();
                    UpdateSelectDateInfo();
                }
            }

            private void comboBoxSelectCampaignMission_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (Game != null && !(bool)e.NewValue)
                {
                    UpdateMapNameInfo();
                    UpdateArmyComboBoxInfo(true);
                    UpdateSelectDefaultAirGroupLabel();
                    UpdateTimeComboBoxInfo();
                    UpdateWeatherComboBoxInfo();
                    UpdateCloudAltitudeComboBoxInfo();
                }
            }

            private void comboBoxSelectArmy_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (Game != null && !(bool)e.NewValue)
                {
                    UpdateAirForceComboBoxInfo(true);
                }
            }

            private void comboBoxSelectAirForce_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (Game != null && !(bool)e.NewValue)
                {
                    UpdateRankComboBoxInfo();
                    UpdateAirGroupComboBoxInfo();
                }
            }

            private void comboBoxSelectAirGroup_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (Game != null && !(bool)e.NewValue)
                {
                    UpdateMissionTypeComboBoxInfo();
                    UpdateFlightComboBoxInfo();
                    UpdateFormationComboBoxInfo();
                    UpdateFormationDefaultLabel();
                    UpdateSkillComboBoxInfo();
                    GeneralSettingsGroupBox.UpdateSkillComboBoxSkillValueInfo(FrameworkElement.comboBoxSelectSkill, SelectedSkill, SelectedAirGroup, Game.Core.Config.Skills);
                    UpdateFuelComboBoxInfo();
                }
            }

            private void comboBoxSelectMissionType_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (Game != null && !(bool)e.NewValue)
                {
                    UpdateSpawnComboBoxInfo();
                    UpdateFlightDefaultLabel();
                }
            }

            private void comboBoxSpawn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (Game != null && !(bool)e.NewValue)
                {
                    UpdateSpeedComboBoxInfo();
                }
            }

            #endregion

            #region GeneralSettingsGroupBox

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

            #endregion

            #region DatePicker

            private void datePickerStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
            {
                UpdateButtonStatus();
            }

            #endregion

            #region CheckBox CheckedChange

            private void checkBoxSelectCampaignFilter_CheckedChange(object sender, RoutedEventArgs e)
            {
                // FrameworkElement.comboBoxSelectCampaign.IsEditable = FrameworkElement.checkBoxSelectCampaignFilter.IsChecked != null && FrameworkElement.checkBoxSelectCampaignFilter.IsChecked.Value;
            }

            private void checkBoxSelectAirgroupFilter_CheckedChange(object sender, RoutedEventArgs e)
            {
                // FrameworkElement.comboBoxSelectAirGroup.IsEditable = FrameworkElement.checkBoxSelectAirgroupFilter.IsChecked!= null && FrameworkElement.checkBoxSelectAirgroupFilter.IsChecked.Value;
            }

            #endregion

            #region Button Click

            private void buttonReload_Click(object sender, RoutedEventArgs e)
            {
                if (Reload(sender, e))
                {
                    Game.gameInterface.PageChange(new QuickMissionPage(), null);
                }
            }

            private void buttonStats_Click(object sender, RoutedEventArgs e)
            {
                string pilotName = Game.gameInterface.Player().Name();
                string resultFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, pilotName, Config.StatsInfoFileName);
                ISectionFile resultFile = Game.gpLoadSectionFile(resultFileName);
                if (resultFile != null)
                {
                    Career career = new Career(pilotName, -1, -1, -1);
                    career.ReadResult(resultFile);
                    TotalStatsWindow window = new TotalStatsWindow(career);
                    window.Title = "Quick Mission Total Status [IL2DCE]";
                    window.ShowDialog();
                }
            }

            private void Back_Click(object sender, RoutedEventArgs e)
            {
                if (Game.gameInterface.BattleIsRun())
                {
                    Game.gameInterface.BattleStop();
                }

                if (Game.Core.CurrentCareer != null)
                {
                    Game.Core.CurrentCareer.InitQuickMssionInfo();
                    Game.Core.CurrentCareer = null;
                }

                Game.gameInterface.PagePop(null);
            }

            private void Start_Click(object sender, RoutedEventArgs e)
            {
                Config config = Game.Core.Config;
                GameIterface gameInterface = Game.gameInterface;
                string pilotName = gameInterface.Player().Name();

                int armyIndex = SelectedArmyIndex;
                int airForceIndex = SelectedAirForceIndex;
                int rankIndex = SelectedRank;
                AirGroup airGroup = SelectedAirGroup;
                CampaignInfo campaignInfo = SelectedCampaign;
                GeneralSettingsGroupBox generalSettings = FrameworkElement.GeneralSettingsGroupBox;

                try
                {
                    Career career = new Career(pilotName, armyIndex, airForceIndex, rankIndex);
                    career.BattleType = EBattleType.QuickMission;
                    career.CampaignInfo = campaignInfo;
                    career.CampaignMode = SelectedCampaignMissionIndex == 0 ? ECampaignMode.Default : ECampaignMode.Progress;
                    career.MissionIndex = SelectedCampaignMissionIndex;
                    career.AirGroup = airGroup.ToString();
                    career.AirGroupDisplay = airGroup.VirtualAirGroupKey;
                    career.PlayerAirGroup = airGroup;
                    career.Aircraft = AircraftInfo.CreateDisplayName(airGroup.Class);
                    career.Date = FrameworkElement.datePickerStart.SelectedDate.Value;
                    career.MissionType = SelectedMissionType;
                    career.Flight = SelectedFlight;
                    career.Formation = SelectedFormation;
                    career.Spawn = SelectedSpawn;
                    career.Fuel = SelectedFuel;
                    career.Speed = SelectedSpeed;
                    career.PlayerAirGroupSkill = SelectedSkill != null && SelectedSkill != Skill.Random ? new Skill[] { SelectedSkill } : null;
                    career.Time = SelectedTime;
                    career.Weather = SelectedWeather;
                    career.CloudAltitude = SelectedCloudAltitude;
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
                    career.SpawnRandomLocationPlayer = generalSettings.SelectedSpawnRandomLocationPlayer;
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

                    Game.Core.CurrentCareer = career;

                    career.EndDate = career.StartDate = career.Date.Value;
                    Game.Core.CreateQuickMission(Game, career);

                    Game.gameInterface.PageChange(new BattleIntroPage(), null);
                }
                catch (Exception ex)
                {
                    string message = string.Format("{0} - {1} {2} {3} {4}", "QuickMissionPage.Start_Click", ex.Message, campaignInfo.Name, airGroup.DisplayDetailName, ex.StackTrace);
                    Core.WriteLog(message);
                    MessageBox.Show(string.Format("{0}", ex.Message), Config.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void buttonUserLoad_Click(object sender, RoutedEventArgs e)
            {
                string filePath = string.Format("{0}/{1}", Config.UserMissionsFolder, Config.DQMSettingFileName);
                string fileSystemPath = Game.gameInterface.ToFileSystemPath(filePath);
                if (File.Exists(fileSystemPath))
                {
                    try
                    {
                        ISectionFile file = Game.gameInterface.SectionFileLoad(filePath);
                        Read(file);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("{0} - {1}", "QuickMissionPage.buttonUserLoad_Click", ex.Message, ex.StackTrace);
                        Core.WriteLog(message);
                    }
                }
                else
                {
                    MessageBox.Show(string.Format(GeneralSettingsGroupBox.MsgErrorFileNotFound, fileSystemPath), Config.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            private void buttonUserSave_Click(object sender, RoutedEventArgs e)
            {
                string filePath = string.Format("{0}/{1}", Config.UserMissionsFolder, Config.DQMSettingFileName);
                string fileSystemPath = Game.gameInterface.ToFileSystemPath(filePath);
                if (FileUtil.IsFileWritable(fileSystemPath))
                {
                    try
                    {
                        ISectionFile file = Game.gameInterface.SectionFileCreate();
                        Write(file);
                        file.save(filePath);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("{0} - {1}", "QuickMissionPage.buttonUserSaveAs_Click", ex.Message, ex.StackTrace);
                        Core.WriteLog(message);
                    }
                }
                else
                {
                    MessageBox.Show(string.Format(GeneralSettingsGroupBox.MsgErrorFileLocked, fileSystemPath), Config.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            #endregion

            #endregion

            #region Import and Convert Mission File

            protected override void ImportMissionProgressWindow(ProgressWindowModel model, bool isCampaign)
            {
                base.ImportMissionProgressWindow(model, isCampaign);
                Game.gameInterface.PageChange(new QuickMissionPage(), null);
            }

            #endregion

            #region ComboBox Cotrol

            private void UpdateCampaignComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectCampaign;
                // comboBox.IsEditable = Game.Core.Config.EnableFilterSelectCampaign;

                comboBox.Items.Clear();

                foreach (CampaignInfo campaignInfo in Game.Core.CampaignInfos)
                {
                    comboBox.Items.Add(campaignInfo);
                }

                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateCampaignMissonComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectCampaignMission;
                comboBox.Items.Clear();

                CampaignInfo campaignInfo = SelectedCampaign;
                if (campaignInfo != null)
                {
                    for (int i = 0; i < campaignInfo.InitialMissionTemplateFileCount; i++)
                    {
                        string missionFile = campaignInfo.MissionTemplateFile(ECampaignMode.Progress, i, null);
                        comboBox.Items.Add(new ComboBoxItem() { Tag = missionFile, Content = FileUtil.GetGameFileNameWithoutExtension(missionFile), });
                    }
                }

                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
                comboBox.IsEnabled = comboBox.Items.Count > 1;
            }

            private void UpdateArmyComboBoxInfo(bool checkArmy = false)
            {

                ComboBox comboBox = FrameworkElement.comboBoxSelectArmy;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                if (SelectedCampaignMissionIndex != -1 && (!checkArmy || currentMissionFile != null))
                {
                    var armys = checkArmy ? currentMissionFile.AirGroups.Select(x => x.ArmyIndex).Distinct() : new int[0];
                    for (EArmy army = EArmy.Red; army <= EArmy.Blue; army++)
                    {
                        if (!checkArmy || armys.Contains((int)army))
                        {
                            comboBox.Items.Add(new ComboBoxItem() { Tag = (int)army, Content = army.ToString() });
                        }
                    }
                }

                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            private void UpdateAirForceComboBoxInfo(bool checkAirForce = false)
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectAirForce;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                int armyIndex = SelectedArmyIndex;
                if (SelectedCampaignMissionIndex != -1 && armyIndex != -1 && (!checkAirForce || currentMissionFile != null))
                {
                    var airForces = checkAirForce ? currentMissionFile.AirGroups.Where(x => x.ArmyIndex == armyIndex).Select(x => x.AirGroupInfo.AirForceIndex).Distinct() : new int[0];
                    if (armyIndex == (int)EArmy.Red)
                    {
                        foreach (var item in Enum.GetValues(typeof(EAirForceRed)))
                        {
                            if (!checkAirForce || airForces.Contains((int)item))
                            {
                                comboBox.Items.Add(new ComboBoxItem() { Tag = (int)item, Content = ((EAirForceRed)item).ToDescription() });
                            }
                        }
                    }
                    else if (armyIndex == (int)EArmy.Blue)
                    {
                        foreach (var item in Enum.GetValues(typeof(EAirForceBlue)))
                        {
                            if (!checkAirForce || airForces.Contains((int)item))
                            {
                                comboBox.Items.Add(new ComboBoxItem() { Tag = (int)item, Content = ((EAirForceBlue)item).ToDescription() });
                            }
                        }
                    }
                }

                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            private void UpdateRankComboBoxInfo()
            {
                UpdateRankComboBoxInfo(FrameworkElement.comboBoxSelectRank);
            }

            private void UpdateAirGroupComboBoxInfo()
            {
                UpdateAirGroupComboBoxInfo(FrameworkElement.comboBoxSelectAirGroup);
            }

            protected override void UpdateAirGroupContent()
            {
                UpdateAirGroupContent(FrameworkElement.comboBoxSelectAirGroup);
            }

            private void UpdateSelectDefaultAirGroupLabel()
            {
                UpdateSelectDefaultAirGroupLabel(FrameworkElement.labelDefaultSelectAirGroup);
            }

            private void UpdateMissionTypeComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectMissionType;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                CampaignInfo campaignInfo = SelectedCampaign;
                AirGroup airGroup = SelectedAirGroup;
                if (campaignInfo != null && currentMissionFile != null && airGroup != null)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = RandomString });
                    // AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                    EMissionType[] disable = Game.Core.Config.DisableMissionType;
                    AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                    foreach (var item in aircraftInfo.MissionTypes)
                    {
                        if (!disable.Contains(item))
                        {
                            comboBox.Items.Add(new ComboBoxItem() { Tag = item, Content = item.ToDescription() });
                        }
                    }
                }

                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            private void UpdateFlightComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectFlight;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                AirGroup airGroup = SelectedAirGroup;
                if (currentMissionFile != null && airGroup != null)
                {
                    Config config = Game.Core.Config;
                    AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                    int flightCount;
                    int flightSize;
                    Flight.GetOptimaizeValue(out flightCount, out flightSize, airGroupInfo.FlightCount, airGroupInfo.FlightSize, config.FlightCount, config.FlightSize);
                    // comboBox.Items.Add(new ComboBoxItem() { Tag = (int)EFlight.MissionDefault, Content = Flight.CreateDisplayString((int)EFlight.MissionDefault) });
                    comboBox.Items.Add(new ComboBoxItem() { Tag = (int)EFlight.Default, Content = Flight.CreateDisplayString((int)EFlight.Default) });
                    for (int i = 0; i < flightCount; i++)
                    {
                        for (int j = 0; j < flightSize; j++)
                        {
                            int value = Flight.Value(i + 1, j + 1);
                            comboBox.Items.Add(new ComboBoxItem() { Tag = value, Content = Flight.CreateDisplayString(value) });
                        }
                    }
                }

                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            private void UpdateFlightDefaultLabel()
            {
                string missionTypeDefault = string.Empty;
                AirGroup airGroup = SelectedAirGroup;
                if (airGroup != null && airGroup.AirGroupInfo != null)
                {
                    AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                    EMissionType? missionType = SelectedMissionType;
                    if (missionType != null)
                    {
                        Config config = Game.Core.Config;
                        int flightCount;
                        int flightSize;
                        Flight.GetOptimaizeValue(out flightCount, out flightSize, missionType.Value, airGroupInfo.FlightCount, airGroupInfo.FlightSize,
                                                    config.FlightCount, config.FlightSize, airGroup.TargetAirGroup != null ? airGroup.TargetAirGroup.Flights.Count : 0);
                        missionTypeDefault = Flight.CreateDisplayString(flightCount, flightSize);
                    }
                    else
                    {
                        missionTypeDefault = "Randomly";
                    }
                }
                FrameworkElement.labelDefaultFlight.Content = string.IsNullOrEmpty(missionTypeDefault) ? string.Empty : string.Format(MissionTypeDefaultFormat, missionTypeDefault);
            }

            private void UpdateFormationComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectFormation;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                AirGroup airGroup = SelectedAirGroup;
                if (currentMissionFile != null && airGroup != null)
                {
                    Config config = Game.Core.Config;
                    AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                    Formations formations = Formations.Default[airGroupInfo.FormationsType];

                    // comboBox.Items.Add(new ComboBoxItem() { Tag = EFormation.Random, Content = EFormation.Random.ToDescription() });
                    comboBox.Items.Add(new ComboBoxItem() { Tag = EFormation.Default, Content = EFormation.Default.ToDescription() });
                    formations.ForEach(x => comboBox.Items.Add(new ComboBoxItem() { Tag = x, Content = x.ToDescription() }));
                }

                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            private void UpdateFormationDefaultLabel()
            {
                string missionDefault = string.Empty;
                AirGroup airGroup = SelectedAirGroup;
                if (airGroup != null)
                {
                    missionDefault = airGroup.Formation;
                }
                FrameworkElement.labelDefaultFormation.Content = string.IsNullOrEmpty(missionDefault) ? string.Empty : string.Format(MissionDefaultFormat, missionDefault);
            }

            private void UpdateSpawnComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSpawn;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                defaultMissionAltitude = -1;
                CampaignInfo campaignInfo = SelectedCampaign;
                AirGroup airGroup = SelectedAirGroup;
                if (campaignInfo != null && currentMissionFile != null && airGroup != null)
                {
                    AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                    EMissionType? missionType = SelectedMissionType;
                    AircraftParametersInfo aircraftParam = missionType != null && aircraftInfo != null ? aircraftInfo.GetAircraftParametersInfo(missionType.Value).FirstOrDefault() : null;
                    AirGroupWaypoint way = airGroup.Waypoints.FirstOrDefault();
                    if (way != null)
                    {
                        // comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = RandomString });
                        comboBox.Items.Add(new ComboBoxItem() { Tag = ESpawn.Default, Content = ESpawn.Default.ToDescription() });
                        if (way.Type == AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF)
                        {
                            comboBox.Items.Add(new ComboBoxItem() { Tag = ESpawn.Parked, Content = ESpawn.Parked.ToDescription() });
                            comboBox.Items.Add(new ComboBoxItem() { Tag = ESpawn.Idle, Content = ESpawn.Idle.ToDescription() });
                            // comboBox.Items.Add(new ComboBoxItem() { Tag = ESpawn.Scramble, Content = ESpawn.Scramble.ToDescription() });
                        }
                        int startAlt = aircraftParam != null && aircraftParam.MinAltitude != null ? Math.Max((int)aircraftParam.MinAltitude.Value, Spawn.SelectStartAltitude) : Spawn.SelectStartAltitude;
                        int endAlt = aircraftParam != null && aircraftParam.MaxAltitude != null ? Math.Min((int)aircraftParam.MaxAltitude.Value, Spawn.SelectEndAltitude) : Spawn.SelectEndAltitude;
                        for (int i = startAlt; i <= endAlt; i += Spawn.SelectStepAltitude)
                        {
                            comboBox.Items.Add(new ComboBoxItem() { Tag = i, Content = Spawn.CreateDisplayString(i) });
                        }
                        defaultMissionAltitude = airGroup.SetOnParked ? (int)ESpawn.Parked :
                                            way.Type == AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF ? (int)ESpawn.Idle : (int)way.Z;
                    }
                }
                EnableSelectItem(comboBox, selected, currentMissionFile == null);
                FrameworkElement.labelDefaultAltitude.Content = defaultMissionAltitude >= 0 ? string.Format(Config.NumberFormat, MissionDefaultFormat, defaultMissionAltitude) :
                                                                defaultMissionAltitude >= (int)ESpawn.Parked && defaultMissionAltitude <= (int)ESpawn.AirStart ?
                                                                        string.Format(Config.NumberFormat, MissionDefaultFormat, ((ESpawn)defaultMissionAltitude).ToDescription()) : string.Empty;
            }

            private void UpdateSpeedComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSpeed;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;

                if (comboBox.Items.Count == 0)
                {
                    // comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = RandomString });
                    comboBox.Items.Add(new ComboBoxItem() { Tag = -1, Content = DefaultString });
                    for (int i = Speed.SelectMinSpeed; i <= Speed.SelectMaxSpeed; i += Speed.SelectStepSpeed)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = i, Content = Speed.CreateDisplayString(i) });
                    }
                }

                int speedMissonDefault = -1;
                int spawn = SelectedSpawn;
                if (spawn == (int)ESpawn.Parked || spawn == (int)ESpawn.Idle || ((spawn == (int)ESpawn.Default) && defaultMissionAltitude <= 0))
                {
                    speedMissonDefault = 0;
                    comboBox.SelectedIndex = 0;
                    comboBox.IsEnabled = false;
                }
                else
                {
                    comboBox.IsEnabled = true;
                    AirGroup airGroup = SelectedAirGroup;
                    if (SelectedCampaignMissionIndex != -1 && currentMissionFile != null && airGroup != null)
                    {
                        AirGroupWaypoint way = airGroup.Waypoints.FirstOrDefault();
                        if (way != null)
                        {
                            speedMissonDefault = ((int)way.V);
                        }
                    }
                    EnableSelectItem(comboBox, selected, currentMissionFile == null);
                }
                FrameworkElement.labelDefaultSpeed.Content = speedMissonDefault >= 0 ? string.Format(Config.NumberFormat, MissionDefaultFormat, speedMissonDefault) : string.Empty;
            }

            private void UpdateFuelComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxFuel;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;

                if (comboBox.Items.Count == 0)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = -1, Content = DefaultString });
                    for (int i = Fuel.SelectMaxFuel; i >= Fuel.SelectMinFuel; i -= Fuel.SelectStepFuel)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = i, Content = Fuel.CreateDisplayString(i) });
                    }
                }

                int fuelMissionDefault = -1;
                AirGroup airGroup = SelectedAirGroup;
                if (SelectedCampaignMissionIndex != -1 && currentMissionFile != null && airGroup != null)
                {
                    fuelMissionDefault = airGroup.Fuel;
                }
                FrameworkElement.labelDefaultFuel.Content = fuelMissionDefault >= 0 ? string.Format(Config.NumberFormat, MissionDefaultFormat, airGroup.Fuel) : string.Empty;
                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            private void UpdateSkillComboBoxInfo()
            {
                UpdateSkillComboBoxInfo(FrameworkElement.comboBoxSelectSkill, FrameworkElement.labelDefaultSkill);
            }

            private void UpdateTimeComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectTime;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;

                if (comboBox.Items.Count == 0)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = MissionTime.Default, Content = DefaultString });
                    comboBox.Items.Add(new ComboBoxItem() { Tag = MissionTime.Random, Content = RandomString });
                    for (double d = MissionTime.Begin; d <= MissionTime.End; d += 0.5)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = d, Content = MissionTime.ToString(d) });
                    }
                }

                string defaultString = string.Empty;
                if (currentMissionFile != null)
                {
                    double time = currentMissionFile.Time;
                    defaultString = time >= 0 ? string.Format(MissionDefaultFormat, MissionTime.ToString(time)) : string.Empty;
                }
                FrameworkElement.labelDefaultTime.Content = defaultString;

                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            private void UpdateWeatherComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectWeather;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;

                if (comboBox.Items.Count == 0)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = EWeather.Default, Content = DefaultString });
                    comboBox.Items.Add(new ComboBoxItem() { Tag = EWeather.Random, Content = RandomString });
                    for (EWeather w = EWeather.Clear; w <= EWeather.MediumClouds; w++)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = w, Content = w.ToDescription() });
                    }
                }

                string defaultString = string.Empty;
                if (currentMissionFile != null)
                {
                    int weather = currentMissionFile.WeatherIndex;
                    defaultString = weather >= (int)EWeather.Clear && weather <= (int)EWeather.MediumClouds ?
                        string.Format(Config.NumberFormat, MissionDefaultFormat, ((EWeather)weather).ToDescription()) : string.Empty;
                }
                FrameworkElement.labelDefaultWeather.Content = defaultString;

                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            private void UpdateCloudAltitudeComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectCloudAltitude;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;

                if (comboBox.Items.Count == 0)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = CloudAltitude.Default, Content = DefaultString });
                    comboBox.Items.Add(new ComboBoxItem() { Tag = CloudAltitude.Random, Content = RandomString });
                    for (int alt = (int)CloudAltitude.Min; alt <= CloudAltitude.Max; alt += CloudAltitude.Step)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = alt, Content = CloudAltitude.CreateDisplayString(alt) });
                    }
                }

                string defaultString = string.Empty;
                if (currentMissionFile != null)
                {
                    int alt = currentMissionFile.CloudsHeight;
                    defaultString = alt >= 0 ? string.Format(Config.NumberFormat, MissionDefaultFormat, CloudAltitude.CreateDisplayString(alt)) : string.Empty;
                }
                FrameworkElement.labelDefaultCloudAltitude.Content = defaultString;

                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            #endregion

            private void UpdateMapNameInfo()
            {
                UpdateMapNameInfo(FrameworkElement.labelMapName);
            }

            private void UpdateSelectDateInfo()
            {
                DatePicker picker = FrameworkElement.datePickerStart;
                CampaignInfo campaignInfo = SelectedCampaign;
                picker.SelectedDate = campaignInfo != null ? (Nullable<DateTime>)campaignInfo.StartDate : null;
                FrameworkElement.labelDefaultSelectDate.Content = string.Format(Config.DateTimeFormat, MissionDefaultDateFormat, campaignInfo.StartDate, campaignInfo.EndDate);
            }

            private void UpdateButtonStatus()
            {
                GeneralSettingsGroupBox generalSettings = FrameworkElement.GeneralSettingsGroupBox;
                int addGroundOpe = generalSettings.SelectedAdditionalGroundOperations;
                int timeBegin = generalSettings.SelectedRandomTimeBegin;
                int timeEnd = generalSettings.SelectedRandomTimeEnd;
                bool timeEnable = generalSettings.SelectedSpawnRandomTimeEnemy || generalSettings.SelectedSpawnRandomTimeFriendly;
                double timeBattleBegin = generalSettings.SelectedBattleTimeBegin;
                double timeBattleEnd = generalSettings.SelectedBattleTimeEnd;
                FrameworkElement.Start.IsEnabled = SelectedArmyIndex != -1 && SelectedAirForceIndex != -1 && SelectedCampaign != null &&
                    !string.IsNullOrEmpty(SelectedCampaignMission) && SelectedCampaignMissionIndex != -1 && SelectedAirGroup != null && SelectedRank != -1 &&
                    addGroundOpe >= Config.MinAdditionalGroundOperations && addGroundOpe <= Config.MaxAdditionalGroundOperations &&
                    (!timeEnable || timeEnable && timeBegin >= SpawnTime.MinimumBeginSec && timeEnd <= SpawnTime.MaximumEndSec && timeBegin <= timeEnd) &&
                    FrameworkElement.datePickerStart.SelectedDate != null && FrameworkElement.datePickerStart.SelectedDate.HasValue &&
                    timeBattleBegin != -1 && timeBattleEnd != -1 && timeBattleBegin <= timeBattleEnd;
            }

            private void SelectLastInfo(Career career)
            {
                GeneralSettingsGroupBox generalSettings = FrameworkElement.GeneralSettingsGroupBox;
                FrameworkElement.comboBoxSelectCampaign.Text = career.CampaignInfo.ToString();
                FrameworkElement.comboBoxSelectCampaignMission.SelectedIndex = career.MissionIndex < FrameworkElement.comboBoxSelectCampaignMission.Items.Count ? career.MissionIndex : FrameworkElement.comboBoxSelectCampaignMission.Items.Count > 0 ? 0 : -1;
                EnableSelectItem(FrameworkElement.comboBoxSelectArmy, ((EArmy)career.ArmyIndex).ToString());
                EnableSelectItem(FrameworkElement.comboBoxSelectAirForce, ((EArmy)career.ArmyIndex) == EArmy.Red ? ((EAirForceRed)career.AirForceIndex).ToDescription() : ((EAirForceBlue)career.AirForceIndex).ToDescription());
                FrameworkElement.comboBoxSelectRank.SelectedIndex = career.RankIndex;
                EnableSelectItem(FrameworkElement.comboBoxSelectAirGroup, CreateAirGroupContent(career.PlayerAirGroup, career.CampaignInfo, string.Empty));
                EnableSelectItem(FrameworkElement.comboBoxSelectMissionType, career.MissionType != null ? career.MissionType.ToDescription() : string.Empty);
                EnableSelectItem(FrameworkElement.comboBoxSelectFlight, Flight.CreateDisplayString(career.Flight));
                EnableSelectItem(FrameworkElement.comboBoxSelectFormation, career.Formation.ToDescription());
                EnableSelectItem(FrameworkElement.comboBoxSpawn, Spawn.CreateDisplayString(career.Spawn));
                EnableSelectItem(FrameworkElement.comboBoxSpeed, Speed.CreateDisplayString(career.Speed));
                EnableSelectItem(FrameworkElement.comboBoxFuel, Fuel.CreateDisplayString(career.Fuel));
                EnableSelectItem(FrameworkElement.comboBoxSelectSkill, career.PlayerAirGroupSkill != null && career.PlayerAirGroupSkill.Length > 0 ? career.PlayerAirGroupSkill.First().Name : string.Empty);
                FrameworkElement.datePickerStart.SelectedDate = career.Date != null && career.Date.HasValue ? (Nullable<DateTime>)career.Date.Value : null;
                EnableSelectItem(FrameworkElement.comboBoxSelectTime, career.Time >= 0 ? MissionTime.ToString(career.Time) : string.Empty);
                EnableSelectItem(FrameworkElement.comboBoxSelectWeather, (int)career.Weather >= 0 ? ((EWeather)career.Weather).ToDescription() : string.Empty);
                EnableSelectItem(FrameworkElement.comboBoxSelectCloudAltitude, career.CloudAltitude >= 0 ? CloudAltitude.CreateDisplayString(career.CloudAltitude) : string.Empty);
                EnableSelectItem(generalSettings.comboBoxSelectAdditionalAirOperations, career.AdditionalAirOperations >= 0 ? career.AdditionalAirOperations.ToString() : string.Empty);
                EnableSelectItem(generalSettings.comboBoxSelectAdditionalGroundOperations, career.AdditionalGroundOperations >= 0 ? career.AdditionalGroundOperations.ToString() : string.Empty);
                generalSettings.checkBoxAdditionalAirGroups.IsChecked = career.AdditionalAirGroups;
                generalSettings.checkBoxAdditionalGroundGroups.IsChecked = career.AdditionalGroundGroups;
                generalSettings.checkBoxAdditionalStationaryUnits.IsChecked = career.AdditionalStationaries;
                generalSettings.checkBoxKeepUnitsAirGroups.IsChecked = career.SpawnDynamicAirGroups;
                generalSettings.checkBoxKeepUnitsGroundGroups.IsChecked = career.SpawnDynamicGroundGroups;
                generalSettings.checkBoxKeepUnitsStationaryUnits.IsChecked = career.SpawnDynamicStationaries;
                generalSettings.checkBoxSpawnRandomLocationFriendly.IsChecked = career.SpawnRandomLocationFriendly;
                generalSettings.checkBoxSpawnRandomLocationEnemy.IsChecked = career.SpawnRandomLocationEnemy;
                generalSettings.checkBoxSpawnRandomLocationPlayer.IsChecked = career.SpawnRandomLocationPlayer;
                generalSettings.checkBoxSpawnRandomAltitudeFriendly.IsChecked = career.SpawnRandomAltitudeFriendly;
                generalSettings.checkBoxSpawnRandomAltitudeEnemy.IsChecked = career.SpawnRandomAltitudeEnemy;
                EnableSelectItem(generalSettings.comboBoxSelectRandomTimeBegin, career.SpawnRandomTimeBeginSec >= 0 ? career.SpawnRandomTimeBeginSec.ToString() : string.Empty);
                EnableSelectItem(generalSettings.comboBoxSelectRandomTimeEnd, career.SpawnRandomTimeEndSec >= 0 ? career.SpawnRandomTimeEndSec.ToString() : string.Empty);
                generalSettings.checkBoxSpawnRandomTimeFriendly.IsChecked = career.SpawnRandomTimeFriendly;
                generalSettings.checkBoxSpawnRandomTimeEnemy.IsChecked = career.SpawnRandomTimeEnemy;
                EnableSelectItem(generalSettings.comboBoxSelectUnitNumsArmor, career.ArmorUnitNumsSet.ToDescription());
                EnableSelectItem(generalSettings.comboBoxSelectUnitNumsShip, career.ShipUnitNumsSet.ToDescription());
                generalSettings.checkBoxGroundGroupGeneric.IsChecked = career.GroundGroupGenerateType == EGroundGroupGenerateType.Generic;
                generalSettings.checkBoxStatiomaryGeneric.IsChecked = career.StationaryGenerateType == EStationaryGenerateType.Generic;
                EnableSelectItem(generalSettings.comboBoxSelectAISkill, career.AISkill.ToDescription());
                EnableSelectItem(generalSettings.comboBoxSelectArtilleryTimeout, ArtilleryOption.CreateDisplayStringTimeout(career.ArtilleryTimeout));
                EnableSelectItem(generalSettings.comboBoxSelectArtilleryRhide, ArtilleryOption.CreateDisplayStringRHide(career.ArtilleryRHide));
                EnableSelectItem(generalSettings.comboBoxSelectArtilleryZOffset, ArtilleryOption.CreateDisplayStringZOffsete(career.ArtilleryZOffset));
                EnableSelectItem(generalSettings.comboBoxSelectShipSkill, career.ShipSkill.ToDescription());
                EnableSelectItem(generalSettings.comboBoxSelectShipSleep, ShipOption.CreateDisplayStringSleep(career.ShipSleep));
                EnableSelectItem(generalSettings.comboBoxSelectShipSlowfire, ShipOption.CreateDisplayStringSlowfire(career.ShipSlowfire));
                generalSettings.checkBoxAutoReArm.IsChecked = career.ReArmTime >= 0;
                generalSettings.checkBoxAutoReFuel.IsChecked = career.ReFuelTime >= 0;
                generalSettings.checkBoxTrackRecording.IsChecked = career.TrackRecording;
                EnableSelectItem(generalSettings.comboBoxSelectBattleTimeBegin, MissionTime.ToString(career.RandomTimeBegin >= 0 ? career.RandomTimeBegin : MissionTime.Begin));
                EnableSelectItem(generalSettings.comboBoxSelectBattleTimeEnd, MissionTime.ToString(career.RandomTimeEnd >= 0 ? career.RandomTimeEnd : MissionTime.End));
            }

            private void UpdateAircraftImage()
            {
                UpdateAircraftImage(FrameworkElement.borderImage);
            }

            private void Write(ISectionFile file)
            {
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectCampaign.Name, FrameworkElement.comboBoxSelectCampaign.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectCampaignMission.Name, FrameworkElement.comboBoxSelectCampaignMission.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectArmy.Name, FrameworkElement.comboBoxSelectArmy.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectAirForce.Name, FrameworkElement.comboBoxSelectAirForce.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectRank.Name, FrameworkElement.comboBoxSelectRank.Text);

                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectAirGroup.Name, FrameworkElement.comboBoxSelectAirGroup.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectSkill.Name, FrameworkElement.comboBoxSelectSkill.Text);

                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectMissionType.Name, FrameworkElement.comboBoxSelectMissionType.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectFlight.Name, FrameworkElement.comboBoxSelectFlight.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectFormation.Name, FrameworkElement.comboBoxSelectFormation.Text);

                file.add(SectionDQMSetting, FrameworkElement.comboBoxSpawn.Name, FrameworkElement.comboBoxSpawn.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSpeed.Name, FrameworkElement.comboBoxSpeed.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxFuel.Name, FrameworkElement.comboBoxFuel.Text);

                file.add(SectionDQMSetting, FrameworkElement.datePickerStart.Name, FrameworkElement.datePickerStart.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectTime.Name, FrameworkElement.comboBoxSelectTime.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectWeather.Name, FrameworkElement.comboBoxSelectWeather.Text);
                file.add(SectionDQMSetting, FrameworkElement.comboBoxSelectCloudAltitude.Name, FrameworkElement.comboBoxSelectCloudAltitude.Text);
            }

            private void Read(ISectionFile file)
            {
                GeneralSettingsGroupBox.SelectReadValue(file, SectionDQMSetting, FrameworkElement.comboBoxSelectCampaign);
                GeneralSettingsGroupBox.SelectReadValue(file, SectionDQMSetting, FrameworkElement.comboBoxSelectCampaignMission);
                GeneralSettingsGroupBox.SelectReadValue(file, SectionDQMSetting, FrameworkElement.comboBoxSelectArmy);
                GeneralSettingsGroupBox.SelectReadValue(file, SectionDQMSetting, FrameworkElement.comboBoxSelectAirForce);
                GeneralSettingsGroupBox.SelectReadValue(file, SectionDQMSetting, FrameworkElement.comboBoxSelectRank);

                GeneralSettingsGroupBox.SelectReadValue(file, SectionDQMSetting, FrameworkElement.comboBoxSelectAirGroup);
                FrameworkElement.comboBoxSelectSkill.Text = file.get(SectionDQMSetting, FrameworkElement.comboBoxSelectSkill.Name, Skill.Default.Name);

                FrameworkElement.comboBoxSelectMissionType.Text = file.get(SectionDQMSetting, FrameworkElement.comboBoxSelectMissionType.Name, RandomString);
                FrameworkElement.comboBoxSelectFlight.Text = file.get(SectionDQMSetting, FrameworkElement.comboBoxSelectFlight.Name, Flight.CreateDisplayString((int)EFlight.Default));
                FrameworkElement.comboBoxSelectFormation.Text = file.get(SectionDQMSetting, FrameworkElement.comboBoxSelectFormation.Name, EFormation.Default.ToDescription());

                FrameworkElement.comboBoxSpawn.Text = file.get(SectionDQMSetting, FrameworkElement.comboBoxSpawn.Name, ESpawn.Default.ToDescription());
                FrameworkElement.comboBoxSpeed.Text = file.get(SectionDQMSetting, FrameworkElement.comboBoxSpeed.Name, DefaultString);
                FrameworkElement.comboBoxFuel.Text = file.get(SectionDQMSetting, FrameworkElement.comboBoxFuel.Name, DefaultString);

                FrameworkElement.datePickerStart.Text = file.get(SectionDQMSetting, FrameworkElement.datePickerStart.Name, string.Empty);
                FrameworkElement.comboBoxSelectTime.Text = file.get(SectionDQMSetting, FrameworkElement.comboBoxSelectTime.Name, DefaultString);
                FrameworkElement.comboBoxSelectWeather.Text = file.get(SectionDQMSetting, FrameworkElement.comboBoxSelectWeather.Name, DefaultString);
                FrameworkElement.comboBoxSelectCloudAltitude.Text = file.get(SectionDQMSetting, FrameworkElement.comboBoxSelectCloudAltitude.Name, DefaultString);
            }
        }
    }
}