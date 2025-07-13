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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game;
using Microsoft.Win32;

namespace IL2DCE.Pages.Controls
{
    /// <summary>
    /// GeneralSettingsGroupBox.xaml の相互作用ロジック
    /// </summary>
    public partial class GeneralSettingsGroupBox : GroupBox
    {
        public const string RandomString = "Random";
        public const string DefaultString = "Default";

        public const string SectionGeneralSettings = "GeneralSettings";
        public const string GeneralSettingsFileFilter = "General Settings File(.ini)|GeneralSettings*.ini";

        public const string MsgErrorFileLocked = "Unable to save configuration file. File is locked.[{0}]";
        public const string MsgErrorFileNotFound = "Configuration file not found, please save your configuration.[{0}]";
                                                         
        public event SelectionChangedEventHandler ComboBoxSelectionChangedEvent;

        public event TextChangedEventHandler ComboBoxTextChangedEvent;

        #region Property

        public GameIterface GameInterface
        {
            get;
            private set;
        }

        public Config Config
        {
            get;
            private set;
        }

        public int SelectedAdditionalAirOperations
        {
            get
            {
                int? selected = comboBoxSelectAdditionalAirOperations.SelectedItem as int?;
                if (selected != null)
                {
                    return selected.Value;
                }

                return -1;
            }
        }

        public int SelectedAdditionalGroundOperations
        {
            get
            {
                int? selected = comboBoxSelectAdditionalGroundOperations.SelectedItem as int?;
                if (selected != null)
                {
                    return selected.Value;
                }
                else
                {
                    string text = comboBoxSelectAdditionalGroundOperations.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        int num;
                        if (int.TryParse(text, NumberStyles.Integer, Config.NumberFormat, out num))
                        {
                            return num;
                        }
                    }
                }
                return -1;
            }
        }

