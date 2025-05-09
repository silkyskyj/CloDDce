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
using IL2DCE.MissionObjectModel;
using IL2DCE.Pages.Controls;
using IL2DCE.Util;
using maddox.game;
using maddox.game.play;
using Microsoft.Win32;

namespace IL2DCE.Pages
{
    public class MissionPage : PageDefImpl
    {
        #region Definition

        #region Constant

        public const string DlgFileNameMission = "Mission File";
        public const string DlgFileNameMissionFilter = "Mission File(.mis)|*.mis";

        public const string MissionDefaultFormat = "Mission Default: {0}";

        #endregion

        class ImportMissionInfo
        {
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
                model.Context = new ImportMissionInfo() { FilePaths = new string[0], SorceFolderFileName = config.SorceFolderFileName, SorceFolderFolderName = config.SorceFolderFolderName };
                ImportMissionProgressWindow(model);
            }
        }

        protected void ImportMissionFolder(object sender, RoutedEventArgs e)
        {
            const string title = "Select Missoion Folder";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = title;
            dlg.InitialDirectory = Game.gameInterface.ToFileSystemPath(Config.UserMissionsDefaultFolder);
            dlg.FileName = title;
            dlg.CheckFileExists = false;
            dlg.DefaultExt = ".";
            dlg.Filter = "Folder|.";
            bool? result = dlg.ShowDialog();
            if (result != null && result.Value)
            {
                if (IsConfirmImportMission())
                {
                    string path = Path.GetDirectoryName(dlg.FileName);
                    ProgressWindowModel model = new ProgressWindowModel();
                    model.Context = new ImportMissionInfo() { FilePaths = new string[0], SorceFolderFileName = new string[0], SorceFolderFolderName = new string[] { path } };
                    ImportMissionProgressWindow(model);
                }
            }
        }

        protected void ImportMissionFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Game.gameInterface.ToFileSystemPath(Config.UserFolder);
            // dlg.FileName = "*.mis";
            dlg.DefaultExt = Config.MissionFileExt;
            dlg.Filter = DlgFileNameMissionFilter;
            bool? result = dlg.ShowDialog();
            if (result != null && result.Value)
            {
                if (IsConfirmImportMission())
                {
                    string path = dlg.FileName;
                    ProgressWindowModel model = new ProgressWindowModel();
                    model.Context = new ImportMissionInfo() { FilePaths = new string[] { path }, SorceFolderFileName = new string[0], SorceFolderFolderName = new string[0] };
                    ImportMissionProgressWindow(model);
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

        protected virtual void ImportMissionProgressWindow(ProgressWindowModel model)
        {
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
        }

        protected void BackgrowndWorkerEventHandler(object sender, BackgrowndWorkerEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            DoWorkEventArgs args = e.Args;
            ProgressWindowModel model = args.Argument as ProgressWindowModel;

            MissionFileConverter converter = new MissionFileConverter(Game.gameInterface);
            ImportMissionInfo importMissionInfo = model.Context as ImportMissionInfo;

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
                IEnumerable<string> filePaths = importMissionInfo.FilePaths.Distinct().OrderBy(x => x);
                IEnumerable<string> filesFileType = converter.GetFiles(importMissionInfo.SorceFolderFileName).Distinct().OrderBy(x => x);
                IEnumerable<string> filesFolderType = converter.GetFiles(importMissionInfo.SorceFolderFolderName).Distinct().OrderBy(x => x);
                filesFileType = filesFileType.Concat(filePaths);
                files = filesFileType.Count() + filesFolderType.Count();

                logFileSystemPath = Game.gameInterface.ToFileSystemPath(string.Format("{0}/{1}", Config.UserMissionsFolder, Config.ConvertLogFileName));
                FileUtil.BackupFiles(logFileSystemPath, 5, false);
                logFileSystemPath = FileUtil.CreateWritablePath(logFileSystemPath, 10);

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
                string message = string.Format("{0} - {1} {2} {3}", "MissionPage.buttonImportMissi_Click", ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
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
                    gameIterface.MissionLoad(campaignInfo.InitialMissionTemplateFiles.First());
                    missionLoaded = true;
                    UpdateAirGroupContent();
                    gameIterface.BattleStart();
                    gameIterface.BattleStop();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        protected virtual void UpdateAirGroupContent()
        {
            ;
        }
    }
}
