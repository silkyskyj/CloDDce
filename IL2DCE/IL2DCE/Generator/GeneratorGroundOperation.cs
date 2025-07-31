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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;
using maddox.GP;
using XLAND;
using static IL2DCE.MissionObjectModel.MissionStatus;
using static IL2DCE.MissionObjectModel.Skill;

namespace IL2DCE.Generator
{
    class GeneratorGroundOperation : GeneratorBase
    {
        #region Definition

        private const float MaxSpawnDifDistanceAirportRRateGroundGroup = 10.0f;
        private const float MaxSpawnDifDistanceAirportRRateStationary = 10.0f;
        private const float MaxSpawnDifDistanceAirportRRateStationaryAircraft = 1.0f;
        private const int MaxRetryCreateRandomPoint = 100;
        private const int MaxRetryCreateOneGroundGroup = 25;
        private const int MaxRetryCreateAllGroundGroups = 1000;
        private const int MaxRetryCreateOneStationary = 25;
        private const int MaxRetryCreateAllStationary = 1000;

        #endregion

        private IMissionStatus MissionStatus
        {
            get;
            set;
        }

        private wRECTF RangeBattleArea
        {
            get;
            set;
        }

        public wRECTF Range
        {
            get;
            set;
        }

        public bool HasAvailableGroundGroup
        {
            get
            {
                return AvailableGroundGroups.Count > 0;
            }
        }

        public bool HasAssignedGroundGroup
        {
            get
            {
                return AssigneGroundGroups.Count > 0;
            }
        }

        private bool EnableMissionMultiAssign
        {
            get;
            set;
        }

        private int ArtilleryTimeout
        {
            get;
            set;
        }

        private int ArtilleryRHide
        {
            get;
            set;
        }

        private int ArtilleryZOffset  // hstart
        {
            get;
            set;
        }

        private int ShipSleep
        {
            get;
            set;
        }

        private ESkillSetShip SkillShip
        {
            get;
            set;
        }

        private IEnumerable<ESystemType> SkillsShip
        {
            get;
            set;
        }

        private float ShipSlowfire
        {
            get;
            set;
        }

        private EGroundGroupGenerateType GroundGroupGenerateType
        {
            get;
            set;
        }

        private EStationaryGenerateType StationaryGenerateType
        {
            get;
            set;
        }

        private EArmorUnitNumsSet ArmorUnitNumsSet
        {
            get;
            set;
        }

        private EShipUnitNumsSet ShipUnitNumsSet
        {
            get;
            set;
        }

        private List<GroundGroup> AvailableGroundGroups = new List<GroundGroup>();
        private List<GroundGroup> AssigneGroundGroups = new List<GroundGroup>();
        private List<Stationary> AvailableStationaries = new List<Stationary>();
        private List<GroundGroup> AllGroundGroups = new List<GroundGroup>();
        private List<Stationary> AllStationaries = new List<Stationary>();

        private IEnumerable<Point3d> FrontMarkers;

        public GeneratorGroundOperation(IGamePlay gamePlay, IRandom random, IMissionStatus missionStatus, wRECTF rangeBattleArea, IEnumerable<GroundGroup> groundGroups, IEnumerable<Stationary> stationaries, IEnumerable<Point3d> frontMarkers, bool enableMissionMultiAssign, EGroundGroupGenerateType groundGroupGenerateType, EStationaryGenerateType stationaryGenerateType, EArmorUnitNumsSet armorUnitNumsSet, EShipUnitNumsSet shipUnitNumsSet, int artilleryTimeout = ArtilleryOption.TimeoutMissionDefault, int artilleryRHide = ArtilleryOption.RHideMissionDefault, int artilleryZOffset = ArtilleryOption.ZOffsetMissionDefault, int shipSleep = ShipOption.SleepMissionDefault, ESkillSetShip shipSkil = ESkillSetShip.Random, float shipSlowfire = ShipOption.SlowFireMissionDefault)
            : base(gamePlay, random)
        {
            MissionStatus = missionStatus;
            RangeBattleArea = rangeBattleArea;

            SetGroundObjects(groundGroups, stationaries);
            EnableMissionMultiAssign = enableMissionMultiAssign;

            FrontMarkers = frontMarkers;
            Debug.WriteLine("FrontMarkers");
            int i = 0;
            foreach (var item in frontMarkers)
            {
                Debug.WriteLine("  {0:2}: {1}", i++, item.ToString());
            }

            GroundGroupGenerateType = groundGroupGenerateType;
            StationaryGenerateType = stationaryGenerateType;

            ArmorUnitNumsSet = armorUnitNumsSet;
            ShipUnitNumsSet = shipUnitNumsSet;

            ArtilleryTimeout = artilleryTimeout;
            ArtilleryRHide = artilleryRHide;
            ArtilleryZOffset = artilleryZOffset;

            ShipSleep = shipSleep;
            SkillShip = shipSkil;
            ShipSlowfire = shipSlowfire;

            SkillsShip = CreateSkills(shipSkil);
        }

        public void SetGroundObjects(IEnumerable<GroundGroup> groundGroups, IEnumerable<Stationary> stationaries)
        {
            AvailableGroundGroups.Clear();
            AvailableGroundGroups.AddRange(groundGroups);
            AvailableStationaries.Clear();
            AvailableStationaries.AddRange(stationaries);
            AllGroundGroups.Clear();
            AllGroundGroups.AddRange(groundGroups);
            AllStationaries.Clear();
            AllStationaries.AddRange(stationaries);

            SetRange(AvailableGroundGroups.Select(x => x.Position).Concat(AvailableStationaries.Select(x => x.Position)));
        }

        private void SetRange(IEnumerable<Point2d> points)
        {
            wRECTF range = MapUtil.GetRange(points);
            Debug.WriteLine("SetRange({0},{1})-({2},{3})[{4},{5}]", range.x1, range.y1, range.x2, range.y2, range.x2 - range.x1 + 1, range.y2 - range.y1 + 1);
            Range = range;
        }

