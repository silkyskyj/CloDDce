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
using IL2DCE.MissionObjectModel;
using maddox.game;
using maddox.game.world;
using maddox.GP;
using part;

namespace IL2DCE
{
    namespace Mission
    {
        abstract public class Mission : AMission
        {

            #region definition 

            private const int ProcSecInterval = 25;
            private const string MsgProcFormat = "{0}{1} {2}";
            private const string MsgProcSecFormat = "{0}{1}ing now [{2}sec]";
            private const string MsgBulletsReArm = "Bullets re-Arm";
            private const string MsgReFuel = "re-Fuel";
            private const string MsgProcCompletedFormat = "Completed!";

            #endregion

            protected abstract Core Core
            {
                get;
            }

            protected IGame Game
            {
                get
                {
                    return (GamePlay as IGame);
                }
            }

            public string PlayerActorName
            {
                get;
                set;
            }

            public Dictionary<string, List<DamagerScore>> ActorDead
            {
                get;
            }

            public List<AircraftState> AircraftLanded
            {
                get;
            }

#if DEBUG
            public List<AiAirGroup> AirGroups
            {
                get;
            }
#endif

            private double TimeGameLatest = 0;

            public Mission()
            {
                Debug.WriteLine("Mission.Mission()");
                ActorDead = new Dictionary<string, List<DamagerScore>>();
                AircraftLanded = new List<AircraftState>();
#if DEBUG
                AirGroups = new List<AiAirGroup>();
#endif
            }

            #region AMission Override

            public override bool IsMissionListener(int missionNumber)
            {
                return base.IsMissionListener(missionNumber);
            }

            public override double GetBattleLengthTicks()
            {
                return base.GetBattleLengthTicks();
            }

            public override void OnTickGame()
            {
                base.OnTickGame();

                ITime time = Core.GamePlay.gpTime();
                if ((time.current() - TimeGameLatest) > ProcSecInterval)
                {
                    // TraceAirGroupInfo();
                    // TraceGameInfo();
                    ProcessLandedAircrafts();
                    TimeGameLatest = time.current();
                }
            }

            public override void OnTickReal()
            {
                base.OnTickReal();
            }

            public override void Timeout(double sec, DoTimeout doTimeout)
            {
                base.Timeout(sec, doTimeout);
            }

            public override object[] OnIntraMissionsMessage(string sMsg, object[] args = null)
            {
                Debug.WriteLine("Mission.OnIntraMissionsMessage({0}, {1})", sMsg, args != null ? args.Length : 0);
                return base.OnIntraMissionsMessage(sMsg, args);
            }

            public override void OnTrigger(int missionNumber, string shortName, bool active)
            {
                Debug.WriteLine("Mission.OnTrigger({0}, {1}, {2})", missionNumber, shortName, active);
                base.OnTrigger(missionNumber, shortName, active);

                AiAction action = GamePlay.gpGetAction(ActorName.Full(missionNumber, shortName));
                if (action != null)
                {
                    action.Do();
                }

                AiTrigger trigger = GamePlay.gpGetTrigger(shortName);
                if (trigger != null)
                {
                    trigger.Enable = false;
                }
            }

            #region Battle & Mission

            public override void Inited()
            {
                Debug.WriteLine("Mission.Inited()");
                base.Inited();
            }

            public override void Init(ABattle battle, int missionNumber)
            {
                Debug.WriteLine("Mission.Init({0}, {1})", battle.ToString(), missionNumber);
                base.Init(battle, missionNumber);
                if (Core != null)
                {
                    Core.Mission = this;
                }
            }

            public override void OnBattleInit()
            {
                Debug.WriteLine("Mission.OnBattleInit()");
                base.OnBattleInit();
            }

            public override void OnBattleStarted()
            {
                Debug.WriteLine("Mission.OnBattleStarted()");
                base.OnBattleStarted();
#if DEBUG
                TraceGameInfo();
#endif
                Career career = Core.CurrentCareer;
                if (career.TrackRecording && !Game.gameInterface.TrackRecording())
                {
                    string trackFile = string.Format("{0}/{1}_{2}{3}", Config.RecordFolder, career.CampaignInfo.Id, DateTime.Now.ToString("yyyyMMdd_HHmmss"), Config.RecordExt);
                    Game.gameInterface.TrackRecordStart(Game.gameInterface.ToFileSystemPath(trackFile));
                }
            }

