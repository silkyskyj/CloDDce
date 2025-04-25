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

using System.Diagnostics;
using System.Linq;
using maddox.game.world;

namespace IL2DCE.MissionObjectModel
{
    internal class MissionProc
    {
        private IGame Game
        {
            get;
            set;
        }

        MissionProc(IGame game)
        {
            Game = game;
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
            if (aiGroundGroup.IsValid() && aiGroundGroup.IsAlive() && aiGroundGroup.GetItems() != null && aiGroundGroup.GetItems().Where(x => x.IsAlive()).Any())
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
