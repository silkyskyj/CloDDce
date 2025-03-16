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
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;
using maddox.GP;
using XLAND;
using static IL2DCE.MissionObjectModel.Skill;

namespace IL2DCE.Generator
{
    class GeneratorAirOperation
    {
        #region definition

        private const float SpawnRangeInflateRate = 1.33f;
        private const int SpawnMaxDifDistanceAirstart = 500;
        private const int SpawnMaxDifDistanceAirport = 1500;
        private const int SpawnNeedParkCountFree = 12;

        #endregion

        #region Property & Variable

        private GeneratorGroundOperation GeneratorGroundOperation
        {
            get;
            set;
        }

        private GeneratorBriefing GeneratorBriefing
        {
            get;
            set;
        }

        private CampaignInfo CampaignInfo
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
            get;
            set;
        }

        public wRECTF Range
        {
            get;
            set;
        }

        public List<AirGroup> AssignedAirGroups = new List<AirGroup>();
        public List<AirGroup> AvailableAirGroups = new List<AirGroup>();

        #endregion

        #region Constructor

        public GeneratorAirOperation(IGamePlay gamePlay, Config config, IRandom random, GeneratorGroundOperation generatorGroundOperation, GeneratorBriefing generatorBriefing, CampaignInfo campaignInfo, IEnumerable<AirGroup> airGroups)
        {
            GamePlay = gamePlay;
            Config = config;
            Random = random;
            GeneratorGroundOperation = generatorGroundOperation;
            GeneratorBriefing = generatorBriefing;
            CampaignInfo = campaignInfo;

            AvailableAirGroups.AddRange(airGroups);
#if true
            SetRange(AvailableAirGroups.Select(x => x.Position));
#else
            var groundPositions = GeneratorGroundOperation.AvailableGroundGroups.Select(x => x.Position);
#endif
        }

        #endregion

