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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;
using maddox.GP;
using part;
using static IL2DCE.MissionObjectModel.MissionStatus;

namespace IL2DCE.MissionObjectModel
{
    interface ISpawnDynamicProcArgs
    {
        IGame Game
        {
            get;
        }

        IRandom Random
        {
            get;
        }

        Config Config
        {
            get;
        }

        Career Career
        {
            get;
        }

        IMissionStatus MissionStatus
        {
            get;
        }
    }

    public class MissionProc : ISpawnDynamicProcArgs
    {
        #region Definition

        private const int TimeAfterTaskComplate = 120;
        private const int LandedIASMax = 50;

        private const string MsgProcFormat = "{0}{1} {2}";
        private const string MsgProcSecFormat = "{0}{1}ing now [{2}sec]";
        private const string MsgBulletsReArm = "Bullets re-Arm";
        private const string MsgReFuel = "re-Fuel";
        private const string MsgProcCompletedFormat = "Completed!";

        #endregion

        #region Property

        public IGame Game
        {
            get;
            private set;
        }

        public IRandom Random
        {
            get;
            private set;
        }

        public Config Config
        {
            get;
            private set;
        }

        public Career Career
        {
            get;
            private set;
        }

        public IMissionStatus MissionStatus
        {
            get;
            private set;
        }

        public bool IsBusy
        {
            get
            {
                return worker != null && worker.IsBusy;
            }
        }

        #endregion

        #region Variable

        private BackgroundWorker worker;
        private string spawnFilePath;
        private object spawnFilePathObject = new object();

        #endregion

        public MissionProc(IGame game, IRandom random, Config config, Career career, IMissionStatus missionStatus)
        {
            Game = game;
            Config = config;
            Random = random;
            Career = career;
            MissionStatus = missionStatus;

            worker = null;
            spawnFilePath = string.Empty;
        }

        public void SpawnDynamic()
        {
            if (Career.SpawnDynamicAirGroups || Career.SpawnDynamicGroundGroups || Career.SpawnDynamicStationaries)
            {
                lock (spawnFilePathObject)
                {
                    if (!string.IsNullOrEmpty(spawnFilePath))
                    {
                        //ISectionFile file = Game.gpLoadSectionFile(spawnFilePath);
                        //Game.gpPostMissionLoad(file);
                        Game.gpPostMissionLoad(spawnFilePath);
#if !DEBUG && false
                        FileUtil.DeleteFile(Game.gameInterface, spawnFilePath);
#endif
                        spawnFilePath = string.Empty;
                    }
                    else
                    {
                        if (worker == null)
                        {
                            worker = new BackgroundWorker();
                            worker.WorkerSupportsCancellation = true;
                            worker.DoWork += DoWorkSpawnDynamic;
                            worker.RunWorkerCompleted += RunWorkerCompletedSpawnDynamic;
                            worker.RunWorkerAsync(this);
                        }
                        else
                        {
                            ;
                        }
                    }
                }
            }
        }

        public void Cancel()
        {
            if (IsBusy)
            {
                worker.CancelAsync();
            }
        }

        private void DoWorkSpawnDynamic(object sender, DoWorkEventArgs e)
        {
            ISpawnDynamicProcArgs args = e.Argument as ISpawnDynamicProcArgs;

            try
            {
                Generator.Generator generator = new Generator.Generator(args.Game, args.Random, args.Config, args.Career);
                ISectionFile missionFile;
                BriefingFile briefingFile;
                if (generator.GenerateSubMission(args.MissionStatus, out missionFile, out briefingFile, e))
                {
                    string missionFilePath = string.Format("{0}/{1}/{2}{3}", Config.UserMissionFolder, args.Career.PilotName, Config.DynamicSpawnFileName, Config.MissionFileExt);
                    FileUtil.BackupFiles(args.Game.gameInterface.ToFileSystemPath(missionFilePath), 5, false);
                    missionFile.save(missionFilePath);
                    string briefingFilePath = string.Format("{0}/{1}/{2}{3}", Config.UserMissionFolder, args.Career.PilotName, Config.DynamicSpawnFileName, Config.BriefingFileExt);
                    string briefingFileSystemPath = args.Game.gameInterface.ToFileSystemPath(briefingFilePath);
                    FileUtil.BackupFiles(briefingFileSystemPath, 5, false);
                    briefingFile.SaveTo(briefingFileSystemPath, Config.DynamicSpawnFileName);
                    e.Result = missionFilePath;
                }
                else
                {
                    e.Result = null;
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("DoWorkSpawnDynamic Error[{0} {1}]", ex.Message, ex.StackTrace);
                Core.WriteLog(message);
                e.Result = null;
            }
        }

        private void RunWorkerCompletedSpawnDynamic(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null && e.Result != null)
            {
                lock (spawnFilePathObject)
                {
                    spawnFilePath = e.Result as string;
                }
            }
            worker.Dispose();
            worker = null;
        }

