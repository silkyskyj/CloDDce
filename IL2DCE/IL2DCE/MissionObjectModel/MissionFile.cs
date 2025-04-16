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
using System.Linq;
using IL2DCE.Generator;
using maddox.game;
using maddox.GP;
using XLAND;

namespace IL2DCE.MissionObjectModel
{
    public class MissionFile
    {
        #region Definition

        public const string SectionParts = "PARTS";
        public const string SectionMain = "MAIN";
        public const string SectionGlobalWind = "GlobalWind";
        public const string SectionSplines = "splines";
        public const string SectionCustomChiefs = "CustomChiefs";
        public const string SectionStationary = "Stationary";
        public const string SectionBuildings = "Buildings";
        public const string SectionBuildingsLinks = "BuildingsLinks";
        public const string SectionFrontMarker = "FrontMarker";
        public const string SectionAirGroups = "AirGroups";
        public const string SectionChiefs = "Chiefs";
        public const string SectionChiefsRoad = "Chiefs_Road";
        public const string SectionRoad = "Road";
        public const string SectionTrigger = "Trigger";
        public const string SectionAction = "Action";
        public const string SectionAirdromes = "Airdromes";
        public const string SectionWay = "Way";
        public const string KeyMap = "MAP";
        public const string KeyBattleArea = "BattleArea";
        public const string KeyRunways = "Runways";
        public const string KeyWeatherIndex = "WeatherIndex";
        public const string KeyCloudsHeight = "CloudsHeight";
        public const string KeyTime = "TIME";
        public const string KeyPoints = "Points";
        public const string KeyPlayer = "player";
        public const string KeyVirtualAirGroupKey = "VirtualAirGroupKey";
        public const string KeyFlight = "Flight";
        public const string KeyClass = "Class";
        public const string KeyFormation = "Formation";
        public const string KeyCallSign = "CallSign";
        public const string KeyFuel = "Fuel";
        public const string KeyWeapons = "Weapons";
        public const string KeyDetonator = "Detonator";
        public const string KeySetOnPark = "SetOnPark";
        public const string KeySpawnFromScript = "SpawnFromScript";
        public const string KeySkill = "Skill";
        public const string KeyAging = "Aging";
        public const string KeySkin = "Skin";
        public const string KeyMarkingsOn = "MarkingsOn";
        public const string KeyBandColor = "BandColor";
        public const string KeyBriefing = "Briefing";
        public const string ValueTTime = "TTime";
        public const string ValueASpawnGroup = "ASpawnGroup";
        public const string KeyChief = "Chief";
        public const string KeyStatic = "Static";
        public const string KeyPartsCore = "core.100";
        public const string KeyPartsBob = "bob.100";
        public const string KeyPartsTobruk = "tobruk.100";

        public const float DefaultTime = 12.0f;
        public const int DefaulWeatherIndex = 0;
        public const int DefaulCloudsHeight = 1000;

        public static readonly char[] SplitChars = new char[] { ' ' };

        #endregion

        #region Property (& Variable)

        public IEnumerable<string> Parts
        {
            get;
            private set;
        }

        public bool IsTypeDLC
        {
            get
            {
                return DLCParts.Any();
            }
        }

        public IEnumerable<string> DLCParts
        {
            get
            {
                return Parts != null ? Parts.Except(new string[] { KeyPartsCore, KeyPartsBob }) :new string[0];
            }
        }

        public IEnumerable<string> DLC
        {
            get
            {
                return Parts != null ? Parts.Except(new string[] { KeyPartsCore, KeyPartsBob }).Select(x => 
                                                    { 
                                                        int i = x.IndexOf("."); 
                                                        return i != -1 ? x.Substring(0, i) : x;
                                                    }): 
                                       new string [0];
            }
        }

        public string Map
        {
            get;
            private set;
        }

        public wRECTF BattleArea
        {
            get;
            private set;
        }

        public int Sector
        {
            get;
            private set;
        }


        public float Time
        {
            get;
            private set;
        }

        public int WeatherIndex
        {
            get;
            private set;
        }

        public int CloudsHeight
        {
            get;
            private set;
        }

#if false
        public IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>> GlobalWind
        {
            get;
            private set;
        }
#endif

        public List<AirGroup> AirGroups
        {
            get;
            private set;
        }

        internal List<GroundGroup> GroundGroups
        {
            get;
            private set;
        }

        internal List<Stationary> Stationaries
        {
            get;
            private set;
        }

        public List<Groundway> Roads
        {
            get;
            private set;
        }

        public List<Groundway> Waterways
        {
            get;
            private set;
        }

        public List<Groundway> Railways
        {
            get;
            private set;
        }

        internal List<Building> Depots
        {
            get;
            private set;
        }

        internal List<Stationary> OtherStationaries
        {
            get;
            private set;
        }

