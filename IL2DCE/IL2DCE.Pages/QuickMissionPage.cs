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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using IL2DCE.Pages.Controls;
using IL2DCE.Util;
using maddox.game;
using maddox.game.play;
using static IL2DCE.MissionObjectModel.Spawn;

namespace IL2DCE
{
    namespace Pages
    {
        public class QuickMissionPage : PageDefImpl
        {

            #region Constant

            private const string RandomString = "[Random]";
            private const string DefaultString = "Default";
            private const string MissionDefaultFormat = "Mission Default: {0}";
            private const string MissionTypeDefaultFormat = "Mission Type Default: {0}";

            #endregion

            #region Property

            private QuickMission FrameworkElement
            {
                get
                {
                    return FE as QuickMission;
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

            private MissionFile currentMissionFile = null;

            private bool hookComboSelectionChanged = false;

            private int defaultMissionAltitude = 0;

            #endregion

            public QuickMissionPage()
                : base("Quick Mission", new QuickMission())
            {
                FrameworkElement.Start.Click += new System.Windows.RoutedEventHandler(Start_Click);
                FrameworkElement.Back.Click += new System.Windows.RoutedEventHandler(Back_Click);
                FrameworkElement.comboBoxSelectCampaign.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectCampaign_SelectionChanged);
                FrameworkElement.comboBoxSelectArmy.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectArmy_SelectionChanged);
                FrameworkElement.comboBoxSelectAirForce.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirForce_SelectionChanged);
                FrameworkElement.comboBoxSelectAirGroup.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirGroup_SelectionChanged);
                FrameworkElement.comboBoxSelectSkill.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectSkill_SelectionChanged);
                FrameworkElement.comboBoxSelectMissionType.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectMissionType_SelectionChanged);
                FrameworkElement.comboBoxSpawn.SelectionChanged += new SelectionChangedEventHandler(comboBoxSpawn_SelectionChanged);
                FrameworkElement.comboBoxSpawn.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSpawn_IsEnabledChanged);
                FrameworkElement.comboBoxSelectCampaign.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSelectCampaign_IsEnabledChanged);
                FrameworkElement.comboBoxSelectArmy.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSelectArmy_IsEnabledChanged);
                FrameworkElement.comboBoxSelectAirForce.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSelectAirForce_IsEnabledChanged);
                FrameworkElement.comboBoxSelectAirGroup.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSelectAirGroup_IsEnabledChanged);
                FrameworkElement.comboBoxSelectMissionType.IsEnabledChanged += new DependencyPropertyChangedEventHandler(comboBoxSelectMissionType_IsEnabledChanged);

                FrameworkElement.GeneralSettingsGroupBox.ComboBoxSelectionChangedEvent += new SelectionChangedEventHandler(GeneralSettingsGroupBox_ComboBoxSelectionChangedEvent);
                FrameworkElement.GeneralSettingsGroupBox.ComboBoxTextChangedEvent += new TextChangedEventHandler(GeneralSettingsGroupBox_ComboBoxTextChangedEvent);

                FrameworkElement.checkBoxSelectCampaignFilter.Checked += new RoutedEventHandler(checkBoxSelectCampaignFilter_CheckedChange);
                FrameworkElement.checkBoxSelectCampaignFilter.Unchecked += new RoutedEventHandler(checkBoxSelectCampaignFilter_CheckedChange);
                FrameworkElement.checkBoxSelectAirgroupFilter.Checked += new RoutedEventHandler(checkBoxSelectAirgroupFilter_CheckedChange);
                FrameworkElement.checkBoxSelectAirgroupFilter.Unchecked += new RoutedEventHandler(checkBoxSelectAirgroupFilter_CheckedChange);

                FrameworkElement.buttonImportMission.Click += new RoutedEventHandler(buttonImportMission_Click);
                FrameworkElement.buttonReload.Click += new RoutedEventHandler(buttonReload_Click);

                FrameworkElement.labelSelectMissionTarget.Visibility = Visibility.Hidden;
                FrameworkElement.comboBoxSelectTarget.Visibility = Visibility.Hidden;
                FrameworkElement.checkBoxSelectCampaignFilter.Visibility = Visibility.Hidden;
                FrameworkElement.checkBoxSelectAirgroupFilter.Visibility = Visibility.Hidden;
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                _game = play as IGame;

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

                _game = null;
            }

            #region Event Handler

            #region ComboBox SelectionChanged

            private void comboBoxSelectCampaign_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    CampaignInfo campaignInfo = SelectedCampaign;
                    if (campaignInfo != null)
                    {
                        currentMissionFile = new MissionFile(Game, campaignInfo.InitialMissionTemplateFiles, campaignInfo.AirGroupInfos);
                    }
                    UpdateArmyComboBoxInfo(true);
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
                    UpdateSkillComboBoxSkillValueInfo();
                    UpdateFuelComboBoxInfo();
                }

                UpdateAircraftImage();
                UpdateButtonStatus();
            }

            private void comboBoxSelectSkill_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                // if (e.AddedItems.Count > 0)
                {
                    UpdateSkillComboBoxSkillValueInfo();
                }
            }

            private void UpdateSkillComboBoxSkillValueInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectSkill;
                ToolTip toolTip = comboBox.ToolTip as ToolTip;
                if (toolTip == null)
                {
                    comboBox.ToolTip = toolTip = new ToolTip();
                    toolTip.FontFamily = new System.Windows.Media.FontFamily("Consolas");
                }
                string str = string.Empty;
                Skill skill = SelectedSkill;
                if (skill != null)
                {
                    if (skill == Skill.Default)
                    {
                        AirGroup airGroup = SelectedAirGroup;
                        if (airGroup != null)
                        {
                            str = airGroup.Skills.Count > 0 ? string.Join("\n\n", airGroup.Skills.Values.Select(x => Skill.ToDetailString(x))) : airGroup.Skill != null ? Skill.ToDetailString(airGroup.Skill) : string.Empty;
                        }
                    }
                    else if (skill == Skill.Random)
                    {
                        str = "Randomly determined";
                    }
                    else
                    {
                        str = skill.ToDetailString();
                    }
                }
                toolTip.Content = str;
            }

            private void comboBoxSelectMissionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    UpdateFlightDefaultLabel();
                    UpdateSelectTargetComboBoxInfo();
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

            #endregion

            #region ComboBox IsEnabledChanged

            private void comboBoxSelectCampaign_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (Game != null && !(bool)e.NewValue)
                {
                    UpdateArmyComboBoxInfo(true);
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
                    UpdateSkillComboBoxSkillValueInfo();
                    UpdateFuelComboBoxInfo();
                }
            }

            private void comboBoxSelectMissionType_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (Game != null && !(bool)e.NewValue)
                {
                    UpdateSelectTargetComboBoxInfo();
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

            private void buttonImportMission_Click(object sender, RoutedEventArgs e)
            {
                if (MessageBox.Show("The system will convert and import existing mission files in the CloD folder for use in IL2DCE.\n" +
                    "The copyright of files converted by this process belongs to the original author, not you, and they cannot be distributed or shared without the consent of the original author.\n" +
                    "The converted files can only be used by you and on this PC.\n" +
                    "\nDo you agree to this ?",
                    "Confimation [IL2DCE]",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ProgressWindowModel model = new ProgressWindowModel();
                    ProgressWindow window = new ProgressWindow(model, BackgrowndWorkerEventHandler);
                    window.Title = "Mission file Conversion and Import in progress ... [IL2DCE]";
                    bool? result = window.ShowDialog();

                    object[] results = model.Result as object[];

                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0} to Import Missions", window.IsCanceled ? "Canceled" : "Completed");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendFormat("Total files: {0}", (int)results[0]);
                    sb.AppendLine();
                    sb.AppendFormat("  Completed: {0}", (int)results[1]);
                    sb.AppendLine();
                    if ((int)results[2] > 0)
                    {
                        sb.AppendFormat("  Error: {0}", (int)results[2]);
                        sb.AppendLine();
                    }
                    sb.AppendLine();
                    sb.AppendFormat("Log File: {0}", results[3] as string);
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("Do you want to open this log file ?");
                    if (MessageBox.Show(sb.ToString(), "Information [IL2DCE] Convert & Import", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        Process.Start(results[3] as string);
                    }

                    Game.Core.ReadCampaignInfo();
                    Game.Core.ReadCareerInfo();

                    Game.gameInterface.PageChange(new QuickMissionPage(), null);
                }
            }

            private void buttonReload_Click(object sender, RoutedEventArgs e)
            {
                if (MessageBox.Show("Your current selections will be lost.\nDo you want to reload this page ?", "Confimation [IL2DCE]",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Game.Core.ReadCampaignInfo();
                    Game.Core.ReadCareerInfo();

                    Game.gameInterface.PageChange(new QuickMissionPage(), null);
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
                GameIterface gameInterface = Game.gameInterface;
                string pilotName = gameInterface.Player().Name();

                int armyIndex = SelectedArmyIndex;
                int airForceIndex = SelectedAirForceIndex;
                int rankIndex = SelectedRank;
                AirGroup airGroup = SelectedAirGroup;
                CampaignInfo campaignInfo = SelectedCampaign;

                try
                {
                    Career career = new Career(pilotName, armyIndex, airForceIndex, rankIndex);
                    career.BattleType = EBattleType.QuickMission;
                    career.CampaignInfo = campaignInfo;
                    career.AirGroup = airGroup.ToString();
                    career.AirGroupDisplay = airGroup.VirtualAirGroupKey;
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
                    career.PlayerAirGroup = airGroup;
                    career.Aircraft = campaignInfo.GetAircraftInfo(airGroup.Class).DisplayName;
                    career.AdditionalAirOperations = FrameworkElement.GeneralSettingsGroupBox.SelectedAdditionalAirOperationsComboBox;
                    career.AdditionalGroundOperations = FrameworkElement.GeneralSettingsGroupBox.SelectedAdditionalGroundOperationsComboBox;
                    career.AdditionalAirGroups = FrameworkElement.GeneralSettingsGroupBox.SelectedAdditionalAirGroups;
                    career.SpawnRandomLocationFriendly = FrameworkElement.GeneralSettingsGroupBox.SelectedSpawnRandomLocationFriendly;
                    career.SpawnRandomLocationEnemy = FrameworkElement.GeneralSettingsGroupBox.SelectedSpawnRandomLocationEnemy;
                    career.SpawnRandomLocationPlayer = FrameworkElement.GeneralSettingsGroupBox.SelectedSpawnRandomLocationPlayer;
                    career.SpawnRandomAltitudeFriendly = FrameworkElement.GeneralSettingsGroupBox.SelectedSpawnRandomAltitudeFriendly;
                    career.SpawnRandomAltitudeEnemy = FrameworkElement.GeneralSettingsGroupBox.SelectedSpawnRandomAltitudeEnemy;
                    career.SpawnRandomTimeFriendly = FrameworkElement.GeneralSettingsGroupBox.SelectedSpawnRandomTimeFriendly;
                    career.SpawnRandomTimeEnemy = FrameworkElement.GeneralSettingsGroupBox.SelectedSpawnRandomTimeEnemy;
                    career.SpawnRandomTimeBeginSec = FrameworkElement.GeneralSettingsGroupBox.SelectedRandomTimeBeginComboBox;
                    career.SpawnRandomTimeEndSec = FrameworkElement.GeneralSettingsGroupBox.SelectedRandomTimeEndComboBox;

                    Game.Core.CurrentCareer = career;

                    campaignInfo.EndDate = campaignInfo.StartDate;
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

            #endregion

            #endregion

            #region Import and Convert Mission File

            private void BackgrowndWorkerEventHandler(object sender, BackgrowndWorkerEventArgs e)
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                DoWorkEventArgs args = e.Args;
                ProgressWindowModel model = args.Argument as ProgressWindowModel;

                MissionFileConverter converter = new MissionFileConverter(Game.gameInterface);
                Config config = Game.Core.Config;

#if DEBUG && false
                string destFolder = string.Format("{0}/{1}", Config.HomeFolder, "CampaignsImported");
#else
                string destFolder = Config.CampaignsFolderDefault;
#endif
                string logFileSystemPath = string.Empty;
                int files = 0;
                int count = 0;
                int error = 0;
                try
                {
                    IEnumerable<string> filesFileType = converter.GetFiles(config.SorceFolderFileName).Distinct().OrderBy(x => x);
                    IEnumerable<string> filesFolderType = converter.GetFiles(config.SorceFolderFolderName).Distinct().OrderBy(x => x);
                    files = filesFileType.Count() + filesFolderType.Count();

                    logFileSystemPath = CreateConvertLogFilePath();
                    using (FileStream stream = new FileStream(logFileSystemPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                        {
                            bool result;
                            string name;
                            worker.ReportProgress(-1, string.Format(CultureInfo.InvariantCulture, "{0}|{1}", 0, files));
                            foreach (var item in filesFileType)
                            {
                                worker.ReportProgress(count, string.Join("\n", converter.SplitTargetSystemPathInfo(item)));
                                if (worker.CancellationPending)
                                {
                                    args.Cancel = true;
                                    break;
                                }
                                name = Path.GetFileNameWithoutExtension(item.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar).LastOrDefault());
                                converter.ErrorMsg.Clear();
                                error += (result = converter.ConvertSystemPath(item, name, destFolder)) ? 0 : 1;
                                WriteConvertLog(writer, result, name, item, converter.ErrorMsg);
                                count++;
                            }
                            foreach (var item in filesFolderType)
                            {
                                worker.ReportProgress(count, string.Join("\n", converter.SplitTargetSystemPathInfo(item)));
                                if (worker.CancellationPending)
                                {
                                    args.Cancel = true;
                                    break;
                                }
                                string[] str = item.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
                                if (str.Length >= 2)
                                {
                                    name = string.Format("{0}_{1}", str[str.Length - 2], Path.GetFileNameWithoutExtension(str[str.Length - 1]));
                                    converter.ErrorMsg.Clear();
                                    error += (result = converter.ConvertSystemPath(item, name, destFolder)) ? 0 : 1;
                                    WriteConvertLog(writer, result, name, item, converter.ErrorMsg);
                                }
                                count++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = string.Format("{0} - {1} {2} {3}", "QuickMissionPage.buttonImportMissi_Click", ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                    Core.WriteLog(message);
                    MessageBox.Show(string.Format("{0}", ex.Message), Config.AppName, MessageBoxButton.OK, MessageBoxImage.Stop);
                }

                model.Result = new object[] { files, converter.CovertedMission.Count, error, logFileSystemPath, };
            }

            private void WriteConvertLog(StreamWriter writer, bool result, string name, string path, List<string> errorMsg)
            {
                // Result,Name(ID),FilePath,Error
                char[] del = new char[] { '\n' };
                const string ResultsSuccess = "Success";
                const string ResultsFail = "Fail";
                string error = string.Join("|", errorMsg.Select(x => x.Replace("\"", "\"\"").TrimEnd(del)));
                writer.WriteLine("\"{0}\",\"{1}\",\"{2}\",\"{3}\"", result ? ResultsSuccess : ResultsFail, name, path, error);
            }

            private string CreateConvertLogFilePath()
            {
                const int MaxErrorCount = 10;
                string logFileSystemPath = string.Empty;
                int i = 0;
                do
                {
                    string logFilePath = string.Format("{0}/{1}", Config.UserMissionsFolder, i == 0 ? Config.ConvertLogFileName :
                                            Path.GetFileNameWithoutExtension(Config.ConvertLogFileName) + i + Path.GetExtension(Config.ConvertLogFileName));
                    logFileSystemPath = Game.gameInterface.ToFileSystemPath(logFilePath);
                }
                while (!FileUtil.IsFileWritable(logFileSystemPath) && i++ < MaxErrorCount);
                return logFileSystemPath;
            }

            #endregion

            #region ComboBox Cotrol

            private void UpdateCampaignComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectCampaign;
                comboBox.IsEditable = Game.Core.Config.EnableFilterSelectCampaign;

                comboBox.Items.Clear();

                foreach (CampaignInfo campaignInfo in Game.Core.CampaignInfos)
                {
                    comboBox.Items.Add(campaignInfo);
                }

                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateArmyComboBoxInfo(bool checkArmy = false)
            {
                CampaignInfo campaignInfo = SelectedCampaign;

                ComboBox comboBox = FrameworkElement.comboBoxSelectArmy;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                if (campaignInfo != null && (!checkArmy || currentMissionFile != null))
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
                CampaignInfo campaignInfo = SelectedCampaign;
                if (campaignInfo != null && armyIndex != -1 && (!checkAirForce || currentMissionFile != null))
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

            private void UpdateAirGroupComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectAirGroup;
                comboBox.IsEditable = Game.Core.Config.EnableFilterSelectAirGroup;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
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

                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            private string CreateAirGroupContent(AirGroup airGroup, CampaignInfo campaignInfo, AircraftInfo aircraftInfo = null)
            {
                if (aircraftInfo == null)
                {
                    aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                }
                // return string.Format("{0} ({1})", airGroup.DisplayName, aircraftInfo.DisplayName);
                return string.Format("{0} ({1}){2}", airGroup.DisplayName, aircraftInfo.DisplayName, airGroup.Airstart ? " [AIRSTART]" : string.Empty);
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
                FrameworkElement.labelDefaultAltitude.Content = defaultMissionAltitude >= 0 ? string.Format(Config.Culture, MissionDefaultFormat, defaultMissionAltitude) :
                                                                defaultMissionAltitude >= (int)ESpawn.Parked && defaultMissionAltitude <= (int)ESpawn.AirStart ?
                                                                        string.Format(Config.Culture, MissionDefaultFormat, ((ESpawn)defaultMissionAltitude).ToDescription()) : string.Empty;
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
                    CampaignInfo campaignInfo = SelectedCampaign;
                    AirGroup airGroup = SelectedAirGroup;
                    if (campaignInfo != null && currentMissionFile != null && airGroup != null)
                    {
                        AirGroupWaypoint way = airGroup.Waypoints.FirstOrDefault();
                        if (way != null)
                        {
                            speedMissonDefault = ((int)way.V);
                        }
                    }
                    EnableSelectItem(comboBox, selected, currentMissionFile == null);
                }
                FrameworkElement.labelDefaultSpeed.Content = speedMissonDefault >= 0 ? string.Format(Config.Culture, MissionDefaultFormat, speedMissonDefault) : string.Empty;
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
                CampaignInfo campaignInfo = SelectedCampaign;
                AirGroup airGroup = SelectedAirGroup;
                if (campaignInfo != null && currentMissionFile != null && airGroup != null)
                {
                    fuelMissionDefault = airGroup.Fuel;
                }
                FrameworkElement.labelDefaultFuel.Content = fuelMissionDefault >= 0 ? string.Format(Config.Culture, MissionDefaultFormat, airGroup.Fuel) : string.Empty;
                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            private void UpdateSkillComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectSkill;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;

                if (comboBox.Items.Count == 0)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = Skill.Default, Content = DefaultString });
                    comboBox.Items.Add(new ComboBoxItem() { Tag = Skill.Random, Content = RandomString });
#if false
                    for (Skill.ESystemType skill = Skill.ESystemType.Rookie; skill < Skill.ESystemType.Count; skill++)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = Skill.GetSystemType(skill), Content = skill.ToString() });
                    }
#else
                    Config config = Game.Core.Config;
                    config.Skills.ForEach(x => comboBox.Items.Add(new ComboBoxItem() { Tag = x, Content = x.Name }));
#endif
                }

                string defaultString = string.Empty;
                if (SelectedAirGroup != null)
                {
                    AirGroup airGroup = SelectedAirGroup;
                    Skill skill = string.IsNullOrEmpty(airGroup.Skill) ? null : Skill.Parse(airGroup.Skill);
                    defaultString = string.Format(Config.Culture, MissionDefaultFormat, airGroup.Skills != null && airGroup.Skills.Count > 0 ?
                                    Skill.SkillNameMulti : skill != null ? skill.IsTyped() ? skill.GetTypedName() : Skill.SkillNameCustom : string.Empty);
                }
                FrameworkElement.labelDefaultSkill.Content = defaultString;

                EnableSelectItem(comboBox, selected, SelectedAirGroup == null);
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
                    defaultString = time >= 0 ? string.Format(Config.Culture, MissionDefaultFormat, MissionTime.ToString(time)) : string.Empty;
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
                        string.Format(Config.Culture, MissionDefaultFormat, ((EWeather)weather).ToDescription()) : string.Empty;
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
                    defaultString = alt >= 0 ? string.Format(Config.Culture, MissionDefaultFormat, CloudAltitude.CreateDisplayString(alt)) : string.Empty;
                }
                FrameworkElement.labelDefaultCloudAltitude.Content = defaultString;

                EnableSelectItem(comboBox, selected, currentMissionFile == null);
            }

            private void UpdateSelectTargetComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectTarget;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                CampaignInfo campaignInfo = SelectedCampaign;
                AirGroup airGroup = SelectedAirGroup;
                EMissionType? missionType = SelectedMissionType;
                if (campaignInfo != null && currentMissionFile != null && airGroup != null && missionType != null)
                {
                    //AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                    //AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                    //foreach (var item in aircraftInfo.MissionTypes)
                    //{
                    //    comboBox.Items.Add(item.ToDescription());
                    //}

                    switch (missionType.Value)
                    {
                        case EMissionType.RECON:
                            break;

                        case EMissionType.MARITIME_RECON:
                            break;

                        case EMissionType.ARMED_RECON:
                            break;

                        case EMissionType.ARMED_MARITIME_RECON:
                            break;

                        case EMissionType.ATTACK_ARMOR:
                            break;

                        case EMissionType.ATTACK_VEHICLE:
                            break;

                        case EMissionType.ATTACK_TRAIN:
                            break;

                        case EMissionType.ATTACK_SHIP:
                            break;

                        case EMissionType.ATTACK_ARTILLERY:
                            break;

                        case EMissionType.ATTACK_RADAR:
                            break;

                        case EMissionType.ATTACK_AIRCRAFT:
                            break;

                        case EMissionType.ATTACK_DEPOT:
                            break;

                        case EMissionType.INTERCEPT:
                            break;

                        case EMissionType.ESCORT:
                            break;

                        case EMissionType.COVER:
                            break;

                        default:
                            break;
                    }
                }

                EnableSelectItem(comboBox, selected);
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

            #endregion

            private void UpdateButtonStatus()
            {
                int addGroundOpe = FrameworkElement.GeneralSettingsGroupBox.SelectedAdditionalGroundOperationsComboBox;
                int timeBegin = FrameworkElement.GeneralSettingsGroupBox.SelectedRandomTimeBeginComboBox;
                int timeEnd = FrameworkElement.GeneralSettingsGroupBox.SelectedRandomTimeEndComboBox;
                bool timeEnable = FrameworkElement.GeneralSettingsGroupBox.SelectedSpawnRandomTimeEnemy || FrameworkElement.GeneralSettingsGroupBox.SelectedSpawnRandomTimeFriendly;
                FrameworkElement.Start.IsEnabled = SelectedArmyIndex != -1 && SelectedAirForceIndex != -1 && SelectedCampaign != null &&
                                                    SelectedAirGroup != null && SelectedRank != -1 &&
                                                    addGroundOpe >= Config.MinAdditionalGroundOperations && addGroundOpe <= Config.MaxAdditionalGroundOperations &&
                                                    (!timeEnable || timeEnable && timeBegin >= SpawnTime.MinimumBeginSec && timeEnd <= SpawnTime.MaximumEndSec && timeBegin <= timeEnd);
            }

            private void SelectLastInfo(Career career)
            {
                FrameworkElement.comboBoxSelectCampaign.SelectedItem = career.CampaignInfo;
                EnableSelectItem(FrameworkElement.comboBoxSelectArmy, ((EArmy)career.ArmyIndex).ToString());
                EnableSelectItem(FrameworkElement.comboBoxSelectAirForce, ((EArmy)career.ArmyIndex) == EArmy.Red ? ((EAirForceRed)career.AirForceIndex).ToDescription() : ((EAirForceBlue)career.AirForceIndex).ToDescription());
                FrameworkElement.comboBoxSelectRank.SelectedIndex = career.RankIndex;
                EnableSelectItem(FrameworkElement.comboBoxSelectAirGroup, CreateAirGroupContent(career.PlayerAirGroup, career.CampaignInfo));
                EnableSelectItem(FrameworkElement.comboBoxSelectMissionType, career.MissionType != null ? career.MissionType.ToDescription() : string.Empty);
                EnableSelectItem(FrameworkElement.comboBoxSelectFlight, Flight.CreateDisplayString(career.Flight));
                EnableSelectItem(FrameworkElement.comboBoxSelectFormation, career.Formation.ToDescription());
                EnableSelectItem(FrameworkElement.comboBoxSpawn, Spawn.CreateDisplayString(career.Spawn));
                EnableSelectItem(FrameworkElement.comboBoxSpeed, career.Speed.ToString());
                EnableSelectItem(FrameworkElement.comboBoxFuel, career.Fuel.ToString());
                EnableSelectItem(FrameworkElement.comboBoxSelectSkill, career.PlayerAirGroupSkill != null && career.PlayerAirGroupSkill.Length > 0 ? career.PlayerAirGroupSkill.First().Name : string.Empty);
                EnableSelectItem(FrameworkElement.comboBoxSelectTime, career.Time >= 0 ? MissionTime.ToString(career.Time) : string.Empty);
                EnableSelectItem(FrameworkElement.comboBoxSelectWeather, (int)career.Weather >= 0 ? ((EWeather)career.Weather).ToDescription() : string.Empty);
                EnableSelectItem(FrameworkElement.comboBoxSelectCloudAltitude, career.CloudAltitude >= 0 ? career.CloudAltitude.ToString() : string.Empty);
                EnableSelectItem(FrameworkElement.GeneralSettingsGroupBox.comboBoxSelectAdditionalAirOperations, career.AdditionalAirOperations >= 0 ? career.AdditionalAirOperations.ToString() : string.Empty);
                EnableSelectItem(FrameworkElement.GeneralSettingsGroupBox.comboBoxSelectAdditionalGroundOperations, career.AdditionalGroundOperations >= 0 ? career.AdditionalGroundOperations.ToString() : string.Empty);
                FrameworkElement.GeneralSettingsGroupBox.checkBoxAdditionalAirgroups.IsChecked = career.AdditionalAirGroups;
                FrameworkElement.GeneralSettingsGroupBox.checkBoxSpawnRandomLocationFriendly.IsChecked = career.SpawnRandomLocationFriendly;
                FrameworkElement.GeneralSettingsGroupBox.checkBoxSpawnRandomLocationEnemy.IsChecked = career.SpawnRandomLocationEnemy;
                FrameworkElement.GeneralSettingsGroupBox.checkBoxSpawnRandomLocationPlayer.IsChecked = career.SpawnRandomLocationPlayer;
                FrameworkElement.GeneralSettingsGroupBox.checkBoxSpawnRandomAltitudeFriendly.IsChecked = career.SpawnRandomAltitudeFriendly;
                FrameworkElement.GeneralSettingsGroupBox.checkBoxSpawnRandomAltitudeEnemy.IsChecked = career.SpawnRandomAltitudeEnemy;
                EnableSelectItem(FrameworkElement.GeneralSettingsGroupBox.comboBoxSelectRandomTimeBegin, career.SpawnRandomTimeBeginSec >= 0 ? career.SpawnRandomTimeBeginSec.ToString() : string.Empty);
                EnableSelectItem(FrameworkElement.GeneralSettingsGroupBox.comboBoxSelectRandomTimeEnd, career.SpawnRandomTimeEndSec >= 0 ? career.SpawnRandomTimeEndSec.ToString() : string.Empty);
                FrameworkElement.GeneralSettingsGroupBox.checkBoxSpawnRandomTimeFriendly.IsChecked = career.SpawnRandomTimeFriendly;
                FrameworkElement.GeneralSettingsGroupBox.checkBoxSpawnRandomTimeEnemy.IsChecked = career.SpawnRandomTimeEnemy;
            }

            private void UpdateAircraftImage()
            {
                AirGroup airGroup = SelectedAirGroup;
                FrameworkElement.borderImage.DisplayImage(Game.gameInterface, airGroup != null ? airGroup.Class : string.Empty);
            }
        }
    }
}