            public override void OnBattleStoped()
            {
                Debug.WriteLine("Mission.OnBattleStoped()");
                base.OnBattleStoped();

                if (Game.gameInterface.TrackRecording())
                {
                    Game.gameInterface.TrackRecordStop();
                }
            }

            public override void OnMissionLoaded(int missionNumber)
            {
                Debug.WriteLine(string.Format("Mission.OnMissionLoaded({0})", missionNumber));
                base.OnMissionLoaded(missionNumber);
            }

            public override void OnSingleBattleSuccess(bool success)
            {
                Debug.WriteLine(string.Format("Mission.OnSingleBattleSuccess({0})", success));
                base.OnSingleBattleSuccess(success);
            }

            #endregion

            #region Player

            public override void OnPlayerConnected(Player player)
            {
                Debug.WriteLine(string.Format("Mission.OnPlayerConnected({0}", player.Name()));
                base.OnPlayerConnected(player);
            }

            public override void OnPlayerDisconnected(Player player, string diagnostic)
            {
                Debug.WriteLine("Mission.OnPlayerDisconnected({0}, {1})", player.Name(), diagnostic);
                base.OnPlayerDisconnected(player, diagnostic);
            }

            public override void OnPlayerArmy(Player player, int army)
            {
                Debug.WriteLine("Mission.OnPlayerArmy({0}, {1}) Place={2},  PersonPrimary={3}",
                    player.Name(), army, player.Place() != null ? player.Place().Name() : "null", player.PersonPrimary() != null ? player.PersonPrimary().Name() : "null");
                base.OnPlayerArmy(player, army);
            }

            public override void OnPlaceEnter(Player player, AiActor actor, int placeIndex)
            {
                Debug.WriteLine("Mission.OnPlaceEnter({0}, {1}, {2})", player.Name(), actor!= null ? actor.Name(): string.Empty, placeIndex);
                base.OnPlaceEnter(player, actor, placeIndex);

                if (actor is AiCart)
                {
                    // 0:tobruk:Tobruk_LW_JG53_10.001 -> tobruk:Tobruk_LW_JG53_10.001
                    // _human(0).0:tobruk:Tobruk_LW_JG53_10.001 -> tobruk:Tobruk_LW_JG53_10.001
                    string name = actor.Name();
                    int idx = name.IndexOf(":");
                    if (idx != -1)
                    {
                        PlayerActorName = name.Substring(idx + 1);
                    }
                }
            }

            public override void OnPlaceLeave(Player player, AiActor actor, int placeIndex)
            {
                Debug.WriteLine("Mission.OnPlaceLeave({0}, {1}, {2})", player.Name(), actor != null ? actor.Name() : string.Empty, placeIndex);
                base.OnPlaceLeave(player, actor, placeIndex);
            }

            public override void OnOrderMissionMenuSelected(Player player, int ID, int menuItemIndex)
            {
                Debug.WriteLine("Mission.OnOrderMissionMenuSelected(Name={0}, ID={1}, Menu={2})", player.Name(), ID, menuItemIndex);
                base.OnOrderMissionMenuSelected(player, ID, menuItemIndex);
            }

            #endregion

            #region Actor

            public override void OnActorCreated(int missionNumber, string shortName, AiActor actor)
            {
                base.OnActorCreated(missionNumber, shortName, actor);
#if DEBUG
                TraceActorCreated(missionNumber, shortName, actor);
#endif
            }

            public override void OnActorDestroyed(int missionNumber, string shortName, AiActor actor)
            {
                Debug.WriteLine("Mission.OnActorDestroyed({0}, {1}, {2}, {3}, Valid={4}, Alive={5}, TaskComplete={6})", missionNumber, shortName, actor.Name(),
                    actor is AiAircraft ? "Aircraft" : actor is AiGroundActor ? "AiGroundActor" : string.Empty, actor.IsValid(), actor.IsAlive(), actor.IsTaskComplete());
                base.OnActorDestroyed(missionNumber, shortName, actor);

#if DEBUG
                TraceActorDestroyed(missionNumber, shortName, actor);
#endif
            }

