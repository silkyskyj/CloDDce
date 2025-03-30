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
using System.Text;
using IL2DCE.Generator;
using maddox.game;
using maddox.game.world;
using maddox.GP;

namespace IL2DCE.MissionObjectModel
{
    public class AirGroup
    {
        public const string SquadronFormat = "{0}.{1:D}";
        public const string SquadronValueFormat = "D";
        public const int FlightCount = 4;
        public const string DefaultFormation = "LINEABREAST"; // "Line Abreast" or "Line Astern" is All Flight Group & All Country

        #region Public properties

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

        public string AirGroupKey
        {
            get;
            private set;
        }

        public string VirtualAirGroupKey
        {
            get;
            private set;
        }

        public int SquadronIndex
        {
            get;
            private set;
        }

        public Dictionary<int, IList<string>> Flights
        {
            get;
            private set;
        }

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
            get;
            private set;
        }

        public int Fuel
        {
            get;
            private set;
        }

        public int[] Weapons
        {
            get;
            set;
        }

        public List<string> Detonator
        {
            get;
            set;
        }

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
            get;
            set;
        }

        public double Speed
        {
            // TODO: Use aicraft info to determine speed
            get;
            private set;
        }

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

        public bool SpawnFromScript
        {
            get;
            private set;
        }

        public string Briefing
        {
            get;
            private set;
        }

        public AirGroupInfo AirGroupInfo
        {
            get;
            private set;
        }

        public List<AirGroupWaypoint> Waypoints
        {
            get;
            private set;
        }

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
            get;
            private set;
        }

        public string DisplayName
        {
            get
            {
                string airGroupKey = string.IsNullOrEmpty(VirtualAirGroupKey) ? CreateDisplayName(AirGroupKey) : VirtualAirGroupKey;
                return string.Format(CultureInfo.InvariantCulture.NumberFormat, SquadronFormat, airGroupKey, SquadronIndex);
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

        public bool MissionAssigned
        {
            get
            {
                return MissionType != null && MissionType.HasValue;
            }
        }

        public EMissionType? MissionType
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
            int val;
            if (!int.TryParse(id.Substring(id.LastIndexOf(".") + 1, 1), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out val))
            {
                Debug.Assert(false);
                throw new FormatException(string.Format("Invalid AirGroup ID[{0}]", id));
            }
            SquadronIndex = val;

            // KeyVirtualAirGroupKey
            if (sectionFile.exist(id, MissionFile.KeyVirtualAirGroupKey))
            {
                VirtualAirGroupKey = sectionFile.get(id, MissionFile.KeyVirtualAirGroupKey);
            }

            // Flight
            Flights = new Dictionary<int, IList<string>>();
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
            int.TryParse(sectionFile.get(id, MissionFile.KeyCallSign, "0"), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out val);
            CallSign = val;

            // Fuel
            int.TryParse(sectionFile.get(id, MissionFile.KeyFuel, "100"), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out val);
            Fuel = val;

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
            Detonator = new List<string>();
            int lines = sectionFile.lines(id);
            for (int i = 0; i < lines; i++)
            {
                string key;
                string value;
                sectionFile.get(id, i, out key, out value);
                if (key == MissionFile.KeyDetonator)
                {
                    Detonator.Add(value);
                }
            }

            // Belt
            // TODO: Parse belt

            // Skill
            Skill = sectionFile.get(id, MissionFile.KeySkill, string.Empty);
            Skills = ReadFligthTypeValue(sectionFile, id, Flights, MissionFile.KeySkill);
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

            // MarkingsOn
            MarkingsOn = ReadFligthTypeValue(sectionFile, id, Flights, MissionFile.KeyMarkingsOn);

            // BandColor
            BandColor = ReadFligthTypeValue(sectionFile, id, Flights, MissionFile.KeyBandColor);

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
            Waypoints = new List<AirGroupWaypoint>();
            lines = sectionFile.lines(string.Format("{0}_{1}", Id, MissionFile.SectionWay));
            for (int i = 0; i < lines; i++)
            {
                AirGroupWaypoint waypoint = AirGroupWaypoint.Create(sectionFile, Id, i);
                if (waypoint != null)
                {
                    Waypoints.Add(waypoint);
                }
            }
            if (Waypoints.Count > 0)
            {
                AirGroupWaypoint way = Waypoints.First();
                Position = new Point3d(way.X, way.Y, way.Z);
                if (way.Type == AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF)
                {
                    Airstart = false;
                    Speed = AirGroupWaypoint.DefaultTakeoffV;
                }
                else if (way.Type != AirGroupWaypoint.AirGroupWaypointTypes.LANDING)
                {
                    Airstart = true;
                    Speed = way.V;
                }
            }


            // SetOnPark
            SetOnParked = string.Compare(sectionFile.get(id, MissionFile.KeySetOnPark, "0"), "1") == 0;

            // SpawnFromScript
            SpawnFromScript = string.Compare(sectionFile.get(id, MissionFile.KeySpawnFromScript, "0"), "1") == 0;

            Optimize();
        }

        public AirGroup(string id, AircraftInfo aircraftInfo, Point3d point, AircraftLoadoutInfo aircraftLoadoutInfo)
        {
            // AirGroupKey
            AirGroupKey = id.Substring(0, id.IndexOf("."));

            // SquadronIndex
            int val;
            if (!int.TryParse(id.Substring(id.LastIndexOf(".") + 1, 1), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out val))
            {
                Debug.Assert(false);
                throw new FormatException(string.Format("Invalid AirGroup ID[{0}]", id));
            }
            SquadronIndex = val;

            // KeyVirtualAirGroupKey

            // Flight
            Flights = new Dictionary<int, IList<string>>();
            Flights.Add(0, new string[] { "1" }.ToList());

            // Class
            Class = aircraftInfo.Aircraft;
           
            // Formation
            Formation = DefaultFormation;

            // CallSign
            CallSign = 0;

            // Fuel
            Fuel = 100;

            // Weapons
            Weapons = aircraftLoadoutInfo.Weapons;

            // Detonator
            Detonator = aircraftLoadoutInfo.Detonator;

            // Belt
            // TODO: Parse belt

            // Skill
            Skill = MissionObjectModel.Skill.GetDefaultTyped().ToString();
            Skills = new Dictionary<int, string>();
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
            // Skin
            // MarkingsOn
            // BandColor

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

            // Postion 
            Position = point;
            
            // Speed
            Speed = AirGroupWaypoint.DefaultNormaflyV;

            // Waypoints            
            Waypoints = new List<AirGroupWaypoint>();
            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, point, Speed));
            Airstart = true;

            // SetOnPark
            // SpawnFromScript

            Optimize();
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return CreateSquadronString(AirGroupKey, SquadronIndex);
        }

        public static string CreateSquadronString(string airGroupKey, int squadronIndex)
        {
            return string.Format(CultureInfo.InvariantCulture.NumberFormat, SquadronFormat, airGroupKey, squadronIndex);
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
            if (Waypoints.Count > 0)
            {
                // Section AirGroup
                SilkySkyCloDFile.Write(sectionFile, MissionFile.SectionAirGroups, Id, string.Empty, true);

                // VirtualAirGroupKey
                if (!String.IsNullOrEmpty(VirtualAirGroupKey))
                {
                    SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeyVirtualAirGroupKey, VirtualAirGroupKey, true);
                }

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
                        SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeyFlight + flightIndex, acNumberLine.TrimEnd(), true);
                    }
                }

                SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeyClass, Class, true);
                SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeyFormation, Formation, true);
                SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeyCallSign, CallSign.ToString(CultureInfo.InvariantCulture.NumberFormat), true);
                SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeyFuel, Fuel.ToString(CultureInfo.InvariantCulture.NumberFormat), true);

                // Weapons
                if (Weapons != null && Weapons.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (int weapon in Weapons)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, "{0} ", weapon);
                    }
                    SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeyWeapons, sb.ToString().TrimEnd(), true);
                }

                if (Detonator != null && Detonator.Count > 0)
                {
                    foreach (string detonator in Detonator)
                    {
                        SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeyDetonator, detonator, true);
                    }
                }

                // Update FlightCount & FlightSize 
                UpdateFlightTypeValue();

                // Skill
                if (Skills != null && Skills.Count > 0)
                {
                    //// TODO: Multi Skill(=Different)
                    WriteToFlightTypeValue(sectionFile, Id, Skills, MissionFile.KeySkill);
                }
                else if (!string.IsNullOrEmpty(Skill))
                {
                    SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeySkill, Skill, true);
                }
                else
                {
                    Debug.Assert(false);
                }

                // Aging
                if (!string.IsNullOrEmpty(Aging))
                {
                    SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeyAging, Aging, true);
                }

                // Skin
                if (Skin != null && Skin.Count > 0)
                {
                    WriteToFlightTypeValue(sectionFile, Id, Skin, MissionFile.KeySkin);
                }

                // MarkingsOn 
                if (MarkingsOn != null && MarkingsOn.Count > 0)
                {
                    WriteToFlightTypeValue(sectionFile, Id, MarkingsOn, MissionFile.KeyMarkingsOn);
                }

                // BandColor
                if (BandColor != null && BandColor.Count > 0)
                {
                    WriteToFlightTypeValue(sectionFile, Id, BandColor, MissionFile.KeyBandColor);
                }

                // Spawn (SetOnPark/Idle/Scramble/SpawnFromScript)
                if (Spawn != null)
                {
                    switch (Spawn.Type)
                    {
                        case ESpawn.Parked:
                            SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeySetOnPark, "1", true);
                            break;
                        case ESpawn.Idle:
                            SilkySkyCloDFile.Write(sectionFile, Id, "Idle", "1", true);
                            break;
                        case ESpawn.Scramble:
                            SilkySkyCloDFile.Write(sectionFile, Id, "Scramble", "1", true);
                            break;
                            //case ESpawn.AirStart:
                            //default:
                            //    sectionFile.add(Id, "SetOnPark", "0");
                            //    sectionFile.add(Id, "Idle", "0");
                            //    sectionFile.add(Id, "Scramble", "0");
                            //    break;
                    }

                    if (Spawn.Time.IsDelay)
                    {
                        string action = string.Format("Spawn_{0}", Id);
                        SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeySpawnFromScript, "1", true);
                        SilkySkyCloDFile.Write(sectionFile, MissionFile.SectionTrigger, action, string.Format("{0} {1}", MissionFile.ValueTTime, Spawn.Time.Value.ToString(CultureInfo.InvariantCulture.NumberFormat)), true);
                        SilkySkyCloDFile.Write(sectionFile, MissionFile.SectionAction, action, string.Format("{0} {1} {2}", MissionFile.ValueASpawnGroup, "1", Id), true);
                    }
                }
                // else
                {
                    if (SetOnParked)
                    {
                        SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeySetOnPark, "1", true);
                    }
                }

                // Waypoint
                foreach (AirGroupWaypoint waypoint in Waypoints)
                {
                    SilkySkyCloDFile.Write(sectionFile,
                                    string.Format("{0}_{1}", Id, MissionFile.SectionWay),
                                    waypoint.Type.ToString(),
                                    string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:F2} {1:F2} {2:F2} {3:F2} {4}", 
                                                    waypoint.X, waypoint.Y, waypoint.Z, waypoint.V, waypoint.Target ?? string.Empty));
                }

                // Briefing
                SilkySkyCloDFile.Write(sectionFile, Id, MissionFile.KeyBriefing, this.Id, true);
            }
        }

        #region Operation

        public void Transfer(double altitude, AiAirport landingAirport = null)
        {
            reset();
            Altitude = altitude;

            createStartWaypoints();

            Point3d target = new Point3d(Position.x, Position.y, altitude);
            createEndInbetweenPoints(target, landingAirport, true);
            createEndWaypoints(landingAirport);

            MissionType = EMissionType.TRANSFER;
        }

        public void Cover(AirGroup offensiveAirGroup, double altitude, AiAirport landingAirport = null)
        {
            reset();
            Altitude = altitude;
            TargetAirGroup = offensiveAirGroup;  // Enemy AirGroup

            Point3d? position = null;
            GroundGroup groundGroup = offensiveAirGroup.TargetGroundGroup; // Friendly GroundGroup
            if (groundGroup != null)
            {
                TargetGroundGroup = groundGroup;
                if (groundGroup.Waypoints.Count > 0)
                {
                    GroundGroupWaypoint way = groundGroup.Waypoints.First();
                    position = new Point3d(way.Position.x, way.Position.y, altitude);
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

                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x, position.Value.y, altitude, AirGroupWaypoint.DefaultFlyV));

                AirGroupWaypoint start = Waypoints.Last();

                while (distanceBetween(start, Waypoints.Last()) < 200000.0)
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x + 5000.0, position.Value.y + 5000.0, altitude, AirGroupWaypoint.DefaultFlyV));
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x + 5000.0, position.Value.y - 5000.0, altitude, AirGroupWaypoint.DefaultFlyV));
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x - 5000.0, position.Value.y - 5000.0, altitude, AirGroupWaypoint.DefaultFlyV));
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x - 5000.0, position.Value.y + 5000.0, altitude, AirGroupWaypoint.DefaultFlyV));
                }

                createEndInbetweenPoints(position.Value, landingAirport);
                createEndWaypoints(landingAirport);
            }

            MissionType = EMissionType.COVER;
        }

        public void Hunting(Point2d targetArea, double altitude, AiAirport landingAirport = null)
        {
            reset();
            Altitude = altitude;
            TargetArea = targetArea;

            createStartWaypoints();

            Point3d p = new Point3d(targetArea.x, targetArea.y, altitude);
            createStartInbetweenPoints(p, true);

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.HUNTING, targetArea.x, targetArea.y, altitude, AirGroupWaypoint.DefaultFlyV));

            createEndInbetweenPoints(p, landingAirport, true);
            createEndWaypoints(landingAirport);
            MissionType = EMissionType.HUNTING;
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
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, AirGroupWaypoint.DefaultNormaflyV));
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createInbetweenWaypoints(rendevouzPosition.Value, pStart);
            }
            else
            {
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createStartInbetweenPoints(pStart);
            }

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG, targetStationary.X, targetStationary.Y, altitude, AirGroupWaypoint.DefaultFlyV, targetStationary.Id));

            Point3d pEnd = new Point3d(targetStationary.X, targetStationary.Y, altitude);
            createEndInbetweenPoints(pEnd, landingAirport);
            createEndWaypoints(landingAirport);

            MissionType = EMissionType.ATTACK_ARTILLERY;
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
                Point2d point2d = targetWaypoints.First().Position;
                if (rendevouzPosition != null && rendevouzPosition.HasValue)
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, AirGroupWaypoint.DefaultNormaflyV));
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
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG, point2d.x, point2d.y, altitude, AirGroupWaypoint.DefaultFlyV, targetGroundGroup.Id + " " + targetWaypoints.IndexOf(groundGroupWaypoint)));
                    if (start == null)
                    {
                        start = Waypoints.Last();
                    }
                    else
                    {
                        if (distanceBetween(start, Waypoints.Last()) > 20000.0)
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

            MissionType = EMissionType.ATTACK_ARMOR;
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
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, AirGroupWaypoint.DefaultNormaflyV));
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createInbetweenWaypoints(rendevouzPosition.Value, pStart, true);
            }
            else
            {
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createStartInbetweenPoints(pStart, true);
            }

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.RECON, targetStationary.X, targetStationary.Y, altitude, AirGroupWaypoint.DefaultFlyV, targetStationary.Id));

            Point3d pEnd = new Point3d(targetStationary.X, targetStationary.Y, altitude);
            createEndInbetweenPoints(pEnd, landingAirport, true);
            createEndWaypoints(landingAirport);

            MissionType = EMissionType.RECON;
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
                GroundGroupWaypoint way = targetWaypoints.First();
                createStartWaypoints();

                if (rendevouzPosition != null && rendevouzPosition.HasValue)
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, AirGroupWaypoint.DefaultNormaflyV));
                    Point3d pStart = new Point3d(way.Position.x, way.Position.y, altitude);
                    createInbetweenWaypoints(rendevouzPosition.Value, pStart, true);
                }
                else
                {
                    Point3d pStart = new Point3d(way.Position.x, way.Position.y, altitude);
                    createStartInbetweenPoints(pStart, true);
                }

                GroundGroupWaypoint lastGroundGroupWaypoint = null;
                AirGroupWaypoint start = null;
                foreach (GroundGroupWaypoint groundGroupWaypoint in targetWaypoints)
                {
                    lastGroundGroupWaypoint = groundGroupWaypoint;
                    Point2d point2d = groundGroupWaypoint.Position;
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.RECON, point2d.x, point2d.y, altitude, AirGroupWaypoint.DefaultFlyV, targetGroundGroup.Id + " " + targetWaypoints.IndexOf(groundGroupWaypoint)));
                    if (start == null)
                    {
                        start = Waypoints.Last();
                    }
                    else
                    {
                        if (distanceBetween(start, Waypoints.Last()) > 20000.0)
                        {
                            break;
                        }
                    }
                }

                if (lastGroundGroupWaypoint != null)
                {
                    Point3d pEnd = new Point3d(lastGroundGroupWaypoint.Position.x, lastGroundGroupWaypoint.Position.y, altitude);
                    createEndInbetweenPoints(pEnd, landingAirport, true);
                }

                createEndWaypoints(landingAirport);

                MissionType = EMissionType.RECON;
            }
        }

        public void Escort(AirGroup targetAirGroup, AiAirport landingAirport = null)
        {
            reset();
            Altitude = targetAirGroup.Altitude;
            TargetAirGroup = targetAirGroup;    // Friendly AirGroup
            IList<AirGroupWaypoint> targetWaypoints = targetAirGroup.Waypoints;

            createStartWaypoints();

            foreach (AirGroupWaypoint waypoint in targetWaypoints)
            {
                if (waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF && waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.LANDING)
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.ESCORT, waypoint.X, waypoint.Y, waypoint.Z, AirGroupWaypoint.DefaultFlyV, 
                                                        targetAirGroup.Id + " " + targetWaypoints.IndexOf(waypoint)));
                }
            }

            createEndWaypoints(landingAirport);
            MissionType = EMissionType.ESCORT;
        }

        public void Follow(AirGroup targetAirGroup, AiAirport landingAirport = null)
        {
            reset();
            Altitude = targetAirGroup.Altitude;     // Friendly AirGroup
            TargetAirGroup = targetAirGroup;
            IList<AirGroupWaypoint> targetWaypoints = targetAirGroup.Waypoints;

            createStartWaypoints();

            foreach (AirGroupWaypoint waypoint in targetWaypoints)
            {
                if (waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF && waypoint.Type != AirGroupWaypoint.AirGroupWaypointTypes.LANDING)
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.FOLLOW, waypoint.X, waypoint.Y, waypoint.Z, AirGroupWaypoint.DefaultFlyV, 
                                                        targetAirGroup.Id + " " + targetWaypoints.IndexOf(waypoint)));
                }
            }

            createEndWaypoints(landingAirport);
            MissionType = EMissionType.FOLLOW;
        }

        public void Intercept(AirGroup targetAirGroup, AiAirport landingAirport = null)
        {
            reset();
            Altitude = targetAirGroup.Altitude;   // Enemy AirGroup
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
                createStartInbetweenPoints(interceptWaypoint.Position, true);
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, interceptWaypoint.X, interceptWaypoint.Y, interceptWaypoint.Z, AirGroupWaypoint.DefaultFlyV, targetAirGroup.Id + " " + targetWaypoints.IndexOf(interceptWaypoint)));

                if (targetWaypoints.IndexOf(interceptWaypoint) - 1 >= 0)
                {
                    AirGroupWaypoint nextInterceptWaypoint = targetWaypoints[targetWaypoints.IndexOf(interceptWaypoint) - 1];
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, nextInterceptWaypoint.X, nextInterceptWaypoint.Y, nextInterceptWaypoint.Z, AirGroupWaypoint.DefaultFlyV, targetAirGroup.Id + " " + targetWaypoints.IndexOf(nextInterceptWaypoint)));

                    createEndInbetweenPoints(nextInterceptWaypoint.Position, landingAirport, true);
                }
                else
                {
                    createEndInbetweenPoints(interceptWaypoint.Position, landingAirport, true);
                }
            }
            else if (closestInterceptWaypoint != null)
            {
                createStartInbetweenPoints(closestInterceptWaypoint.Position, true);
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, closestInterceptWaypoint.X, closestInterceptWaypoint.Y, closestInterceptWaypoint.Z, AirGroupWaypoint.DefaultFlyV, targetAirGroup.Id + " " + targetWaypoints.IndexOf(closestInterceptWaypoint)));

                if (targetWaypoints.IndexOf(closestInterceptWaypoint) + 1 < targetWaypoints.Count)
                {
                    AirGroupWaypoint nextInterceptWaypoint = targetWaypoints[targetWaypoints.IndexOf(interceptWaypoint) + 1];
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, nextInterceptWaypoint.X, nextInterceptWaypoint.Y, nextInterceptWaypoint.Z, AirGroupWaypoint.DefaultFlyV, targetAirGroup.Id + " " + targetWaypoints.IndexOf(nextInterceptWaypoint)));

                    createEndInbetweenPoints(nextInterceptWaypoint.Position, landingAirport, true);
                }
                else
                {
                    createEndInbetweenPoints(closestInterceptWaypoint.Position, landingAirport, true);
                }
            }

            createEndWaypoints(landingAirport);
            MissionType = EMissionType.INTERCEPT;
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
                        type = Spawn.CreateRandomSpawnType();
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
                    Position = new Point3d(wayNew.X, wayNew.Y, wayNew.Z);
                    Speed = wayNew.V;

                    // TODO: Update after Waypoint Z(Altitude) value
                }
            }
        }

        public void SetSpeed(int speed)
        {
            if (speed != -1)
            {
                Speed = speed;
                AirGroupWaypoint way = Waypoints.FirstOrDefault();
                if (way != null && way.Type != AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF && way.Type != AirGroupWaypoint.AirGroupWaypointTypes.LANDING)
                {
                    way.V = speed;
                }
            }
        }

        public void SetFuel(int fuel)
        {
            if (fuel != -1)
            {
                Fuel = fuel;
            }
        }

        public void SetFlights(int flightCount, int flightSize)
        {
            Flights.Clear();
            int aircraftNumber = 1;
            for (int i = 0; i < flightCount; i++)
            {
                List<string> aircraftNumbers = new List<string>();
                for (int j = 0; j < flightSize; j++)
                {
                    aircraftNumbers.Add(aircraftNumber.ToString(CultureInfo.InvariantCulture.NumberFormat));
                    aircraftNumber++;
                }
                Flights[i] = aircraftNumbers;
            }
        }

        public void SetFormation(EFormation formation)
        {
            if (formation == EFormation.Random)
            {
                Formations formations = Formations.Default[AirGroupInfo.FormationsType];
                formation = formations.GetRandom();
            }
            if (formation != EFormation.Default)
            {
                Formation = formation.ToString();
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
                Skill = MissionObjectModel.Skill.GetDefaultTyped().ToString();
            }

            if (string.IsNullOrEmpty(Formation))
            {
                Formation = DefaultFormation;
            }
        }

        public void UpdateStartPoint(ref Point3d point, AirGroupWaypoint.AirGroupWaypointTypes? type = null)
        {
            AirGroupWaypoint way = Waypoints.FirstOrDefault();
            if (way != null)
            {
                way.X = point.x;
                way.Y = point.y;
                way.Z = point.z;
                Position = point;
                Altitude = point.z;
                if (type != null)
                {
                    way.Type = type.Value;
                    if (way.Type == AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF || way.Type == AirGroupWaypoint.AirGroupWaypointTypes.LANDING)
                    {
                        Airstart = false;
                        Speed = way.V = AirGroupWaypoint.DefaultTakeoffV;
                    }
#if false
                    else if (speed == AirGroupWaypoint.DefaultTakeoffV)
                    {
                        Airstart = true;
                        speed = way.V = AirGroupWaypoint.DefaultNormaflyV;
                    }
#endif
                    else
                    {
                        Airstart = true;
                        if (Speed == AirGroupWaypoint.DefaultTakeoffV || way.V == AirGroupWaypoint.DefaultTakeoffV)
                        {
                            Speed = way.V = AirGroupWaypoint.DefaultNormaflyV;
                        }
                    }
                }
            }
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

        private void WriteToFlightTypeValue(ISectionFile sectionFile, string id, IDictionary<int, string> dic, string keyInfo)
        {
            foreach (var item in dic)
            {
                int flight = item.Key / 10;
                string key = string.Format("{0}{1}{2}", keyInfo,
                    (flight > 0 ? flight.ToString(CultureInfo.InvariantCulture.NumberFormat) : string.Empty), (item.Key % 10));
                SilkySkyCloDFile.Write(sectionFile, id, key, item.Value, true);
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
                        // Debug.Assert(false);
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
            if (Skills != null && Skills.Count > 0)
            {
                Skills = UpdateFlightTypeValue(Flights, Skills);
            }

            // Skin 
            if (Skin != null && Skin.Count > 0)
            {
                Skin = UpdateFlightTypeValue(Flights, Skin);
            }

            // MarkingsOn
            if (MarkingsOn != null && MarkingsOn.Count > 0)
            {
                MarkingsOn = UpdateFlightTypeValue(Flights, MarkingsOn);
            }

            // BandColor
            if (BandColor != null && BandColor.Count > 0)
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
            if (Waypoints.Contains(waypoint))
            {
                foreach (AirGroupWaypoint wp in Waypoints)
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
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF, Position, AirGroupWaypoint.DefaultTakeoffV));
            }
            else
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, Position, Speed));
            }
        }

        private void createEndWaypoints(AiAirport landingAirport = null)
        {
            if (landingAirport != null)
            {
                Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.LANDING, landingAirport.Pos(), AirGroupWaypoint.DefaultLandingV));
            }
            else
            {
                if (!Airstart)
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.LANDING, Position, AirGroupWaypoint.DefaultLandingV));
                }
                else
                {
                    Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, Position, Speed));
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
                mpX *= Random.Default.Next(75, 125) / 100.0;
                mpY *= Random.Default.Next(75, 125) / 100.0;
            }
            Point3d p1 = new Point3d(from.x + 0.25 * mpX, from.y + 0.25 * mpY, from.z + 1.00 * mpZ);
            Point3d p2 = new Point3d(from.x + 0.50 * mpX, from.y + 0.50 * mpY, from.z + 1.00 * mpZ);
            Point3d p3 = new Point3d(from.x + 0.75 * mpX, from.y + 0.75 * mpY, from.z + 1.00 * mpZ);

            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p1, AirGroupWaypoint.DefaultNormaflyV));
            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p2, AirGroupWaypoint.DefaultNormaflyV));
            Waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p3, AirGroupWaypoint.DefaultNormaflyV));
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
            Waypoints.Clear();

            Altitude = null;
            EscortAirGroup = null;
            TargetAirGroup = null;
            TargetGroundGroup = null;
            TargetStationary = null;
            TargetArea = null;
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