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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using IL2DCE.MissionObjectModel;
using maddox.game;
using maddox.GP;

namespace IL2DCE.Generator
{
    class GeneratorAirOperation
    {
        private Generator Generator
        {
            get;
            set;
        }

        private CampaignInfo CampaignInfo
        {
            get;
            set;
        }

        private MissionFile MissionTemplate
        {
            get;
            set;
        }

        IGamePlay GamePlay
        {
            get;
            set;
        }

        private Config Config
        {
            get;
            set;
        }

        private IRandom Random
        {
            get
            {
                return Generator.Random;
            }
        }

        public List<AirGroup> AvailableAirGroups = new List<AirGroup>();

        public GeneratorAirOperation(Generator generator, CampaignInfo campaignInfo, MissionFile missionTemplate, IGamePlay gamePlay, Config config)
        {
            Generator = generator;
            CampaignInfo = campaignInfo;
            MissionTemplate = missionTemplate;
            GamePlay = gamePlay;
            Config = config;

            AvailableAirGroups.Clear();

            foreach (AirGroup airGroup in MissionTemplate.AirGroups)
            {
                AvailableAirGroups.Add(airGroup);
            }
        }

        private double getRandomAltitude(AircraftParametersInfo missionParameters)
        {
            if (missionParameters.MinAltitude != null && missionParameters.MinAltitude.HasValue && missionParameters.MaxAltitude != null && missionParameters.MaxAltitude.HasValue)
            {
                return (double)Random.Next((int)missionParameters.MinAltitude.Value, (int)missionParameters.MaxAltitude.Value);
            }
            else
            {
                GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, "No altitude defined for: " + missionParameters.LoadoutId + ". Using default altitude.", null);
                // Use some default altitudes
                return (double)Random.Next(300, 7000);
            }
        }

        /// <remarks>
        /// 
        ///     // Tweaked AI settings http://bobgamehub.blogspot.de/2012/03/ai-settings-in-cliffs-of-dover.html
        ///
        ///     // Fighter (the leading space is important)
        ///     string frookie = " 0.30 0.11 0.78 0.40 0.64 0.85 0.85 0.21";
        ///     string faverage = " 0.32 0.12 0.87 0.60 0.74 0.90 0.90 0.31";
        ///     string fexperienced = " 0.52 0.13 0.89 0.70 0.74 0.95 0.92 0.31";
        ///     string fveteran = " 0.73 0.14 0.92 0.80 0.74 1 0.95 0.41";
        ///     string face = " 0.93 0.15 0.96 0.92 0.84 1 1 0.51";
        ///
        ///     // Fighter Bomber (the leading space is important)
        ///     string xrookie = " 0.30 0.11 0.78 0.30 0.74 0.85 0.90 0.40";
        ///     string xaverage = " 0.32 0.12 0.87 0.35 0.74 0.90 0.95 0.52";
        ///     string xexperienced = " 0.52 0.13 0.89 0.38 0.74 0.92 0.95 0.52";
        ///     string xveteran = " 0.73 0.14 0.92 0.40 0.74 0.95 0.95 0.55";
        ///     string xace = " 0.93 0.15 0.96 0.45 0.74 1 1 0.65";
        ///
        ///     // Bomber (the leading space is important)
        ///     string brookie = " 0.30 0.11 0.78 0.20 0.74 0.85 0.90 0.88";
        ///     string baverage = " 0.32 0.12 0.87 0.25 0.74 0.90 0.95 0.91";
        ///     string bexperienced = " 0.52 0.13 0.89 0.28 0.74 0.92 0.95 0.91";
        ///     string bveteran = " 0.73 0.14 0.92 0.30 0.74 0.95 0.95 0.95";
        ///     string bace = " 0.93 0.15 0.96 0.35 0.74 1 1 0.97";
        /// 
        ///     Min 0 - Max 1
        ///     Rookie:     0.79 0.26 0.26 0.16 0.16 0.26 0.37 0.26
        ///     Avarage:    0.84 0.53 0.53 0.37 0.37 0.53 0.53 0.53
        ///     Veteran:    1,0.74 0.84 0.63 0.84 0.84 0.74 0.74
        ///     Ace:        1 0.95 0.95 0.84 0.84 0.95 0.89 0.89
        ///     Min:        0 0 0 0 0 0 0 0
        ///     Max:        1 1 1 1 1 1 1 1
        /// </remarks>
        private string getTweakedSkill(EMissionType missionType, int level)
        {
            if (missionType == EMissionType.COVER || missionType == EMissionType.ESCORT
                || missionType == EMissionType.INTERCEPT)
            {
                // Fighter
                string[] skills = new string[] {
                    "0.30 0.11 0.78 0.40 0.64 0.85 0.85 0.21",
                    "0.32 0.12 0.87 0.60 0.74 0.90 0.90 0.31",
                    "0.52 0.13 0.89 0.70 0.74 0.95 0.92 0.31",
                    "0.73 0.14 0.92 0.80 0.74 1 0.95 0.41",
                    "0.93 0.15 0.96 0.92 0.84 1 1 0.51",
                };

                return skills[level];
            }
            // TODO: Find a way to identify that aircraft is fighter bomber.
            //else if()
            //{
            //    // FighterBomber
            //    string[] skills = new string[] {
            //        "0.30 0.11 0.78 0.30 0.74 0.85 0.90 0.40",
            //        "0.32 0.12 0.87 0.35 0.74 0.90 0.95 0.52",
            //        "0.52 0.13 0.89 0.38 0.74 0.92 0.95 0.52",
            //        "0.73 0.14 0.92 0.40 0.74 0.95 0.95 0.55",
            //        "0.93 0.15 0.96 0.45 0.74 1 1 0.6",
            //    };

            //    return skills[level];
            //}
            else
            {
                // Bomber
                string[] skills = new string[] {
                    "0.30 0.11 0.78 0.20 0.74 0.85 0.90 0.88",
                    "0.32 0.12 0.87 0.25 0.74 0.90 0.95 0.91",
                    "0.52 0.13 0.89 0.28 0.74 0.92 0.95 0.91",
                    "0.73 0.14 0.92 0.30 0.74 0.95 0.95 0.95",
                    "0.93 0.15 0.96 0.35 0.74 1.00 1.00 0.97",
                };

                return skills[level];
            }
        }

