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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;

namespace IL2DCE.MissionObjectModel
{
    public class MissionProc
    {
        #region Definition

        class SpawnDynamicProcArgs
        {
            public IGame Game
            {
                get;
                set;
            }

            public IRandom Random
            {
                get;
                set;
            }

            public Config Config
            {
                get;
                set;
            }

            public Career Career
            {
                get;
                set;
            }

            public MissionFile MissionFile
            {
                get;
                set;
            }

            public IMissionStatus MissionStatus
            {
                get;
                set;
            }
        }

        #endregion

        #region Property

        private IGame Game
        {
            get;
            set;
        }

        private IRandom Random
        {
            get;
            set;
        }

        private Config Config
        {
            get;
            set;
        }

        private Career Career
        {
            get;
            set;
        }

        #endregion

        #region Variable

        private MissionFile missionFile;
        private BackgroundWorker worker;
        private string spawnFilePath;
        private object spawnFileNameObject = new object();

        #endregion

        public MissionProc(IGame game, IRandom random, Config config, Career career)
        {
            Game = game;
            Config = config;
            Random = random;
            Career  = career;

            worker = null;
            spawnFilePath = string.Empty;

            missionFile = new MissionFile(Game, new string[] { career.MissionFileName }, career.AirGroupInfos, 
                career.SpawnDynamicStationaries ? MissionFile.LoadLevel.AirGroundGroupUnit : career.SpawnDynamicGroundGroups ? MissionFile.LoadLevel.AirGroundGroup : MissionFile.LoadLevel.AirGroup);
        }

        public void SpawnDynamic(IMissionStatus missionStatus)
        {
            if (Career.SpawnDynamicAirGroups || Career.SpawnDynamicGroundGroups || Career.SpawnDynamicStationaries)
            {
                lock (spawnFileNameObject)
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
                            worker.RunWorkerAsync(new SpawnDynamicProcArgs()
                            {
                                Game = Game,
                                Random = Random,
                                Config = Config,
                                Career = Career,
                                MissionFile = missionFile,
                                MissionStatus = missionStatus,
                            });
                        }
                        else
                        {
                            ;
                        }
                    }
                }
            }
        }

        private void DoWorkSpawnDynamic(object sender, DoWorkEventArgs e)
        {
            SpawnDynamicProcArgs args = e.Argument as SpawnDynamicProcArgs;

            try
            {
                Generator.Generator generator = new Generator.Generator(args.Game, args.Random, args.Config, args.Career);
                ISectionFile missionFile;
                BriefingFile briefingFile;
                if (generator.GenerateSubMission(args.MissionStatus, out missionFile, out briefingFile))
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
            catch(Exception ex)
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
                lock (spawnFileNameObject)
                {
                    spawnFilePath = e.Result as string;
                }
            }
            worker.Dispose();
            worker = null;
        }

        private void UpdateTasks()
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

        private void UpdateTask(AiAirGroup aiAirGroup)
        {
            if (aiAirGroup.IsValid() && aiAirGroup.IsAlive() && aiAirGroup.NOfAirc > 0)
            {
                int army = aiAirGroup.Army();
                AiAirGroupTask task = aiAirGroup.getTask();
                AiWayPoint[] ways = aiAirGroup.GetWay();
                int way = aiAirGroup.GetCurrentWayPoint();
                // AiAirGroup palyerAirGroup = PalyerAirGroup();
                if (ways != null && way <= ways.Length - 1 && way == 0 && /*&& aiAirGroup .IsTaskComplete() ||*/ (task == AiAirGroupTask.UNKNOWN || task == AiAirGroupTask.DO_NOTHING))
                {
                    Debug.Write(string.Format("Task{0}={1}", aiAirGroup.Name(), task.ToString()));
                    AiAirWayPointType wayAction = (ways[0] as AiAirWayPoint).Action;
                    AiAirGroupTask taskNew = ConvertWayActionToTask(wayAction);
                    aiAirGroup.setTask(taskNew, null);
                    task = aiAirGroup.getTask();
                    Debug.WriteLine(" => {0}", task.ToString());
                }
            }
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

        private void UpdateTask(AiGroundGroup aiGroundGroup)
        {
            AiActor [] actors = CloDAPIUtil.GetItems(aiGroundGroup);
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

        private void UpdateTask(AiAircraft aiAircraft)
        {
            if (aiAircraft.IsValid() && aiAircraft.IsAlive())
            {
                int army = aiAircraft.Army();

                ;
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
    }
}
