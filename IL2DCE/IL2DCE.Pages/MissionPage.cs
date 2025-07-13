// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using IL2DCE.Pages.Controls;
using IL2DCE.Util;
using maddox.game;
using maddox.game.play;
using maddox.GP;
using Microsoft.Win32;

namespace IL2DCE.Pages
{
    public class MissionPage : PageDefImpl
    {
        #region Definition

        #region Constant

        public const string MsgMission = "Mission";
        public const string MsgCampaign = "Campaign";
        public const string DlgFileNameMissionFileFilter = "Mission File(.mis)|*.mis";
        public const string DlgFileNameMissionFolderFilter = "Mission Folder|.";
        public const string DlgFileNameCampaignFileFilter = "Campaign Setting|Campaign.ini";
        public const string DlgFileNameCampaignFolderFilter = "Campaign Folder|.";

        public const string MissionDefaultFormat = "Mission Default: {0}";
        public const string MissionDefaultDateFormat = "Mission Default: {0:M/d/yyyy} - {1:M/d/yyyy}";
        public const string MapFormat = "Map: {0}";

        #endregion

        class ImportMissionInfo
        {
            public bool IsCampaign
            {
                get;
                set;
            }

            public IEnumerable<string> FilePaths
            {
                get;
                set;
            }

            public IEnumerable<string> SorceFolderFileName
            {
                get;
                set;
            }

            public IEnumerable<string> SorceFolderFolderName
            {
                get;
                set;
            }
        }

        #endregion

        #region Property

        protected IGame Game
        {
            get
            {
                return _game;
            }
        }
        private IGame _game;

        protected virtual CampaignInfo SelectedCampaign
        {
            get
            {
                Debug.Assert(false);
                return null;
            }
        }

        protected virtual string SelectedCampaignMission
        {
            get
            {
                Debug.Assert(false);
                return null;
            }
        }

        protected virtual int SelectedArmyIndex
        {
            get
            {
                Debug.Assert(false);
                return -1;
            }
        }

        protected virtual int SelectedAirForceIndex
        {
            get
            {
                Debug.Assert(false);
                return -1;
            }
        }

        protected virtual AirGroup SelectedAirGroup
        {
            get
            {
                Debug.Assert(false);
                return null;
            }
        }

        #endregion

        #region Variable

        protected MissionFile currentMissionFile = null;
        protected bool missionLoaded = false;

        #endregion

        public MissionPage(string name, FrameworkElement fe)
            : base(name, fe)
        {

        }

        public override void _enter(maddox.game.IGame play, object arg)
        {
            base._enter(play, arg);

            _game = play as IGame;
        }

        public override void _leave(maddox.game.IGame play, object arg)
        {
            base._leave(play, arg);

            _game = null;
        }

        #region Import and Convert Mission File

        protected void ImportMission(object sender, RoutedEventArgs e)
        {
            if (IsConfirmImportMission())
            {
                Config config = Game.Core.Config;
                ProgressWindowModel model = new ProgressWindowModel();
                model.Context = new ImportMissionInfo() { IsCampaign = false, FilePaths = new string[0], SorceFolderFileName = config.SorceFolderFileName, SorceFolderFolderName = config.SorceFolderFolderName };
                ImportMissionProgressWindow(model, false);
            }
        }

        protected void ImportMissionFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select Missoion File";
            dlg.InitialDirectory = Game.gameInterface.ToFileSystemPath(Config.UserFolder);
            dlg.FileName = "*.mis";
            dlg.CheckFileExists = true;
            dlg.DefaultExt = Config.MissionFileExt;
            dlg.Filter = DlgFileNameMissionFileFilter;
            bool? result = dlg.ShowDialog();
            if (result != null && result.Value)
            {
                if (IsConfirmImportMission())
                {
                    string path = dlg.FileName;
                    ProgressWindowModel model = new ProgressWindowModel();
                    model.Context = new ImportMissionInfo() { IsCampaign = false, FilePaths = new string[] { path }, SorceFolderFileName = new string[0], SorceFolderFolderName = new string[0] };
                    ImportMissionProgressWindow(model, false);
                }
            }
        }

