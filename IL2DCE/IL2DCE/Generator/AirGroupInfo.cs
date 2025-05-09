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

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using IL2DCE.MissionObjectModel;
using maddox.game;

namespace IL2DCE.Generator
{
#if false
    interface IAirGroupInfo
    {
        #region Public properties

        List<string> Aircrafts
        {
            get;
        }

        List<string> AirGroupKeys
        {
            get;
        }

        int SquadronCount
        {
            get;
        }

        int FlightCount
        {
            get;
        }

        int FlightSize
        {
            get;
        }

        int AircraftMaxCount
        {
            get;
        }

        int ArmyIndex
        {
            get;
        }

        int AirForceIndex
        {
            get;
        }

        #endregion
    }
#endif

    public class AirGroupInfo/* : IAirGroupInfo*/
    {
        public const string FileInfo = "AirGroupinfo";

        public const string SectionMain = "Main";
        public const string SectionAircrafts = "Aircrafts";
        public const string SectionAirGroupKeys = "AirGroupKeys";

        public const string KeySquadronCount = "SquadronCount";
        public const string KeyFlightCount = "FlightCount";
        public const string KeyFlightSize = "FlightSize";
        public const string KeyArmyIndex = "ArmyIndex";
        public const string KeyAirForceIndex = "AirForceIndex";
        public const string KeyFormationsType = "FormationsType";

        #region Public properties

        public string Name
        {
            get;
            set;
        }

        public List<string> Aircrafts
        {
            get;
            set;
        }

        public List<string> AirGroupKeys
        {
            get;
            set;
        }

        public Dictionary<string, string> SquadronInfo
        {
            get;
            set;
        }

        public int SquadronCount
        {
            get;
            set;
        }

        public int FlightCount
        {
            get;
            set;
        }

        public int FlightSize
        {
            get;
            set;
        }

        public int AircraftMaxCount
        {
            get
            {
                return FlightCount * FlightSize;
            }
        }

        public int ArmyIndex
        {
            get;
            set;
        }

        public int AirForceIndex
        {
            get;
            set;
        }

        public int FormationsType
        {
            get;
            set;
        }

        #endregion

        public bool Read(ISectionFile file)
        {
            return true;
        }

        public void Write(ISectionFile file, string airGroupKey = null, string aircraftClass = null)
        {
            SilkySkyCloDFile.Write(file, SectionMain, Name, string.Empty, true);
            SilkySkyCloDFile.Write(file, Name, KeySquadronCount, SquadronCount.ToString(Config.NumberFormat), false);
            SilkySkyCloDFile.Write(file, Name, KeyFlightCount, FlightCount.ToString(Config.NumberFormat), false);
            SilkySkyCloDFile.Write(file, Name, KeyFlightSize, FlightSize.ToString(Config.NumberFormat), false);
            SilkySkyCloDFile.Write(file, Name, KeyArmyIndex, ArmyIndex.ToString(Config.NumberFormat), false);
            SilkySkyCloDFile.Write(file, Name, KeyAirForceIndex, AirForceIndex.ToString(Config.NumberFormat), false);
            SilkySkyCloDFile.Write(file, Name, KeyFormationsType, FormationsType.ToString(Config.NumberFormat), false);
            if (string.IsNullOrEmpty(aircraftClass))
            {
                Aircrafts.ForEach(x => SilkySkyCloDFile.Write(file, string.Format("{0}.{1}", Name, SectionAircrafts), x, string.Empty, false)); // All
            }
            else
            {
                foreach (var item in Aircrafts.Where(x => string.Compare(x, aircraftClass, true) == 0))
                {
                    SilkySkyCloDFile.Write(file, string.Format("{0}.{1}", Name, SectionAircrafts), item, string.Empty, false);
                }
            }
            if (string.IsNullOrEmpty(airGroupKey))
            {
                AirGroupKeys.ForEach(x => SilkySkyCloDFile.Write(file, string.Format("{0}.{1}", Name, SectionAirGroupKeys), x, string.Empty, false)); // All
            }
            else
            {
                foreach (var item in AirGroupKeys.Where(x => string.Compare(x, airGroupKey, true) == 0))
                {
                    SilkySkyCloDFile.Write(file, string.Format("{0}.{1}", Name, SectionAirGroupKeys), item, string.Empty, false);
                }
            }
        }