        private string getRandomSkill(EMissionType missionType)
        {
            int randomLevel = Random.Next(0, 4);

            return getTweakedSkill(missionType, randomLevel);
        }

        private void getRandomFlightSize(AirGroup airGroup, EMissionType missionType)
        {
            airGroup.Flights.Clear();
            int aircraftNumber = 1;

            AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
            int flightCount = (int)Math.Ceiling(airGroupInfo.FlightCount * this.Config.FlightCount);
            int flightSize = (int)Math.Ceiling(airGroupInfo.FlightSize * this.Config.FlightSize);

            if (missionType == EMissionType.RECON || missionType == EMissionType.MARITIME_RECON)
            {
                flightCount = 1;
                flightSize = 1;
            }
            else if (missionType == EMissionType.ARMED_RECON || missionType == EMissionType.ARMED_MARITIME_RECON)
            {
                flightCount = 1;
            }
            else if (missionType == EMissionType.ESCORT || missionType == EMissionType.INTERCEPT || missionType == EMissionType.COVER)
            {
                if (airGroup.TargetAirGroup != null)
                {
                    if (airGroup.TargetAirGroup.Flights.Count < flightCount)
                    {
                        flightCount = airGroup.TargetAirGroup.Flights.Count;
                    }
                }
            }

            for (int i = 0; i < flightCount; i++)
            {
                List<string> aircraftNumbers = new List<string>();
                for (int j = 0; j < flightSize; j++)
                {
                    aircraftNumbers.Add(aircraftNumber.ToString(CultureInfo.InvariantCulture.NumberFormat));
                    aircraftNumber++;
                }
                airGroup.Flights[i] = aircraftNumbers;
            }
        }

        public static List<List<T>> SplitList<T>(List<T> locations, int numberOfChunks)
        {
            List<List<T>> result = new List<List<T>>();
            for (int i = 0; i < locations.Count; i += numberOfChunks)
            {
                result.Add(locations.GetRange(i, Math.Min(numberOfChunks, locations.Count - i)));
            }
            return result;
        }

        public AirGroup getRandomAirGroupBasedOnDistance(List<AirGroup> availableAirGroups, AirGroup referenceAirGroup)
        {
            return getRandomAirGroupBasedOnDistance(availableAirGroups, referenceAirGroup.Position);
        }

        public AirGroup getRandomAirGroupBasedOnDistance(List<AirGroup> availableAirGroups, Point2d targetPosition)
        {
            return getRandomAirGroupBasedOnDistance(availableAirGroups, new Point3d(targetPosition.x, targetPosition.y, 0.0));
        }

        public AirGroup getRandomAirGroupBasedOnDistance(List<AirGroup> availableAirGroups, Point3d targetPosition)
        {
            AirGroup selectedAirGroup = null;

            if (availableAirGroups.Count > 1)
            {
                var copy = new List<AirGroup>(availableAirGroups);

                copy.Sort(new DistanceComparer(targetPosition));

                Point3d position = targetPosition;
                Point3d last = copy[copy.Count - 1].Position;
                double maxDistance = last.distance(ref position);

                List<KeyValuePair<AirGroup, int>> elements = new List<KeyValuePair<AirGroup, int>>();

                int previousWeight = 0;

                foreach (AirGroup airGroup in copy)
                {
                    double distance = airGroup.Position.distance(ref position);
                    int weight = Convert.ToInt32(Math.Ceiling(maxDistance - distance));
                    int cumulativeWeight = previousWeight + weight;
                    elements.Add(new KeyValuePair<AirGroup, int>(airGroup, cumulativeWeight));

                    previousWeight = cumulativeWeight;
                }

                int diceRoll = Random.Next(0, previousWeight);
                int cumulative = 0;
                for (int i = 0; i < elements.Count; i++)
                {
                    cumulative += elements[i].Value;
                    if (diceRoll <= cumulative)
                    {
                        selectedAirGroup = elements[i].Key;
                        break;
                    }
                }
            }
            else if (availableAirGroups.Count == 1)
            {
                selectedAirGroup = availableAirGroups[0];
            }

            return selectedAirGroup;
        }

        public int getRandomIndex(ref List<AirGroup> airGroups, Point3d position)
        {
            // Sort the air groups by their distance to the position.
            airGroups.Sort(new DistanceComparer(position));

            List<AirGroup> selectedElement = null;
            AirGroup selectedAirGroup = null;

            int numberOfChunks = 4;
            if (airGroups.Count < 4)
            {
                // Split the air groups into chunks based on their distance to the position.
                List<List<AirGroup>> airGroupChunks = SplitList(airGroups, numberOfChunks);

                List<KeyValuePair<List<AirGroup>, double>> elements = new List<KeyValuePair<List<AirGroup>, double>>();

                // The closer the chunk to the position, the higher is the propability for the selection.
                // Cumulative propability: 
                // http://www.vcskicks.com/random-element.php

                elements.Add(new KeyValuePair<List<AirGroup>, double>(airGroupChunks[3], 5));                //  5%
                elements.Add(new KeyValuePair<List<AirGroup>, double>(airGroupChunks[2], 5 + 15));           // 15%
                elements.Add(new KeyValuePair<List<AirGroup>, double>(airGroupChunks[1], 5 + 15 + 30));      // 30%
                elements.Add(new KeyValuePair<List<AirGroup>, double>(airGroupChunks[0], 5 + 15 + 30 + 50)); // 50%

                int diceRoll = Random.Next(0, 100);

                double cumulative = 0.0;
                for (int i = 0; i < elements.Count; i++)
                {
                    cumulative += elements[i].Value;
                    if (diceRoll < cumulative)
                    {
                        selectedElement = elements[i].Key;
                        break;
                    }
                }
            }
            else
            {
                // Fallback if there are not enough air groups to split them into chunks.              
                selectedElement = airGroups;
            }

            // Randomly choose one air group within the chunk.
            selectedAirGroup = selectedElement[Random.Next(0, selectedElement.Count - 1)];

            // Return the global index of the selected air group (and not the index within the chunk).
            return airGroups.IndexOf(selectedAirGroup);
        }

