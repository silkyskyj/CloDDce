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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game;
using maddox.game.play;

namespace IL2DCE
{
    public class Core
    {
        #region Property

        public Config Config
        {
            get
            {
                return _config;
            }
        }
        private Config _config;

        public Career CurrentCareer
        {
            get
            {
                return _currentCareer;
            }
            set
            {
                if (_currentCareer != value)
                {
                    _currentCareer = value;
                }
            }
        }
        private Career _currentCareer;

        public IList<Career> AvailableCareers
        {
            get
            {
                return _availableCareers;
            }
        }
        private IList<Career> _availableCareers = new List<Career>();

        public IList<CampaignInfo> CampaignInfos
        {
            get
            {
                return _campaigns;
            }
        }
        private IList<CampaignInfo> _campaigns = new List<CampaignInfo>();

        public IGamePlay GamePlay
        {
            get
            {
                return _gamePlay;
            }
        }
        private IGamePlay _gamePlay;

        public IRandom Random
        {
            get
            {
                return _random;
            }
        }
        private IRandom _random;

        public AMission Mission
        {
            get;
            set;
        }

        #endregion

        private static StreamWriter writerLog;
        private static object writerLogObject = new object();

        private string _debugFolderSystemPath;
        private string _careersFolderSystemPath;
        private string _campaignsFolderSystemPath;

        #region Constructor

        public Core(IGame game)
            : this(game, new Random())
        {
        }