        public bool CreateRandomGroundOperation(ISectionFile missionFile, GroundGroup groundGroup, int formationCount = -1)
        {
            bool result = false;

            AvailableGroundGroups.Remove(groundGroup);

            if (!groundGroup.MissionAssigned)
            {

                //if (groundGroup.Type == EGroundGroupType.Ship)
                //{
                //    if (groundGroup is ShipGroup)
                //    {
                //        SetSkill(groundGroup as ShipGroup);
                //    }
                //    // Ships already have the correct waypoint from the mission template. Only remove some waypoints to make the position more random, but leave at least 2 waypoints.
                //    groundGroup.Waypoints.RemoveRange(0, Random.Next(0, groundGroup.Waypoints.Count - 1));
                //    groundGroup.WriteTo(missionFile);
                //    generateColumnFormation(missionFile, groundGroup, formationCount);
                //    result = true;
                //}
                //else 
                if (groundGroup.Type == EGroundGroupType.Train)
                {
                    groundGroup.Waypoints.RemoveRange(0, Random.Next(0, groundGroup.Waypoints.Count - 1));
                    if (groundGroup.Waypoints.Count >= 2)
                    {
                        groundGroup.WriteTo(missionFile);
                        result = true;
                    }
                }
                else
                {     // Vehicle Armor Unknown
                    IEnumerable<Point3d> friendlyMarkers = FrontMarkers.Where(x => x.z == groundGroup.Army);
                    if (friendlyMarkers.Any())
                    {
                        List<Point3d> availableFriendlyMarkers = new List<Point3d>(friendlyMarkers);

                        // Find closest friendly marker
                        Point3d? nearestMarker = GetNearestPoint(groundGroup.Position, availableFriendlyMarkers);
                        if (nearestMarker != null && nearestMarker.HasValue)
                        {
                            if (groundGroup is Armor && (groundGroup.Type == EGroundGroupType.Armor || groundGroup.Type == EGroundGroupType.Vehicle))
                            {
                                Armor armor = groundGroup as Armor;
                                int numUnits = GetArmorNumUnitsCount();
                                if (numUnits != -1)
                                {
                                    armor.SetNumUnits(numUnits);
                                }
                                Point2d start = groundGroup.Position;
                                Point3d? point = CreateRandomPoint(groundGroup.Army, (float)nearestMarker.Value.x, (float)nearestMarker.Value.y, 5000, 0, GroundGroup.GetLandTypes(groundGroup.Type));
                                Point2d end = point != null ? new Point2d(point.Value.x, point.Value.y) : new Point2d(nearestMarker.Value.x, nearestMarker.Value.y);
                                IEnumerable<GroundGroupWaypoint> wayPoints = CreateWaypoints(armor.Type, armor.Army, start, end);
                                if (wayPoints != null && wayPoints.Count() >= 2)
                                {
                                    armor.Waypoints.Clear();
                                    armor.Waypoints.AddRange(wayPoints);
                                }
                                if (armor.Waypoints != null && armor.Waypoints.Count >= 2)
                                {
                                    armor.WriteTo(missionFile);
                                    result = true;
                                }
                            }
                            else
                            {   // Ship & Unknouw
                                Debug.Assert(groundGroup.Type == EGroundGroupType.Ship);
                                if (groundGroup is ShipGroup && groundGroup.Type == EGroundGroupType.Ship)
                                {
                                    SetSkill(groundGroup as ShipGroup);
                                }
                                Point2d start = groundGroup.Position;
#if true
                                Point3d? point = CreateRandomPoint(groundGroup.Army, (float)start.x, (float)start.y, 20000, 0, GroundGroup.GetLandTypes(groundGroup.Type));
                                Point2d end = point != null ? new Point2d(point.Value.x, point.Value.y) : new Point2d(nearestMarker.Value.x, nearestMarker.Value.y);
#else
                                wRECTF rect = RangeBattleArea;
                                float r = Math.Min(rect.x2 - rect.x1, rect.x2 - rect.x1) / 4;
                                Point3d? posEnd = CreateRandomPoint((float)start.x, (float)start.y, r, 0, new LandTypes[] { LandTypes.WATER });
                                end = posEnd != null ? new Point2d(posEnd.Value.x, posEnd.Value.y): end;
#endif
                                IEnumerable<GroundGroupWaypoint> wayPoints = CreateWaypoints(groundGroup.Type, groundGroup.Army, start, end);
                                if (wayPoints != null && wayPoints.Count() >= 2)
                                {
                                    groundGroup.Waypoints.Clear();
                                    groundGroup.Waypoints.AddRange(wayPoints);
                                }
                                if (groundGroup.Waypoints != null && groundGroup.Waypoints.Count >= 2)
                                {
                                    groundGroup.WriteTo(missionFile);
                                    if (formationCount == -1)
                                    {
                                        formationCount = GetFormationCount();
                                    }
                                    generateColumnFormation(missionFile, groundGroup, formationCount);
                                    result = true;
                                }
                            }
                        }
                    }
                }
                if (result)
                {
                    AssigneGroundGroups.Add(groundGroup);
                    groundGroup.MissionAssigned = true;
                }
            }
            return result;
        }

        public void OptimizeMissionObjects(IMissionStatus missionStatus = null)
        {
            if (missionStatus == null)
            {
                missionStatus = MissionStatus;
            }

            for (int i = AvailableGroundGroups.Count - 1; i >= 0; i--)
            {
                GroundGroup groundGroup = AvailableGroundGroups[i];
                GroundGroupObj groundGroupObject = missionStatus.GroundGroups.Where(x => string.Compare(x.Name, groundGroup.Id, true) == 0).FirstOrDefault();
                if (groundGroupObject != null)
                {
                    groundGroup.MissionAssigned = true;
                    AvailableGroundGroups.Remove(groundGroup);
                    AssigneGroundGroups.Add(groundGroup);
                }
            }

            for (int i = AvailableStationaries.Count - 1; i >= 0; i--)
            {
                Stationary stationary = AvailableStationaries[i];
                StationaryObj stationaryObj = missionStatus.Stationaries.Where(x => string.Compare(x.Name, stationary.Id, true) == 0 /*&&
                                                            string.Compare(x.Class, stationary.Class, true) == 0*/).FirstOrDefault();
                GroundObj groundObject = missionStatus.GroundActors.Where(x => string.Compare(x.Name, stationary.Id, true) == 0 /*&&
                                                            string.Compare(x.Class, stationary.Class, true) == 0*/).FirstOrDefault();
                if (stationaryObj != null || groundObject != null)
                {
                    AvailableStationaries.Remove(stationary);
                }
            }
        }

        private Point3d? GetNearestPoint(Point2d point, IEnumerable<Point3d> points)
        {
            Point3d? nearestPoint = null;
            foreach (Point3d pos in points)
            {
                if (nearestPoint == null)
                {
                    nearestPoint = pos;
                }
                else if (nearestPoint.HasValue)
                {
                    Point2d p1 = new Point2d(pos.x, pos.y);
                    Point2d p2 = new Point2d(nearestPoint.Value.x, nearestPoint.Value.y);
                    if (point.distance(ref p1) < point.distance(ref p2))
                    {
                        nearestPoint = pos;
                    }
                }
            }
            return nearestPoint;
        }

        private int GetArmorNumUnitsCount()
        {
            int count;
            switch (ArmorUnitNumsSet)
            {
                case EArmorUnitNumsSet.Random:
                    count = Random.Next(0, 8 + 1);
                    break;

                case EArmorUnitNumsSet.Range1_3:
                    count = Random.Next(1, 3 + 1);
                    break;

                case EArmorUnitNumsSet.Range3_5:
                    count = Random.Next(3, 5 + 1);
                    break;

                case EArmorUnitNumsSet.Range5_8:
                    count = Random.Next(5, 8 + 1);
                    break;

                case EArmorUnitNumsSet.Default:
                default:
                    count = -1;
                    break;
            }
            return count;
        }

        private int GetFormationCount()
        {
            int count;
            switch (ShipUnitNumsSet)
            {
                case EShipUnitNumsSet.Random:
                    count = Random.Next(0, 5 + 1);
                    break;

                case EShipUnitNumsSet.Range1_3:
                    count = Random.Next(1, 3 + 1);
                    break;

                case EShipUnitNumsSet.Range3_5:
                    count = Random.Next(3, 5 + 1);
                    break;

                case EShipUnitNumsSet.Range1:
                case EShipUnitNumsSet.Default:
                default:
                    count = 1;
                    break;
            }
            return count;
        }

        public GroundGroup getRandomTargetBasedOnRange(IEnumerable<GroundGroup> availableGroundGroups, AirGroup offensiveAirGroup)
        {
            GroundGroup selectedGroundGroup = null;

            if (availableGroundGroups.Any())
            {
                var copy = new List<GroundGroup>(availableGroundGroups);
                copy.Sort(new DistanceComparer(offensiveAirGroup.Position));

                Point3d pos = offensiveAirGroup.Position;
                Point2d position = new Point2d(pos.x, pos.y);

                // TODO: Use range of the aircraft instead of the maxDistance.
                // Problem is that range depends on loadout, so depending on loadout different targets would be available.

                Point2d last = copy.Last().Position;
                double maxDistance = last.distance(ref position);

                List<KeyValuePair<GroundGroup, int>> elements = new List<KeyValuePair<GroundGroup, int>>();

                int previousWeight = 0;

                foreach (GroundGroup groundGroup in copy)
                {
                    double distance = groundGroup.Position.distance(ref position);
                    int weight = Convert.ToInt32(Math.Ceiling(maxDistance - distance));
                    int cumulativeWeight = previousWeight + weight;
                    elements.Add(new KeyValuePair<GroundGroup, int>(groundGroup, cumulativeWeight));

                    previousWeight = cumulativeWeight;
                }

                int diceRoll = Random.Next(0, previousWeight);
                int cumulative = 0;
                for (int i = 0; i < elements.Count; i++)
                {
                    cumulative += elements[i].Value;
                    if (diceRoll <= cumulative)
                    {
                        selectedGroundGroup = elements[i].Key;
                        break;
                    }
                }
            }
            else if (availableGroundGroups.Count() == 1)
            {
                selectedGroundGroup = availableGroundGroups.First();
            }

            return selectedGroundGroup;
        }

