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
using System.Text;
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;
using maddox.GP;
using static IL2DCE.MissionObjectModel.MissionStatus;

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
                            actor.Name(), aiAircraft.TypedName(), aiAircraft.Type(), MissionActorObj.GetInternalTypeName(aiAircraft), string.Empty, aiAircraft.Type(), aiAircraft.IsAlive(), aiAircraft.IsKilled(), aiAircraft.IsValid());
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
                    //Debug.WriteLine("Mission.OnActorCreated({0,2}, {1,-30}, {2})", missionNumber, shortName, CloDAPIUtil.ActorInfo(actor));
                    //TraceAiAircraft(game, actor as AiAircraft, false);
                }
                else if (actor is AiGroundActor)
                {
                    //Debug.WriteLine("Mission.OnActorCreated({0,2}, {1,-30}, {2})", missionNumber, shortName, CloDAPIUtil.ActorInfo(actor));
                    //TraceAiGroundActor(game, actor as AiGroundActor);
                }
                else if (actor is AiGroup)
                {
                    if (actor is AiAirGroup)
                    {
                        Debug.WriteLine("Mission.OnActorCreated({0,2}, {1,-30}, {2})", missionNumber, shortName, CloDAPIUtil.ActorInfo(actor));
                        AiAirGroup airGroup = actor as AiAirGroup;
                        TraceAiAirGroup(game, actor as AiAirGroup, true, true, false, false, false);
                    }
                    else if (actor is AiGroundGroup)
                    {
                        Debug.WriteLine("Mission.OnActorCreated({0,2}, {1,-30}, {2})", missionNumber, shortName, CloDAPIUtil.ActorInfo(actor));
                        AiGroundGroup groundGroup = actor as AiGroundGroup;
                        TraceAiGroundGroup(game, groundGroup, true, true);
                    }
                }
                else if (actor is AiPerson)
                {
                    //Debug.WriteLine("Mission.OnActorCreated({0,2}, {1,-30}, {2})", missionNumber, shortName, CloDAPIUtil.ActorInfo(actor));
                    //TraceAiPerson(game, actor as AiPerson);
                }
                else
                {
                    //Debug.WriteLine("Mission.OnActorCreated({0,2}, {1,-30}, {2}) Pos={3}",
                    //                        missionNumber, shortName, CloDAPIUtil.ActorInfo(actor), actor.Pos());
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
                Debug.WriteLine("Mission.OnActorDestroyed({0,2}, {1,-30}, {2})", missionNumber, shortName, actor.Name());
                TraceAiAircraft(game, actor as AiAircraft, false);
            }
            else if (actor is AiAirGroup)
            {
                Debug.WriteLine("Mission.OnActorDestroyed({0,2}, {1,-30}, {2})", missionNumber, shortName, actor.Name());
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
                    AiWayPoint[] airGroupWay = CloDAPIUtil.GetWays(airGroup);
                    int way = CloDAPIUtil.GetCurrentWayPoint(airGroup);
                    AiAirGroupTask? task = CloDAPIUtil.GetTask(airGroup);
                    AiActor actorTarget = airGroupWay != null && way != -1 ? (airGroupWay[way] as AiAirWayPoint).Target : null;
                    Debug.WriteLine("  AiAirGroup: Army={0,1}, ID={1,3}, Name={2,-45}, Type={3,-35}, NOf={4,2}, Init={5,2}, Died={6,2}, Valid={7,-5}, Alive={8,-5}, Way=[{9,2}/{10,2}], Task={11,-5}({12,-15}), Idle={13,-5}, Pos={14,-30}, Target={15}",
                        airGroup.Army(), airGroup.ID(), airGroup.Name(), MissionObjBase.CreateClassShortShortName(aiAircraft?.InternalTypeName()) ?? string.Empty, airGroup.NOfAirc, airGroup.InitNOfAirc, airGroup.DiedAircrafts,
                        airGroup.IsValid(), airGroup.IsAlive(), way, airGroupWay != null ? airGroupWay.Length : 0, airGroup.IsTaskComplete(), task != null ? task.Value.ToString() : string.Empty, airGroup.Idle, MissionObjBase.ToString(airGroup.Pos()), actorTarget != null ? CloDAPIUtil.ActorInfo(actorTarget) : string.Empty);
                    if (ways)
                    {
                        if (airGroupWay != null)
                        {
                            foreach (AiAirWayPoint item in airGroupWay)
                            {
                                Debug.WriteLine("    WayPoint: Action={0,-15}, P=({1:F2},{2:F2},{3:F2}) V={4}, Target={5}",
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
                                Debug.WriteLine("    Aircraft: Name={0,-35}, CallSign={1} CallSignNumber={2}, IsValid={3} IsAlive={4}, Pos={5}",
                                    item.Name(), item.CallSign(), item.CallSignNumber(), item.IsValid(), item.IsAlive(), MissionObjBase.ToString(item.Pos()));
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
                                    aiAircraft = actors != null ? actors.FirstOrDefault() as AiAircraft : null;
                                    Debug.WriteLine("    Enemies: ID={0}, Name={1}, TypeName={2}, NOf={3}, Init={4}, Died={5}, IsValid={6}, IsAlive={7}, Way=[{8}/{9}], Task={10}, Idle={11}, Pos={12}",
                                        item.ID(), item.Name(), aiAircraft != null ? MissionActorObj.GetInternalTypeName(aiAircraft) : string.Empty, item.NOfAirc, item.InitNOfAirc, item.DiedAircrafts, item.IsValid(), item.IsAlive(), item.GetWay() != null ? item.GetCurrentWayPoint() : -1, item.GetWay() != null ? item.GetWay().Length : -1, item.getTask().ToString(), item.Idle, MissionObjBase.ToString(item.Pos()));
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
                                        item.ID(), item.Name(), aiAircraft != null ? MissionActorObj.GetInternalTypeName(aiAircraft) : string.Empty, item.NOfAirc, item.InitNOfAirc, item.DiedAircrafts, item.IsValid(), item.IsAlive(), item.GetWay() != null ? item.GetCurrentWayPoint() : -1, item.GetWay() != null ? item.GetWay().Length : -1, item.getTask().ToString(), item.Idle, MissionObjBase.ToString(item.Pos()));
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
                                    aiAircraft = actors != null ? actors.FirstOrDefault() as AiAircraft : null;
                                    Debug.WriteLine("    AttachedGroups: ID={0}, Name={1}, TypeName={2}, NOf={3}, Init={4}, Died={5}, IsValid={6}, IsAlive={7}, Way=[{8}/{9}], Task={10}, Idle={11}, Pos={12}",
                                        item.ID(), item.Name(), aiAircraft != null ? MissionActorObj.GetInternalTypeName(aiAircraft) : string.Empty, item.NOfAirc, item.InitNOfAirc, item.DiedAircrafts, item.IsValid(), item.IsAlive(), item.GetWay() != null ? item.GetCurrentWayPoint() : -1, item.GetWay() != null ? item.GetWay().Length : -1, item.getTask().ToString(), item.Idle, MissionObjBase.ToString(item.Pos()));
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
                    sb.AppendFormat("  AiGroundGroup: Army={0,1}, Name={1,-30}, Type={2,-25}, Count={3,2}, IsAlive={4,-5}, Task={5,-5}",
                        groundGroup.Army(), groundGroup.Name(), aiActor != null ? MissionObjBase.CreateClassShortShortName(aiActor.InternalTypeName()) : string.Empty, aiActors != null ? aiActors.Count() : 0, groundGroup.IsAlive(), groundGroup.IsTaskComplete());
                    if (groundGroup.IsValid())
                    {
                        sb.AppendFormat(", IsValid={0,-5}, Idle={1,-5}, Pos={2,-65}", groundGroup.IsValid(), groundGroup.Idle, MissionObjBase.ToString(groundGroup.Pos()));
                        AiWayPoint[] waysPoints = groundGroup.GetWay();   // null
                        if (waysPoints != null)
                        {
                            sb.AppendFormat(", Way=[{0,2}/{1,2}], Current={2,2}",
                                groundGroup.GetCurrentWayPoint(), waysPoints.Length, waysPoints[groundGroup.GetCurrentWayPoint()].P.ToString());
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
                                sb.AppendFormat("    WayPoint: RoadWidth={0}, P=({1:F2},{2:F2},{3:F2}) V={4}, waitTime={5}, BridgeIdx={6}",
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
                                    item.Name(), item.Health(), item.IsValid(), item.IsAlive(), MissionObjBase.ToString(item.Pos()));
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
                    Debug.WriteLine("  AiAircraft: Army={0,1}, Name={1,-35}, Type={2,-35}, Valid={3,-5}, Alive={4,-5}, Task={5,-5}, Typed={6,-20}, Variant={7,-20}, Pos={8}, CallSignNumber={9}, CallSign={10}, HullNumber={11}, FuelQuantityInPercent={12}",
                        aiAircraft.Army(), aiAircraft.Name(), MissionObjBase.CreateClassShortShortName(MissionActorObj.GetInternalTypeName(aiAircraft)), aiAircraft.IsValid(), aiAircraft.IsAlive(), aiAircraft.IsTaskComplete(),
                        aiAircraft.TypedName(), string.Empty, MissionObjBase.ToString(aiAircraft.Pos()), aiAircraft.CallSignNumber(), aiAircraft.CallSign(), aiAircraft.HullNumber(), string.Empty);

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
                    Debug.WriteLine("  AiGroundActor: Army={0,1}, Name={1,-35}, Type={2,-15}, TypeName={3,-25}, Valid={4,-5}, Alive={5,-5}, Task={6,-5}, Pos={7}, Fuel={8}, Health={9}",
                        aiGroundActor.Army(), aiGroundActor.Name(), aiGroundActor.Type().ToString(), MissionObjBase.CreateClassShortShortName(MissionActorObj.GetInternalTypeName(aiGroundActor)), aiGroundActor.IsValid(), aiGroundActor.IsAlive(), aiGroundActor.IsTaskComplete(),
                        MissionObjBase.ToString(aiGroundActor.Pos()), aiGroundActor.Fuel(), aiGroundActor.Health());
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
                    Debug.WriteLine("  AiPerson: Army={0,1}, Id={1,3}, Name={2,-30}, Player={3,-20}, Health={4,2}, Valid={5,-5}, Alive={6,-5}, Task={7,-5}, Pos={8}",
                        aiPerson.Army(), aiPerson.Id, aiPerson.Name(), aiPerson.Player() != null ? aiPerson.Player().Name() : string.Empty, aiPerson.Health, aiPerson.IsValid(),
                        aiPerson.IsAlive(), aiPerson.IsTaskComplete(), MissionObjBase.ToString(aiPerson.Pos()));
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
                    Debug.WriteLine("  GroundStationary: Country={0,-2}, Title={1,-30}, Name={2,-35}, Type={3,-30}, Category={4,-15}, IsAlive={5,-5}, Pos={6}",
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
            Debug.WriteLine("Pos=({0},{1}) Army={2,1}", pos.x, pos.y, game.gpFrontArmy(pos.x, pos.y));
            var landAiports = game.gpAirports().Where(x => x.Pos().distance(ref pos) < x.FieldR() && game.gpFrontArmy(x.Pos().x, x.Pos().y) == army);
            foreach (var item in landAiports)
            {
                Debug.WriteLine("Pos=({0},{1}) Army={2} Distance={3} Name={4}", item.Pos().x, item.Pos().y, item.Army(), item.Pos().distance(ref pos), item.Name());
            }
        }

        [Conditional("DEBUG")]
        public static void TraceAiAirports(IGame game)
        {
            AiAirport[] aiAirport = game.gpAirports();
            if (aiAirport != null)
            {
                Point3d point;
                foreach (var item in aiAirport)
                {
                    point = item.Pos();
                    AiActor[] queueTakeoff = item.QueueTakeoff();
                    Debug.WriteLine("aiAirport Army={0}[{1}], Pos=({2:F2},{3:F2},{4:F2}) FieldR={5}, CoverageR={6}, Name={7}, Type={8}, ParkCountAll={9}, ParkCountFree={10},",
                        item.Army(), game.gpFrontArmy(point.x, point.y), point.x, point.y, point.z, item.FieldR(), item.CoverageR(), item.Name(), item.Type(), item.ParkCountAll(), item.ParkCountFree());
                    if (queueTakeoff != null)
                    {
                        foreach (var item1 in queueTakeoff)
                        {
                            Debug.WriteLine(string.Format("queueTakeoff: {0}", item1.Name()));
                        }
                    }
                }
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
