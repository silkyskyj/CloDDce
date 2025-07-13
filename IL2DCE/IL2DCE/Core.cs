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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game;

namespace IL2DCE
{
    public class Core
    {
        #region Property

        public Config Config
        {
            get
            {
                return config;
            }
        }
        private Config config;

        public Career CurrentCareer
        {
            get
            {
                return currentCareer;
            }
            set
            {
                if (currentCareer != value)
                {
                    currentCareer = value;
                }
            }
        }
        private Career currentCareer;

        public IList<Career> AvailableCareers
        {
            get
            {
                return availableCareers;
            }
        }
        private IList<Career> availableCareers = new List<Career>();

        public IList<CampaignInfo> CampaignInfos
        {
            get
            {
                return campaigns;
            }
        }
        private IList<CampaignInfo> campaigns = new List<CampaignInfo>();

        public IGamePlay GamePlay
        {
            get
            {
                return gamePlay;
            }
        }
        private IGamePlay gamePlay;

        public IRandom Random
        {
            get
            {
                return random;
            }
        }
        private IRandom random;

        public AMission Mission
        {
            get;
            set;
        }

        #endregion

        private static StreamWriter writerLog;
        private static object writerLogObject = new object();

        private string debugFolderSystemPath;
        private string careersFolderSystemPath;
        private string campaignsFolderSystemPath;

        #region Constructor

        public Core(IGame game)
            : this(game, new Random())
        {
        }

        public Core(IGame game, IRandom random)
        {
            gamePlay = game;
            this.random = random;

            Initialize();

            if (writerLog == null)
            {
                string path = (GamePlay as IGame).gameInterface.ToFileSystemPath(string.Format("{0}/{1}", Config.UserMissionsFolder, Config.LogFileName));
                FileUtil.BackupFiles(path, 5, false);
                path = FileUtil.CreateWritablePath(path, 10);
                writerLog = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
            }

            GameIterface gameInterface = game.gameInterface;

            // Config
            ISectionFile confFile = gameInterface.SectionFileLoad(Config.ConfigFilePath);
            config = new Config(confFile);

            // CampaignInfo
            ReadCampaignInfo();

            // Career
            ReadCareerInfo();

            debugFolderSystemPath = gameInterface.ToFileSystemPath(string.Format("{0}/{1}", Config.UserMissionsFolder, Config.DebugFolderName));
        }

        #endregion

        private void Initialize()
        {
            GameIterface gameInterface = (GamePlay as IGame).gameInterface;
            string userMissionFolderSystemPath = gameInterface.ToFileSystemPath(Config.UserMissionFolder);
            if (!Directory.Exists(userMissionFolderSystemPath))
            {
                Directory.CreateDirectory(userMissionFolderSystemPath);
            }
            string userMissionsFolderSystemPath = gameInterface.ToFileSystemPath(Config.UserMissionsFolder);
            if (!Directory.Exists(userMissionsFolderSystemPath))
            {
                Directory.CreateDirectory(userMissionsFolderSystemPath);
            }
        }