        public Stationary getRandomTargetBasedOnRange(IEnumerable<Stationary> availableStationaries, AirGroup offensiveAirGroup)
        {
            Stationary selectedStationary = null;

            if (availableStationaries.Any())
            {
                var copy = new List<Stationary>(availableStationaries);
                copy.Sort(new DistanceComparer(offensiveAirGroup.Position));

                Point3d pos = offensiveAirGroup.Position;
                Point2d position = new Point2d(pos.x, pos.y);

                // TODO: Use range of the aircraft instead of the maxDistance.
                // Problem is that range depends on loadout, so depending on loadout different targets would be available.

                Point2d last = copy.Last().Position;
                double maxDistance = last.distance(ref position);

                List<KeyValuePair<Stationary, int>> elements = new List<KeyValuePair<Stationary, int>>();

                int previousWeight = 0;

                foreach (Stationary stationary in copy)
                {
                    double distance = stationary.Position.distance(ref position);
                    int weight = Convert.ToInt32(Math.Ceiling(maxDistance - distance));
                    int cumulativeWeight = previousWeight + weight;
                    elements.Add(new KeyValuePair<Stationary, int>(stationary, cumulativeWeight));

                    previousWeight = cumulativeWeight;
                }

                int diceRoll = Random.Next(0, previousWeight);
                int cumulative = 0;
                for (int i = 0; i < elements.Count; i++)
                {
                    cumulative += elements[i].Value;
                    if (diceRoll <= cumulative)
                    {
                        selectedStationary = elements[i].Key;
                        break;
                    }
                }
            }
            else if (availableStationaries.Count() == 1)
            {
                selectedStationary = availableStationaries.First();
            }

            return selectedStationary;
        }

        private IEnumerable<GroundGroupWaypoint> CreateWaypoints(EGroundGroupType type, int army, Point2d start, Point2d end)
        {
            IRecalcPathParams pathParams = null;
            if (type == EGroundGroupType.Armor || type == EGroundGroupType.Vehicle)
            {
                pathParams = GamePlay.gpFindPath(start, 10.0, end, 20.0, PathType.GROUND, army);
            }
            else if (type == EGroundGroupType.Ship)
            {
                pathParams = GamePlay.gpFindPath(start, 10.0, end, 20.0, PathType.WATER, army);
            }

            if (pathParams != null)
            {
                while (pathParams.State == RecalcPathState.WAIT)
                {
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Wait for path.", null);
                    Thread.Sleep(100);
                }

                if (pathParams.State == RecalcPathState.SUCCESS)
                {
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Path found (" + pathParams.Path.Length.ToString(System.Globalization.Config.NumberFormat) + ").", null);

                    List<GroundGroupWaypoint> waypoints = new List<GroundGroupWaypoint>();
                    GroundGroupWaypoint lastGroundGroupWaypoint = null;
                    foreach (AiWayPoint aiWayPoint in pathParams.Path)
                    {
                        if (aiWayPoint is AiGroundWayPoint)
                        {
                            AiGroundWayPoint aiGroundWayPoint = aiWayPoint as AiGroundWayPoint;
                            Point3d point = aiGroundWayPoint.P;
                            if (point.z == -1)
                            {
                                GroundGroupWaypoint groundGroupWaypoint = new GroundGroupWaypointLine(point.x, point.y, aiGroundWayPoint.roadWidth, aiGroundWayPoint.Speed);
                                lastGroundGroupWaypoint = groundGroupWaypoint;
                                waypoints.Add(groundGroupWaypoint);
                            }
                            else if (lastGroundGroupWaypoint != null)
                            {
                                // TODO: Fix calculated param

                                GroundGroupWaypoint groundGroupSubWaypoint;
                                if (aiGroundWayPoint.Speed > 0)
                                {
                                    string s = string.Format(Config.NumberFormat, "{0:F2} {1:F2} {2:F2} {3:F2}",
                                        aiGroundWayPoint.P.x, aiGroundWayPoint.P.y, aiGroundWayPoint.P.z, aiGroundWayPoint.roadWidth);
                                    groundGroupSubWaypoint = new GroundGroupWaypointSpline(point.x, point.y, point.z, s);
                                }
                                else
                                {
                                    groundGroupSubWaypoint = new GroundGroupWaypointLine(point.x, point.y, point.z, null);
                                }
                                lastGroundGroupWaypoint.SubWaypoints.Add(groundGroupSubWaypoint);
                            }
                        }
                    }
                    return waypoints;
                }
                else if (pathParams.State == RecalcPathState.FAILED)
                {
                    Debug.WriteLine("Path not found. EGroundGroupType={0}, Army={1}, start={2}, end={3}", type, army, start, end);
                    //Game.gpLogServer(new Player[] { Game.gpPlayer() }, "Path not found.", null);
                }
            }
            return null;
        }

        private IEnumerable<GroundGroupWaypoint> CreateWaypoints(GroundGroup groundGroup, Point2d start, Point2d end, IEnumerable<Groundway> roads)
        {
            if (roads != null && roads.Any())
            {
                Groundway closestRoad = null;
                double closestRoadDistance = 0.0;
                foreach (Groundway road in roads)
                {
                    if (road.Start != null && road.End != null)
                    {
                        Point2d roadStart = road.Start.Position;
                        double distanceStart = start.distance(ref roadStart);
                        Point2d roadEnd = road.End.Position;
                        double distanceEnd = end.distance(ref roadEnd);

                        Point2d p = new Point2d(end.x, end.y);
                        if (distanceEnd < start.distance(ref p))
                        {
                            if (closestRoad == null)
                            {
                                closestRoad = road;
                                closestRoadDistance = distanceStart + distanceEnd;
                            }
                            else
                            {
                                if (distanceStart + distanceEnd < closestRoadDistance)
                                {
                                    closestRoad = road;
                                    closestRoadDistance = distanceStart + distanceEnd;
                                }
                            }
                        }
                    }
                }

                if (closestRoad != null)
                {
                    // CreateWaypoints(groundGroup, start, new Point2d(closestRoad.Start.X, closestRoad.Start.Y));

                    List<GroundGroupWaypoint> waypoints = new List<GroundGroupWaypoint>();
                    waypoints.AddRange(closestRoad.Waypoints);

                    List<Groundway> availableRoads = new List<Groundway>(roads);
                    availableRoads.Remove(closestRoad);

                    Point2d point = closestRoad.End.Position;
                    IEnumerable<GroundGroupWaypoint> results = CreateWaypoints(groundGroup, new Point2d(point.x, point.y), end, availableRoads);
                    if (results != null && results.Any())
                    {
                        waypoints.AddRange(results);
                    }

                    return waypoints;
                }
            }

            return null;
        }

        private bool generateColumnFormation(ISectionFile missionFile, GroundGroup groundGroup, int columnSize)
        {
            string groundGroupId = groundGroup.Id;

            const int offsetBase = 500;

            int count = 0;
            List<GroundGroupWaypoint> waypoints = groundGroup.Waypoints;
            for (int i = 1; i < columnSize; i++)
            {
                double xOffset = -1.0;
                double yOffset = -1.0;

                bool subWaypointUsed = false;
                GroundGroupWaypoint wayPointFirst = waypoints.First();
                Point2d p1 = wayPointFirst.Position;
                if (wayPointFirst.SubWaypoints.Count > 0)
                {
                    Point2d p2 = wayPointFirst.SubWaypoints[0].Position;
                    double distance = p1.distance(ref p2);
                    xOffset = offsetBase * ((p2.x - p1.x) / distance);
                    yOffset = offsetBase * ((p2.y - p1.y) / distance);
                    subWaypointUsed = true;
                }
                if (subWaypointUsed == false)
                {
                    Point2d p2 = waypoints[1].Position;
                    double distance = p1.distance(ref p2);
                    xOffset = offsetBase * ((p2.x - p1.x) / distance);
                    yOffset = offsetBase * ((p2.y - p1.y) / distance);
                }

                wayPointFirst.X += xOffset;
                wayPointFirst.Y += yOffset;

                subWaypointUsed = false;
                GroundGroupWaypoint wayPointLast = waypoints.Last();
                p1 = new Point2d(wayPointLast.X, wayPointLast.Y);
                GroundGroupWaypoint wayPointLast2 = waypoints[waypoints.Count - 2];
                if (wayPointLast2.SubWaypoints.Count > 0)
                {
                    GroundGroupWaypoint subWaypoint = wayPointLast2.SubWaypoints.Last();
                    Point2d p2 = subWaypoint.Position;
                    double distance = p1.distance(ref p2);
                    xOffset = offsetBase * ((p2.x - p1.x) / distance);
                    yOffset = offsetBase * ((p2.y - p1.y) / distance);
                    subWaypointUsed = true;
                }
                if (subWaypointUsed == false)
                {
                    Point2d p2 = new Point2d(wayPointLast2.X, wayPointLast2.Y);
                    double distance = p1.distance(ref p2);
                    xOffset = offsetBase * ((p2.x - p1.x) / distance);
                    yOffset = offsetBase * ((p2.y - p1.y) / distance);
                }

                wayPointLast.X -= xOffset;
                wayPointLast.Y -= yOffset;

                groundGroup.UpdateId(string.Format(Config.NumberFormat, "{0}.{1}", groundGroupId, i));

                if (waypoints.Count >= 2)
                {
                    groundGroup.WriteTo(missionFile);
                    count++;
                }
            }

            return count > 0;
        }