        public Core(IGame game, IRandom random)
        {
            _gamePlay = game;
            _random = random;

            Initialize();

            if (writerLog == null)
            {
                writerLog = new StreamWriter(new FileStream(CreatetLogFilePath(), FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
            }

            GameIterface gameInterface = game.gameInterface;

            // Config
            ISectionFile confFile = gameInterface.SectionFileLoad(Config.ConfigFilePath);
            _config = new Config(confFile);

            // CampaignInfo
            ReadCampaignInfo();

            // Career
            ReadCareerInfo();

            _debugFolderSystemPath = gameInterface.ToFileSystemPath(string.Format("{0}/{1}", Config.UserMissionsFolder, Config.DebugFolderName));
        }

        #endregion

        public void ReadCampaignInfo()
        {
            CampaignInfos.Clear();
            GameIterface gameInterface = (GamePlay as IGame).gameInterface;
            string campaignsFolderPath = Config.CampaignsFolderDefault;
            this._campaignsFolderSystemPath = gameInterface.ToFileSystemPath(Config.CampaignsFolderDefault);
            DirectoryInfo campaignsFolderSystemPath = new DirectoryInfo(_campaignsFolderSystemPath);
            if (campaignsFolderSystemPath.Exists && campaignsFolderSystemPath.GetDirectories().Length > 0)
            {
                ISectionFile globalAircraftInfoFile = gameInterface.SectionFileLoad(string.Format("{0}/{1}", campaignsFolderPath, Config.AircraftInfoFileName));
#if DEBUG && false
                AircraftInfo.TraceAircraftInfo(globalAircraftInfoFile, string.Format("{0}/{1}", campaignsFolderPath, Config.AircraftInfoFileName));
#endif
                ISectionFile globalAirGroupInfoFile = gameInterface.SectionFileLoad(string.Format("{0}/{1}", campaignsFolderPath, Config.AirGroupInfoFileName));
                AirGroupInfos.Default = AirGroupInfos.Create(globalAirGroupInfoFile);
                foreach (DirectoryInfo campaignFolderSystemPath in campaignsFolderSystemPath.GetDirectories())
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

            _careersFolderSystemPath = gameInterface.ToFileSystemPath(Config.UserMissionFolder);
            DirectoryInfo careersFolder = new DirectoryInfo(_careersFolderSystemPath);
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
                            Career career = new Career(careerFolder.Name, CampaignInfos, careerFile, Config);
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
                else if (career.Date >= campaignInfo.EndDate)
                {
                    result = ECampaignStatus.DateEnd;
                }
                else
                {
                    result = ECampaignStatus.InProgress;
                    career.ProgressDateTime(Config, Random);
                }
            }

            string missionFolderSystemPath = string.Format("{0}\\{1}", _careersFolderSystemPath, career.PilotName);
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

            if (result != ECampaignStatus.DateEnd && result != ECampaignStatus.Dead)
            {
                string missionTemplateFileName = campaignInfo.InitialMissionTemplateFiles.FirstOrDefault();
                // Preload mission file for path calculation.
                gameInterface.MissionLoad(missionTemplateFileName);

                DateTime dt = career.Date.Value;
                string missionId = string.Format(Config.NumberFormat,
                                            "{0}_{1:d4}-{2:d2}-{3:d2}_{4:d2}", campaignInfo.Id, dt.Year, dt.Month, dt.Day, dt.Hour);
                string missionFileName = string.Format("{0}/{1}/{2}{3}", Config.UserMissionFolder, career.PilotName, missionId, Config.MissionFileExt);
                career.MissionFileName = missionFileName;

                // Load MissionStatus
                string missionStatusFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, career.PilotName, Config.MissionStatusResultFileName);
                string missionStatusFileNameSystemPath = gameInterface.ToFileSystemPath(missionStatusFileName);
                ISectionFile missionStatusFile = File.Exists(missionStatusFileNameSystemPath) ? gameInterface.SectionFileLoad(missionStatusFileName) : gameInterface.SectionFileCreate();
                MissionStatus missionStatus = MissionStatus.Create(missionStatusFile, Random);

                // ReinForce
                generator.ReinForce(missionStatus, career.Date.Value);

                // Generate the next mission based on the template.
                ISectionFile missionTemplateFile = GamePlay.gpLoadSectionFile(missionTemplateFileName);
                ISectionFile missionFile = null;
                BriefingFile briefingFile = null;
                generator.GenerateMission(missionTemplateFile, missionId, missionStatus, out missionFile, out briefingFile);

                // Save mission file
                missionFile.save(missionFileName);

                // Copy mission script file
                string scriptSourceFileSystemPath = string.Format("{0}\\{1}\\{2}", _campaignsFolderSystemPath, campaignInfo.Id, campaignInfo.ScriptFileName);
                string scriptDestinationFileSystemPath = string.Format("{0}\\{1}\\{2}{3}", _careersFolderSystemPath, career.PilotName, missionId, Config.ScriptFileExt);
                File.Copy(scriptSourceFileSystemPath, scriptDestinationFileSystemPath, true);

                // Save briefing file
                string briefingFileSystemPath = string.Format(_careersFolderSystemPath + "\\" + career.PilotName + "\\{0}.briefing", missionId);
                briefingFile.SaveTo(briefingFileSystemPath);

#if DEBUG
                Config.Debug = 1;
#endif
                if (Config.Debug == 1)
                {
                    if (!Directory.Exists(this._debugFolderSystemPath))
                    {
                        Directory.CreateDirectory(this._debugFolderSystemPath);
                    }
                    missionTemplateFile.save(string.Format("{0}/{1}/{2}", Config.UserMissionsFolder, Config.DebugFolderName, Config.DebugMissionTemplateFileName));
                    missionFile.save(string.Format("{0}/{1}/{2}", Config.UserMissionsFolder, Config.DebugFolderName, Config.DebugMissionFileName));
                    briefingFile.SaveTo(string.Format("{0}\\{1}", _debugFolderSystemPath, Config.DebugBriefingFileName));
                    File.Copy(scriptSourceFileSystemPath, string.Format("{0}\\{1}", _debugFolderSystemPath, Config.DebugMissionScriptFileName), true);
                }
            }

            // Stop the preloaded battle to prevent a postload.
            gameInterface.BattleStop();

            career.WriteTo(careerFile, Config.KillsHistoryMax);
            careerFile.save(careerFileName);

            return result;
        }

        public void CreateQuickMission(IGame game, Career career)
        {
            Generator.Generator generator = new Generator.Generator(GamePlay, Random, Config, CurrentCareer);

            CampaignInfo campaignInfo = career.CampaignInfo;
            GameIterface gameInterface = game.gameInterface;

            career.InitializeDateTime(Config);
            career.InitializeExperience();

            string missionFolderSystemPath = string.Format("{0}\\{1}", _careersFolderSystemPath, career.PilotName);
            if (!Directory.Exists(missionFolderSystemPath))
            {
                Directory.CreateDirectory(missionFolderSystemPath);
            }

            if (gameInterface.BattleIsRun())
            {
                gameInterface.BattleStop();
            }

            string missionTemplateFileName = campaignInfo.InitialMissionTemplateFiles.FirstOrDefault();

            // Preload mission file for path calculation.
            gameInterface.MissionLoad(missionTemplateFileName);
            gameInterface.BattleStop();

            string missionId = campaignInfo.Id;
            string missionFileName = string.Format("{0}/{1}/{2}{3}", Config.UserMissionFolder, career.PilotName, missionId, Config.MissionFileExt);
            career.MissionFileName = missionFileName;

            // Generate the next mission based on the new template.
            ISectionFile missionTemplateFile = GamePlay.gpLoadSectionFile(missionTemplateFileName);
            ISectionFile missionFile = null;
            BriefingFile briefingFile = null;
            generator.GenerateMission(missionTemplateFile, missionId, null, out missionFile, out briefingFile);

            // Save mission file
            missionFile.save(missionFileName);

            // Copy mission script file
            string scriptSourceFileSystemPath = string.Format("{0}\\{1}\\{2}", _campaignsFolderSystemPath, campaignInfo.Id, campaignInfo.ScriptFileName);
            string scriptDestinationFileSystemPath = string.Format("{0}\\{1}\\{2}{3}", _careersFolderSystemPath, career.PilotName, missionId, Config.ScriptFileExt);
            File.Copy(scriptSourceFileSystemPath, scriptDestinationFileSystemPath, true);

            // Save briefing file
            string briefingFileSystemPath = string.Format(_careersFolderSystemPath + "\\" + career.PilotName + "\\{0}.briefing", missionId);
            briefingFile.SaveTo(briefingFileSystemPath);

#if DEBUG
            Config.Debug = 1;
#endif
            if (Config.Debug == 1)
            {
                if (!Directory.Exists(this._debugFolderSystemPath))
                {
                    Directory.CreateDirectory(this._debugFolderSystemPath);
                }
                missionTemplateFile.save(string.Format("{0}/{1}/{2}", Config.UserMissionsFolder, Config.DebugFolderName, Config.DebugMissionTemplateFileName));
                missionFile.save(string.Format("{0}/{1}/{2}", Config.UserMissionsFolder, Config.DebugFolderName, Config.DebugMissionFileName));
                briefingFile.SaveTo(string.Format("{0}\\{1}", _debugFolderSystemPath, Config.DebugBriefingFileName));
                File.Copy(scriptSourceFileSystemPath, string.Format("{0}\\{1}", _debugFolderSystemPath, Config.DebugMissionScriptFileName), true);
            }

            string statsFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, career.PilotName, Config.StatsInfoFileName);
            ISectionFile statsFile = GamePlay.gpLoadSectionFile(statsFileName);
            if (statsFile != null)
            {
                career.ReadResult(statsFile);
            }
        }

