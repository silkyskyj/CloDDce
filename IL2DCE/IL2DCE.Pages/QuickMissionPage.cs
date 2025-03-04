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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using IL2DCE.Pages.Controls;
using IL2DCE.Pages.Input;
using IL2DCE.Util;
using maddox.game;
using maddox.game.play;
using static IL2DCE.Pages.Controls.ProgressWindow;

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
                    ComboBoxItem selected =  FrameworkElement.comboBoxSelectAirGroup.SelectedItem as ComboBoxItem;
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

            private MissionFile CurrentMissionFile = null;

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
                FrameworkElement.comboBoxSelectMissionType.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectMissionType_SelectionChanged);
                FrameworkElement.comboBoxSpawn.SelectionChanged += new SelectionChangedEventHandler(comboBoxSpawn_SelectionChanged);

                FrameworkElement.checkBoxSelectCampaignFilter.Checked += new RoutedEventHandler(checkBoxSelectCampaignFilter_CheckedChange);
                FrameworkElement.checkBoxSelectCampaignFilter.Unchecked += new RoutedEventHandler(checkBoxSelectCampaignFilter_CheckedChange);
                FrameworkElement.checkBoxSelectAirgroupFilter.Checked += new RoutedEventHandler(checkBoxSelectAirgroupFilter_CheckedChange);
                FrameworkElement.checkBoxSelectAirgroupFilter.Unchecked += new RoutedEventHandler(checkBoxSelectAirgroupFilter_CheckedChange);

                FrameworkElement.buttonImportMission.Click += new RoutedEventHandler(buttonImportMissi_Click);

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
                    UpdateNoRelatedComboBoxInfo();
                    hookComboSelectionChanged = false;
                    SelectLastInfo(Game.Core.CurrentCareer);
                }
                else
                {
                    UpdateCampaignComboBoxInfo();
                    UpdateNoRelatedComboBoxInfo();
                }

                UpdateButtonStatus();
            }

            private void UpdateNoRelatedComboBoxInfo()
            {
                UpdateSkillComboBoxInfo();
                UpdateTimeComboBoxInfo();
                UpdateWeatherComboBoxInfo();
                UpdateCloudAltitudeComboBoxInfo();
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            #region Event Handler

            private void comboBoxSelectCampaign_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    CampaignInfo campaignInfo = SelectedCampaign;
                    if (campaignInfo != null)
                    {
                        CurrentMissionFile = new MissionFile(Game, campaignInfo.InitialMissionTemplateFiles, campaignInfo.AirGroupInfos);
                    }
                    UpdateArmyComboBoxInfo(true);

                    if (Game.Core.Config.EnableAutoSelectComboBoxItem)
                    {
                        UpdateComboBoxSelectInfoAirForce();
                        UpdateComboBoxSelectInfoArmy();
                    }
                }

                UpdateButtonStatus();
            }

            private void comboBoxSelectArmy_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    UpdateAirForceComboBoxInfo(true);

                    if (Game.Core.Config.EnableAutoSelectComboBoxItem)
                    {
                        UpdateComboBoxSelectInfoAirForce();
                    }
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
                    UpdateFuelComboBoxInfo();
                }

                UpdateAircraftImage();
                UpdateButtonStatus();
            }

            private void comboBoxSelectMissionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
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

            private void checkBoxSelectCampaignFilter_CheckedChange(object sender, RoutedEventArgs e)
            {
                FrameworkElement.comboBoxSelectCampaign.IsEditable = FrameworkElement.checkBoxSelectCampaignFilter.IsChecked != null && FrameworkElement.checkBoxSelectCampaignFilter.IsChecked.Value;
            }

            private void checkBoxSelectAirgroupFilter_CheckedChange(object sender, RoutedEventArgs e)
            {
                FrameworkElement.comboBoxSelectAirGroup.IsEditable = FrameworkElement.checkBoxSelectAirgroupFilter.IsChecked!= null && FrameworkElement.checkBoxSelectAirgroupFilter.IsChecked.Value;
            }

            private void buttonImportMissi_Click(object sender, RoutedEventArgs e)
            {
                if (MessageBox.Show("The system will convert and import existing mission files in the CloD folder for use in IL2DCE.\n" + 
                    "The copyright of files coverted by this process belongs to the original author, not you, and they cannot be distributed or shared without the consent of the original author.\n" + 
                    "\n\nDo you agree to this ?", 
                    "Confimation", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ProgressWindowModel model = new ProgressWindowModel();
                    ProgressWindow window = new ProgressWindow(model, BackgrowndWorkerEventHandler);
                    window.Title = "Mission file Conversion and Import in progress ... [IL2DCE]";
                    bool? result =  window.ShowDialog();

                    MissionFileConverter converter = model.Result as MissionFileConverter;

                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0} to Import Missions", window.IsCanceled ? "Canceled": "Completed");
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendFormat("Completed files: {0}", converter.CovertedMission.Count);
                    sb.AppendLine();
                    if (converter.ErrorMsg.Count > 0)
                    {
                        sb.AppendFormat("Error files: {0}", converter.ErrorMsg.Count);
                        sb.AppendLine();
                        // sb.AppendLine(string.Join(", ", converter.ErrorMsg));
                    }
                    MessageBox.Show(sb.ToString(), "Information [IL2DCE] Convert & Import", MessageBoxButton.OK, MessageBoxImage.Information);

                    Game.Core.ReadCampaignInfo();
                    Game.Core.ReadCareerInfo();

                    Game.gameInterface.PageChange(new QuickMissionPage(), null);
                }
            }

            private void BackgrowndWorkerEventHandler(object sender, BackgrowndWorkerEventArgs e)
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                DoWorkEventArgs args = e.Args;
                ProgressWindowModel model = args.Argument as ProgressWindowModel;

                MissionFileConverter converter = new MissionFileConverter(Game.gameInterface);
                model.Result = converter;
                Config config = Game.Core.Config;

