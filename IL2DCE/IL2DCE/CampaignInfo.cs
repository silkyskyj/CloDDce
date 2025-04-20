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
using IL2DCE.Generator;
using IL2DCE.MissionObjectModel;
using maddox.game;

namespace IL2DCE
{
    public enum ECampaignStatus
    {
        Empty,
        InProgress,
        DateEnd,
        Dead,
        Count,
    };

    /// <summary>
    /// The campaign info object holds the configuration of a campaign.
    /// </summary>
    public class CampaignInfo
    {

        #region Definition

        public const string SectionMain = "Main";
        public const string KeyName = "name";
        public const string KeyInitialTemplate = "initialTemplate";
        public const string KeyScriptFile = "scriptFile";
        public const string KeyStartDate = "startDate";
        public const string KeyEndDate = "endDate";
        public const string KeyDynamicFrontMarker = "DynamicFrontMarker";
        public const string FormatDate = "yyyy-MM-dd";
        
        /// <summary>
        /// Max Campaign Period
        /// </summary>
        public const int MaxCampaignPeriod = 730;

        #endregion

        #region Property & Variable

        /// <summary>
        /// The id of the campaign.
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
        }
        string _id;

        /// <summary>
        /// The name of the campaign.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }
        string name;

        /// <summary>
        /// The list of initial mission template files that contain the starting location of air and ground groups.
        /// </summary>
        public List<string> InitialMissionTemplateFiles
        {
            get
            {
                return _initialMissionTemplateFiles;
            }
        }
        private List<string> _initialMissionTemplateFiles = new List<string>();

        /// <summary>
        /// The name of the script file that will be used in the generated missions.
        /// </summary>
        public string ScriptFileName
        {
            get
            {
                return _scriptFileName;
            }
        }
        private string _scriptFileName;