            public override void OnActorDamaged(int missionNumber, string shortName, AiActor actor, AiDamageInitiator initiator, NamedDamageTypes damageType)
            {
                //Debug.WriteLine("Mission.OnActorDamaged({0}, {1}, {2}, {3}, {4})", 
                //    missionNumber, shortName, actor.Name(), initiator.Player != null ? initiator.Player.Name(): string.Empty, damageType.ToString());
                base.OnActorDamaged(missionNumber, shortName, actor, initiator, damageType);
            }

            public override void OnActorDead(int missionNumber, string shortName, AiActor actor, List<DamagerScore> damages)
            {
                Debug.WriteLine("Mission.OnActorDead({0}, {1}, {2}, {3}, Valid={4}, Alive={5}, TaskComplete={6}))",
                    missionNumber, shortName, actor.Name(),
                        // string.Join("|", damages.Where(x => x.initiator != null && x.initiator.Player != null).Select(x => string.Format("{0}[{1}]", x.score, x.initiator.Player.Name()))),
                        string.Join("|", damages.Where(x => x.initiator != null).Select(x => string.Format("{0}[{1}]", x.score, x.initiator.Player != null ? x.initiator.Player.Name() : x.initiator.Person != null ? x.initiator.Person.Name() : string.Empty))),
                        actor.IsValid(), actor.IsAlive(), actor.IsTaskComplete());
                base.OnActorDead(missionNumber, shortName, actor, damages);

                if (actor is AiAircraft || actor is AiGroundActor)
                {
                    string key = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                                                actor.Army(), PlayerStats.ActorDeadInfoSplitChar,                   // Army
                                                actor is AiAircraft ? 0 : 1, PlayerStats.ActorDeadInfoSplitChar,    // ActorType
                                                shortName, PlayerStats.ActorDeadInfoSplitChar,                      // Actor Name
                                                (actor as AiCart).InternalTypeName());                  // Actor Type Name
                    if (ActorDead.ContainsKey(key))
                    {
                        ActorDead[key].AddRange(damages);
                    }
                    else
                    {
                        ActorDead.Add(key, damages);
                    }
                }
            }