        public void UpdateTasks()
        {
            // AirGroup
            AiAirGroup[] aiAirGroupRed = Game.gpAirGroups((int)EArmy.Red);
            if (aiAirGroupRed != null)
            {
                foreach (var item in aiAirGroupRed)
                {
                    UpdateTask(item);
                }
            }

            AiAirGroup[] aiAirGroupBlue = Game.gpAirGroups((int)EArmy.Blue);
            if (aiAirGroupBlue != null)
            {
                foreach (var item in aiAirGroupBlue)
                {
                    UpdateTask(item);
                }
            }

            AiAirGroup[] aiAirGroupNone = Game.gpAirGroups((int)EArmy.None);
            if (aiAirGroupNone != null)
            {
                foreach (var item in aiAirGroupNone)
                {
                    UpdateTask(item);
                }
            }

            AiGroundGroup[] aiGroundGroupRed = Game.gpGroundGroups((int)EArmy.Red);
            if (aiGroundGroupRed != null)
            {
                foreach (var item in aiGroundGroupRed)
                {
                    UpdateTask(item);
                }
            }

            AiGroundGroup[] aiGroundGroupBlue = Game.gpGroundGroups((int)EArmy.Blue);
            if (aiGroundGroupBlue != null)
            {
                foreach (var item in aiGroundGroupBlue)
                {
                    UpdateTask(item);
                }
            }

            AiGroundGroup[] aiGroundGroupNone = Game.gpGroundGroups((int)EArmy.Blue);
            if (aiGroundGroupNone != null)
            {
                foreach (var item in aiGroundGroupNone)
                {
                    UpdateTask(item);
                }
            }
        }

        private void UpdateTask(AiActor aiActor)
        {
            if (aiActor is AiAircraft)
            {
                UpdateTask(aiActor as AiAircraft);
            }
            else if (aiActor is AiGroundActor)
            {
                UpdateTask(aiActor as AiGroundActor);
            }
            else if (aiActor is AiAirGroup)
            {
                UpdateTask(aiActor as AiAirGroup);
            }
            else if (aiActor is AiGroundGroup)
            {
                UpdateTask(aiActor as AiGroundGroup);
            }
            else if (aiActor is AiPerson)
            {
                UpdateTask(aiActor as AiPerson);
            }
        }

        #region AirGroup

