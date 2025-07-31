// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover
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
using System.Text;
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using maddox.game;
using maddox.GP;

namespace IL2DCE.Util
{
    public class MissionFileConverter
    {
        public static readonly char[] SplitChars = Environment.NewLine.ToCharArray();
        public const string DefaultMissionFileSearchPettern = "*.mis";
        public const string DefaultCampaignFileSearchPettern = Config.CampaignFileName;
        public const string ErrorFormatSectionOrKey = "Error:No available Section or Key[{0}]";
        public const string ErrorFormatNotEnough = "Error:Not enough info[{0}]";
        public const string WarnFormatDuplicateSquadron = "Warn:Duplicate squadron[{0}]";
        public const string WarnNoAviableAddedGenericOne = "Warn:No avialable[{0}] Converter added generic one";

        private GameIterface gameInterface;
        private ISectionFile globalAircraftInfoFile;
        private AirGroupInfos airGroupInfos;

        private string homeFolder;
        private string userFolder;
        private BackgroundWorker worker;

        public List<string> ErrorWarnMsg
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }

        public int CovertedMissionCount
        {
            get;
            private set;
        }

        public MissionFileConverter(GameIterface gameInterface, ISectionFile globalAircraftInfoFile = null, AirGroupInfos airGroupInfos = null, BackgroundWorker worker = null)
        {
            this.gameInterface = gameInterface;
            homeFolder = gameInterface.ToFileSystemPath(Config.HomeFolder);
            userFolder = gameInterface.ToFileSystemPath(Config.UserFolder);
            if (globalAircraftInfoFile == null)
            {
                globalAircraftInfoFile = gameInterface.SectionFileLoad(string.Format("{0}/{1}", Config.CampaignsFolderDefault, Config.AircraftInfoFileName));
            }
            this.globalAircraftInfoFile = globalAircraftInfoFile;
            if (airGroupInfos == null)
            {
                ISectionFile globalAirGroupInfoFile = gameInterface.SectionFileLoad(string.Format("{0}/{1}", Config.CampaignsFolderDefault, Config.AirGroupInfoFileName));
                airGroupInfos = AirGroupInfos.Create(globalAirGroupInfoFile);
            }
            this.airGroupInfos = airGroupInfos;
            this.worker = worker;
            ErrorWarnMsg = new List<string>();
            Index = 0;
            CovertedMissionCount = 0;
        }

        public bool ConvertSystemPath(string fileSystemPath, string name, string outputBasetFolder = null)
        {
            Debug.WriteLine("MissionFileConverter.ConvertSystemPath({0}, {1}, {2})", fileSystemPath, name, outputBasetFolder != null ? outputBasetFolder : string.Empty);
#if true
            // string filePathSrcSystemPath = gameInterface.ToFileSystemPath(filePathSrc.Trim());
            SilkySkyCloDFile fileSorce = SilkySkyCloDFile.Load(fileSystemPath, false);
#else
            ISectionFile fileSorce = gameInterface.SectionFileLoad(filePathSrc.Trim());
#endif
            if (string.IsNullOrEmpty(name))
            {
                name = GetMissionName(fileSystemPath);
            }
            if (worker != null)
            {
                worker.ReportProgress(Index, string.Join("\n", SplitTargetSystemPathInfo(fileSystemPath)));
            }
            return Convert(fileSorce, name, outputBasetFolder);
        }