        public void ReadCampaignInfo()
        {
            CampaignInfos.Clear();
            GameIterface gameInterface = (GamePlay as IGame).gameInterface;
            string campaignsFolderPath = Config.CampaignsFolderDefault;
            this.campaignsFolderSystemPath = gameInterface.ToFileSystemPath(Config.CampaignsFolderDefault);
            DirectoryInfo directoryInfo = new DirectoryInfo(this.campaignsFolderSystemPath);
            if (directoryInfo.Exists && directoryInfo.GetDirectories().Length > 0)
            {
                ISectionFile globalAircraftInfoFile = gameInterface.SectionFileLoad(string.Format("{0}/{1}", campaignsFolderPath, Config.AircraftInfoFileName));
#if DEBUG && false
                AircraftInfo.TraceAircraftInfo(globalAircraftInfoFile, string.Format("{0}/{1}", campaignsFolderPath, Config.AircraftInfoFileName));
#endif
                ISectionFile globalAirGroupInfoFile = gameInterface.SectionFileLoad(string.Format("{0}/{1}", campaignsFolderPath, Config.AirGroupInfoFileName));
                AirGroupInfos.Default = AirGroupInfos.Create(globalAirGroupInfoFile);
                foreach (DirectoryInfo campaignFolderSystemPath in directoryInfo.GetDirectories())
                {
                    FileInfo[] fileInfo = campaignFolderSystemPath.GetFiles(Config.CampaignInfoFileName);
                    if (fileInfo.Length == 1)
                    {
                        string campaignFolder = string.Format("{0}/{1}/", campaignsFolderPath, campaignFolderSystemPath.Name);
                        ISectionFile campaignInfoFile = gameInterface.SectionFileLoad(campaignFolder + Config.CampaignInfoFileName);

                        ISectionFile localAircraftInfoFile = null;
                        if (File.Exists(gameInterface.ToFileSystemPath(campaignFolder + Config.AircraftInfoFileName)))
                        {
                            localAircraftInfoFile = gameInterface.SectionFileLoad(campaignFolder + Config.AircraftInfoFileName);
                        }
                        AirGroupInfos localAirGroupInfos = null;
                        if (File.Exists(gameInterface.ToFileSystemPath(campaignFolder + Config.AirGroupInfoFileName)))
                        {
                            ISectionFile localAirGroupInfoFile = gameInterface.SectionFileLoad(campaignFolder + Config.AirGroupInfoFileName);
                            localAirGroupInfos = AirGroupInfos.Create(localAirGroupInfoFile);
                        }

                        CampaignInfo campaignInfo = new CampaignInfo(campaignFolderSystemPath.Name, campaignFolder, campaignInfoFile, globalAircraftInfoFile, localAircraftInfoFile, localAirGroupInfos);
                        CampaignInfos.Add(campaignInfo);
                    }
                }
            }
        }

        public void ReadCareerInfo()
        {
            AvailableCareers.Clear();

            GameIterface gameInterface = (GamePlay as IGame).gameInterface;

            careersFolderSystemPath = gameInterface.ToFileSystemPath(Config.UserMissionFolder);
            DirectoryInfo careersFolder = new DirectoryInfo(careersFolderSystemPath);
            if (careersFolder.Exists)
            {
                foreach (DirectoryInfo careerFolder in careersFolder.GetDirectories())
                {
                    FileInfo[] fileInfo = careerFolder.GetFiles(Config.CareerInfoFileName);
                    if (fileInfo.Length == 1)
                    {
                        string path = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, careerFolder.Name, Config.CareerInfoFileName);
                        ISectionFile careerFile = gameInterface.SectionFileLoad(path);
                        try
                        {
                            Career career = new Career(careerFolder.Name, CampaignInfos, careerFile, Config, fileInfo.First().LastWriteTime);
                            AvailableCareers.Add(career);
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format("Error: read & parse Career file [{0}] {1} {2}", path, ex.Message, ex.StackTrace);
                            WriteLog(message);
                        }
                    }                                                               
                }
            }
        }

        public void InitCampaign()
        {

        }

        public void ResetCampaign(IGame game)
        {
            // Reset campaign state
            CurrentCareer.InitializeDateTime();

            AdvanceCampaign(game);
        }

        public ECampaignStatus AdvanceCampaign(IGame game)
        {
            ECampaignStatus result;
            Career career = CurrentCareer;
            Generator.Generator generator = new Generator.Generator(GamePlay, Random, Config, career);
            CampaignInfo campaignInfo = career.CampaignInfo;
            GameIterface gameInterface = game.gameInterface;

            if (!career.Date.HasValue)
            {
                result = ECampaignStatus.Empty;
                // It is the first mission.
                career.InitializeDateTime(Config, Random);
                career.InitializeExperience();
                career.InitializeProgressMission();
            }
            else
            {
                if (game is IGameSingle)
                {
                    IGameSingle gameSingle = game as IGameSingle;
                    career.UpdateExperience(gameSingle.BattleResult);
                }

                if (career.StrictMode && career.Status == (int)EPlayerStatus.Dead)
                {
                    result = ECampaignStatus.Dead;
                }
                else if (!career.IsProgressNextMission())
                {
                    career.ProgressEndMission();
                    result = ECampaignStatus.ProgressEnd;
                }
                else if (career.Date >= career.EndDate)
                {
                    result = ECampaignStatus.DateEnd;
                }
                else
                {
                    result = ECampaignStatus.InProgress;
                    career.ProgressDateTime(Config, Random);
                }
            }

            string missionFolderSystemPath = string.Format("{0}\\{1}", careersFolderSystemPath, career.PilotName);
            if (!Directory.Exists(missionFolderSystemPath))
            {
                Directory.CreateDirectory(missionFolderSystemPath);
            }

            ISectionFile careerFile = GamePlay.gpCreateSectionFile();
            string careerFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, career.PilotName, Config.CareerInfoFileName);