        #region GroundGroup

        #region GetAvailable

        public GroundGroup GetAvailableRandomGroundGroup()
        {
            if (HasAvailableGroundGroup)
            {
                return AvailableGroundGroups[Random.Next(AvailableGroundGroups.Count)];
            }
            return null;
        }

        public IEnumerable<GroundGroup> getAvailableEnemyGroundGroups(int armyIndex)
        {
            return EnableMissionMultiAssign ? AllGroundGroups.Where(x => x.Army == Army.Enemy(armyIndex)) : AvailableGroundGroups.Where(x => x.Army == Army.Enemy(armyIndex));
        }

        //public IEnumerable<GroundGroup> getAvailableFriendlyGroundGroups(int armyIndex)
        //{
        //    return AvailableGroundGroups.Where(x => x.Army == armyIndex);
        //}

        public IEnumerable<GroundGroup> getAvailableEnemyGroundGroups(int armyIndex, IEnumerable<EGroundGroupType> groundGroupTypes)
        {
            IEnumerable<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(armyIndex);
            return groundGroups.Where(x => groundGroupTypes.Contains(x.Type));
        }

        public bool HasAvailableEnemyGroundGroups(int armyIndex, IEnumerable<EGroundGroupType> groundGroupTypes)
        {
            IEnumerable<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(armyIndex);
            return groundGroups.Any(x => groundGroupTypes.Contains(x.Type));
        }

        //public IEnumerable<GroundGroup> getAvailableFriendlyGroundGroups(int armyIndex, IEnumerable<EGroundGroupType> groundGroupTypes)
        //{
        //    IEnumerable<GroundGroup> groundGroups = getAvailableFriendlyGroundGroups(armyIndex);
        //    return groundGroups.Where(x => groundGroupTypes.Contains(x.Type));
        //}

        //public GroundGroup getAvailableRandomEnemyGroundGroup(int armyIndex)
        //{
        //    IEnumerable<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(armyIndex);
        //    if (groundGroups.Any())
        //    {
        //        return groundGroups.ElementAt(Random.Next(groundGroups.Count()));
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public GroundGroup getAvailableRandomEnemyGroundGroup(AirGroup airGroup, EMissionType missionType)
        {
            if (missionType == EMissionType.ARMED_MARITIME_RECON || missionType == EMissionType.MARITIME_RECON || missionType == EMissionType.ATTACK_SHIP)
            {
                return getAvailableRandomEnemyGroundGroup(airGroup, new EGroundGroupType[] { EGroundGroupType.Ship });
            }
            else if (missionType == EMissionType.ARMED_RECON || missionType == EMissionType.RECON)
            {
                return getAvailableRandomEnemyGroundGroup(airGroup, new EGroundGroupType[] { EGroundGroupType.Armor, EGroundGroupType.Train, EGroundGroupType.Vehicle });
            }
            else if (missionType == EMissionType.ATTACK_ARMOR)
            {
                return getAvailableRandomEnemyGroundGroup(airGroup, new EGroundGroupType[] { EGroundGroupType.Armor });
            }
            else if (missionType == EMissionType.ATTACK_VEHICLE)
            {
                return getAvailableRandomEnemyGroundGroup(airGroup, new EGroundGroupType[] { EGroundGroupType.Vehicle });
            }
            else if (missionType == EMissionType.ATTACK_TRAIN)
            {
                return getAvailableRandomEnemyGroundGroup(airGroup, new EGroundGroupType[] { EGroundGroupType.Train });
            }
            else
            {
                return null;
            }
        }

        public GroundGroup getAvailableRandomEnemyGroundGroup(AirGroup airGroup, IEnumerable<EGroundGroupType> groundGroupTypes)
        {
            IEnumerable<GroundGroup> groundGroups = getAvailableEnemyGroundGroups(airGroup.ArmyIndex, groundGroupTypes);
            if (groundGroups.Any())
            {
                //GroundGroup targetGroundGroup = groundGroups[Random.Next(groundGroups.Count)];
                return getRandomTargetBasedOnRange(groundGroups, airGroup);
            }
            else
            {
                return null;
            }
        }

        //public GroundGroup getAvailableRandomFriendlyGroundGroup(AirGroup airGroup)
        //{
        //    IEnumerable<GroundGroup> groundGroups = getAvailableFriendlyGroundGroups(airGroup.ArmyIndex);
        //    if (groundGroups.Any())
        //    {
        //        //GroundGroup targetGroundGroup = groundGroups[Random.Next(groundGroups.Count)];
        //        return getRandomTargetBasedOnRange(groundGroups, airGroup);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public GroundGroup getAvailableRandomFriendlyGroundGroup(AirGroup airGroup, IEnumerable<EGroundGroupType> groundGroupTypes)
        //{
        //    IEnumerable<GroundGroup> groundGroups = getAvailableFriendlyGroundGroups(airGroup.ArmyIndex, groundGroupTypes);
        //    if (groundGroups.Any())
        //    {
        //        //GroundGroup targetGroundGroup = groundGroups[Random.Next(groundGroups.Count)];
        //        return getRandomTargetBasedOnRange(groundGroups, airGroup);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        #endregion

        public GroundGroup UpdateGroundGroup(ISectionFile sectionFile, GroundGroup groundGroup)
        {
            Point2d point = groundGroup.Position;
            // Debug.WriteLine("GroundGroup Name={0}, LandTypes={1}", groundGroup.DisplayName, gamePlay.gpLandType(point.x, point.y).ToString());
            int army = GamePlay.gpFrontArmy(point.x, point.y);
            if (army == (int)EArmy.Red || army == (int)EArmy.Blue)
            {
                if (groundGroup.Army == Army.Enemy(army))
                {
                    groundGroup.UpdateArmy(army);
                }
                if (sectionFile != null)
                {
                    groundGroup.WriteTo(sectionFile);
                }
                return groundGroup;
            }
            else
            {
                Debug.WriteLine("no Army GroundGroup[X:{0:F2}, Y:{1:F2}] {2}[{3}]", point.x, point.y, groundGroup.DisplayName, groundGroup.Id);
            }
            return null;
        }

        public IEnumerable<GroundGroup> CreateGroundGroups(ISectionFile sectionFile, IEnumerable<GroundGroup> groundGroups)
        {
            List<GroundGroup> newGroundGroups = new List<GroundGroup>();
            if (GroundGroupGenerateType == EGroundGroupGenerateType.Default)
            {
                foreach (GroundGroup groundGroup in groundGroups)
                {
                    GroundGroup groundGroupUpdated = UpdateGroundGroup(sectionFile, groundGroup);
                    if (groundGroupUpdated != null)
                    {
                        newGroundGroups.Add(groundGroupUpdated);
                    }
                }
            }
            else if (GroundGroupGenerateType == EGroundGroupGenerateType.Generic)
            {
                foreach (GroundGroup groundGroup in groundGroups)
                {
                    if (groundGroup.Type == EGroundGroupType.Armor || groundGroup.Type == EGroundGroupType.Armor)
                    {
                        Armor armor = CreateGroundGroupRoad(sectionFile, new Groundway(groundGroup.Id, groundGroup.Waypoints));
                        if (armor != null)
                        {
                            newGroundGroups.Add(armor);
                        }
                    }
                    else if (groundGroup.Type == EGroundGroupType.Ship)
                    {
                        ShipGroup shipGroup = CreateGroundGroupWaterway(sectionFile, new Groundway(groundGroup.Id, groundGroup.Waypoints));
                        if (shipGroup != null)
                        {
                            newGroundGroups.Add(shipGroup);
                        }
                    }
                    else if (groundGroup.Type == EGroundGroupType.Train)
                    {
                        GroundGroup groundGroupNew = CreateGroundGroupRailway(sectionFile, new Groundway(groundGroup.Id, groundGroup.Waypoints));
                        if (groundGroupNew != null)
                        {
                            newGroundGroups.Add(groundGroupNew);
                        }
                    }
                }
            }
            return newGroundGroups;
        }

