// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            public const char ActorDeadInfoSplitChar = '|';

            public enum ActorDeadInfoKey
            {
                Army,
                ActorType,
                ActorName,
                ActorTypeName,
                Count,
            }

            protected abstract Core Core
            {
                get;
            }

            public Dictionary<string, List<DamagerScore>> ActorDead
            {
                get;
            }

            public Mission()
            {
                Debug.WriteLine("Mission.Mission()");
                ActorDead = new Dictionary<string, List<DamagerScore>>();
            }


            public string PlayerActorName
            {
                get;
                set;
            }

            /// <summary>
            /// React on the AircraftTookOff event.
            /// </summary>
            /// <param name="missionNumber"></param>
            /// <param name="shortName"></param>
            /// <param name="aircraft"></param>
            /// <remarks>
            /// Remove the player from the aircraft for a few ms. This is a workaround needed so that AI aicraft do not stay on the ground after the human player took off.
            /// </remarks>
            //public override void OnAircraftTookOff(int missionNumber, string shortName, AiAircraft aircraft)
            //{
            //    base.OnAircraftTookOff(missionNumber, shortName, aircraft);

            //    if (aircraft.Player(0) != null)
            //    {
            //        Player player = aircraft.Player(0);

            //        player.PlaceLeave(0);

            //        Timeout(0.1, () =>
            //        {
            //            player.PlaceEnter(aircraft, 0);
            //        });
            //    }
            //}

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
            }

            public override void OnTickReal()
            {
                base.OnTickReal();
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
            }

            public override void OnBattleStoped()
            {
                Debug.WriteLine("Mission.OnBattleStoped()");
                base.OnBattleStoped();
            }

            public override void OnMissionLoaded(int missionNumber)
            {
                Debug.WriteLine("Mission.OnMissionLoaded({0})", missionNumber);
                base.OnMissionLoaded(missionNumber);
            }

            public override void OnPlayerConnected(Player player)
            {
                Debug.WriteLine("Mission.OnPlayerConnected({0}", player.Name());
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
                    player.Name(), army, player.Place() != null ? player.Place().Name(): "null", player.PersonPrimary() != null ? player.PersonPrimary().Name(): "null");
                base.OnPlayerArmy(player, army);
            }

            public override void OnActorCreated(int missionNumber, string shortName, AiActor actor)
            {
//#if DEBUG
//                if (string.Compare(shortName, "NONAME", true) != 0)
//                {
//                    object o = actor.Tag;
//                    Debug.WriteLine("Mission.OnActorCreated({0}, {1}, {2})", missionNumber, shortName, actor.Name());
//                }
//#endif
                base.OnActorCreated(missionNumber, shortName, actor);
            }

            public override void OnActorDestroyed(int missionNumber, string shortName, AiActor actor)
            {
                Debug.WriteLine("Mission.OnActorDestroyed({0}, {1}, {2}, {3}, Valid={4}, Alive={5}, TaskComplete={6})", missionNumber, shortName, actor.Name(),
                    actor is AiAircraft ? "Aircraft": actor is AiGroundActor ? "AiGroundActor": string.Empty, actor.IsValid(), actor.IsAlive(), actor.IsTaskComplete());
                base.OnActorDestroyed(missionNumber, shortName, actor);
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
                        string.Join("|", damages.Where(x => x.initiator != null).Select(x => string.Format("{0}[{1}]", x.score,  x.initiator.Player != null ? x.initiator.Player.Name(): x.initiator.Person != null ? x.initiator.Person.Name(): string.Empty))),
                        actor.IsValid(), actor.IsAlive(), actor.IsTaskComplete());
                base.OnActorDead(missionNumber, shortName, actor, damages);

                if (actor is AiAircraft || actor is AiGroundActor)
                {
                    string key = string.Format("{0}{1}{2}{3}{4}{5}{6}", 
                                                actor.Army(), ActorDeadInfoSplitChar,                   // Army
                                                actor is AiAircraft ? 0: 1, ActorDeadInfoSplitChar,     // ActorType
                                                shortName, ActorDeadInfoSplitChar,                      // Actor Name
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
                // Debug.WriteLine("Mission.OnActorTaskCompleted({0}, {1}, {2})", missionNumber, shortName, actor.Name());
                base.OnActorTaskCompleted(missionNumber, shortName, actor);
            }

            public override void OnTrigger(int missionNumber, string shortName, bool active)
            {
                //Debug.WriteLine("Mission.OnTrigger({0}, {1}, {2})", missionNumber, shortName, active);
                base.OnTrigger(missionNumber, shortName, active);
            }

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
                //Debug.WriteLine("Mission.OnAircraftTookOff({0}, {1}, {2})", missionNumber, shortName, aircraft.InternalTypeName());
                base.OnAircraftTookOff(missionNumber, shortName, aircraft);
            }

            public override void OnAircraftLanded(int missionNumber, string shortName, AiAircraft aircraft)
            {
                //Debug.WriteLine("Mission.OnAircraftLanded({0}, {1}, {2})", missionNumber, shortName, aircraft.InternalTypeName());
                base.OnAircraftLanded(missionNumber, shortName, aircraft);
            }

            public override void OnAircraftCrashLanded(int missionNumber, string shortName, AiAircraft aircraft)
            {
                //Debug.WriteLine("Mission.OnAircraftCrashLanded({0}, {1}, {2})", missionNumber, shortName, aircraft.InternalTypeName());
                base.OnAircraftCrashLanded(missionNumber, shortName, aircraft);
            }

            public override void OnAircraftKilled(int missionNumber, string shortName, AiAircraft aircraft)
            {
                Debug.WriteLine("Mission.OnAircraftKilled({0}, {1}, {2})", missionNumber, shortName, aircraft.InternalTypeName());
                base.OnAircraftKilled(missionNumber, shortName, aircraft);
            }

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

            public override void OnPlaceEnter(Player player, AiActor actor, int placeIndex)
            {
                Debug.WriteLine("Mission.OnPlaceEnter({0}, {1}, {2})", player.Name(), actor.Name(), placeIndex);
                base.OnPlaceEnter(player, actor, placeIndex);

                if (actor is AiCart)
                {
                    // 0:tobruk:Tobruk_LW_JG53_10.00 -> tobruk:Tobruk_LW_JG53_10.001
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
                Debug.WriteLine("Mission.OnPlaceLeave({0}, {1}, {2})", player.Name(), actor.Name(), placeIndex);
                base.OnPlaceLeave(player, actor, placeIndex);
            }

            public override void OnCarter(AiActor actor, int placeIndex)
            {
                Debug.WriteLine("Mission.OnCarter({0}, {1})", actor.Name(), placeIndex);
                base.OnAutopilotOn(actor, placeIndex);
            }

            public override void OnAutopilotOn(AiActor actor, int placeIndex)
            {
                base.OnAutopilotOn(actor, placeIndex);
            }

            public override void OnAutopilotOff(AiActor actor, int placeIndex)
            {
                base.OnAutopilotOff(actor, placeIndex);
            }

            public override void OnAiAirNewEnemy(AiAirEnemyElement element, int army)
            {
                base.OnAiAirNewEnemy(element, army);
            }

            public override void OnSingleBattleSuccess(bool success)
            {
                Debug.WriteLine("Mission.OnSingleBattleSuccess({0})", success);
                base.OnSingleBattleSuccess(success);
            }

            public override void OnOrderMissionMenuSelected(Player player, int ID, int menuItemIndex)
            {
                base.OnOrderMissionMenuSelected(player, ID, menuItemIndex);
            }

            public override void OnBuildingKilled(string title, Point3d pos, AiDamageInitiator initiator, int eventArgInt)
            {
                Debug.WriteLine("Mission.OnBuildingKilled({0}, {1}, {2})", title, initiator.Player != null ? initiator.Player.Name(): string.Empty, eventArgInt);
                base.OnBuildingKilled(title, pos, initiator, eventArgInt);
            }

            public override void OnStationaryKilled(int missionNumber, GroundStationary _stationary, AiDamageInitiator initiator, int eventArgInt)
            {
                Debug.WriteLine("Mission.OnStationaryKilled({0}, {1}, {2}, {3})", missionNumber, _stationary.Name, initiator.Player != null ? initiator.Player.Name() : string.Empty, eventArgInt);
                base.OnStationaryKilled(missionNumber, _stationary, initiator, eventArgInt);
            }

            public override void OnBombExplosion(string title, double mass, Point3d pos, AiDamageInitiator initiator, int eventArgInt)
            {
                base.OnBombExplosion(title, mass, pos, initiator, eventArgInt);
            }

            public override void OnUserCreateUserLabel(GPUserLabel ul)
            {
                base.OnUserCreateUserLabel(ul);
            }

            public override void OnUserDeleteUserLabel(GPUserLabel ul)
            {
                base.OnUserDeleteUserLabel(ul);
            }

            public override void Timeout(double sec, DoTimeout doTimeout)
            {
                base.Timeout(sec, doTimeout);
            }

            public override object[] OnIntraMissionsMessage(string sMsg, object[] args = null)
            {
                return base.OnIntraMissionsMessage(sMsg, args);
            }

            public override void Inited()
            {
                // Debug.WriteLine("Mission.Inited()");
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
        }
    }
}