        public static AirGroupInfo Create(ISectionFile file, string section, string secAircrafts, string secAirGroupKeys)
        {
            if (file.exist(section) && file.exist(secAircrafts) && file.exist(secAirGroupKeys))
            {
                string key;
                string value;

                // Aircraft
                List<string> aircrafts = new List<string>();
                int lines = file.lines(secAircrafts);
                for (int j = 0; j < lines; j++)
                {
                    file.get(secAircrafts, j, out key, out value);
                    aircrafts.Add(key);
                }

                // AirGroup
                List<string> airGroupKeys = new List<string>();
                Dictionary<string, string> squadronInfo = new Dictionary<string, string>();
                lines = file.lines(secAirGroupKeys);
                for (int j = 0; j < lines; j++)
                {
                    file.get(secAirGroupKeys, j, out key, out value);
                    airGroupKeys.Add(key);
                    if (!string.IsNullOrEmpty(value))
                    {
                        squadronInfo.Add(key, AirGroup.CreateSquadronName(value));      // VirtualAirGroupKey AirGroupID
                    }
                }

                return new AirGroupInfo()
                {
                    Name = section,
                    Aircrafts = aircrafts,
                    AirGroupKeys = airGroupKeys,
                    SquadronInfo = squadronInfo,
                    SquadronCount = SilkySkyCloDFile.ReadNumeric(file, section, KeySquadronCount, FileInfo),
                    FlightCount = SilkySkyCloDFile.ReadNumeric(file, section, KeyFlightCount, FileInfo),
                    FlightSize = SilkySkyCloDFile.ReadNumeric(file, section, KeyFlightSize, FileInfo),
                    ArmyIndex = SilkySkyCloDFile.ReadNumeric(file, section, KeyArmyIndex, FileInfo),
                    AirForceIndex = SilkySkyCloDFile.ReadNumeric(file, section, KeyAirForceIndex, FileInfo),
                    FormationsType = file.get(section, KeyFormationsType, (int)EFormationsType.Unknown)
                };
            }

            return null;
        }
    }

    public class AirGroupInfos
    {
        public static AirGroupInfos Default;

        public AirGroupInfo[] AirGroupInfo
        {
            get;
            protected set;
        }

        public static AirGroupInfos Create(ISectionFile file)
        {
            return new AirGroupInfos() { AirGroupInfo = CreateAirGroupInfo(file) };
        }

        public static AirGroupInfo[] CreateAirGroupInfo(ISectionFile file)
        {
            List<AirGroupInfo> infos = new List<AirGroupInfo>();

            string key;
            string value;
            int lines = file.lines(IL2DCE.Generator.AirGroupInfo.SectionMain);
            // Debug.WriteLine("{0}, Count={1}", SectionMain, lines);
            for (int i = 0; i < lines; i++)
            {
                file.get(IL2DCE.Generator.AirGroupInfo.SectionMain, i, out key, out value);
                if (!string.IsNullOrEmpty(key))
                {
                    string secAircrafts = string.Format("{0}.{1}", key, IL2DCE.Generator.AirGroupInfo.SectionAircrafts);
                    string secAirGroupKeys = string.Format("{0}.{1}", key, IL2DCE.Generator.AirGroupInfo.SectionAirGroupKeys);
                    AirGroupInfo airGroupInfo = IL2DCE.Generator.AirGroupInfo.Create(file, key, secAircrafts, secAirGroupKeys);
                    if (airGroupInfo != null)
                    {
                        infos.Add(airGroupInfo);
                    }
                    else
                    {
                        Debug.WriteLine("No AirGroupInfo[{0}, {1}, {2}]", key, secAircrafts, secAirGroupKeys);
                    }
                }
            }

            return infos.ToArray();
        }

        //public AirGroupInfo[] GetAirGroupInfos(int armyIndex)
        //{
        //    return AirGroupInfo.Where(x => x.ArmyIndex == armyIndex).ToArray();
        //}