        public Armor CreateGroundGroupRoad(ISectionFile sectionFile, Groundway groundway)
        {
            GroundGroupWaypoint point = groundway.End;
            int army = GamePlay.gpFrontArmy(point.X, point.Y);

            string option = GroundGroup.DefaultClasses[(int)EGroundGroupType.Armor][GroundGroup.DefaultClassesOption];
            ArmorOption armorOption = ArmorOption.Create(option);
            if (army == (int)EArmy.Red || army == (int)EArmy.Blue)
            {
                // For groundways only the end must be in friendly territory.
                Armor armor = new Armor(groundway.Id, GroundGroup.DefaultClasses[(int)EGroundGroupType.Armor][army - 1], army, Army.DefaultCountry((EArmy)army), option, armorOption, groundway.Waypoints);
                if (sectionFile != null)
                {
                    armor.WriteTo(sectionFile);
                }
                return armor;
            }
            else
            {
                Debug.WriteLine("no Army Roads[End.X:{0}, End.Y:{1}]", point.X, point.Y);
            }
            return null;
        }

        public IEnumerable<Armor> CreateGroundGroupsRoads(ISectionFile sectionFile, IEnumerable<Groundway> groundways)
        {
            List<Armor> armors = new List<Armor>();
            foreach (Groundway groundway in groundways)
            {
                Armor armor = CreateGroundGroupRoad(sectionFile, groundway);
                if (armor != null)
                {
                    armors.Add(armor);
                }
            }
            return armors;
        }

        public ShipGroup CreateGroundGroupWaterway(ISectionFile sectionFile, Groundway waterway)
        {
            GroundGroupWaypoint point = waterway.End;
            string option = GroundGroup.DefaultClasses[(int)EGroundGroupType.Ship][GroundGroup.DefaultClassesOption];
            ShipOption shipOption = ShipOption.Create(option);
            int army = GamePlay.gpFrontArmy(point.X, point.Y);
            // For waterways only the end must be in friendly territory.
            if (army == (int)EArmy.Red || army == (int)EArmy.Blue)
            {
                ShipGroup supplyShip = new ShipGroup(waterway.Id, GroundGroup.DefaultClasses[(int)EGroundGroupType.Ship][army - 1], army, Army.DefaultCountry((EArmy)army), option, shipOption, waterway.Waypoints);  // British Tanker
                if (sectionFile != null)
                {
                    supplyShip.WriteTo(sectionFile);
                }
                return supplyShip;
            }
            else
            {
                Debug.WriteLine("no Army Waterway[End.X:{0}, End.Y:{1}]", point.X, point.Y);
            }
            return null;
        }

        public IEnumerable<ShipGroup> CreateGroundGroupsWaterways(ISectionFile sectionFile, IEnumerable<Groundway> waterways)
        {
            List<ShipGroup> ships = new List<ShipGroup>();
            foreach (Groundway waterway in waterways)
            {
                ShipGroup shipGroup = CreateGroundGroupWaterway(sectionFile, waterway);
                if (shipGroup != null)
                {
                    ships.Add(shipGroup);
                }
            }
            return ships;
        }

        public GroundGroup CreateGroundGroupRailway(ISectionFile sectionFile, Groundway railway)
        {
            GroundGroupWaypoint point = railway.Start;
            GroundGroupWaypoint point2 = railway.End;
            int army = GamePlay.gpFrontArmy(point.X, point.Y);
            int army2 = GamePlay.gpFrontArmy(point2.X, point2.Y);
            // For waterways only the end must be in friendly territory.
            if ((army == (int)EArmy.Red && army2 == (int)EArmy.Red) || (army == (int)EArmy.Blue && army2 == (int)EArmy.Blue))
            {
                GroundGroup train = new GroundGroup(railway.Id, GroundGroup.DefaultClasses[(int)EGroundGroupType.Train][army - 1], army, Army.DefaultCountry((EArmy)army), string.Empty, railway.Waypoints);       // United Kingdom 57xx 0-6-0PT c0
                if (sectionFile != null)
                {
                    train.WriteTo(sectionFile);
                }
                return train;
            }
            else
            {
                Debug.WriteLine("no Army Railway[Start.X:{0}, Start.Y:{1}, End.X:{2}, End.Y:{3}]", point.X, point.Y, point2.X, point2.Y);
            }
            return null;
        }

        public IEnumerable<GroundGroup> CreateGroundGroupsRailways(ISectionFile sectionFile, IEnumerable<Groundway> railways)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            foreach (Groundway railway in railways)
            {
                GroundGroup groundGroup = CreateGroundGroupRailway(sectionFile, railway);
                if (groundGroup != null)
                {
                    groundGroups.Add(groundGroup);
                }
            }
            return groundGroups;
        }

        public int AddRandomGroundGroupsByOperations(int additionalGroundOperations, IEnumerable<IEnumerable<string>> groundActors)
        {
            int needGroups = (additionalGroundOperations * Config.AverageGroundOperationGroundGroupCount - AllGroundGroups.Count) / Random.Next(2, 6);
            return AddRandomGroundGroups(needGroups, groundActors);
        }

        public int AddRandomGroundGroups(int needGroups, IEnumerable<IEnumerable<string>> groundActors)
        {
            if (needGroups > 0)
            {
                IEnumerable<GroundGroup> groundGroups = GetRandomGroundGroups(needGroups, groundActors);
                AvailableGroundGroups.AddRange(groundGroups);
                AllGroundGroups.AddRange(groundGroups);
                return groundGroups.Count();
            }
            return 0;
        }

        private IEnumerable<GroundGroup> GetRandomGroundGroups(int needGroups, IEnumerable<IEnumerable<string>> groundActors)
        {
            List<GroundGroup> groundGroups = new List<GroundGroup>();
            Point3d pos;
            IEnumerable<AiAirport> airPortsRed = GamePlay.gpAirports().Where(x => { pos = x.Pos(); return GamePlay.gpFrontArmy(pos.x, pos.y) == (int)EArmy.Red/* return MapUtil.IsInRange(ref rangeRed, ref pos)*/; });
            IEnumerable<AiAirport> airPortsBlue = GamePlay.gpAirports().Where(x => { pos = x.Pos(); return GamePlay.gpFrontArmy(pos.x, pos.y) == (int)EArmy.Blue /*return MapUtil.IsInRange(ref rangeBlue, ref pos)*/; });
            int reTries = -1;
            while (groundGroups.Count < needGroups && reTries < MaxRetryCreateAllGroundGroups)
            {
                int army = Random.Next((int)EArmy.Red, (int)EArmy.Blue + 1);
                ECountry country = Army.DefaultCountry((EArmy)army);
                EGroundGroupType type = (EGroundGroupType)Random.Next((int)EGroundGroupType.Vehicle, (int)EGroundGroupType.Ship/*Train*/ + 1);
                LandTypes[] landTypesValid = GroundGroup.GetLandTypes(type);
                string groundClass = GroundGroupGenerateType == EGroundGroupGenerateType.Generic ? GroundGroup.DefaultClasses[(int)type][army - 1] : GetRandomActorClass(groundActors, army, (int)type, GroundGroup.DefaultClasses);
                string option = GroundGroup.DefaultClasses[(int)type][GroundGroup.DefaultClassesOption];

                // Id
                string id = CreateAvailableGroundGroupId(groundGroups);
                if (!string.IsNullOrEmpty(id))
                {
                    List<GroundGroupWaypoint> waypoints = new List<GroundGroupWaypoint>();
                    if (type == EGroundGroupType.Armor || type == EGroundGroupType.Vehicle/* || type == EGroundGroupType.Unknown*/)
                    {
                        AiAirport aiAirport = GetRandomAirport((army == (int)EArmy.Red) ? airPortsRed : airPortsBlue);
                        pos = aiAirport.Pos();
                        double r = aiAirport.FieldR();
                        Point3d? posSpawn = CreateRandomPoint(army, (float)pos.x, (float)pos.y, (float)r * MaxSpawnDifDistanceAirportRRateGroundGroup, GroundGroup.DefaultNormalMoveZ, landTypesValid);
                        if (posSpawn != null)
                        {
                            waypoints.Add(new GroundGroupWaypointLine(posSpawn.Value.x, posSpawn.Value.y, posSpawn.Value.z, null));
                            posSpawn = CreateRandomPoint(army, (float)pos.x, (float)pos.y, (float)r * MaxSpawnDifDistanceAirportRRateGroundGroup, GroundGroup.DefaultNormalMoveZ, landTypesValid);
                            if (posSpawn != null)
                            {
                                waypoints.Add(new GroundGroupWaypointLine(posSpawn.Value.x, posSpawn.Value.y, posSpawn.Value.z, null));
                                GroundGroup groundGroup = type == EGroundGroupType.Armor || type == EGroundGroupType.Vehicle ?
                                    new Armor(id, groundClass, army, country, option, ArmorOption.Create(option), waypoints) : new GroundGroup(id, groundClass, army, country, option, waypoints);
                                groundGroups.Add(groundGroup);
                                continue;
                            }
                        }
                    }
                    else if (type == EGroundGroupType.Ship || type == EGroundGroupType.Train)
                    {
                        Point3d? posSpawn = CreateRandomPoint(army, GroundGroup.DefaultNormalMoveZ, landTypesValid);
                        if (posSpawn != null)
                        {
                            waypoints.Add(new GroundGroupWaypointLine(posSpawn.Value.x, posSpawn.Value.y, posSpawn.Value.z, null));
                            posSpawn = CreateRandomPoint(army, GroundGroup.DefaultNormalMoveZ, landTypesValid);
                            if (posSpawn != null)
                            {
                                waypoints.Add(new GroundGroupWaypointLine(posSpawn.Value.x, posSpawn.Value.y, posSpawn.Value.z, null));
                                ShipGroup groundGroup = new ShipGroup(id, groundClass, army, country, option, ShipOption.Create(option), waypoints);
                                groundGroups.Add(groundGroup);
                                continue;
                            }
                        }
                    }
                }
                reTries++;
            }
            return groundGroups;
        }

