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
using System.Globalization;
using System.Linq;
using System.Reflection;
using IL2DCE.MissionObjectModel;
using maddox.game;

namespace IL2DCE
{
    public class Config
    {

        #region Definition

        public const char Comma = ',';
        public const string CommaStr = ",";
        public const char Or = '|';
        public static readonly char[] SplitSpace = new char[] { ' ' };
        public static readonly char[] SplitComma = new char[] { ',' };
        public static readonly char[] SplitOr = new char[] { '|' };
        public static readonly char[] SplitDQ = new char[] { '"' };

        public const string AppName = "IL2DCE";
        public const string HomeFolder = "$home/";
        public const string UserFolder = "$user/";
        public const string PartsFolder = "$home/parts";
        public const string CampaignsFolderDefault = "$home/parts/IL2DCE/Campaigns";
        public const string MissionFolderSingle = "$home/missions/Single";
        public const string MissionFolderFormatSingle = "$home/parts/{0}/missions/Single";
        public const string MissionFolderFormatQuick = "$home/parts/{0}/mission/Quick";
        public const string MissionFolderFormatCampaign = "$home/parts/{0}/mission/campaign";
        public const string RecordFolder = "$user/records";

        public const string ConfigFilePath = "$home/parts/IL2DCE/conf.ini";
        public const string CampaignInfoFileName = "CampaignInfo.ini";
        public const string AircraftInfoFileName = "AircraftInfo.ini";
        public const string AirGroupInfoFileName = "AirGroupInfo.ini";
        public const string CareerInfoFileName = "Career.ini";
        public const string StatsInfoFileName = "Stats.ini";
        public const string CampaignFileName = "Campaign.ini";
        public const string MissionScriptFileName = "MissionSingle.cs";
        public const string UserMissionFolder = "$user/mission/IL2DCE";
        public const string UserMissionsFolder = "$user/missions/IL2DCE";
        public const string UserMissionsDefaultFolder = "$user/missions";
        public const string DebugFolderName = "debug";
        public const string DebugFileName = "IL2DCEDebug";
        public const string MissionStatusStartFileName = "MissionStatusStart.ini";
        public const string MissionStatusEndFileName = "MissionStatusEnd.ini";
        public const string MissionStatusResultFileName = "MissionStatusResult.ini";
        public const string RecordFileExt = ".trk";
        public const string MissionFileExt = ".mis";
        public const string ScriptFileExt = ".cs";
        public const string BriefingFileExt = ".briefing";
        
        public const string IniFileExt = ".ini";

        public const string SectionMain = "Main";
        public const string SectionCore = "Core";
        public const string SectionMissionFileConverter = "MissionFileConverter";
        public const string SectionQuickMissionPage = "QuickMissionPage";
        public const string SectionSkill = "Skill";
        public const string SectionMissionType = "MissionType";

        public const string SectionAircraft = "Aircraft";
        public const string SectionGroundVehicle = "Ground.Vehicle";
        public const string SectionGroundArmor = "Ground.Armor";
        public const string SectionGroundShip = "Ground.Ship";
        public const string SectionGroundTrain = "Ground.Train";
        public const string SectionStationaryRadar = "Stationary.Radar";
        public const string SectionStationaryAircraft = "Stationary.Aircraft";
        public const string SectionStationaryArtillery = "Stationary.Artillery";
        public const string SectionStationaryFlak = "Stationary.Flak";
        public const string SectionStationaryDepot = "Stationary.Depot";
        public const string SectionStationaryShip = "Stationary.Ship";
        public const string SectionStationaryAmmo = "Stationary.Ammo";
        public const string SectionStationaryWeapons = "Stationary.Weapons";
        public const string SectionStationaryCar = "Stationary.Car";
        public const string SectionStationaryConstCar = "Stationary.ConstCar";
        public const string SectionStationaryEnvironment = "Stationary.Environment";
        public const string SectionStationarySearchlight = "Stationary.Searchlight";
        public const string SectionStationaryAeroanchored = "Stationary.Aeroanchored";
        public const string SectionStationaryAirfield = "Stationary.Airfield";
        public const string SectionStationaryUnknown = "Stationary.Unknown";
        public const string SectionBattles = "Battles";