            if (gameInterface.BattleIsRun())
            {
                // Stop the currntly running battle.
                gameInterface.BattleStop();
            }

            if (result != ECampaignStatus.DateEnd && result != ECampaignStatus.Dead && result != ECampaignStatus.ProgressEnd)
            {
                DateTime dt = career.Date.Value;
                string missionId = string.Format(Config.NumberFormat,
                                            "{0}_{1:d4}-{2:d2}-{3:d2}_{4:d2}", campaignInfo.Id, dt.Year, dt.Month, dt.Day, dt.Hour);
                string missionFileName = string.Format("{0}/{1}/{2}{3}", Config.UserMissionFolder, career.PilotName, missionId, Config.MissionFileExt);

                // Load MissionStatus
                MissionStatus missionStatus = null;
                if (career.StrictMode)
                {
                    string missionStatusFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, career.PilotName, Config.MissionStatusResultFileName);
                    string missionStatusFileNameSystemPath = gameInterface.ToFileSystemPath(missionStatusFileName);
                    ISectionFile missionStatusFile = File.Exists(missionStatusFileNameSystemPath) ? gameInterface.SectionFileLoad(missionStatusFileName) : gameInterface.SectionFileCreate();
                    missionStatus = MissionStatus.Create(missionStatusFile, Random);

                    // ReinForce & Save
                    if (missionStatus != null)
                    {
                        generator.ReinForce(missionStatus, career.Date.Value);
                        missionStatus.WriteTo(missionStatusFile, true);
                        missionStatusFile.save(missionStatusFileName);
                    }
                }

                string missionTemplateFileName = career.ProgressNextMission(Random);
                ISectionFile missionTemplateFile = GamePlay.gpLoadSectionFile(missionTemplateFileName);
                string mapName = campaignInfo.ProgressMapName(dt);
                if (!string.IsNullOrEmpty(mapName))
                {
                    SilkySkyCloDFile.Write(missionTemplateFile, MissionFile.SectionMain, MissionFile.KeyMap, mapName, true);
                }

                // Preload mission file for path calculation.
                gameInterface.MissionLoad(missionTemplateFile);

                // Generate the next mission based on the template.
                ISectionFile missionFile = null;
                BriefingFile briefingFile = null;
                generator.GenerateMission(missionTemplateFile, missionStatus, out missionFile, out briefingFile);

                // Save mission file
                missionFile.save(missionFileName);
                career.MissionFileName = missionFileName;

                // Save briefing file
                string briefingFileSystemPath = string.Format(careersFolderSystemPath + "\\" + career.PilotName + "\\{0}{1}", missionId, Config.BriefingFileExt);
                briefingFile.SaveTo(briefingFileSystemPath, missionId);

                // Copy mission script file
                string scriptSourceFileSystemPath = string.Format("{0}\\{1}\\{2}", campaignsFolderSystemPath, campaignInfo.Id, campaignInfo.ScriptFileName);
                string scriptDestinationFileSystemPath = string.Format("{0}\\{1}\\{2}{3}", careersFolderSystemPath, career.PilotName, missionId, Config.ScriptFileExt);
                File.Copy(scriptSourceFileSystemPath, scriptDestinationFileSystemPath, true);

#if DEBUG
                Config.Debug = 1;
#endif
                if (Config.Debug >= 1)
                {
                    if (!Directory.Exists(this.debugFolderSystemPath))
                    {
                        Directory.CreateDirectory(this.debugFolderSystemPath);
                    }
                    missionTemplateFile.save(string.Format("{0}/{1}/{2}Template{3}", Config.UserMissionsFolder, Config.DebugFolderName, Config.DebugFileName, Config.MissionFileExt));
                    missionFile.save(string.Format("{0}/{1}/{2}{3}", Config.UserMissionsFolder, Config.DebugFolderName, Config.DebugFileName, Config.MissionFileExt));
                    briefingFile.SaveTo(string.Format("{0}\\{1}{2}", debugFolderSystemPath, Config.DebugFileName, Config.BriefingFileExt), missionId);
                    File.Copy(scriptSourceFileSystemPath, string.Format("{0}\\{1}", debugFolderSystemPath, Config.DebugFileName, Config.ScriptFileExt), true);
                }

