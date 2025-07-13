// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC
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
using System.Threading;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
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

            #region Definition 

            private const int ProcBeginSec = 150;

            #endregion

            #region Property

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
                private set;
            }

            public Dictionary<string, List<DamagerScore>> ActorDead
            {
                get;
            }

            public List<AircraftState> AircraftLanded
            {
                get;
            }

            public MissionStatus MissionStatus
            {
                get;
                private set;
            }

            public MissionProc MissionProc
            {
                get;
                private set;
            }

            #endregion

            #region Variable

#if DEBUG
            private List<AiAirGroup> AirGroups = new List<AiAirGroup>();
#endif

            private double TimeGameLatest = 0;

            #endregion

            public Mission()
            {
                Debug.WriteLine("Mission.Mission()");
                ActorDead = new Dictionary<string, List<DamagerScore>>();
            }

            #region AMission Override

            public override bool IsMissionListener(int missionNumber)
            {
                base.IsMissionListener(missionNumber);
                return true;
            }

            public override double GetBattleLengthTicks()
            {
                return base.GetBattleLengthTicks();
            }

            public override void OnTickGame()
            {
                base.OnTickGame();

                ITime time = Game.gpTime();
                if ((time.current() - TimeGameLatest) > Core.Config.ProcessInterval)
                {
#if DEBUG
                    MissionDebug.TraceAirGroupInfo(Game);
#endif
                    if (MissionStatus != null)
                    {
                        MissionStatus.Update(Game, GameEventId.Trigger, PlayerActorName, Core.CurrentCareer.Date.Value);
                    }

                    if (time.current() > ProcBeginSec)
                    {
                        ProcSpawnDynamic();
                        ProcUpdateTasks();
                    }

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

                AiAction action = Game.gpGetAction(ActorName.Full(missionNumber, shortName));
                if (action != null)
                {
                    action.Do();
                }

                AiTrigger trigger = Game.gpGetTrigger(shortName);
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

                if (((IGame)battle.GamePlay).Core.Config.NoCheckBattleGoal)
                {
                    IGamePlay gamePlay = battle.GamePlay;
                    battle = new MissionObjectModel.Battle();
                    battle.Init(gamePlay);
                }
                base.Init(battle, missionNumber);

                if (Core != null)
                {
                    Core.Mission = this;
                    MissionStatus = new MissionStatus(Core.Random, Core.CurrentCareer.Date.Value);
                    MissionProc = new MissionProc(Game, Core.Random, Core.Config, Core.CurrentCareer, MissionStatus);
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

                Career career = Core.CurrentCareer;

#if DEBUG
                // MissionDebug.Trace(Game, DataDictionary);
                Core.SaveCurrentStatus(Config.MissionStatusStartFileName, PlayerActorName, career.Date.Value, true);

#if false
                MissionDebug.TraceAiAirports(Game);
                MissionDebug.TraceGameInfo(Game);
#endif
#endif
                if (MissionStatus != null)
                {
                    MissionStatus.Update(Game, GameEventId.BattleStarted, PlayerActorName, career.Date.Value);
                }

                // UpdateTasks();

                if (career.TrackRecording && !Game.gameInterface.TrackRecording())
                {
                    string trackFile = string.Format("{0}/{1}_{2}{3}", Config.RecordFolder, career.CampaignInfo.Id, DateTime.Now.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat), Config.RecordFileExt);
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

                if (MissionStatus != null)
                {
                    Career career = Core.CurrentCareer;
                    MissionStatus.Update(Game, GameEventId.BattleStoped, PlayerActorName, career.Date.Value.AddSeconds(Game.gpTime().current()));
#if DEBUG
                    // Trace(DataDictionary);
                    Core.SaveCurrentStatus(Config.MissionStatusEndFileName, PlayerActorName, career.Date.Value.AddSeconds(Game.gpTime().current()), true);
#endif
                }

                if (MissionProc != null)
                {
                    MissionProc.Cancel();
                    while (MissionProc.IsBusy)
                    {
                        Thread.Sleep(0);
                    }
                }

#if false
                TraceGameInfo();
#endif

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
                Debug.WriteLine("Mission.OnPlaceEnter({0}, {1}, {2})", player.Name(), CloDAPIUtil.ActorInfo(actor), placeIndex);
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
                        if (MissionStatus != null)
                        {
                            MissionStatus.Update(player, PlayerActorName);
                        }
                    }
                }
            }

            public override void OnPlaceLeave(Player player, AiActor actor, int placeIndex)
            {
                Debug.WriteLine("Mission.OnPlaceLeave({0}, {1}, {2})", player.Name(), CloDAPIUtil.ActorInfo(actor), placeIndex);
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
                MissionDebug.TraceActorCreated(Game, missionNumber, shortName, actor);
#endif
                if (MissionStatus != null)
                {
                    MissionStatus.Update(actor, GameEventId.ActorCreated, false);
                }
            }

            public override void OnActorDestroyed(int missionNumber, string shortName, AiActor actor)
            {
                Debug.WriteLine("Mission.OnActorDestroyed({0}, {1}, {2}, {3}, Valid={4}, Alive={5}, Task={6})", missionNumber, shortName, CloDAPIUtil.ActorInfo(actor),
                    actor is AiAircraft ? "Aircraft" : actor is AiGroundActor ? "AiGroundActor" : string.Empty, actor.IsValid(), actor.IsAlive(), actor.IsTaskComplete());
                base.OnActorDestroyed(missionNumber, shortName, actor);
#if DEBUG
                //MissionDebug.TraceActorDestroyed(Game, missionNumber, shortName, actor);
#endif
                if (MissionStatus != null)
                {
                    MissionStatus.Update(actor, GameEventId.ActorDestroyed);
                }
            }

            public override void OnActorDamaged(int missionNumber, string shortName, AiActor actor, AiDamageInitiator initiator, NamedDamageTypes damageType)
            {
                //Debug.WriteLine("Mission.OnActorDamaged({0}, {1}, {2}, {3}, {4})", 
                //    missionNumber, shortName, actor.Name(), initiator.Player != null ? initiator.Player.Name(): string.Empty, damageType.ToString());
                base.OnActorDamaged(missionNumber, shortName, actor, initiator, damageType);
            }

            public override void OnActorDead(int missionNumber, string shortName, AiActor actor, List<DamagerScore> damages)
            {
                Debug.WriteLine("Mission.OnActorDead({0}, {1}, Actor={2}, Damege:{3}, Valid={4}, Alive={5}, Task={6}, Army={7}))",
                    missionNumber, shortName, CloDAPIUtil.ActorInfo(actor), string.Join("|", damages.Where(x => x.initiator != null).Select(x => string.Format("{0}={1}", CloDAPIUtil.GetName(x.initiator), x.score))),
                        actor.IsValid(), actor.IsAlive(), actor.IsTaskComplete(), actor.Army());
                base.OnActorDead(missionNumber, shortName, actor, damages);

                if (actor is AiAircraft || actor is AiGroundActor)
                {
                    string key = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                                                actor.Army(), PlayerStats.ActorDeadInfoSplitChar,                   // Army
                                                actor is AiAircraft ? 0 : 1, PlayerStats.ActorDeadInfoSplitChar,    // ActorType
                                                shortName, PlayerStats.ActorDeadInfoSplitChar,                      // Actor Name
                                                (actor as AiCart).InternalTypeName());                              // Actor Type Name
                    if (ActorDead.ContainsKey(key))
                    {
                        ActorDead[key].AddRange(damages);
                    }
                    else
                    {
                        ActorDead.Add(key, damages);
                    }
                }

                if (MissionStatus != null)
                {
                    MissionStatus.Update(actor, GameEventId.ActorDead);
                }
            }

            public override void OnActorTaskCompleted(int missionNumber, string shortName, AiActor actor)
            {
                Debug.WriteLine("Mission.OnActorTaskCompleted({0}, {1}, {2})", missionNumber, shortName, CloDAPIUtil.ActorInfo(actor));
                base.OnActorTaskCompleted(missionNumber, shortName, actor);
                if (MissionStatus != null)
                {
                    MissionStatus.Update(actor, GameEventId.ActorTaskCompleted, false);
                }

                // UpdateTask(actor);
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

                if (MissionStatus != null)
                {
                    MissionStatus.Update(aircraft, GameEventId.AircraftTookOff, false);
                }
            }

            public override void OnAircraftLanded(int missionNumber, string shortName, AiAircraft aircraft)
            {
                Debug.WriteLine("Mission.OnAircraftLanded({0}, {1}, {2})", missionNumber, shortName, aircraft.InternalTypeName());
                base.OnAircraftLanded(missionNumber, shortName, aircraft);

                if (MissionStatus != null)
                {
                    MissionStatus.Update(aircraft, GameEventId.AircraftLanded);
                }
            }

            public override void OnAircraftCrashLanded(int missionNumber, string shortName, AiAircraft aircraft)
            {
                Debug.WriteLine("Mission.OnAircraftCrashLanded({0}, {1}, {2})", missionNumber, shortName, aircraft.InternalTypeName());
                base.OnAircraftCrashLanded(missionNumber, shortName, aircraft);

                if (MissionStatus != null)
                {
                    MissionStatus.Update(aircraft, GameEventId.AircraftCrashLanded);
                }
            }

            public override void OnAircraftKilled(int missionNumber, string shortName, AiAircraft aircraft)
            {
                Debug.WriteLine("Mission.OnAircraftKilled({0}, {1}, {2})", missionNumber, shortName, aircraft.InternalTypeName());
                base.OnAircraftKilled(missionNumber, shortName, aircraft);

                if (MissionStatus != null)
                {
                    MissionStatus.Update(aircraft, GameEventId.AircraftKilled);
                }
            }

            #endregion

            #region Person

            public override void OnPersonMoved(AiPerson person, AiActor fromCart, int fromPlaceIndex)
            {
                Debug.WriteLine("Mission.OnPersonMoved({0}, {1}, {2})", person.Name(), fromCart != null ? fromCart.Name() : string.Empty, fromPlaceIndex);
                base.OnPersonMoved(person, fromCart, fromPlaceIndex);
            }

            public override void OnPersonHealth(AiPerson person, AiDamageInitiator initiator, float deltaHealth)
            {
                Debug.WriteLine("Mission.OnPersonHealth({0}, {1}, {2})", person.Name(), initiator != null ? CloDAPIUtil.GetName(initiator) : string.Empty, deltaHealth);
                base.OnPersonHealth(person, initiator, deltaHealth);
            }

            public override void OnPersonParachuteLanded(AiPerson person)
            {
                Debug.WriteLine("Mission.OnPersonParachuteLanded({0}, {1}, {2})", person.Name(), person.Id, person.Health);
#if DEBUG
                MissionDebug.TraceAiPerson(Game, person);
#endif
                base.OnPersonParachuteLanded(person);

                if (MissionStatus != null)
                {
                    MissionStatus.Update(person, GameEventId.PersonParachuteLanded);
                }
            }

            public override void OnPersonParachuteFailed(AiPerson person)
            {
                Debug.WriteLine("Mission.OnPersonParachuteFailed({0}, {1}, {2})", person.Name(), person.Id, person.Health);
#if DEBUG
                MissionDebug.TraceAiPerson(Game, person);
#endif
                base.OnPersonParachuteFailed(person);

                if (MissionStatus != null)
                {
                    MissionStatus.Update(person, GameEventId.PersonParachuteFailed);
                }
            }

            #endregion

            #region AutoPilot

            public override void OnAutopilotOn(AiActor actor, int placeIndex)
            {
                Debug.WriteLine("Mission.OnAutopilotOn({0}, {1})", CloDAPIUtil.ActorInfo(actor), placeIndex);
                base.OnAutopilotOn(actor, placeIndex);
            }

            public override void OnAutopilotOff(AiActor actor, int placeIndex)
            {
                Debug.WriteLine("Mission.OnAutopilotOff({0}, {1})", CloDAPIUtil.ActorInfo(actor), placeIndex);
                base.OnAutopilotOff(actor, placeIndex);
            }

            #endregion

            public override void OnAiAirNewEnemy(AiAirEnemyElement element, int army)
            {
                //Debug.WriteLine("Mission.OnAiAirNewEnemy(army={0}, agID={1}, state={2})", element.army, element.agID, element.state);
                base.OnAiAirNewEnemy(element, army);
#if DEBUG && false
                AiAirGroup aiAirGroup = Game.gpAiAirGroup(element.agID, element.army);
                if (aiAirGroup != null)
                {
                    MissionDebug.TraceAiAirGroup(Game, aiAirGroup, false, false, false, false, false);
                }
#endif
            }

            public override void OnBombExplosion(string title, double mass, Point3d pos, AiDamageInitiator initiator, int eventArgInt)
            {
                // Debug.WriteLine("Mission.OnBombExplosion({0}, {1}, {2}, {3})", title, mass, initiator != null ? CloDAPIUtil.GetName(initiator) : string.Empty, eventArgInt);
                base.OnBombExplosion(title, mass, pos, initiator, eventArgInt);
            }

            public override void OnCarter(AiActor actor, int placeIndex)
            {
                Debug.WriteLine("Mission.OnCarter({0}, {1})", CloDAPIUtil.ActorInfo(actor), placeIndex);
                base.OnAutopilotOn(actor, placeIndex);
            }

            public override void OnBuildingKilled(string title, Point3d pos, AiDamageInitiator initiator, int eventArgInt)
            {
                Debug.WriteLine("Mission.OnBuildingKilled({0}, {1}, {2})", title, initiator != null ? CloDAPIUtil.GetName(initiator) : string.Empty, eventArgInt);
                base.OnBuildingKilled(title, pos, initiator, eventArgInt);
            }

            public override void OnStationaryKilled(int missionNumber, GroundStationary _stationary, AiDamageInitiator initiator, int eventArgInt)
            {
                Debug.WriteLine("Mission.OnStationaryKilled({0}, {1}, {2}, {3}, {4})", missionNumber, _stationary.Name, _stationary.Title, initiator != null ? CloDAPIUtil.GetName(initiator) : string.Empty, eventArgInt);
                base.OnStationaryKilled(missionNumber, _stationary, initiator, eventArgInt);
                if (MissionStatus != null)
                {
                    MissionStatus.Update(_stationary, GameEventId.StationaryKilled);
                }
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

            private void ProcSpawnDynamic()
            {
                if (MissionProc != null)
                {
                    MissionProc.SpawnDynamic();
                }
            }

            private void ProcUpdateTasks()
            {
                if (MissionProc != null)
                {
                    MissionProc.UpdateTasks();
                }
            }

            #endregion
        }
    }
}