        private void UpdateTask(AiAirGroup aiAirGroup)
        {
            if (aiAirGroup.IsValid() && aiAirGroup.NOfAirc > 0)
            {
                AiActor[] actors = CloDAPIUtil.GetItems(aiAirGroup);
                if (aiAirGroup.IsAlive() || (actors != null && actors.Any(x => x.IsAlive())))
                {
                    int army = aiAirGroup.Army();
                    AiAirGroupTask task = aiAirGroup.getTask();
                    AiWayPoint[] ways = CloDAPIUtil.GetWays(aiAirGroup);
                    int way = CloDAPIUtil.GetCurrentWayPoint(aiAirGroup);
                    if (ways != null && way >= 0 && way < ways.Length)
                    {
                        AiActor actorTarget = ways != null ? (ways[aiAirGroup.GetCurrentWayPoint()] as AiAirWayPoint).Target : null;
                        if (way == 0 && (task == AiAirGroupTask.UNKNOWN || task == AiAirGroupTask.DO_NOTHING) && IsLanded(aiAirGroup))
                        {
                            AiAirWayPoint wayPoint = (ways[0] as AiAirWayPoint);
                            AiAirGroupTask taskNew = ConvertWayActionToTask(wayPoint.Action);
                            aiAirGroup.setTask(ConvertWayActionToTask(wayPoint.Action), aiAirGroup);
                            Debug.WriteLine(string.Format("Change Task({0})={1} => {2}", CloDAPIUtil.ActorInfo(aiAirGroup), task.ToString(), taskNew.ToString()));
                        }
                        else if (way == ways.Length - 1 && aiAirGroup.IsTaskComplete())
                        {
                            AirGroupObj airGroupObj = MissionStatus.AirGroups.Where(x => x.Id == aiAirGroup.ID()).FirstOrDefault();
                            if (airGroupObj != null)
                            {
                                if (!airGroupObj.IsMissionCompleted)
                                {
                                    if (airGroupObj.TaskComplateTime == 0 && airGroupObj.RequestTask == AiAirGroupTask.UNKNOWN)
                                    {
                                        airGroupObj.TaskComplateTime = (int)Game.gpTime().current();
                                        airGroupObj.RequestTask = AiAirGroupTask.LANDING;
                                    }
                                    else if ((int)Game.gpTime().current() - airGroupObj.TaskComplateTime >= TimeAfterTaskComplate)
                                    {
                                        if (airGroupObj.RequestTask == AiAirGroupTask.LANDING)
                                        {
                                            UpdateTaskLanging(aiAirGroup, airGroupObj);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    UpdateTask(actors.Select(x => x as AiAircraft));
                }
                else
                {
                    AiAirGroup playerGroup = CloDAPIUtil.PlayerAirGroup(Game);
                    Debug.Assert(playerGroup != null);
                    if (Config.GroupNotAliveToDestroy && string.Compare(aiAirGroup.Name(), ValueNoName, true) == 0 && playerGroup != aiAirGroup)
                    {
                        Destroy(aiAirGroup);
                    }
                }
            }
        }

        private void UpdateTaskLanging(AiAirGroup aiAirGroup, AirGroupObj airGroupObj)
        {
            int army = aiAirGroup.Army();
            AiAirGroupTask task = aiAirGroup.getTask();
            AiWayPoint[] ways = CloDAPIUtil.GetWays(aiAirGroup);
            AiActor actorTarget = ways != null ? (ways[aiAirGroup.GetCurrentWayPoint()] as AiAirWayPoint).Target : null;
            if (IsLanded(aiAirGroup))
            {
                airGroupObj.IsMissionCompleted = true;
                airGroupObj.TaskComplateTime = 0;
                Debug.WriteLine(string.Format("AirGroup.IsMissionCompleted = true, Landed({0})={1}", CloDAPIUtil.ActorInfo(aiAirGroup), task.ToString()));
            }
            else if (task != AiAirGroupTask.LANDING && task != AiAirGroupTask.UNKNOWN && !(actorTarget is AiAirport))
            {
                AiAirport aiAirport = GetNearestAirPort(aiAirGroup.Pos(), army);
                if (aiAirport != null)
                {
                    Debug.Write(string.Format("Change Task({0})={1} => {2}", CloDAPIUtil.ActorInfo(aiAirGroup), task.ToString(), "LandingWay"));
                    aiAirGroup.changeGoalTarget(aiAirport);
                    aiAirGroup.SetWay(CreateLandingWayPoints(aiAirGroup.Pos(), aiAirport, false));
#if true
                    aiAirGroup.setTask(AiAirGroupTask.RETURN, aiAirGroup);
#endif
#if DEBUG
                    MissionDebug.TraceAiAirGroup(Game, aiAirGroup, true, true, true, true, true);
#endif
                }
            }
        }

        private void Destroy(AiAirGroup aiAirGroup)
        {
            AiActor[] aiActors = CloDAPIUtil.GetItems(aiAirGroup);
            if (aiActors != null)
            {
                foreach (var item in aiActors)
                {
                    if (item is AiAircraft)
                    {
                        Debug.WriteLine(string.Format("Destroy AiAircraft => {0}", CloDAPIUtil.ActorInfo(item)));
                        (item as AiAircraft).Destroy();
                    }
                }
            }
        }

        private void UpdateTask(IEnumerable<AiAircraft> aiAircrafts)
        {
            int reArmTime = Career.ReArmTime;
            int reFuelTime = Career.ReFuelTime;
            foreach (AiAircraft aiAircraft in aiAircrafts)
            {
                string name = MissionObjBase.CreateShortName(aiAircraft.Name());
                AircraftObj aircraft = MissionStatus.Aircrafts.Where(x => string.Compare(x.Name, name) == 0).FirstOrDefault();
                if (aircraft != null)
                {
                    if (aircraft.IsLanded)
                    {
                        if (aircraft.IsValid && aircraft.IsAlive)
                        {
                            if (reArmTime >= 0 || reFuelTime >= 0)
                            {
#if Blitz
                                UpdateReArmReFuel(aiAircraft, aircraft);
#endif
                            }
                            else
                            {
                                if (AircraftObj.IsStop(aiAircraft))
                                {
                                    ITime time = Game.gpTime();
                                    if (aircraft.IsStoped && aircraft.StopedTime > 0)
                                    {
                                        AiAirGroupTask? task = AircraftObj.GetCurrentTask(aiAircraft);
                                        if (AircraftObj.IsLastWaypoint(aiAircraft) && task != null && task.HasValue && task.Value == AiAirGroupTask.UNKNOWN)
                                        {
                                            if (time.current() - aircraft.StopedTime > Config.MissionCompletedTime)
                                            {
                                                aircraft.IsMissionCompleted = true; // -> Safe Destroyed
                                                Debug.WriteLine("aircraft.IsMissionCompleted = true({0})", aircraft.Name);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        aircraft.IsStoped = true;
                                        aircraft.StopedTime = time.current();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

#if Blitz

        private void UpdateReArmReFuel(AiAircraft aiAircraft, AircraftObj aircraft)
        {
            Point3d pos = aircraft.Point;
            Point3d posAirport;
            int army = aiAircraft.Army();
            if (army == Game.gpFrontArmy(pos.x, pos.y) && Game.gpAirports().Where(
                            x => (posAirport = x.Pos()).distance(ref pos) <= x.FieldR() && Game.gpFrontArmy(posAirport.x, posAirport.y) == army).Any())
            {
                if (AircraftObj.IsStop(aiAircraft))
                {
                    ITime time = Game.gpTime();
                    if (aircraft.IsStoped)
                    {
                        StringBuilder msg = new StringBuilder();
                        int reArmTime = Career.ReArmTime;
                        if (reArmTime >= 0 && !aircraft.IsReArmed)
                        {
                            int waitSec = (int)((aircraft.StopedTime + reArmTime) - time.current());
                            if (waitSec < 0)
                            {
                                aiAircraft.RearmPlane(true);
                                aircraft.IsReArmed = true;
                                msg.AppendFormat(MsgProcFormat, string.Empty, MsgBulletsReArm, MsgProcCompletedFormat);
                            }
                            else
                            {
                                msg.AppendFormat(Config.NumberFormat, MsgProcSecFormat, msg.Length > 0 ? " " : string.Empty, MsgBulletsReArm, waitSec);
                            }
                        }

                        int reFuelTime = Career.ReFuelTime;
                        if (reFuelTime >= 0 && !aircraft.IsReFueled)
                        {
                            int minFuelPer = aiAircraft.GetMinimumFuelInPercent();
                            if (aiAircraft.GetCurrentFuelQuantityInPercent() < minFuelPer)
                            {
                                int waitSec = (int)((aircraft.StopedTime + reFuelTime) - time.current());
                                if (waitSec < 0)
                                {
                                    aiAircraft.RefuelPlane((minFuelPer + 100) / 2);
                                    aircraft.IsReFueled = true;
                                    msg.AppendFormat(MsgProcFormat, msg.Length > 0 ? " " : string.Empty, MsgBulletsReArm, MsgProcCompletedFormat);
                                }
                                else
                                {
                                    msg.AppendFormat(Config.NumberFormat, MsgProcSecFormat, msg.Length > 0 ? " " : string.Empty, MsgReFuel, waitSec);
                                }
                            }
                        }
                                
                        if (PlayerObj.IsPlayer(aiAircraft)/* && string.Compare(aircraft.Name, MissionStatus.PlayerInfo.Type, true) == 0*/ && msg.Length > 0)
                        {
                            Game.gpHUDLogCenter(msg.ToString());
                        }
                    }
                    else
                    {
                        aircraft.IsStoped = true;
                        aircraft.StopedTime = time.current();
                    }
                }
                else
                {
                    aircraft.StopedTime += Config.ProcessInterval;
                }
            }
        }

#endif

        private bool IsLanded(AiAirGroup aiAirGroup)
        {
            AiActor[] actors = CloDAPIUtil.GetItems(aiAirGroup);
            if (actors != null)
            {
                return actors.Where(x => IsLanded(x as AiAircraft)).Count() == actors.Length;
            }
            return false;
        }

        private bool IsLanded(AiAircraft aiAircraft)
        {
            if (aiAircraft != null)
            {
                Debug.WriteLine("Aircraft.IsLanded {0}={1},{2},{3},{4},{5},{6},{7},{8}", aiAircraft.Name(),
                    aiAircraft.getParameter(ParameterTypes.C_Magneto, 0), aiAircraft.getParameter(ParameterTypes.C_Magneto, -1), aiAircraft.getParameter(ParameterTypes.C_Throttle, 0), aiAircraft.getParameter(ParameterTypes.C_Throttle, -1),
                    aiAircraft.getParameter(ParameterTypes.Z_VelocityIAS, 0), aiAircraft.getParameter(ParameterTypes.Z_VelocityIAS, -1), aiAircraft.getParameter(ParameterTypes.Z_VelocityTAS, 0), aiAircraft.getParameter(ParameterTypes.Z_VelocityTAS, -1));
                return aiAircraft.getParameter(ParameterTypes.Z_VelocityIAS, -1) <= 0/* || aiAircraft.getParameter(ParameterTypes.Z_VelocityTAS, -1) <= 0*/;
            }
            return false;
        }

        private AiAirGroupTask ConvertWayActionToTask(AiAirWayPointType type)
        {
            AiAirGroupTask task;
            switch (type)
            {
                case AiAirWayPointType.NORMFLY:
                    task = AiAirGroupTask.FLY_WAYPOINT;
                    break;

                case AiAirWayPointType.TAKEOFF:
                    task = AiAirGroupTask.TAKEOFF;
                    break;

                case AiAirWayPointType.LANDING:
                    task = AiAirGroupTask.LANDING;
                    break;

                case AiAirWayPointType.GATTACK_TARG:
                    task = AiAirGroupTask.ATTACK_GROUND;
                    break;

                case AiAirWayPointType.GATTACK_POINT:
                    task = AiAirGroupTask.ATTACK_GROUND;
                    break;

                case AiAirWayPointType.AATTACK_BOMBERS:
                    task = AiAirGroupTask.ATTACK_AIR;
                    break;

                case AiAirWayPointType.AATTACK_FIGHTERS:
                    task = AiAirGroupTask.ATTACK_AIR;
                    break;

                case AiAirWayPointType.HUNTING:
                    task = AiAirGroupTask.ATTACK_AIR;
                    break;

                case AiAirWayPointType.FOLLOW:
                    task = AiAirGroupTask.FLY_WAYPOINT;
                    break;

                case AiAirWayPointType.ESCORT:
                    task = AiAirGroupTask.FLY_WAYPOINT;
                    break;

                case AiAirWayPointType.COVER:
                    task = AiAirGroupTask.FLY_WAYPOINT;
                    break;

                case AiAirWayPointType.RECON:
                    task = AiAirGroupTask.FLY_WAYPOINT;
                    break;

                default:
                    task = AiAirGroupTask.FLY_WAYPOINT;
                    break;
            }
            return task;
        }

        #endregion

        private AiAirport GetNearestAirPort(Point3d pos, int army)
        {
            AiAirport aiAirport = null;
            double distanceNear = Double.MaxValue;
            Point3d p;
            foreach (var item in Game.gpAirports().Where(x => { p = x.Pos(); return Game.gpFrontArmy(p.x, p.y) == army; }))
            {
                double d = item.Pos().distance(ref pos);
                if (d < distanceNear)
                {
                    aiAirport = item;
                    distanceNear = d;
                }
            }
            return aiAirport;
        }

        #region GroundGroup

        private void UpdateTask(AiGroundGroup aiGroundGroup)
        {
            AiActor[] actors = CloDAPIUtil.GetItems(aiGroundGroup);
            if (aiGroundGroup.IsValid() && aiGroundGroup.IsAlive() && actors != null && actors.Where(x => x.IsAlive()).Any())
            {
                int army = aiGroundGroup.Army();
                AiWayPoint[] ways = aiGroundGroup.GetWay();
                int way = aiGroundGroup.GetCurrentWayPoint();
                if (ways != null && way == ways.Length - 1 && aiGroundGroup.IsTaskComplete())
                {
                    ;
                }
            }
        }

        #endregion

        private void UpdateTask(AiAircraft aiAircraft)
        {
            if (aiAircraft.IsValid() && aiAircraft.IsAlive())
            {
                int army = aiAircraft.Army();
                if (IsLanded(aiAircraft))
                {

                }
            }
        }

        private void UpdateTask(AiGroundActor aiGroundActor)
        {
            if (aiGroundActor.IsValid() && aiGroundActor.IsAlive())
            {
                int army = aiGroundActor.Army();

                ;
            }
        }

        private void UpdateTask(AiPerson aiPerson)
        {
            if (aiPerson.IsValid() && aiPerson.IsAlive())
            {
                int army = aiPerson.Army();

                ;
            }
        }

        private AiAirWayPoint[] CreateLandingWayPoints(Point3d pos, AiAirport landingAirport, bool plusRandomValue = false)
        {
            List<AiAirWayPoint> lists = new List<AiAirWayPoint>();
            lists.AddRange(CreateLandingInbetweenPoints(pos, landingAirport, plusRandomValue));
            lists.Add(CreateLandingWayPoint(landingAirport));
            return lists.ToArray();
        }

        private AiAirWayPoint CreateLandingWayPoint(AiAirport landingAirport)
        {
            Point3d pos = landingAirport.Pos();
            return new AiAirWayPoint(ref pos, AirGroupWaypoint.DefaultLandingV) { Action = AiAirWayPointType.LANDING, Target = landingAirport };
        }

        private IEnumerable<AiAirWayPoint> CreateLandingInbetweenPoints(Point3d target, AiAirport landingAirport, bool plusRandomValue = false)
        {
            List<AiAirWayPoint> lists = new List<AiAirWayPoint>();
            Point3d point = new Point3d(landingAirport.Pos().x, landingAirport.Pos().y, target.z);
            lists.AddRange(CreateInbetweenWayPoints(target, point, plusRandomValue));
            return lists;
        }

        private IEnumerable<AiAirWayPoint> CreateInbetweenWayPoints(Point3d from, Point3d to, bool plusRandomValue = false)
        {
            List<AiAirWayPoint> lists = new List<AiAirWayPoint>();
            double mpX = (to.x - from.x);
            double mpY = (to.y - from.y);
            double mpZ = (to.z - from.z);
            if (plusRandomValue)
            {
                mpX *= Random.Next(75, 125) / 100.0;
                mpY *= Random.Next(75, 125) / 100.0;
            }
            Point3d p1 = new Point3d(from.x + 0.25 * mpX, from.y + 0.25 * mpY, from.z + 1.00 * mpZ);
            Point3d p2 = new Point3d(from.x + 0.50 * mpX, from.y + 0.50 * mpY, from.z + 1.00 * mpZ);
            Point3d p3 = new Point3d(from.x + 0.75 * mpX, from.y + 0.75 * mpY, from.z + 1.00 * mpZ);

            lists.Add(new AiAirWayPoint(ref p1, AirGroupWaypoint.DefaultNormalflyV) { Action = AiAirWayPointType.NORMFLY });
            lists.Add(new AiAirWayPoint(ref p2, AirGroupWaypoint.DefaultNormalflyV) { Action = AiAirWayPointType.NORMFLY });
            lists.Add(new AiAirWayPoint(ref p3, AirGroupWaypoint.DefaultNormalflyV) { Action = AiAirWayPointType.NORMFLY });

            return lists;
        }
    }
}