        private string GetRandomActorClass(IEnumerable<IEnumerable<string>> actors, int army, int type, string[][] defaultClasses)
        {
            IEnumerable<string> typedActors = actors.ElementAt(type * 2 + (army - 1));
            return typedActors.Any() ? typedActors.ElementAt(Random.Next(0, typedActors.Count())) : defaultClasses[type][army - 1];
        }

        private string CreateAvailableGroundGroupId(List<GroundGroup> groundGroups)
        {
            int idx = Config.AddGroundGroupStartIdNo;
            string id;
            int reTry = -1;
            while (reTry++ <= MaxRetryCreateAllGroundGroups)
            {
                id = string.Format("{0}_{1}", idx.ToString(Config.NumberFormat), MissionFile.KeyChief);
                if (!groundGroups.Any(x => string.Compare(x.Id, id) == 0) && !AllGroundGroups.Any(x => string.Compare(x.Id, id) == 0))
                {
                    return id;
                }
                idx++;
            }
            return string.Empty;
        }

        private AiAirport GetRandomAirport(IEnumerable<AiAirport> airPorts)
        {
            return airPorts.ElementAt(Random.Next(0, airPorts.Count()));
        }

        private Point3d? CreateRandomPoint(int army, float z, LandTypes[] landTypes)
        {
            Array arrayLandTypes = Enum.GetValues(typeof(LandTypes));
            int reTry = -1;
            while (reTry++ <= MaxRetryCreateRandomPoint)
            {
                double x = Random.Next((int)(RangeBattleArea.x1), (int)(RangeBattleArea.x2 + 1));
                double y = Random.Next((int)(RangeBattleArea.y1), (int)(RangeBattleArea.y2 + 1));
                if (GamePlay.gpFrontArmy(x, y) == army)
                {
                    LandTypes landType = GamePlay.gpLandType(x, y);
                    foreach (LandTypes item in arrayLandTypes)
                    {
                        if (item != LandTypes.NONE && (item & landType) == item)
                        {
                            if (landTypes.Contains(item))
                            {
                                return new Point3d(x, y, z);
                            }
                        }
                    }
                }
            }
            Debug.WriteLine("Error CreateRandomPoint({0}, {1})", z, string.Join("|", landTypes));
            return null;
        }

        private Point3d? CreateRandomPoint(int army, float x, float y, float r, float z, LandTypes[] landTypes)
        {
            return CreateRandomPoint(GamePlay, Random, army, x, y, r, z, landTypes);
        }

        public static Point3d? CreateRandomPoint(IGamePlay gamePlay, IRandom random, int army, float x, float y, float r, float z, LandTypes[] landTypes)
        {
            Array arrayLandTypes = Enum.GetValues(typeof(LandTypes));
            int reTry = -1;
            while (reTry++ <= MaxRetryCreateRandomPoint)
            {
                double xNew = random.Next((int)(x - r), (int)(x + r + 1));
                double yNew = random.Next((int)(y - r), (int)(y + r + 1));
                if (gamePlay.gpFrontArmy(xNew, yNew) == army)
                {
                    LandTypes landType = gamePlay.gpLandType(xNew, yNew);
                    foreach (LandTypes item in arrayLandTypes)
                    {
                        if (item != LandTypes.NONE && (item & landType) == item)
                        {
                            if (landTypes.Contains(item))
                            {
                                return new Point3d(xNew, yNew, z);
                            }
                        }
                    }
                }
            }
            Debug.WriteLine("Error CreateRandomPoint(x={0}, y={1}, r={2}, z={3}, {4})", x, y, r, z, string.Join("|", landTypes));
            return null;
        }

        #endregion

        #region Stationary

        #region GetAvailable

        public IEnumerable<Stationary> getAvailableEnemyStationaries(int armyIndex)
        {
            return EnableMissionMultiAssign ? AllStationaries.Where(x => x.Army == Army.Enemy(armyIndex)) : AvailableStationaries.Where(x => x.Army == Army.Enemy(armyIndex));
        }

        //public IEnumerable<Stationary> getAvailableFriendlyStationaries(int armyIndex)
        //{
        //    return AvailableStationaries.Where(x => x.Army == armyIndex);
        //}

        public IEnumerable<Stationary> getAvailableEnemyStationaries(int armyIndex, IEnumerable<EStationaryType> stationaryTypes)
        {
            IEnumerable<Stationary> stationaries = getAvailableEnemyStationaries(armyIndex);
            return stationaries.Where(x => stationaryTypes.Contains(x.Type));
        }

        public bool HasAvailableEnemyStationaries(int armyIndex, IEnumerable<EStationaryType> stationaryTypes)
        {
            IEnumerable<Stationary> stationaries = getAvailableEnemyStationaries(armyIndex);
            return stationaries.Any(x => stationaryTypes.Contains(x.Type));
        }

        //public IEnumerable<Stationary> getAvailableFriendlyStationaries(int armyIndex, IEnumerable<EStationaryType> stationaryTypes)
        //{
        //    IEnumerable<Stationary> stationaries = getAvailableFriendlyStationaries(armyIndex);
        //    return stationaries.Where(x => stationaryTypes.Contains(x.Type));
        //}

        //public Stationary getAvailableRandomEnemyStationary(int armyIndex)
        //{
        //    IEnumerable<Stationary> stationaries = getAvailableEnemyStationaries(armyIndex);
        //    if (stationaries.Any())
        //    {
        //        return stationaries.ElementAt(Random.Next(stationaries.Count()));
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public Stationary getAvailableRandomEnemyStationary(AirGroup airGroup, EMissionType missionType)
        {
            if (missionType == EMissionType.ARMED_MARITIME_RECON || missionType == EMissionType.MARITIME_RECON || missionType == EMissionType.ATTACK_SHIP)
            {
                return getAvailableRandomEnemyStationary(airGroup, new EStationaryType[] { EStationaryType.Ship });
            }
            else if (missionType == EMissionType.ARMED_RECON || missionType == EMissionType.RECON)
            {
                return getAvailableRandomEnemyStationary(airGroup, new EStationaryType[] { EStationaryType.Artillery, EStationaryType.Flak, EStationaryType.Ammo, EStationaryType.Weapons,
                                                    EStationaryType.Aircraft, EStationaryType.Radar, EStationaryType.Depot, EStationaryType.Car, EStationaryType.ConstCar, });
            }
            else if (missionType == EMissionType.ATTACK_AIRCRAFT)
            {
                return getAvailableRandomEnemyStationary(airGroup, new EStationaryType[] { EStationaryType.Aircraft });
            }
            else if (missionType == EMissionType.ATTACK_ARTILLERY)
            {
                return getAvailableRandomEnemyStationary(airGroup, new EStationaryType[] { EStationaryType.Artillery, EStationaryType.Flak, EStationaryType.Ammo, EStationaryType.Weapons, });
            }
            else if (missionType == EMissionType.ATTACK_RADAR)
            {
                return getAvailableRandomEnemyStationary(airGroup, new EStationaryType[] { EStationaryType.Radar });
            }
            else if (missionType == EMissionType.ATTACK_DEPOT)
            {
                return getAvailableRandomEnemyStationary(airGroup, new EStationaryType[] { EStationaryType.Depot });
            }

            return null;
        }

