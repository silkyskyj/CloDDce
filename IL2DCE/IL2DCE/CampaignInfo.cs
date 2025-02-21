// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2016 Stefan Rothdach
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
using maddox.game;
using maddox.game.world;

namespace IL2DCE
{
    public enum CampaignStatus
    {
        Empty,
        InProgress,
        DateEnd,
        Count,
    };

    /// <summary>
    /// The campaign info object holds the configuration of a campaign.
    /// </summary>
    public class CampaignInfo
    {
        /// <summary>
        /// Max Campaign Period
        /// </summary>
        public const int MaxCampaignPeriod = 730;

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
        /// The environment template file that contains the definition of scenery objects.
        /// </summary>
        public string EnvironmentTemplateFile
        {
            get
            {
                return _environmentTemplateFile;
            }
        }
        string _environmentTemplateFile;

        /// <summary>
        /// The list of static tempalte files that contain the definiton of the supply routes.
        /// </summary>
        public List<string> StaticTemplateFiles
        {
            get
            {
                return _staticTemplateFiles;
            }
        }
        private List<string> _staticTemplateFiles = new List<string>();

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

        public AirGroupInfos AirGroupInfos
        {
            get
            {
                return _localAirGroupInfos;
            }
        }
        private AirGroupInfos _localAirGroupInfos;

        private ISectionFile _globalAircraftInfoFile;
        private ISectionFile _localAircraftInfoFile;
 
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

            if (campaignFile.exist("Main", "name"))
            {
                name = campaignFile.get("Main", "name");
            }
            else
            {
                throw new FormatException("name");
            }

            if (campaignFile.exist("Main", "environmentTemplate"))
            {
                _environmentTemplateFile = campaignFolderPath + campaignFile.get("Main", "environmentTemplate").Trim();
            }
            else
            {
                throw new FormatException("environmentTemplate");
            }

            if (campaignFile.exist("Main", "staticTemplate"))
            {
                var staticTemplates = campaignFile.get("Main", "staticTemplate").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string staticTemplate in staticTemplates)
                {
                    StaticTemplateFiles.Add(campaignFolderPath + staticTemplate.Trim());
                }
            }
            if (StaticTemplateFiles.Count < 1)
            {
                throw new FormatException("staticTemplate");
            }

            if (campaignFile.exist("Main", "initialTemplate"))
            {
                var initialTemplates = campaignFile.get("Main", "initialTemplate").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string initialTemplate in initialTemplates)
                {
                    InitialMissionTemplateFiles.Add(campaignFolderPath + initialTemplate.Trim());
                }
            }
            if (InitialMissionTemplateFiles.Count < 1)
            {
                throw new FormatException("initialTemplate");
            }

            if (campaignFile.exist("Main", "scriptFile"))
            {
                _scriptFileName = campaignFile.get("Main", "scriptFile");
            }
            else
            {
                throw new FormatException("scriptFile");
            }

            if (campaignFile.exist("Main", "startDate"))
            {
                string startDateString = campaignFile.get("Main", "startDate");
                _startDate = DateTime.Parse(startDateString);
            }
            else
            {
                throw new FormatException("startDate");
            }

            if (campaignFile.exist("Main", "endDate"))
            {
                string endDateString = campaignFile.get("Main", "endDate");
                _endDate = DateTime.Parse(endDateString);
            }
            else
            {
                throw new FormatException("endDate");
            }
        }

        /// <summary>
        /// The textual representation of a CampaignInfo object.
        /// </summary>
        /// <returns>The name of the campaign.</returns>
        public override string ToString()
        {
            return Name;
        }

        public string ToSummaryString()
        {
            return string.Format("Campaign\n Name: {0}\n StartDate: {1}\n EndDate: {2}\n",
                                Name,
                                StartDate.ToString("d", DateTimeFormatInfo.InvariantInfo),
                                EndDate.ToString("d", DateTimeFormatInfo.InvariantInfo));
        }

         /// <summary>
        /// Gets the aircraft info for the given aicraft name. 
        /// </summary>
        /// <param name="aircraft">The name of the aircraft.</param>
        /// <returns>If available it returns the definition of the local aircraft info file, otherwise the definiton of the global aircraft info is returned.</returns>
        public AircraftInfo GetAircraftInfo(string aircraft)
        {
            if (_localAircraftInfoFile != null && _localAircraftInfoFile.exist("Main", aircraft))
            {
                return new AircraftInfo(_localAircraftInfoFile, aircraft);
            }
            else if (_globalAircraftInfoFile.exist("Main", aircraft))
            {
                return new AircraftInfo(_globalAircraftInfoFile, aircraft);
            }
            else
            {
                throw new ArgumentException(string.Format("no aircraft[{0}] info in the file[{1}]", aircraft, "Aircraftinfo.ini"));
            }
        }

        public AirGroupInfo GetAirGroupInfo(string airGroupKey)
        {
            AirGroupInfo airGroupInfo = null;
            if (_localAirGroupInfos != null && (airGroupInfo = _localAirGroupInfos.GetAirGroupInfo(airGroupKey)) != null)
            {
                return airGroupInfo;
            }
            else if ((airGroupInfo = AirGroupInfos.Default.GetAirGroupInfo(airGroupKey)) != null)
            {
                return airGroupInfo;
            }
            else
            {
                throw new ArgumentException(string.Format("no AirGroup[{0}] info in the file[{1}]", airGroupKey, "AirGroupinfo.ini"));
            }
        }
    }
}