        private bool Convert(ISectionFile fileSorce, string name, string outputBasetFolder = null)
        {
            Debug.WriteLine("MissionFileConverter.Convert({0}, {1})", name, outputBasetFolder != null ? outputBasetFolder : string.Empty);

            StringBuilder sb = new StringBuilder();
            int error = 0;

            try
            {
                // 1. Initialize
                if (string.IsNullOrEmpty(outputBasetFolder))
                {
                    outputBasetFolder = Config.CampaignsFolderDefault;
                }
                string fileName = name.Replace(",", " ");

                string outputBasetFolderrSystemPath = gameInterface.ToFileSystemPath(outputBasetFolder);

                // 2. Create AircraftInfo.ini 
                ISectionFile fileAircraft = gameInterface.SectionFileCreate();

                // 3. Create AirGroupInfo.ini 
                ISectionFile fileAirGroup = gameInterface.SectionFileCreate();

                // 4. Read Mission File
                MissionFile missionFile = new MissionFile(fileSorce, airGroupInfos, MissionFile.LoadLevel.AirGroup);

                // 5. Create Mission initialTemplate File
                //      2. AircraftInfo.ini
                //      3. AirGroupInfo.ini 
                ISectionFile fileMissionInitial = gameInterface.SectionFileCreate();
                ConvertCore(fileSorce, missionFile, name, fileMissionInitial, fileAircraft, fileAirGroup, ref error, sb, outputBasetFolder);

                if (error == 0)
                {
                    string fileNameMissionInitial = string.Format("{0}{1}", fileName, Config.MissionFileExt);
                    string filePathMissionInitial = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, fileNameMissionInitial);

                    // 7. Create CampaignInfo.ini 
                    ISectionFile fileCampaign = gameInterface.SectionFileCreate();
                    string filePathCampaign = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, Config.CampaignInfoFileName);
                    IEnumerable<KeyValuePair<int, string>> mapPeriods = Map.GetDefaultMapPeriod(missionFile.Map);
                    CampaignInfo campaignInfo = new CampaignInfo(name, ECampaignMode.Default, new string[] { fileNameMissionInitial }, Config.MissionScriptFileName, null, null, mapPeriods, false);
                    campaignInfo.Write(fileCampaign);

                    // 8. Create Mission Folder (Mission Filename without Extension)
                    string missionFolderSystemPath = string.Format("{0}\\{1}", outputBasetFolderrSystemPath, fileName);
                    if (!Directory.Exists(missionFolderSystemPath))
                    {
                        Directory.CreateDirectory(missionFolderSystemPath);
                    }

                    // 9. Save All Files
                    string filePathAircraft = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, Config.AircraftInfoFileName);
                    fileAircraft.save(filePathAircraft);
                    string filePathAirGroup = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, Config.AirGroupInfoFileName);
                    fileAirGroup.save(filePathAirGroup);
                    fileMissionInitial.save(filePathMissionInitial);

                    // 10. Create Mission Script File
                    string filePathScriptSystemPathSrc = string.Format("{0}\\{1}", gameInterface.ToFileSystemPath(Config.CampaignsFolderDefault), Config.MissionScriptFileName);
                    string filePathScriptSystemPathDst = string.Format("{0}\\{1}\\{2}", outputBasetFolderrSystemPath, fileName, Config.MissionScriptFileName);
                    File.Copy(filePathScriptSystemPathSrc, filePathScriptSystemPathDst, true);
                    fileCampaign.save(filePathCampaign);

                    CovertedMissionCount += 1;
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("{0} - {1} {2} {3}", "MissionFileConverter.ConvertSystemPath", ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                Core.WriteLog(message);
                sb.AppendLine(message);
                error++;
            }

            Index += 1;
            if (sb.Length > 0)
            {
                ErrorWarnMsg.Add(sb.ToString());
            }

            return error == 0;
        }

        private void CreateAndWriteFrontMarker(ISectionFile file, MissionFile missionFile)
        {
            IEnumerable<Point3d> positionsRed = missionFile.GroundGroups.Where(x => x.Army == (int)EArmy.Red).Select(x => x.Position)
                                .Concat(missionFile.Stationaries.Where(x => x.Army == (int)EArmy.Red).Select(x => x.Position)).Select(x => new Point3d(x.x, x.y, (int)EArmy.Red));
            IEnumerable<Point3d> positionsBlue = missionFile.GroundGroups.Where(x => x.Army == (int)EArmy.Blue).Select(x => x.Position)
                                .Concat(missionFile.Stationaries.Where(x => x.Army == (int)EArmy.Blue).Select(x => x.Position)).Select(x => new Point3d(x.x, x.y, (int)EArmy.Blue));



            // TODO: Create FrontMarker



            WriteFrontMarkers(file, missionFile.FrontMarkers);
        }

        private void WriteFrontMarkers(ISectionFile file, IEnumerable<Point3d> frontMarkers)
        {
            int i = 0;
            foreach (Point3d point in frontMarkers)
            {
                string key = string.Format(Config.NumberFormat, "{0}{1}", MissionFile.SectionFrontMarker, i + 1);
                string value = string.Format(Config.NumberFormat, "{0:F2} {1:F2} {2}", point.x, point.y, (int)point.z);
                file.add(MissionFile.SectionFrontMarker, key, value);
                i++;
            }
        }

        public bool ConvertCampaign(string fileSystemPath, string name = null, string outputBasetFolder = null)
        {
            Debug.WriteLine("MissionFileConverter.ConvertCampaign({0}, {1}, {2})", fileSystemPath, name, outputBasetFolder != null ? outputBasetFolder : string.Empty);

#if true
            // string filePathSrcSystemPath = gameInterface.ToFileSystemPath(filePathSrc.Trim());
            SilkySkyCloDFile fileSorce = SilkySkyCloDFile.Load(fileSystemPath, false);
#else
            ISectionFile fileSorce = gameInterface.SectionFileLoad(filePathSrc.Trim());
#endif
            string dirSystemPath = Path.GetDirectoryName(fileSystemPath);

            if (string.IsNullOrEmpty(name))
            {
                name = GetMissionName(dirSystemPath);
            }

            List<string> missionNameLists = new List<string>();
            string key;
            string value;
            int lines = fileSorce.lines(Config.SectionBattles);
            for (int i = 0; i < lines; i++)
            {
                if (fileSorce.Get(Config.SectionBattles, i, out key, out value))
                {
                    string missionName = string.IsNullOrEmpty(value) ? key : string.Format("{0} {1}", key, value);
                    string[] missionNames = missionName.Split(missionName.IndexOfAny(Config.SplitDQ) != -1 ? Config.SplitDQ : Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
                    if (missionNames.Length > 0)
                    {
                        missionNameLists.Add(missionNames.First());
                    }
                }
            }

            return ConvertCampaign(dirSystemPath, missionNameLists, name, outputBasetFolder);
        }

        private bool ConvertCampaign(string dirSystemPath, IEnumerable<string> missionNames, string name, string outputBasetFolder = null)
        {
            Debug.WriteLine("MissionFileConverter.ConvertCampaign({0}, {1})", name, outputBasetFolder != null ? outputBasetFolder : string.Empty);

            // 1. Initialize
            if (string.IsNullOrEmpty(outputBasetFolder))
            {
                outputBasetFolder = Config.CampaignsFolderDefault;
            }
            string fileName = name.Replace(",", " ");

            string outputBasetFolderrSystemPath = gameInterface.ToFileSystemPath(outputBasetFolder);

            // 2. Create AircraftInfo.ini 
            ISectionFile fileAircraft = gameInterface.SectionFileCreate();

            // 3. Create AirGroupInfo.ini 
            ISectionFile fileAirGroup = gameInterface.SectionFileCreate();

            Dictionary<string, ISectionFile> missionPaths = new Dictionary<string, ISectionFile>();
            IEnumerable<KeyValuePair<int, string>> mapPeriods = null;

            string pathCampaign = string.Format("{0}{1}{2}", dirSystemPath, Path.DirectorySeparatorChar, Config.CampaignFileName);

            int success = 0;
            int error;
            foreach (var missionName in missionNames)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(missionName);
                error = 0;
                try
                {
                    if (worker != null)
                    {
                        worker.ReportProgress(Index, string.Format("{0} - [{1}{2}]", string.Join("\n", SplitTargetSystemPathInfo(pathCampaign)), missionName, Config.MissionFileExt));
                        if (worker.CancellationPending)
                        {
                            break;
                        }
                    }

                    // 4. Read Mission File
                    string path = string.Format("{0}{1}{2}{3}", dirSystemPath, Path.DirectorySeparatorChar, missionName, Config.MissionFileExt);

                    SilkySkyCloDFile fileSorce = SilkySkyCloDFile.Load(path, false);
                    MissionFile missionFile = new MissionFile(fileSorce, airGroupInfos, MissionFile.LoadLevel.AirGroup);

                    // 5. Create Mission initialTemplate File
                    //      2. AircraftInfo.ini
                    //      3. AirGroupInfo.ini 
                    ISectionFile fileMissionInitial = gameInterface.SectionFileCreate();
                    ConvertCore(fileSorce, missionFile, name, fileMissionInitial, fileAircraft, fileAirGroup, ref error, sb, outputBasetFolder);

                    if (mapPeriods == null || !mapPeriods.Any())
                    {
                        mapPeriods = Map.GetDefaultMapPeriod(missionFile.Map);
                    }

                    if (error == 0)
                    {
                        string fileNameMissionInitial = string.Format("{0}{1}", missionName, Config.MissionFileExt);
                        string filePathMissionInitial = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, fileNameMissionInitial);
                        missionPaths.Add(filePathMissionInitial, fileMissionInitial);
                        success++;
                    }
                }
                catch (Exception ex)
                {
                    string message = string.Format("{0} - {1} {2} {3}", "MissionFileConverter.ConvertCampaign", ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                    Core.WriteLog(message);
                    sb.AppendLine(message);
                    error++;
                }
                sb.AppendLine(error.ToString(Config.NumberFormat));
                ErrorWarnMsg.Add(sb.ToString());
                Index += 1;
            }

            if (success > 0)
            {
                // 7. Create CampaignInfo.ini 
                ISectionFile fileCampaign = gameInterface.SectionFileCreate();
                string filePathCampaign = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, Config.CampaignInfoFileName);
                CampaignInfo campaignInfo = new CampaignInfo(name, ECampaignMode.Progress, missionPaths.Select(x => Path.GetFileName(x.Key)), Config.MissionScriptFileName, null, null, mapPeriods, false);
                campaignInfo.Write(fileCampaign);

                // 8. Create Mission Folder (Mission Filename without Extension)
                string missionFolderSystemPath = string.Format("{0}\\{1}", outputBasetFolderrSystemPath, fileName);
                if (!Directory.Exists(missionFolderSystemPath))
                {
                    Directory.CreateDirectory(missionFolderSystemPath);
                }

                // 9. Save All Files
                string filePathAircraft = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, Config.AircraftInfoFileName);
                fileAircraft.save(filePathAircraft);
                string filePathAirGroup = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, Config.AirGroupInfoFileName);
                fileAirGroup.save(filePathAirGroup);
                foreach (var item in missionPaths)
                {
                    item.Value.save(item.Key);
                    CovertedMissionCount += 1;
                }

                // 10. Create Mission Script File
                string filePathScriptSystemPathSrc = string.Format("{0}\\{1}", gameInterface.ToFileSystemPath(Config.CampaignsFolderDefault), Config.MissionScriptFileName);
                string filePathScriptSystemPathDst = string.Format("{0}\\{1}\\{2}", outputBasetFolderrSystemPath, fileName, Config.MissionScriptFileName);
                File.Copy(filePathScriptSystemPathSrc, filePathScriptSystemPathDst, true);
                fileCampaign.save(filePathCampaign);
            }

            return success > 0;
        }

        private void ConvertCore(ISectionFile fileSorce, MissionFile missionFile, string name, ISectionFile fileMissionInitial, ISectionFile fileAircraft, ISectionFile fileAirGroup, ref int error, StringBuilder sb, string outputBasetFolder = null)
        {
            IList<AirGroup> airGroups = missionFile.AirGroups;
            // 2. AircraftInfo.ini
            // 3. AirGroupInfo.ini 
            var armys = airGroups.Select(x => x.ArmyIndex).Distinct().OrderBy(x => x);
            if (armys.Count() < 1)
            {
                error++;
                sb.AppendLine(string.Format(ErrorFormatNotEnough, "Army"));
            }

            foreach (var army in armys)
            {
                int enemy = Army.Enemy(army);
                var airGroupsArmy = airGroups.Where(x => x.ArmyIndex == army).OrderBy(x => x.Id);
                bool addMissionTransfer = armys.Count() == 1 || (missionFile.GroundGroups.Where(x => x.Army == enemy).Count() + missionFile.Stationaries.Where(x => x.Army == enemy).Count()) < (airGroupsArmy.Count() / 2);
                foreach (var airGroup in airGroupsArmy)
                {
                    if (globalAircraftInfoFile.exist(AircraftInfo.SectionMain, airGroup.Class))
                    {
                        string airGoupKey = string.IsNullOrEmpty(airGroup.VirtualAirGroupKey) ? airGroup.AirGroupKey : airGroup.VirtualAirGroupKey;
                        if (airGroup.AirGroupInfo != null)
                        {
                            try
                            {
                                AircraftInfo aircraftInfo = new AircraftInfo(globalAircraftInfoFile, airGroup.Class);
                                aircraftInfo.Write(fileAircraft, addMissionTransfer);           //  AircraftInfo.ini
                                airGroup.AirGroupInfo.Write(fileAirGroup, airGoupKey, airGroup.Class);  //  AirGroupInfo.ini
                            }
                            catch (Exception ex)
                            {
                                string message = string.Format("Error [{0}] AirGroup Info[{1}] MissionFile:[{2}]", ex.Message, airGoupKey, name);
                                Core.WriteLog(message);
                                sb.AppendLine(message);
                                error++;
                            }
                        }
                        else
                        {
                            string message = string.Format("No AirGroup Info[{0}] MissionFile:[{1}]", airGoupKey, name);
                            sb.AppendLine(message);
                            Core.WriteLog(message);
                            error++;
                        }
                    }
                    else
                    {
                        string message = string.Format("No Aircraft Info[{0}] MissionFile:[{1}]", airGroup.Class, name);
                        sb.AppendLine(message);
                        Core.WriteLog(message);
                        error++;
                    }
                }
            }

            // 5. Create Mission initialTemplate File
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, MissionFile.SectionParts);
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, MissionFile.SectionMain);
            int i = 0;
            while (SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, string.Format(Config.NumberFormat, "{0}_{1}", MissionFile.SectionGlobalWind, i)) > 0)
            {
                i++;
            }
            foreach (var army in armys)
            {
                var airGroupsArmy = airGroups.Where(x => x.ArmyIndex == army).OrderBy(x => x.Id);
                string squadronNameOld = string.Empty;
                foreach (var airGroup in airGroupsArmy)
                {
                    if (string.Compare(airGroup.SquadronName, squadronNameOld, true) != 0)
                    {
                        airGroup.WriteTo(fileMissionInitial);
                    }
                    else
                    {
                        sb.AppendLine(string.Format(WarnFormatDuplicateSquadron, squadronNameOld));
                    }
                    squadronNameOld = airGroup.SquadronName;
                }
            }
            IEnumerable<string> values = SilkySkyCloDFile.CopySectionGetValue(fileSorce, fileMissionInitial, MissionFile.SectionSplines, 0);
            foreach (var item in values)
            {
                SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, item);
            }
            IEnumerable<string> countries = Country.ToStrings().Select(x => string.Format(" {0} ", x));
            IEnumerable<string> keys = SilkySkyCloDFile.CopySectionGetKey(fileSorce, fileMissionInitial, MissionFile.SectionChiefs);
            foreach (var item in keys)
            {
                SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, string.Format("{0}_{1}", item, MissionFile.SectionRoad));
            }
            keys = SilkySkyCloDFile.CopySectionGetKey(fileSorce, fileMissionInitial, MissionFile.SectionCustomChiefs);
            foreach (var item in keys)
            {
                SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, item);
            }
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, MissionFile.SectionNpc);
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, MissionFile.SectionStationary);
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, MissionFile.SectionBuildings);
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, MissionFile.SectionBuildingsLinks);
            keys = SilkySkyCloDFile.CopySectionGetKey(fileSorce, fileMissionInitial, MissionFile.SectionAirdromes);
            foreach (var item in keys)
            {
                SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, string.Format("{0}_{1}", item, MissionFile.KeyRunways));
                SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, string.Format("{0}_{1}", item, MissionFile.KeyPoints));
            }
            if (SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, MissionFile.SectionFrontMarker) == 0)
            {
                IEnumerable<Point3d> frontMarkers = Map.GetDefaultFrontMarkers(missionFile.Map);
                if (frontMarkers == null)
                {
                    sb.AppendLine(string.Format(ErrorFormatSectionOrKey, MissionFile.SectionFrontMarker));
                    error++;
                }
                else
                {
                    sb.AppendLine(string.Format(WarnNoAviableAddedGenericOne, MissionFile.SectionFrontMarker));
                    fileMissionInitial.delete(MissionFile.SectionFrontMarker);
                    WriteFrontMarkers(fileMissionInitial, frontMarkers);
                }
            }
        }

        private string GetMissionName(string path)
        {
            int idx = path.LastIndexOf("/");
            if (idx == -1)
            {
                idx = path.LastIndexOf("\\");
            }
            if (idx != -1)
            {
                path = path.Substring(idx + 1);
            }
            return path;
        }

        public IEnumerable<string> GetFiles(IEnumerable<string> folders, string fileSearchPattern = DefaultMissionFileSearchPettern)
        {
            List<string> files = new List<string>();
            foreach (var item in folders)
            {
                string folderSystemPath = gameInterface.ToFileSystemPath(item.Trim());
                if (Directory.Exists(folderSystemPath) && IsTargetFolderSystemPath(folderSystemPath))
                {
                    files.AddRange(GetFiles(folderSystemPath, fileSearchPattern, SearchOption.AllDirectories));
                }
            }
            return files;
        }

        public bool IsTargetFolderSystemPath(string folder)
        {
            if (!string.IsNullOrEmpty(folder))
            {
                folder = folder.TrimStart();
                string homeFolder = gameInterface.ToFileSystemPath(Config.HomeFolder);
                string userFolder = gameInterface.ToFileSystemPath(Config.UserFolder);
                return folder.StartsWith(homeFolder) || folder.StartsWith(userFolder);
            }
            return false;
        }

        public string[] SplitTargetSystemPathInfo(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                path = path.Trim();
                if (path.StartsWith(homeFolder))
                {
                    return new string[] { homeFolder, path.Substring(homeFolder.Length) };
                }
                if (path.StartsWith(userFolder))
                {
                    return new string[] { userFolder, path.Substring(userFolder.Length) };
                }
            }
            return new string[] { string.Empty, path };
        }

        public static IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            string[] files = Directory.GetFiles(path, searchPattern, searchOption);
            int idx = searchPattern.LastIndexOf(".");
            if (idx != -1)
            {
                string ext = searchPattern.Substring(idx);
                return files.Where(x => x.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase));
            }
            return files;
        }

        public int CountCampaignMissionFiles(string fileSystemPath)
        {
            int files = 0;

#if true
            // string filePathSrcSystemPath = gameInterface.ToFileSystemPath(filePathSrc.Trim());
            SilkySkyCloDFile fileSorce = SilkySkyCloDFile.Load(fileSystemPath, false);
#else
            ISectionFile fileSorce = gameInterface.SectionFileLoad(filePathSrc.Trim());
#endif
            string dirSystemPath = Path.GetDirectoryName(fileSystemPath);
            string key;
            string value;
            int lines = fileSorce.lines(Config.SectionBattles);
            for (int i = 0; i < lines; i++)
            {
                if (fileSorce.Get(Config.SectionBattles, i, out key, out value))
                {
                    string missionName = string.IsNullOrEmpty(value) ? key : string.Format("{0} {1}", key, value);
                    string[] missionNames = missionName.Split(missionName.IndexOfAny(Config.SplitDQ) != -1 ? Config.SplitDQ : Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
                    if (missionNames.Length > 0)
                    {
                        files++;
                    }
                }
            }
            return files;
        }

        #region Reserve

        private bool IsTargetFolder(string folder)
        {
            if (!string.IsNullOrEmpty(folder))
            {
                folder = folder.TrimStart();
                return folder.StartsWith(Config.HomeFolder) || folder.StartsWith(Config.UserFolder);
            }
            return false;
        }

        private bool Convert(string filePathSrc, string name, string outputBasetFolder = null)
        {
#if false
                    // string filePathSrcSystemPath = gameInterface.ToFileSystemPath(filePathSrc.Trim());
                    SilkySkyCloDFile fileSorce = SilkySkyCloDFile.Load(filePathSrc, false);
#else
            ISectionFile fileSorce = gameInterface.SectionFileLoad(filePathSrc.Trim());
#endif
            if (string.IsNullOrEmpty(name))
            {
                int idx = filePathSrc.LastIndexOf("/");
                if (idx == -1)
                {
                    idx = filePathSrc.LastIndexOf("\\");
                }
                if (idx != -1)
                {
                    name = filePathSrc.Substring(idx + 1);
                }
            }
            return Convert(fileSorce, name, outputBasetFolder);
        }

        private bool ConvertFolder(string srcFolder, string destFolder = null, bool useFolderName = false, string fileSearchPattern = DefaultMissionFileSearchPettern)
        {
            int error = 0;

            string folderSystemPath = gameInterface.ToFileSystemPath(srcFolder.Trim());
            if (Directory.Exists(folderSystemPath))
            {
                string nameDir = folderSystemPath.Split(Path.DirectorySeparatorChar).LastOrDefault();
                string[] filesPath = Directory.GetFiles(folderSystemPath, fileSearchPattern);
                foreach (var filePath in filesPath)
                {
                    string nameFile = filePath.Split(Path.DirectorySeparatorChar).LastOrDefault();
                    string path = string.Format("{0}/{1}", srcFolder, nameFile);
                    nameFile = Path.GetFileNameWithoutExtension(nameFile);
                    string name = useFolderName ? string.Format("{0}_{1}", nameDir, nameFile) : nameFile;
                    if (!Convert(path, name, destFolder))
                    {
                        error++;
                    }
                }

                string[] subDirs = Directory.GetDirectories(folderSystemPath);
                foreach (var subDir in subDirs)
                {
                    nameDir = subDir.Split(Path.DirectorySeparatorChar).LastOrDefault();
                    string dir = string.Format("{0}/{1}", srcFolder, nameDir);
                    if (!ConvertFolder(dir, destFolder, useFolderName))
                    {
                        error++;
                    }
                }
            }

            return error == 0;
        }

        public int CountFiles(IEnumerable<string> folders, string fileSearchPattern = DefaultMissionFileSearchPettern)
        {
            int files = 0;
            foreach (var item in folders)
            {
                string folderSystemPath = gameInterface.ToFileSystemPath(item.Trim());
                if (Directory.Exists(folderSystemPath) && IsTargetFolderSystemPath(folderSystemPath))
                {
                    files += GetFiles(folderSystemPath, fileSearchPattern, SearchOption.AllDirectories).Count();
                }
            }
            return files;
        }

        #endregion
    }
}