// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC
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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using maddox.game.play;

namespace IL2DCE
{
    namespace Pages
    {
        public class SelectCareerPage : PageDefImpl
        {

            #region Definition

            private const string AllString = "[All]";
            private const string NoFileString = "[no file]";
            private const string NoSkillValueMessage = "(The value will be displayed after the next mission)";

            private enum ECareerSort
            {
                [Description("Item Asc")]
                DisplayAscending,

                [Description("Item Desc")]
                DisplayDescending,

                [Description("Campaign Asc")]
                CampaignAscending,

                [Description("Campaign Desc")]
                CampaignDescending,

                [Description("CampaignMode Asc")]
                CampaignModeAscending,

                [Description("CampaignMode Desc")]
                CampaignModeDescending,

                [Description("ArmyAirForce Asc")]
                ArmyAirForceAscending,

                [Description("ArmyAirForce Desc")]
                ArmyAirForceDescending,

                [Description("PilotName Asc")]
                PilotNameAscending,

                [Description("PilotName Desc")]
                PilotNameDescending,

                [Description("AirGroup Asc")]
                AirGroupAscending,

                [Description("AirGroup Desc")]
                AirGroupDescending,

                [Description("Aircraft Asc")]
                AircraftAscending,

                [Description("Aircraft Desc")]
                AircraftDescending,

                [Description("Date Asc")]
                DateAscending,

                [Description("Date Desc")]
                DateDescending,

                [Description("StartDate Asc")]
                StartDateAscending,

                [Description("StartDate Desc")]
                StartDateDescending,

                [Description("EndDate Asc")]
                EndDateAscending,

                [Description("EndDate Desc")]
                EndDateDescending,
                
                [Description("Rank Asc")]
                RankAscending,
                
                [Description("Rank Desc")]
                RankDescending,

                [Description("Experience Asc")]
                ExperienceAscending,

                [Description("Experience Desc")]
                ExperienceDescending,

                [Description("UpdateDateTime Asc")]
                UpdateDateTimeAscending,

                [Description("UpdateDateTime Desc")]
                UpdateDateTimeDescending,

                Count,
            }

            static readonly string [] CareerSortProperty = new string[]
                {
                    "DisplayString",
                    "CampaignString",
                    "CampaignMode",
                    "ArmyAirForce",
                    "PilotName",
                    "AirGroupDisplayString",
                    "Aircraft",
                    "Date",
                    "StartDate",
                    "EndDate",
                    "RankIndex",
                    "Experience",
                    "UpdateDateTime",
                    "",
                };

            #endregion

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

            #region Filter

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

            protected ECampaignMode? SelectedCampaignMode
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectCampaignMode.SelectedItem as ComboBoxItem;
                    if (selected != null && selected.Tag != null)
                    {
                        return (ECampaignMode)selected.Tag;
                    }

                    return null;
                }
            }

            private CampaignInfo SelectedCampaign
            {
                get
                {
                    return FrameworkElement.comboBoxSelectCampaign.SelectedItem as CampaignInfo;
                }
            }