        /// <summary>
        /// The start date of the campaign.
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                _startDate = value;
            }
        }
        private DateTime _startDate;

        /// <summary>
        /// The end date of the campaign.
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                _endDate = value;
            }
        }
        private DateTime _endDate;

        /// <summary>
        /// The end date of the campaign.
        /// </summary>
        public bool DynamicFrontMarker
        {
            get;
            private set;
        }

        public AirGroupInfos AirGroupInfos
        {
            get
            {
                return _localAirGroupInfos;
            }
        }
        private AirGroupInfos _localAirGroupInfos;

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

        public object Data
        {
            get;
            set;
        }

        private ISectionFile _globalAircraftInfoFile;
        private ISectionFile _localAircraftInfoFile;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="initialMissionTemplateFiles"></param>
        /// <param name="scriptFileName"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public CampaignInfo(string name, string[] initialMissionTemplateFiles, string scriptFileName, DateTime startDate, DateTime endDate)
        {
            this.name = name;
            _id = name;
            _initialMissionTemplateFiles = initialMissionTemplateFiles.ToList();
            _scriptFileName = scriptFileName;
            _startDate = startDate;
            _endDate = endDate;
            DynamicFrontMarker = false;
        }

        /// <summary>
        /// The constructor parses the campaign info file.
        /// </summary>
        /// <param name="id">The id of the campaign.</param>
        /// <param name="campaignFolderPath">The folder of the campaign.</param>
        /// <param name="campaignFile">The section file with the campaign configuration.</param>
        /// <param name="globalAircraftInfoFile">The global aircraft info file.</param>
        /// <param name="localAircraftInfoFile">If available the local aircraft info file, otherwise the global aircraft info file is used.</param>
        /// <param name="localAirGroupInfos">If available the local aigroup info file, otherwise the global aigroup info file is used.</param>
        public CampaignInfo(string id, string campaignFolderPath, ISectionFile campaignFile, ISectionFile globalAircraftInfoFile, ISectionFile localAircraftInfoFile = null, AirGroupInfos localAirGroupInfos = null)
        {
            _id = id;
            _globalAircraftInfoFile = globalAircraftInfoFile;
            _localAircraftInfoFile = localAircraftInfoFile;
            _localAirGroupInfos = localAirGroupInfos;

            if (campaignFile.exist(SectionMain, KeyName))
            {
                name = campaignFile.get(SectionMain, KeyName);
            }
            else
            {
                InvalidInifileFormatException(campaignFolderPath, SectionMain, KeyName);
            }

            if (campaignFile.exist(SectionMain, KeyInitialTemplate))
            {
                var initialTemplates = campaignFile.get(SectionMain, KeyInitialTemplate).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string initialTemplate in initialTemplates)
                {
                    InitialMissionTemplateFiles.Add(campaignFolderPath + initialTemplate.Trim());
                }
            }
            if (InitialMissionTemplateFiles.Count < 1)
            {
                InvalidInifileFormatException(campaignFolderPath, SectionMain, KeyInitialTemplate);
            }

            if (campaignFile.exist(SectionMain, KeyScriptFile))
            {
                _scriptFileName = campaignFile.get(SectionMain, KeyScriptFile);
            }
            else
            {
                InvalidInifileFormatException(campaignFolderPath, SectionMain, KeyScriptFile);
            }

            if (campaignFile.exist(SectionMain, KeyStartDate))
            {
                string startDateString = campaignFile.get(SectionMain, KeyStartDate);
                _startDate = DateTime.Parse(startDateString);
            }
            else
            {
                InvalidInifileFormatException(campaignFolderPath, SectionMain, KeyStartDate);
            }

            if (campaignFile.exist(SectionMain, KeyEndDate))
            {
                string endDateString = campaignFile.get(SectionMain, KeyEndDate);
                _endDate = DateTime.Parse(endDateString);
            }
            else
            {
                InvalidInifileFormatException(campaignFolderPath, SectionMain, KeyEndDate);
            }

            DynamicFrontMarker = campaignFile.get(SectionMain, KeyDynamicFrontMarker, false);

            ReadRandomUnitInfo(campaignFile);
        }

        #endregion

        private void ReadRandomUnitInfo(ISectionFile confFile)
        {
            string value;

            if (confFile.exist(Config.SectionAircraft, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionAircraft, Config.KeyRandomRed);
                AircraftRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionAircraft, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionAircraft, Config.KeyRandomBlue);
                AircraftRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionGroundVehicle, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionGroundVehicle, Config.KeyRandomRed);
                GroundVehicleRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundVehicleRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionGroundVehicle, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionGroundVehicle, Config.KeyRandomBlue);
                GroundVehicleRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundVehicleRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionGroundArmor, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionGroundArmor, Config.KeyRandomRed);
                GroundArmorRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundArmorRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionGroundArmor, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionGroundArmor, Config.KeyRandomBlue);
                GroundArmorRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundArmorRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionGroundShip, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionGroundShip, Config.KeyRandomRed);
                GroundShipRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundShipRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionGroundShip, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionGroundShip, Config.KeyRandomBlue);
                GroundShipRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundShipRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionGroundTrain, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionGroundTrain, Config.KeyRandomRed);
                GroundTrainRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundTrainRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionGroundTrain, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionGroundTrain, Config.KeyRandomBlue);
                GroundTrainRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundTrainRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryRadar, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryRadar, Config.KeyRandomRed);
                StationaryRadarRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryRadarRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryRadar, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryRadar, Config.KeyRandomBlue);
                StationaryRadarRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryRadarRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryAircraft, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryAircraft, Config.KeyRandomRed);
                StationaryAircraftRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAircraftRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryAircraft, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryAircraft, Config.KeyRandomBlue);
                StationaryAircraftRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAircraftRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryArtillery, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryArtillery, Config.KeyRandomRed);
                StationaryArtilleryRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryArtilleryRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryArtillery, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryArtillery, Config.KeyRandomBlue);
                StationaryArtilleryRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryArtilleryRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryFlak, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryFlak, Config.KeyRandomRed);
                StationaryFlakRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryFlakRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryFlak, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryFlak, Config.KeyRandomBlue);
                StationaryFlakRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryFlakRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryDepot, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryDepot, Config.KeyRandomRed);
                StationaryDepotRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryDepotRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryDepot, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryDepot, Config.KeyRandomBlue);
                StationaryDepotRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryDepotRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryShip, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryShip, Config.KeyRandomRed);
                StationaryShipRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryShipRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryShip, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryShip, Config.KeyRandomBlue);
                StationaryShipRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryShipRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryAmmo, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryAmmo, Config.KeyRandomRed);
                StationaryAmmoRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAmmoRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryAmmo, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryAmmo, Config.KeyRandomBlue);
                StationaryAmmoRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAmmoRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryWeapons, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryWeapons, Config.KeyRandomRed);
                StationaryWeaponsRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryWeaponsRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryWeapons, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryWeapons, Config.KeyRandomBlue);
                StationaryWeaponsRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryWeaponsRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryCar, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryCar, Config.KeyRandomRed);
                StationaryCarRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryCarRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryCar, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryCar, Config.KeyRandomBlue);
                StationaryCarRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryCarRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryConstCar, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryConstCar, Config.KeyRandomRed);
                StationaryConstCarRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryConstCarRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryConstCar, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryConstCar, Config.KeyRandomBlue);
                StationaryConstCarRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryConstCarRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryEnvironment, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryEnvironment, Config.KeyRandomBlue);
                StationaryEnvironmentRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryEnvironmentRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryEnvironment, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryEnvironment, Config.KeyRandomRed);
                StationaryEnvironmentRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryEnvironmentRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationarySearchlight, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationarySearchlight, Config.KeyRandomBlue);
                StationarySearchlightRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationarySearchlightRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationarySearchlight, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationarySearchlight, Config.KeyRandomRed);
                StationarySearchlightRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationarySearchlightRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryAeroanchored, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryAeroanchored, Config.KeyRandomBlue);
                StationaryAeroanchoredRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAeroanchoredRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryAeroanchored, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryAeroanchored, Config.KeyRandomRed);
                StationaryAeroanchoredRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAeroanchoredRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryAirfield, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryAirfield, Config.KeyRandomBlue);
                StationaryAirfieldRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAirfieldRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryAirfield, Config.KeyRandomRed))
            {
                value = confFile.get(Config.SectionStationaryAirfield, Config.KeyRandomRed);
                StationaryAirfieldRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAirfieldRandomBlue = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryUnknown, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryUnknown, Config.KeyRandomBlue);
                StationaryUnknownRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryUnknownRandomRed = new string[0];
            }

            if (confFile.exist(Config.SectionStationaryUnknown, Config.KeyRandomBlue))
            {
                value = confFile.get(Config.SectionStationaryUnknown, Config.KeyRandomBlue);
                StationaryUnknownRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryUnknownRandomBlue = new string[0];
            }
        }

        public static void InvalidInifileFormatException(string folder, string section, string key)
        {
            throw new FormatException(string.Format("Invalid Campaign File Format [Folder:{0}, Section:{1}, Key:{2}]", folder, section, key));
        }

        /// <summary>
        /// The textual representation of a CampaignInfo object.
        /// </summary>
        /// <returns>The name of the campaign.</returns>
        public override string ToString()
        {
            return _id;
        }

        public string ToSummaryString()
        {
            return string.Format(DateTimeFormatInfo.InvariantInfo, "Name: {0}\nStartDate: {1,-12:d}\n  EndDate: {2,-12:d}\n", Name, StartDate, EndDate);
        }

         /// <summary>
        /// Gets the aircraft info for the given aicraft name. 
        /// </summary>
        /// <param name="aircraft">The name of the aircraft.</param>
        /// <returns>If available it returns the definition of the local aircraft info file, otherwise the definiton of the global aircraft info is returned.</returns>
        public AircraftInfo GetAircraftInfo(string aircraft)
        {
            if (_localAircraftInfoFile != null && _localAircraftInfoFile.exist(SectionMain, aircraft))
            {
                return new AircraftInfo(_localAircraftInfoFile, aircraft);
            }
            else if (_globalAircraftInfoFile.exist(SectionMain, aircraft))
            {
                return new AircraftInfo(_globalAircraftInfoFile, aircraft);
            }
            else
            {
                throw new ArgumentException(string.Format("no aircraft[{0}] info in the file[{1}]", aircraft, "Aircraftinfo.ini"));
            }
        }

        public void Write(ISectionFile file, string separator = Config.CommaStr)
        {
            SilkySkyCloDFile.Write(file, SectionMain, KeyName, name, true);
            SilkySkyCloDFile.Write(file, SectionMain, KeyInitialTemplate, string.Join(separator, InitialMissionTemplateFiles), true);
            SilkySkyCloDFile.Write(file, SectionMain, KeyScriptFile, ScriptFileName, true);
            SilkySkyCloDFile.Write(file, SectionMain, KeyStartDate, StartDate.ToString(FormatDate, DateTimeFormatInfo.InvariantInfo), true);
            SilkySkyCloDFile.Write(file, SectionMain, KeyEndDate, EndDate.ToString(FormatDate, DateTimeFormatInfo.InvariantInfo), true);
            SilkySkyCloDFile.Write(file, SectionMain, KeyDynamicFrontMarker, DynamicFrontMarker ? "1": "0", true);
        }
    }
}