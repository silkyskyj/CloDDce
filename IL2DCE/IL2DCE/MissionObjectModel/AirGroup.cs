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
using System.Globalization;
using System.Linq;
using System.Net;
using IL2DCE.Generator;
using maddox.game;
using maddox.game.world;
using maddox.GP;

namespace IL2DCE.MissionObjectModel
{
    public class AirGroup
    {
        public const string SquadronFormat = "D";
        public const int FlightCount = 4;
        public const string DefaultFormation = "LINEABREAST"; // "Line Abreast" or "Line Astern" is All Flight Group & All Country

        #region Public properties

        public string AirGroupKey
        {
            get;
            private set;
        }

        public int SquadronIndex
        {
            get
            {
                return _squadronIndex;
            }
        }
        private int _squadronIndex;

        public IDictionary<int, IList<string>> Flights
        {
            get
            {
                return _flights;
            }
        }
        IDictionary<int, IList<string>> _flights = new Dictionary<int, IList<string>>();

        public string Class
        {
            get;
            private set;
        }

        public string Formation
        {
            get;
            private set;
        }

        public int CallSign
        {
            get
            {
                return _callSign;
            }
        }
        private int _callSign;

        public int Fuel
        {
            get
            {
                return this._fuel;
            }
        }
        private int _fuel;

        public int[] Weapons
        {
            get;
            set;
        }

        public IList<string> Detonator
        {
            get
            {
                return _detonator;
            }
            set
            {
                _detonator = value;
            }
        }
        private IList<string> _detonator = new List<string>();

        public string Skill
        {
            get;
            set;
        }

        public IDictionary<int, string> Skills
        {
            get;
            set;
        }

        public string Aging
        {
            get;
            set;
        }

        public IDictionary<int, string> Skin
        {
            get;
            set;
        }

        public IDictionary<int, string> MarkingsOn
        {
            get;
            set;
        }

        public IDictionary<int, string> BandColor
        {
            get;
            set;
        }

        public Point3d Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }
        private Point3d _position;

        public double Speed
        {
            get
            {
                // TODO: Use aicraft info to determine speed
                return speed;
            }
        }
        public double speed = 300.0;

        public bool Airstart
        {
            get;
            set;
        }

        public bool SetOnParked
        {
            get;
            set;
        }

        public string Briefing
        {
            get;
            set;
        }

        public string Id
        {
            get;
            private set;
        }

        public int ArmyIndex
        {
            get;
            private set;
        }

        public AirGroupInfo AirGroupInfo
        {
            get;
            private set;
        }

        public IList<AirGroupWaypoint> Waypoints
        {
            get
            {
                return _waypoints;
            }
        }
        private IList<AirGroupWaypoint> _waypoints = new List<AirGroupWaypoint>();

        public double? Altitude
        {
            get;
            private set;
        }

        public AirGroup EscortAirGroup
        {
            get;
            private set;
        }

        public Stationary TargetStationary
        {
            get;
            private set;
        }

        public GroundGroup TargetGroundGroup
        {
            get;
            private set;
        }

        public AirGroup TargetAirGroup
        {
            get;
            private set;
        }

        public Point2d? TargetArea
        {
            get
            {
                return _targetArea;
            }
            set
            {
                _targetArea = value;
            }
        }
        private Point2d? _targetArea = null;

        public string DisplayName
        {
            get
            {
                return string.Format("{0}.{1}", CreateDisplayName(AirGroupKey), _squadronIndex.ToString(SquadronFormat, CultureInfo.InvariantCulture.NumberFormat));
            }
        }

        public string DisplayDetailName
        {
            get
            {
                return string.Format("{0} ({1})", DisplayName, AircraftInfo.CreateDisplayName(Class));
            }
        }

        public Spawn Spawn
        {
            get;
            private set;
        }

        public object Data
        {
            get;
            set;
        }

        #endregion

        #region Public constructors