            public override void OnActorTaskCompleted(int missionNumber, string shortName, AiActor actor)
            {
                Debug.WriteLine("Mission.OnActorTaskCompleted({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                base.OnActorTaskCompleted(missionNumber, shortName, actor);
            }

            #endregion

            #region Aircraft

            public override void OnAircraftDamaged(int missionNumber, string shortName, AiAircraft aircraft, AiDamageInitiator initiator, NamedDamageTypes damageType)
            {
                //Debug.WriteLine("Mission.OnAircraftDamaged({0}, {1}, {2}, {3}, {4})",
                //    missionNumber, shortName, aircraft.InternalTypeName(), initiator.Player != null ? initiator.Player.Name() : string.Empty, damageType.ToString());
                base.OnAircraftDamaged(missionNumber, shortName, aircraft, initiator, damageType);
            }

            public override void OnAircraftLimbDamaged(int missionNumber, string shortName, AiAircraft aircraft, AiLimbDamage limbDamage)
            {
                base.OnAircraftLimbDamaged(missionNumber, shortName, aircraft, limbDamage);
            }

            public override void OnAircraftCutLimb(int missionNumber, string shortName, AiAircraft aircraft, AiDamageInitiator initiator, LimbNames limbName)
            {
                base.OnAircraftCutLimb(missionNumber, shortName, aircraft, initiator, limbName);
            }

            public override void OnAircraftTookOff(int missionNumber, string shortName, AiAircraft aircraft)
            {
                Debug.WriteLine("Mission.OnAircraftTookOff({0}, {1}, {2})", missionNumber, shortName, aircraft.InternalTypeName());
                base.OnAircraftTookOff(missionNumber, shortName, aircraft);

                Career career = Core.CurrentCareer;
                if (career.ReArmTime >= 0 || career.ReFuelTime >= 0)
                {
                    var result = AircraftLanded.Where(x => string.Compare(aircraft.Name(), x.Aircraft.Name(), true) == 0);
                    if (result.Count() == 1)
                    {
                        result.First().IsLanded = false;
                    }
                }
            }

#if false
            /// <summary>
            /// React on the AircraftTookOff event.
            /// </summary>
            /// <param name="missionNumber"></param>
            /// <param name="shortName"></param>
            /// <param name="aircraft"></param>
            /// <remarks>
            /// Remove the player from the aircraft for a few ms. This is a workaround needed so that AI aicraft do not stay on the ground after the human player took off.
            /// </remarks>
            public override void OnAircraftTookOff(int missionNumber, string shortName, AiAircraft aircraft)
            {
                base.OnAircraftTookOff(missionNumber, shortName, aircraft);

                if (aircraft.Player(0) != null)
                {
                    Player player = aircraft.Player(0);

                    player.PlaceLeave(0);

                    Timeout(0.1, () =>
                    {
                        player.PlaceEnter(aircraft, 0);
                    });
                }
            }
#endif

            public override void OnAircraftLanded(int missionNumber, string shortName, AiAircraft aircraft)
            {
                Debug.WriteLine("Mission.OnAircraftLanded({0}, {1}, {2})", missionNumber, shortName, aircraft.InternalTypeName());
                base.OnAircraftLanded(missionNumber, shortName, aircraft);

                Career career = Core.CurrentCareer;
                if (career.ReArmTime >= 0 || career.ReFuelTime >= 0)
                {
                    var result = AircraftLanded.Where(x => string.Compare(aircraft.Name(), x.Aircraft.Name(), true) == 0);
                    if (result.Any())
                    {
                        result.Select(x => x.IsLanded = true);
                    }
                    else
                    {
                        AircraftLanded.Add(new AircraftState(aircraft) { IsLanded = true });
                    }
                }
            }

            public override void OnAircraftCrashLanded(int missionNumber, string shortName, AiAircraft aircraft)
            {
                Debug.WriteLine("Mission.OnAircraftCrashLanded({0}, {1}, {2})", missionNumber, shortName, aircraft.InternalTypeName());
                base.OnAircraftCrashLanded(missionNumber, shortName, aircraft);
            }

            public override void OnAircraftKilled(int missionNumber, string shortName, AiAircraft aircraft)
            {
                Debug.WriteLine("Mission.OnAircraftKilled({0}, {1}, {2})", missionNumber, shortName, aircraft.InternalTypeName());
                base.OnAircraftKilled(missionNumber, shortName, aircraft);
            }

            #endregion

            #region Person

            public override void OnPersonMoved(AiPerson person, AiActor fromCart, int fromPlaceIndex)
            {
                base.OnPersonMoved(person, fromCart, fromPlaceIndex);
            }

            public override void OnPersonHealth(AiPerson person, AiDamageInitiator initiator, float deltaHealth)
            {
                base.OnPersonHealth(person, initiator, deltaHealth);
            }

            public override void OnPersonParachuteLanded(AiPerson person)
            {
                base.OnPersonParachuteLanded(person);
            }

            public override void OnPersonParachuteFailed(AiPerson person)
            {
                base.OnPersonParachuteFailed(person);
            }

            #endregion

            #region AutoPilot

            public override void OnAutopilotOn(AiActor actor, int placeIndex)
            {
                base.OnAutopilotOn(actor, placeIndex);
            }

            public override void OnAutopilotOff(AiActor actor, int placeIndex)
            {
                base.OnAutopilotOff(actor, placeIndex);
            }

            #endregion

            public override void OnAiAirNewEnemy(AiAirEnemyElement element, int army)
            {
                Debug.WriteLine("Mission.OnAiAirNewEnemy(army={0}, agID={1}, state={2})", element.army, element.agID, element.state);
                base.OnAiAirNewEnemy(element, army);
            }

            public override void OnBombExplosion(string title, double mass, Point3d pos, AiDamageInitiator initiator, int eventArgInt)
            {
                Debug.WriteLine("Mission.OnBombExplosion({0}, {1}, {2}, {3})", title, mass, initiator.Player != null ? initiator.Player.Name() : string.Empty, eventArgInt);
                base.OnBombExplosion(title, mass, pos, initiator, eventArgInt);
            }

            public override void OnCarter(AiActor actor, int placeIndex)
            {
                Debug.WriteLine("Mission.OnCarter({0}, {1})", actor.Name(), placeIndex);
                base.OnAutopilotOn(actor, placeIndex);
            }

            public override void OnBuildingKilled(string title, Point3d pos, AiDamageInitiator initiator, int eventArgInt)
            {
                Debug.WriteLine("Mission.OnBuildingKilled({0}, {1}, {2})", title, initiator.Player != null ? initiator.Player.Name() : string.Empty, eventArgInt);
                base.OnBuildingKilled(title, pos, initiator, eventArgInt);
            }

            public override void OnStationaryKilled(int missionNumber, GroundStationary _stationary, AiDamageInitiator initiator, int eventArgInt)
            {
                Debug.WriteLine("Mission.OnStationaryKilled({0}, {1}, {2}, {3})", missionNumber, _stationary.Name, initiator.Player != null ? initiator.Player.Name() : string.Empty, eventArgInt);
                base.OnStationaryKilled(missionNumber, _stationary, initiator, eventArgInt);
            }

            public override void OnUserCreateUserLabel(GPUserLabel ul)
            {
                Debug.WriteLine("Mission.OnUserCreateUserLabel({0}, {1}, {2}, {3})", ul.time, ul.Text, ul.Player != null ? ul.Player.Name() : string.Empty, ul.type);
                base.OnUserCreateUserLabel(ul);
            }

            public override void OnUserDeleteUserLabel(GPUserLabel ul)
            {
                Debug.WriteLine("Mission.OnUserDeleteUserLabel({0}, {1}, {2}, {3})", ul.time, ul.Text, ul.Player != null ? ul.Player.Name() : string.Empty, ul.type);
                base.OnUserDeleteUserLabel(ul);
            }

            #endregion

            #region Private Implementation

            private void ProcessLandedAircrafts()
            {
                ITime time = Core.GamePlay.gpTime();
                Debug.WriteLine("CheckLandedAircraft() Time={0}[{1}]", time.current(), time.currentReal());
                Career career = Core.CurrentCareer;
                for (int i = AircraftLanded.Count - 1; i >= 0; i--)
                {
                    AircraftState aircraftState = AircraftLanded[i];
                    if (aircraftState.IsLanded)
                    {
                        AiAircraft aircraft = aircraftState.Aircraft;
                        if (!aircraft.IsKilled() && aircraft.IsAlive() && aircraft.IsValid())
                        {
                            Point3d pos = aircraft.Pos();
                            int army = aircraft.Army();
#if DEBUG
                            TraceLandedPosNearAirport(pos, army);
#endif
                            Point3d posAirport;
                            if (army == GamePlay.gpFrontArmy(pos.x, pos.y) && GamePlay.gpAirports().Where(
                                            x => (posAirport = x.Pos()).distance(ref pos) <= x.FieldR() && GamePlay.gpFrontArmy(posAirport.x, posAirport.y) == army).Any())
                            {
                                if (aircraft.getParameter(ParameterTypes.C_Magneto, 0) == 0 && aircraft.getParameter(ParameterTypes.C_Throttle, 0) <= 0/* && aircraft.getParameter(ParameterTypes.Z_VelocityIAS, 0) <= 0*/)
                                {
                                    if (aircraftState.IsStoped)
                                    {
                                        StringBuilder msg = new StringBuilder();
                                        int waitSec = (int)((Math.Max(aircraftState.StopedTime, aircraftState.ReArmedTime) + career.ReArmTime) - time.current());
                                        if (waitSec < 0)
                                        {
                                            aircraft.RearmPlane(true);
                                            aircraftState.ReArmedTime = time.current();
                                            msg.AppendFormat(MsgProcFormat, string.Empty, MsgBulletsReArm, MsgProcCompletedFormat);
                                        }
                                        else
                                        {
                                            msg.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, MsgProcSecFormat, msg.Length > 0 ? " " : string.Empty, MsgBulletsReArm, waitSec);
                                        }

                                        int minFuelPer = aircraft.GetMinimumFuelInPercent();
                                        if (aircraft.GetCurrentFuelQuantityInPercent() < minFuelPer)
                                        {
                                            if ((Math.Max(aircraftState.StopedTime, aircraftState.ReFueledTime) + career.ReFuelTime) < time.current())
                                            {
                                                aircraft.RefuelPlane((minFuelPer + 100) / 2);
                                                aircraftState.ReFueledTime = time.current();
                                                msg.AppendFormat(MsgProcFormat, msg.Length > 0 ? " " : string.Empty, MsgBulletsReArm, MsgProcCompletedFormat);
                                            }
                                            else
                                            {
                                                msg.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, MsgProcSecFormat, msg.Length > 0 ? " " : string.Empty, MsgReFuel, waitSec);
                                            }
                                        }

                                        if (msg.Length > 0)
                                        {
                                            GamePlay.gpHUDLogCenter(msg.ToString());
                                        }
                                    }
                                    else
                                    {
                                        aircraftState.IsStoped = true;
                                        aircraftState.StopedTime = time.current();
                                    }
                                }
                                else
                                {
                                    aircraftState.StopedTime += ProcSecInterval;
                                }
                                continue;
                            }
                        }
                    }
                    AircraftLanded.RemoveAt(i);
                }
            }