#if DEBUG && false
                string destFolder = string.Format("{0}/{1}", Config.HomeFolder, "CampaignsImported");
#else
                string destFolder = Config.CampaignsFolderDefault;
#endif
                try
                {
                    IEnumerable<string> filesFileType = converter.GetFiles(config.SorceFolderFileName).OrderBy(x => x);
                    IEnumerable<string> filesFolderType = converter.GetFiles(config.SorceFolderFolderName).OrderBy(x => x);
                    int files = filesFileType.Count() + filesFolderType.Count();

                    //model.Minimum = 0;
                    //model.Maximum = files;
                    //model.Value = 0;
                    //model.Title = "Importing Missions ...";

                    int count = 0;
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
                        converter.ConvertSystemPath(item, name, destFolder);
                        count++;
                        //model.Value++;
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
                            converter.ConvertSystemPath(item, name, destFolder);
                        }
                        count++;
                        //model.Value++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0}, {1}, {2}, {3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                    MessageBox.Show(string.Format("{0}", ex.Message), "IL2DCE", MessageBoxButton.OK, MessageBoxImage.Stop);
                    Game.gameInterface.LogErrorToConsole(string.Format("{0} - {1}", "QuickMissionPage.buttonImportMissi_Click", ex.Message));
                }
                finally
                {
                    ;
                }

                args.Result = converter;
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
                    career.MissionType = SelectedMissionType;
                    career.Spawn = SelectedSpawn;
                    career.Fuel = SelectedFuel;
                    career.Speed = SelectedSpeed;
                    career.PlayerAirGroupSkill = SelectedSkill;
                    career.Time = SelectedTime;
                    career.Weather = SelectedWeather;
                    career.CloudAltitude = SelectedCloudAltitude;
                    career.PlayerAirGroup = airGroup;
                    career.Aircraft = campaignInfo.GetAircraftInfo(airGroup.Class).DisplayName;

                    Game.Core.CurrentCareer = career;

                    campaignInfo.EndDate = campaignInfo.StartDate;
                    Game.Core.CreateQuickMission(Game, career);

                    Game.gameInterface.PageChange(new BattleIntroPage(), null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0}", ex.Message), "IL2DCE", MessageBoxButton.OK, MessageBoxImage.Error);
                    gameInterface.LogErrorToConsole(string.Format("{0} - {1}", "QuickMissionPage.Start_Click", ex.Message));
                }
            }

            private void Loadout_Click(object sender, RoutedEventArgs e)
            {
                Game.gameInterface.PagePush(new PlaneLoadoutPage(), SelectedAirGroup.Class);
            }