        public bool SelectedAdditionalAirGroups
        {
            get
            {
                bool? isCheckd = checkBoxAdditionalAirGroups.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedAdditionalGroundGroups
        {
            get
            {
                bool? isCheckd = checkBoxAdditionalGroundGroups.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedAdditionalStasionaries
        {
            get
            {
                bool? isCheckd = checkBoxAdditionalStationaryUnits.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedKeepTotalAirGroups
        {
            get
            {
                bool? isCheckd = checkBoxKeepUnitsAirGroups.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedKeepTotalGroundGroups
        {
            get
            {
                bool? isCheckd = checkBoxKeepUnitsGroundGroups.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedKeepTotalStationaries
        {
            get
            {
                bool? isCheckd = checkBoxKeepUnitsStationaryUnits.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomLocationFriendly
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomLocationFriendly.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomLocationEnemy
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomLocationEnemy.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomLocationPlayer
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomLocationPlayer.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomAltitudeFriendly
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomAltitudeFriendly.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomAltitudeEnemy
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomAltitudeEnemy.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomTimeFriendly
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomTimeFriendly.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedSpawnRandomTimeEnemy
        {
            get
            {
                bool? isCheckd = checkBoxSpawnRandomTimeEnemy.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public int SelectedRandomTimeBegin
        {
            get
            {
                int? selected = comboBoxSelectRandomTimeBegin.SelectedItem as int?;
                if (selected != null)
                {
                    return selected.Value;
                }
                else
                {
                    string text = comboBoxSelectRandomTimeBegin.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        int num;
                        if (int.TryParse(text, NumberStyles.Integer, Config.NumberFormat, out num))
                        {
                            return num;
                        }
                    }
                }
                return -1;
            }
        }

        public int SelectedRandomTimeEnd
        {
            get
            {
                int? selected = comboBoxSelectRandomTimeEnd.SelectedItem as int?;
                if (selected != null)
                {
                    return selected.Value;
                }
                else
                {
                    string text = comboBoxSelectRandomTimeEnd.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        int num;
                        if (int.TryParse(text, NumberStyles.Integer, Config.NumberFormat, out num))
                        {
                            return num;
                        }
                    }
                }
                return -1;
            }
        }

        public bool SelectedGroundGroupGeneric
        {
            get
            {
                bool? isCheckd = checkBoxGroundGroupGeneric.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedStationaryGeneric
        {
            get
            {
                bool? isCheckd = checkBoxStatiomaryGeneric.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public EArmorUnitNumsSet SelectedUnitNumsArmor
        {
            get
            {
                ComboBoxItem selected = comboBoxSelectUnitNumsArmor.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (EArmorUnitNumsSet)selected.Tag;
                }
                return EArmorUnitNumsSet.Random;
            }
        }

        public EShipUnitNumsSet SelectedUnitNumsShip
        {
            get
            {
                ComboBoxItem selected = comboBoxSelectUnitNumsShip.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (EShipUnitNumsSet)selected.Tag;
                }
                return EShipUnitNumsSet.Random;
            }
        }

        public ESkillSet SelecteAISkill
        {
            get
            {
                ComboBoxItem selected = comboBoxSelectAISkill.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (ESkillSet)selected.Tag;
                }

                return ESkillSet.Random;
            }
        }

        public int SelectedArtilleryTimeout
        {
            get
            {
                ComboBoxItem selected = comboBoxSelectArtilleryTimeout.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (int)selected.Tag;
                }
                return -1;
            }
        }

        public int SelectedArtilleryRHide
        {
            get
            {
                ComboBoxItem selected = comboBoxSelectArtilleryRhide.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (int)selected.Tag;
                }
                return -1;
            }
        }

        public int SelectedArtilleryZOffset
        {
            get
            {
                ComboBoxItem selected = comboBoxSelectArtilleryZOffset.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (int)selected.Tag;
                }
                return -1;
            }
        }

        public ESkillSetShip SelectedShipSkill
        {
            get
            {
                ComboBoxItem selected = comboBoxSelectShipSkill.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (ESkillSetShip)selected.Tag;
                }
                return ESkillSetShip.Random;
            }
        }

        public int SelectedShipSleep
        {
            get
            {
                ComboBoxItem selected = comboBoxSelectShipSleep.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (int)selected.Tag;
                }
                return -1;
            }
        }

        public float SelectedShipSlowfire
        {
            get
            {
                ComboBoxItem selected = comboBoxSelectShipSlowfire.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (float)selected.Tag;
                }
                return -1;
            }
        }

        public bool SelectedAutoReArm
        {
            get
            {
                bool? isCheckd = checkBoxAutoReArm.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedAutoReFuel
        {
            get
            {
                bool? isCheckd = checkBoxAutoReFuel.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public bool SelectedTrackRecoding
        {
            get
            {
                bool? isCheckd = checkBoxTrackRecording.IsChecked;
                if (isCheckd != null)
                {
                    return isCheckd.Value;
                }

                return false;
            }
        }

        public double SelectedBattleTimeBegin
        {
            get
            {
                ComboBoxItem selected = comboBoxSelectBattleTimeBegin.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (double)selected.Tag;
                }

                return -1;
            }
        }

        public double SelectedBattleTimeEnd
        {
            get
            {
                ComboBoxItem selected = comboBoxSelectBattleTimeEnd.SelectedItem as ComboBoxItem;
                if (selected != null && selected.Tag != null)
                {
                    return (double)selected.Tag;
                }

                return -1;
            }
        }

        #endregion

        public GeneralSettingsGroupBox()
        {
            InitializeComponent();

            comboBoxSelectAdditionalAirOperations.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectAdditionalGroundOperations.Loaded += new RoutedEventHandler(comboBox_Loaded);
            comboBoxSelectAdditionalGroundOperations.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectUnitNumsArmor.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectUnitNumsShip.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectRandomTimeBegin.Loaded += new RoutedEventHandler(comboBox_Loaded);
            comboBoxSelectRandomTimeBegin.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectRandomTimeEnd.Loaded += new RoutedEventHandler(comboBox_Loaded);
            comboBoxSelectRandomTimeEnd.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            checkBoxSpawnRandomTimeFriendly.Checked += new RoutedEventHandler(checkBoxSpawnRandomTime_CheckedChange);
            checkBoxSpawnRandomTimeFriendly.Unchecked += new RoutedEventHandler(checkBoxSpawnRandomTime_CheckedChange);
            checkBoxSpawnRandomTimeEnemy.Checked += new RoutedEventHandler(checkBoxSpawnRandomTime_CheckedChange);
            checkBoxSpawnRandomTimeEnemy.Unchecked += new RoutedEventHandler(checkBoxSpawnRandomTime_CheckedChange);
            comboBoxSelectAISkill.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectArtilleryTimeout.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectArtilleryRhide.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectArtilleryZOffset.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectShipSkill.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectShipSleep.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectShipSlowfire.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectBattleTimeBegin.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            comboBoxSelectBattleTimeEnd.SelectionChanged += new SelectionChangedEventHandler(comboBoxSelect_SelectionChanged);
            UpdateSelectAdditionalAirOperationsComboBox();
            UpdateSelectAdditionalGroundOperationsComboBox();
            UpdateSelectUnitNumsArmorComboBox();
            UpdateSelectUnitNumsShipComboBox();
            UpdateSelectRandomTimeBeginComboBox();
            UpdateSelectRandomTimeEndComboBox();
            UpdateSelectAISkillComboBox();
            UpdateSelectArtilleryTimeoutComboBox();
            UpdateSelectArtilleryRhideComboBox();
            UpdateSelectArtilleryZOffsetComboBox();
            UpdateSelectShipSkillComboBox();
            UpdateSelectShipSleepComboBox();
            UpdateSelectShipSlowfireComboBox();
            UpdatSelectBattleTimeBeginComboBox();
            UpdatSelectBattleTimeEndComboBox();

            labelSelectArtilleryZOffset.Visibility = Visibility.Hidden;
            comboBoxSelectArtilleryZOffset.Visibility = Visibility.Hidden;
        }

        public void SetRelationInfo(GameIterface gameInterface, Config config)
        {
            GameInterface = gameInterface;
            Config = config;

            UpdateSkillComboBoxSkillValueInfo(comboBoxSelectAISkill, SelecteAISkill);
            UpdatSelectBattleTimeBeginComboBox();
            UpdatSelectBattleTimeEndComboBox();
        }

        #region Event Handler

        private void GroupBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void comboBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ComboBoxEx comboBox = sender as ComboBoxEx;
            if (comboBox != null)
            {
                comboBox.TextBox.TextChanged += new TextChangedEventHandler(comboBox_TextChanged);
            }
        }

        private void comboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ComboBoxTextChangedEvent != null)
            {
                ComboBoxTextChangedEvent(sender, e);
            }
        }

        private void comboBoxSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxSelectionChangedEvent != null)
            {
                ComboBoxSelectionChangedEvent(sender, e);
            }
        }

        private void checkBoxSpawnRandomTime_CheckedChange(object sender, RoutedEventArgs e)
        {
            comboBoxSelectRandomTimeBegin.IsEnabled =
                comboBoxSelectRandomTimeEnd.IsEnabled = SelectedSpawnRandomTimeFriendly || SelectedSpawnRandomTimeEnemy;
        }

        private void comboBoxSelectAISkill_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                UpdateSkillComboBoxSkillValueInfo(comboBoxSelectAISkill, SelecteAISkill);
            }
        }

        #region Button Click

        private void buttonRandomizeAllCheck_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxSpawnRandomLocationFriendly.IsEnabled && checkBoxSpawnRandomLocationFriendly.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationFriendly.IsChecked = true;
            }
            if (checkBoxSpawnRandomLocationEnemy.IsEnabled && checkBoxSpawnRandomLocationEnemy.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationEnemy.IsChecked = true;
            }
            if (checkBoxSpawnRandomLocationPlayer.IsEnabled && checkBoxSpawnRandomLocationPlayer.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationPlayer.IsChecked = true;
            }
            checkBoxSpawnRandomAltitudeFriendly.IsChecked = true;
            checkBoxSpawnRandomAltitudeEnemy.IsChecked = true;
            checkBoxSpawnRandomTimeFriendly.IsChecked = true;
            checkBoxSpawnRandomTimeEnemy.IsChecked = true;
        }

        private void buttonRandomizeAllUnCheck_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxSpawnRandomLocationFriendly.IsEnabled && checkBoxSpawnRandomLocationFriendly.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationFriendly.IsChecked = false;
            }
            if (checkBoxSpawnRandomLocationEnemy.IsEnabled && checkBoxSpawnRandomLocationEnemy.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationEnemy.IsChecked = false;
            }
            if (checkBoxSpawnRandomLocationPlayer.IsEnabled && checkBoxSpawnRandomLocationPlayer.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationPlayer.IsChecked = false;
            }
            checkBoxSpawnRandomAltitudeFriendly.IsChecked = false;
            checkBoxSpawnRandomAltitudeEnemy.IsChecked = false;
            checkBoxSpawnRandomTimeFriendly.IsChecked = false;
            checkBoxSpawnRandomTimeEnemy.IsChecked = false;
        }

        private void buttonAllDefault_Click(object sender, RoutedEventArgs e)
        {
            comboBoxSelectAdditionalAirOperations.SelectedItem = Config.DefaultAdditionalAirOperations;
            comboBoxSelectAdditionalGroundOperations.SelectedItem = Config.DefaultAdditionalGroundOperations;
            checkBoxAdditionalAirGroups.IsChecked = false;
            checkBoxAdditionalGroundGroups.IsChecked = false;
            checkBoxAdditionalStationaryUnits.IsChecked = false;
            checkBoxKeepUnitsAirGroups.IsChecked = false;
            checkBoxKeepUnitsGroundGroups.IsChecked = false;
            checkBoxKeepUnitsStationaryUnits.IsChecked = false;

            if (comboBoxSelectUnitNumsArmor.IsEnabled && comboBoxSelectUnitNumsArmor.Visibility == Visibility.Visible)
            {
                comboBoxSelectUnitNumsArmor.Text = EArmorUnitNumsSet.Random.ToDescription();
            }
            if (comboBoxSelectUnitNumsShip.IsEnabled && comboBoxSelectUnitNumsShip.Visibility == Visibility.Visible)
            {
                comboBoxSelectUnitNumsShip.Text = EShipUnitNumsSet.Random.ToDescription();
            }
            checkBoxGroundGroupGeneric.IsChecked = false;
            checkBoxStatiomaryGeneric.IsChecked = false;

            if (checkBoxSpawnRandomLocationFriendly.IsEnabled && checkBoxSpawnRandomLocationFriendly.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationFriendly.IsChecked = false;
            }
            if (checkBoxSpawnRandomLocationEnemy.IsEnabled && checkBoxSpawnRandomLocationEnemy.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationEnemy.IsChecked = true;
            }
            if (checkBoxSpawnRandomLocationPlayer.IsEnabled && checkBoxSpawnRandomLocationPlayer.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationPlayer.IsChecked = false;
            }
            checkBoxSpawnRandomAltitudeFriendly.IsChecked = false;
            checkBoxSpawnRandomAltitudeEnemy.IsChecked = true;
            checkBoxSpawnRandomTimeFriendly.IsChecked = false;
            checkBoxSpawnRandomTimeEnemy.IsChecked = true;

            comboBoxSelectRandomTimeBegin.SelectedItem = Spawn.SpawnTime.DefaultBeginSec;
            comboBoxSelectRandomTimeEnd.SelectedItem = Spawn.SpawnTime.DefaultEndSec;

            comboBoxSelectAISkill.Text = ESkillSet.Random.ToDescription();
            comboBoxSelectArtilleryTimeout.Text = DefaultString;
            comboBoxSelectArtilleryRhide.Text = DefaultString;
            comboBoxSelectArtilleryZOffset.Text = DefaultString;
            comboBoxSelectShipSkill.Text = ESkillSetShip.Random.ToDescription();
            comboBoxSelectShipSleep.Text = DefaultString;
            comboBoxSelectShipSlowfire.Text = DefaultString;

            checkBoxAutoReArm.IsChecked = false;
            checkBoxAutoReFuel.IsChecked = false;
            checkBoxTrackRecording.IsChecked = false;
        }

        private void buttonUserLoad_Click(object sender, RoutedEventArgs e)
        {
            string filePath = string.Format("{0}/{1}", Config.UserMissionsFolder, Config.GeneralSettingsFileName);
            string fileSystemPath = GameInterface.ToFileSystemPath(filePath);
            if (File.Exists(fileSystemPath))
            {
                ISectionFile file = GameInterface.SectionFileLoad(filePath);
                Read(file);
            }
            else
            {
                MessageBox.Show(string.Format(MsgErrorFileNotFound, fileSystemPath), Config.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void buttonUserSave_Click(object sender, RoutedEventArgs e)
        {
            string filePath = string.Format("{0}/{1}", Config.UserMissionsFolder, Config.GeneralSettingsFileName);
            string fileSystemPath = GameInterface.ToFileSystemPath(filePath);
            if (FileUtil.IsFileWritable(fileSystemPath))
            {
                ISectionFile file = GameInterface.SectionFileCreate();
                Write(file);
                file.save(filePath);
            }
            else
            {
                MessageBox.Show(string.Format(MsgErrorFileLocked, fileSystemPath), Config.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonUserLoadAs_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = GameInterface.ToFileSystemPath(Config.UserMissionsFolder);
            dlg.FileName = SectionGeneralSettings; 
            dlg.DefaultExt = Config.IniFileExt;
            dlg.Filter = GeneralSettingsFileFilter;
            bool? result = dlg.ShowDialog();
            if (result != null && result.Value)
            {
                try
                {
                    SilkySkyCloDFile file = SilkySkyCloDFile.Load(dlg.FileName);
                    Read(file);
                }
                catch (Exception ex)
                {
                    string message = string.Format("{0} - {1}", "GeneralSettingsGroupBox.buttonUserLoadAs_Click", ex.Message, ex.StackTrace);
                    Core.WriteLog(message);
                }
            }
        }

        private void buttonUserSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = GameInterface.ToFileSystemPath(Config.UserMissionsFolder);
            dlg.FileName = string.Format("{0}_{1}{2}", SectionGeneralSettings, DateTime.Now.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat), Config.IniFileExt);
            dlg.DefaultExt = Config.IniFileExt;
            dlg.Filter = GeneralSettingsFileFilter;
            bool? result = dlg.ShowDialog();
            if (result != null && result.Value)
            {
                try
                {
                    SilkySkyCloDFile file = SilkySkyCloDFile.Create();
                    Write(file);
                    file.Save(dlg.FileName);
                }
                catch (Exception ex)
                {
                    string message = string.Format("{0} - {1}", "GeneralSettingsGroupBox.buttonUserSaveAs_Click", ex.Message, ex.StackTrace);
                    Core.WriteLog(message);
                }
            }
        }

        #endregion

        #endregion

        public void UpdateSelectAdditionalAirOperationsComboBox()
        {
            ComboBox comboBox = comboBoxSelectAdditionalAirOperations;
            foreach (var item in Enumerable.Range(Config.MinAdditionalAirOperations, Config.MaxAdditionalAirOperations))
            {
                comboBox.Items.Add(item);
            }
            comboBox.SelectedItem = Config.DefaultAdditionalAirOperations;
        }

        public void UpdateSelectAdditionalGroundOperationsComboBox()
        {
            ComboBox comboBox = comboBoxSelectAdditionalGroundOperations;
            for (int i = Config.MinAdditionalGroundOperations; i <= Config.MaxAdditionalGroundOperations; i += 10)
            {
                comboBox.Items.Add(i);
            }
            comboBox.SelectedItem = Config.DefaultAdditionalGroundOperations;
        }

        public void UpdateSelectUnitNumsArmorComboBox()
        {
            ComboBox comboBox = comboBoxSelectUnitNumsArmor;
            comboBox.Items.Add(new ComboBoxItem() { Tag = EArmorUnitNumsSet.Random, Content = EArmorUnitNumsSet.Random.ToDescription() });
            comboBox.Items.Add(new ComboBoxItem() { Tag = EArmorUnitNumsSet.Default, Content = EArmorUnitNumsSet.Default.ToDescription() });
            for (EArmorUnitNumsSet i = EArmorUnitNumsSet.Range1_3; i <= EArmorUnitNumsSet.Range5_8; i++)
            {
                comboBox.Items.Add(new ComboBoxItem() { Tag = i, Content = i.ToDescription() });
            }
            comboBox.Text = EArmorUnitNumsSet.Random.ToDescription();
        }

        public void UpdateSelectUnitNumsShipComboBox()
        {
            ComboBox comboBox = comboBoxSelectUnitNumsShip;
            comboBox.Items.Add(new ComboBoxItem() { Tag = EShipUnitNumsSet.Random, Content = EShipUnitNumsSet.Random.ToDescription() });
            comboBox.Items.Add(new ComboBoxItem() { Tag = EShipUnitNumsSet.Default, Content = EShipUnitNumsSet.Default.ToDescription() });
            for (EShipUnitNumsSet i = EShipUnitNumsSet.Range1; i <= EShipUnitNumsSet.Range3_5; i++)
            {
                comboBox.Items.Add(new ComboBoxItem() { Tag = i, Content = i.ToDescription() });
            }
            comboBox.Text = EShipUnitNumsSet.Random.ToDescription();
        }

        public void UpdateSelectRandomTimeBeginComboBox()
        {
            ComboBox comboBox = comboBoxSelectRandomTimeBegin;
            for (int i = Spawn.SpawnTime.MinimumBeginSec; i <= Spawn.SpawnTime.MaximumEndSec; i += i < 60 ? 15 : i < 300 ? 60 : i < 1800 ? 300 : 1800)
            {
                comboBox.Items.Add(i);
            }
            if (comboBox.Items.IndexOf(Spawn.SpawnTime.MaximumEndSec) == -1)
            {
                comboBox.Items.Add(Spawn.SpawnTime.MaximumEndSec);
            }
            comboBox.SelectedItem = Spawn.SpawnTime.DefaultBeginSec;
            comboBox.IsEnabled = SelectedSpawnRandomTimeFriendly || SelectedSpawnRandomTimeEnemy;
        }

        public void UpdateSelectRandomTimeEndComboBox()
        {
            ComboBox comboBox = comboBoxSelectRandomTimeEnd;
            for (int i = Spawn.SpawnTime.MinimumBeginSec; i <= Spawn.SpawnTime.MaximumEndSec; i += i < 60 ? 15 : i < 300 ? 60 : i < 1800 ? 300 : 1800)
            {
                comboBox.Items.Add(i);
            }
            if (comboBox.Items.IndexOf(Spawn.SpawnTime.MaximumEndSec) == -1)
            {
                comboBox.Items.Add(Spawn.SpawnTime.MaximumEndSec);
            }
            comboBox.SelectedItem = Spawn.SpawnTime.DefaultEndSec;
            comboBox.IsEnabled = SelectedSpawnRandomTimeFriendly || SelectedSpawnRandomTimeEnemy;
        }

        public void UpdateSelectAISkillComboBox()
        {
            ComboBox comboBox = comboBoxSelectAISkill;
            for (ESkillSet i = ESkillSet.Random; i < ESkillSet.Count; i++)
            {
                comboBox.Items.Add(new ComboBoxItem() { Tag = i, Content = i.ToDescription() });
            }
            comboBox.Text = ESkillSet.Random.ToDescription();
        }

        public void UpdateSelectArtilleryTimeoutComboBox()
        {
            ComboBox comboBox = comboBoxSelectArtilleryTimeout;
            comboBox.Items.Add(new ComboBoxItem() { Tag = ArtilleryOption.TimeoutRandom, Content = RandomString });
            comboBox.Items.Add(new ComboBoxItem() { Tag = ArtilleryOption.TimeoutMissionDefault, Content = DefaultString });
#if false
            IEnumerable<int> range = Util.Collection.GetRange(ArtilleryOption.MinTimeout, ArtilleryOption.MaxTimeout);
            foreach (var item in range)
            {
                // comboBox.Items.Add(new ComboBoxItem() { Tag = item, Content = string.Format(Config.NumberFormat, "{0,3}[{1}]", item, MissionTime.ToString(item / 60.0)) });
                comboBox.Items.Add(new ComboBoxItem() { Tag = item, Content = string.Format(Config.NumberFormat, "{0,3}", item) });
            }
#else
            IEnumerable<int> range1 = new int[] { 0, 1, 2, 3, 5, 10, 15, 30, 45, 60, };
            IEnumerable<int> range2 = new int[] { 2, 3, 4, 5, 6, 8, 10, 12, }.Select(x => x * 60);
            foreach (var item in range1.Concat(range2).Distinct().OrderBy(x => x))
            {
                comboBox.Items.Add(new ComboBoxItem() { Tag = item, Content = ArtilleryOption.CreateDisplayStringTimeout(item) });
            }
#endif
            comboBox.Text = DefaultString; // string.Format(Config.NumberFormat, "{0,3}", ArtilleryOption.MinTimeout);
        }

        public void UpdateSelectArtilleryRhideComboBox()
        {
            ComboBox comboBox = comboBoxSelectArtilleryRhide;
            comboBox.Items.Add(new ComboBoxItem() { Tag = ArtilleryOption.RHideRandom, Content = RandomString });
            comboBox.Items.Add(new ComboBoxItem() { Tag = ArtilleryOption.RHideMissionDefault, Content = DefaultString });
            IEnumerable<int> range = Util.Collection.GetRange(ArtilleryOption.MinRHide, ArtilleryOption.MaxRHide);
            foreach (var item in range)
            {
                comboBox.Items.Add(new ComboBoxItem() { Tag = item, Content = ArtilleryOption.CreateDisplayStringRHide(item)/*string.Format(Config.NumberFormat, "{0,5}", item)*/ });
            }
            comboBox.Text = DefaultString; // string.Format(Config.NumberFormat, "{0,5}", ArtilleryOption.MinRHide);
        }

        public void UpdateSelectArtilleryZOffsetComboBox()
        {
            ComboBox comboBox = comboBoxSelectArtilleryZOffset;
            comboBox.Items.Add(new ComboBoxItem() { Tag = ArtilleryOption.ZOffsetRandom, Content = RandomString });
            comboBox.Items.Add(new ComboBoxItem() { Tag = ArtilleryOption.ZOffsetMissionDefault, Content = DefaultString });
            IEnumerable<int> range1 = Util.Collection.GetRange(0, -1 * ArtilleryOption.MinZOffset).Select(x => x * -1);
            IEnumerable<int> range2 = Util.Collection.GetRange(0, ArtilleryOption.MaxZOffset);
            foreach (var item in range1.Concat(range2).Distinct().OrderBy(x => x))
            {
                comboBox.Items.Add(new ComboBoxItem() { Tag = item, Content = ArtilleryOption.CreateDisplayStringZOffsete(item)/*string.Format(Config.NumberFormat, "{0,4}", item)*/ });
            }
            comboBox.Text = DefaultString; // string.Format(Config.NumberFormat, "{0,5}", ArtilleryOption.MinRHide);
        }

        public void UpdateSelectShipSkillComboBox()
        {
            ComboBox comboBox = comboBoxSelectShipSkill;
            for (ESkillSetShip i = ESkillSetShip.Random; i < ESkillSetShip.Count; i++)
            {
                comboBox.Items.Add(new ComboBoxItem() { Tag = i, Content = i.ToDescription() });
            }
            comboBox.Text = ESkillSetShip.Random.ToDescription();
        }

        public void UpdateSelectShipSleepComboBox()
        {
            ComboBox comboBox = comboBoxSelectShipSleep;
            comboBox.Items.Add(new ComboBoxItem() { Tag = ShipOption.SleepRandom, Content = RandomString });
            comboBox.Items.Add(new ComboBoxItem() { Tag = ShipOption.SleepMissionDefault, Content = DefaultString });
            // IEnumerable<int> range = Util.Collection.GetRange(ShipOption.MinSleep, ShipOption.MaxSleep);
            IEnumerable<int> range1 = new int[] { 0, 1, 2, 3, 5, 10, 15, 30, 45, 60, };
            IEnumerable<int> range2 = new int[] { 2, 3, 4, 5, 6, 8, 10, 12, 15, 30, 45, 60, 90, }.Select(x => x * 60);
            foreach (var item in range1.Concat(range2).Distinct().OrderBy(x => x))
            {
                comboBox.Items.Add(new ComboBoxItem() { Tag = item, Content = ShipOption.CreateDisplayStringSleep(item) });
            }
            comboBox.Items.Add(new ComboBoxItem() { Tag = ShipOption.MaxSleep, Content = MissionTime.ToString(ShipOption.MaxSleep / 60.0) });
            comboBox.Text = DefaultString; //string.Format(Config.NumberFormat, "{0,4}", ShipOption.MinSleep);
        }

        public void UpdateSelectShipSlowfireComboBox()
        {
            ComboBox comboBox = comboBoxSelectShipSlowfire;
            comboBox.Items.Add(new ComboBoxItem() { Tag = ShipOption.SlowFireRandom, Content = RandomString });
            comboBox.Items.Add(new ComboBoxItem() { Tag = ShipOption.SlowFireMissionDefault, Content = DefaultString });
            List<float> range = Util.Collection.GetRange(1, (int)ShipOption.MaxSlowFire).Select(x => (float)x).ToList();
            range.Add(ShipOption.MinSlowFire);
            foreach (var item in range.OrderBy(x => x))
            {
                comboBox.Items.Add(new ComboBoxItem() { Tag = item, Content = ShipOption.CreateDisplayStringSlowfire(item)/*string.Format(Config.NumberFormat, "{0,3}", item)*/ });
            }
            comboBox.Text = DefaultString; // string.Format(Config.NumberFormat, "{0,3}", ShipOption.MinSlowFire);
        }

        public static void UpdateSkillComboBoxSkillValueInfo(ComboBox comboBox, Skill skill, AirGroup airGroup, Skills skills)
        {
            ToolTip toolTip = comboBox.ToolTip as ToolTip;
            if (toolTip == null)
            {
                comboBox.ToolTip = toolTip = new ToolTip();
                toolTip.FontFamily = new FontFamily(Config.DefaultFixedFontName);
            }
            string str = string.Empty;
            if (skill != null)
            {
                if (skill == Skill.Default)
                {
                    if (airGroup != null)
                    {
                        str = airGroup.Skills.Any() ? Skill.ToDetailString(airGroup.Skills.Values) : airGroup.Skill != null ? Skill.ToDetailString(new string[] { airGroup.Skill }) : string.Empty;
                    }
                }
                else if (skill == Skill.Random)
                {
                    Skills skillsRandom = new Skills(Skill.TweakedSkills);
                    Skills.UpdateSkillValue(skillsRandom, skills);
                    str = string.Format("Randomly determined.\n\n{0}", Skill.ToDetailString(skillsRandom));
                }
                else
                {
                    str = skill.ToDetailString();
                }
            }
            toolTip.Content = str;
        }

        public void UpdateSkillComboBoxSkillValueInfo(ComboBox comboBox, ESkillSet skillSet)
        {
            ToolTip toolTip = comboBox.ToolTip as ToolTip;
            if (toolTip == null)
            {
                comboBox.ToolTip = toolTip = new ToolTip();
                toolTip.FontFamily = new FontFamily(Config.DefaultFixedFontName);
                toolTip.ClipToBounds = true;
            }
            string str = string.Empty;
            if (skillSet == ESkillSet.Default)
            {
                str = "Each value in Mission file";
            }
            else
            {
                Skills skills;
                if (skillSet == ESkillSet.Random)
                {
                    skills = new Skills(Skill.TweakedSkills);
                }
                else if (skillSet == ESkillSet.UserSettings)
                {
                    skills = new Skills(Config.Skills.Except(Skill.TweakedSkills).Except(Skill.SystemSkills));
                }
                else
                {
                    skills = Skills.Create(skillSet);
                }
                if (Config != null)
                {
                    Skills.UpdateSkillValue(skills, Config.Skills);
                }
                // str = skills.Count > 0 ? string.Join("\n", skills.Select(x => x.ToDetailString())) : string.Empty;
                str = skills.Count > 0 ? Skill.ToDetailString(skills) : string.Empty;
            }
            toolTip.Content = str;
        }

        private void UpdatSelectBattleTimeBeginComboBox()
        {
            ComboBox comboBox = comboBoxSelectBattleTimeBegin;

            if (comboBox.Items.Count == 0)
            {
                for (double d = MissionTime.Begin; d <= MissionTime.End; d += 0.5)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = d, Content = MissionTime.ToString(d) });
                }
            }
            comboBox.Text = MissionTime.ToString(Config != null ? Config.RandomTimeBegin: MissionTime.Begin); 
        }

        private void UpdatSelectBattleTimeEndComboBox()
        {
            ComboBox comboBox = comboBoxSelectBattleTimeEnd;

            if (comboBox.Items.Count == 0)
            {
                for (double d = MissionTime.Begin; d <= MissionTime.End; d += 0.5)
                {
                    comboBox.Items.Add(new ComboBoxItem() { Tag = d, Content = MissionTime.ToString(d) });
                }
            }
            comboBox.Text = MissionTime.ToString(Config != null ? Config.RandomTimeEnd: MissionTime.End);
        }

        public void EnableBattleTimeComboBox(bool enable)
        {
            labelSelectBattleTime.Visibility = enable ? Visibility.Visible : Visibility.Hidden;
            comboBoxSelectBattleTimeBegin.IsEnabled = enable;
            comboBoxSelectBattleTimeBegin.Visibility = enable ? Visibility.Visible : Visibility.Hidden;
            labelSelectPeriodRangeBattleTime.Visibility = enable ? Visibility.Visible : Visibility.Hidden;
            comboBoxSelectBattleTimeEnd.IsEnabled = enable;
            comboBoxSelectBattleTimeEnd.Visibility = enable ? Visibility.Visible : Visibility.Hidden;
        }

        private void Write(ISectionFile file)
        {
            file.add(SectionGeneralSettings, comboBoxSelectAdditionalAirOperations.Name, comboBoxSelectAdditionalAirOperations.Text);
            file.add(SectionGeneralSettings, comboBoxSelectAdditionalGroundOperations.Name, comboBoxSelectAdditionalGroundOperations.Text);
            file.add(SectionGeneralSettings, checkBoxAdditionalAirGroups.Name, checkBoxAdditionalAirGroups.IsChecked != null && checkBoxAdditionalAirGroups.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxAdditionalGroundGroups.Name, checkBoxAdditionalGroundGroups.IsChecked != null && checkBoxAdditionalGroundGroups.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxAdditionalStationaryUnits.Name, checkBoxAdditionalStationaryUnits.IsChecked != null && checkBoxAdditionalStationaryUnits.IsChecked.Value ? "1" : "0");

            file.add(SectionGeneralSettings, checkBoxKeepUnitsAirGroups.Name, checkBoxKeepUnitsAirGroups.IsChecked != null && checkBoxKeepUnitsAirGroups.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxKeepUnitsGroundGroups.Name, checkBoxKeepUnitsGroundGroups.IsChecked != null && checkBoxKeepUnitsGroundGroups.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxKeepUnitsStationaryUnits.Name, checkBoxKeepUnitsStationaryUnits.IsChecked != null && checkBoxKeepUnitsStationaryUnits.IsChecked.Value ? "1" : "0");

            file.add(SectionGeneralSettings, comboBoxSelectUnitNumsArmor.Name, comboBoxSelectUnitNumsArmor.Text);
            file.add(SectionGeneralSettings, comboBoxSelectUnitNumsShip.Name, comboBoxSelectUnitNumsShip.Text);
            file.add(SectionGeneralSettings, checkBoxGroundGroupGeneric.Name, checkBoxGroundGroupGeneric.IsChecked != null && checkBoxGroundGroupGeneric.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxStatiomaryGeneric.Name, checkBoxStatiomaryGeneric.IsChecked != null && checkBoxStatiomaryGeneric.IsChecked.Value ? "1" : "0");

            file.add(SectionGeneralSettings, checkBoxSpawnRandomLocationFriendly.Name, checkBoxSpawnRandomLocationFriendly.IsChecked != null && checkBoxSpawnRandomLocationFriendly.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxSpawnRandomLocationEnemy.Name, checkBoxSpawnRandomLocationEnemy.IsChecked != null && checkBoxSpawnRandomLocationEnemy.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxSpawnRandomLocationPlayer.Name, checkBoxSpawnRandomLocationPlayer.IsChecked != null && checkBoxSpawnRandomLocationPlayer.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxSpawnRandomAltitudeFriendly.Name, checkBoxSpawnRandomAltitudeFriendly.IsChecked != null && checkBoxSpawnRandomAltitudeFriendly.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxSpawnRandomAltitudeEnemy.Name, checkBoxSpawnRandomAltitudeEnemy.IsChecked != null && checkBoxSpawnRandomAltitudeEnemy.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxSpawnRandomTimeFriendly.Name, checkBoxSpawnRandomTimeFriendly.IsChecked != null && checkBoxSpawnRandomTimeFriendly.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxSpawnRandomTimeEnemy.Name, checkBoxSpawnRandomTimeEnemy.IsChecked != null && checkBoxSpawnRandomTimeEnemy.IsChecked.Value ? "1" : "0");

            file.add(SectionGeneralSettings, comboBoxSelectRandomTimeBegin.Name, comboBoxSelectRandomTimeBegin.Text);
            file.add(SectionGeneralSettings, comboBoxSelectRandomTimeEnd.Name, comboBoxSelectRandomTimeEnd.Text);

            file.add(SectionGeneralSettings, comboBoxSelectAISkill.Name, comboBoxSelectAISkill.Text);
            file.add(SectionGeneralSettings, comboBoxSelectArtilleryTimeout.Name, comboBoxSelectArtilleryTimeout.Text);
            file.add(SectionGeneralSettings, comboBoxSelectArtilleryRhide.Name, comboBoxSelectArtilleryRhide.Text);
            file.add(SectionGeneralSettings, comboBoxSelectArtilleryZOffset.Name, comboBoxSelectArtilleryZOffset.Text);
            file.add(SectionGeneralSettings, comboBoxSelectShipSkill.Name, comboBoxSelectShipSkill.Text);
            file.add(SectionGeneralSettings, comboBoxSelectShipSleep.Name, comboBoxSelectShipSleep.Text);
            file.add(SectionGeneralSettings, comboBoxSelectShipSlowfire.Name, comboBoxSelectShipSlowfire.Text);

            file.add(SectionGeneralSettings, checkBoxAutoReArm.Name, checkBoxAutoReArm.IsChecked != null && checkBoxAutoReArm.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxAutoReFuel.Name, checkBoxAutoReFuel.IsChecked != null && checkBoxAutoReFuel.IsChecked.Value ? "1" : "0");
            file.add(SectionGeneralSettings, checkBoxTrackRecording.Name, checkBoxTrackRecording.IsChecked != null && checkBoxTrackRecording.IsChecked.Value ? "1" : "0");

            file.add(SectionGeneralSettings, comboBoxSelectBattleTimeBegin.Name, comboBoxSelectBattleTimeBegin.Text);
            file.add(SectionGeneralSettings, comboBoxSelectBattleTimeEnd.Name, comboBoxSelectBattleTimeEnd.Text);
        }

        private void Read(ISectionFile file)
        {
            comboBoxSelectAdditionalAirOperations.SelectedItem = file.get(SectionGeneralSettings, comboBoxSelectAdditionalAirOperations.Name, Config.DefaultAdditionalAirOperations);
            comboBoxSelectAdditionalGroundOperations.SelectedItem = file.get(SectionGeneralSettings, comboBoxSelectAdditionalGroundOperations.Name, Config.DefaultAdditionalGroundOperations);
            checkBoxAdditionalAirGroups.IsChecked = file.get(SectionGeneralSettings, checkBoxAdditionalAirGroups.Name, false);
            checkBoxAdditionalGroundGroups.IsChecked = file.get(SectionGeneralSettings, checkBoxAdditionalGroundGroups.Name, false);
            checkBoxAdditionalStationaryUnits.IsChecked = file.get(SectionGeneralSettings, checkBoxAdditionalStationaryUnits.Name, false);

            checkBoxKeepUnitsAirGroups.IsChecked = file.get(SectionGeneralSettings, checkBoxKeepUnitsAirGroups.Name, false);
            checkBoxKeepUnitsGroundGroups.IsChecked = file.get(SectionGeneralSettings, checkBoxKeepUnitsGroundGroups.Name, false);
            checkBoxKeepUnitsStationaryUnits.IsChecked = file.get(SectionGeneralSettings, checkBoxKeepUnitsStationaryUnits.Name, false);

            if (comboBoxSelectUnitNumsArmor.IsEnabled && comboBoxSelectUnitNumsArmor.Visibility == Visibility.Visible)
            {
                comboBoxSelectUnitNumsArmor.Text = file.get(SectionGeneralSettings, comboBoxSelectUnitNumsArmor.Name, EArmorUnitNumsSet.Random.ToDescription());
            }
            if (comboBoxSelectUnitNumsShip.IsEnabled && comboBoxSelectUnitNumsShip.Visibility == Visibility.Visible)
            {
                comboBoxSelectUnitNumsShip.Text = file.get(SectionGeneralSettings, comboBoxSelectUnitNumsShip.Name, EShipUnitNumsSet.Random.ToDescription());
            }
            checkBoxGroundGroupGeneric.IsChecked = file.get(SectionGeneralSettings, checkBoxGroundGroupGeneric.Name, false);
            checkBoxStatiomaryGeneric.IsChecked = file.get(SectionGeneralSettings, checkBoxStatiomaryGeneric.Name, false);

            if (checkBoxSpawnRandomLocationFriendly.IsEnabled && checkBoxSpawnRandomLocationFriendly.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationFriendly.IsChecked = file.get(SectionGeneralSettings, checkBoxSpawnRandomLocationFriendly.Name, false);
            }
            if (checkBoxSpawnRandomLocationEnemy.IsEnabled && checkBoxSpawnRandomLocationEnemy.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationEnemy.IsChecked = file.get(SectionGeneralSettings, checkBoxSpawnRandomLocationEnemy.Name, true);
            }
            if (checkBoxSpawnRandomLocationPlayer.IsEnabled && checkBoxSpawnRandomLocationPlayer.Visibility == Visibility.Visible)
            {
                checkBoxSpawnRandomLocationPlayer.IsChecked = file.get(SectionGeneralSettings, checkBoxSpawnRandomLocationPlayer.Name, false);
            }
            checkBoxSpawnRandomAltitudeFriendly.IsChecked = file.get(SectionGeneralSettings, checkBoxSpawnRandomAltitudeFriendly.Name, false);
            checkBoxSpawnRandomAltitudeEnemy.IsChecked = file.get(SectionGeneralSettings, checkBoxSpawnRandomAltitudeEnemy.Name, true);
            checkBoxSpawnRandomTimeFriendly.IsChecked = file.get(SectionGeneralSettings, checkBoxSpawnRandomTimeFriendly.Name, false);
            checkBoxSpawnRandomTimeEnemy.IsChecked = file.get(SectionGeneralSettings, checkBoxSpawnRandomTimeEnemy.Name, true);

            comboBoxSelectRandomTimeBegin.SelectedItem = file.get(SectionGeneralSettings, comboBoxSelectRandomTimeBegin.Name, Spawn.SpawnTime.DefaultBeginSec);
            comboBoxSelectRandomTimeEnd.SelectedItem = file.get(SectionGeneralSettings, comboBoxSelectRandomTimeEnd.Name, Spawn.SpawnTime.DefaultEndSec);

            comboBoxSelectAISkill.Text = file.get(SectionGeneralSettings, comboBoxSelectAISkill.Name, ESkillSet.Random.ToDescription());
            comboBoxSelectArtilleryTimeout.Text = file.get(SectionGeneralSettings, comboBoxSelectArtilleryTimeout.Name, DefaultString);
            comboBoxSelectArtilleryRhide.Text = file.get(SectionGeneralSettings, comboBoxSelectArtilleryRhide.Name, DefaultString);
            comboBoxSelectArtilleryZOffset.Text = file.get(SectionGeneralSettings, comboBoxSelectArtilleryZOffset.Name, DefaultString);
            comboBoxSelectShipSkill.Text = file.get(SectionGeneralSettings, comboBoxSelectShipSkill.Name, ESkillSetShip.Random.ToDescription());
            comboBoxSelectShipSleep.Text = file.get(SectionGeneralSettings, comboBoxSelectShipSleep.Name, DefaultString);
            comboBoxSelectShipSlowfire.Text = file.get(SectionGeneralSettings, comboBoxSelectShipSlowfire.Name, DefaultString);

            checkBoxAutoReArm.IsChecked = file.get(SectionGeneralSettings, checkBoxAutoReArm.Name, false);
            checkBoxAutoReFuel.IsChecked = file.get(SectionGeneralSettings, checkBoxAutoReFuel.Name, false);
            checkBoxTrackRecording.IsChecked = file.get(SectionGeneralSettings, checkBoxTrackRecording.Name, false);

            comboBoxSelectBattleTimeBegin.Text = file.get(SectionGeneralSettings, comboBoxSelectBattleTimeBegin.Name, MissionTime.ToString(MissionTime.Begin));
            comboBoxSelectBattleTimeEnd.Text = file.get(SectionGeneralSettings, comboBoxSelectBattleTimeEnd.Name, MissionTime.ToString(MissionTime.End));
        }

        public static void SelectReadValue(ISectionFile file, string section, ComboBox comboBox)
        {
            string val = file.get(section, comboBox.Name, string.Empty);
            comboBox.Text = string.IsNullOrEmpty(val) ? comboBox.Items.Count > 0 ? comboBox.Items[0] is ComboBoxItem ? (comboBox.Items[0] as ComboBoxItem).Content as string : comboBox.Items[0].ToString() : string.Empty : val;
        }
    }
}