        #region Get Random object

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
                || missionType == EMissionType.INTERCEPT || missionType == EMissionType.HUNTING)
            {
                // Fighter
#if false
                string[] skills = new string[] {
                    "0.30 0.11 0.78 0.40 0.64 0.85 0.85 0.21",
                    "0.32 0.12 0.87 0.60 0.74 0.90 0.90 0.31",
                    "0.52 0.13 0.89 0.70 0.74 0.95 0.92 0.31",
                    "0.73 0.14 0.92 0.80 0.74 1 0.95 0.41",
                    "0.93 0.15 0.96 0.92 0.84 1 1 0.51",
                };
                return skills[level];
#else
                return Skill.TweakedSkills[level].ToString();
#endif
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
#if false
                string[] skills = new string[] {
                    "0.30 0.11 0.78 0.20 0.74 0.85 0.90 0.88",
                    "0.32 0.12 0.87 0.25 0.74 0.90 0.95 0.91",
                    "0.52 0.13 0.89 0.28 0.74 0.92 0.95 0.91",
                    "0.73 0.14 0.92 0.30 0.74 0.95 0.95 0.95",
                    "0.93 0.15 0.96 0.35 0.74 1.00 1.00 0.97",
                };

                return skills[level];
#else
                return Skill.TweakedSkills[5 + level].ToString();
#endif
            }
        }

        private string getTweakedSkill(EAircraftType aircraftType, int level)
        {
            if (aircraftType == EAircraftType.Fighter)
            {
                // Fighter
                return Skill.TweakedSkills[level].ToString();
            }
            else if (aircraftType == EAircraftType.FighterBomber || aircraftType == EAircraftType.Bomber)
            {
                // Fighter Bomber
                return Skill.TweakedSkills[(int)ETweakedType.FighterBomberRookie + level].ToString();
            }
            else if (aircraftType == EAircraftType.BomberSub || aircraftType == EAircraftType.FighterSub ||
                aircraftType == EAircraftType.FighterBomberSub || aircraftType == EAircraftType.OtherSub)
            {
                // Bomber
                return Skill.TweakedSkills[(int)ETweakedType.BomberRookie + level].ToString();
            }

            // return Skill.TweakedSkills[level].ToString();
            return Skill.TweakedSkills[(int)ETweakedType.BomberRookie + level].ToString();
        }

        private string getRandomSkill(EMissionType missionType, EAircraftType type)
        {
            int randomLevel = Random.Next(0, 4);

            return getTweakedSkill(missionType, randomLevel);
        }

        private string getRandomSkill(EAircraftType aircraftType)
        {
            int randomLevel = Random.Next(0, 4);

            return getTweakedSkill(aircraftType, randomLevel);
        }

        private void getRandomFlightSize(AirGroup airGroup, EMissionType missionType)
        {
            AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
            int flightCount = (int)Math.Ceiling(airGroupInfo.FlightCount * Config.FlightCount);
            int flightSize = (int)Math.Ceiling(airGroupInfo.FlightSize * Config.FlightSize);

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

            airGroup.Flights.Clear();
            int aircraftNumber = 1;
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

        private AirGroup getRandomAirGroupBasedOnDistance(IEnumerable<AirGroup> availableAirGroups, AirGroup referenceAirGroup)
        {
            return getRandomAirGroupBasedOnDistance(availableAirGroups, referenceAirGroup.Position);
        }

        private AirGroup getRandomAirGroupBasedOnDistance(IEnumerable<AirGroup> availableAirGroups, Point2d targetPosition)
        {
            return getRandomAirGroupBasedOnDistance(availableAirGroups, new Point3d(targetPosition.x, targetPosition.y, 0.0));
        }

        private AirGroup getRandomAirGroupBasedOnDistance(IEnumerable<AirGroup> availableAirGroups, Point3d targetPosition)
        {
            AirGroup selectedAirGroup = null;

            if (availableAirGroups.Count() > 1)
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
            else if (availableAirGroups.Count() == 1)
            {
                selectedAirGroup = availableAirGroups.First();
            }

            return selectedAirGroup;
        }

        private AiAirport getRandomAiAirportBasedOnDistance(IEnumerable<AiAirport> aiAirports, Point3d targetPosition)
        {
            if (aiAirports.Count() >= 2)
            {
                var copy = new List<AiAirport>(aiAirports);

                copy.Sort(new DistanceComparer(targetPosition));

                Point3d position = targetPosition;
                Point3d last = copy[copy.Count - 1].Pos();
                double maxDistance = last.distance(ref position);

                List<KeyValuePair<AiAirport, int>> elements = new List<KeyValuePair<AiAirport, int>>();

                int previousWeight = 0;
                foreach (AiAirport aiAirport in copy)
                {
                    double distance = aiAirport.Pos().distance(ref position);
                    int weight = Convert.ToInt32(Math.Ceiling(maxDistance - distance));
                    int cumulativeWeight = previousWeight + weight;
                    elements.Add(new KeyValuePair<AiAirport, int>(aiAirport, cumulativeWeight));
                    previousWeight = cumulativeWeight;
                }

                int diceRoll = Random.Next(0, previousWeight);
                int cumulative = 0;
                for (int i = 0; i < elements.Count; i++)
                {
                    cumulative += elements[i].Value;
                    if (diceRoll <= cumulative)
                    {
                        return elements[i].Key;
                    }
                }
            }

            return aiAirports.FirstOrDefault();
        }

        private int getRandomIndex(ref List<AirGroup> airGroups, Point3d position)
        {
            // Sort the air groups by their distance to the position.
            airGroups.Sort(new DistanceComparer(position));

            List<AirGroup> selectedElement = null;
            AirGroup selectedAirGroup = null;

            int numberOfChunks = 4;
            if (airGroups.Count < 4)
            {
                // Split the air groups into chunks based on their distance to the position.
                List<List<AirGroup>> airGroupChunks = Collection.SplitList(airGroups, numberOfChunks);

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

        private EMissionType? GetRandomMissionType(AirGroup airGroup)
        {
            IEnumerable<EMissionType> availableMissionTypes = GetAvailableMissionTypes(airGroup);
            if (availableMissionTypes.Any())
            {
                int randomMissionTypeIndex = Random.Next(availableMissionTypes.Count());
                return availableMissionTypes.ElementAt(randomMissionTypeIndex);
            }
            return null;
        }

        #endregion

        #region Create air Operation

        public bool CreateRandomAirOperation(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, bool spawnRandom, Skill[] skill = null, Spawn spawn = null, int speed = -1, int fuel = -1)
        {
            bool result = false;
            IEnumerable<EMissionType> availableMissionTypes = GetAvailableMissionTypes(airGroup);
            if (availableMissionTypes.Any())
            {
                int randomMissionTypeIndex = Random.Next(availableMissionTypes.Count());
                EMissionType randomMissionType = availableMissionTypes.ElementAt(randomMissionTypeIndex);
                result = CreateAirOperation(sectionFile, briefingFile, airGroup, spawnRandom, randomMissionType, true, null, null, null, skill, spawn, speed, fuel);
            }
            else
            {
                AvailableAirGroups.Remove(airGroup);
            }
            return result;
        }

        public bool CreateAirOperation(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, bool spawnRandom, EMissionType missionType, bool allowDefensiveOperation,
            AirGroup forcedEscortAirGroup, GroundGroup forcedTargetGroundGroup, Stationary forcedTargetStationary, Skill[] skill = null, Spawn spawn = null, int speed = -1, int fuel = -1)
        {
            bool result = false;
            if (isMissionTypeAvailable(airGroup, missionType))
            {
                AvailableAirGroups.Remove(airGroup);
                // jamRunway(airGroup);

                airGroup.SetOnParked = Config.SpawnParked;
                OptimizeSpawnPosition(airGroup, spawnRandom);

                AircraftInfo aircraftInfo = CampaignInfo.GetAircraftInfo(airGroup.Class);
                AircraftParametersInfo aircraftParametersInfo = getAvailableRandomAircratParametersInfo(aircraftInfo, missionType);
                AircraftLoadoutInfo aircraftLoadoutInfo = aircraftInfo.GetAircraftLoadoutInfo(aircraftParametersInfo.LoadoutId);
                airGroup.Weapons = aircraftLoadoutInfo.Weapons;
                airGroup.Detonator = aircraftLoadoutInfo.Detonator;
                // airGroup.TraceLoadoutInfo();

                AirGroup escortAirGroup = forcedEscortAirGroup;
                if (escortAirGroup == null && isMissionTypeEscorted(missionType))  // if Mission is Attack ground actor or Attack static aircraft ... 
                {
                    escortAirGroup = getAvailableRandomEscortAirGroup(airGroup);
                }

                if (missionType == EMissionType.RECON || missionType == EMissionType.MARITIME_RECON)
                {
                    result = CreateAirOperationRecon(sectionFile, airGroup, missionType, aircraftParametersInfo, forcedTargetGroundGroup, forcedTargetStationary, escortAirGroup);
                }
                else if (missionType == EMissionType.ATTACK_ARTILLERY || missionType == EMissionType.ATTACK_RADAR
                        || missionType == EMissionType.ATTACK_AIRCRAFT || missionType == EMissionType.ATTACK_DEPOT)
                {
                    result = CreateAirOperationAttackArtillery(airGroup, missionType, aircraftParametersInfo, forcedTargetStationary, escortAirGroup);
                }
                else if (missionType == EMissionType.ATTACK_ARMOR || missionType == EMissionType.ATTACK_VEHICLE ||
                        missionType == EMissionType.ATTACK_TRAIN || missionType == EMissionType.ATTACK_SHIP ||
                        missionType == EMissionType.ARMED_RECON || missionType == EMissionType.ARMED_MARITIME_RECON)
                {
                    result = CreateAirOperationAttackArmor(sectionFile, airGroup, missionType, aircraftParametersInfo, forcedTargetGroundGroup, forcedTargetStationary, escortAirGroup);
                }
                else if (missionType == EMissionType.INTERCEPT)
                {
                    result = CreateAirOperationIntercept(sectionFile, briefingFile, airGroup, spawnRandom);
                }
                else if (missionType == EMissionType.ESCORT)
                {
                    result = CreateAirOperationEscort(sectionFile, briefingFile, airGroup, spawnRandom);
                }
                else if (missionType == EMissionType.COVER)
                {
                    result = CreateAirOperationCover(sectionFile, briefingFile, airGroup, spawnRandom);
                }
                else if (missionType == EMissionType.FOLLOW)
                {
                    result = CreateAirOperationFollow(sectionFile, briefingFile, airGroup, spawnRandom);
                }
                else if (missionType == EMissionType.HUNTING)
                {
                    result = CreateAirOperationHunting(sectionFile, briefingFile, airGroup, spawnRandom);
                }
                else if (missionType == EMissionType.TRANSFER)
                {
                    result = CreateAirOperationTransfer(sectionFile, briefingFile, airGroup);
                }

                // Debug.Assert(result);

                if (result)
                {
                    // Flight Size
                    getRandomFlightSize(airGroup, missionType);

                    // Skill
                    SetSkill(airGroup, aircraftInfo.AircraftType, skill);

                    // Spawn
                    if (spawn != null)
                    {
                        airGroup.SetSpawn(spawn);
                    }

                    // Speed
                    airGroup.SetSpeed(speed);

                    // Fuel
                    airGroup.SetFuel(fuel);

                    // Create Briefing 
                    GeneratorBriefing.CreateBriefing(briefingFile, airGroup, missionType, escortAirGroup);

                    // Write to Mission file
                    airGroup.WriteTo(sectionFile);

                    // create relational operation 1 (Friendly group Operation)
                    if (forcedEscortAirGroup == null && escortAirGroup != null)
                    {
                        escortAirGroup.SetOnParked = Config.SpawnParked;
                        OptimizeSpawnPosition(escortAirGroup, spawnRandom);
                        CreateAirOperationEscorted(sectionFile, briefingFile, escortAirGroup, airGroup);
                        AssignedAirGroups.Add(escortAirGroup);
                    }

                    // create relational operation 2 (Enemy group Operation)
                    if (isMissionTypeOffensive(missionType) && allowDefensiveOperation)
                    {
                        AirGroup defensiveAirGroup = getAvailableRandomDefensiveAirGroup(airGroup);
                        if (defensiveAirGroup != null)
                        {
                            defensiveAirGroup.SetOnParked = Config.SpawnParked;
                            OptimizeSpawnPosition(defensiveAirGroup, spawnRandom);
                            CreateAirOperationDefensiv(sectionFile, briefingFile, airGroup, defensiveAirGroup);
                            AssignedAirGroups.Add(defensiveAirGroup);
                        }
                    }
                }
            }
            else
            {
                // throw new ArgumentException(string.Format("no available MissionType[{0}]", missionType.ToDescription()));
            }

            if (result)
            {
                AssignedAirGroups.Add(airGroup);
            }

            return result;
        }

        #endregion 

        #region Create air Operation Detail

        private bool CreateAirOperationRecon(ISectionFile sectionFile, AirGroup airGroup, EMissionType missionType, AircraftParametersInfo aircraftParametersInfo, GroundGroup forcedTargetGroundGroup, Stationary forcedTargetStationary, AirGroup escortAirGroup)
        {
            bool result = false;
            GroundGroup groundGroup = forcedTargetGroundGroup;
            Stationary stationary = forcedTargetStationary;

            do
            {
                if (groundGroup == null)
                {
                    groundGroup = GeneratorGroundOperation.getAvailableRandomEnemyGroundGroup(airGroup, missionType);
                    if (groundGroup == null)
                    {
                        if (stationary == null)
                        {
                            stationary = GeneratorGroundOperation.getAvailableRandomEnemyStationary(airGroup, missionType);
                            if (stationary == null)
                            {
                                break;
                            }
                        }
                    }
                }
                double altitude = getRandomAltitude(aircraftParametersInfo);
                if (groundGroup != null)
                {
                    if (GeneratorGroundOperation.CreateRandomGroundOperation(sectionFile, groundGroup))
                    {
                        airGroup.Recon(groundGroup, altitude, escortAirGroup);
                        result = true;
                    }
                    else
                    {
                        airGroup = null;
                    }
                }
                else if (stationary != null)
                {
                    airGroup.Recon(stationary, altitude, escortAirGroup);
                    result = true;
                }
            } while (!result);
            return result;
        }

        private bool CreateAirOperationAttackArtillery(AirGroup airGroup, EMissionType missionType, AircraftParametersInfo aircraftParametersInfo, Stationary forcedTargetStationary, AirGroup escortAirGroup)
        {
            bool result = false;
            Stationary stationary = forcedTargetStationary;
            if (stationary == null)
            {
                stationary = GeneratorGroundOperation.getAvailableRandomEnemyStationary(airGroup, missionType);
            }
            // No need to generate a random ground operation for the stationary as the stationary objects are always generated
            // into the file.
            //GeneratorGroundOperation.CreateRandomGroundOperation(sectionFile, stationary);
            double altitude = getRandomAltitude(aircraftParametersInfo);

            airGroup.GroundAttack(stationary, altitude, escortAirGroup);
            result = true;
            return result;
        }

        private bool CreateAirOperationAttackArmor(ISectionFile sectionFile, AirGroup airGroup, EMissionType missionType, AircraftParametersInfo aircraftParametersInfo, GroundGroup forcedTargetGroundGroup, Stationary forcedTargetStationary, AirGroup escortAirGroup)
        {
            bool result = false;
            GroundGroup groundGroup = forcedTargetGroundGroup;
            Stationary stationary = forcedTargetStationary;

            do
            {
                if (groundGroup == null)
                {
                    groundGroup = GeneratorGroundOperation.getAvailableRandomEnemyGroundGroup(airGroup, missionType);
                    if (groundGroup == null)
                    {
                        if (stationary == null)
                        {
                            stationary = GeneratorGroundOperation.getAvailableRandomEnemyStationary(airGroup, missionType);
                            if (stationary == null)
                            {
                                break;
                            }
                        }
                    }
                }
                double altitude = getRandomAltitude(aircraftParametersInfo);
                if (groundGroup != null)
                {
                    if (GeneratorGroundOperation.CreateRandomGroundOperation(sectionFile, groundGroup))
                    {
                        airGroup.GroundAttack(groundGroup, altitude, escortAirGroup);
                        result = true;
                    }
                    else
                    {
                        airGroup = null;
                    }
                }
                else if (stationary != null)
                {
                    airGroup.GroundAttack(stationary, altitude, escortAirGroup);
                    result = true;
                }
            } while (!result);
            return result;
        }

        private bool CreateAirOperationIntercept(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, bool spawnRandom)
        {
            bool result = false;
            GroundGroup targetGroundGroup;
            Stationary targetStationary;
            EMissionType offensiveMissionType;
            AirGroup offensiveAirGroup = getAvailableRandomOffensiveAirGroup(airGroup, out offensiveMissionType, out targetGroundGroup, out targetStationary);
            if (offensiveAirGroup != null)
            {
                CreateAirOperation(sectionFile, briefingFile, offensiveAirGroup, spawnRandom, offensiveMissionType, false, null, targetGroundGroup, targetStationary);
                airGroup.Intercept(offensiveAirGroup);
                result = true;
            }
            return result;
        }

        private bool CreateAirOperationEscort(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, bool spawnRandom)
        {
            bool result = false;
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
                    CreateAirOperation(sectionFile, briefingFile, escortedAirGroup, spawnRandom, randomEscortedMissionType, true, airGroup, null, null);

                    airGroup.Escort(escortedAirGroup);
                    result = true;
                }
            }
            return result;
        }

        private bool CreateAirOperationCover(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, bool spawnRandom)
        {
            bool result = false;
            GroundGroup targetGroundGroup;
            Stationary targetStationary;
            EMissionType offensiveMissionType;
            AirGroup offensiveAirGroup = getAvailableRandomOffensiveAirGroup(airGroup, out offensiveMissionType, out targetGroundGroup, out targetStationary);
            if (offensiveAirGroup != null)
            {
                CreateAirOperation(sectionFile, briefingFile, offensiveAirGroup, spawnRandom, offensiveMissionType, false, null, targetGroundGroup, targetStationary);
                if (offensiveAirGroup.Altitude.HasValue && (offensiveAirGroup.TargetGroundGroup != null || offensiveAirGroup.TargetStationary != null))
                {
                    airGroup.Cover(offensiveAirGroup, offensiveAirGroup.Altitude.Value);
                    result = true;
                }
            }
            return result;
        }

        private bool CreateAirOperationFollow(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, bool spawnRandom)
        {
            var friendlyAirGroups = AvailableAirGroups.Where(x => x.ArmyIndex == airGroup.ArmyIndex && x != airGroup).ToArray();
            foreach (var item in friendlyAirGroups)
            {
                AirGroup targetAirGroup = getRandomAirGroupBasedOnDistance(friendlyAirGroups, airGroup);
                EMissionType? missionType = GetRandomMissionType(targetAirGroup);
                if (missionType != null)
                {
                    if (CreateAirOperation(sectionFile, briefingFile, targetAirGroup, spawnRandom, missionType.Value, true, null, null, null)
                        && GetTargetPoint(targetAirGroup) != null)
                    {
                        airGroup.Follow(targetAirGroup);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CreateAirOperationHunting(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, bool spawnRandom)
        {
            var enemyAirGroups = AvailableAirGroups.Where(x => x.ArmyIndex != airGroup.ArmyIndex).ToArray();
            foreach (var item in enemyAirGroups)
            {
                AirGroup targetAirGroup = getRandomAirGroupBasedOnDistance(enemyAirGroups, airGroup);
                EMissionType? missionType = GetRandomMissionType(targetAirGroup);
                if (missionType != null)
                {
                    if (CreateAirOperation(sectionFile, briefingFile, targetAirGroup, spawnRandom, missionType.Value, true, null, null, null)
                        && GetTargetPoint(targetAirGroup) != null && GetTargetAltitude(targetAirGroup) != null)
                    {
                        Point2d? targetPoint = GetTargetPoint(targetAirGroup);
                        double? targetAltitude = GetTargetAltitude(targetAirGroup);
                        airGroup.Hunting(targetPoint.Value, targetAltitude.Value);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CreateAirOperationTransfer(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup)
        {
            AiAirport[] aiAirports = GamePlay.gpAirports();
            Point3d point;
            IEnumerable<AiAirport> aiAirportFriendly = aiAirports.Where(x => { point = x.Pos(); return (GamePlay.gpFrontArmy(point.x, point.y) == airGroup.ArmyIndex); });
            AiAirport aiAirportTarget = getRandomAiAirportBasedOnDistance(aiAirportFriendly, airGroup.Position);
            if (aiAirportTarget != null)
            {
                AircraftInfo aircraftInfo = CampaignInfo.GetAircraftInfo(airGroup.Class);
                AircraftParametersInfo aircraftParam = getAvailableRandomAircratParametersInfo(aircraftInfo, EMissionType.TRANSFER);
                int startAlt = aircraftParam != null && aircraftParam.MinAltitude != null ? Math.Max((int)aircraftParam.MinAltitude.Value, Spawn.SelectStartAltitude) : Spawn.SelectStartAltitude;
                int endAlt = aircraftParam != null && aircraftParam.MaxAltitude != null ? Math.Min((int)aircraftParam.MaxAltitude.Value, Spawn.SelectEndAltitude) : Spawn.SelectEndAltitude;
                double altitude = Random.Next(startAlt, endAlt);
                airGroup.Transfer(altitude, aiAirportTarget);
                return true;
            }
            return false;
        }

        private bool CreateAirOperationEscorted(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, AirGroup airGroupEscorted)
        {
            bool result = false;
            // TODO: Consider calling CreateAirOperation with a forcedEscortedAirGroup.
            AvailableAirGroups.Remove(airGroup);
            // jamRunway(airGroup);

            AircraftInfo aircraftInfo = CampaignInfo.GetAircraftInfo(airGroup.Class);
            AircraftParametersInfo aircraftParametersInfo = getAvailableRandomAircratParametersInfo(aircraftInfo, EMissionType.ESCORT);
            AircraftLoadoutInfo aircraftLoadoutInfo = aircraftInfo.GetAircraftLoadoutInfo(aircraftParametersInfo.LoadoutId);
            airGroup.Weapons = aircraftLoadoutInfo.Weapons;

            airGroup.Escort(airGroupEscorted);

            getRandomFlightSize(airGroup, EMissionType.ESCORT);
            airGroup.Skill = getRandomSkill(aircraftInfo.AircraftType);
            GeneratorBriefing.CreateBriefing(briefingFile, airGroup, EMissionType.ESCORT, null);
            airGroup.WriteTo(sectionFile);
            return result;
        }

        private bool CreateAirOperationDefensiv(ISectionFile sectionFile, BriefingFile briefingFile, AirGroup airGroup, AirGroup defensiveAirGroup)
        {
            bool result = false;
            AvailableAirGroups.Remove(defensiveAirGroup);
            // jamRunway(defensiveAirGroup);

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

                AircraftInfo aircraftInfo = CampaignInfo.GetAircraftInfo(defensiveAirGroup.Class);
                AircraftParametersInfo aircraftParametersInfo = getAvailableRandomAircratParametersInfo(aircraftInfo, randomDefensiveMissionType);
                AircraftLoadoutInfo aircraftLoadoutInfo = aircraftInfo.GetAircraftLoadoutInfo(aircraftParametersInfo.LoadoutId);
                defensiveAirGroup.Weapons = aircraftLoadoutInfo.Weapons;
                defensiveAirGroup.Detonator = aircraftLoadoutInfo.Detonator;
                // defensiveAirGroup.TraceLoadoutInfo();

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
                defensiveAirGroup.Skill = getRandomSkill(aircraftInfo.AircraftType);
                GeneratorBriefing.CreateBriefing(briefingFile, defensiveAirGroup, randomDefensiveMissionType, null);
                defensiveAirGroup.WriteTo(sectionFile);
                result = true;
            }
            return result;
        }

#if false
        private bool CreateAirOperationRecon(ISectionFile sectionFile, AirGroup airGroup, AircraftParametersInfo aircraftParametersInfo, AirGroup escortAirGroup, GroundGroup groundGroup, Stationary stationary)
        {
            bool result = false;
            //GroundGroup groundGroup = forcedTargetGroundGroup;
            //Stationary stationary = forcedTargetStationary;
            if (groundGroup == null && stationary == null)
            {
                groundGroup = GeneratorGroundOperation.getAvailableRandomEnemyGroundGroup(airGroup, missionType);
                stationary = GeneratorGroundOperation.getAvailableRandomEnemyStationary(airGroup, missionType);

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
                GeneratorGroundOperation.CreateRandomGroundOperation(sectionFile, groundGroup);
                airGroup.Recon(groundGroup, altitude, escortAirGroup);
                result = true;
            }
            else if (stationary != null)
            {
                airGroup.Recon(stationary, altitude, escortAirGroup);
                result = true;
            }

            return result;
        }
#endif

        #endregion

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

                        GroundGroup possibleTargetGroundGroup = GeneratorGroundOperation.getAvailableRandomEnemyGroundGroup(possibleOffensiveAirGroup, possibleOffensiveMissionType);
                        Stationary possibleTargetStationary = GeneratorGroundOperation.getAvailableRandomEnemyStationary(possibleOffensiveAirGroup, possibleOffensiveMissionType);

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
                targetGroundGroup = GeneratorGroundOperation.getRandomTargetBasedOnRange(possibleTargetGroundGroups, defensiveAirGroup);
                targetStationary = GeneratorGroundOperation.getRandomTargetBasedOnRange(possibleTargetStationaries, defensiveAirGroup);

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

        private AirGroup getAvailableRandomEscortAirGroup(AirGroup escortedAirGroup)
        {
            List<AirGroup> airGroups = new List<AirGroup>();
            foreach (AirGroup airGroup in AvailableAirGroups)
            {
                if (airGroup.ArmyIndex == escortedAirGroup.ArmyIndex)
                {
                    if (CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes.Contains(EMissionType.ESCORT))
                    {
                        airGroups.Add(airGroup);
                    }
                }
            }

            if (airGroups.Count > 0)
            {
                AirGroup escortAirGroup = getRandomAirGroupBasedOnDistance(airGroups, escortedAirGroup);
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
                List<GroundGroup> groundGroups = GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
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
                List<GroundGroup> groundGroups = GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle, EGroundGroupType.Train });
                List<Stationary> stationaries = GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Aircraft, EStationaryType.Artillery, EStationaryType.Radar, EStationaryType.Depot });
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
                List<GroundGroup> groundGroups = GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor });
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
                IList<Stationary> stationaries = GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Radar });
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
                IList<Stationary> stationaries = GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Aircraft });
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
                IList<Stationary> stationaries = GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Artillery });
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
                IList<Stationary> stationaries = GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Depot });
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
                List<GroundGroup> groundGroups = GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
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
                List<GroundGroup> groundGroups = GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Vehicle });
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
                List<GroundGroup> groundGroups = GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Train });
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
                List<GroundGroup> groundGroups = GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Ship });
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
                List<GroundGroup> groundGroups = GeneratorGroundOperation.getAvailableEnemyGroundGroups(airGroup.ArmyIndex, new List<EGroundGroupType> { EGroundGroupType.Armor, EGroundGroupType.Vehicle, EGroundGroupType.Train });
                List<Stationary> stationaries = GeneratorGroundOperation.getAvailableEnemyStationaries(airGroup.ArmyIndex, new List<EStationaryType> { EStationaryType.Aircraft, EStationaryType.Artillery, EStationaryType.Radar, EStationaryType.Depot });
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
                return AvailableAirGroups.Any(x => x.ArmyIndex == airGroup.ArmyIndex && x != airGroup);
                // return AvailableAirGroups.Where(x => x.ArmyIndex == airGroup.ArmyIndex && x != airGroup).Any();
            }
            else if (missionType == EMissionType.HUNTING)
            {
                return AvailableAirGroups.Any(x => x.ArmyIndex != airGroup.ArmyIndex);
                // return AvailableAirGroups.Where(x => x.ArmyIndex != airGroup.ArmyIndex).Any();
            }
            else if (missionType == EMissionType.TRANSFER)
            {
                return true;
            }
            else
            {
                throw new NotImplementedException(string.Format("Invalid MissionType[{0}]", missionType.ToString()));
            }
        }

        private bool isMissionTypePassivelyGenerated(EMissionType missionType)
        {
            if (missionType == EMissionType.FOLLOW
                || missionType == EMissionType.ESCORT
                /*|| missionType == EMissionType.HUNTING
                || missionType == EMissionType.INTERCEPT
                || missionType == EMissionType.COVER*/)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        public IEnumerable<EMissionType> GetAvailableMissionTypes(AirGroup airGroup)
        {
            return CampaignInfo.GetAircraftInfo(airGroup.Class).MissionTypes.Where(x => isMissionTypeAvailable(airGroup, x));
        }

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

        private void SetSkill(AirGroup airGroup, EAircraftType aircraftType, Skill[] skill)
        {
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
                airGroup.Skill = getRandomSkill(aircraftType);
                airGroup.Skills.Clear();
            }
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
                int wayIdx = (airGroup.Waypoints.Count - 1) / 2;
                AirGroupWaypoint way = airGroup.Waypoints[wayIdx];
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
                int wayIdx = (airGroup.Waypoints.Count - 1) / 2;
                AirGroupWaypoint way = airGroup.Waypoints[wayIdx];
                return way.Z;
            }

            return null;
        }

        private void SetRange(IEnumerable<Point3d> points)
        {
#if false
            wRECTF range = new wRECTF();
            range.x1 = (float)points.Min(x => x.x);
            range.x2 = (float)points.Max(x => x.x);
            range.y1 = (float)points.Min(x => x.y);
            range.y2 = (float)points.Max(x => x.y);
#else
            wRECTF range = new wRECTF() { x1 = float.MaxValue, x2 = float.MinValue, y1 = float.MaxValue, y2 = float.MinValue };
            foreach (var item in points)
            {
                if (item.x < range.x1)
                {
                    range.x1 = (float)item.x;
                }
                if (item.x > range.x2)
                {
                    range.x2 = (float)item.x;
                }
                if (item.y < range.y1)
                {
                    range.y1 = (float)item.y;
                }
                if (item.y > range.y2)
                {
                    range.y2 = (float)item.y;
                }
            }
#endif
            Debug.WriteLine("SetRange({0},{1})-({2},{3})[{4},{5}]", range.x1, range.y1, range.x2, range.y2, range.x2 - range.x1 + 1, range.y2 - range.y1 + 1);
            Range = range;
        }

        private void OptimizeSpawnPosition(AirGroup airGroup, bool random = false)
        {
            Point3d point = airGroup.Position;
            Debug.Write(string.Format("Optimize AirGroup Spawn Position Name={0} Pos={1}", airGroup.DisplayDetailName, point.ToString()));
            AirGroupWaypoint way = airGroup.Waypoints.FirstOrDefault();
            if (way != null)
            {
                if (random)
                {
                    wRECTF range = Range;
                    // Debug.WriteLine("Range({0},{1})-({2},{3})[{4},{5}]", range.x1, range.y1, range.x2, range.y2, range.x2 - range.x1 + 1, range.y2 - range.y1 + 1);
                    if (airGroup.Airstart)
                    {
                        do
                        {
                            point.x = Random.Next((int)range.x1, (int)range.x2);
                            point.y = Random.Next((int)range.y1, (int)range.y2);
                        } while (GamePlay.gpFrontArmy(point.x, point.y) != airGroup.ArmyIndex);
                        airGroup.UpdateStartPoint(ref point);
                        Debug.WriteLine(string.Format(" => AirStart Pos Changed={0}", airGroup.Position.ToString()));
                    }
                    else
                    {
                        MapUtil.InflateRate(ref range, SpawnRangeInflateRate);
                        IEnumerable<Point3d> posAirGroups = AssignedAirGroups.Select(x => x.Position);
                        Point3d pointAirport;
                        IEnumerable<AiAirport> aiAirports = GamePlay.gpAirports().Where(x =>
                        {
                            pointAirport = x.Pos();
                            return GamePlay.gpFrontArmy(pointAirport.x, pointAirport.y) == airGroup.ArmyIndex &&
                                    /*x.ParkCountFree() > SpawnNeedParkCountFree && */MapUtil.IsInRange(ref range, ref pointAirport) &&
                                    !posAirGroups.Any(y => y.distance(ref pointAirport) < SpawnMaxDifDistanceAirport);
                        });
                        if (aiAirports.Any())
                        {
                            AiAirport aiAirport = aiAirports.ElementAt(Random.Next(aiAirports.Count()));
                            pointAirport = aiAirport.Pos();
                            airGroup.UpdateStartPoint(ref pointAirport);
                            Debug.WriteLine(string.Format(" => Airport Name={0} => Pos Changed={1}", aiAirport.Name(), airGroup.Position.ToString()));
                        }
                        else
                        {
                            airGroup.SetOnParked = true;
                            Debug.WriteLine(string.Format(" => Pos no Changed. SetOnParked={0}", airGroup.SetOnParked));
                        }
                    }
                }
                else
                {
                    if (airGroup.Airstart)
                    {
                        bool updated = false;
                        IEnumerable<Point3d> posAirGroups = AssignedAirGroups.Select(x => x.Position);
                        while (posAirGroups.Where(x => x.distance(ref point) < SpawnMaxDifDistanceAirstart).Any())
                        {
                            point.z += Random.Next(SpawnMaxDifDistanceAirstart / 2, SpawnMaxDifDistanceAirstart);
                            updated = true;
                        }
                        if (updated)
                        {
                            airGroup.UpdateStartPoint(ref point);
                        }
                        Debug.WriteLine(updated ? string.Format(" => Altitude Changed={0}, Pos={1}", updated, airGroup.Position.ToString()): string.Empty);
                    }
                    else
                    {
                        if (AssignedAirGroups.Select(x => x.Position).Any(x => x.distance(ref point) < SpawnMaxDifDistanceAirport))
                        //if (AssignedAirGroups.Select(x => x.Position).Where(x => x.distance(ref point) < SpawnMaxDifDistanceAirport).Any())
                        {
                            airGroup.SetOnParked = true;
                        }
                        Debug.WriteLine(airGroup.SetOnParked ? string.Format(" => Pos no Changed. SetOnParked={0}", airGroup.SetOnParked): string.Empty);
                    }
                }
            }
        }
    }
}