            private string SelectedAircraft
            {
                get
                {
                    ComboBoxItem selected = FrameworkElement.comboBoxSelectAircraft.SelectedItem as ComboBoxItem;
                    if (selected != null)
                    {
                        return selected.Tag as string;
                    }

                    return string.Empty;
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

            public bool SelectedPlayable
            {
                get
                {
                    bool? isCheckd = FrameworkElement.checkBoxPlayable.IsChecked;
                    if (isCheckd != null)
                    {
                        return isCheckd.Value;
                    }

                    return false;
                }
            }

            #endregion

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

            private AirGroup SelecedtAirGroup;

            #endregion

            public SelectCareerPage()
                : base("Select Career", new SelectCareer())
            {
                FrameworkElement.comboBoxSelectArmy.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectArmy_SelectionChanged);
                FrameworkElement.comboBoxSelectAirForce.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAirForce_SelectionChanged);
                FrameworkElement.comboBoxSelectCampaignMode.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectCampaignMode_SelectionChanged);
                FrameworkElement.comboBoxSelectCampaign.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectCampaign_SelectionChanged);
                FrameworkElement.comboBoxSelectAircraft.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelectAircraft_SelectionChanged);
                FrameworkElement.ListCareer.SelectionChanged += new SelectionChangedEventHandler(ListCareerSelectionChanged);
                FrameworkElement.comboBoxCareerSort.SelectionChanged += new SelectionChangedEventHandler(ComboBoxCareerSort_SelectionChanged);

                FrameworkElement.Back.Click += new RoutedEventHandler(bBack_Click);
                FrameworkElement.checkBoxStrictMode.Checked += new RoutedEventHandler(checkBox_CheckedChange);
                FrameworkElement.checkBoxStrictMode.Unchecked += new RoutedEventHandler(checkBox_CheckedChange);
                FrameworkElement.checkBoxPlayable.Checked += new RoutedEventHandler(checkBox_CheckedChange);
                FrameworkElement.checkBoxPlayable.Unchecked += new RoutedEventHandler(checkBox_CheckedChange);

                FrameworkElement.New.Click += new RoutedEventHandler(bNew_Click);
                FrameworkElement.Delete.Click += new RoutedEventHandler(Delete_Click);
                FrameworkElement.Continue.Click += new RoutedEventHandler(bContinue_Click);
                FrameworkElement.buttonFilterClear.Click += new RoutedEventHandler(buttonFilterClear_Click);
                FrameworkElement.buttonReload.Click += new RoutedEventHandler(buttonReload_Click);

                FrameworkElement.Continue.IsEnabled = false;
                FrameworkElement.Delete.IsEnabled = false;
            }

            public override void _enter(maddox.game.IGame play, object arg)
            {
                base._enter(play, arg);

                _game = play as IGame;

                UpdateArmyComboBoxInfo();
                UpdateAirForceComboBoxInfo();
                UpdateCampaignModeComboBoxInfo();
                UpdateCampaignComboBoxInfo();
                UpdateAircraftComboBoxInfo();

                UpdateCareerList();
                UpdateCareerSortComboBoxInfo();
            }

            public override void _leave(maddox.game.IGame play, object arg)
            {
                base._leave(play, arg);

                _game = null;
            }

            #region Button Click

            private void bBack_Click(object sender, RoutedEventArgs e)
            {
                Game.gameInterface.PagePop(null);
            }

            private void bNew_Click(object sender, RoutedEventArgs e)
            {
                Game.gameInterface.PageChange(new CareerIntroPage(), 
                    new CareerIntroPage.PageArgs() { Army = SelectedArmyIndex, AirForce = SelectedAirForceIndex });
            }

            private void bContinue_Click(object sender, RoutedEventArgs e)
            {
                Career career = SelectedCareer;
                if (career != null && career.CampaignInfo != null)
                {
                    AirGroup airGroup = SelecedtAirGroup;
                    career.BattleType = EBattleType.Campaign;
                    career.PlayerAirGroup = airGroup;
                    career.AirGroupDisplay = airGroup.VirtualAirGroupKey;
                    career.Aircraft = AircraftInfo.CreateDisplayName(SelecedtAirGroup.Class);
                    career.UpdatePlayerAirGroupSkill();
                    Game.Core.CurrentCareer = career;
                    Game.Core.InitCampaign();
                    Game.gameInterface.PageChange(new BattleIntroPage(), null);
                }
                else
                {
                    Debug.Assert(false);
                    Game.gameInterface.PageChange(new SelectCampaignPage(), null);
                }
            }

            void Delete_Click(object sender, RoutedEventArgs e)
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
                    UpdateAircraftComboBoxInfo();
                }
            }

            private void buttonFilterClear_Click(object sender, RoutedEventArgs e)
            {
                FrameworkElement.comboBoxSelectArmy.SelectedIndex = FrameworkElement.comboBoxSelectArmy.Items.Count > 0 ? 0 : -1;
                FrameworkElement.comboBoxSelectAirForce.SelectedIndex = FrameworkElement.comboBoxSelectAirForce.Items.Count > 0 ? 0 : -1;
                FrameworkElement.comboBoxSelectCampaign.SelectedIndex = FrameworkElement.comboBoxSelectCampaign.Items.Count > 0 ? 0 : -1;
                FrameworkElement.comboBoxSelectCampaignMode.SelectedIndex = FrameworkElement.comboBoxSelectCampaignMode.Items.Count > 0 ? 0 : -1;
                FrameworkElement.comboBoxSelectAircraft.SelectedIndex = FrameworkElement.comboBoxSelectAircraft.Items.Count > 0 ? 0 : -1;
                FrameworkElement.checkBoxPlayable.IsChecked = false;
                FrameworkElement.checkBoxStrictMode.IsChecked = false;
            }

            private void buttonReload_Click(object sender, RoutedEventArgs e)
            {
                if (MessageBox.Show("Your current selections will be lost.\nDo you want to reload this page ?", "Confimation [IL2DCE]",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Game.Core.ReadCampaignInfo();
                    Game.Core.ReadCareerInfo();

                    Game.gameInterface.PageChange(new SelectCareerPage(), null);
                }
            }

            private void checkBox_CheckedChange(object sender, RoutedEventArgs e)
            {
                UpdateCareerListFilter();
                UpdateButtonStatus();
            }

            #endregion

            #region ComboBox & ListBox SelectionChanged

            private void comboBoxSelectArmy_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {
                    UpdateAirForceComboBoxInfo();
                }

                UpdateCareerListFilter();
                UpdateButtonStatus();
            }

            private void comboBoxSelectAirForce_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {

                }

                UpdateCareerListFilter();
                UpdateButtonStatus();
            }

            private void comboBoxSelectCampaignMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {

                }

                UpdateCareerListFilter();
                UpdateButtonStatus();
            }

            private void comboBoxSelectCampaign_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {

                }

                UpdateCareerListFilter();
                UpdateButtonStatus();
            }

            private void comboBoxSelectAircraft_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0 && !hookComboSelectionChanged)
                {

                }

                UpdateCareerListFilter();
                UpdateButtonStatus();
            }

            private void ListCareerSelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                Career career = SelectedCareer;
                if (career != null && career.CampaignInfo != null)
                {
                    CampaignInfo campaignInfo = career.CampaignInfo;
                    FrameworkElement.textBoxStatusCampaign.Text = campaignInfo.ToSummaryString();
                    FrameworkElement.textBoxStatusCurrent.Text = career.ToStringCurrestStatus();
                    FrameworkElement.textBoxStatusTotal.Text = career.ToStringTotalResult();
                    UpdateDisplaySkillInfo(career.PlayerAirGroupSkill);
                }
                else if (career != null)
                {
                    FrameworkElement.textBoxStatusCampaign.Text = NoFileString;
                    FrameworkElement.textBoxStatusCurrent.Text = career.ToStringCurrestStatus();
                    FrameworkElement.textBoxStatusTotal.Text = career.ToStringTotalResult();
                    UpdateDisplaySkillInfo(career.PlayerAirGroupSkill);
                }
                else
                {
                    FrameworkElement.textBoxStatusCampaign.Text = NoFileString;
                    FrameworkElement.textBoxStatusCurrent.Text = NoFileString;
                    FrameworkElement.textBoxStatusTotal.Text = NoFileString;
                    UpdateDisplaySkillInfo(null);
                }

                UpdateAircraftImage(career);
                UpdateButtonStatus();
            }

            private void ComboBoxCareerSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    ComboBox comboBox = FrameworkElement.comboBoxCareerSort;
                    ComboBoxItem item = comboBox.SelectedItem as ComboBoxItem;
                    if (item != null)
                    {
                        ECareerSort sort = (ECareerSort)item.Tag;
                        // ListBox listBox = sender as ListBox;
                        ListBox listBox = FrameworkElement.ListCareer;
                        string property = CareerSortProperty[(int)sort / 2];
                        ListSortDirection direction = (ListSortDirection)((int)sort % 2);
                        listBox.Items.SortDescriptions.Clear();
                        listBox.Items.SortDescriptions.Add(new SortDescription(property, direction));
                        listBox.Items.Refresh();
                        listBox.SelectedIndex = listBox.Items.Count > 0 ? 0 : -1; 
                    }
                }
            }

            #endregion

            private void UpdateArmyComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectArmy;
                comboBox.Items.Add(new ComboBoxItem() { Tag = -1, Content = AllString });
                for (EArmy army = EArmy.Red; army <= EArmy.Blue; army++)
                {
                    comboBox.Items.Add(new ComboBoxItem() {Tag = (int)army, Content = army.ToString() });
                }
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateAirForceComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectAirForce;
                comboBox.Items.Clear();

                comboBox.Items.Add(new ComboBoxItem() { Tag = -1, Content = AllString });
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
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateCampaignModeComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectCampaignMode;
                comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = AllString });
                for (ECampaignMode mode = ECampaignMode.Default; mode < ECampaignMode.Count; mode++)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = mode, Content = mode.ToDescription(), });
                }
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateCampaignComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectCampaign;
                comboBox.Items.Add(new ComboBoxItem() { Tag = null, Content = AllString });
                foreach (CampaignInfo campaignInfo in Game.Core.CampaignInfos)
                {
                    comboBox.Items.Add(campaignInfo);
                }
                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateAircraftComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxSelectAircraft;

                hookComboSelectionChanged = true;
                comboBox.Items.Clear();

                comboBox.Items.Add(new ComboBoxItem() { Tag = string.Empty, Content = AllString });
                var aicrafts = Game.Core.AvailableCareers.Select(x => x.Aircraft).Distinct().Where(x => !string.IsNullOrEmpty(x)).OrderBy(x => x);
                foreach (var aircraft in aicrafts)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = aircraft, Content = aircraft });
                }
                hookComboSelectionChanged = false;

                comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateCareerList()
            {
                ListBox listBox = FrameworkElement.ListCareer;
                listBox.Items.Clear();
                foreach (Career career in Game.Core.AvailableCareers)
                {
                    listBox.Items.Add(career);
                }
                // listBox.Items.Refresh();
                listBox.SelectedIndex = FrameworkElement.ListCareer.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateCareerListFilter()
            {
                ListBox listBox = FrameworkElement.ListCareer;
                listBox.Items.Filter = new Predicate<object>(ContainsCareer);
                listBox.SelectedIndex = listBox.Items.Count > 0 ? 0 : -1;
            }

            private bool ContainsCareer(object obj)
            {
                int armyIndex = SelectedArmyIndex;
                int airForceIndex = SelectedAirForceIndex;
                ECampaignMode? mode = SelectedCampaignMode;
                CampaignInfo campaignInfo = SelectedCampaign;
                string aircraft = SelectedAircraft;
                bool strictMode = SelectedStrictMode;
                bool playable = SelectedPlayable;
                Career career = obj as Career;
                return ((armyIndex == -1 || career.ArmyIndex == armyIndex) &&
                        (airForceIndex == -1 || career.AirForceIndex == airForceIndex) &&
                        (mode == null || career.CampaignMode == mode.Value) &&
                        (campaignInfo == null || career.CampaignInfo == campaignInfo) &&
                        (string.IsNullOrEmpty(aircraft) || string.Compare(career.Aircraft, aircraft, true) == 0) && 
                        (!strictMode || (strictMode && career.StrictMode)) && 
                        (!playable || (playable && career.CampaignInfo != null && career.Date <= career.EndDate && career.IsProgressEnableMission()) && 
                        (!career.StrictMode || career.StrictMode && career.Status == (int)EPlayerStatus.Alive)));
            }

            private void UpdateCareerSortComboBoxInfo()
            {
                ComboBox comboBox = FrameworkElement.comboBoxCareerSort;
                comboBox.Items.Clear();

                for (ECareerSort i = ECareerSort.DisplayAscending; i < ECareerSort.Count; i++)
                {
                    comboBox.Items.Add(
                        new ComboBoxItem()
                        {
                            Tag = i,
                            Content = i.ToDescription(),
                        });
                }

                comboBox.Text = ECareerSort.UpdateDateTimeDescending.ToDescription();
            }

            private void UpdateCareerListSort()
            {
                ListBox listBox = FrameworkElement.ListCareer;
                listBox.Items.Filter = new Predicate<object>(ContainsCareer);
                listBox.SelectedIndex = listBox.Items.Count > 0 ? 0 : -1;
            }

            private void UpdateDisplaySkillInfo(Skill[] skills)
            {
                ToolTip toolTip = FrameworkElement.textBoxStatusCurrentSkill.ToolTip as ToolTip;
                if (toolTip == null)
                {
                    FrameworkElement.textBoxStatusCurrentSkill.ToolTip = toolTip = new ToolTip();
                    toolTip.FontFamily = new FontFamily(Config.DefaultFixedFontName);
                }
                string strToolTips = skills != null ? Skill.ToDetailDisplayString(skills): null;
                //if (str == null)
                //{
                //    str = NoFileString;
                //}
                //else if (str.Length == 0)
                if (string.IsNullOrEmpty(strToolTips))
                {
                    strToolTips = NoSkillValueMessage;
                }
                toolTip.Content = strToolTips;

                string strText = skills != null ? Skill.ToDetailDisplayStringHorizontal(skills) : null;
                if (string.IsNullOrEmpty(strText))
                {
                    strText = NoSkillValueMessage;
                }
                FrameworkElement.textBoxStatusCurrentSkill.Text = strText;
            }

            private void UpdateAircraftImage(Career career)
            {
                if (career != null && career.CampaignInfo != null)
                {
                    MissionFile missionFile = new MissionFile(Game.gpLoadSectionFile(career.MissionFileName), career.CampaignInfo.AirGroupInfos, MissionFile.LoadLevel.AirGroup);
                    SelecedtAirGroup = missionFile.AirGroups.Where(x => x.ArmyIndex == career.ArmyIndex && string.Compare(x.ToString(), career.AirGroup) == 0).FirstOrDefault();
                }
                else
                {
                    SelecedtAirGroup = null;
                }

                FrameworkElement.borderImage.DisplayImage(Game.gameInterface, SelecedtAirGroup != null ? SelecedtAirGroup.Class : string.Empty);
            }

            private void UpdateButtonStatus()
            {
                Career career = SelectedCareer;
                FrameworkElement.Continue.IsEnabled = career != null && career.CampaignInfo != null && career.Date <= career.EndDate && career.IsProgressEnableMission() &&
                                                    ((career.StrictMode && career.Status == (int)EPlayerStatus.Alive) || !career.StrictMode) && SelecedtAirGroup != null;
                FrameworkElement.Delete.IsEnabled = career != null;
            }
        }
    }
}