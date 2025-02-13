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
using System.Globalization;
using System.Linq;
using maddox.game;
using maddox.game.world;
using maddox.GP;

namespace IL2DCE
{
    public class AirGroup
    {
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

        public string Skill
        {
            get;
            set;
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
                return 300.0;
            }
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

        //public Point2d? TargetArea
        //{
        //    get
        //    {
        //        return _targetArea;
        //    }
        //    set
        //    {
        //        _targetArea = value;
        //    }
        //}
        //private Point2d? _targetArea = null;

        public string DisplayName
        {
            get
            {
                return CreateDisplayName(AirGroupKey) + "." + SquadronIndex;
            }
        }

        #endregion

        #region private member

        AirGroupInfos airGroupInfos;

        #endregion

        #region Public constructors

        public AirGroup(ISectionFile sectionFile, string id, AirGroupInfos airGroupInfos = null)
        {
            this.airGroupInfos = airGroupInfos;
            // airGroupId = <airGroupKey>.<squadronIndex><flightMask>

            // AirGroupKey
            AirGroupKey = id.Substring(0, id.IndexOf("."));

            // SquadronIndex
            int.TryParse(id.Substring(id.LastIndexOf(".") + 1, 1), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _squadronIndex);

            // Flight
            for (int i = 0; i < 4; i++)
            {
                if (sectionFile.exist(id, "Flight" + i.ToString(CultureInfo.InvariantCulture.NumberFormat)))
                {
                    string acNumberLine = sectionFile.get(id, "Flight" + i.ToString(CultureInfo.InvariantCulture.NumberFormat));
                    string[] acNumberList = acNumberLine.Split(new char[] { ' ' });
                    if (acNumberList != null && acNumberList.Length > 0)
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
            Class = sectionFile.get(id, "Class");

            // Formation
            Formation = sectionFile.get(id, "Formation");

            // CallSign
            int.TryParse(sectionFile.get(id, "CallSign"), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _callSign);

            // Fuel
            int.TryParse(sectionFile.get(id, "Fuel"), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out _fuel);

            // Weapons
            string weaponsLine = sectionFile.get(id, "Weapons");
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
                if (key == "Detonator")
                {
                    _detonator.Add(value);
                }
            }

            // Belt
            // TODO: Parse belt

            // Skill
            Skill = sectionFile.get(id, "Skill");

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
            Id = AirGroupKey + "." + SquadronIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) + flightMask.ToString("X");

            // Waypoints            
            for (int i = 0; i < sectionFile.lines(Id + "_Way"); i++)
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

            // AirGroupInfo
            // ArmyIndex
            if ((AirGroupInfo = GetAirGroupInfo(1, AirGroupKey)) != null)
            {
                ArmyIndex = 1;
            }
            else if ((AirGroupInfo = GetAirGroupInfo(2, AirGroupKey)) != null)
            {
                ArmyIndex = 2;
            }
            else
            {
                AirGroupInfo = null;
                ArmyIndex = 0;
            }
        }

        #endregion

