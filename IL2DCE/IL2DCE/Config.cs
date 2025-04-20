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
        public const string MissionScriptFileName = "MissionSingle.cs";
        public const string UserMissionFolder = "$user/mission/IL2DCE";
        public const string UserMissionsFolder = "$user/missions/IL2DCE";
        public const string DebugFolderName = "debug";
        public const string DebugMissionTemplateFileName = "IL2DCEDebugTemplate.mis";
        public const string DebugMissionFileName = "IL2DCEDebug.mis";
        public const string DebugBriefingFileName = "IL2DCEDebug.briefing";
        public const string DebugMissionScriptFileName = "IL2DCEDebug.cs";
        public const string MissionStatusStartFileName = "MissionStatusStart.ini";
        public const string MissionStatusEndFileName = "MissionStatusEnd.ini";
        public const string MissionStatusResultFileName = "MissionStatusResult.ini";
        public const string RecordFileExt = ".trk";
        public const string MissionFileExt = ".mis";
        public const string ScriptFileExt = ".cs";
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
        public const string KeyKillsHistoryMax = "KillsHistoryMax";
        public const string KeyRandomTimeBegin = "RandomTimeBegin";
        public const string KeyRandomTimeEnd = "RandomTimeEnd";
        public const string KeyGroupDisableRate = "GroupDisableRate";
        public const string KeyReinForceDay = "ReinForceDay";

        public const string LogFileName = "il2dce.log";
        public const string ConvertLogFileName = "Convert.log";

        public const string GeneralSettingsFileName = "GeneralSettings.ini";
        public const string DQMSettingFileName = "DQMSetting.ini";
        
        public const string DefaultFixedFontName = "Consolas";
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
        public const int DefaultProcessTimeReArm = 300;
        public const int DefaultProcessTimeReFuel = 300;
        public const int KillsHistoryMaxDefault = 1000;

        public const int GroundGroupFormationCountDefault = 3;

        public const int RankupExp = 1000;
        public const int ExpSuccess = 200;
        public const int ExpFail = 0;
        public const int ExpDraw = 100;
        public const float GroupDisableRateDefault = 0.25f;
        public const int ReinForceDayDefault = 3;

        #endregion

        public string CampaignsFolder
        {
            get
            {
                return _campaignsFolder;
            }
        }
        private string _campaignsFolder;

        public int AdditionalAirOperations
        {
            get
            {
                return _additionalAirOperations;
            }
        }
        private int _additionalAirOperations = DefaultAdditionalAirOperations;

        public int AdditionalGroundOperations
        {
            get
            {
                return _additionalGroundOperations;
            }
        }
        private int _additionalGroundOperations = DefaultAdditionalGroundOperations;

        public double FlightSize
        {
            get
            {
                return _flightSize;
            }
        }
        private double _flightSize = 1.0;

        public double FlightCount
        {
            get
            {
                return _flightCount;
            }
        }
        private double _flightCount = 1.0;

        public bool SpawnParked
        {
            get
            {
                return _spawnParked;
            }
            set
            {
                _spawnParked = value;
            }
        }
        public static bool _spawnParked = false;

        public int Debug
        {
            get
            {
                return _debug;
            }
            set
            {
                _debug = value;
            }
        }
        private int _debug = 0;

        public int StatType
        {
            get
            {
                return _statType;
            }
            set
            {
                _statType = value;
            }
        }
        private int _statType = 0;

        public double StatKillsOver
        {
            get
            {
                return _statKillsOver;
            }
            set
            {
                _statKillsOver = value;
            }
        }
        private double _statKillsOver = 0.5;

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

        #region Random Unit

        public string[] AircraftRandomRed
        {
            get;
            private set;
        }

        public string[] AircraftRandomBlue
        {
            get;
            private set;
        }

        public string[] GroundVehicleRandomRed
        {
            get;
            private set;
        }

        public string[] GroundVehicleRandomBlue
        {
            get;
            private set;
        }

        public string[] GroundArmorRandomRed
        {
            get;
            private set;
        }

        public string[] GroundArmorRandomBlue
        {
            get;
            private set;
        }

        public string[] GroundShipRandomRed
        {
            get;
            private set;
        }

        public string[] GroundShipRandomBlue
        {
            get;
            private set;
        }

        public string[] GroundTrainRandomRed
        {
            get;
            private set;
        }

        public string[] GroundTrainRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryRadarRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryRadarRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryAircraftRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryAircraftRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryArtilleryRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryArtilleryRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryFlakRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryFlakRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryDepotRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryDepotRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryShipRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryShipRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryAmmoRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryAmmoRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryWeaponsRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryWeaponsRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryCarRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryCarRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryConstCarRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryConstCarRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryEnvironmentRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryEnvironmentRandomBlue
        {
            get;
            private set;
        }

        public string[] StationarySearchlightRandomRed
        {
            get;
            private set;
        }

        public string[] StationarySearchlightRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryAeroanchoredRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryAeroanchoredRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryAirfieldRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryAirfieldRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryUnknownRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryUnknownRandomBlue
        {
            get;
            private set;
        }

        #endregion

        public bool EnableMissionMultiAssign
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

        // public static CultureInfo Culture = new CultureInfo("en-US", true);
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
                _campaignsFolder = confFile.get(SectionMain, "campaignsFolder");
            }
            else
            {
                _campaignsFolder = CampaignsFolderDefault;
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
                int.TryParse(value, NumberStyles.Integer, NumberFormat, out _additionalAirOperations);
            }

            if (confFile.exist(SectionCore, "additionalGroundOperations"))
            {
                value = confFile.get(SectionCore, "additionalGroundOperations");
                int.TryParse(value, NumberStyles.Integer, NumberFormat, out _additionalGroundOperations);
            }

            if (confFile.exist(SectionCore, "flightSize"))
            {
                value = confFile.get(SectionCore, "flightSize");
                double.TryParse(value, NumberStyles.Float, NumberFormat, out _flightSize);
            }

            if (confFile.exist(SectionCore, "flightCount"))
            {
                value = confFile.get(SectionCore, "flightCount");
                double.TryParse(value, NumberStyles.Float, NumberFormat, out _flightCount);
            }

            if (confFile.exist(SectionCore, "debug"))
            {
                value = confFile.get(SectionCore, "debug");
                int.TryParse(value, NumberStyles.Integer, NumberFormat, out _debug);
            }

            if (confFile.exist(SectionCore, "statType"))
            {
                value = confFile.get(SectionCore, "statType");
                int.TryParse(value, NumberStyles.Integer, NumberFormat, out _statType);
            }

            if (confFile.exist(SectionCore, "statKillsOver"))
            {
                value = confFile.get(SectionCore, "statKillsOver");
                double.TryParse(value, NumberStyles.Float, NumberFormat, out _statKillsOver);
            }

            ProcessTimeReArm = confFile.get(SectionCore, KeyProcessTimeReArm, DefaultProcessTimeReArm);
            ProcessTimeReFuel = confFile.get(SectionCore, KeyProcessTimeReFuel, DefaultProcessTimeReFuel);

            if (confFile.exist(SectionCore, "statType"))
            {
                value = confFile.get(SectionCore, "statType");
                int.TryParse(value, NumberStyles.Integer, NumberFormat, out _statType);
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

            ReadRandomUnitInfo(confFile);

            EnableMissionMultiAssign = confFile.get(SectionCore, KeyEnableMissionMultiAssign, 0) == 1;

            KillsHistoryMax = confFile.get(SectionCore, KeyKillsHistoryMax, KillsHistoryMaxDefault);
            RandomTimeBegin = confFile.get(SectionCore, KeyRandomTimeBegin, (int)MissionTime.Begin);
            RandomTimeEnd = confFile.get(SectionCore, KeyRandomTimeEnd, (int)MissionTime.End);

            value = confFile.get(SectionCore, KeyGroupDisableRate, string.Empty);
            float fValue;
            GroupDisableRate = float.TryParse(value, NumberStyles.Float, NumberFormat, out fValue) ? fValue : GroupDisableRateDefault;
            ReinForceDay = confFile.get(SectionCore, KeyReinForceDay, ReinForceDayDefault);
        }

        private void ReadRandomUnitInfo(ISectionFile confFile)
        {
            string value;

            if (confFile.exist(SectionAircraft, KeyRandomRed))
            {
                value = confFile.get(SectionAircraft, KeyRandomRed);
                AircraftRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomRed = new string[0];
            }

            if (confFile.exist(SectionAircraft, KeyRandomBlue))
            {
                value = confFile.get(SectionAircraft, KeyRandomBlue);
                AircraftRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomBlue = new string[0];
            }

            if (confFile.exist(SectionGroundVehicle, KeyRandomRed))
            {
                value = confFile.get(SectionGroundVehicle, KeyRandomRed);
                GroundVehicleRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundVehicleRandomRed = new string[0];
            }

            if (confFile.exist(SectionGroundVehicle, KeyRandomBlue))
            {
                value = confFile.get(SectionGroundVehicle, KeyRandomBlue);
                GroundVehicleRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundVehicleRandomBlue = new string[0];
            }

            if (confFile.exist(SectionGroundArmor, KeyRandomRed))
            {
                value = confFile.get(SectionGroundArmor, KeyRandomRed);
                GroundArmorRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundArmorRandomRed = new string[0];
            }

            if (confFile.exist(SectionGroundArmor, KeyRandomBlue))
            {
                value = confFile.get(SectionGroundArmor, KeyRandomBlue);
                GroundArmorRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundArmorRandomBlue = new string[0];
            }

            if (confFile.exist(SectionGroundShip, KeyRandomRed))
            {
                value = confFile.get(SectionGroundShip, KeyRandomRed);
                GroundShipRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundShipRandomRed = new string[0];
            }

            if (confFile.exist(SectionGroundShip, KeyRandomBlue))
            {
                value = confFile.get(SectionGroundShip, KeyRandomBlue);
                GroundShipRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundShipRandomBlue = new string[0];
            }

            if (confFile.exist(SectionGroundTrain, KeyRandomRed))
            {
                value = confFile.get(SectionGroundTrain, KeyRandomRed);
                GroundTrainRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundTrainRandomRed = new string[0];
            }

            if (confFile.exist(SectionGroundTrain, KeyRandomBlue))
            {
                value = confFile.get(SectionGroundTrain, KeyRandomBlue);
                GroundTrainRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundTrainRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryRadar, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryRadar, KeyRandomRed);
                StationaryRadarRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryRadarRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryRadar, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryRadar, KeyRandomBlue);
                StationaryRadarRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryRadarRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryAircraft, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryAircraft, KeyRandomRed);
                StationaryAircraftRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAircraftRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryAircraft, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryAircraft, KeyRandomBlue);
                StationaryAircraftRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAircraftRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryArtillery, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryArtillery, KeyRandomRed);
                StationaryArtilleryRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryArtilleryRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryArtillery, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryArtillery, KeyRandomBlue);
                StationaryArtilleryRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryArtilleryRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryFlak, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryFlak, KeyRandomRed);
                StationaryFlakRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryFlakRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryFlak, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryFlak, KeyRandomBlue);
                StationaryFlakRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryFlakRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryDepot, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryDepot, KeyRandomRed);
                StationaryDepotRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryDepotRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryDepot, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryDepot, KeyRandomBlue);
                StationaryDepotRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryDepotRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryShip, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryShip, KeyRandomRed);
                StationaryShipRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryShipRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryShip, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryShip, KeyRandomBlue);
                StationaryShipRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryShipRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryAmmo, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryAmmo, KeyRandomRed);
                StationaryAmmoRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAmmoRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryAmmo, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryAmmo, KeyRandomBlue);
                StationaryAmmoRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAmmoRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryWeapons, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryWeapons, KeyRandomRed);
                StationaryWeaponsRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryWeaponsRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryWeapons, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryWeapons, KeyRandomBlue);
                StationaryWeaponsRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryWeaponsRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryCar, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryCar, KeyRandomRed);
                StationaryCarRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryCarRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryCar, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryCar, KeyRandomBlue);
                StationaryCarRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryCarRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryConstCar, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryConstCar, KeyRandomRed);
                StationaryConstCarRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryConstCarRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryConstCar, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryConstCar, KeyRandomBlue);
                StationaryConstCarRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryConstCarRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryEnvironment, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryEnvironment, KeyRandomBlue);
                StationaryEnvironmentRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryEnvironmentRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryEnvironment, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryEnvironment, KeyRandomRed);
                StationaryEnvironmentRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryEnvironmentRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationarySearchlight, KeyRandomBlue))
            {
                value = confFile.get(SectionStationarySearchlight, KeyRandomBlue);
                StationarySearchlightRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationarySearchlightRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationarySearchlight, KeyRandomRed))
            {
                value = confFile.get(SectionStationarySearchlight, KeyRandomRed);
                StationarySearchlightRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationarySearchlightRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryAeroanchored, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryAeroanchored, KeyRandomBlue);
                StationaryAeroanchoredRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAeroanchoredRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryAeroanchored, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryAeroanchored, KeyRandomRed);
                StationaryAeroanchoredRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAeroanchoredRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryAirfield, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryAirfield, KeyRandomBlue);
                StationaryAirfieldRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAirfieldRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryAirfield, KeyRandomRed))
            {
                value = confFile.get(SectionStationaryAirfield, KeyRandomRed);
                StationaryAirfieldRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAirfieldRandomBlue = new string[0];
            }

            if (confFile.exist(SectionStationaryUnknown, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryUnknown, KeyRandomBlue);
                StationaryUnknownRandomRed = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryUnknownRandomRed = new string[0];
            }

            if (confFile.exist(SectionStationaryUnknown, KeyRandomBlue))
            {
                value = confFile.get(SectionStationaryUnknown, KeyRandomBlue);
                StationaryUnknownRandomBlue = value.Split(SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryUnknownRandomBlue = new string[0];
            }
        }
    }
}