        public bool CreateRandomAirOperation(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, Skill [] skill = null, Spawn spawn = null, int speed = -1, int fuel = -1)
        {
            bool result = false;
            IList<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes;
            if (missionTypes.Count > 0)
            {
                List<EMissionType> availableMissionTypes = new List<EMissionType>();
                foreach (EMissionType missionType in missionTypes)
                {
                    if (isMissionTypeAvailable(airGroup, missionType))
                    {
                        availableMissionTypes.Add(missionType);
                    }
                }

                if (availableMissionTypes.Count > 0)
                {
                    int randomMissionTypeIndex = Random.Next(availableMissionTypes.Count);
                    EMissionType randomMissionType = availableMissionTypes[randomMissionTypeIndex];
                    result = CreateAirOperation(sectionFile, briefingFile, airGroup, randomMissionType, true, null, null, null, skill, spawn, speed, fuel);
                }
            }
            return result;
        }

        public bool CreateAirOperation(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, EMissionType missionType, bool allowDefensiveOperation, 
            AirGroup forcedEscortAirGroup, GroundGroup forcedTargetGroundGroup, Stationary forcedTargetStationary, Skill [] skill = null, Spawn spawn = null, int speed = -1, int fuel = -1)
        {
            bool result = false;
            if (isMissionTypeAvailable(airGroup, missionType))
            {
                AvailableAirGroups.Remove(airGroup);
                jamRunway(airGroup);

                AircraftInfo aircraftInfo = CampaignInfo.GetAircraftInfo(airGroup.Class);
                AircraftParametersInfo aircraftParametersInfo = getAvailableRandomAircratParametersInfo(aircraftInfo, missionType);
                AircraftLoadoutInfo aircraftLoadoutInfo = aircraftInfo.GetAircraftLoadoutInfo(aircraftParametersInfo.LoadoutId);
                airGroup.Weapons = aircraftLoadoutInfo.Weapons;
                airGroup.Detonator = aircraftLoadoutInfo.Detonator;
                airGroup.TraceLoadoutInfo();

                AirGroup escortAirGroup = forcedEscortAirGroup;
                if (isMissionTypeEscorted(missionType))
                {
                    if (escortAirGroup == null)
                    {
                        escortAirGroup = getAvailableRandomEscortAirGroup(airGroup);
                    }
                }

                if (missionType == EMissionType.RECON || missionType == EMissionType.MARITIME_RECON)
                {
                    GroundGroup groundGroup = forcedTargetGroundGroup;
                    Stationary stationary = forcedTargetStationary;
                    if (groundGroup == null && stationary == null)
                    {
                        groundGroup = Generator.GeneratorGroundOperation.getAvailableRandomEnemyGroundGroup(airGroup, missionType);
                        stationary = Generator.GeneratorGroundOperation.getAvailableRandomEnemyStationary(airGroup, missionType);
                        if (groundGroup != null && stationary != null)
                        {
                            // Randomly select one of them
                            int type = Random.Next(2);
                            if (type == 0)
                            {
                                stationary = null;
                            }
                            else
                            {
                                groundGroup = null;
                            }
                        }
                    }

                    double altitude = getRandomAltitude(aircraftParametersInfo);
                    if (groundGroup != null)
                    {
                        Generator.GeneratorGroundOperation.CreateRandomGroundOperation(sectionFile, groundGroup);
                        airGroup.Recon(groundGroup, altitude, escortAirGroup);
                        result = true;
                    }
                    else if (stationary != null)
                    {
                        airGroup.Recon(stationary, altitude, escortAirGroup);
                        result = true;
                    }
                }
                else if (missionType == EMissionType.ATTACK_ARTILLERY
                    || missionType == EMissionType.ATTACK_RADAR
                    || missionType == EMissionType.ATTACK_AIRCRAFT
                    || missionType == EMissionType.ATTACK_DEPOT)
                {
                    Stationary stationary = forcedTargetStationary;
                    if (stationary == null)
                    {
                        stationary = Generator.GeneratorGroundOperation.getAvailableRandomEnemyStationary(airGroup, missionType);
                    }
                    // No need to generate a random ground operation for the stationary as the stationary objects are always generated
                    // into the file.
                    //Generator.GeneratorGroundOperation.CreateRandomGroundOperation(sectionFile, stationary);
                    double altitude = getRandomAltitude(aircraftParametersInfo);

                    airGroup.GroundAttack(stationary, altitude, escortAirGroup);
                    result = true;
                }
                else if (missionType == EMissionType.ATTACK_ARMOR ||
                    missionType == EMissionType.ATTACK_VEHICLE ||
                    missionType == EMissionType.ATTACK_TRAIN ||
                    missionType == EMissionType.ATTACK_SHIP ||
                    missionType == EMissionType.ARMED_RECON ||
                    missionType == EMissionType.ARMED_MARITIME_RECON)
                {
                    GroundGroup groundGroup = forcedTargetGroundGroup;
                    Stationary stationary = forcedTargetStationary;
                    if (groundGroup == null && stationary == null)
                    {
                        groundGroup = Generator.GeneratorGroundOperation.getAvailableRandomEnemyGroundGroup(airGroup, missionType);
                        stationary = Generator.GeneratorGroundOperation.getAvailableRandomEnemyStationary(airGroup, missionType);

                        if (groundGroup != null && stationary != null)
                        {
                            // Randomly select one of them
                            int type = Random.Next(2);
                            if (type == 0)
                            {
                                stationary = null;
                            }
                            else
                            {
                                groundGroup = null;
                            }
                        }
                    }

                    double altitude = getRandomAltitude(aircraftParametersInfo);
                    if (groundGroup != null)
                    {
                        Generator.GeneratorGroundOperation.CreateRandomGroundOperation(sectionFile, groundGroup);
                        airGroup.GroundAttack(groundGroup, altitude, escortAirGroup);
                        result = true;
                    }
                    else if (stationary != null)
                    {
                        airGroup.GroundAttack(stationary, altitude, escortAirGroup);
                        result = true;
                    }
                }
                else if (missionType == EMissionType.INTERCEPT)
                {
                    GroundGroup targetGroundGroup;
                    Stationary targetStationary;
                    EMissionType offensiveMissionType;
                    AirGroup offensiveAirGroup = getAvailableRandomOffensiveAirGroup(airGroup, out offensiveMissionType, out targetGroundGroup, out targetStationary);
                    if (offensiveAirGroup != null)
                    {
                        CreateAirOperation(sectionFile, briefingFile, offensiveAirGroup, offensiveMissionType, false, null, targetGroundGroup, targetStationary);
                        airGroup.Intercept(offensiveAirGroup);
                        result = true;
                    }
                }
                else if (missionType == EMissionType.ESCORT)
                {
                    AirGroup escortedAirGroup = getAvailableRandomEscortedAirGroup(airGroup);
                    if (escortedAirGroup != null)
                    {
                        List<EMissionType> availableEscortedMissionTypes = new List<EMissionType>();
                        IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(escortedAirGroup.Class).MissionTypes;
                        foreach (EMissionType targetMissionType in missionTypes)
                        {
                            if (isMissionTypeAvailable(escortedAirGroup, targetMissionType) && isMissionTypeEscorted(targetMissionType))
                            {
                                availableEscortedMissionTypes.Add(targetMissionType);
                            }
                        }

                        if (availableEscortedMissionTypes.Count > 0)
                        {
                            int escortedMissionTypeIndex = Random.Next(availableEscortedMissionTypes.Count);
                            EMissionType randomEscortedMissionType = availableEscortedMissionTypes[escortedMissionTypeIndex];
                            CreateAirOperation(sectionFile, briefingFile, escortedAirGroup, randomEscortedMissionType, true, airGroup, null, null);

                            airGroup.Escort(escortedAirGroup);
                            result = true;
                        }
                    }
                }
                else if (missionType == EMissionType.COVER)
                {
                    GroundGroup targetGroundGroup;
                    Stationary targetStationary;
                    EMissionType offensiveMissionType;
                    AirGroup offensiveAirGroup = getAvailableRandomOffensiveAirGroup(airGroup, out offensiveMissionType, out targetGroundGroup, out targetStationary);
                    if (offensiveAirGroup != null)
                    {
                        CreateAirOperation(sectionFile, briefingFile, offensiveAirGroup, offensiveMissionType, false, null, targetGroundGroup, targetStationary);
                        if (offensiveAirGroup.Altitude.HasValue && (offensiveAirGroup.TargetGroundGroup != null || offensiveAirGroup.TargetStationary != null))
                        {
                            airGroup.Cover(offensiveAirGroup, offensiveAirGroup.Altitude.Value);
                            result = true;
                        }
                    }
                }
                else if (missionType == EMissionType.FOLLOW)
                {
                    var friendlyAirGroups = AvailableAirGroups.Where(x => x.ArmyIndex == airGroup.ArmyIndex && x != airGroup);
                    var targetAirGroups = friendlyAirGroups.Where(x => GetTargetPoint(x) != null);
                    if (targetAirGroups.Count() > 0)
                    {
                        AirGroup targetAirGroup = targetAirGroups.ToArray()[Random.Next(0, targetAirGroups.Count() - 1)];
                        airGroup.Follow(targetAirGroup);
                        result = true;
                    }
                }
                else if (missionType == EMissionType.HUNTING)
                {
                    var enemyAirGroups = AvailableAirGroups.Where(x => x.ArmyIndex != airGroup.ArmyIndex);
                    var targetAirGroups = enemyAirGroups.Where(x => GetTargetPoint(x) != null && GetTargetAltitude(x) != null);
                    if (targetAirGroups.Count() > 0)
                    {
                        AirGroup targetAirGroup = targetAirGroups.ToArray()[Random.Next(0, targetAirGroups.Count() - 1)];
                        Point2d? targetPoint = GetTargetPoint(targetAirGroup);
                        airGroup.Hunting(targetPoint.Value, GetTargetAltitude(targetAirGroup).Value);
                        result = true;
                    }
                }

                if (!result)
                {
                    Debug.Assert(false);
                }

                // Flight Size
                getRandomFlightSize(airGroup, missionType);

                // Skill
#if false
                airGroup.Skill = skill != null && skill.Length > 0 ? skill[0].ToString(): getRandomSkill(missionType);
#else
                if (skill != null)
                {
                    if (skill.Length == 1)
                    {
                        if (skill[0] != Skill.Default)
                        {
                            airGroup.Skill = skill[0].ToString();
                            airGroup.Skills.Clear();
                        }
                    }
                    else
                    {
                        airGroup.Skill = string.Empty;
                        int s = 0;
                        for (int i = 0; i < airGroup.Flights.Count(); i++)
                        {
                            IList<string> list = airGroup.Flights[i];
                            for (int j = 0; j < list.Count; j++)
                            {
                                airGroup.Skills.Add(i, skill[s].ToString());
                                if (s + 1 < skill.Length)
                                {
                                    s++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    airGroup.Skill = getRandomSkill(missionType);
                    airGroup.Skills.Clear();
                }
#endif

                // Spawn
                airGroup.SetOnParked = Config.SpawnParked;
                if (spawn != null)
                {
                    airGroup.SetSpawn(spawn);
                }

                // Speed
                airGroup.SetSpeed(speed);

                // Fuel
                airGroup.SetFuel(fuel);

                // Create Briefing 
                Generator.GeneratorBriefing.CreateBriefing(briefingFile, airGroup, missionType, escortAirGroup);
                airGroup.WriteTo(sectionFile);

                if (forcedEscortAirGroup == null && escortAirGroup != null)
                {
                    // TODO: Consider calling CreateAirOperation with a forcedEscortedAirGroup.
                    AvailableAirGroups.Remove(escortAirGroup);
                    jamRunway(escortAirGroup);

                    aircraftInfo = CampaignInfo.GetAircraftInfo(escortAirGroup.Class);
                    aircraftParametersInfo = getAvailableRandomAircratParametersInfo(aircraftInfo, EMissionType.ESCORT);
                    aircraftLoadoutInfo = aircraftInfo.GetAircraftLoadoutInfo(aircraftParametersInfo.LoadoutId);
                    escortAirGroup.Weapons = aircraftLoadoutInfo.Weapons;

                    escortAirGroup.Escort(airGroup);

                    getRandomFlightSize(escortAirGroup, EMissionType.ESCORT);
                    escortAirGroup.Skill = getRandomSkill(EMissionType.ESCORT);
                    Generator.GeneratorBriefing.CreateBriefing(briefingFile, escortAirGroup, EMissionType.ESCORT, null);
                    escortAirGroup.WriteTo(sectionFile);
                }

                if (isMissionTypeOffensive(missionType))
                {
                    if (allowDefensiveOperation)
                    {
                        AirGroup defensiveAirGroup = getAvailableRandomDefensiveAirGroup(airGroup);
                        if (defensiveAirGroup != null)
                        {
                            AvailableAirGroups.Remove(defensiveAirGroup);
                            jamRunway(defensiveAirGroup);

                            List<EMissionType> availableDefensiveMissionTypes = new List<EMissionType>();
                            IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(defensiveAirGroup.Class).MissionTypes;
                            foreach (EMissionType targetMissionType in missionTypes)
                            {
                                // "isMissionTypeAvailable" checks if there is any air group available for 
                                // a offensive mission. As the air group for the offensive mission is already 
                                // determined, the defensive mission type is always available.
                                if (/*isMissionTypeAvailable(defensiveAirGroup, targetMissionType) &&*/ isMissionTypeDefensive(targetMissionType))
                                {
                                    availableDefensiveMissionTypes.Add(targetMissionType);
                                }
                            }

                            if (availableDefensiveMissionTypes.Count > 0)
                            {
                                int defensiveMissionTypeIndex = Random.Next(availableDefensiveMissionTypes.Count);
                                EMissionType randomDefensiveMissionType = availableDefensiveMissionTypes[defensiveMissionTypeIndex];

                                // TODO: Consider calling CreateAirOperation with a forcedOffensiveAirGroup to remove duplicated code.

                                aircraftInfo = CampaignInfo.GetAircraftInfo(defensiveAirGroup.Class);
                                aircraftParametersInfo = getAvailableRandomAircratParametersInfo(aircraftInfo, randomDefensiveMissionType);
                                aircraftLoadoutInfo = aircraftInfo.GetAircraftLoadoutInfo(aircraftParametersInfo.LoadoutId);
                                defensiveAirGroup.Weapons = aircraftLoadoutInfo.Weapons;
                                defensiveAirGroup.TraceLoadoutInfo();

                                if (randomDefensiveMissionType == EMissionType.INTERCEPT)
                                {
                                    defensiveAirGroup.Intercept(airGroup);
                                }
                                else if (randomDefensiveMissionType == EMissionType.COVER)
                                {
                                    if (airGroup.Altitude.HasValue && (airGroup.TargetGroundGroup != null || airGroup.TargetStationary != null))
                                    {
                                        defensiveAirGroup.Cover(airGroup, airGroup.Altitude.Value);
                                    }
                                }

                                getRandomFlightSize(defensiveAirGroup, randomDefensiveMissionType);
                                defensiveAirGroup.Skill = getRandomSkill(randomDefensiveMissionType);
                                Generator.GeneratorBriefing.CreateBriefing(briefingFile, defensiveAirGroup, randomDefensiveMissionType, null);
                                defensiveAirGroup.WriteTo(sectionFile);
                            }
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException(string.Format("no available MissionType[{0}]", missionType.ToDescription()));
            }

            return result;
        }

        private Point2d? GetTargetPoint(AirGroup airGroup)
        {
            if (airGroup.TargetArea != null && airGroup.TargetArea.HasValue)
            {
                return airGroup.TargetArea.Value;
            }
            else if (airGroup.TargetAirGroup != null && airGroup.TargetAirGroup.Waypoints != null && airGroup.TargetAirGroup.Waypoints.Count > 0)
            {
                AirGroupWaypoint way = airGroup.TargetAirGroup.Waypoints[airGroup.TargetAirGroup.Waypoints.Count - 1];
                return new Point2d(way.X, way.Y);
            }
            else if (airGroup.TargetGroundGroup != null && airGroup.TargetGroundGroup.Waypoints != null && airGroup.TargetGroundGroup.Waypoints.Count > 0)
            {
                GroundGroupWaypoint way = airGroup.TargetGroundGroup.Waypoints[airGroup.TargetGroundGroup.Waypoints.Count - 1];
                return new Point2d(way.X, way.Y);
            }
            else if (airGroup.TargetStationary != null)
            {
                return new Point2d(airGroup.TargetStationary.X, airGroup.TargetStationary.Y);
            }
            else if (airGroup.Waypoints != null && airGroup.Waypoints.Count > 0)
            {
                AirGroupWaypoint way = airGroup.Waypoints[airGroup.Waypoints.Count - 1];
                return new Point2d(way.X, way.Y);
            }

            return null;
        }

        private double? GetTargetAltitude(AirGroup airGroup)
        {
            if (airGroup.Altitude != null && airGroup.Altitude.HasValue)
            {
                return airGroup.Altitude.Value;
            }
            else if (airGroup.TargetAirGroup != null && airGroup.TargetAirGroup.Waypoints != null && airGroup.TargetAirGroup.Waypoints.Count > 0)
            {
                AirGroupWaypoint way = airGroup.TargetAirGroup.Waypoints[airGroup.TargetAirGroup.Waypoints.Count - 1];
                return way.Z;
            }
            else if (airGroup.Waypoints != null && airGroup.Waypoints.Count > 0)
            {
                AirGroupWaypoint way = airGroup.Waypoints[airGroup.Waypoints.Count - 1];
                return way.Z;
            }

            return null;
        }

        //private bool CreateAirOperationRecon(ISectionFile sectionFile, AirGroup airGroup, AircraftParametersInfo aircraftParametersInfo, AirGroup escortAirGroup, GroundGroup groundGroup, Stationary stationary)
        //{
        //    bool result = false;
        //    //GroundGroup groundGroup = forcedTargetGroundGroup;
        //    //Stationary stationary = forcedTargetStationary;
        //    if (groundGroup == null && stationary == null)
        //    {
        //        groundGroup = Generator.GeneratorGroundOperation.getAvailableRandomEnemyGroundGroup(airGroup, missionType);
        //        stationary = Generator.GeneratorGroundOperation.getAvailableRandomEnemyStationary(airGroup, missionType);

        //        if (groundGroup != null && stationary != null)
        //        {
        //            // Randomly select one of them
        //            int type = Random.Next(2);
        //            if (type == 0)
        //            {
        //                stationary = null;
        //            }
        //            else
        //            {
        //                groundGroup = null;
        //            }
        //        }
        //    }

        //    double altitude = getRandomAltitude(aircraftParametersInfo);
        //    if (groundGroup != null)
        //    {
        //        Generator.GeneratorGroundOperation.CreateRandomGroundOperation(sectionFile, groundGroup);
        //        airGroup.Recon(groundGroup, altitude, escortAirGroup);
        //        result = true;
        //    }
        //    else if (stationary != null)
        //    {
        //        airGroup.Recon(stationary, altitude, escortAirGroup);
        //        result = true;
        //    }

        //    return result;
        //}

        private void jamRunway(AirGroup airGroup)
        {
            // Remove all air groups from the available list that are within 5km distance
            int count = AvailableAirGroups.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                Point3d position = AvailableAirGroups[i].Position;
                double distance = airGroup.Position.distance(ref position);
                if (distance < 1000)
                {
                    if (Config.Debug == 1)
                    {
                        AirGroup unavailableAirGroup = AvailableAirGroups[i];

                        GamePlay.gpLogServer(new Player[] { GamePlay.gpPlayer() }, AvailableAirGroups[i].Id + " unavailable. Airfield jammed by " + airGroup.Id, null);
                    }

                    AvailableAirGroups.RemoveAt(i);
                }
            }
        }

        #region Get Available AirGroup

        private List<AirGroup> getAvailableOffensiveAirGroups(int opposingArmyIndex)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in AvailableAirGroups)
            {
                if (airGroup.ArmyIndex != opposingArmyIndex)
                {
                    IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes; 
                    foreach (EMissionType missionType in missionTypes)
                    {
                        if (isMissionTypeOffensive(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            airGroups.Add(airGroup);
                            break;
                        }
                    }
                }
            }
            return airGroups;
        }

        public AirGroup getAvailableRandomOffensiveAirGroup(AirGroup defensiveAirGroup, out EMissionType offensiveMissionType, out GroundGroup targetGroundGroup, out Stationary targetStationary)
        {
            List<AirGroup> airGroups = getAvailableOffensiveAirGroups(defensiveAirGroup.ArmyIndex);

            if (airGroups.Count > 0)
            {
                List<GroundGroup> possibleTargetGroundGroups = new List<GroundGroup>();
                List<Stationary> possibleTargetStationaries = new List<Stationary>();
                Dictionary<GroundGroup, Dictionary<AirGroup, EMissionType>> possibleOffensiveAirGroups = new Dictionary<GroundGroup, Dictionary<AirGroup, EMissionType>>();
                Dictionary<Stationary, Dictionary<AirGroup, EMissionType>> possibleOffensiveAirGroupsStationary = new Dictionary<Stationary, Dictionary<AirGroup, EMissionType>>();

                foreach (AirGroup possibleOffensiveAirGroup in airGroups)
                {
                    List<EMissionType> availableOffensiveMissionTypes = new List<EMissionType>();
                    IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(possibleOffensiveAirGroup.Class).MissionTypes;
                    foreach (EMissionType targetMissionType in missionTypes)
                    {
                        if (isMissionTypeAvailable(possibleOffensiveAirGroup, targetMissionType) && isMissionTypeOffensive(targetMissionType))
                        {
                            availableOffensiveMissionTypes.Add(targetMissionType);
                        }
                    }

                    if (availableOffensiveMissionTypes.Count > 0)
                    {
                        int offensiveMissionTypeIndex = Random.Next(availableOffensiveMissionTypes.Count);
                        EMissionType possibleOffensiveMissionType = availableOffensiveMissionTypes[offensiveMissionTypeIndex];

                        GroundGroup possibleTargetGroundGroup = Generator.GeneratorGroundOperation.getAvailableRandomEnemyGroundGroup(possibleOffensiveAirGroup, possibleOffensiveMissionType);
                        Stationary possibleTargetStationary = Generator.GeneratorGroundOperation.getAvailableRandomEnemyStationary(possibleOffensiveAirGroup, possibleOffensiveMissionType);

                        if (possibleTargetGroundGroup != null)
                        {
                            possibleTargetGroundGroups.Add(possibleTargetGroundGroup);

                            if (!possibleOffensiveAirGroups.ContainsKey(possibleTargetGroundGroup))
                            {
                                possibleOffensiveAirGroups.Add(possibleTargetGroundGroup, new Dictionary<AirGroup, EMissionType>());
                            }
                            possibleOffensiveAirGroups[possibleTargetGroundGroup].Add(possibleOffensiveAirGroup, possibleOffensiveMissionType);
                        }
                        else if (possibleTargetStationary != null)
                        {
                            possibleTargetStationaries.Add(possibleTargetStationary);

                            if (!possibleOffensiveAirGroupsStationary.ContainsKey(possibleTargetStationary))
                            {
                                possibleOffensiveAirGroupsStationary.Add(possibleTargetStationary, new Dictionary<AirGroup, EMissionType>());
                            }
                            possibleOffensiveAirGroupsStationary[possibleTargetStationary].Add(possibleOffensiveAirGroup, possibleOffensiveMissionType);
                        }
                    }
                }

                // Select target considering the distance to the defensiveAirGroup
                targetGroundGroup = Generator.GeneratorGroundOperation.getRandomTargetBasedOnRange(possibleTargetGroundGroups, defensiveAirGroup);
                targetStationary = Generator.GeneratorGroundOperation.getRandomTargetBasedOnRange(possibleTargetStationaries, defensiveAirGroup);

                if (targetGroundGroup != null && targetStationary != null)
                {
                    // Randomly select one of them
                    int type = Random.Next(2);
                    if (type == 0)
                    {
                        targetStationary = null;
                    }
                    else
                    {
                        targetGroundGroup = null;
                    }
                }

                // Now select the offensiveAirGroup for the selected target, also considering the distance to the target
                if (targetGroundGroup != null && possibleOffensiveAirGroups.ContainsKey(targetGroundGroup))
                {
                    targetStationary = null;

                    var offensiveAirGroups = possibleOffensiveAirGroups[targetGroundGroup].Keys.ToList();
                    AirGroup offensiveAirGroup = getRandomAirGroupBasedOnDistance(offensiveAirGroups, targetGroundGroup.Position);
                    offensiveMissionType = possibleOffensiveAirGroups[targetGroundGroup][offensiveAirGroup];
                    return offensiveAirGroup;
                }
                else if (targetStationary != null && possibleOffensiveAirGroupsStationary.ContainsKey(targetStationary))
                {
                    targetGroundGroup = null;

                    var offensiveAirGroups = possibleOffensiveAirGroupsStationary[targetStationary].Keys.ToList();
                    AirGroup offensiveAirGroup = getRandomAirGroupBasedOnDistance(offensiveAirGroups, targetStationary.Position);
                    offensiveMissionType = possibleOffensiveAirGroupsStationary[targetStationary][offensiveAirGroup];
                    return offensiveAirGroup;
                }
                else
                {
                    targetGroundGroup = null;
                    targetStationary = null;
                    offensiveMissionType = EMissionType.RECON;
                    return null;
                }
            }
            else
            {
                targetGroundGroup = null;
                targetStationary = null;
                offensiveMissionType = EMissionType.RECON;
                return null;
            }
        }

        private List<AirGroup> getAvailableDefensiveAirGroups(int opposingArmyIndex)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in AvailableAirGroups)
            {
                if (airGroup.ArmyIndex != opposingArmyIndex)
                {
                    IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes;
                    foreach (EMissionType missionType in missionTypes)
                    {
                        if (isMissionTypeDefensive(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            airGroups.Add(airGroup);
                            break;
                        }
                    }
                }
            }
            return airGroups;
        }

        public AirGroup getAvailableRandomDefensiveAirGroup(AirGroup offensiveAirGroup)
        {
            List<AirGroup> airGroups = getAvailableDefensiveAirGroups(offensiveAirGroup.ArmyIndex);
            
            if (airGroups.Count > 0)
            {
                if (offensiveAirGroup.Altitude != null && offensiveAirGroup.Altitude.HasValue)
                {
                    if (offensiveAirGroup.TargetGroundGroup != null)
                    {
                        Point3d targetPosition = new Point3d(offensiveAirGroup.TargetGroundGroup.Position.x, offensiveAirGroup.TargetGroundGroup.Position.y, 0.0);
                        AirGroup defensiveAirGroup = getRandomAirGroupBasedOnDistance(airGroups, targetPosition);
                        return defensiveAirGroup;
                    }
                    else if (offensiveAirGroup.TargetStationary != null)
                    {
                        Point3d targetPosition = new Point3d(offensiveAirGroup.TargetStationary.Position.x, offensiveAirGroup.TargetStationary.Position.y, 0.0);
                        AirGroup defensiveAirGroup = getRandomAirGroupBasedOnDistance(airGroups, targetPosition);
                        return defensiveAirGroup;
                    }
                    else if (offensiveAirGroup.TargetArea != null && offensiveAirGroup.TargetArea.HasValue)
                    {
                        Point3d targetPosition = new Point3d(offensiveAirGroup.TargetArea.Value.x, offensiveAirGroup.TargetArea.Value.y, 0.0);
                        AirGroup defensiveAirGroup = getRandomAirGroupBasedOnDistance(airGroups, targetPosition);
                        return defensiveAirGroup;
                    }
                }
            }

            return null;
        }

        private List<AirGroup> getAvailableEscortedAirGroups(int armyIndex)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in AvailableAirGroups)
            {
                if (airGroup.ArmyIndex == armyIndex)
                {
                    IEnumerable<EMissionType> missionTypes = CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes;
                    foreach (EMissionType missionType in missionTypes)
                    {
                        if (isMissionTypeEscorted(missionType) && isMissionTypeAvailable(airGroup, missionType))
                        {
                            airGroups.Add(airGroup);
                            break;
                        }
                    }
                }
            }
            return airGroups;
        }

        private AirGroup getAvailableRandomEscortedAirGroup(AirGroup escortAirGroup)
        {
            List<AirGroup> airGroups = getAvailableEscortedAirGroups(escortAirGroup.ArmyIndex);

            if (airGroups.Count > 0)
            {
                AirGroup escortedAirGroup = getRandomAirGroupBasedOnDistance(airGroups, escortAirGroup);

                return escortedAirGroup;
            }
            else
            {
                return null;
            }
        }

        private AirGroup getAvailableRandomEscortAirGroup(AirGroup escortedAirUnit)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in AvailableAirGroups)
            {
                if (airGroup.ArmyIndex == escortedAirUnit.ArmyIndex)
                {
                    if (CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes.Contains(EMissionType.ESCORT))
                    {
                        airGroups.Add(airGroup);
                    }
                }
            }

            if (airGroups.Count > 0)
            {
                AirGroup escortAirGroup = getRandomAirGroupBasedOnDistance(airGroups, escortedAirUnit);

                return escortAirGroup;
            }
            else
            {
                return null;
            }
        }