        public const string KeySourceFolderFileName = "SourceFolderFileName";
        public const string KeySourceFolderFolderName = "SourceFolderFolderName";
        public const string KeyEnableFilterSelectCampaign = "EnableFilterSelectCampaign";
        public const string KeyEnableFilterSelectAirGroup = "EnableFilterSelectAirGroup";
        public const string KeyDisableMissionType = "Disable";
        public const string KeyRandomRed = "RandomRed";
        public const string KeyRandomBlue = "RandomBlue";
        public const string KeyEnableMissionMultiAssign = "EnableMissionMultiAssign";
        public const string KeyProcessTimeReArm = "ProcessTimeReArm";
        public const string KeyProcessTimeReFuel = "ProcessTimeReFuel";
        public const string KeyProcessInterval = "ProcessInterval";
        public const string KeyKillsHistoryMax = "KillsHistoryMax";
        public const string KeyRandomTimeBegin = "RandomTimeBegin";
        public const string KeyRandomTimeEnd = "RandomTimeEnd";
        public const string KeyGroupDisableRate = "GroupDisableRate";
        public const string KeyReinForceDay = "ReinForceDay";
        public const string KeyGroupNotAliveToDestroy = "GroupNotAliveToDestroy";
        public const string KeyNoCheckBattleGoal = "NoCheckBattleGoal";
        public const string KeyMissionCompletedTime = "MissionCompletedTime";

        public const string DynamicSpawnFileName = "DynamicSpawn";

        public const string LogFileName = "il2dce.log";
        public const string ConvertLogFileName = "Convert.log";

        public const string GeneralSettingsFileName = "GeneralSettings.ini";
        public const string DQMSettingFileName = "DQMSetting.ini";
        
        public const string DefaultFixedFontName = "Consolas,Courier New";
        public const string KillsFormat = "F0";
        public const string PointValueFormat = "F2";
        public const string DateTimeDefaultLongFormat = "yyyyMMdd_HHmmss";
        public const string DateTimeDefaultLongLongFormat = "yyyy/MM/dd HH:mm:ss.fff";

        public const int AddGroundGroupStartIdNo = 10000;
        public const int AddStationaryUnitStartIdNo = 10000;

        public const int DefaultAdditionalAirOperations = 3;
        public const int MaxAdditionalAirOperations = 12;
        public const int MinAdditionalAirOperations = 1;
        public const int DefaultAdditionalGroundOperations = 100;
        public const int MaxAdditionalGroundOperations = 300;
        public const int MinAdditionalGroundOperations = 10;
        public const int DefaultProcessInterval = 30;
        public const int DefaultProcessTimeReArm = 300;
        public const int DefaultProcessTimeReFuel = 300;
        public const int DefaultKillsHistoryMax = 1000;

        public const int GroundGroupFormationCountDefault = 3;

        public const int AverageAirOperationAirGroupCount = 3;
        public const int AverageGroundOperationGroundGroupCount = 1;
        public const int AverageStationaryOperationUnitCount = 1;

        public const int RankupExp = 1000;
        public const int ExpSuccess = 200;
        public const int ExpFail = 0;
        public const int ExpDraw = 100;
        public const float DefaultGroupDisableRate = 0.25f;
        public const int DefaultReinForceDay = 3;

        public const int DefaultMissionCompletedTime = 300;

        #endregion

        public string CampaignsFolder
        {
            get
            {
                return campaignsFolder;
            }
        }
        private string campaignsFolder;

        public int AdditionalAirOperations
        {
            get
            {
                return additionalAirOperations;
            }
        }
        private int additionalAirOperations = DefaultAdditionalAirOperations;

        public int AdditionalGroundOperations
        {
            get
            {
                return additionalGroundOperations;
            }
        }
        private int additionalGroundOperations = DefaultAdditionalGroundOperations;

        public double FlightSize
        {
            get
            {
                return flightSize;
            }
        }
        private double flightSize = 1.0;

        public double FlightCount
        {
            get
            {
                return flightCount;
            }
        }
        private double flightCount = 1.0;

        public bool SpawnParked
        {
            get
            {
                return spawnParked;
            }
            set
            {
                spawnParked = value;
            }
        }
        public static bool spawnParked = false;

        public int Debug
        {
            get
            {
                return debug;
            }
            set
            {
                debug = value;
            }
        }
        private int debug = 0;

        public int StatType
        {
            get
            {
                return statType;
            }
            set
            {
                statType = value;
            }
        }
        private int statType = 0;

        public double StatKillsOver
        {
            get
            {
                return statKillsOver;
            }
            set
            {
                statKillsOver = value;
            }
        }
        private double statKillsOver = 0.5;