        #region Private methods

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
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.TAKEOFF, Position, 0.0));
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
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.LANDING, landingAirport.Pos(), 0.0));
            }
            else
            {
                if (!Airstart)
                {
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.LANDING, Position, 0.0));
                }
                else
                {
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, Position, Speed));
                }
            }
        }

        private void createInbetweenWaypoints(Point3d a, Point3d b)
        {
            Point3d p1 = new Point3d(a.x + 0.25 * (b.x - a.x), a.y + 0.25 * (b.y - a.y), a.z + 1.00 * (b.z - a.z));
            Point3d p2 = new Point3d(a.x + 0.50 * (b.x - a.x), a.y + 0.50 * (b.y - a.y), a.z + 1.00 * (b.z - a.z));
            Point3d p3 = new Point3d(a.x + 0.75 * (b.x - a.x), a.y + 0.75 * (b.y - a.y), a.z + 1.00 * (b.z - a.z));

            _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p1, 300.0));
            _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p2, 300.0));
            _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, p3, 300.0));
        }

        private void createStartInbetweenPoints(Point3d target)
        {
            createInbetweenWaypoints(Position, target);
        }

        private void createEndInbetweenPoints(Point3d target, AiAirport landingAirport = null)
        {
            if (landingAirport != null)
            {
                Point3d point = new Point3d(landingAirport.Pos().x, landingAirport.Pos().y, target.z);
                createInbetweenWaypoints(target, point);
            }
            else
            {
                Point3d point = new Point3d(Position.x, Position.y, target.z);
                createInbetweenWaypoints(target, point);
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
            //this.TargetArea = null;
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

        #endregion

        #region Public methods

        public void WriteTo(ISectionFile sectionFile, Config config)
        {
            if (_waypoints.Count > 0)
            {
                sectionFile.add("AirGroups", Id, "");

                foreach (int flightIndex in Flights.Keys)
                {
                    if (Flights[flightIndex].Count > 0)
                    {
                        string acNumberLine = "";
                        foreach (string acNumber in Flights[flightIndex])
                        {
                            acNumberLine += acNumber + " ";
                        }
                        sectionFile.add(Id, "Flight" + flightIndex, acNumberLine.TrimEnd());
                    }
                }

                sectionFile.add(Id, "Class", Class);
                sectionFile.add(Id, "Formation", Formation);
                sectionFile.add(Id, "CallSign", CallSign.ToString(CultureInfo.InvariantCulture.NumberFormat));
                sectionFile.add(Id, "Fuel", Fuel.ToString(CultureInfo.InvariantCulture.NumberFormat));

                if (Weapons != null && Weapons.Length > 0)
                {
                    string weaponsLine = "";
                    foreach (int weapon in Weapons)
                    {
                        weaponsLine += weapon.ToString(CultureInfo.InvariantCulture.NumberFormat) + " ";
                    }
                    sectionFile.add(Id, "Weapons", weaponsLine.TrimEnd());
                }

                if (Detonator != null && Detonator.Count > 0)
                {
                    foreach (string detonator in Detonator)
                    {
                        sectionFile.add(Id, "Detonator", detonator);
                    }
                }

                if (config.SpawnParked == true)
                {
                    sectionFile.add(Id, "SetOnPark", "1");
                }
                else
                {
                    sectionFile.add(Id, "SetOnPark", "0");
                }

                sectionFile.add(Id, "Skill", Skill);

                foreach (AirGroupWaypoint waypoint in _waypoints)
                {
                    if (waypoint.Target == null)
                    {
                        sectionFile.add(Id + "_Way", waypoint.Type.ToString(), waypoint.X.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Y.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Z.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.V.ToString(CultureInfo.InvariantCulture.NumberFormat));
                    }
                    else
                    {
                        sectionFile.add(Id + "_Way", waypoint.Type.ToString(), waypoint.X.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Y.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Z.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.V.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + waypoint.Target);
                    }
                }

                sectionFile.add(Id, "Briefing", this.Id);
            }
        }

        public override string ToString()
        {
            return AirGroupKey + "." + SquadronIndex;
        }

        //public void Transfer(double altitude, AiAirport landingAirport = null)
        //{
        //    this.reset();
        //    this.Altitude = altitude;

        //    createStart_waypoints();

        //    Point3d target = new Point3d(Position.x, Position.y, altitude);
        //    createEndInbetweenPoints(target, landingAirport);

        //    createEndWaypoints(landingAirport);
        //}

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

                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x, position.Value.y, altitude, 300.0));

                AirGroupWaypoint start = _waypoints[_waypoints.Count - 1];

                while (distanceBetween(start, _waypoints[_waypoints.Count - 1]) < 200000.0)
                {
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x + 5000.0, position.Value.y + 5000.0, altitude, 300.0));
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x + 5000.0, position.Value.y - 5000.0, altitude, 300.0));
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x - 5000.0, position.Value.y - 5000.0, altitude, 300.0));
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.COVER, position.Value.x - 5000.0, position.Value.y + 5000.0, altitude, 300.0));
                }

                createEndInbetweenPoints(position.Value, landingAirport);

                createEndWaypoints(landingAirport);
            }
        }

        //public void Hunting(Point2d targetArea, double altitude, AiAirport landingAirport = null)
        //{
        //    this.reset();
        //    this.Altitude = altitude;
        //    this.TargetArea = targetArea;

        //    createStartWaypoints();

        //    Point3d p = new Point3d(targetArea.x, targetArea.y, altitude);
        //    createStartInbetweenPoints(p);

        //    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.HUNTING, targetArea.x, targetArea.y, altitude, 300.0));

        //    createEndInbetweenPoints(p, landingAirport);
        //    createEndWaypoints(landingAirport);
        //}

        public void GroundAttack(Stationary targetStationary, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            this.reset();
            this.Altitude = altitude;
            this.TargetStationary = targetStationary;
            this.EscortAirGroup = escortAirGroup;

            Point3d? rendevouzPosition = null;
            if (EscortAirGroup != null)
            {
                rendevouzPosition = calculateRendevouzPoint(EscortAirGroup);
            }

            createStartWaypoints();

            if (rendevouzPosition != null && rendevouzPosition.HasValue)
            {
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, 300.0));
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createInbetweenWaypoints(rendevouzPosition.Value, pStart);
            }
            else
            {
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createStartInbetweenPoints(pStart);
            }

            _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG, targetStationary.X, targetStationary.Y, altitude, 300.0, targetStationary.Id));

            Point3d pEnd = new Point3d(targetStationary.X, targetStationary.Y, altitude);
            createEndInbetweenPoints(pEnd, landingAirport);

            createEndWaypoints(landingAirport);
        }

        public void GroundAttack(GroundGroup targetGroundGroup, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            this.reset();
            this.Altitude = altitude;
            this.TargetGroundGroup = targetGroundGroup;
            this.EscortAirGroup = escortAirGroup;

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
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, 300.0));
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
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.GATTACK_TARG, point2d.x, point2d.y, altitude, 300.0, targetGroundGroup.Id + " " + targetWaypoints.IndexOf(groundGroupWaypoint)));
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
            this.reset();
            this.Altitude = altitude;
            this.TargetStationary = targetStationary;
            this.EscortAirGroup = escortAirGroup;

            Point3d? rendevouzPosition = null;
            if (EscortAirGroup != null)
            {
                rendevouzPosition = calculateRendevouzPoint(EscortAirGroup);
            }

            createStartWaypoints();

            if (rendevouzPosition != null && rendevouzPosition.HasValue)
            {
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, 300.0));
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createInbetweenWaypoints(rendevouzPosition.Value, pStart);
            }
            else
            {
                Point3d pStart = new Point3d(targetStationary.X, targetStationary.Y, altitude);
                createStartInbetweenPoints(pStart);
            }

            _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.RECON, targetStationary.X, targetStationary.Y, altitude, 300.0, targetStationary.Id));

            Point3d pEnd = new Point3d(targetStationary.X, targetStationary.Y, altitude);
            createEndInbetweenPoints(pEnd, landingAirport);

            createEndWaypoints(landingAirport);
        }

        public void Recon(GroundGroup targetGroundGroup, double altitude, AirGroup escortAirGroup = null, AiAirport landingAirport = null)
        {
            this.reset();
            this.Altitude = altitude;
            this.TargetGroundGroup = targetGroundGroup;
            this.EscortAirGroup = escortAirGroup;

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
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY, rendevouzPosition.Value, 300.0));
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
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.RECON, point2d.x, point2d.y, altitude, 300.0, targetGroundGroup.Id + " " + targetWaypoints.IndexOf(groundGroupWaypoint)));
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
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.ESCORT, waypoint.X, waypoint.Y, waypoint.Z, 300.0, targetAirGroup.Id + " " + targetWaypoints.IndexOf(waypoint)));
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
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, interceptWaypoint.X, interceptWaypoint.Y, interceptWaypoint.Z, 300.0, targetAirGroup.Id + " " + targetWaypoints.IndexOf(interceptWaypoint)));

                if (targetWaypoints.IndexOf(interceptWaypoint) - 1 >= 0)
                {
                    AirGroupWaypoint nextInterceptWaypoint = targetWaypoints[targetWaypoints.IndexOf(interceptWaypoint) - 1];
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, nextInterceptWaypoint.X, nextInterceptWaypoint.Y, nextInterceptWaypoint.Z, 300.0, targetAirGroup.Id + " " + targetWaypoints.IndexOf(nextInterceptWaypoint)));

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
                _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, closestInterceptWaypoint.X, closestInterceptWaypoint.Y, closestInterceptWaypoint.Z, 300.0, targetAirGroup.Id + " " + targetWaypoints.IndexOf(closestInterceptWaypoint)));


                if (targetWaypoints.IndexOf(closestInterceptWaypoint) + 1 < targetWaypoints.Count)
                {
                    AirGroupWaypoint nextInterceptWaypoint = targetWaypoints[targetWaypoints.IndexOf(interceptWaypoint) + 1];
                    _waypoints.Add(new AirGroupWaypoint(AirGroupWaypoint.AirGroupWaypointTypes.AATTACK_BOMBERS, nextInterceptWaypoint.X, nextInterceptWaypoint.Y, nextInterceptWaypoint.Z, 300.0, targetAirGroup.Id + " " + targetWaypoints.IndexOf(nextInterceptWaypoint)));

                    createEndInbetweenPoints(nextInterceptWaypoint.Position, landingAirport);
                }
                else
                {
                    createEndInbetweenPoints(closestInterceptWaypoint.Position, landingAirport);
                }
            }

            createEndWaypoints(landingAirport);
        }

        public static string CreateDisplayName(string airGroupKey)
        {
            // tobruk:Tobruk_RA_30St_87_Gruppo_192Sq -> Tobruk_RA_30St_87_Gruppo_192Sq
            const string del = ":";
            int idx = airGroupKey.IndexOf(del, StringComparison.CurrentCultureIgnoreCase);
            return idx != -1 ? airGroupKey.Substring(idx + del.Length): airGroupKey;
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