        private AircraftParametersInfo getAvailableRandomAircratParametersInfo(AircraftInfo aircraftInfo, EMissionType missionType)
        {
            IList<AircraftParametersInfo> aircraftParametersInfos = aircraftInfo.GetAircraftParametersInfo(missionType);
            int aircraftParametersInfoIndex = Random.Next(aircraftParametersInfos.Count);
            return aircraftParametersInfos[aircraftParametersInfoIndex];

        }

        //private AirGroup getAvailableRandomInterceptAirGroup(AirGroup interceptedAirUnit)
        //{
        //    List<AirGroup> airGroups = new List<AirGroup>();
        //    foreach (AirGroup airGroup in AvailableAirGroups)
        //    {
        //        if (airGroup.ArmyIndex != interceptedAirUnit.ArmyIndex)
        //        {
        //            if (CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes.Contains(EMissionType.INTERCEPT))
        //            {
        //                airGroups.Add(airGroup);
        //            }
        //        }
        //    }

        //    if (airGroups.Count > 0)
        //    {
        //        int interceptAirGroupIndex = getRandomIndex(ref airGroups, interceptedAirUnit.Position);
        //        AirGroup interceptAirGroup = airGroups[interceptAirGroupIndex];

        //        return interceptAirGroup;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        #endregion