#endregion

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
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string: string.Empty;
                comboBox.Items.Clear();

                if (campaignInfo != null && (!checkArmy || CurrentMissionFile != null))
                {
                    var armys = checkArmy ? CurrentMissionFile.AirGroups.Select(x => x.ArmyIndex).Distinct(): new int [0];
                    for (int i = 0; i < (int)EArmy.Count; i++)
                    {
                        if (!checkArmy || armys.Contains(i + 1))
                        {
                            comboBox.Items.Add(new ComboBoxItem() { Tag = i + 1, Content = Career.Army[i] });
                        }
                    }
                }

                EnableSelectItem(comboBox, selected);
            }

            private void UpdateAirForceComboBoxInfo(bool checkAirForce = false)
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectAirForce;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                int armyIndex = SelectedArmyIndex;
                CampaignInfo campaignInfo = SelectedCampaign;
                if (campaignInfo != null && armyIndex != -1 && (!checkAirForce || CurrentMissionFile != null))
                {
                    var airForces = checkAirForce ? CurrentMissionFile.AirGroups.Where(x => x.ArmyIndex == armyIndex).Select(x => x.AirGroupInfo.AirForceIndex).Distinct(): new int [0];
                    if (armyIndex == (int)EArmy.Red)
                    {
                        for (int i = 0; i < (int)AirForceRed.Count; i++)
                        {
                            if (!checkAirForce || airForces.Contains(i + 1))
                            {
                                comboBox.Items.Add(new ComboBoxItem() { Tag = i + 1, Content = Career.AirForce[i] });
                            }
                        }
                    }
                    else if (armyIndex == (int)EArmy.Blue)
                    {
                        int diff = (int)AirForceRed.Count;
                        for (int i = 0; i < (int)AirForceBlue.Count; i++)
                        {
                            if (!checkAirForce || airForces.Contains(i + 1))
                            {
                                comboBox.Items.Add(new ComboBoxItem() { Tag = i + 1, Content = Career.AirForce[i + diff] });
                            }
                        }
                    }
                }

                EnableSelectItem(comboBox, selected);
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
                    int airforce = (armyIndex - 1) * 3 + airForceIndex - 1;

                    for (int i = 0; i <= Career.RankMax; i++)
                    {
                        comboBox.Items.Add(
                            new ComboBoxItem()
                            {
                                Content = Career.Rank[airforce][i],
                                Tag = i,
                            });
                    }
                }

                if (comboBox.Items.Count > 0)
                {
                    comboBox.IsEnabled = true;
                    comboBox.SelectedIndex = selected != -1 ? selected: 0;
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
                if (campaignInfo != null && CurrentMissionFile != null)
                {
                    int armyIndex = SelectedArmyIndex;
                    int airForceIndex = SelectedAirForceIndex;
                    if (armyIndex != -1 && airForceIndex != -1)
                    {
                        foreach (AirGroup airGroup in CurrentMissionFile.AirGroups.OrderBy(x => x.Class))
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

                EnableSelectItem(comboBox, selected);
            }

            private string CreateAirGroupContent(AirGroup airGroup, CampaignInfo campaignInfo, AircraftInfo aircraftInfo = null)
            {
                if (aircraftInfo == null)
                {
                    aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                }
                return string.Format("{0} ({1})", airGroup.DisplayName, aircraftInfo.DisplayName);
            }

            private void UpdateMissionTypeComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectMissionType;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                CampaignInfo campaignInfo = SelectedCampaign;
                AirGroup airGroup = SelectedAirGroup;
                if (campaignInfo != null && CurrentMissionFile != null && airGroup != null)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = RandomString });
                    // AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
                    AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                    foreach (var item in aircraftInfo.MissionTypes)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = item, Content = item.ToDescription() });
                    }
                }

                EnableSelectItem(comboBox, selected);
            }

            private void UpdateSpawnComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSpawn;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                CampaignInfo campaignInfo = SelectedCampaign;
                AirGroup airGroup = SelectedAirGroup;
                AircraftInfo aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
                EMissionType? missionType = SelectedMissionType;
                if (campaignInfo != null && CurrentMissionFile != null && airGroup != null && aircraftInfo != null)
                {
                    AircraftParametersInfo aircraftParam = missionType != null ? aircraftInfo.GetAircraftParametersInfo(missionType.Value).FirstOrDefault(): null;
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
                            comboBox.Items.Add(new ComboBoxItem() { Tag = i, Content = Spawn.CreateDisplayName(i) });
                        }
#if false
                        if (string.IsNullOrEmpty(selected) || Game.Core.Config.EnableAutoSelectComboBoxItem)
                        {
                            selected = airGroup.SetOnParked ? 
                                            ESpawn.Parked.ToDescription() : way.Type == AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF ? 
                                                    ESpawn.Idle.ToDescription() : ((((int)way.Z) / Spawn.SelectStepAltitude) * Spawn.SelectStepAltitude).ToString(CultureInfo.InvariantCulture.NumberFormat);
                        }
