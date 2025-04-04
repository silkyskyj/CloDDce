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
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using maddox.game;

namespace IL2DCE.Util
{
    public class MissionFileConverter
    {
        public const string DefaultFileSearchPettern = "*.mis";
        public const string ErrorFormatSectionOrKey = "no avialable Section or Key[{0}]";
        public const string ErrorFormatNotNnough = "not enough info[{0}]";

        private GameIterface gameInterface;
        private ISectionFile globalAircraftInfoFile;
        private AirGroupInfos airGroupInfos;

        private string homeFolder;
        private string userFolder;

        public List<string> ErrorMsg
        {
            get;
            private set;
        }

        public List<string> CovertedMission
        {
            get;
            private set;
        }

        public MissionFileConverter(GameIterface gameInterface, ISectionFile globalAircraftInfoFile = null, AirGroupInfos airGroupInfos = null)
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
            ErrorMsg = new List<string>();
            CovertedMission = new List<string>();
        }

        public bool Convert(string filePathSrc, string name, string outputBasetFolder = null)
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
                    name = name.Substring(idx);
                }
            }
            return Convert(fileSorce, name, outputBasetFolder);
        }

        public bool ConvertSystemPath(string filePathSrcSystem, string name, string outputBasetFolder = null)
        {
#if true
            // string filePathSrcSystemPath = gameInterface.ToFileSystemPath(filePathSrc.Trim());
            SilkySkyCloDFile fileSorce = SilkySkyCloDFile.Load(filePathSrcSystem, false);
#else
            ISectionFile fileSorce = gameInterface.SectionFileLoad(filePathSrc.Trim());
#endif
            if (string.IsNullOrEmpty(name))
            {
                int idx = filePathSrcSystem.LastIndexOf("/");
                if (idx == -1)
                {
                    idx = filePathSrcSystem.LastIndexOf("\\");
                }
                if (idx != -1)
                {
                    name = name.Substring(idx);
                }
            }
            return Convert(fileSorce, name, outputBasetFolder);
        }

        public bool Convert(ISectionFile fileSorce, string name, string outputBasetFolder = null)
        {
            Debug.WriteLine("MissionFileConverter.Convert({0}, {1})", name, outputBasetFolder != null ? outputBasetFolder : string.Empty);

            // 1. Initialize
            if (string.IsNullOrEmpty(outputBasetFolder))
            {
                outputBasetFolder = Config.CampaignsFolderDefault;
            }
            string fileName = name.Replace(",", " ");

            string outputBasetFolderrSystemPath = gameInterface.ToFileSystemPath(outputBasetFolder);

            // 2. Read Mission File
            MissionFile missionFile = new MissionFile(fileSorce, airGroupInfos);

            // 3. Create AircraftInfo.ini 
            ISectionFile fileAircraft = gameInterface.SectionFileCreate();
            string filePathAircraft = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, Config.AircraftInfoFileName);

            // 4. Create AirGroupInfo.ini 
            ISectionFile fileAirGroup = gameInterface.SectionFileCreate();
            string filePathAirGroup = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, Config.AirGroupInfoFileName);

            IList<AirGroup> airGroups = missionFile.AirGroups;
            //    3. & 4. 
            if (airGroups.Count < 2)
            {
                ErrorMsg.Add(string.Format(ErrorFormatNotNnough, MissionFile.SectionAirGroups));
            }

            var armys = airGroups.Select(x => x.ArmyIndex).Distinct().OrderBy(x => x);
            if (armys.Count() < 2)
            {
                ErrorMsg.Add(string.Format(ErrorFormatNotNnough, "Army"));
            }
            foreach (var army in armys)
            {
                var airGroupsArmy = airGroups.Where(x => x.ArmyIndex == army).OrderBy(x => x.Id);
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
                                aircraftInfo.Write(fileAircraft);           //  AircraftInfo.ini
                                airGroup.AirGroupInfo.Write(fileAirGroup, airGoupKey, airGroup.Class);  //  AirGroupInfo.ini
                            }
                            catch (Exception ex)
                            {
                                string message = string.Format("Error [{0}] AirGroup Info[{1}] MissionFile:[{2}]\n", ex.Message, airGoupKey, name);
                                Core.WriteLog(message);
                                ErrorMsg.Add(message);
                            }
                        }
                        else
                        {
                            string message = string.Format("No AirGroup Info[{0}] MissionFile:[{1}]\n", airGoupKey, name);
                            ErrorMsg.Add(message);
                            Core.WriteLog(message);
                        }
                    }
                    else
                    {
                        string message = string.Format("No Aircraft Info[{0}] MissionFile:[{1}]\n", airGroup.Class, name);
                        ErrorMsg.Add(message);
                        Core.WriteLog(message);
                    }
                }
            }

            // 5. Create Mission environmentTemplate File
            ISectionFile fileMissionEnvironment = gameInterface.SectionFileCreate();
            string fileNameMissionEnvironment = string.Format("{0}_Environment.mis", fileName);
            string filePathMissionEnvironment = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, fileNameMissionEnvironment);
            if (SilkySkyCloDFile.CopySection(fileSorce, fileMissionEnvironment, MissionFile.SectionParts) == 0)
            {
                ErrorMsg.Add(string.Format(ErrorFormatSectionOrKey, MissionFile.SectionParts));
            }
            if (SilkySkyCloDFile.CopySection(fileSorce, fileMissionEnvironment, MissionFile.SectionMain) == 0)
            {
                ErrorMsg.Add(string.Format(ErrorFormatSectionOrKey, MissionFile.SectionMain));
            }
            int i = 0;
            while (SilkySkyCloDFile.CopySection(fileSorce, fileMissionEnvironment, string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}_{1}", MissionFile.SectionGlobalWind, i)) > 0)
            {
                i++;
            }

            // 6. Create Mission staticTemplate File
            ISectionFile fileMissionStatic = gameInterface.SectionFileCreate();
            string fileNameMissionStatic = string.Format("{0}_Static.mis", fileName);
            string filePathMissionStatic = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, fileNameMissionStatic);
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionParts);
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionMain);
            //while (SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}_{1}", MissionFile.SectionGlobalWind, i)) > 0)
            //{
            //    i++;
            //}
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionSplines);
            IEnumerable<string> countries = Country.ToStrings().Select(x => string.Format(" {0} ", x));
            string replaceCountry = string.Format(" {0} ", ECountry.nn.ToString());
            IEnumerable<string> keys = SilkySkyCloDFile.CopySectionGetKey(fileSorce, fileMissionStatic, MissionFile.SectionChiefs);
            foreach (var item in keys)
            {
                SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, string.Format("{0}_{1}", item, MissionFile.SectionRoad));
            }
            keys = SilkySkyCloDFile.CopySectionGetKey(fileSorce, fileMissionStatic, MissionFile.SectionCustomChiefs);
            foreach (var item in keys)
            {
                SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, item);
            }
            SilkySkyCloDFile.CopySectionReplace(fileSorce, fileMissionStatic, MissionFile.SectionStationary, countries, replaceCountry);
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionBuildings);
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionBuildingsLinks);
            keys = SilkySkyCloDFile.CopySectionGetKey(fileSorce, fileMissionStatic, MissionFile.SectionAirdromes);
            foreach (var item in keys)
            {
                SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, string.Format("{0}_{1}", item, MissionFile.KeyRunways));
                SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, string.Format("{0}_{1}", item, MissionFile.KeyPoints));
            }
            if (SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionFrontMarker) == 0)
            {
                ErrorMsg.Add(string.Format(ErrorFormatSectionOrKey, MissionFile.SectionFrontMarker));
            }
            // SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionTrigger);
            // SilkySkyCloDFile.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionAction);

            // 7. Create Mission initialTemplate File
            ISectionFile fileMissionInitial = gameInterface.SectionFileCreate();
            string fileNameMissionInitial = string.Format("{0}_Initial.mis", fileName);
            string filePathMissionInitial = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, fileNameMissionInitial);
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, MissionFile.SectionParts);
            SilkySkyCloDFile.CopySection(fileSorce, fileMissionInitial, MissionFile.SectionMain);
            foreach (var army in armys)
            {
                var airGroupsArmy = airGroups.Where(x => x.ArmyIndex == army).OrderBy(x => x.Id);
                foreach (var airGroup in airGroupsArmy)
                {
                    airGroup.WriteTo(fileMissionInitial);
                }
            }

            // 8. Create Mission Script File
            string filePathScriptSystemPathSrc = string.Format("{0}\\{1}", gameInterface.ToFileSystemPath(Config.CampaignsFolderDefault), Config.MissionScriptFileName);
            string filePathScriptSystemPathDst = string.Format("{0}\\{1}\\{2}", outputBasetFolderrSystemPath, fileName, Config.MissionScriptFileName);

            // 9. Create CampaignInfo.ini 
            ISectionFile fileCampaign = gameInterface.SectionFileCreate();
            string filePathCampaign = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, Config.CampaignInfoFileName);
            CampaignInfo campaignInfo = new CampaignInfo(name, fileNameMissionEnvironment, new string[] { fileNameMissionStatic },
                new string[] { fileNameMissionInitial }, Config.MissionScriptFileName, new System.DateTime(1940, 07, 10), new System.DateTime(1940, 8, 11));
            campaignInfo.Write(fileCampaign);

            if (ErrorMsg.Count == 0)
            {
                // 10. Create Mission Folder (Mission Filename without Extension)
                string missionFolderSystemPath = string.Format("{0}\\{1}", outputBasetFolderrSystemPath, fileName);
                if (!Directory.Exists(missionFolderSystemPath))
                {
                    Directory.CreateDirectory(missionFolderSystemPath);
                }

                // 11. Save All Files
                fileAircraft.save(filePathAircraft);
                fileAirGroup.save(filePathAirGroup);
                fileMissionEnvironment.save(filePathMissionEnvironment);
                fileMissionStatic.save(filePathMissionStatic);
                fileMissionInitial.save(filePathMissionInitial);
                File.Copy(filePathScriptSystemPathSrc, filePathScriptSystemPathDst, true);
                fileCampaign.save(filePathCampaign);

                CovertedMission.Add(name);
            }

            return ErrorMsg.Count == 0;
        }

        public bool ConvertFolder(string srcFolder, string destFolder = null, bool useFolderName = false, string fileSearchPattern = DefaultFileSearchPettern)
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

        //public static bool Convert(string filePath, string outputBasetFolder)
        //{

        //    return true;
        //}


        public int CountFiles(IEnumerable<string> folders, string fileSearchPattern = DefaultFileSearchPettern)
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

        public IEnumerable<string> GetFiles(IEnumerable<string> folders, string fileSearchPattern = DefaultFileSearchPettern)
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

        public bool IsTargetFolder(string folder)
        {
            if (!string.IsNullOrEmpty(folder))
            {
                folder = folder.TrimStart();
                return folder.StartsWith(Config.HomeFolder) || folder.StartsWith(Config.UserFolder);
            }
            return false;
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
    }
}