        public Stationary getAvailableRandomEnemyStationary(AirGroup airGroup, IEnumerable<EStationaryType> stationaryTypes)
        {
            IEnumerable<Stationary> stationaries = getAvailableEnemyStationaries(airGroup.ArmyIndex, stationaryTypes);
            if (stationaries.Any())
            {
                //GroundGroup targetGroundGroup = groundGroups[Random.Next(groundGroups.Count];
                return getRandomTargetBasedOnRange(stationaries, airGroup);
            }
            else
            {
                return null;
            }
        }

        //public Stationary getAvailableRandomFriendlyStationary(AirGroup airGroup)
        //{
        //    IEnumerable<Stationary> stationaries = getAvailableFriendlyStationaries(airGroup.ArmyIndex);
        //    if (stationaries.Any())
        //    {
        //        //GroundGroup targetGroundGroup = groundGroups[Random.Next(groundGroups.Count)];
        //        return getRandomTargetBasedOnRange(stationaries, airGroup);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public Stationary getAvailableRandomFriendlyStationary(AirGroup airGroup, IEnumerable<EStationaryType> stationaryTypes)
        //{
        //    IEnumerable<Stationary> stationaries = getAvailableFriendlyStationaries(airGroup.ArmyIndex, stationaryTypes);
        //    if (stationaries.Any())
        //    {
        //        //GroundGroup targetGroundGroup = groundGroups[Random.Next(groundGroups.Count)];
        //        return getRandomTargetBasedOnRange(stationaries, airGroup);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}                                                       

        #endregion

        public Stationary CreateStationary(ISectionFile sectionFile, EStationaryType stationaryType, Stationary artillery)
        {
            // Debug.WriteLine("Stationary Name={0}, LandTypes={1}", artillery.DisplayName, gamePlay.gpLandType(artillery.X, artillery.Y).ToString());
            int army = GamePlay.gpFrontArmy(artillery.X, artillery.Y);
            if (StationaryGenerateType == EStationaryGenerateType.Default)
            {
                if (army == (int)EArmy.Red || army == (int)EArmy.Blue)
                {
                    if (artillery.Army != army)
                    {
                        artillery.UpdateArmy(army);
                    }
                    if (sectionFile != null)
                    {
                        artillery.WriteTo(sectionFile);
                    }
                }
                return artillery;
            }
            else if (StationaryGenerateType == EStationaryGenerateType.Generic)
            {
                string option = Stationary.DefaultClasses[(int)stationaryType][Stationary.DefaultClassesOption];
                if (army == (int)EArmy.Red || army == (int)EArmy.Blue)
                {
                    // For red army
                    Stationary stationary = stationaryType == EStationaryType.Artillery ?
                        new Artillery(artillery.Id, Stationary.DefaultClasses[(int)stationaryType][army - 1], army, Army.DefaultCountry((EArmy)army), artillery.X, artillery.Y, artillery.Direction, option, ArtilleryOption.Create(option)) :
                        stationaryType == EStationaryType.Ship ?
                        new ShipUnit(artillery.Id, Stationary.DefaultClasses[(int)stationaryType][army - 1], army, Army.DefaultCountry((EArmy)army), artillery.X, artillery.Y, artillery.Direction, option, ShipOption.Create(option)) :
                        new Stationary(artillery.Id, Stationary.DefaultClasses[(int)stationaryType][army - 1], army, Army.DefaultCountry((EArmy)army), artillery.X, artillery.Y, artillery.Direction, option);
                    if (sectionFile != null)
                    {
                        stationary.WriteTo(sectionFile);
                    }
                    return stationary;
                }
                else
                {
                    Debug.WriteLine("no Army Stationary[Class:{0}, Id:{1}, X:{2}, Y:{3}]", artillery.Class, artillery.Id, artillery.X, artillery.Y);
                }
            }
            return null;
        }

        public IEnumerable<Stationary> CreateStationaries(ISectionFile sectionFile, IEnumerable<Stationary> stationaries)
        {
            List<Stationary> stationariesNew = new List<Stationary>();
            foreach (Stationary stationary in stationaries)
            {
                Stationary stationaryNew = CreateStationary(sectionFile, stationary.Type, stationary);
                if (stationaryNew != null)
                {
                    stationariesNew.Add(stationaryNew);
                }
            }
            return stationariesNew;
        }

        public GroundObject CreateStationaryBuilding(ISectionFile sectionFile, Building depot)
        {
            // For depots the position must be in friendly territory.
            if (StationaryGenerateType == EStationaryGenerateType.Default)
            {
                if (sectionFile != null)
                {
                    depot.WriteTo(sectionFile);
                }
                return depot;
            }
            else if (StationaryGenerateType == EStationaryGenerateType.Generic)
            {
                int army = GamePlay.gpFrontArmy(depot.X, depot.Y);
                if (army == (int)EArmy.Red || army == (int)EArmy.Blue)
                {
                    // For red army
                    Stationary fuelTruck = new Stationary(depot.Id, Stationary.DefaultClasses[(int)EStationaryType.Depot][army - 1], army, Army.DefaultCountry((EArmy)army), depot.X, depot.Y, depot.Direction);
                    if (sectionFile != null)
                    {
                        fuelTruck.WriteTo(sectionFile);
                    }
                    return fuelTruck;
                }
                else
                {
                    Debug.WriteLine("no Army Stationary.Building[Class:{0}, Id:{1}, X:{2}, Y:{3}]", depot.Class, depot.Id, depot.X, depot.Y);
                }
            }
            return null;
        }

        public IEnumerable<GroundObject> CreateStationaryBuildings(ISectionFile sectionFile, IEnumerable<Building> depots)
        {
            List<GroundObject> groundObjects = new List<GroundObject>();
            foreach (Building depot in depots)
            {
                GroundObject groundObject = CreateStationaryBuilding(sectionFile, depot);
                if (groundObject != null)
                {
                    groundObjects.Add(groundObject);
                }
            }
            return groundObjects;
        }

        public int StationaryWriteTo(ISectionFile file)
        {
            int count = 0;
            foreach (Stationary stationary in AvailableStationaries.OrderBy(x => x.Id))
            {
                if (stationary is ShipUnit)
                {
                    SetSkill(stationary as ShipUnit);
                }

                if (stationary is Artillery)
                {
                    SetSkill(stationary as Artillery);
                }

                stationary.WriteTo(file);
                count++;
            }
            return count;
        }

        public int AddRandomStationariesByOperations(int additionalStationaries, IEnumerable<IEnumerable<string>> stationaries)
        {
            int needUnits = additionalStationaries * Config.AverageStationaryOperationUnitCount - AllGroundGroups.Count - AllStationaries.Count;
            return AddRandomStationaries(needUnits, stationaries);
        }

        public int AddRandomStationaries(int needUnits, IEnumerable<IEnumerable<string>> stationaries)
        {
            if (needUnits > 0)
            {
                IEnumerable<Stationary> stationariesRandom = GetRandomStationaries(needUnits, stationaries);
                AvailableStationaries.AddRange(stationariesRandom);
                AllStationaries.AddRange(stationariesRandom);
                return stationariesRandom.Count();
            }
            return 0;
        }