#endif
                        defaultMissionAltitude = (int)way.Z;

                        FrameworkElement.labelDefaultAltitude.Content = string.Format(Config.Culture, MissionDefaultFormat, defaultMissionAltitude);
                    }
                }

                EnableSelectItem(comboBox, selected);
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

                int spawn = SelectedSpawn;
                if (spawn == (int)ESpawn.Parked || spawn == (int)ESpawn.Idle || ((spawn == (int)ESpawn.Default) && defaultMissionAltitude == 0))
                {
                    comboBox.SelectedIndex = 0;
                    comboBox.IsEnabled = false;
                }
                else
                {
                    comboBox.IsEnabled = true;
                    CampaignInfo campaignInfo = SelectedCampaign;
                    AirGroup airGroup = SelectedAirGroup;
                    if (campaignInfo != null && CurrentMissionFile != null && airGroup != null)
                    {
                        AirGroupWaypoint way = airGroup.Waypoints.FirstOrDefault();
                        if (way != null)
                        {
#if false
                            if (string.IsNullOrEmpty(selected) || Game.Core.Config.EnableAutoSelectComboBoxItem)
                            {
                                selected = Speed.CreateDisplayString(((int)way.V) != 0 ? (((int)way.V) / Speed.SelectStepSpeed) * Speed.SelectStepSpeed: AirGroupWaypoint.DefaultFlyV);
                                comboBox.Text = selected;
                            }
#endif

                            FrameworkElement.labelDefaultSpeed.Content = string.Format(Config.Culture, MissionDefaultFormat, ((int)way.V));
                        }
                    }
                    EnableSelectItem(comboBox, selected);
                }
            }

            private void UpdateFuelComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxFuel;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;

                if (comboBox.Items.Count == 0)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = -1, Content = DefaultString });
                    for (int i = Fuel.SelectMinFuel; i <= Fuel.SelectMaxFuel; i += Fuel.SelectStepFuel)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = i, Content = Fuel.CreateDisplayString(i) });
                    }
                }

                CampaignInfo campaignInfo = SelectedCampaign;
                AirGroup airGroup = SelectedAirGroup;
                if (campaignInfo != null && CurrentMissionFile != null && airGroup != null)
                {

#if false
                    if (string.IsNullOrEmpty(selected) || Game.Core.Config.EnableAutoSelectComboBoxItem)
                    {
                        selected = airGroup.Fuel != -1 ? Fuel.CreateDisplayString(((airGroup.Fuel / Fuel.SelectStepFuel) * Fuel.SelectStepFuel)): string.Empty;
                    }
#endif
                    FrameworkElement.labelDefaultFuel.Content = string.Format(Config.Culture, MissionDefaultFormat, airGroup.Fuel);
                }

                EnableSelectItem(comboBox, selected);
            }

            private void UpdateTimeComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectTime;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;

                if (comboBox.Items.Count == 0)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = RandomString });
                    for (double d = MissionTime.Begin; d <= MissionTime.End; d += 0.5)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = d, Content = MissionTime.ToString(d) });
                    }
                }

                EnableSelectItem(comboBox, selected);
            }

            private void UpdateSkillComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectSkill;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = RandomString });
                for (Skill.ESystemType skill = Skill.ESystemType.Rookie; skill < Skill.ESystemType.Count; skill++)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = Skill.GetSystemType(skill), Content = skill.ToString() });
                }

                EnableSelectItem(comboBox, selected);
            }

            private void UpdateWeatherComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectWeather;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;

                if (comboBox.Items.Count == 0)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = RandomString });
                    for (EWeather w = EWeather.Clear; w <= EWeather.MediumClouds; w++)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = w, Content = w.ToDescription() });
                    }
                }

                EnableSelectItem(comboBox, selected);
            }

            private void UpdateCloudAltitudeComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectCloudAltitude;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;

                if (comboBox.Items.Count == 0)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = RandomString });
                    for (int alt = (int)CloudAltitude.Min; alt <= CloudAltitude.Max; alt += CloudAltitude.Step)
                    {
                        comboBox.Items.Add(new ComboBoxItem() { Tag = alt, Content = CloudAltitude.CreateDisplayString(alt) });
                    }
                }

                EnableSelectItem(comboBox, selected);
            }

            private void UpdateSelectTargetComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectTarget;
                string selected = comboBox.SelectedItem != null ? (comboBox.SelectedItem as ComboBoxItem).Content as string : string.Empty;
                comboBox.Items.Clear();

                CampaignInfo campaignInfo = SelectedCampaign;
                AirGroup airGroup = SelectedAirGroup;
                EMissionType? missionType = SelectedMissionType;
                if (campaignInfo != null && CurrentMissionFile != null && airGroup != null && missionType != null)
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

            private void EnableSelectItem(ComboBox comboBox, string selected)
            {
                if (comboBox.Items.Count > 0)
                {
                    comboBox.IsEnabled = true;
                    comboBox.Text = selected;
                    if (comboBox.SelectedIndex == -1)
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
                FrameworkElement.Start.IsEnabled = SelectedArmyIndex != -1 && SelectedAirForceIndex != -1 && SelectedCampaign != null &&
                                                    SelectedAirGroup != null && SelectedRank != -1;
            }

            private void UpdateComboBoxSelectInfoAirForce()
            {
                int start = FrameworkElement.comboBoxSelectAirForce.SelectedIndex;
                if (FrameworkElement.comboBoxSelectAirGroup.Items.Count == 0)
                {
                    // AirForce
                    while (FrameworkElement.comboBoxSelectAirForce.SelectedIndex + 1 < FrameworkElement.comboBoxSelectAirForce.Items.Count
                            && FrameworkElement.comboBoxSelectAirGroup.Items.Count == 0)
                    {
                        FrameworkElement.comboBoxSelectAirForce.SelectedIndex++;
                        Thread.Sleep(1);
                    }
                    if (FrameworkElement.comboBoxSelectAirGroup.Items.Count == 0 && start > 0)
                    {
                        FrameworkElement.comboBoxSelectAirForce.SelectedIndex = start;
                        while (FrameworkElement.comboBoxSelectAirForce.SelectedIndex - 1 >= 0
                                && FrameworkElement.comboBoxSelectAirGroup.Items.Count == 0)
                        {
                            FrameworkElement.comboBoxSelectAirForce.SelectedIndex--;
                            Thread.Sleep(1);
                        }
                    }
                }
            }

            private void SelectLastInfo(Career career)
            {
                FrameworkElement.comboBoxSelectCampaign.SelectedItem = career.CampaignInfo;
                EnableSelectItem(FrameworkElement.comboBoxSelectArmy, Career.Army[career.ArmyIndex - 1]);
                EnableSelectItem(FrameworkElement.comboBoxSelectAirForce, Career.AirForce[(career.ArmyIndex - 1) * 3 + career.AirForceIndex - 1]);
                FrameworkElement.comboBoxSelectRank.SelectedIndex = career.RankIndex;
                EnableSelectItem(FrameworkElement.comboBoxSelectAirGroup, CreateAirGroupContent(career.PlayerAirGroup, career.CampaignInfo));
                EnableSelectItem(FrameworkElement.comboBoxSelectMissionType, career.MissionType != null ? career.MissionType.ToDescription() : string.Empty);
                EnableSelectItem(FrameworkElement.comboBoxSpawn, Spawn.CreateDisplayName(career.Spawn));
                EnableSelectItem(FrameworkElement.comboBoxSelectSkill, career.PlayerAirGroupSkill != null ? career.PlayerAirGroupSkill.Name : string.Empty);
                EnableSelectItem(FrameworkElement.comboBoxSelectTime, career.Time != -1 ? MissionTime.ToString(career.Time) : string.Empty);
                EnableSelectItem(FrameworkElement.comboBoxSelectWeather, (int)career.Weather != -1 ? ((EWeather)career.Weather).ToDescription() : string.Empty);
                EnableSelectItem(FrameworkElement.comboBoxSelectCloudAltitude, career.CloudAltitude != -1 ? career.CloudAltitude.ToString() : string.Empty);
            }

            private void UpdateComboBoxSelectInfoArmy()
            {
                // Army
                if (FrameworkElement.comboBoxSelectAirGroup.Items.Count == 0)
                {
                    if (FrameworkElement.comboBoxSelectArmy.SelectedIndex == 0)
                    {
                        FrameworkElement.comboBoxSelectArmy.SelectedIndex = 1;
                    }
                    else if (FrameworkElement.comboBoxSelectArmy.SelectedIndex == 1)
                    {
                        FrameworkElement.comboBoxSelectArmy.SelectedIndex = 0;
                    }
                }
            }

            private void UpdateAircraftImage()
            {
                AirGroup airGroup = SelectedAirGroup;
                DisplayAircraftImage(airGroup != null ? airGroup.Class : string.Empty);
            }

            private void DisplayAircraftImage(string aircraftClass)
            {
                string path;
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