        public string[] SorceFolderFileName
        {
            get
            {
                return sorceFolderFileName;
            }
        }
        private string[] sorceFolderFileName;

        public string[] SorceFolderFolderName
        {
            get
            {
                return sorceFolderFolderName;
            }
        }
        private string[] sorceFolderFolderName;

        public bool EnableFilterSelectCampaign
        {
            get;
            private set;
        }

        public bool EnableFilterSelectAirGroup
        {
            get;
            private set;
        }

        public Skills Skills
        {
            get;
            private set;
        }

        public EMissionType[] DisableMissionType
        {
            get;
            private set;
        }

        public RandomUnitSet RandomUnits
        {
            get;
            private set;
        }

        public bool EnableMissionMultiAssign
        {
            get;
            private set;
        }

        public int ProcessInterval
        {
            get;
            private set;
        }

        public int ProcessTimeReArm
        {
            get;
            private set;
        }

        public int ProcessTimeReFuel
        {
            get;
            private set;
        }

        public int KillsHistoryMax
        {
            get;
            private set;
        }

        public int RandomTimeBegin
        {
            get;
            private set;
        }

        public int RandomTimeEnd
        {
            get;
            private set;
        }

        public float GroupDisableRate
        {
            get;
            private set;
        }

        public int ReinForceDay
        {
            get;
            private set;
        }

        public bool GroupNotAliveToDestroy
        {
            get;
            private set;
        }

        public bool NoCheckBattleGoal
        {
            get;
            private set;
        }

        public int MissionCompletedTime
        {
            get;
            private set;
        }

        public static NumberFormatInfo NumberFormat = CultureInfo.InvariantCulture.NumberFormat;
        public static DateTimeFormatInfo DateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
        
        public static Version Version
        {
            get;
        }

        static Config()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static string CreateVersionString(Version targetVersion)
        {
            return string.Format("Version {0} [Core{1}]", targetVersion.ToString(), Version.ToString());
        }

