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
using IL2DCE.Generator;
using maddox.game;

namespace IL2DCE.MissionObjectModel
{
    public enum ECountry
    {
        nn,
        gb,
        de,
    }

    public class MissionFile
    {

        public IList<Waterway> Roads
        {
            get
            {
                return _roads;
            }
        }
        private List<Waterway> _roads = new List<Waterway>();

        public IList<Waterway> Waterways
        {
            get
            {
                return _waterways;
            }
        }
        private List<Waterway> _waterways = new List<Waterway>();

        public IList<Waterway> Railways
        {
            get
            {
                return _railways;
            }
        }
        private List<Waterway> _railways = new List<Waterway>();

        public IList<Building> Depots
        {
            get
            {
                return _depots;
            }
        }
        private List<Building> _depots = new List<Building>();

        public IList<Stationary> Radar
        {
            get
            {
                return _radars;
            }
        }
        private List<Stationary> _radars = new List<Stationary>();

        public IList<Stationary> Aircraft
        {
            get
            {
                return _aircrafts;
            }
        }
        private List<Stationary> _aircrafts = new List<Stationary>();

        public IList<Stationary> Artilleries
        {
            get
            {
                return _artilleries;
            }
        }
        private List<Stationary> _artilleries = new List<Stationary>();

        //public IList<Point3d> RedFrontMarkers
        //{
        //    get
        //    {
        //        return _redFrontMarkers;
        //    }
        //}

        //public IList<Point3d> BlueFrontMarkers
        //{
        //    get
        //    {
        //        return _blueFrontMarkers;
        //    }
        //}

        public IList<AirGroup> AirGroups
        {
            get
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                airGroups.AddRange(_redAirGroups);
                airGroups.AddRange(_blueAirGroups);
                return airGroups;
            }
        }