        public List<Point3d> FrontMarkers
        {
            get;
            private set;
        }

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

#endregion

        private AirGroupInfos airGroupInfos;

        public MissionFile(IGamePlay game, IEnumerable<string> fileNames, AirGroupInfos airGroupInfos = null)
        {
            this.airGroupInfos = airGroupInfos;

            init();
            foreach (string fileName in fileNames)
            {
                load(game.gpLoadSectionFile(fileName));
            }
        }

        public MissionFile(ISectionFile file, AirGroupInfos airGroupInfos = null)
        {
            this.airGroupInfos = airGroupInfos;

            init();
            load(file);
        }

        private void init()
        {
            AirGroups = new List<AirGroup>();
            GroundGroups = new List<GroundGroup>();
            Stationaries = new List<Stationary>();

            Roads = new List<Groundway>();
            Waterways = new List<Groundway>();
            Railways = new List<Groundway>();
            Depots = new List<Building>();
            OtherStationaries = new List<Stationary>();

            FrontMarkers = new List<Point3d>();
        }

        private void load(ISectionFile file)
        {
            // Parts
            Parts = SilkySkyCloDFile.ReadSectionKeies(file, SectionParts);
            // Main
            Map = file.get(SectionMain, KeyMap, string.Empty);
            string value = file.get(SectionMain, KeyBattleArea, string.Empty);
            string[] values = value.Split(SplitChars);
            if (values.Length >= 5)
            {
                int x;
                int y;
                int w;
                int h;
                int sector;
                if (int.TryParse(values[0], NumberStyles.Integer, Config.NumberFormat, out x) &&
                    int.TryParse(values[1], NumberStyles.Integer, Config.NumberFormat, out y) &&
                    int.TryParse(values[2], NumberStyles.Integer, Config.NumberFormat, out w) &&
                    int.TryParse(values[3], NumberStyles.Integer, Config.NumberFormat, out h) &&
                    int.TryParse(values[4], NumberStyles.Integer, Config.NumberFormat, out sector))
                {
                    BattleArea = new wRECTF() { x1 = x, x2 = x + w - 1, y1 = y, y2 = y + h - 1, };
                    Sector = sector;
                }
            }
            Time = file.get(SectionMain, KeyTime, DefaultTime);
            WeatherIndex = file.get(SectionMain, KeyWeatherIndex, DefaulWeatherIndex);
            CloudsHeight = file.get(SectionMain, KeyCloudsHeight, DefaulCloudsHeight);

            string key;
            int lines;
            int i = 0;

#if false
            // GlobalWind
            List<KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>> globalWindList = new List<KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>>();
            string esction;
            while (file.exist(esction = string.Format(Config.NumberFormat, "{0}_{1}", MissionFile.SectionGlobalWind, i++)))
            {
                List<KeyValuePair<string, string>> keyValueList = new List<KeyValuePair<string, string>>();
                lines = file.lines(esction);
                for (i = 0; i < lines; i++)
                {
                    file.get(SectionStationary, i, out key, out value);
                    keyValueList.Add(new KeyValuePair<string, string>(key, value));
                }
                globalWindList.Add(new KeyValuePair<string, IEnumerable<KeyValuePair<string, string>>>(esction, keyValueList));
            }
            GlobalWind = globalWindList;
#endif

            // Stationary
            lines = file.lines(SectionStationary);
            for (i = 0; i < lines; i++)
            {
                file.get(SectionStationary, i, out key, out value);

                Stationary stationary = Stationary.Create(file, key);
                if (stationary != null)
                {
                    if (stationary.Army == (int)EArmy.Red || stationary.Army == (int)EArmy.Blue)
                    {
                        Stationaries.Add(stationary);
                    }
                    else
                    {   // Army = none
                        OtherStationaries.Add(stationary);
                        // Debug.WriteLine("Stationary Type={0}, ID={1}, Army={2}, Country={3}, Class={4}", stationary.Type, stationary.Id, stationary.Army, stationary.Country, stationary.Class);
                    }
                }
            }

            // Buildings
            lines = file.lines(SectionBuildings);
            for (i = 0; i < lines; i++)
            {
                file.get(SectionBuildings, i, out key, out value);
                Building building = Building.Create(key, value);
                if (building != null)
                {
                    Depots.Add(building);
                }
            }

            // FrontMaker
            lines = file.lines(SectionFrontMarker);
            for (i = 0; i < lines; i++)
            {
                file.get(SectionFrontMarker, i, out key, out value);

                string[] valueParts = value.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                if (valueParts.Length == 3)
                {
                    double x;
                    double y;
                    int army;
                    if (double.TryParse(valueParts[0], NumberStyles.Float, Config.NumberFormat, out x)
                        && double.TryParse(valueParts[1], NumberStyles.Float, Config.NumberFormat, out y)
                        && int.TryParse(valueParts[2], NumberStyles.Integer, Config.NumberFormat, out army))
                    {
                        if (!FrontMarkers.Any(a => a.x == x && a.y == y && a.z == army))
                        {
                            FrontMarkers.Add(new Point3d(x, y, army));
                        }
                    }
                }
            }

            // AirGroups
            lines = file.lines(SectionAirGroups);
            for (i = 0; i < lines; i++)
            {
                file.get(SectionAirGroups, i, out key, out value);

                AirGroup airGroup = new AirGroup(file, key);
                string airGoupKey = string.IsNullOrEmpty(airGroup.VirtualAirGroupKey) ? airGroup.AirGroupKey : airGroup.VirtualAirGroupKey;
                IEnumerable<AirGroupInfo> airGroupInfo = GetAirGroupInfo(airGoupKey, airGroup.Class, false);
                if (airGroupInfo.Any())
                {
                    AirGroupInfo airGroupInfoTarget = airGroupInfo.FirstOrDefault();
                    airGroup.SetAirGroupInfo(airGroupInfoTarget);
                    if (airGroup.ArmyIndex == (int)EArmy.Red || airGroup.ArmyIndex == (int)EArmy.Blue)
                    {
                        AirGroups.Add(airGroup);
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
                else
                {
                    Debug.WriteLine("no AirGroup info[{0}] and aircraft [{1}] in the file[{2}]", airGoupKey, airGroup.Class, "AirGroupInfo.ini");
                    Debug.Assert(false);
                }
            }

            // Chiefs
            lines = file.lines(SectionChiefs);
            for (i = 0; i < lines; i++)
            {
                file.get(SectionChiefs, i, out key, out value);

                GroundGroup groundGroup = GroundGroup.Create(file, key);
                if (groundGroup != null && (groundGroup.Army == (int)EArmy.Red || groundGroup.Army == (int)EArmy.Blue))
                {
                    GroundGroups.Add(groundGroup);
                }
                else
                {   // Army = none
                    Groundway road = Groundway.Create(file, key);
                    if (road != null)
                    {
                        if (value.StartsWith("Vehicle") || value.StartsWith("Armor"))
                        {
                            Roads.Add(road);
                        }
                        else if (value.StartsWith("Ship"))
                        {
                            Waterways.Add(road);
                        }
                        else if (value.StartsWith("Train"))
                        {
                            Railways.Add(road);
                        }
                    }
                }
            }

            // Trigger & Action  (Support: Time & Spawn only)
            IList<AirGroup> airGroups = AirGroups;
            lines = file.lines(SectionAction);
            for (i = 0; i < lines; i++)
            {
                file.get(SectionAction, i, out key, out value);
                if (file.exist(SectionTrigger, key) && !string.IsNullOrEmpty(value))
                {
                    string[] actions = value.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                    string trigger = file.get(SectionTrigger, key);
                    string[] triggers = trigger.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (actions.Length >= 3 && string.Compare(actions[0], ValueASpawnGroup, true) == 0 && string.Compare(actions[1], "1") == 0
                        && triggers.Length >= 2 && string.Compare(triggers[0], ValueTTime, true) == 0)
                    {
                        var airGroup = airGroups.Where(x => string.Compare(x.Id, actions[2], true) == 0).FirstOrDefault();
                        if (airGroup != null)
                        {
                            int time;
                            if (int.TryParse(triggers[1], out time) && time >= 0)
                            {
                                airGroup.SetSpawn(new Spawn((int)ESpawn.Default, false, false, new Spawn.SpawnLocation(), false, false, new Spawn.SpawnTime(true, time)));
                            }
                        }
                    }
                }
            }

            // Aircraft (Random)
            if (file.exist(Config.SectionAircraft, Config.KeyRandomRed))
            {
                value = file.get(Config.SectionAircraft, Config.KeyRandomRed);
                AircraftRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomRed = new string[0];
            }

            if (file.exist(Config.SectionAircraft, Config.KeyRandomBlue))
            {
                value = file.get(Config.SectionAircraft, Config.KeyRandomBlue);
                AircraftRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomBlue = new string[0];
            }
        }

        private IEnumerable<AirGroupInfo> GetAirGroupInfo(string airGroupKey, string aircraft, bool ignoreCase)
        {
            IEnumerable<AirGroupInfo> airGroupInfo;
            if (airGroupInfos != null && (airGroupInfo = airGroupInfos.GetAirGroupInfo(airGroupKey, aircraft, ignoreCase)).Any())
            {
                return airGroupInfo;
            }
            else if (AirGroupInfos.Default != null && (airGroupInfo = AirGroupInfos.Default.GetAirGroupInfo(airGroupKey, aircraft, ignoreCase)).Any())
            {
                return airGroupInfo;
            }

            return new AirGroupInfo [0];
        }
    }
}