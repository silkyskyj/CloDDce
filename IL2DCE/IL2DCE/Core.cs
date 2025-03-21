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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;

namespace IL2DCE
{
    public class Core
    {
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

        private static StreamWriter writerLog;
        private static object writerLogObject = new object();

        private string _debugFolderSystemPath;
        private string _careersFolderSystemPath;
        private string _campaignsFolderSystemPath;

        public Core(IGame game)
            : this(game, new Random())
        {
        }

        public Core(IGame game, IRandom random)
        {
            _gamePlay = game;
            _random = random;

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
            CurrentCareer.Date = null;

            AdvanceCampaign(game);
        }

        public CampaignStatus AdvanceCampaign(IGame game)
        {
            CampaignStatus result;
            Generator.Generator generator = new Generator.Generator(GamePlay, Random, Config, CurrentCareer);

            Career career = CurrentCareer;
            CampaignInfo campaignInfo = career.CampaignInfo;

            ISectionFile initialMissionTemplateFile = null;
            if (!CurrentCareer.Date.HasValue)
            {
                // It is the first mission.
                career.Date = career.CampaignInfo.StartDate;
                career.Experience = career.RankIndex * 1000;

                // Generate the initial mission tempalte
                generator.GenerateInitialMissionTempalte(campaignInfo.InitialMissionTemplateFiles, out initialMissionTemplateFile, campaignInfo.AirGroupInfos);
                result = CampaignStatus.Empty;
            }
            else
            {
                if (game is IGameSingle)
                {
                    IGameSingle gameSingle = game as IGameSingle;
                    if (gameSingle.BattleSuccess == EBattleResult.SUCCESS)
                    {
                        career.Experience += 200;
                    }
                    else if (gameSingle.BattleSuccess == EBattleResult.DRAW)
                    {
                        career.Experience += 100;
                    }
                }

                if (career.RankIndex < Rank.RankMax && career.Experience >= (career.RankIndex + 1) * 1000)
                {
                    career.RankIndex += 1;
                }

                if (career.Date >= campaignInfo.EndDate)
                {
                    result = CampaignStatus.DateEnd;
                }
                else
                {
                    result = CampaignStatus.InProgress;
                    career.Date = career.Date.Value.Add(new TimeSpan(1, 0, 0, 0));
                }

                initialMissionTemplateFile = game.gpLoadSectionFile(career.MissionTemplateFileName);
            }

            string missionFolderSystemPath = string.Format("{0}\\{1}", _careersFolderSystemPath, career.PilotName);
            if (!Directory.Exists(missionFolderSystemPath))
            {
                Directory.CreateDirectory(missionFolderSystemPath);
            }

            if (game.gameInterface.BattleIsRun())
            {
                // Stop the currntly running battle.
                game.gameInterface.BattleStop();
            }

            // Preload mission file for path calculation.
            game.gameInterface.MissionLoad(campaignInfo.StaticTemplateFiles[0]);

            ISectionFile careerFile = GamePlay.gpCreateSectionFile();
            string careerFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, career.PilotName, Config.CareerInfoFileName);

            if (result != CampaignStatus.DateEnd)
            {
                string missionId = string.Format("{0}_{1}-{2}-{3}",
                                                    campaignInfo.Id,
                                                    career.Date.Value.Year.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                                    career.Date.Value.Month.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                                    career.Date.Value.Day.ToString(CultureInfo.InvariantCulture.NumberFormat));
                string missionFileName = string.Format("{0}/{1}/{2}.mis", Config.UserMissionFolder, career.PilotName, missionId);
                career.MissionFileName = missionFileName;

                // Generate the template for the next mission
                ISectionFile missionTemplateFile = null;
                generator.GenerateMissionTemplate(campaignInfo.StaticTemplateFiles, initialMissionTemplateFile, out missionTemplateFile, campaignInfo.AirGroupInfos);
                missionTemplateFile.save(career.MissionTemplateFileName);

                // Generate the next mission based on the new template.
                ISectionFile missionFile = null;
                BriefingFile briefingFile = null;
                generator.GenerateMission(campaignInfo.EnvironmentTemplateFile, career.MissionTemplateFileName, missionId, out missionFile, out briefingFile);

                // Save mission file
                missionFile.save(missionFileName);

                // Copy mission script file
                string scriptSourceFileSystemPath = string.Format("{0}\\{1}\\{2}", _campaignsFolderSystemPath, campaignInfo.Id, campaignInfo.ScriptFileName);
                string scriptDestinationFileSystemPath = string.Format("{0}\\{1}\\{2}.cs", _careersFolderSystemPath, career.PilotName, missionId);
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
            game.gameInterface.BattleStop();

            career.WriteTo(careerFile);
            careerFile.save(careerFileName);

            return result;
        }

        public void CreateQuickMission(IGame game, Career career)
        {
            Generator.Generator generator = new Generator.Generator(GamePlay, Random, Config, CurrentCareer);

            CampaignInfo campaignInfo = career.CampaignInfo;

            career.Date = career.CampaignInfo.StartDate;
            career.Experience = career.RankIndex * 1000;

            ISectionFile initialMissionTemplateFile = null;
            generator.GenerateInitialMissionTempalte(campaignInfo.InitialMissionTemplateFiles, out initialMissionTemplateFile, campaignInfo.AirGroupInfos);

            string missionFolderSystemPath = string.Format("{0}\\{1}", _careersFolderSystemPath, career.PilotName);
            if (!Directory.Exists(missionFolderSystemPath))
            {
                Directory.CreateDirectory(missionFolderSystemPath);
            }

            if (game.gameInterface.BattleIsRun())
            {
                game.gameInterface.BattleStop();
            }

            // Preload mission file for path calculation.
            game.gameInterface.MissionLoad(campaignInfo.StaticTemplateFiles[0]);
            game.gameInterface.BattleStop();

            // ISectionFile careerFile = GamePlay.gpCreateSectionFile();
            // string careerFileName = string.Format("{0}/{1}/{2}", UserMissionFolder, career.PilotName, CareerInfoFileName);

            string missionId = campaignInfo.Id;
            string missionFileName = string.Format("{0}/{1}/{2}.mis", Config.UserMissionFolder, career.PilotName, missionId);
            career.MissionFileName = missionFileName;

            // Generate the template for the next mission
            ISectionFile missionTemplateFile = null;
            generator.GenerateMissionTemplate(campaignInfo.StaticTemplateFiles, initialMissionTemplateFile, out missionTemplateFile, campaignInfo.AirGroupInfos);
            missionTemplateFile.save(career.MissionTemplateFileName);

            // Generate the next mission based on the new template.
            ISectionFile missionFile = null;
            BriefingFile briefingFile = null;
            generator.GenerateMission(campaignInfo.EnvironmentTemplateFile, career.MissionTemplateFileName, missionId, out missionFile, out briefingFile);

            // Save mission file
            missionFile.save(missionFileName);

            // Copy mission script file
            string scriptSourceFileSystemPath = string.Format("{0}\\{1}\\{2}", _campaignsFolderSystemPath, campaignInfo.Id, campaignInfo.ScriptFileName);
            string scriptDestinationFileSystemPath = string.Format("{0}\\{1}\\{2}.cs", _careersFolderSystemPath, career.PilotName, missionId);
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
                writerLog.WriteLine("{0} \"{1}\"", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff", Config.Culture), message.Replace("\"", "\"\"").Replace("\n", "|"));
            }
        }
    }
}