        public IList<AirGroup> RedAirGroups
        {
            get
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                airGroups.AddRange(_redAirGroups);
                return airGroups;
            }
        }
        private List<AirGroup> _redAirGroups = new List<AirGroup>();

        public IList<AirGroup> BlueAirGroups
        {
            get
            {
                List<AirGroup> airGroups = new List<AirGroup>();
                airGroups.AddRange(_blueAirGroups);
                return airGroups;
            }
        }
        private List<AirGroup> _blueAirGroups = new List<AirGroup>();

        public IList<GroundGroup> GroundGroups
        {
            get
            {
                List<GroundGroup> groundGroups = new List<GroundGroup>();
                groundGroups.AddRange(_redGroundGroups);
                groundGroups.AddRange(_blueGroundGroups);
                return groundGroups;
            }
        }

        public IList<GroundGroup> RedGroundGroups
        {
            get
            {
                List<GroundGroup> groundGroups = new List<GroundGroup>();
                groundGroups.AddRange(_redGroundGroups);
                return groundGroups;
            }
        }
        private List<GroundGroup> _redGroundGroups = new List<GroundGroup>();

        public IList<GroundGroup> BlueGroundGroups
        {
            get
            {
                List<GroundGroup> groundGroups = new List<GroundGroup>();
                groundGroups.AddRange(_blueGroundGroups);
                return groundGroups;
            }
        }
        private List<GroundGroup> _blueGroundGroups = new List<GroundGroup>();

        public IList<Stationary> Stationaries
        {
            get
            {
                List<Stationary> stationaries = new List<Stationary>();
                stationaries.AddRange(_redStationaries);
                stationaries.AddRange(_blueStationaries);
                return stationaries;
            }
        }
        private List<Stationary> _redStationaries = new List<Stationary>();
        private List<Stationary> _blueStationaries = new List<Stationary>();

        //private List<Point3d> _redFrontMarkers = new List<Point3d>();
        //private List<Point3d> _blueFrontMarkers = new List<Point3d>();
        //private List<Point3d> _neutralFrontMarkers = new List<Point3d>();

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
            _roads.Clear();
            _waterways.Clear();
            _railways.Clear();
            _depots.Clear();
            _aircrafts.Clear();
            _artilleries.Clear();

            //_redFrontMarkers.Clear();
            //_blueFrontMarkers.Clear();
            //_neutralFrontMarkers.Clear();

            _redAirGroups.Clear();
            _blueAirGroups.Clear();
            _redGroundGroups.Clear();
            _blueGroundGroups.Clear();

            _redStationaries.Clear();
            _blueStationaries.Clear();
        }

        private void load(ISectionFile file)
        {
            for (int i = 0; i < file.lines("Stationary"); i++)
            {
                string key;
                string value;
                file.get("Stationary", i, out key, out value);

                Stationary stationary = new Stationary(file, key);

                if (stationary.Army == 1)
                {
                    _redStationaries.Add(stationary);
                }
                else if (stationary.Army == 2)
                {
                    _blueStationaries.Add(stationary);
                }
                else
                {
                    if (stationary.Type == EStationaryType.Radar)
                    {
                        _radars.Add(stationary);
                    }
                    else if (stationary.Type == EStationaryType.Artillery)
                    {
                        _artilleries.Add(stationary);
                    }
                    else if (stationary.Type == EStationaryType.Aircraft)
                    {
                        _aircrafts.Add(stationary);
                    }
                }
            }

            for (int i = 0; i < file.lines("Buildings"); i++)
            {
                string key;
                string value;
                file.get("Buildings", i, out key, out value);


                string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (valueParts.Length > 4)
                {
                    // Depots
                    if (valueParts[0] == "buildings.House$Oil_Bunker-Small" || valueParts[0] == "buildings.House$Oil_Bunker-Middle" || valueParts[0] == "buildings.House$Oil_Bunker-Big")
                    {
                        Building building = new Building(file, key);
                        _depots.Add(building);
                    }

                    // Other buldings ...
                }
            }

            //for (int i = 0; i < file.lines("FrontMarker"); i++)
            //{
            //    string key;
            //    string value;
            //    file.get("FrontMarker", i, out key, out value);

            //    string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //    if (valueParts.Length == 3)
            //    {
            //        double x;
            //        double y;
            //        int army;
            //        if (double.TryParse(valueParts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out x)
            //            && double.TryParse(valueParts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out y)
            //            && int.TryParse(valueParts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out army))
            //        {
            //            if (army == 0)
            //            {
            //                _neutralFrontMarkers.Add(new Point3d(x, y, 0.0));
            //            }
            //            else if (army == 1)
            //            {
            //                _redFrontMarkers.Add(new Point3d(x, y, 0.0));
            //            }
            //            else if (army == 2)
            //            {
            //                _blueFrontMarkers.Add(new Point3d(x, y, 0.0));
            //            }
            //        }
            //    }
            //}

            for (int i = 0; i < file.lines("AirGroups"); i++)
            {
                string key;
                string value;
                file.get("AirGroups", i, out key, out value);

                AirGroup airGroup = new AirGroup(file, key, airGroupInfos);

                if (GetAirGroupInfo(1, airGroup.AirGroupKey) != null)
                {
                    _redAirGroups.Add(airGroup);
                }
                else if (GetAirGroupInfo(2, airGroup.AirGroupKey) != null)
                {
                    _blueAirGroups.Add(airGroup);
                }
            }

            for (int i = 0; i < file.lines("Chiefs"); i++)
            {
                string key;
                string value;
                file.get("Chiefs", i, out key, out value);

                GroundGroup groundGroup = new GroundGroup(file, key);

                if (groundGroup.Army == 1)
                {
                    _redGroundGroups.Add(groundGroup);
                }
                else if (groundGroup.Army == 2)
                {
                    _blueGroundGroups.Add(groundGroup);
                }
                else
                {
                    Waterway road = new Waterway(file, key);
                    if (value.StartsWith("Vehicle") || value.StartsWith("Armor"))
                    {
                        _roads.Add(road);
                    }
                    else if (value.StartsWith("Ship"))
                    {
                        _waterways.Add(road);
                    }
                    else if (value.StartsWith("Train"))
                    {
                        _railways.Add(road);
                    }
                }
            }
        }
        public IList<GroundGroup> GetGroundGroups(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return _redGroundGroups;
            }
            else if (armyIndex == 2)
            {
                return _blueGroundGroups;
            }
            else
            {
                return new List<GroundGroup>();
            }
        }

        public IList<AirGroup> GetAirGroups(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return _redAirGroups;
            }
            else if (armyIndex == 2)
            {
                return _blueAirGroups;
            }
            else
            {
                return new List<AirGroup>();
            }
        }

        public IList<Stationary> GetFriendlyStationaries(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return _redStationaries;
            }
            else if (armyIndex == 2)
            {
                return _blueStationaries;
            }
            else
            {
                return new List<Stationary>();
            }
        }

        public IList<Stationary> GetEnemyStationaries(int armyIndex)
        {
            if (armyIndex == 1)
            {
                return _blueStationaries;
            }
            else if (armyIndex == 2)
            {
                return _redStationaries;
            }
            else
            {
                return new List<Stationary>();
            }
        }

        private AirGroupInfo GetAirGroupInfo(int armyIndex, string airGroupKey)
        {
            AirGroupInfo airGroupInfo;
            if (airGroupInfos != null && (airGroupInfo = airGroupInfos.GetAirGroupInfo(armyIndex, airGroupKey)) != null)
            {
                return airGroupInfo;
            }
            else if ((airGroupInfo = AirGroupInfos.Default.GetAirGroupInfo(armyIndex, airGroupKey)) != null)
            {
                return airGroupInfo;
            }

            return null;
        }

        //public IList<Point3d> GetFriendlyMarkers(int armyIndex)
        //{
        //    if (armyIndex == 1)
        //    {
        //        return _redFrontMarkers;
        //    }
        //    else if (armyIndex == 2)
        //    {
        //        return _blueFrontMarkers;
        //    }
        //    else
        //    {
        //        return new List<Point3d>();
        //    }
        //}

            //public IList<Point3d> GetEnemyMarkers(int armyIndex)
            //{
            //    if (armyIndex == 1)
            //    {
            //        return _blueFrontMarkers;
            //    }
            //    else if (armyIndex == 2)
            //    {
            //        return _redFrontMarkers;
            //    }
            //    else
            //    {
            //        return new List<Point3d>();
            //    }
            //}
     }
}