                // Stop the preloaded battle to prevent a postload.
                gameInterface.BattleStop();
            }

            career.WriteTo(careerFile, Config.KillsHistoryMax, DateTime.Now);
            careerFile.save(careerFileName);

            return result;
        }

        public void CreateQuickMission(IGame game, Career career)
        {
            Generator.Generator generator = new Generator.Generator(GamePlay, Random, Config, CurrentCareer);

            CampaignInfo campaignInfo = career.CampaignInfo;
            GameIterface gameInterface = game.gameInterface;

            // career.InitializeDateTime(Config);
            career.InitializeExperience();

            string missionFolderSystemPath = string.Format("{0}\\{1}", careersFolderSystemPath, career.PilotName);
            if (!Directory.Exists(missionFolderSystemPath))
            {
                Directory.CreateDirectory(missionFolderSystemPath);
            }

            if (gameInterface.BattleIsRun())
            {
                gameInterface.BattleStop();
            }

            string missionId = campaignInfo.Id;
            string missionFileName = string.Format("{0}/{1}/{2}{3}", Config.UserMissionFolder, career.PilotName, missionId, Config.MissionFileExt);

            // string missionTemplateFileName = campaignInfo.InitialMissionTemplateFile;
            string missionTemplateFileName = career.MissionTemplateFile(null);
            ISectionFile missionTemplateFile = GamePlay.gpLoadSectionFile(missionTemplateFileName);
            string mapName = campaignInfo.ProgressMapName(career.Date.Value);
            if (!string.IsNullOrEmpty(mapName))
            {
                SilkySkyCloDFile.Write(missionTemplateFile, MissionFile.SectionMain, MissionFile.KeyMap, mapName, true);
            }

            // Preload mission file for path calculation.
            gameInterface.MissionLoad(missionTemplateFile);
            gameInterface.BattleStop();

            // Generate the next mission based on the new template.
            ISectionFile missionFile = null;
            BriefingFile briefingFile = null;
            generator.GenerateMission(missionTemplateFile, null, out missionFile, out briefingFile);

            // Save mission file
            missionFile.save(missionFileName);
            career.MissionFileName = missionFileName;

            // Save briefing file
            string briefingFileSystemPath = string.Format(careersFolderSystemPath + "\\" + career.PilotName + "\\{0}{1}", missionId, Config.BriefingFileExt);
            briefingFile.SaveTo(briefingFileSystemPath, missionId);

            // Copy mission script file
            string scriptSourceFileSystemPath = string.Format("{0}\\{1}\\{2}", campaignsFolderSystemPath, campaignInfo.Id, campaignInfo.ScriptFileName);
            string scriptDestinationFileSystemPath = string.Format("{0}\\{1}\\{2}{3}", careersFolderSystemPath, career.PilotName, missionId, Config.ScriptFileExt);
            File.Copy(scriptSourceFileSystemPath, scriptDestinationFileSystemPath, true);

#if DEBUG
            Config.Debug = 1;
#endif
            if (Config.Debug >= 1)
            {
                if (!Directory.Exists(this.debugFolderSystemPath))
                {
                    Directory.CreateDirectory(this.debugFolderSystemPath);
                }
                missionTemplateFile.save(string.Format("{0}/{1}/{2}Template{3}", Config.UserMissionsFolder, Config.DebugFolderName, Config.DebugFileName, Config.MissionFileExt));
                missionFile.save(string.Format("{0}/{1}/{2}{3}", Config.UserMissionsFolder, Config.DebugFolderName, Config.DebugFileName, Config.MissionFileExt));
                briefingFile.SaveTo(string.Format("{0}\\{1}{2}", debugFolderSystemPath, Config.DebugFileName, Config.BriefingFileExt), missionId);
                File.Copy(scriptSourceFileSystemPath, string.Format("{0}\\{1}{2}", debugFolderSystemPath, Config.DebugFileName, Config.ScriptFileExt), true);
            }

            string statsFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, career.PilotName, Config.StatsInfoFileName);
            ISectionFile statsFile = GamePlay.gpLoadSectionFile(statsFileName);
            if (statsFile != null)
            {
                career.ReadResult(statsFile);
            }
        }

        public void UpdateMissionResult(MissionStatus missionStatus)
        {
            GameIterface gameInterface = (GamePlay as IGame).gameInterface;
            string missionStatusFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, CurrentCareer.PilotName, Config.MissionStatusResultFileName);
            string missionStatusFileNameSystemPath = gameInterface.ToFileSystemPath(missionStatusFileName);
            ISectionFile missionStatusFile = File.Exists(missionStatusFileNameSystemPath) ? gameInterface.SectionFileLoad(missionStatusFileName): gameInterface.SectionFileCreate();
            missionStatus.UpdateWriteTo(missionStatusFile, Config.ReinForceDay, true);
            missionStatusFile.save(missionStatusFileName);
        }

        public void UpdateResult(Career career)
        {
            string statsFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, career.PilotName, Config.StatsInfoFileName);
            ISectionFile statsFile = GamePlay.gpCreateSectionFile();
            career.WriteResult(statsFile, Config.KillsHistoryMax);
            statsFile.save(statsFileName);
        }

        public void DeleteCareer(Career career)
        {
            AvailableCareers.Remove(career);
            if (CurrentCareer == career)
            {
                CurrentCareer = null;
            }

            List<DirectoryInfo> deleteFolders = new List<DirectoryInfo>();
            DirectoryInfo careersFolder = new DirectoryInfo(this.careersFolderSystemPath);
            if (careersFolder.Exists)
            {
                foreach (DirectoryInfo careerFolder in careersFolder.GetDirectories())
                {
                    if (career.PilotName == careerFolder.Name)
                    {
                        deleteFolders.Add(careerFolder);
                    }
                }
            }

            foreach (var item in deleteFolders)
            {
                try
                {
                    item.Delete(true);
                }
                catch (Exception ex)
                {
                    string message = string.Format("Error: delete folder [{0}] {1} {2}", item, ex.Message, ex.StackTrace);
                    WriteLog(message);
                }
            }
        }

        public static void WriteLog(string message)
        {
            Debug.WriteLine(message);
            lock (writerLogObject)
            {
                try
                {
                    writerLog.WriteLine("{0} \"{1}\"", DateTime.Now.ToString(Config.DateTimeDefaultLongLongFormat, Config.DateTimeFormat), message.Replace("\"", "\"\"").Replace("\n", "|"));
                    writerLog.Flush();
                }
                catch (Exception ex)
                {
                    string error = string.Format("Error: WriteLog {0} {1} [{2}]", ex.Message, ex.StackTrace, message);
                    Debug.WriteLine(error);
                }
            }
        }