        public Config(ISectionFile confFile)
        {
            string value;
            if (confFile.exist(SectionMain, "campaignsFolder"))
            {
                campaignsFolder = confFile.get(SectionMain, "campaignsFolder");
            }
            else
            {
                campaignsFolder = CampaignsFolderDefault;
            }

            SpawnParked = false;
            if (confFile.exist(SectionCore, "forceSetOnPark"))
            {
                value = confFile.get(SectionCore, "forceSetOnPark");
                if (value == "1")
                {
                    SpawnParked = true;
                }
                else
                {
                    SpawnParked = false;
                }
            }

            if (confFile.exist(SectionCore, "additionalAirOperations"))
            {
                value = confFile.get(SectionCore, "additionalAirOperations");
                int.TryParse(value, NumberStyles.Integer, NumberFormat, out additionalAirOperations);
            }

            if (confFile.exist(SectionCore, "additionalGroundOperations"))
            {
                value = confFile.get(SectionCore, "additionalGroundOperations");
                int.TryParse(value, NumberStyles.Integer, NumberFormat, out additionalGroundOperations);
            }

            if (confFile.exist(SectionCore, "flightSize"))
            {
                value = confFile.get(SectionCore, "flightSize");
                double.TryParse(value, NumberStyles.Float, NumberFormat, out flightSize);
            }

            if (confFile.exist(SectionCore, "flightCount"))
            {
                value = confFile.get(SectionCore, "flightCount");
                double.TryParse(value, NumberStyles.Float, NumberFormat, out flightCount);
            }

            if (confFile.exist(SectionCore, "debug"))
            {
                value = confFile.get(SectionCore, "debug");
                int.TryParse(value, NumberStyles.Integer, NumberFormat, out debug);
            }

            if (confFile.exist(SectionCore, "statType"))
            {
                value = confFile.get(SectionCore, "statType");
                int.TryParse(value, NumberStyles.Integer, NumberFormat, out statType);
            }

            if (confFile.exist(SectionCore, "statKillsOver"))
            {
                value = confFile.get(SectionCore, "statKillsOver");
                double.TryParse(value, NumberStyles.Float, NumberFormat, out statKillsOver);
            }

            ProcessInterval = confFile.get(SectionCore, KeyProcessInterval, DefaultProcessInterval);
            ProcessTimeReArm = confFile.get(SectionCore, KeyProcessTimeReArm, DefaultProcessTimeReArm);
            ProcessTimeReFuel = confFile.get(SectionCore, KeyProcessTimeReFuel, DefaultProcessTimeReFuel);

            if (confFile.exist(SectionCore, "statType"))
            {
                value = confFile.get(SectionCore, "statType");
                int.TryParse(value, NumberStyles.Integer, NumberFormat, out statType);
            }

            if (confFile.exist(SectionMissionFileConverter, KeySourceFolderFileName))
            {
                value = confFile.get(SectionMissionFileConverter, KeySourceFolderFileName);
                sorceFolderFileName = string.IsNullOrEmpty(value) ? new string[0] : value.Split(SplitComma, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                sorceFolderFileName = new string[0];
            }

            if (confFile.exist(SectionMissionFileConverter, KeySourceFolderFolderName))
            {
                value = confFile.get(SectionMissionFileConverter, KeySourceFolderFolderName);
                sorceFolderFolderName = string.IsNullOrEmpty(value) ? new string[0] : value.Split(SplitComma, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                sorceFolderFolderName = new string[0];
            }

            EnableFilterSelectCampaign = confFile.get(SectionQuickMissionPage, KeyEnableFilterSelectCampaign, 0) == 1;
            EnableFilterSelectAirGroup = confFile.get(SectionQuickMissionPage, KeyEnableFilterSelectAirGroup, 0) == 1;

            Skills = Skills.CreateDefault();
            if (confFile.exist(SectionSkill))
            {
                string key;
                int lines = confFile.lines(SectionSkill);
                for (int i = 0; i < lines; i++)
                {
                    confFile.get(SectionSkill, i, out key, out value);
                    System.Diagnostics.Debug.WriteLine("Skill[{0}] name={1} Value={2}", i, key, value ?? string.Empty);
                    // if you need delete default defined skill, please write no value key in ini file.
                    var targetSkills = Skills.Where(x => string.Compare(x.Name, key, true) == 0).ToArray();
                    if (targetSkills.Any())
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            foreach (var item in targetSkills)
                            {
                                Skills.Remove(item);
                            }
                        }
                        else
                        {
                            Skill skill;
                            if (Skill.TryParse(value, out skill))
                            {
                                foreach (var item in targetSkills)
                                {
                                    item.Skills = skill.Skills;
                                }
                            }
                        }
                    }
                    else
                    {
                        Skill skill;
                        if (Skill.TryParse(value, out skill))
                        {
                            skill.Name = key;
                            Skills.Add(skill);
                        }
                    }
                }
            }

            if (confFile.exist(SectionMissionType, KeyDisableMissionType))
            {
                value = confFile.get(SectionMissionType, KeyDisableMissionType);
                string[] values = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
                List<EMissionType> missionTypes = new List<EMissionType>();
                foreach (var item in values)
                {
                    EMissionType missionType;
                    if (Enum.TryParse<EMissionType>(item, true, out missionType))
                    {
                        missionTypes.Add(missionType);
                    }
                }
                DisableMissionType = missionTypes.ToArray();
            }
            else
            {
                DisableMissionType = new EMissionType[0];
            }

            RandomUnits = new RandomUnitSet();
            RandomUnits.Read(confFile);

            EnableMissionMultiAssign = confFile.get(SectionCore, KeyEnableMissionMultiAssign, 0) == 1;

            KillsHistoryMax = confFile.get(SectionCore, KeyKillsHistoryMax, DefaultKillsHistoryMax);
            RandomTimeBegin = confFile.get(SectionCore, KeyRandomTimeBegin, (int)MissionTime.Begin);
            RandomTimeEnd = confFile.get(SectionCore, KeyRandomTimeEnd, (int)MissionTime.End);

            value = confFile.get(SectionCore, KeyGroupDisableRate, string.Empty);
            float fValue;
            GroupDisableRate = float.TryParse(value, NumberStyles.Float, NumberFormat, out fValue) ? fValue : DefaultGroupDisableRate;
            ReinForceDay = confFile.get(SectionCore, KeyReinForceDay, DefaultReinForceDay);

            GroupNotAliveToDestroy = confFile.get(SectionCore, KeyGroupNotAliveToDestroy, 0) == 1;

            NoCheckBattleGoal = confFile.get(SectionCore, KeyNoCheckBattleGoal, 0) == 1;

            MissionCompletedTime = confFile.get(SectionCore, KeyMissionCompletedTime, DefaultMissionCompletedTime);
        }
    }
}