        private IEnumerable<Stationary> GetRandomStationaries(int needUnitNums, IEnumerable<IEnumerable<string>> stationaries)
        {
            List<Stationary> stationariesRandom = new List<Stationary>();
            Point3d pos;
            IEnumerable<AiAirport> airPortsRed = GamePlay.gpAirports().Where(x => { pos = x.Pos(); return GamePlay.gpFrontArmy(pos.x, pos.y) == (int)EArmy.Red/* return MapUtil.IsInRange(ref rangeRed, ref pos)*/; });
            IEnumerable<AiAirport> airPortsBlue = GamePlay.gpAirports().Where(x => { pos = x.Pos(); return GamePlay.gpFrontArmy(pos.x, pos.y) == (int)EArmy.Blue /*return MapUtil.IsInRange(ref rangeBlue, ref pos)*/; });
            int reTries = -1;
            while (stationariesRandom.Count < needUnitNums && reTries < MaxRetryCreateAllStationary)
            {
                int army = Random.Next((int)EArmy.Red, (int)EArmy.Blue + 1);
                ECountry country = Army.DefaultCountry((EArmy)army);
                EStationaryType type = (EStationaryType)Random.Next((int)EStationaryType.Radar, (int)EStationaryType.Unknown + 1);
                LandTypes[] landTypesValid = Stationary.GetLandTypes(type);
                string unitClass = StationaryGenerateType == EStationaryGenerateType.Generic ? Stationary.DefaultClasses[(int)type][army - 1] : GetRandomActorClass(stationaries, army, (int)type, Stationary.DefaultClasses);
                string option = Stationary.DefaultClasses[(int)type][Stationary.DefaultClassesOption];

                // Id
                string id = CreateAvailableStationaryId(stationariesRandom);
                if (!string.IsNullOrEmpty(id))
                {
                    if (type == EStationaryType.Ship)
                    {
                        Point3d? posSpawn = CreateRandomPoint(army, Stationary.DefaultSpawnZ, landTypesValid);
                        if (posSpawn != null)
                        {
                            Stationary stationary = new ShipUnit(id, unitClass, army, country, posSpawn.Value.x, posSpawn.Value.y, posSpawn.Value.z, option, ShipOption.Create(option));
                            stationariesRandom.Add(stationary);
                            continue;
                        }
                    }
                    else
                    {
                        AiAirport aiAirport = GetRandomAirport((army == (int)EArmy.Red) ? airPortsRed : airPortsBlue);
                        pos = aiAirport.Pos();
                        double r = aiAirport.FieldR();
                        Point3d? posSpawn = CreateRandomPoint(army, (float)pos.x, (float)pos.y,
                            (float)r * (type == EStationaryType.Aircraft || type == EStationaryType.Weapons ? MaxSpawnDifDistanceAirportRRateStationaryAircraft : MaxSpawnDifDistanceAirportRRateStationary),
                            Stationary.DefaultSpawnZ, landTypesValid);
                        if (posSpawn != null)
                        {
                            Stationary stationary = type == EStationaryType.Artillery ?
                                new Artillery(id, unitClass, army, country, posSpawn.Value.x, posSpawn.Value.y, posSpawn.Value.z, option, ArtilleryOption.Create(option)) :
                                new Stationary(id, unitClass, army, country, posSpawn.Value.x, posSpawn.Value.y, posSpawn.Value.z, option);
                            stationariesRandom.Add(stationary);
                            continue;
                        }
                    }
                }
                reTries++;
            }
            return stationariesRandom;
        }

        private string CreateAvailableStationaryId(List<Stationary> stationaries)
        {
            int idx = Config.AddStationaryUnitStartIdNo;
            string id;
            int reTry = -1;
            while (reTry++ <= MaxRetryCreateAllStationary)
            {
                id = string.Format("{0}{1}", MissionFile.KeyStatic, idx.ToString(Config.NumberFormat));
                if (!stationaries.Any(x => string.Compare(x.Id, id) == 0) && !AllStationaries.Any(x => string.Compare(x.Id, id) == 0))
                {
                    return id;
                }
                idx++;
            }
            return string.Empty;
        }

        #endregion

        #region Skill

        private IEnumerable<ESystemType> CreateSkills(ESkillSetShip skillSetShip)
        {
            IEnumerable<ESystemType> skills;
            if (skillSetShip == ESkillSetShip.Default)
            {
                skills = null;
            }
            else if (skillSetShip == ESkillSetShip.Random)
            {
                skills = Enum.GetValues(typeof(ESystemType)).Cast<ESystemType>().Except(new ESystemType[] { ESystemType.Count });
            }
            else
            {
                skills = Skills.Create(skillSetShip);
            }

            return skills;
        }

        private void SetSkill(ShipGroup shipGroup)
        {
            SetSkill(shipGroup.Option);
        }

        private void SetSkill(ShipUnit shipUnit)
        {
            SetSkill(shipUnit.Option);
        }

        private void SetSkill(ShipOption shipOption)
        {
            // Skill
            if (SkillShip == ESkillSetShip.Default)
            {
                ;
            }
            else/* if (SkillShip >= ESkillSetShip.Rookie && SkillShip <= ESkillSetShip.Ace || SkillShip == ESkillSetShip.Random)*/
            {
                shipOption.Skill = GetRandomSkill();
            }

            // Sleep
            if (ShipSleep == ShipOption.SleepMissionDefault)
            {
                ;
            }
            else if (ShipSleep == ShipOption.SleepRandom)
            {
                shipOption.Sleep = ShipOption.CreateRandomSleep(Random);
            }
            else
            {
                shipOption.Sleep = ShipSleep;
            }

            // Slowfire
            if (ShipSlowfire == ShipOption.SlowFireMissionDefault)
            {
                ;
            }
            else if (ShipSlowfire == ShipOption.SlowFireRandom)
            {
                shipOption.Slowfire = ShipOption.CreateRandomSlowfire(Random);
            }
            else
            {
                shipOption.Slowfire = ShipSlowfire;
            }
        }

        private void SetSkill(Artillery artillery)
        {
            SetSkill(artillery.Option);
        }

        private void SetSkill(ArtilleryOption artilleryOption)
        {
            // Timeout
            if (ArtilleryTimeout == ArtilleryOption.TimeoutMissionDefault)
            {
                ;
            }
            else if (ShipSleep == ArtilleryOption.TimeoutRandom)
            {
                artilleryOption.Timeout = ArtilleryOption.CreateRandomTimeout(Random);
            }
            else
            {
                artilleryOption.Timeout = ArtilleryTimeout;
            }

            // RHide
            if (ArtilleryRHide == ArtilleryOption.RHideMissionDefault)
            {
                ;
            }
            else if (ArtilleryRHide == ArtilleryOption.TimeoutRandom)
            {
                artilleryOption.RHide = ArtilleryOption.CreateRandomRHide(Random);
            }
            else
            {
                artilleryOption.RHide = ArtilleryRHide;
            }

            // ZOffset
            if (ArtilleryZOffset == ArtilleryOption.ZOffsetMissionDefault)
            {
                ;
            }
            else if (ArtilleryZOffset == ArtilleryOption.ZOffsetRandom)
            {
                artilleryOption.ZOffset = ArtilleryOption.CreateRandomZOffset(Random);
            }
            else
            {
                artilleryOption.ZOffset = ArtilleryZOffset;
            }
        }

        private ESystemType GetRandomSkill()
        {
            int randomIdx = Random.Next(0, SkillsShip.Count());
            return SkillsShip.ElementAt(randomIdx);
        }

        #endregion

#if DEBUG

        [Conditional("DEBUG")]
        public void TraceAssignedGroundGroups()
        {
            foreach (var item in AssigneGroundGroups.Where(x => x.Army == (int)EArmy.Red).OrderBy(x => x.Position.x))
            {
                GroundGroupWaypoint way = item.Waypoints.FirstOrDefault();
                if (way != null)
                {
                    Debug.WriteLine("Name={0} Army={1}, Id={2} Pos=({3:F2},{4:F2}) V={5:F2}, Class={6}, Country={7}, Options={8}, Type={9}",
                                item.DisplayName, item.Army, item.Id, way.X, way.Y, way.V, item.Class, item.Country, item.Options, item.Type);
                }
            }
            foreach (var item in AssigneGroundGroups.Where(x => x.Army == (int)EArmy.Blue).OrderBy(x => x.Position.x))
            {
                GroundGroupWaypoint way = item.Waypoints.FirstOrDefault();
                if (way != null)
                {
                    Debug.WriteLine("Name={0} Army={1}, Id={2} Pos=({3:F2},{4:F2}) V={5:F2}, Class={6}, Country={7}, Options={8}, Type={9}",
                                item.DisplayName, item.Army, item.Id, way.X, way.Y, way.V, item.Class, item.Country, item.Options, item.Type);
                }
            }
        }
#endif
    }
}