            #endregion

            #region Debug & Trace

#if DEBUG

            [Conditional("DEBUG")]
            private void TraceGameInfo()
            {
                string gpDictionaryFilePath = GamePlay.gpDictionaryFilePath;
                AiAirGroup[] aiAirGroupRed = GamePlay.gpAirGroups((int)EArmy.Red);
                AiAirGroup[] aiAirGroupBlue = GamePlay.gpAirGroups((int)EArmy.Blue);
                AiAirport[] aiAirport = GamePlay.gpAirports();
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
                AiBirthPlace[] aiBirthPlace = GamePlay.gpBirthPlaces();
                AiGroundGroup[] aiGroundGroupRed = GamePlay.gpGroundGroups((int)EArmy.Red);
                AiGroundGroup[] aiGroundGroupBlue = GamePlay.gpGroundGroups((int)EArmy.Blue);
                GroundStationary[] groundStationary = GamePlay.gpGroundStationarys();
            }

            [Conditional("DEBUG")]
            private void TraceAirGroupInfo()
            {
                Debug.WriteLine(DateTime.Now);
                AirGroups.ForEach(x =>
                {
                    if (x.IsValid() && x.IsAlive())
                    {
                        Debug.WriteLine("AiAirGroup: ID={0}, Name={1}, NOf={2}, Init={3}, Died={4}, IsValid={5}, IsAlive={6}, Way=[{7}/{8}], Task{9}, Idle={10}, Pos={11}",
                            x.ID(), x.Name(), x.NOfAirc, x.InitNOfAirc, x.DiedAircrafts, x.IsValid(), x.IsAlive(), x.GetCurrentWayPoint(), x.GetWay().Length, x.getTask().ToString(), x.Idle, x.Pos().ToString());
                        //AiWayPoint[] airGroupWay = x.GetWay();
                        //foreach (AiAirWayPoint item in airGroupWay)
                        //{
                        //    Debug.WriteLine("AiAirGroup: Action={0}, P=({1},{2},{3}) V={4}, Target={5}",
                        //        item.Action.ToString(), item.P.x, item.P.y, item.P.z, item.Speed, item.Target != null ? item.Target.Name() : string.Empty);
                        //}
                        AiAircraft aiAircraft = x.GetItems().FirstOrDefault() as AiAircraft;
                        Regiment regiment = aiAircraft.Regiment();
                        Debug.WriteLine("   aiAircraft:InternalTypeName={0}, AircraftType={1}, name={2}, gruppeNumber={3}, Army={4}, IsKilled={5}, IsAlive={6}, IsTaskComplete={7}",
                            aiAircraft.InternalTypeName(), aiAircraft.Type(), regiment.name(), regiment.gruppeNumber(), aiAircraft.Army(), aiAircraft.IsKilled(), aiAircraft.IsAlive(), aiAircraft.IsTaskComplete());

                        AiAirGroup[] enemies = x.enemies();
                        if (enemies != null && enemies.Length > 0)
                        {
                            AiAirGroup enemy = enemies.FirstOrDefault();
                            aiAircraft = enemy.GetItems().FirstOrDefault() as AiAircraft;
                            regiment = aiAircraft.Regiment();
                            Point3d enemyPos = enemy.Pos();
                            Debug.WriteLine("   Enemies: Count={0}, ID={1}, InternalTypeName={2}, AircraftType={3}, name={4}, Pos={5}, Distance={6}",
                                enemies.Length, x.ID(), aiAircraft.InternalTypeName(), aiAircraft.Type(), regiment.name(), enemyPos.ToString(), x.Pos().distance(ref enemyPos));
                        }

                        //foreach (var item in items)
                        //{
                        //        if (item is AiAircraft)
                        //        {
                        //            AiAircraft aiAircraft = item as AiAircraft;
                        //            Regiment regiment = aiAircraft.Regiment();
                        //            Debug.WriteLine("aiAircraft:InternalTypeName={0}, AircraftType={1}, name={2}, gruppeNumber={3}, Army={4}, IsKilled={5}, IsAlive={6}, IsTaskComplete={7}",
                        //                aiAircraft.InternalTypeName(), aiAircraft.Type(), regiment.name(), regiment.gruppeNumber(), aiAircraft.Army(), aiAircraft.IsKilled(), aiAircraft.IsAlive(), aiAircraft.IsTaskComplete());
                        //        }
                        //        else
                        //        {

                        //        }
                        //    }
                        //}
                    }
                }
                );
            }