#if DEBUG
        [Conditional("DEBUG")]
        public void SaveMissionResult(MissionStatus missionStatus)
        {
            string missionStatusFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, CurrentCareer.PilotName, Config.MissionStatusResultFileName);
            ISectionFile missionStatusFile = (GamePlay as IGame).gameInterface.SectionFileCreate();
            missionStatus.WriteTo(missionStatusFile, false);
            missionStatusFile.save(missionStatusFileName);
        }

        [Conditional("DEBUG")]
        public void SaveCurrentStatus(string fileName, string playerActorName, DateTime dateTime, bool forceCreate = false)
        {
            GameIterface gameInterface = (GamePlay as IGame).gameInterface;
            Career career = CurrentCareer;
            string careersFolderSystemPath = gameInterface.ToFileSystemPath(Config.UserMissionFolder);
            string missionFolderSystemPath = string.Format("{0}\\{1}", careersFolderSystemPath, career.PilotName);
            if (!Directory.Exists(missionFolderSystemPath))
            {
                Directory.CreateDirectory(missionFolderSystemPath);
            }
            // ISectionFile missionStatusFile = gameInterface.SectionFileCreate();
            string missionStatusFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, career.PilotName, fileName);
            ISectionFile missionStatusFile = forceCreate ? gameInterface.SectionFileCreate() : gameInterface.SectionFileLoad(missionStatusFileName);
            // MissionStatus.Update(missionStatusFile, GamePlay as IGame, playerActorName);
            MissionStatus missionStatus = new MissionStatus(Random, career.Date.Value);
            missionStatus.Update(GamePlay as IGame, GameEventId.Trigger, playerActorName, dateTime, false);
            missionStatus.WriteTo(missionStatusFile, true);
            missionStatusFile.save(missionStatusFileName);

            ISectionFile missionStatusFileLoad = gameInterface.SectionFileLoad(missionStatusFileName);
            missionStatus = MissionStatus.Create(missionStatusFileLoad, Random);
            Debug.Assert(missionStatus != null);
        }
#endif
    }
}