        //public AirGroupInfo GetAirGroupInfo(int armyIndex, string airGroupKey)
        //{
        //    return AirGroupInfo.Where(x => x.ArmyIndex == armyIndex && x.AirGroupKeys.Contains(airGroupKey)).FirstOrDefault();
        //}

        public IEnumerable<AirGroupInfo> GetAirGroupInfoGroupKey(string airGroupKey, bool ignoreCase = false)
        {
            return ignoreCase ? AirGroupInfo.Where(x => x.AirGroupKeys.Any(y => string.Compare(y, airGroupKey, ignoreCase, CultureInfo.InvariantCulture) == 0)) :
                                AirGroupInfo.Where(x => x.AirGroupKeys.Contains(airGroupKey));
        }

        public IEnumerable<AirGroupInfo> GetAirGroupInfoGroupKeyEx(string airGroupKey, bool ignoreCase = false)
        {
            return ignoreCase ? AirGroupInfo.Where(x => x.AirGroupKeys.Any(y => string.Compare(y, airGroupKey, ignoreCase, CultureInfo.InvariantCulture) == 0) || x.SquadronInfo.Values.Any(y => string.Compare(y, airGroupKey, ignoreCase, CultureInfo.InvariantCulture) == 0)) :
                                AirGroupInfo.Where(x => x.AirGroupKeys.Contains(airGroupKey) || x.SquadronInfo.ContainsValue(airGroupKey));
        }

        public IEnumerable<AirGroupInfo> GetAirGroupInfoSquadron(string squadron, bool ignoreCase = false)
        {
            return ignoreCase ? AirGroupInfo.Where(x => x.SquadronInfo.Values.Any(y => string.Compare(y, squadron, ignoreCase, CultureInfo.InvariantCulture) == 0)) :
                                AirGroupInfo.Where(x => x.SquadronInfo.ContainsValue(squadron));
        }

        public IEnumerable<AirGroupInfo> GetAirGroupInfoAircraft(string aircraft, bool ignoreCase = false)
        {
            return ignoreCase ? AirGroupInfo.Where(x => x.Aircrafts.Any(y => string.Compare(y, aircraft, ignoreCase, CultureInfo.InvariantCulture) == 0)) :
                                AirGroupInfo.Where(x => x.Aircrafts.Contains(aircraft));
        }

        public IEnumerable<AirGroupInfo> GetAirGroupInfo(string squadron, string aircraft, bool ignoreCase = false, bool addAircraftKey = true, bool addGroupKey = true)
        {
            var airGroups = GetAirGroupInfoSquadron(squadron, ignoreCase);
            if (airGroups.Any())
            {
                airGroups = AddAircraft(airGroups, aircraft, ignoreCase, addAircraftKey);
            }
            else
            {
                string airGroupKey = AirGroup.CreateAirGroupKey(squadron);
                airGroups = GetAirGroupInfoGroupKey(airGroupKey, ignoreCase);
                if (airGroups.Any())
                {
                    airGroups = AddAircraft(airGroups, aircraft, ignoreCase, addAircraftKey);
                }
                else
                {
                    airGroups = GetAirGroupInfoAircraft(aircraft, ignoreCase);
                    if (addGroupKey)
                    {
                        foreach (var item in airGroups)
                        {
                            item.AirGroupKeys.Add(airGroupKey);
                        }

                    }
                }
            }
            return airGroups;
        }

        private IEnumerable<AirGroupInfo> AddAircraft(IEnumerable<AirGroupInfo> airGroups, string aircraft, bool ignoreCase = false, bool addAircraftKey = true)
        {
            var result = airGroups.Where(x => x.Aircrafts.Any(y => string.Compare(y, aircraft, ignoreCase, CultureInfo.InvariantCulture) == 0));
            if (result.Any())
            {
                airGroups = result;
            }
            else
            {
                if (addAircraftKey)
                {
                    foreach (var item in airGroups)
                    {
                        item.Aircrafts.Add(aircraft);
                    }
                }
            }
            return airGroups;
        }
    }
}