            [Conditional("DEBUG")]
            private void TraceActorCreated(int missionNumber, string shortName, AiActor actor)
            {
                if (string.Compare(shortName, "NONAME", true) != 0)
                {
                    // Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2})", missionNumber, shortName, actor.Name());

                    if (actor is AiAircraft)
                    {
                        AiAircraft aiAircraft = actor as AiAircraft;
                        Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                        Debug.WriteLine("  aiAircraft: TypedName={0}, Type={1}, InternalTypeName={2}, VariantName={3}, AircraftType={4}",
                            aiAircraft.TypedName(), aiAircraft.Type(), aiAircraft.InternalTypeName(), aiAircraft.VariantName(), aiAircraft.Type());
                        Regiment regiment = aiAircraft.Regiment();
                        Debug.WriteLine("  Regiment: fileNameEmblem={0}, id={1}, name={2}, gruppeNumber={3}, fileNameEmblem={4}, speech={5}",
                            regiment.fileNameEmblem(), regiment.id(), regiment.name(), regiment.gruppeNumber(), regiment.fileNameEmblem(), regiment.speech());
                    }
                    else if (actor is AiGroundActor)
                    {
                        // AiGroundActor aiGroundActor = actor as AiGroundActor;
                        // Debug.WriteLine("AiAIChief: InternalTypeName={0}, Name={1}, Type={2}", aiGroundActor.InternalTypeName(), aiGroundActor.Name(), aiGroundActor.Type());
                    }
                    else if (actor is AiGroup)
                    {
                        if (actor is AiAirGroup)
                        {
                            Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                            AiAirGroup airGroup = actor as AiAirGroup;
                            Debug.WriteLine("  AiAirGroup: ID={0}, Name={1}, NOf={2}, Init={3}, Died={4}, IsValid={5}, IsAlive={6}, Way=[{7}/{8}], Task{9}, Idle={10}, Pos={11}",
                                airGroup.ID(), airGroup.Name(), airGroup.NOfAirc, airGroup.InitNOfAirc, airGroup.DiedAircrafts, airGroup.IsValid(), airGroup.IsAlive(), airGroup.GetCurrentWayPoint(), airGroup.GetWay().Length, airGroup.getTask().ToString(), airGroup.Idle, airGroup.Pos().ToString());
                            AiWayPoint[] airGroupWay = airGroup.GetWay();
                            foreach (AiAirWayPoint item in airGroupWay)
                            {
                                Debug.WriteLine("  AiAirGroup: Action={0}, P=({1},{2},{3}) V={4}, Target={5}",
                                    item.Action.ToString(), item.P.x, item.P.y, item.P.z, item.Speed, item.Target != null ? item.Target.Name() : string.Empty);
                            }
                            AiAirGroupTask task = airGroup.getTask();
                            Debug.WriteLine(string.Format("  AiAirGroupTask: {0}", task.ToString()));

                            AirGroups.Add(airGroup);
                        }
                        else if (actor is AiGroundGroup)
                        {
                            if (actor is AiAIChief)
                            {
                                AiAIChief aiAIChief = actor as AiAIChief;
                                // Debug.WriteLine("AiAIChief: ID={0}, Name={1}, GroupType={2}", aiAIChief.ID(), aiAIChief.Name(), aiAIChief.GroupType());
                            }
                        }
                    }
                    else if (actor is AiPerson)
                    {
                        AiPerson aiPerson = actor as AiPerson;
                        Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                        Debug.WriteLine("  AiPerson: Id={0}, Name={1}, Health={2}", aiPerson.Id, aiPerson.Name(), aiPerson.Health);
                    }
                }
            }

