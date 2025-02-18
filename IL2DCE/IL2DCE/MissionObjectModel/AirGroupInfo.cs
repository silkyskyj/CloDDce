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
using System.Diagnostics;
using System.Linq;
using maddox.game;

namespace IL2DCE
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
        #region Public properties

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

        #endregion
    }

    public class AirGroupInfos
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static AirGroupInfo[] CreateAirGroupInfo(ISectionFile file)
        {
            List<AirGroupInfo> infos = new List<AirGroupInfo>();

            string key;
            string value;
            int lines = file.lines(SectionMain);
            for (int i = 0; i < lines; i++)
            {
                file.get(SectionMain, i, out key, out value);
                if (!string.IsNullOrEmpty(key))
                {
                    string section = key;
                    string secAircrafts = string.Format("{0}.{1}", section, SectionAircrafts);
                    string secAirGroupKeys = string.Format("{0}.{1}", section, SectionAirGroupKeys);
                    if (file.exist(section) && file.exist(secAircrafts) && file.exist(secAirGroupKeys))
                    {
                        // Aircraft
                        List<string> aircrafts = new List<string>();
                        int lines2 = file.lines(secAircrafts);
                        for (int j = 0; j < lines2; j++)
                        {
                            file.get(secAircrafts, j, out key, out value);
                            aircrafts.Add(key);
                        }

                        // AirGroup
                        List<string> airGroupKeys = new List<string>();
                        lines2 = file.lines(secAirGroupKeys);
                        for (int j = 0; j < lines2; j++)
                        {
                            file.get(secAirGroupKeys, j, out key, out value);
                            airGroupKeys.Add(key);
                        }

                        infos.Add(new AirGroupInfo()
                        {
                            Aircrafts = aircrafts,
                            AirGroupKeys = airGroupKeys,
                            SquadronCount = ReadNumeric(file, section, KeySquadronCount),
                            FlightCount = ReadNumeric(file, section, KeyFlightCount),
                            FlightSize = ReadNumeric(file, section, KeyFlightSize),
                            ArmyIndex = ReadNumeric(file, section, KeyArmyIndex),
                            AirForceIndex = ReadNumeric(file, section, KeyAirForceIndex)
                        });
                    }
                    else
                    {
                        Debug.WriteLine("No AirGroupInfo[{0}, {1}, {2}]", section, secAircrafts, secAirGroupKeys);
                    }
                }
            }

            return infos.ToArray();
        }

        public static int ReadNumeric(ISectionFile file, string section, string key)
        {
            int num = file.get(section, key, -1);
            if (num == -1)
            {
                InvalidInifileFormat(FileInfo, section, key);
            }
            return num;
        }

        public static void InvalidInifileFormat(string file, string section, string key)
        {
            throw new FormatException(string.Format("Invalid Value [File:{0}, Section:{1}, Key:{2}]", file, section, key));
        }

        public AirGroupInfo[] GetAirGroupInfos(int armyIndex)
        {
            return AirGroupInfo.Where(x => x.ArmyIndex == armyIndex).ToArray();
        }

        public AirGroupInfo GetAirGroupInfo(int armyIndex, string airGroupKey)
        {
            return AirGroupInfo.Where(x => x.ArmyIndex == armyIndex && x.AirGroupKeys.Contains(airGroupKey)).FirstOrDefault();
        }

        public AirGroupInfo GetAirGroupInfo(string airGroupKey)
        {
            return AirGroupInfo.Where(x => x.AirGroupKeys.Contains(airGroupKey)).FirstOrDefault();
        }

        public int GetArmyIndex(string airGroupKey)
        {
            AirGroupInfo airGroupInfo = GetAirGroupInfo(airGroupKey);
            return airGroupInfo != null ? airGroupInfo.ArmyIndex: 0;
        }
    }
}