        #region Check Mission Type 

        private bool isMissionTypeEscorted(EMissionType missionType)
        {
            if (missionType == EMissionType.ATTACK_ARMOR
                || missionType == EMissionType.ATTACK_SHIP
                || missionType == EMissionType.ATTACK_VEHICLE
                || missionType == EMissionType.ATTACK_TRAIN

                || missionType == EMissionType.ATTACK_RADAR
                || missionType == EMissionType.ATTACK_AIRCRAFT
                || missionType == EMissionType.ATTACK_ARTILLERY
                || missionType == EMissionType.ATTACK_DEPOT)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool isMissionTypeOffensive(EMissionType missionType)
        {
            if (missionType == EMissionType.ARMED_MARITIME_RECON
                || missionType == EMissionType.ARMED_RECON
                || missionType == EMissionType.ATTACK_ARMOR
                || missionType == EMissionType.ATTACK_SHIP
                || missionType == EMissionType.ATTACK_VEHICLE
                || missionType == EMissionType.ATTACK_TRAIN
                || missionType == EMissionType.MARITIME_RECON
                || missionType == EMissionType.RECON

                || missionType == EMissionType.ATTACK_RADAR
                || missionType == EMissionType.ATTACK_AIRCRAFT
                || missionType == EMissionType.ATTACK_ARTILLERY
                || missionType == EMissionType.ATTACK_DEPOT
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool isMissionTypeDefensive(EMissionType missionType)
        {
            if (missionType == EMissionType.INTERCEPT
                || missionType == EMissionType.COVER)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool isMissionTypeAvailable(AirGroup airGroup, EMissionType missionType)
        {
            if (missionType == EMissionType.COVER)
            {
                List<AirGroup> airGroups = getAvailableOffensiveAirGroups(airGroup.ArmyIndex);
                if (airGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ARMED_MARITIME_RECON)
            {
                List<GroundGroup> groundGroups = Generator.GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ARMED_RECON)
            {
                List<GroundGroup> groundGroups = Generator.GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle, EGroundGroupType.Train });
                List<Stationary> stationaries = Generator.GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Aircraft, EStationaryType.Artillery, EStationaryType.Radar, EStationaryType.Depot });
                if (groundGroups.Count > 0 || stationaries.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_ARMOR)
            {
                List<GroundGroup> groundGroups = Generator.GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_RADAR)
            {
                IList<Stationary> stationaries = Generator.GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Radar });
                if (stationaries.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_AIRCRAFT)
            {
                IList<Stationary> stationaries = Generator.GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Aircraft });
                if (stationaries.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_ARTILLERY)
            {
                IList<Stationary> stationaries = Generator.GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Artillery });
                if (stationaries.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_DEPOT)
            {
                IList<Stationary> stationaries = Generator.GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Depot });
                if (stationaries.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_SHIP)
            {
                List<GroundGroup> groundGroups = Generator.GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_VEHICLE)
            {
                List<GroundGroup> groundGroups = Generator.GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Vehicle });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ATTACK_TRAIN)
            {
                List<GroundGroup> groundGroups = Generator.GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Train });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.ESCORT)
            {
                List<AirGroup> airGroups = getAvailableEscortedAirGroups(airGroup.ArmyIndex);
                if (airGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.INTERCEPT)
            {
                List<AirGroup> airGroups = getAvailableOffensiveAirGroups(airGroup.ArmyIndex);
                if (airGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.MARITIME_RECON)
            {
                List<GroundGroup> groundGroups = Generator.GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
                if (groundGroups.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.RECON)
            {
                List<GroundGroup> groundGroups = Generator.GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle, EGroundGroupType.Train });
                List<Stationary> stationaries = Generator.GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Aircraft, EStationaryType.Artillery, EStationaryType.Radar, EStationaryType.Depot });
                if (groundGroups.Count > 0 || stationaries.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (missionType == EMissionType.FOLLOW)
            {
                return AvailableAirGroups.Where(x => x.ArmyIndex == airGroup.ArmyIndex && x != airGroup).Count() > 0;
            }
            else if (missionType == EMissionType.HUNTING)
            {
                return AvailableAirGroups.Where(x => x.ArmyIndex != airGroup.ArmyIndex).Count() > 0;
            }
            else
            {
                throw new NotImplementedException(string.Format("Invalid MissionType[{0}]", missionType.ToString()));
            }
        }

        #endregion
    }
}