        public AirGroup(ISectionFile sectionFile, string id)
        {
            // airGroupId = <airGroupKey>.<squadronIndex><flightMask>

            // AirGroupKey
            AirGroupKey = id.Substring(0, id.IndexOf("."));

            // SquadronIndex
            if (!int.TryParse(id.Substring(id.LastIndexOf(".") + 1, 1), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _squadronIndex))
            //if (!int.TryParse(id.Substring(id.LastIndexOf(".") + 1), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _squadronIndex))
            {
                Debug.Assert(false);
                throw new FormatException(string.Format("Invalid AirGroup ID[{0}]", id));
            }

            // Flight
            for (int i = 0; i < FlightCount; i++)
            {
                string key = MissionFile.KeyFlight + i.ToString(CultureInfo.InvariantCulture.NumberFormat);
                if (sectionFile.exist(id, key))
                {
                    string acNumberLine = sectionFile.get(id, key);
                    string[] acNumberList = acNumberLine.Split(new char[] { ' ' });
                    if (acNumberList.Length > 0)
                    {
                        List<string> acNumbers = new List<string>();
                        Flights.Add(i, acNumbers);
                        for (int j = 0; j < acNumberList.Length; j++)
                        {
                            acNumbers.Add(acNumberList[j]);
                        }
                    }
                }
            }

            // Class
            Class = sectionFile.get(id, MissionFile.KeyClass);

            // Formation
            Formation = sectionFile.get(id, MissionFile.KeyFormation, string.Empty);

            // CallSign
            int.TryParse(sectionFile.get(id, MissionFile.KeyCallSign, "0"), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _callSign);

            // Fuel
            int.TryParse(sectionFile.get(id, MissionFile.KeyFuel, "100"), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _fuel);

            // Weapons
            string weaponsLine = sectionFile.get(id, MissionFile.KeyWeapons);
            string[] weaponsList = weaponsLine.Split(new char[] { ' ' });
            if (weaponsList != null && weaponsList.Length > 0)
            {
                Weapons = new int[weaponsList.Length];
                for (int i = 0; i < weaponsList.Length; i++)
                {
                    int.TryParse(weaponsList[i], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out Weapons[i]);
                }
            }

            // Detonator
            for (int i = 0; i < sectionFile.lines(id); i++)
            {
                string key;
                string value;
                sectionFile.get(id, i, out key, out value);
                if (key == MissionFile.KeyDetonator)
                {
                    _detonator.Add(value);
                }
            }

            // Belt
            // TODO: Parse belt

            // Skill
            // TODO: Multi Skill(=Different)
            Skill = sectionFile.get(id, MissionFile.KeySkill, string.Empty);
            Skills = ReadFligthTypeValue(sectionFile, id, Flights, MissionFile.KeySkill);
            //    new Dictionary<int, string>();
            //foreach (int flightIndex in Flights.Keys)
            //{
            //    for (int i = 0; i < Flights[flightIndex].Count; i++)
            //    {
            //        string key = string.Format("{0}{1}{2}", MissionFile.KeySkill,
            //            flightIndex > 0 ? flightIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) : string.Empty, i.ToString(CultureInfo.InvariantCulture.NumberFormat));
            //        if (sectionFile.exist(id, key))
            //        {
            //            string skill = sectionFile.get(id, key);
            //            if (!string.IsNullOrEmpty(skill))
            //            {
            //                Skills.Add(flightIndex * 10 + i, skill);
            //            }
            //        }
            //    }
            //}
#if DEBUG && false
            Debug.WriteLine(string.Format("Skill[{0}]={1}", AirGroupKey, Skill));
            if (Skills.Count > 0)
            {
                foreach (var item in Skills)
                {
                    Debug.WriteLine(string.Format("Skill{0}={1}", item.Key, item.Value));
                }
            }
#endif

            // Aging
            Aging = sectionFile.get(id, MissionFile.KeyAging, string.Empty);

            // Skin
            Skin = ReadFligthTypeValue(sectionFile, id, Flights, MissionFile.KeySkin);
            //Skin = new Dictionary<int, string>();
            //foreach (int flightIndex in Flights.Keys)
            //{
            //    for (int i = 0; i < Flights[flightIndex].Count; i++)
            //    {
            //        string key = string.Format("{0}{1}{2}", MissionFile.KeySkin,
            //            flightIndex > 0 ? flightIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) : string.Empty, i.ToString(CultureInfo.InvariantCulture.NumberFormat));
            //        if (sectionFile.exist(id, key))
            //        {
            //            string skin = sectionFile.get(id, key);
            //            if (!string.IsNullOrEmpty(skin))
            //            {
            //                Skin.Add(flightIndex * 10 + i, skin);
            //            }
            //        }
            //    }
            //}

            // MarkingsOn
            MarkingsOn = ReadFligthTypeValue(sectionFile, id, Flights, MissionFile.KeyMarkingsOn);
            //MarkingsOn = new Dictionary<int, string>();
            //foreach (int flightIndex in Flights.Keys)
            //{
            //    for (int i = 0; i < Flights[flightIndex].Count; i++)
            //    {
            //        string key = string.Format("{0}{1}{2}", MissionFile.KeyMarkingsOn,
            //            flightIndex > 0 ? flightIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) : string.Empty, i.ToString(CultureInfo.InvariantCulture.NumberFormat));
            //        if (sectionFile.exist(id, key))
            //        {
            //            string markingsOn = sectionFile.get(id, key);
            //            if (!string.IsNullOrEmpty(markingsOn))
            //            {
            //                MarkingsOn.Add(flightIndex * 10 + i, markingsOn);
            //            }
            //        }
            //    }
            //}

            // BandColor
            BandColor = ReadFligthTypeValue(sectionFile, id, Flights, MissionFile.KeyBandColor);
            //BandColor = new Dictionary<int, string>();
            //foreach (int flightIndex in Flights.Keys)
            //{
            //    for (int i = 0; i < Flights[flightIndex].Count; i++)
            //    {
            //        string key = string.Format("{0}{1}{2}", MissionFile.KeyBandColor,
            //            flightIndex > 0 ? flightIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) : string.Empty, i.ToString(CultureInfo.InvariantCulture.NumberFormat));
            //        if (sectionFile.exist(id, key))
            //        {
            //            string bandColor = sectionFile.get(id, key);
            //            if (!string.IsNullOrEmpty(bandColor))
            //            {
            //                BandColor.Add(flightIndex * 10 + i, bandColor);
            //            }
            //        }
            //    }
            //}

            // Id
            int flightMask = 0x0;
            foreach (int flightIndex in Flights.Keys)
            {
                if (Flights[flightIndex].Count > 0)
                {
                    int bit = (0x1 << flightIndex);
                    flightMask = (flightMask | bit);
                }
            }
            Id = ToString() + flightMask.ToString("X");

            // Waypoints            
            for (int i = 0; i < sectionFile.lines(Id + MissionFile.SectionWay); i++)
            {
                AirGroupWaypoint waypoint = new AirGroupWaypoint(sectionFile, Id, i);
                _waypoints.Add(waypoint);
            }
            if (_waypoints.Count > 0)
            {
                Position = new Point3d(_waypoints[0].X, _waypoints[0].Y, _waypoints[0].Z);
                if (_waypoints[0].Type == AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF)
                {
                    Airstart = false;
                }
                else if (_waypoints[0].Type == AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY)
                {
                    Airstart = true;
                }
            }

            // SetOnPark
            SetOnParked = string.Compare(sectionFile.get(id, MissionFile.KeySetOnPark, "0"), "1") == 0;

            Optimize();
        }

