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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using IL2DCE.Pages.Controls;
using maddox.game;
using maddox.game.play;
using maddox.GP;
using static IL2DCE.MissionObjectModel.Spawn;

namespace IL2DCE.Pages
{
    public class CareerIntroPage : PageDefImpl
    {
        #region Definition

        private const string MissionDefaultFormat = "Mission Default: {0}";

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

        #endregion

        #region Variable

        private bool hookComboSelectionChanged = false;

        private MissionFile currentMissionFile = null;
        private bool missionLoaded = false;

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
            FrameworkElement.buttonMissionLoad.Click += new RoutedEventHandler(buttonMissionLoad_Click);
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
            UpdateProgressComboBoxInfo();
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
            missionLoaded = false;

            UpdateDataPicker();
            UpdateAirGroupComboBoxInfo();
            UpdateButtonStatus();
        }

        private void comboBoxSelectAirGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSkillComboBoxInfo();
            UpdateSkillComboBoxSkillValueInfo();
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
            FrameworkElement.GeneralSettingsGroupBox.checkBoxSpawnRandomLocationPlayer.Visibility 
                = SelectedProgress == ECampaignProgress.AnyTime || SelectedProgress == ECampaignProgress.AnyDayAnyTime ? Visibility.Hidden: Visibility.Visible;
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
                if (IsErrorPilotName = Game.Core.AvailableCareers.Any(x => string.Compare(x.PilotName, pilotName) == 0))
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

        #region Button Click

        private void buttonMissionLoad_Click(object sender, RoutedEventArgs e)
        {
            CampaignInfo campaignInfo = SelectedCampaign;
            if (campaignInfo != null)
            {
                GameIterface gameIterface = Game.gameInterface;
                if (gameIterface.BattleIsRun())
                {
                    gameIterface.BattleStop();
                }
                try
                {
                    gameIterface.AppPartsLoad(gameIterface.AppParts().Where(x => !gameIterface.AppPartIsLoaded(x)).ToList());
                    gameIterface.MissionLoad(campaignInfo.StaticTemplateFiles.First());
                    missionLoaded = true;
                    UpdateAirGroupComboBoxContent();
                    gameIterface.BattleStart();
                    gameIterface.BattleStop();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

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

            try
            {
                Career career = new Career(pilotName, armyIndex, airForceIndex, rankIndex);
                career.BattleType = EBattleType.Campaign;
                career.CampaignInfo = campaign;
                career.AirGroup = airGroup.ToString();
                career.AirGroupDisplay = airGroup.VirtualAirGroupKey;
                AircraftInfo aircraftInfo = career.CampaignInfo.GetAircraftInfo(airGroup.Class);
                career.Aircraft = aircraftInfo.DisplayName;
                career.PlayerAirGroupSkill = SelectedSkill != null && SelectedSkill != Skill.Random ? new Skill[] { SelectedSkill } : null;
                campaign.StartDate = FrameworkElement.datePickerStart.SelectedDate.Value;
                campaign.EndDate = FrameworkElement.datePickerEnd.SelectedDate.Value;
                career.AdditionalAirOperations = generalSettings.SelectedAdditionalAirOperations;
                career.AdditionalGroundOperations = generalSettings.SelectedAdditionalGroundOperations;
                career.AdditionalAirGroups = generalSettings.SelectedAdditionalAirGroups;
                career.SpawnRandomLocationFriendly = generalSettings.SelectedSpawnRandomLocationFriendly;
                career.SpawnRandomLocationEnemy = generalSettings.SelectedSpawnRandomLocationEnemy;
                career.SpawnRandomLocationPlayer = generalSettings.checkBoxSpawnRandomLocationPlayer.Visibility == Visibility.Visible ? generalSettings.SelectedSpawnRandomLocationPlayer: false;
                career.SpawnRandomAltitudeFriendly = generalSettings.SelectedSpawnRandomAltitudeFriendly;
                career.SpawnRandomAltitudeEnemy = generalSettings.SelectedSpawnRandomAltitudeEnemy;
                career.SpawnRandomTimeFriendly = generalSettings.SelectedSpawnRandomTimeFriendly;
                career.SpawnRandomTimeEnemy = generalSettings.SelectedSpawnRandomTimeEnemy;
                career.SpawnRandomTimeBeginSec = generalSettings.SelectedRandomTimeBegin;
                career.SpawnRandomTimeEndSec = generalSettings.SelectedRandomTimeEnd;
                career.ReArmTime = generalSettings.SelectedAutoReArm ? config.ProcessTimeReArm : -1;
                career.ReFuelTime = generalSettings.SelectedAutoReFuel ? config.ProcessTimeReFuel : -1;
                career.TrackRecording = generalSettings.SelectedTrackRecoding;
                career.AISkill = generalSettings.SelecteAISkill;
                career.CampaignProgress = SelectedProgress;

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

        private void UpdateAirGroupComboBoxContent()
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

        private void UpdateSkillComboBoxSkillValueInfo()
        {
            ComboBox comboBox = FrameworkElement.comboBoxSelectSkill;
            ToolTip toolTip = comboBox.ToolTip as ToolTip;
            if (toolTip == null)
            {
                comboBox.ToolTip = toolTip = new ToolTip();
                toolTip.FontFamily = new FontFamily("Consolas");
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

        private void UpdateDataPicker()
        {
            CampaignInfo campaignInfo = SelectedCampaign;
            if (campaignInfo != null)
            {
                FrameworkElement.datePickerStart.SelectedDate = campaignInfo.StartDate;
                FrameworkElement.datePickerEnd.SelectedDate = campaignInfo.EndDate;
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
            FrameworkElement.Start.IsEnabled = SelectedArmyIndex != -1 && SelectedAirForceIndex != -1 && SelectedRank != -1 && 
                                                !IsErrorPilotName/*!Game.Core.AvailableCareers.Any(x => string.Compare(x.PilotName, pilotName) == 0)*/ &&
                                                SelectedCampaign != null && SelectedAirGroup != null &&
                                                datePickerStart.SelectedDate.HasValue && datePickerEnd.SelectedDate.HasValue &&
                                                datePickerStart.SelectedDate.Value <= datePickerEnd.SelectedDate.Value &&
                                                (datePickerEnd.SelectedDate.Value - datePickerStart.SelectedDate.Value).TotalDays <= CampaignInfo.MaxCampaignPeriod &&
                                                addGroundOpe >= Config.MinAdditionalGroundOperations && addGroundOpe <= Config.MaxAdditionalGroundOperations &&
                                                (!timeEnable || timeEnable && timeBegin >= SpawnTime.MinimumBeginSec && timeEnd <= SpawnTime.MaximumEndSec && timeBegin <= timeEnd);
        }
    }
}