        protected void ImportMissionFolder(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select Missoion Folder";
            dlg.InitialDirectory = Game.gameInterface.ToFileSystemPath(Config.UserMissionsDefaultFolder);
            dlg.FileName = dlg.Title;
            dlg.CheckFileExists = false;
            dlg.DefaultExt = ".";
            dlg.Filter = DlgFileNameMissionFolderFilter;
            bool? result = dlg.ShowDialog();
            if (result != null && result.Value)
            {
                if (IsConfirmImportMission())
                {
                    string path = Path.GetDirectoryName(dlg.FileName);
                    ProgressWindowModel model = new ProgressWindowModel();
                    model.Context = new ImportMissionInfo() { IsCampaign = false, FilePaths = new string[0], SorceFolderFileName = new string[0], SorceFolderFolderName = new string[] { path } };
                    ImportMissionProgressWindow(model, false);
                }
            }
        }

        protected void ImportCampaignFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select Campaign File";
            dlg.InitialDirectory = Game.gameInterface.ToFileSystemPath(string.Format(Config.MissionFolderFormatCampaign, "bob"));
            dlg.FileName = MissionFileConverter.DefaultCampaignFileSearchPettern;
            dlg.CheckFileExists = true;
            dlg.DefaultExt = Config.IniFileExt;
            dlg.Filter = DlgFileNameCampaignFileFilter;
            bool? result = dlg.ShowDialog();
            if (result != null && result.Value)
            {
                if (IsConfirmImportMission())
                {
                    string path = dlg.FileName;
                    ProgressWindowModel model = new ProgressWindowModel();
                    model.Context = new ImportMissionInfo() { IsCampaign = true, FilePaths = new string[] { path }, SorceFolderFileName = new string[0], SorceFolderFolderName = new string[0] };
                    ImportMissionProgressWindow(model, true);
                }
            }
        }

        protected void ImportCampaignFolder(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select Campaign Folder";
            dlg.InitialDirectory = Game.gameInterface.ToFileSystemPath(string.Format(Config.MissionFolderFormatCampaign, "bob"));
            dlg.FileName = dlg.Title;
            dlg.CheckFileExists = false;
            dlg.DefaultExt = ".";
            dlg.Filter = DlgFileNameCampaignFolderFilter;
            bool? result = dlg.ShowDialog();
            if (result != null && result.Value)
            {
                if (IsConfirmImportMission())
                {
                    string path = Path.GetDirectoryName(dlg.FileName);
                    ProgressWindowModel model = new ProgressWindowModel();
                    model.Context = new ImportMissionInfo() { IsCampaign = true, FilePaths = new string[0], SorceFolderFileName = new string[0], SorceFolderFolderName = new string[] { path } };
                    ImportMissionProgressWindow(model, true);
                }
            }
        }

        protected bool IsConfirmImportMission()
        {
            return MessageBox.Show("The system will convert and import existing mission files in the CloD folder for use in IL2DCE.\n" +
                "The copyright of files converted by this process belongs to the original author, not you, and they cannot be distributed or shared without the consent of the original author.\n" +
                "The converted files can only be used by you and on this PC.\n" +
                "\nDo you agree to this ?",
                "Confimation [IL2DCE]",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        protected virtual void ImportMissionProgressWindow(ProgressWindowModel model, bool isCampaign)
        {
            ProgressWindow window = new ProgressWindow(model, BackgrowndWorkerEventHandler);
            window.Title = string.Format("{0} file Conversion and Import in progress ... [IL2DCE]", isCampaign ? MsgCampaign : MsgMission);
            bool? result = window.ShowDialog();

            object[] results = model.Result as object[];


            int completed = (int)results[1];

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} to Import {1}s", window.IsCanceled ? "Canceled" : "Completed", isCampaign ? MsgCampaign : MsgMission);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("Total files: {0}", (int)results[0]);
            sb.AppendLine();
            sb.AppendFormat("  Completed: {0}", completed);
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

            if (completed > 0)
            {
                Game.Core.ReadCampaignInfo();
                Game.Core.ReadCareerInfo();
            }
        }

        protected void BackgrowndWorkerEventHandler(object sender, BackgrowndWorkerEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            DoWorkEventArgs args = e.Args;
            ProgressWindowModel model = args.Argument as ProgressWindowModel;

            MissionFileConverter converter = new MissionFileConverter(Game.gameInterface, null, null, worker);
            ImportMissionInfo importMissionInfo = model.Context as ImportMissionInfo;

#if DEBUG && false
                string destFolder = string.Format("{0}/{1}", Config.HomeFolder, "CampaignsImported");
#else
            string destFolder = Config.CampaignsFolderDefault;
#endif
            string logFileSystemPath = string.Empty;
            int files = 0;
            int error = 0;
            try
            {
                bool IsCampaign = importMissionInfo.IsCampaign;
                IEnumerable<string> filePaths = importMissionInfo.FilePaths.Distinct().OrderBy(x => x);
                IEnumerable<string> filesFileType = converter.GetFiles(importMissionInfo.SorceFolderFileName, IsCampaign ? MissionFileConverter.DefaultCampaignFileSearchPettern : MissionFileConverter.DefaultMissionFileSearchPettern).Distinct().OrderBy(x => x);
                IEnumerable<string> filesFolderType = converter.GetFiles(importMissionInfo.SorceFolderFolderName, IsCampaign ? MissionFileConverter.DefaultCampaignFileSearchPettern : MissionFileConverter.DefaultMissionFileSearchPettern).Distinct().OrderBy(x => x);
                filesFileType = filesFileType.Concat(filePaths);
                files = IsCampaign ? filesFileType.Select(x => converter.CountCampaignMissionFiles(x)).Sum() + filesFolderType.Select(x => converter.CountCampaignMissionFiles(x)).Sum() : 
                                        filesFileType.Count() + filesFolderType.Count();

                logFileSystemPath = Game.gameInterface.ToFileSystemPath(string.Format("{0}/{1}", Config.UserMissionsFolder, Config.ConvertLogFileName));
                FileUtil.BackupFiles(logFileSystemPath, 5, false);
                logFileSystemPath = FileUtil.CreateWritablePath(logFileSystemPath, 10);

                using (FileStream stream = new FileStream(logFileSystemPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        bool result;
                        string name;
                        worker.ReportProgress(-1, string.Format(Config.NumberFormat, "{0}|{1}", 0, files));
                        foreach (var item in filesFileType)
                        {
                            if (worker.CancellationPending)
                            {
                                args.Cancel = true;
                                break;
                            }
                            name = Path.GetFileNameWithoutExtension(item.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar).LastOrDefault());
                            converter.ErrorWarnMsg.Clear();
                            error += (result = IsCampaign ? converter.ConvertCampaign(item, string.Empty, destFolder): converter.ConvertSystemPath(item, name, destFolder)) ? 0 : 1;
                            WriteConvertLog(writer, result, name, item, converter.ErrorWarnMsg, IsCampaign);
                        }
                        foreach (var item in filesFolderType)
                        {
                            if (worker.CancellationPending)
                            {
                                args.Cancel = true;
                                break;
                            }
                            converter.ErrorWarnMsg.Clear();
                            string[] str = item.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
                            if (str.Length >= 2)
                            {
                                name = string.Format("{0}_{1}", str[str.Length - 2], Path.GetFileNameWithoutExtension(str[str.Length - 1]));
                                error += (result = IsCampaign ? converter.ConvertCampaign(item, string.Empty, destFolder) : converter.ConvertSystemPath(item, name, destFolder)) ? 0 : 1;
                                WriteConvertLog(writer, result, name, item, converter.ErrorWarnMsg, IsCampaign);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("{0} - {1} {2} {3}", "MissionPage.buttonImportMissi_Click", ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                Core.WriteLog(message);
                MessageBox.Show(string.Format("{0}", ex.Message), Config.AppName, MessageBoxButton.OK, MessageBoxImage.Stop);
            }

            model.Result = new object[] { files, converter.CovertedMissionCount, error, logFileSystemPath, };
        }

        private void WriteConvertLog(StreamWriter writer, bool result, string name, string path, IEnumerable<string> errorMsgs, bool isCampaign)
        {
            // Result,Name(ID),FilePath,Error&Warn
            const string ResultsSuccess = "Success";
            const string ResultsFail = "Fail";
            const string ResultsWarn = "Warning";
            if (isCampaign)
            {
                bool isErrors = errorMsgs.Any(x => x.Split(MissionFileConverter.SplitChars, StringSplitOptions.RemoveEmptyEntries).Length > 2);
                writer.WriteLine("\"{0}\",\"{1}\",\"{2}\",\"{3}\"", result ? isErrors ? ResultsWarn : ResultsSuccess : ResultsFail, name, path, errorMsgs.Count().ToString(Config.NumberFormat));
                foreach (var item in errorMsgs)
                {
                    string[] strs = item.Split(MissionFileConverter.SplitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length >= 2)
                    {
                        string errorMsg = string.Join("|", strs.Take(strs.Length - 1).Skip(1).Select(x => x.Replace("\"", "\"\"")));
                        writer.WriteLine("\"{0}\",\"{1}\",\"{2}\",\"{3}\"", strs.Length == 2 ? ResultsSuccess : string.Compare(strs.Last(), "0") == 0 ? ResultsWarn : ResultsFail, strs.First(), string.Empty, errorMsg);
                    }
                    else
                    {
                        writer.WriteLine("\"{0}\",\"{1}\",\"{2}\",\"{3}\"", ResultsFail, string.Empty, string.Empty, item.Replace("\"", "\"\"").TrimEnd(MissionFileConverter.SplitChars));
                    }
                }
            }
            else
            {
                string errorMsg = string.Join("|", errorMsgs.Select(x => string.Join("|", x.Split(MissionFileConverter.SplitChars, StringSplitOptions.RemoveEmptyEntries).Select(y => y.Replace("\"", "\"\"").TrimEnd(MissionFileConverter.SplitChars)))));
                writer.WriteLine("\"{0}\",\"{1}\",\"{2}\",\"{3}\"", result ? errorMsg.Any() ? ResultsWarn : ResultsSuccess : ResultsFail, name, path, errorMsg);
            }
        }

        #endregion

        protected virtual bool Reload(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Your current selections will be lost.\nDo you want to reload this page ?", "Confimation [IL2DCE]",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Game.Core.ReadCampaignInfo();
                Game.Core.ReadCareerInfo();

                return true;
            }
            return false;
        }

        protected virtual void MissionLoad(object sender, RoutedEventArgs e)
        {
            string missionFile = SelectedCampaignMission;
            if (!string.IsNullOrEmpty(missionFile))
            {
                GameIterface gameIterface = Game.gameInterface;
                if (gameIterface.BattleIsRun())
                {
                    gameIterface.BattleStop();
                }
                try
                {
                    gameIterface.AppPartsLoad(gameIterface.AppParts().Where(x => !gameIterface.AppPartIsLoaded(x)).ToList());
                    gameIterface.MissionLoad(missionFile);
                    missionLoaded = true;
                    UpdateAirGroupContent();
                    gameIterface.BattleStart();
                    gameIterface.BattleStop();
                }
                catch (Exception ex)
                {
                    string message = string.Format("{0} - {1} {2} {3}", "MissionPage.MissionLoad", ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                    Core.WriteLog(message);
                }
            }
        }

        protected void UpdateAirGroupComboBoxInfo(ComboBox comboBox)
        {
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
                            comboBox.Items.Add(new ComboBoxItem()
                            {
                                Tag = airGroup,
                                Content = missionLoaded ? CreateAirGroupContent(airGroup, campaignInfo) : CreateAirGroupContent(airGroup, campaignInfo, string.Empty, aircraftInfo)
                            });
                        }
                    }
                }
            }

            EnableSelectItem(comboBox, selected, currentMissionFile == null);
        }

        protected virtual void UpdateAirGroupContent()
        {
            ;
        }

        protected void UpdateAirGroupContent(ComboBox comboBox)
        {
            CampaignInfo campaignInfo = SelectedCampaign;
            foreach (ComboBoxItem item in comboBox.Items)
            {
                AirGroup airGroup = item.Tag as AirGroup;
                item.Content = CreateAirGroupContent(airGroup, campaignInfo);
            }
        }

        protected string CreateAirGroupContent(AirGroup airGroup, CampaignInfo campaignInfo)
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

        protected string CreateAirGroupContent(AirGroup airGroup, CampaignInfo campaignInfo, string airportName, AircraftInfo aircraftInfo = null, double distance = -1)
        {
            if (aircraftInfo == null)
            {
                aircraftInfo = campaignInfo.GetAircraftInfo(airGroup.Class);
            }
            return string.Format(Config.NumberFormat, "{0} ({1}){2}{3}",
                    airGroup.DisplayName, aircraftInfo.DisplayName, airGroup.Airstart ? " [AIRSTART]" : string.IsNullOrEmpty(airportName) ? string.Empty : string.Format(" [{0}]", airportName),
                    distance >= 0 ? string.Format(Config.NumberFormat, " {0:F2}km", distance / 1000) : string.Empty);
        }

        protected void UpdateRankComboBoxInfo(ComboBox comboBox)
        {
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

        protected void UpdateSelectDefaultAirGroupLabel(Label label)
        {
            string content;
            CampaignInfo campaignInfo = SelectedCampaign;
            if (currentMissionFile != null && campaignInfo != null)
            {
                AirGroup airGroup = currentMissionFile.AirGroups.Where(x => string.Compare(x.SquadronName, AirGroup.CreateSquadronName(currentMissionFile.Player), true) == 0).FirstOrDefault();
                if (airGroup != null)
                {
                    AirForce airForce = AirForces.Default.Where(x => x.ArmyIndex == airGroup.ArmyIndex && x.AirForceIndex == airGroup.AirGroupInfo.AirForceIndex).FirstOrDefault();
                    content = string.Format("{0} - {1} [{2}]", missionLoaded ? 
                                                                CreateAirGroupContent(airGroup, campaignInfo) : 
                                                                CreateAirGroupContent(airGroup, campaignInfo, string.Empty, campaignInfo.GetAircraftInfo(airGroup.Class)),
                                                                ((EArmy)airGroup.ArmyIndex).ToString(), 
                                                                airForce.Name);
                }
                else 
                {
                    content = string.Empty;
                }
            }
            else
            {
                content = string.Empty;
            }
            label.Content = string.Format(MissionDefaultFormat, content);
        }

        protected void UpdateSkillComboBoxInfo(ComboBox comboBox, Label label)
        {
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
            label.Content = defaultString;

            EnableSelectItem(comboBox, selected, SelectedAirGroup == null);
        }

        protected void UpdateMapNameInfo(Label label)
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
            label.Content = string.Format(MapFormat, mapName);
        }

        protected void EnableSelectItem(ComboBox comboBox, string selected, bool forceDisable = false)
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

        protected void UpdateAircraftImage(AircraftImageBorder border)
        {
            AirGroup airGroup = SelectedAirGroup;
            border.DisplayImage(Game.gameInterface, airGroup != null ? airGroup.Class : string.Empty);
        }
    }
}
