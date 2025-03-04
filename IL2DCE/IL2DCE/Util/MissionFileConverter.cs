// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
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
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using maddox.game;

namespace IL2DCE.Util
{
    public class MissionFileConverter
    {
        public const string DefaultFileSearchPettern = "*.mis";

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

            //    3. & 4. 
            var armys = missionFile.AirGroups.Select(x => x.ArmyIndex).Distinct().OrderBy(x => x);
            foreach (var army in armys)
            {
                var airGroups = missionFile.AirGroups.Where(x => x.ArmyIndex == army).OrderBy(x => x.Id);
                foreach (var airGroup in airGroups)
                {
                    if (globalAircraftInfoFile.exist(AircraftInfo.SectionMain, airGroup.Class))
                    {
                        if (airGroup.AirGroupInfo != null)
                        {
                            try
                            {
                                AircraftInfo aircraftInfo = new AircraftInfo(globalAircraftInfoFile, airGroup.Class);
                                aircraftInfo.Write(fileAircraft);           //  AircraftInfo.ini
                                airGroup.AirGroupInfo.Write(fileAirGroup, airGroup.AirGroupKey, airGroup.Class);  //  AirGroupInfo.ini
                            }
                            catch (Exception ex)
                            {
                                ErrorMsg.Add(string.Format("Error [{0}] AirGroup Info[{1}] MissionFile:[{2}]", ex.Message, airGroup.AirGroupKey, name));
                            }
                        }
                        else
                        {
                            ErrorMsg.Add(string.Format("No AirGroup Info[{0}] MissionFile:[{1}]", airGroup.AirGroupKey, name));
                        }
                    }
                    else
                    {
                        ErrorMsg.Add(string.Format("No Aircraft Info[{0}] MissionFile:[{1}]", airGroup.Class, name));
                    }
                }
            }

            // 5. Create Mission environmentTemplate File
            ISectionFile fileMissionEnvironment = gameInterface.SectionFileCreate();
            string fileNameMissionEnvironment = string.Format("{0}_Environment.mis", fileName);
            string filePathMissionEnvironment = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, fileNameMissionEnvironment);
            SectionFileUtil.CopySection(fileSorce, fileMissionEnvironment, MissionFile.SectionParts);
            SectionFileUtil.CopySection(fileSorce, fileMissionEnvironment, MissionFile.SectionMain);

            // 6. Create Mission staticTemplate File
            ISectionFile fileMissionStatic = gameInterface.SectionFileCreate();
            string fileNameMissionStatic = string.Format("{0}_Static.mis", fileName);
            string filePathMissionStatic = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, fileNameMissionStatic);
            SectionFileUtil.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionParts);
            SectionFileUtil.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionMain);
            SectionFileUtil.CopySection(fileSorce, fileMissionStatic, string.Format("{0}_{1}", MissionFile.SectionGlobalWind, "0"));
            SectionFileUtil.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionSplines);
            SectionFileUtil.CopySectionReplace(fileSorce, fileMissionStatic, MissionFile.SectionCustomChiefs, MissionFile.Country, ECountry.nn.ToString());
            IEnumerable<string> keys = SectionFileUtil.CopySectionReplaceGetKey(fileSorce, fileMissionStatic, MissionFile.SectionChiefs, MissionFile.Country, ECountry.nn.ToString());
            foreach (var item in keys)
            {
                SectionFileUtil.CopySection(fileSorce, fileMissionStatic, string.Format("{0}_{1}", item, MissionFile.SectionRoad));
            }
            SectionFileUtil.CopySectionReplace(fileSorce, fileMissionStatic, MissionFile.SectionStationary, MissionFile.Country, ECountry.nn.ToString());
            SectionFileUtil.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionBuildings);
            SectionFileUtil.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionBuildingsLinks);
            keys = SectionFileUtil.CopySectionGetKey(fileSorce, fileMissionStatic, MissionFile.SectionAirdromes);
            foreach (var item in keys)
            {
                SectionFileUtil.CopySection(fileSorce, fileMissionStatic, string.Format("{0}_{1}", item, MissionFile.KeyRunways));
                SectionFileUtil.CopySection(fileSorce, fileMissionStatic, string.Format("{0}_{1}", item, MissionFile.KeyPoints));
            }
            SectionFileUtil.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionFrontMarker);
            SectionFileUtil.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionTrigger);
            SectionFileUtil.CopySection(fileSorce, fileMissionStatic, MissionFile.SectionAction);

            // 7. Create Mission initialTemplate File
            ISectionFile fileMissionInitial = gameInterface.SectionFileCreate();
            string fileNameMissionInitial = string.Format("{0}_Initial.mis", fileName);
            string filePathMissionInitial = string.Format("{0}/{1}/{2}", outputBasetFolder, fileName, fileNameMissionInitial);
            SectionFileUtil.CopySection(fileSorce, fileMissionInitial, MissionFile.SectionParts);
            SectionFileUtil.CopySection(fileSorce, fileMissionInitial, MissionFile.SectionMain);
            foreach (var army in armys)
            {
                var airGroups = missionFile.AirGroups.Where(x => x.ArmyIndex == army).OrderBy(x => x.Id);
                foreach (var airGroup in airGroups)
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

            return ErrorMsg.Count == 0;
        }

        public bool ConvertFolder(string srcFolder, string destFolder = null, bool useFolderName = false, string fileSearchPattern = DefaultFileSearchPettern)
        {
            int error = 0;

            string folderSystemPath = gameInterface.ToFileSystemPath(srcFolder.Trim());
            if (Directory.Exists(folderSystemPath))
            {
                string nameDir = folderSystemPath.Split(Path.DirectorySeparatorChar).LastOrDefault();
                string[] filesPath = Directory.GetFiles(folderSystemPath, "*.mis");
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
                    files += Directory.GetFiles(folderSystemPath, fileSearchPattern, SearchOption.AllDirectories).Length;
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
                    files.AddRange(Directory.GetFiles(folderSystemPath, fileSearchPattern, SearchOption.AllDirectories));
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

        public string [] SplitTargetSystemPathInfo(string path)
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

    }
}