        #endregion

        #region Private methods

        private Dictionary<int, string> ReadFligthTypeValue(ISectionFile sectionFile, string id, IDictionary<int, IList<string>> flights, string keyInfo)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            foreach (int flightIndex in flights.Keys)
            {
                for (int i = 0; i < flights[flightIndex].Count; i++)
                {
                    string key = string.Format("{0}{1}{2}", keyInfo,
                        flightIndex > 0 ? flightIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) : string.Empty, i.ToString(CultureInfo.InvariantCulture.NumberFormat));
                    if (sectionFile.exist(id, key))
                    {
                        string value = sectionFile.get(id, key);
                        if (!string.IsNullOrEmpty(value))
                        {
                            dic.Add(flightIndex * 10 + i, value);
                        }
                    }
                }
            }
            return dic;
        }

        private void WriteFlightTypeValue(ISectionFile sectionFile, string id, IDictionary<int, string> dic, string keyInfo)
        {
            foreach (var item in dic)
            {
                int flight = item.Key / 10;
                string key = string.Format("{0}{1}{2}", keyInfo,
                    (flight > 0 ? flight.ToString(CultureInfo.InvariantCulture.NumberFormat) : string.Empty), (item.Key % 10));
                sectionFile.add(id, key, item.Value);
            }
        }

        private IDictionary<int, string> UpdateFlightTypeValue(IDictionary<int, IList<string>> flights, IDictionary<int, string> dic)
        {
            Dictionary<int, string> dicNew = new Dictionary<int, string>();
            string value = string.Empty;
            foreach (int flightIndex in flights.Keys)
            {
                for (int i = 0; i < flights[flightIndex].Count; i++)
                {
                    int key = flightIndex * 10 + i;
                    if (dic.ContainsKey(key))
                    {
                        value = dic[key];
                    }
                    else if (string.IsNullOrEmpty(value))
                    {
                        Debug.Assert(false);
                    }
                    else
                    {
                        // Use previous value
                    }
                    dicNew.Add(key, value);
                }
            }
            return dicNew;
        }

        private void UpdateFlightTypeValue()
        {
            // Skills
            if (Skills.Count > 0)
            {
                Skills = UpdateFlightTypeValue(Flights, Skills);
            }

            // Skin 
            if (Skin.Count > 0)
            {
                Skin = UpdateFlightTypeValue(Flights, Skin);
            }

            // MarkingsOn
            if (MarkingsOn.Count > 0)
            {
                MarkingsOn = UpdateFlightTypeValue(Flights, MarkingsOn);
            }

            // BandColor
            if (BandColor.Count > 0)
            {
                BandColor = UpdateFlightTypeValue(Flights, BandColor);
            }
        }

        private double distanceBetween(AirGroupWaypoint start, AirGroupWaypoint end)
        {
            double distanceStart = distanceTo(start);
            double distanceEnd = distanceTo(end);
            return distanceEnd - distanceStart;
        }

        private double distanceTo(AirGroupWaypoint waypoint)
        {
            double distance = 0.0;
            AirGroupWaypoint previousWaypoint = null;
            if (_waypoints.Contains(waypoint))
            {
                foreach (AirGroupWaypoint wp in _waypoints)
                {
                    if (previousWaypoint == null)
                    {
                        distance = 0.0;
                    }
                    else
                    {
                        Point3d p = wp.Position;
                        distance += previousWaypoint.Position.distance(ref p);
                    }
                    previousWaypoint = wp;

                    if (wp == waypoint)
                    {
                        break;
                    }
                }
            }

            return distance;
        }

        private void createStartWaypoints()
        {
            if (!Airstart)
            {
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF, Position, AirGroupWaypoint.DefaultTakeoffV));
            }
            else
            {
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, Position, Speed));
            }
        }

        private void createEndWaypoints(AiAirport landingAirport = null)
        {
            if (landingAirport != null)
            {
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.LANDING, landingAirport.Pos(), AirGroupWaypoint.DefaultLandingV));
            }
            else
            {
                if (!Airstart)
                {
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.LANDING, Position, AirGroupWaypoint.DefaultLandingV));
                }
                else
                {
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, Position, Speed));
                }
            }
        }

        private void createInbetweenWaypoints(Point3d from, Point3d to, bool plusRandomValue = false)
        {
            double mpX = (to.x - from.x);
            double mpY = (to.y - from.y);
            double mpZ = (to.z - from.z);
            if (plusRandomValue)
            {
                mpX *= Random.Default.Next(90, 110) / 100.0;
                mpY *= Random.Default.Next(90, 110) / 100.0;
            }
            Point3d p1 = new Point3d(from.x + 0.25 * mpX, from.y + 0.25 * mpY, from.z + 1.00 * mpZ);
            Point3d p2 = new Point3d(from.x + 0.50 * mpX, from.y + 0.50 * mpY, from.z + 1.00 * mpZ);
            Point3d p3 = new Point3d(from.x + 0.75 * mpX, from.y + 0.75 * mpY, from.z + 1.00 * mpZ);

            _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p1, AirGroupWaypoint.DefaultNormaflyV));
            _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p2, AirGroupWaypoint.DefaultNormaflyV));
            _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p3, AirGroupWaypoint.DefaultNormaflyV));
        }

        private void createStartInbetweenPoints(Point3d target, bool plusRandomValue = false)
        {
            createInbetweenWaypoints(Position, target, plusRandomValue);
        }

        private void createEndInbetweenPoints(Point3d target, AiAirport landingAirport = null, bool plusRandomValue = false)
        {
            if (landingAirport != null)
            {
                Point3d point = new Point3d(landingAirport.Pos().x, landingAirport.Pos().y, target.z);
                createInbetweenWaypoints(target, point, plusRandomValue);
            }
            else
            {
                Point3d point = new Point3d(Position.x, Position.y, target.z);
                createInbetweenWaypoints(target, point, plusRandomValue);
            }
        }

        private Point3d? calculateRendevouzPoint(AirGroup escortAirGroup)
        {
            var altitude = Altitude.Value;
            return new Point3d(Position.x + 0.33 * (escortAirGroup.Position.x - Position.x), Position.y + 0.33 * (escortAirGroup.Position.y - Position.y), altitude);
        }

        private void reset()
        {
            _waypoints.Clear();

            this.Altitude = null;
            this.EscortAirGroup = null;
            this.TargetAirGroup = null;
            this.TargetGroundGroup = null;
            this.TargetStationary = null;
            this.TargetArea = null;
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return CreateSquadronString(AirGroupKey, SquadronIndex);
        }

        public static string CreateSquadronString(string airGroupKey, int squadronIndex)
        {
            return string.Format("{0}.{1}", airGroupKey, squadronIndex.ToString(SquadronFormat, CultureInfo.InvariantCulture.NumberFormat));
        }

        public static string CreateDisplayName(string airGroupKey)
        {
            // tobruk:Tobruk_RA_30St_87_Gruppo_192Sq -> Tobruk_RA_30St_87_Gruppo_192Sq
            const string del = ":";
            int idx = airGroupKey.IndexOf(del, StringComparison.CurrentCultureIgnoreCase);
            return idx != -1 ? airGroupKey.Substring(idx + del.Length) : airGroupKey;
        }

        public void WriteTo(ISectionFile sectionFile)
        {
            if (_waypoints.Count > 0)
            {
                // Section AirGroup
                sectionFile.add(MissionFile.SectionAirGroups, Id, string.Empty);

                // Flight
                foreach (int flightIndex in Flights.Keys)
                {
                    if (Flights[flightIndex].Count > 0)
                    {
                        string acNumberLine = string.Empty;
                        foreach (string acNumber in Flights[flightIndex])
                        {
                            acNumberLine += acNumber + " ";
                        }
                        sectionFile.add(Id, MissionFile.KeyFlight + flightIndex, acNumberLine.TrimEnd());
                    }
                }

                sectionFile.add(Id, MissionFile.KeyClass, Class);
                sectionFile.add(Id, MissionFile.KeyFormation, Formation);
                sectionFile.add(Id, MissionFile.KeyCallSign, CallSign.ToString(CultureInfo.InvariantCulture.NumberFormat));
                sectionFile.add(Id, MissionFile.KeyFuel, Fuel.ToString(CultureInfo.InvariantCulture.NumberFormat));

                // Weapons
                if (Weapons != null && Weapons.Length > 0)
                {
                    string weaponsLine = string.Empty;
                    foreach (int weapon in Weapons)
                    {
                        weaponsLine += weapon.ToString(CultureInfo.InvariantCulture.NumberFormat) + " ";
                    }
                    sectionFile.add(Id, MissionFile.KeyWeapons, weaponsLine.TrimEnd());
                }

                if (Detonator != null && Detonator.Count > 0)
                {
                    foreach (string detonator in Detonator)
                    {
                        sectionFile.add(Id, MissionFile.KeyDetonator, detonator);
                    }
                }

                // Update FlightCount & FlightSize 
                UpdateFlightTypeValue();

                // Skill
                if (Skills != null && Skills.Count > 0)
                {
                    //// TODO: Multi Skill(=Different)
                    //foreach (var item in Skills)
                    //{
                    //    int flight = item.Key / 10;
                    //    string key = string.Format("{0}{1}{2}", MissionFile.KeySkill,
                    //        (flight > 0 ? flight.ToString(CultureInfo.InvariantCulture.NumberFormat) : string.Empty), (item.Key % 10));
                    //    sectionFile.add(Id, key, item.Value);
                    //}
                    WriteFlightTypeValue(sectionFile, Id, Skills, MissionFile.KeySkill);
                }
                else if (!string.IsNullOrEmpty(Skill))
                {
                    sectionFile.add(Id, MissionFile.KeySkill, Skill);
                }
                else
                {
                    Debug.Assert(false);
                }

                // Aging
                if (!string.IsNullOrEmpty(Aging))
                {
                    sectionFile.add(Id, MissionFile.KeyAging, Aging);
                }

                // Skin
                if (Skin != null && Skin.Count > 0)
                {
                    //foreach (var item in Skin)
                    //{
                    //    int flight = item.Key / 10;
                    //    string key = string.Format("{0}{1}{2}", MissionFile.KeySkin,
                    //        (flight > 0 ? flight.ToString(CultureInfo.InvariantCulture.NumberFormat) : string.Empty), (item.Key % 10));
                    //    sectionFile.add(Id, key, item.Value);
                    //}
                    WriteFlightTypeValue(sectionFile, Id, Skin, MissionFile.KeySkin);
                }

                // MarkingsOn 
                if (MarkingsOn != null && MarkingsOn.Count > 0)
                {
                    //foreach (var item in MarkingsOn)
                    //{
                    //    int flight = item.Key / 10;
                    //    string key = string.Format("{0}{1}{2}", MissionFile.KeyMarkingsOn,
                    //        (flight > 0 ? flight.ToString(CultureInfo.InvariantCulture.NumberFormat) : string.Empty), (item.Key % 10));
                    //    sectionFile.add(Id, key, item.Value);
                    //}
                    WriteFlightTypeValue(sectionFile, Id, MarkingsOn, MissionFile.KeyMarkingsOn);
                }

                // BandColor
                if (BandColor != null && BandColor.Count > 0)
                {
                    //foreach (var item in BandColor)
                    //{
                    //    int flight = item.Key / 10;
                    //    string key = string.Format("{0}{1}{2}", MissionFile.KeyBandColor,
                    //        (flight > 0 ? flight.ToString(CultureInfo.InvariantCulture.NumberFormat) : string.Empty), (item.Key % 10));
                    //    sectionFile.add(Id, key, item.Value);
                    //}
                    WriteFlightTypeValue(sectionFile, Id, BandColor, MissionFile.KeyBandColor);
                }

                // Spawn (SetOnPark/Idle/Scramble)
                if (Spawn != null)
                {
                    switch (Spawn.Type)
                    {
                        case ESpawn.Parked:
                            sectionFile.add(Id, MissionFile.KeySetOnPark, "1");
                            break;
                        case ESpawn.Idle:
                            sectionFile.add(Id, "Idle", "1");
                            break;
                        case ESpawn.Scramble:
                            sectionFile.add(Id, "Scramble", "1");
                            break;
                            //case ESpawn.AirStart:
                            //default:
                            //    sectionFile.add(Id, "SetOnPark", "0");
                            //    sectionFile.add(Id, "Idle", "0");
                            //    sectionFile.add(Id, "Scramble", "0");
                            //    break;
                    }
                }
                else
                {
                    sectionFile.add(Id, MissionFile.KeySetOnPark, SetOnParked ? "1" : "0");
                }

                foreach (AirGroupWaypoint waypoint in _waypoints)
                {
                    if (waypoint.Target == null)
                    {
                        sectionFile.add(Id + MissionFile.SectionWay,
                                        waypoint.Type.ToString(),
                                        waypoint.X.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Y.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Z.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.V.ToString(CultureInfo.InvariantCulture.NumberFormat));
                    }
                    else
                    {
                        sectionFile.add(Id + MissionFile.SectionWay,
                                        waypoint.Type.ToString(),
                                        waypoint.X.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Y.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Z.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.V.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Target);
                    }
                }

                sectionFile.add(Id, MissionFile.KeyBriefing, this.Id);
            }
        }

        #region Operation

        public void Transfer(double altitude, AiAirport landingAirport = null)
        {
            reset();
            this.Altitude = altitude;

            createStartWaypoints();

            Point3d target = new Point3d(Position.x, Position.y, altitude);
            createEndInbetweenPoints(target, landingAirport);
            createEndWaypoints(landingAirport);
        }

        public void Cover(AirGroup offensiveAirGroup, double altitude, AiAirport landingAirport = null)
        {
            reset();
            Altitude = altitude;
            TargetAirGroup = offensiveAirGroup;

            Point3d? position = null;
            GroundGroup groundGroup = offensiveAirGroup.TargetGroundGroup;
            if (groundGroup != null)
            {
                TargetGroundGroup = groundGroup;
                if (groundGroup.Waypoints.Count > 0)
                {
                    position = new Point3d(groundGroup.Waypoints[0].Position.x, groundGroup.Waypoints[0].Position.y, altitude);
                }
            }
            else if (offensiveAirGroup.TargetStationary != null)
            {
                Stationary stationary = offensiveAirGroup.TargetStationary;
                TargetStationary = stationary;
                position = new Point3d(stationary.Position.x, stationary.Position.y, altitude);
            }

            if (position.HasValue)
            {
                createStartWaypoints();

                createStartInbetweenPoints(position.Value);

                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x, position.Value.y, altitude, AirGroupWaypoint.DefaultFlyV));

                AirGroupWaypoint start = _waypoints[_waypoints.Count - 1];

                while (distanceBetween(start, _waypoints[_waypoints.Count - 1]) < 200000.0)
                {
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x + 5000.0, position.Value.y + 5000.0, altitude, AirGroupWaypoint.DefaultFlyV));
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x + 5000.0, position.Value.y - 5000.0, altitude, AirGroupWaypoint.DefaultFlyV));
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x - 5000.0, position.Value.y - 5000.0, altitude, AirGroupWaypoint.DefaultFlyV));
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x - 5000.0, position.Value.y + 5000.0, altitude, AirGroupWaypoint.DefaultFlyV));
                }

                createEndInbetweenPoints(position.Value, landingAirport);
                createEndWaypoints(landingAirport);
            }
        }

        public void Hunting(Point2d targetArea, double altitude, AiAirport landingAirport = null)
        {
            reset();
            Altitude = altitude;
            TargetArea = targetArea;

            createStartWaypoints();

            Point3d p = new Point3d(targetArea.x, targetArea.y, altitude);
            createStartInbetweenPoints(p, true);

            _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.HUNTING, targetArea.x, targetArea.y, altitude, AirGroupWaypoint.DefaultFlyV));

            createEndInbetweenPoints(p, landingAirport, true);
            createEndWaypoints(landingAirport);
        }

        public void GroundAttack(Stationary targetStationary, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            reset();
            Altitude = altitude;
            TargetStationary = targetStationary;
            EscortAirGroup = escortAirGroup;

            Point3d? rendevouzPosition = null;
            if (EscortAirGroup != null)
            {
                rendevouzPosition = calculateRendevouzPoint(EscortAirGroup);
            }

            createStartWaypoints();

            if (rendevouzPosition != null && rendevouzPosition.HasValue)
            {
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, AirGroupWaypoint.DefaultNormaflyV));
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createInbetweenWaypoints(rendevouzPosition.Value, pStart);
            }
            else
            {
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createStartInbetweenPoints(pStart);
            }

            _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG, targetStationary.X, targetStationary.Y, altitude, AirGroupWaypoint.DefaultFlyV, targetStationary.Id));

            Point3d pEnd = new Point3d(targetStationary.X, targetStationary.Y, altitude);
            createEndInbetweenPoints(pEnd, landingAirport);
            createEndWaypoints(landingAirport);
        }

        public void GroundAttack(GroundGroup targetGroundGroup, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            reset();
            Altitude = altitude;
            TargetGroundGroup = targetGroundGroup;
            EscortAirGroup = escortAirGroup;

            Point3d? rendevouzPosition = null;
            if (EscortAirGroup != null)
            {
                rendevouzPosition = calculateRendevouzPoint(EscortAirGroup);
            }

            if (targetGroundGroup.Waypoints.Count > 0)
            {
                createStartWaypoints();

                List<GroundGroupWaypoint> targetWaypoints = targetGroundGroup.Waypoints;
                Point2d point2d = targetWaypoints[0].Position;
                if (rendevouzPosition != null && rendevouzPosition.HasValue)
                {
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, AirGroupWaypoint.DefaultNormaflyV));
                    Point3d pStart = new Point3d(point2d.x, point2d.y, altitude);
                    createInbetweenWaypoints(rendevouzPosition.Value, pStart);
                }
                else
                {
                    Point3d pStart = new Point3d(point2d.x, point2d.y, altitude);
                    createStartInbetweenPoints(pStart);
                }

                GroundGroupWaypoint lastGroundGroupWaypoint = null;
                AirGroupWaypoint start = null;
                foreach (GroundGroupWaypoint groundGroupWaypoint in targetWaypoints)
                {
                    lastGroundGroupWaypoint = groundGroupWaypoint;
                    point2d = groundGroupWaypoint.Position;
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG, point2d.x, point2d.y, altitude, AirGroupWaypoint.DefaultFlyV, targetGroundGroup.Id + " " + targetWaypoints.IndexOf(groundGroupWaypoint)));
                    if (start == null)
                    {
                        start = _waypoints[_waypoints.Count - 1];
                    }
                    else
                    {
                        if (distanceBetween(start, _waypoints[_waypoints.Count - 1]) > 20000.0)
                        {
                            break;
                        }
                    }
                }

                if (lastGroundGroupWaypoint != null)
                {
                    Point3d pEnd = new Point3d(lastGroundGroupWaypoint.Position.x, lastGroundGroupWaypoint.Position.y, altitude);
                    createEndInbetweenPoints(pEnd, landingAirport);
                }

                createEndWaypoints(landingAirport);
            }
        }

        public void Recon(Stationary targetStationary, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            reset();
            Altitude = altitude;
            TargetStationary = targetStationary;
            EscortAirGroup = escortAirGroup;

            Point3d? rendevouzPosition = null;
            if (EscortAirGroup != null)
            {
                rendevouzPosition = calculateRendevouzPoint(EscortAirGroup);
            }

            createStartWaypoints();

            if (rendevouzPosition != null && rendevouzPosition.HasValue)
            {
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, AirGroupWaypoint.DefaultNormaflyV));
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createInbetweenWaypoints(rendevouzPosition.Value, pStart);
            }
            else
            {
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createStartInbetweenPoints(pStart);
            }

            _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.RECON, targetStationary.X, targetStationary.Y, altitude, AirGroupWaypoint.DefaultFlyV, targetStationary.Id));

            Point3d pEnd = new Point3d(targetStationary.X, targetStationary.Y, altitude);
            createEndInbetweenPoints(pEnd, landingAirport);
            createEndWaypoints(landingAirport);
        }

        public void Recon(GroundGroup targetGroundGroup, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            reset();
            Altitude = altitude;
            TargetGroundGroup = targetGroundGroup;
            EscortAirGroup = escortAirGroup;

            Point3d? rendevouzPosition = null;
            if (EscortAirGroup != null)
            {
                rendevouzPosition = calculateRendevouzPoint(EscortAirGroup);
            }

            List<GroundGroupWaypoint> targetWaypoints = targetGroundGroup.Waypoints;
            if (targetWaypoints.Count > 0)
            {
                createStartWaypoints();

                if (rendevouzPosition != null && rendevouzPosition.HasValue)
                {
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, AirGroupWaypoint.DefaultNormaflyV));
                    Point3d pStart = new Point3d(targetWaypoints[0].Position.x, targetWaypoints[0].Position.y, altitude);
                    createInbetweenWaypoints(rendevouzPosition.Value, pStart);
                }
                else
                {
                    Point3d pStart = new Point3d(targetWaypoints[0].Position.x, targetWaypoints[0].Position.y, altitude);
                    createStartInbetweenPoints(pStart);
                }

                GroundGroupWaypoint lastGroundGroupWaypoint = null;
                AirGroupWaypoint start = null;
                foreach (GroundGroupWaypoint groundGroupWaypoint in targetWaypoints)
                {
                    lastGroundGroupWaypoint = groundGroupWaypoint;
                    Point2d point2d = groundGroupWaypoint.Position;
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.RECON, point2d.x, point2d.y, altitude, AirGroupWaypoint.DefaultFlyV, targetGroundGroup.Id + " " + targetWaypoints.IndexOf(groundGroupWaypoint)));
                    if (start == null)
                    {
                        start = _waypoints[_waypoints.Count - 1];
                    }
                    else
                    {
                        if (distanceBetween(start, _waypoints[_waypoints.Count - 1]) > 20000.0)
                        {
                            break;
                        }
                    }
                }

                if (lastGroundGroupWaypoint != null)
                {
                    Point3d pEnd = new Point3d(lastGroundGroupWaypoint.Position.x, lastGroundGroupWaypoint.Position.y, altitude);
                    createEndInbetweenPoints(pEnd, landingAirport);
                }

                createEndWaypoints(landingAirport);
            }
        }

        public void Escort(AirGroup targetAirGroup, AiAirport landingAirport = null)
        {
            reset();
            Altitude = targetAirGroup.Altitude;
            TargetAirGroup = targetAirGroup;
            IList<AirGroupWaypoint> targetWaypoints = targetAirGroup.Waypoints;

            createStartWaypoints();

            foreach (AirGroupWaypoint waypoint in targetWaypoints)
            {
                if (waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF && waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.LANDING)
                {
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.ESCORT, waypoint.X, waypoint.Y, waypoint.Z, AirGroupWaypoint.DefaultFlyV, targetAirGroup.Id + " " + targetWaypoints.IndexOf(waypoint)));
                }
            }

            createEndWaypoints(landingAirport);
        }

        public void Follow(AirGroup targetAirGroup, AiAirport landingAirport = null)
        {
            reset();
            Altitude = targetAirGroup.Altitude;
            TargetAirGroup = targetAirGroup;
            IList<AirGroupWaypoint> targetWaypoints = targetAirGroup.Waypoints;

            createStartWaypoints();

            foreach (AirGroupWaypoint waypoint in targetWaypoints)
            {
                if (waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF && waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.LANDING)
                {
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.FOLLOW, waypoint.X, waypoint.Y, waypoint.Z, AirGroupWaypoint.DefaultFlyV, targetAirGroup.Id + " " + targetWaypoints.IndexOf(waypoint)));
                }
            }

            createEndWaypoints(landingAirport);
        }

        public void Intercept(AirGroup targetAirGroup, AiAirport landingAirport = null)
        {
            reset();
            Altitude = targetAirGroup.Altitude;
            TargetAirGroup = targetAirGroup;
            IList<AirGroupWaypoint> targetWaypoints = targetAirGroup.Waypoints;

            createStartWaypoints();

            AirGroupWaypoint interceptWaypoint = null;
            AirGroupWaypoint closestInterceptWaypoint = null;
            foreach (AirGroupWaypoint waypoint in targetWaypoints)
            {
                Point3d p = Position;
                if (targetAirGroup.distanceTo(waypoint) > waypoint.Position.distance(ref p))
                {
                    interceptWaypoint = waypoint;
                    break;
                }
                else
                {
                    if (closestInterceptWaypoint == null)
                    {
                        closestInterceptWaypoint = waypoint;
                    }
                    else
                    {
                        if (targetAirGroup.distanceTo(waypoint) < closestInterceptWaypoint.Position.distance(ref p))
                        {
                            closestInterceptWaypoint = waypoint;
                        }
                    }
                }
            }

            if (interceptWaypoint != null)
            {
                createStartInbetweenPoints(interceptWaypoint.Position);
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, interceptWaypoint.X, interceptWaypoint.Y, interceptWaypoint.Z, AirGroupWaypoint.DefaultFlyV, targetAirGroup.Id + " " + targetWaypoints.IndexOf(interceptWaypoint)));

                if (targetWaypoints.IndexOf(interceptWaypoint) - 1 >= 0)
                {
                    AirGroupWaypoint nextInterceptWaypoint = targetWaypoints[targetWaypoints.IndexOf(interceptWaypoint) - 1];
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, nextInterceptWaypoint.X, nextInterceptWaypoint.Y, nextInterceptWaypoint.Z, AirGroupWaypoint.DefaultFlyV, targetAirGroup.Id + " " + targetWaypoints.IndexOf(nextInterceptWaypoint)));

                    createEndInbetweenPoints(nextInterceptWaypoint.Position, landingAirport);
                }
                else
                {
                    createEndInbetweenPoints(interceptWaypoint.Position, landingAirport);
                }
            }
            else if (closestInterceptWaypoint != null)
            {
                createStartInbetweenPoints(closestInterceptWaypoint.Position);
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, closestInterceptWaypoint.X, closestInterceptWaypoint.Y, closestInterceptWaypoint.Z, AirGroupWaypoint.DefaultFlyV, targetAirGroup.Id + " " + targetWaypoints.IndexOf(closestInterceptWaypoint)));


                if (targetWaypoints.IndexOf(closestInterceptWaypoint) + 1 < targetWaypoints.Count)
                {
                    AirGroupWaypoint nextInterceptWaypoint = targetWaypoints[targetWaypoints.IndexOf(interceptWaypoint) + 1];
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, nextInterceptWaypoint.X, nextInterceptWaypoint.Y, nextInterceptWaypoint.Z, AirGroupWaypoint.DefaultFlyV, targetAirGroup.Id + " " + targetWaypoints.IndexOf(nextInterceptWaypoint)));

                    createEndInbetweenPoints(nextInterceptWaypoint.Position, landingAirport);
                }
                else
                {
                    createEndInbetweenPoints(closestInterceptWaypoint.Position, landingAirport);
                }
            }

            createEndWaypoints(landingAirport);
        }

        #endregion 

        public void SetSpawn(Spawn spawn)
        {
            this.Spawn = spawn;

            if (spawn != null)
            {
                AirGroupWaypoint way = Waypoints.FirstOrDefault();
                if (way != null && spawn.Type != ESpawn.Default)
                {
                    ESpawn type = spawn.Type;
                    if (type == ESpawn.Random)
                    {
                        type = new Spawn((int)ESpawn.Random).Type;
                    }
                    AirGroupWaypoint wayNew;
                    switch (type)
                    {
                        case ESpawn.Parked:
                        case ESpawn.Idle:
                        case ESpawn.Scramble:
                            wayNew = new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF, way.X, way.Y, AirGroupWaypoint.DefaultTakeoffZ, AirGroupWaypoint.DefaultTakeoffV, way.Target);
                            Airstart = false;
                            break;
                        case ESpawn.AirStart:
                        default:
                            wayNew = new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, way.X, way.Y, spawn.Altitude, AirGroupWaypoint.DefaultNormaflyV, way.Target);
                            Airstart = true;
                            break;
                    }
                    Waypoints[0] = wayNew;

                    // TODO: Update after Waypoint Z(Altitude) value
                }
            }
        }

        public void SetSpeed(int speed)
        {
            if (speed != -1)
            {
                this.speed = speed;
            }
        }

        public void SetFuel(int fuel)
        {
            if (speed != -1)
            {
                _fuel = fuel;
            }
        }

        public void SetAirGroupInfo(AirGroupInfo airGroupInfo)
        {
            AirGroupInfo = airGroupInfo;
            ArmyIndex = airGroupInfo.ArmyIndex;
        }

        public void Optimize()
        {
            // TODO: Multi Skill(=Different)
            if (string.IsNullOrEmpty(Skill))
            {
                Skill = MissionObjectModel.Skill.GetSystemType().ToString();
            }

            if (string.IsNullOrEmpty(Formation))
            {
                Formation = DefaultFormation;
            }
        }

        #endregion

        #region Debug methods

        [Conditional("DEBUG")]
        public void TraceLoadoutInfo()
        {
            Debug.WriteLine("[AirGroup.Loadout] Class:{0}, AirGroupKey:{1}, Weapons:{2}, Detonator{3}",
                Class, AirGroupKey, string.Join(" ", Weapons.Select(x => x.ToString())), string.Join(" ", Detonator));
        }

        #endregion
    }
}