            [Conditional("DEBUG")]
            private void TraceActorDestroyed(int missionNumber, string shortName, AiActor actor)
            {
                if (actor is AiAircraft)
                {
                    AiAircraft aiAircraft = actor as AiAircraft;
                    Debug.WriteLine("Mission.OnActorDestroyed({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                    Debug.WriteLine("  aiAircraft: TypedName={0}, Type={1}, InternalTypeName={2}, VariantName={3}, AircraftType={4}",
                        aiAircraft.TypedName(), aiAircraft.Type(), aiAircraft.InternalTypeName(), aiAircraft.VariantName(), aiAircraft.Type());
                    Regiment regiment = aiAircraft.Regiment();
                    Debug.WriteLine("  Regiment: fileNameEmblem={0}, id={1}, name={2}, gruppeNumber={3}, fileNameEmblem={4}, speech={5}",
                        regiment.fileNameEmblem(), regiment.id(), regiment.name(), regiment.gruppeNumber(), regiment.fileNameEmblem(), regiment.speech());
                }
                else if (actor is AiAirGroup)
                {
                    AiAirGroup airGroup = actor as AiAirGroup;
                    Debug.WriteLine("  AiAirGroup: ID={0}, Name={1}, NOf={2}, Init={3}, Died={4}, IsValid={5}, IsAlive={6}, Way=[{7}/{8}], Task{9}, Idle={10}, Pos={11}",
                        airGroup.ID(), airGroup.Name(), airGroup.NOfAirc, airGroup.InitNOfAirc, airGroup.DiedAircrafts, airGroup.IsValid(), airGroup.IsAlive(), airGroup.GetCurrentWayPoint(), airGroup.GetWay().Length, airGroup.getTask().ToString(), airGroup.Idle, airGroup.Pos().ToString());
                    AiWayPoint[] airGroupWay = airGroup.GetWay();
                }
            }

            [Conditional("DEBUG")]
            private void TraceLandedPosNearAirport(Point3d pos, int army)
            {
                Debug.WriteLine("Pos=({0},{1}) Army={2}", pos.x, pos.y, GamePlay.gpFrontArmy(pos.x, pos.y));
                var landAiports = GamePlay.gpAirports().Where(x => x.Pos().distance(ref pos) < x.FieldR() && GamePlay.gpFrontArmy(x.Pos().x, x.Pos().y) == army);
                foreach (var item in landAiports)
                {
                    Debug.WriteLine("Pos=({0},{1}) Army={2} Distance={3} Name={4}", item.Pos().x, item.Pos().y, item.Army(), item.Pos().distance(ref pos), item.Name());
                }
            }

#endif

            #endregion
        }
    }
}