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
using System.Linq;
using System.Text;
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;
using maddox.GP;

namespace IL2DCE.MissionObjectModel
{
    public class MissionDebug
    {
        private IGame Game
        {
            get;
            set;
        }

        public MissionDebug(IGame game)
        {
            Game = game;
        }

        #region Debug & Trace

#if DEBUG

        [Conditional("DEBUG")]
        public static void TraceGameInfo(IGame game)
        {
            IPlayer iplayer = game.gameInterface.Player();
            AiPerson person = iplayer.PersonPrimary();
            Debug.WriteLine("Palyer[{0}] Person.Name={1} IsAlive={2}, Health={3} Pos={4}", iplayer.Name(), person?.Name() ?? string.Empty, person?.IsAlive() ?? false, person?.Health ?? 0, person?.Pos().ToString() ?? string.Empty);
            AiPerson person2 = iplayer.PersonSecondary();
            AiActor actor = iplayer.Place();
            if (actor != null)
            {
                AiAircraft aiAircraft = actor as AiAircraft;
                // Regiment regiment = aiAircraft.Regiment();
                Debug.WriteLine("Palyer Actor:{0}, TypedName={1}, Type={2}, InternalTypeName={3}, VariantName={4}, AircraftType={5}, IsAlive={6}, IsKilled={7}, IsValid={8}",
                            actor.Name(), aiAircraft.TypedName(), aiAircraft.Type(), aiAircraft.InternalTypeName(), aiAircraft.VariantName(), aiAircraft.Type(), aiAircraft.IsAlive(), aiAircraft.IsKilled(), aiAircraft.IsValid());
                //Debug.WriteLine("  Regiment: fileNameEmblem={0}, id={1}, name={2}, gruppeNumber={3}, fileNameEmblem={4}, speech={5}",
                //    regiment.fileNameEmblem(), regiment.id(), regiment.name(), regiment.gruppeNumber(), regiment.fileNameEmblem(), regiment.speech());
            }
            IPlayerStat st = iplayer.GetBattleStat();

            string gpDictionaryFilePath = game.gpDictionaryFilePath;
            AiAirGroup[] aiAirGroupRed = game.gpAirGroups((int)EArmy.Red);
            if (aiAirGroupRed != null)
            {
                foreach (var item in aiAirGroupRed)
                {
                    TraceAiAirGroup(game, item, true, true, true, true, true);
                }
            }
            AiAirGroup[] aiAirGroupBlue = game.gpAirGroups((int)EArmy.Blue);
            if (aiAirGroupBlue != null)
            {
                foreach (var item in aiAirGroupBlue)
                {
                    TraceAiAirGroup(game, item, true, true, true, true, true);
                }
            }
            AiAirport[] aiAirport = game.gpAirports();
            if (aiAirport != null)
            {
                Point3d point;
                foreach (var item in aiAirport)
                {
                    point = item.Pos();
                    AiActor[] queueTakeoff = item.QueueTakeoff();
                    if (queueTakeoff != null && queueTakeoff.Length > 0)
                    {
                        Debug.WriteLine("aiAirport Army={0}, Pos=({1:F2},{2:F2},{3:F2}) FieldR={4}, CoverageR={5}, Name={6}, Type={7}, ParkCountAll={8}, ParkCountFree={9},",
                            item.Army(), point.x, point.y, point.z, item.FieldR(), item.CoverageR(), item.Name(), item.Type(), item.ParkCountAll(), item.ParkCountFree());
                        foreach (var item1 in queueTakeoff)
                        {
                            Debug.WriteLine(string.Format("queueTakeoff: {0}", item1.Name()));
                        }
                    }
                }
            }
            AiBirthPlace[] aiBirthPlace = game.gpBirthPlaces();
            AiGroundGroup[] aiGroundGroupRed = game.gpGroundGroups((int)EArmy.Red);
            if (aiGroundGroupRed != null)
            {
                foreach (var item in aiGroundGroupRed)
                {
                    TraceAiGroundGroup(game, item, true, true);
                }
            }
            AiGroundGroup[] aiGroundGroupBlue = game.gpGroundGroups((int)EArmy.Blue);
            if (aiGroundGroupBlue != null)
            {
                foreach (var item in aiGroundGroupBlue)
                {
                    TraceAiGroundGroup(game, item, true, true);
                }
            }
            GroundStationary[] groundStationary = game.gpGroundStationarys();
            if (groundStationary != null)
            {
                foreach (var item in groundStationary)
                {
                    TraceGroundStationary(game, item);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void TraceAirGroupInfo(IGame game, IEnumerable<AiAirGroup> airGroups)
        {
            Debug.WriteLine(DateTime.Now);
            foreach (AiAirGroup item in airGroups)
            {
                TraceAiAirGroup(game, item, false, false, true, true, true);
            }
        }

        [Conditional("DEBUG")]
        public static void TraceAirGroupInfo(IGame game)
        {
            Debug.WriteLine(DateTime.Now);
            AiAirGroup[] aiAirGroupRed = game.gpAirGroups((int)EArmy.Red);
            if (aiAirGroupRed != null)
            {
                foreach (var item in aiAirGroupRed)
                {
                    TraceAiAirGroup(game, item, false, false, true, true, true);
                }
            }
            AiAirGroup[] aiAirGroupBlue = game.gpAirGroups((int)EArmy.Blue);
            if (aiAirGroupBlue != null)
            {
                foreach (var item in aiAirGroupBlue)
                {
                    TraceAiAirGroup(game, item, false, false, true, true, true);
                }
            }
            AiAirGroup[] aiAirGroupNone = game.gpAirGroups((int)EArmy.None);
            if (aiAirGroupNone != null)
            {
                foreach (var item in aiAirGroupNone)
                {
                    TraceAiAirGroup(game, item, false, false, true, true, true);
                }
            }

        }

        [Conditional("DEBUG")]
        public static void TraceActorCreated(IGame game, int missionNumber, string shortName, AiActor actor)
        {
            // if (string.Compare(shortName, MissionStatus.ValueNoName, true) != 0)
            {
                // Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2})", missionNumber, shortName, actor.Name());

                if (actor is AiAircraft)
                {
                    //Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                    //TraceAiAircraft(actor as AiAircraft, false);
                }
                else if (actor is AiGroundActor)
                {
                    //Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                    //TraceAiGroundActor(actor as AiGroundActor);
                }
                else if (actor is AiGroup)
                {
                    if (actor is AiAirGroup)
                    {
                        Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                        AiAirGroup airGroup = actor as AiAirGroup;
                        TraceAiAirGroup(game, actor as AiAirGroup, true, true, false, false, false);
                    }
                    else if (actor is AiGroundGroup)
                    {
                        Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                        AiGroundGroup groundGroup = actor as AiGroundGroup;
                        TraceAiGroundGroup(game, groundGroup, true, true);
                    }
                }
                else if (actor is AiPerson)
                {
                    //Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                    //TraceAiPerson(actor as AiPerson);
                }
            }
            //else
            //{
            //    //Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2}) Army={3}, Tag={4}, AirGroup={5}, Pos={6}, Type={7}", 
            //    //                        missionNumber, shortName, actor.Name(), actor.Army(), actor.Tag != null ? actor.Tag: string.Empty, actor.Group() != null ? actor.Group().Name() :string.Empty, actor.Pos(), actor.GetType().Name);
            //}
        }

        [Conditional("DEBUG")]
        public static void TraceActorDestroyed(IGame game, int missionNumber, string shortName, AiActor actor)
        {
            if (actor is AiAircraft)
            {
                Debug.WriteLine("Mission.OnActorDestroyed({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                TraceAiAircraft(game, actor as AiAircraft, false);
            }
            else if (actor is AiAirGroup)
            {
                Debug.WriteLine("Mission.OnActorDestroyed({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                TraceAiAirGroup(game, actor as AiAirGroup, false, false, false, false, false);
            }
        }

        [Conditional("DEBUG")]
        public static void TraceAiAirGroup(IGame game, AiAirGroup airGroup, bool ways, bool items, bool enemies, bool candidates, bool attachedGroups)
        {
            if (airGroup != null)
            {
                try
                {
                    AiActor[] actors = CloDAPIUtil.GetItems(airGroup);
                    AiAircraft aiAircraft = actors != null ? actors.FirstOrDefault() as AiAircraft : null;
                    Debug.WriteLine("  AiAirGroup: Army={0,1}, ID={1,3}, Name={2,-30}, Type={3,-35}, NOf={4,2}, Init={5,2}, Died={6,2}, Valid={7,-5}, Alive={8,-5}, Way=[{9,2}/{10,2}], Task={11,-15}, Idle={12,-5}, Pos={13,-65}",
                        airGroup.Army(), airGroup.ID(), airGroup.Name(), aiAircraft?.InternalTypeName() ?? string.Empty, airGroup.NOfAirc, airGroup.InitNOfAirc, airGroup.DiedAircrafts,
                        airGroup.IsValid(), airGroup.IsAlive(), airGroup.NOfAirc > 0 ? airGroup.GetCurrentWayPoint() : -1,
                        airGroup.NOfAirc > 0 && airGroup.GetWay() != null ? airGroup.GetWay().Length : 0, airGroup.getTask().ToString(), airGroup.Idle, airGroup.Pos().ToString());
                    if (ways)
                    {
                        AiWayPoint[] airGroupWay = airGroup.GetWay();
                        if (airGroupWay != null)
                        {
                            foreach (AiAirWayPoint item in airGroupWay)
                            {
                                Debug.WriteLine("    WayPoint: Action={0}, P=({1},{2},{3}) V={4}, Target={5}",
                                    item.Action.ToString(), item.P.x, item.P.y, item.P.z, item.Speed, item.Target != null ? item.Target.Name() : string.Empty);
                            }
                        }
                    }
                    if (items)
                    {
                        actors = CloDAPIUtil.GetItems(airGroup);
                        if (actors != null)
                        {
                            foreach (AiAircraft item in actors)
                            {
                                Debug.WriteLine("    Aircraft: Name={0}, CallSign={1} CallSignNumber={2}, IsValid={3} IsAlive={4}, Pos={5}",
                                    item.Name(), item.CallSign(), item.CallSignNumber(), item.IsValid(), item.IsAlive(), item.Pos().ToString());
                            }
                        }
                    }

                    if (enemies)
                    {
                        AiAirGroup[] enemiesGroup = airGroup.enemies();
                        if (enemiesGroup != null)
                        {
                            foreach (var item in enemiesGroup)
                            {
                                if (item.IsValid() && item.IsAlive() && item.NOfAirc > 0)
                                {
                                    actors = CloDAPIUtil.GetItems(item);
                                    aiAircraft = actors != null ? actors.FirstOrDefault() as AiAircraft: null;
                                    Debug.WriteLine("    Enemies: ID={0}, Name={1}, TypeName={2}, NOf={3}, Init={4}, Died={5}, IsValid={6}, IsAlive={7}, Way=[{8}/{9}], Task={10}, Idle={11}, Pos={12}",
                                        item.ID(), item.Name(), aiAircraft != null ? aiAircraft.InternalTypeName() : string.Empty, item.NOfAirc, item.InitNOfAirc, item.DiedAircrafts, item.IsValid(), item.IsAlive(), item.GetWay() != null ? item.GetCurrentWayPoint() : -1, item.GetWay() != null ? item.GetWay().Length : -1, item.getTask().ToString(), item.Idle, item.Pos().ToString());
                                }
                            }
                        }
                    }

                    if (candidates)
                    {
                        AiAirGroup[] candidateGroup = airGroup.candidates();
                        if (candidateGroup != null)
                        {
                            foreach (var item in candidateGroup)
                            {
                                if (item.IsValid() && item.IsAlive() && item.NOfAirc > 0)
                                {
                                    actors = CloDAPIUtil.GetItems(item);
                                    aiAircraft = actors != null ? actors.FirstOrDefault() as AiAircraft : null;
                                    Debug.WriteLine("    Candidates: ID={0}, Name={1}, TypeName={2}, NOf={3}, Init={4}, Died={5}, IsValid={6}, IsAlive={7}, Way=[{8}/{9}], Task={10}, Idle={11}, Pos={12}",
                                        item.ID(), item.Name(), aiAircraft != null ? aiAircraft.InternalTypeName() : string.Empty, item.NOfAirc, item.InitNOfAirc, item.DiedAircrafts, item.IsValid(), item.IsAlive(), item.GetWay() != null ? item.GetCurrentWayPoint() : -1, item.GetWay() != null ? item.GetWay().Length : -1, item.getTask().ToString(), item.Idle, item.Pos().ToString());
                                }
                            }
                        }
                    }

                    if (attachedGroups)
                    {
                        AiAirGroup[] attachedGroup = airGroup.attachedGroups();
                        if (attachedGroup != null)
                        {
                            foreach (var item in attachedGroup)
                            {
                                if (item.IsValid() && item.IsAlive() && item.NOfAirc > 0)
                                {
                                    actors = CloDAPIUtil.GetItems(item);
                                    aiAircraft = actors != null ? actors.FirstOrDefault() as AiAircraft: null;
                                    Debug.WriteLine("    AttachedGroups: ID={0}, Name={1}, TypeName={2}, NOf={3}, Init={4}, Died={5}, IsValid={6}, IsAlive={7}, Way=[{8}/{9}], Task={10}, Idle={11}, Pos={12}",
                                        item.ID(), item.Name(), aiAircraft != null ? aiAircraft.InternalTypeName() : string.Empty, item.NOfAirc, item.InitNOfAirc, item.DiedAircrafts, item.IsValid(), item.IsAlive(), item.GetWay() != null ? item.GetCurrentWayPoint() : -1, item.GetWay() != null ? item.GetWay().Length : -1, item.getTask().ToString(), item.Idle, item.Pos().ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void TraceAiGroundGroup(IGame game, AiGroundGroup groundGroup, bool ways, bool items)
        {
            if (groundGroup != null)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    AiActor[] aiActors = CloDAPIUtil.GetItems(groundGroup);
                    AiGroundActor aiActor = aiActors != null ? aiActors.FirstOrDefault() as AiGroundActor : null;
                    sb.AppendFormat("  AiGroundGroup: Army={0,1}, Name={1,-30}, Type={2,-35}, Count={3,2}, IsAlive={4,-5}",
                        groundGroup.Army(),groundGroup.Name(), aiActor != null ? aiActor.InternalTypeName() : string.Empty, aiActors != null ? aiActors.Count() : 0, groundGroup.IsAlive());
                    if (groundGroup.IsValid())
                    {
                        sb.AppendFormat(", IsValid={0,-5}, Idle={1,-5}, Pos={2,-65}", groundGroup.IsValid(), groundGroup.Idle, groundGroup.Pos().ToString());
                        AiWayPoint[] waysPoints = groundGroup.GetWay();   // null
                        if (waysPoints != null)
                        {
                            sb.AppendFormat(", Way=[{0,2}/{1,2}], Current{2,2}",
                                groundGroup.GetCurrentWayPoint(), waysPoints.Length, waysPoints[groundGroup.GetCurrentWayPoint()].ToString());
                        }
                    }
                    sb.AppendLine();
                    if (ways)
                    {
                        AiWayPoint[] WayPoints = groundGroup.GetWay();
                        if (WayPoints != null)
                        {
                            foreach (AiGroundWayPoint item in WayPoints)
                            {
                                sb.AppendFormat("    WayPoint: RoadWidth={0}, P=({1},{2},{3}) V={4}, waitTime={5}, BridgeIdx={6}",
                                    item.roadWidth.ToString(), item.P.x, item.P.y, item.P.z, item.Speed, item.waitTime, item.BridgeIdx);
                                sb.AppendLine();
                            }
                        }
                    }
                    if (items)
                    {
                        AiActor[] actors = CloDAPIUtil.GetItems(groundGroup);
                        if (actors != null)
                        {
                            foreach (AiGroundActor item in actors)
                            {
                                sb.AppendFormat("    GroundActor: Name={0}, Health={1}, IsValid={2} IsAlive={3}, Pos={4}",
                                    item.Name(), item.Health(), item.IsValid(), item.IsAlive(), item.Pos().ToString());
                                sb.AppendLine();
                            }
                        }
                    }
                    Debug.Write(sb.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0} {1}", ex.Message, ex.StackTrace);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void TraceAiAircraft(IGame game, AiAircraft aiAircraft, bool Regiment)
        {
            if (aiAircraft != null)
            {
                try
                {
                    Debug.WriteLine("  AiAircraft: Army={0}, Name={1}, Type={2}, Valid={3}, Alive={4}, Task={5}, Typed={6}, Variant={7}, Pos={8}, CallSignNumber={9}, CallSign={10}, HullNumber={11}, FuelQuantityInPercent={12}",
                        aiAircraft.Army(), aiAircraft.Name(), aiAircraft.InternalTypeName(), aiAircraft.IsValid(), aiAircraft.IsAlive(), aiAircraft.IsTaskComplete(),
                        aiAircraft.TypedName(), aiAircraft.VariantName(), aiAircraft.Pos(), aiAircraft.CallSignNumber(), aiAircraft.CallSign(), aiAircraft.HullNumber(), aiAircraft.GetCurrentFuelQuantityInPercent());

                    if (Regiment)
                    {
                        Regiment regiment = aiAircraft.Regiment();
                        if (regiment != null)
                        {
                            Debug.WriteLine("    Regiment: fileNameEmblem={0}, id={1}, name={2}, gruppeNumber={3}, fileNameEmblem={4}, speech={5}",
                                regiment.fileNameEmblem(), regiment.id(), regiment.name(), regiment.gruppeNumber(), regiment.fileNameEmblem(), regiment.speech());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void TraceAiGroundActor(IGame game, AiGroundActor aiGroundActor)
        {
            if (aiGroundActor != null)
            {
                try
                {
                    Debug.WriteLine("  AiGroundActor: Army={0}, Name={1}, Type={2}, TypeName={3}, Valid={4}, Alive={5}, Task={6}, Pos={7}, Fuel={8}, Health={9}",
                        aiGroundActor.Army(), aiGroundActor.Name(), aiGroundActor.Type().ToString(), aiGroundActor.InternalTypeName(), aiGroundActor.IsValid(), aiGroundActor.IsAlive(), aiGroundActor.IsTaskComplete(),
                        aiGroundActor.Pos(), aiGroundActor.Fuel(), aiGroundActor.Health());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void TraceAiPerson(IGame game, AiPerson aiPerson)
        {
            if (aiPerson != null)
            {
                try
                {
                    Debug.WriteLine("  AiPerson: Army={0}, Id={1}, Name={2}, Player={3}, Health={4}, Valid={5}, Alive={6}, Task={7}, Pos={8}",
                        aiPerson.Army(), aiPerson.Id, aiPerson.Name(), aiPerson.Player() != null ? aiPerson.Player().Name() : string.Empty, aiPerson.Health, aiPerson.IsValid(),
                        aiPerson.IsAlive(), aiPerson.IsTaskComplete(), aiPerson.Pos());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void TraceGroundStationary(IGame game, GroundStationary groundStationary)
        {
            if (groundStationary != null)
            {
                try
                {
                    Debug.WriteLine("  GroundStationary: Country={0}, Title={1}, Name={2}, Type={3}, Category={4}, IsAlive={5}, Pos={6}",
                    groundStationary.country, groundStationary.Title, groundStationary.Name, groundStationary.Type, groundStationary.Category, groundStationary.IsAlive, groundStationary.pos);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0} {1}", ex.Message, ex.StackTrace);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void TraceLandedPosNearAirport(IGame game, Point3d pos, int army)
        {
            Debug.WriteLine("Pos=({0},{1}) Army={2}", pos.x, pos.y, game.gpFrontArmy(pos.x, pos.y));
            var landAiports = game.gpAirports().Where(x => x.Pos().distance(ref pos) < x.FieldR() && game.gpFrontArmy(x.Pos().x, x.Pos().y) == army);
            foreach (var item in landAiports)
            {
                Debug.WriteLine("Pos=({0},{1}) Army={2} Distance={3} Name={4}", item.Pos().x, item.Pos().y, item.Army(), item.Pos().distance(ref pos), item.Name());
            }
        }

        [Conditional("DEBUG")]
        public static void Trace(IGame game, Dictionary<string, object> dataDictionary)
        {
            Debug.WriteLine(string.Format("Trace DataDictionary Count={0}", dataDictionary.Count));
            foreach (var item in dataDictionary)
            {
                Debug.WriteLine("  Key={0} Value={1}", item.Key, item.Value);
            }
        }
#endif

        #endregion
    }
}
