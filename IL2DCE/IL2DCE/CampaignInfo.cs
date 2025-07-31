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
        ProgressEnd,
        Count,
    };

    public enum ECampaignMode
    {
        [Description("Default")]
        Default,

        [Description("Progress")]
        Progress,

        [Description("Random")]
        Random,

        [Description("Progress → Random")]
        Progress2Random,

        [Description("Progress → Repeat")]
        Progress2Repeat,

        Count,
    };

    /// <summary>
    /// The campaign info object holds the configuration of a campaign.
    /// </summary>
    [Serializable]
    public class CampaignInfo
    {
        #region Definition

        public const string SectionMain = "Main";
        public const string SectionMapPeriod = "Map.Period";
        public const string KeyName = "name";
        public const string KeyCampaignMode = "CampaignMode";
        public const string KeyInitialTemplate = "initialTemplate";
        public const string KeyScriptFile = "scriptFile";
        public const string KeyStartDate = "startDate";
        public const string KeyEndDate = "endDate";
        public const string KeyDynamicFrontMarker = "DynamicFrontMarker";
        public const string FormatDate = "yyyy-MM-dd";

        public static readonly DateTime DefaultStartDate = new DateTime(1940, 07, 10);
        public static readonly DateTime DefaultEndDate = new DateTime(1940, 08, 11);

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
                return id;
            }
        }
        string id;

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

        public ECampaignMode CampaignMode
        {
            get;
            private set;
        }

        public string InitialMissionTemplateFile
        {
            get
            {
                return initialMissionTemplateFiles.FirstOrDefault();
            }
        }

        public int InitialMissionTemplateFileCount
        {
            get
            {
                return initialMissionTemplateFiles.Count;
            }
        }

        /// <summary>
        /// The name of the script file that will be used in the generated missions.
        /// </summary>
        public string ScriptFileName
        {
            get
            {
                return scriptFileName;
            }
        }
        private string scriptFileName;

        /// <summary>
        /// The start date of the campaign.
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                return startDate;
            }
            set
            {
                startDate = value;
            }
        }
        private DateTime startDate;

        /// <summary>
        /// The end date of the campaign.
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                return endDate;
            }
            set
            {
                endDate = value;
            }
        }
        private DateTime endDate;

        public bool DynamicFrontMarker
        {
            get;
            private set;
        }

        public Dictionary<int, string> MapPeriods
        {
            get;
            private set;
        }

        public AirGroupInfos AirGroupInfos
        {
            get
            {
                return localAirGroupInfos;
            }
        }
        private AirGroupInfos localAirGroupInfos;

        public RandomUnitSet RandomUnits
        {
            get;
            private set;
        }

        public object Data
        {
            get;
            set;
        }

        /// <summary>
        /// The list of initial mission template files that contain the starting location of air and ground groups.
        /// </summary>
        private List<string> initialMissionTemplateFiles = new List<string>();

        private ISectionFile globalAircraftInfoFile;
        private ISectionFile localAircraftInfoFile;

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
        /// <param name="mapPeriods"></param>
        /// <param name="dynamicFrontMarker"></param>
        public CampaignInfo(string name, ECampaignMode campaignMode, IEnumerable<string> initialMissionTemplateFiles, string scriptFileName, DateTime? startDate = null, DateTime? endDate = null, IEnumerable<KeyValuePair<int, string>> mapPeriods = null, bool dynamicFrontMarker = false)
        {
            this.name = name;
            id = name;
            CampaignMode = campaignMode;
            this.initialMissionTemplateFiles = initialMissionTemplateFiles.ToList();
            this.scriptFileName = scriptFileName;
            this.startDate = startDate != null && startDate.HasValue ? startDate.Value : DefaultStartDate;
            this.endDate = endDate != null && endDate.HasValue ? endDate.Value : DefaultEndDate;
            MapPeriods = new Dictionary<int, string>();
            if (mapPeriods != null)
            {
                foreach (var item in mapPeriods)
                {
                    MapPeriods.Add(item.Key, item.Value);
                }
            }
            DynamicFrontMarker = dynamicFrontMarker;
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
            this.id = id;
            this.globalAircraftInfoFile = globalAircraftInfoFile;
            this.localAircraftInfoFile = localAircraftInfoFile;
            this.localAirGroupInfos = localAirGroupInfos;

            if (campaignFile.exist(SectionMain, KeyName))
            {
                name = campaignFile.get(SectionMain, KeyName);
            }
            else
            {
                InvalidInifileFormatException(campaignFolderPath, SectionMain, KeyName);
            }

            int val = campaignFile.get(SectionMain, KeyCampaignMode, 0);
            CampaignMode = (Enum.IsDefined(typeof(ECampaignMode), val)) ? (ECampaignMode)val : ECampaignMode.Default;

            if (campaignFile.exist(SectionMain, KeyInitialTemplate))
            {
                var initialTemplates = campaignFile.get(SectionMain, KeyInitialTemplate).Split(Config.SplitComma, StringSplitOptions.RemoveEmptyEntries);

                foreach (string initialTemplate in initialTemplates)
                {
                    initialMissionTemplateFiles.Add(string.Format("{0}{1}", campaignFolderPath, initialTemplate.Trim()));
                }
            }
            if (initialMissionTemplateFiles.Count < 1)
            {
                InvalidInifileFormatException(campaignFolderPath, SectionMain, KeyInitialTemplate);
            }

            if (campaignFile.exist(SectionMain, KeyScriptFile))
            {
                scriptFileName = campaignFile.get(SectionMain, KeyScriptFile);
            }
            else
            {
                InvalidInifileFormatException(campaignFolderPath, SectionMain, KeyScriptFile);
            }

            if (campaignFile.exist(SectionMain, KeyStartDate))
            {
                string startDateString = campaignFile.get(SectionMain, KeyStartDate);
                startDate = DateTime.Parse(startDateString);
            }
            else
            {
                InvalidInifileFormatException(campaignFolderPath, SectionMain, KeyStartDate);
            }

            if (campaignFile.exist(SectionMain, KeyEndDate))
            {
                string endDateString = campaignFile.get(SectionMain, KeyEndDate);
                endDate = DateTime.Parse(endDateString);
            }
            else
            {
                InvalidInifileFormatException(campaignFolderPath, SectionMain, KeyEndDate);
            }

            DynamicFrontMarker = campaignFile.get(SectionMain, KeyDynamicFrontMarker, false);

            IEnumerable<string> mapLists = Map.DefaultList();
            MapPeriods = new Dictionary<int, string>();
            string key;
            string value;
            int lines = campaignFile.lines(SectionMapPeriod);
            for (int i = 0; i < lines; i++)
            {
                campaignFile.get(SectionMapPeriod, i, out key, out value);
                int n;
                if (int.TryParse(key, out n) && !string.IsNullOrWhiteSpace(value))
                {
                    value = value.Trim();
                    if (mapLists.Contains(value))
                    {
                        n = Math.Max(0, n);
                        if (MapPeriods.ContainsKey(n))
                        {
                            MapPeriods[n] = value;
                        }
                        else
                        {
                            MapPeriods.Add(n, value);
                        }
                    }
                }
            }

            RandomUnits = new RandomUnitSet();
            RandomUnits.Read(campaignFile);
        }

        #endregion

        #region ProgressMap

        public string ProgressMapName(DateTime dt)
        {
            // 1940/08/15 (03, 15, 25) -> 1940/08/03, 1940/08/15, 1940/08/25 -> 1940/08/15 
            // 1940/08/15 (03, 25) -> 1940/08/03, 1940/08/25 -> 1940/08/03
            // 1940/08/15 (20, 25) -> 1940/08/20, 1940/08/25 -> (null) xx-> 1940/08/25
            // 1940/08/15 (20, 0825) -> 1940/08/20, 1940/08/25 -> (null) xx-> 1939/08/25
            if (MapPeriods != null && MapPeriods.Any())
            {
                var mapLists = MapPeriods.Select(x => new KeyValuePair<DateTime, string>(Parse(dt, x.Key), x.Value)).OrderByDescending(x => x.Key);
                foreach (var item in mapLists)
                {
                    if (item.Key <= dt)
                    {
                        return item.Value;
                    }
                }
            }
            return string.Empty;
        }

        private DateTime Parse(DateTime dtBase, int dateValue)
        {
            // 1940/08/15, 03 -> 1940/08/03
            // 1940/08/15, 15 -> 1940/08/15
            // 1940/08/15, 25 -> 1940/07/25
            // 1940/08/15, 0525 -> 1940/05/25
            // 1940/08/15, 0825 -> 1939/08/25
            // 1940/08/15, 1210 -> 1939/12/10
            // 1941/01/01, 1210

            int year;
            int month;
            int day;

            if (dateValue > 9999)
            {
                year = Math.Max(Math.Min(dateValue / 10000, DateTime.MaxValue.Year), DateTime.MinValue.Year);
            }
            else
            {
                year = dtBase.Year;
            }

            if (dateValue > 99)
            {
                month = Math.Max(Math.Min((dateValue % 10000) / 100, 12), 1);
            }
            else
            {
                month = dtBase.Month;
            }

            if (dateValue > 0)
            {
                day = Math.Max(Math.Min(dateValue % 100, DateTime.DaysInMonth(year, month)), 1);
            }
            else
            {
                day = dtBase.Day;
            }

            DateTime dt = new DateTime(year, month, day);
            if (dt > dtBase && dt > DateTime.MinValue)
            {
                if (dateValue <= 0)
                {
                    dt = dt.AddDays(-1);
                }
                else if (dateValue <= 99)
                {
                    dt = dt.AddMonths(-1);
                }
                else if (dateValue <= 9999)
                {
                    dt = dt.AddYears(-1);
                }
            }
            return dt;
        }

        #endregion

        #region MissionTemplateFile

        public string MissionTemplateFile(ECampaignMode mode, int index, IRandom random)
        {
            if (mode == ECampaignMode.Progress)
            {
                return MissionTemplateFile(index);
            }
            else if (mode == ECampaignMode.Random)
            {
                return GetRandomMissionTemplateFile(random);
            }
            else if (mode == ECampaignMode.Progress2Random)
            {
                string result = MissionTemplateFile(index);
                if (string.IsNullOrEmpty(result))
                {
                    result = GetRandomMissionTemplateFile(random);
                }
                return result;
            }
            else if (mode == ECampaignMode.Progress2Repeat)
            {
                return MissionTemplateFile(index % initialMissionTemplateFiles.Count);
            }
            return initialMissionTemplateFiles.FirstOrDefault();
        }

        private string MissionTemplateFile(int index)
        {
            if (index >= 0 && index < initialMissionTemplateFiles.Count)
            {
                return initialMissionTemplateFiles[index];
            }
            return string.Empty;
        }

        private string GetRandomMissionTemplateFile(IRandom random)
        {
            if (random == null)
            {
                random = Random.Default;
            }
            return initialMissionTemplateFiles[random.Next(initialMissionTemplateFiles.Count)];
        }

        #endregion

        /// <summary>
        /// The textual representation of a CampaignInfo object.
        /// </summary>
        /// <returns>The name of the campaign.</returns>
        public override string ToString()
        {
            return id;
        }

        public string ToSummaryString()
        {
            return string.Format(DateTimeFormatInfo.InvariantInfo, "Name: {0}\nNums: {1:d} mission{2}",
                Name, initialMissionTemplateFiles.Count, initialMissionTemplateFiles.Count > 1 ? "s" : string.Empty);
        }

        public void Write(ISectionFile file, string separator = Config.CommaStr)
        {
            SilkySkyCloDFile.Write(file, SectionMain, KeyName, name, true);
            SilkySkyCloDFile.Write(file, SectionMain, KeyCampaignMode, ((int)CampaignMode).ToString(Config.NumberFormat), true);
            SilkySkyCloDFile.Write(file, SectionMain, KeyInitialTemplate, string.Join(Config.CommaStr, initialMissionTemplateFiles), true);
            SilkySkyCloDFile.Write(file, SectionMain, KeyScriptFile, ScriptFileName, true);
            SilkySkyCloDFile.Write(file, SectionMain, KeyStartDate, StartDate.ToString(FormatDate, DateTimeFormatInfo.InvariantInfo), true);
            SilkySkyCloDFile.Write(file, SectionMain, KeyEndDate, EndDate.ToString(FormatDate, DateTimeFormatInfo.InvariantInfo), true);
            SilkySkyCloDFile.Write(file, SectionMain, KeyDynamicFrontMarker, DynamicFrontMarker ? "1" : "0", true);
            foreach (var item in MapPeriods)
            {
                string format = string.Format("D{0}", item.Key > 10000 ? 6 : item.Key > 100 ? 4 : 2);
                SilkySkyCloDFile.Write(file, SectionMapPeriod, (item.Key % 1000000).ToString(format, Config.NumberFormat), item.Value, true);
            }
        }

        private void InvalidInifileFormatException(string folder, string section, string key)
        {
            throw new FormatException(string.Format("Invalid Campaign File Format [Folder:{0}, Section:{1}, Key:{2}]", folder, section, key));
        }

        /// <summary>
        /// Gets the aircraft info for the given aicraft name. 
        /// </summary>
        /// <param name="aircraft">The name of the aircraft.</param>
        /// <returns>If available it returns the definition of the local aircraft info file, otherwise the definiton of the global aircraft info is returned.</returns>
        public AircraftInfo GetAircraftInfo(string aircraft)
        {
            if (localAircraftInfoFile != null && localAircraftInfoFile.exist(SectionMain, aircraft))
            {
                return new AircraftInfo(localAircraftInfoFile, aircraft);
            }
            else if (globalAircraftInfoFile.exist(SectionMain, aircraft))
            {
                return new AircraftInfo(globalAircraftInfoFile, aircraft);
            }
            else
            {
                throw new ArgumentException(string.Format("no aircraft[{0}] info in the file[{1}]", aircraft, "Aircraftinfo.ini"));
            }
        }
    }
}