        public void SaveMissionResult(MissionStatus missionStatus)
        {
            string missionStatusFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, CurrentCareer.PilotName, Config.MissionStatusResultFileName);
            ISectionFile missionStatusFile = (GamePlay as IGame).gameInterface.SectionFileCreate();
            missionStatus.WriteTo(missionStatusFile);
            missionStatusFile.save(missionStatusFileName);
        }

        public void UpdateMissionResult(MissionStatus missionStatus)
        {
            GameIterface gameInterface = (GamePlay as IGame).gameInterface;
            string missionStatusFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, CurrentCareer.PilotName, Config.MissionStatusResultFileName);
            string missionStatusFileNameSystemPath = gameInterface.ToFileSystemPath(missionStatusFileName);
            ISectionFile missionStatusFile = File.Exists(missionStatusFileNameSystemPath) ? gameInterface.SectionFileLoad(missionStatusFileName): gameInterface.SectionFileCreate();
            missionStatus.UpdateWriteTo(missionStatusFile, Config.ReinForceDay);
            missionStatusFile.save(missionStatusFileName);
        }

        public void UpdateResult(Career career)
        {
            string statsFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, career.PilotName, Config.StatsInfoFileName);
            ISectionFile statsFile = GamePlay.gpCreateSectionFile();
            career.WriteResult(statsFile, Config.KillsHistoryMax);
            statsFile.save(statsFileName);
        }

        public void InitCampaign()
        {

        }

        public void DeleteCareer(Career career)
        {
            AvailableCareers.Remove(career);
            if (CurrentCareer == career)
            {
                CurrentCareer = null;
            }

            List<DirectoryInfo> deleteFolders = new List<DirectoryInfo>();
            DirectoryInfo careersFolder = new DirectoryInfo(this._careersFolderSystemPath);
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

        private string CreatetLogFilePath()
        {
            const int MaxErrorCount = 10;
            string logFileSystemPath = string.Empty;
            int i = 0;
            do
            {
                string logFilePath = string.Format("{0}/{1}", Config.UserMissionsFolder, i == 0 ? Config.LogFileName :
                                        Path.GetFileNameWithoutExtension(Config.ConvertLogFileName) + i + Path.GetExtension(Config.ConvertLogFileName));
                logFileSystemPath = (GamePlay as IGame).gameInterface.ToFileSystemPath(logFilePath);
            }
            while (!FileUtil.IsFileWritable(logFileSystemPath) && i++ < MaxErrorCount);
            return logFileSystemPath;
        }

        public static void WriteLog(string message)
        {
            Debug.WriteLine(message);
            lock (writerLogObject)
            {
                writerLog.WriteLine("{0} \"{1}\"", DateTime.Now.ToString(Config.DateTimeDefaultLongLongFormat, Config.DateTimeFormat), message.Replace("\"", "\"\"").Replace("\n", "|"));
            }
        }

#if DEBUG
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
            MissionStatus missionStatus = new MissionStatus(Random);
            missionStatus.Update(GamePlay as IGame, playerActorName, dateTime);
            missionStatus.WriteTo(missionStatusFile);
            missionStatusFile.save(missionStatusFileName);

            ISectionFile missionStatusFileLoad = gameInterface.SectionFileLoad(missionStatusFileName);
            missionStatus = MissionStatus.Create(missionStatusFileLoad, Random);
            Debug.Assert(missionStatus != null);
        }
#endif
    }
}