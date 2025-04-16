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
using IL2DCE.MissionObjectModel;
using maddox.game;
using maddox.GP;
using static IL2DCE.MissionObjectModel.Spawn;

namespace IL2DCE.Generator
{
    /// <summary>
    /// The generator for the next mission.
    /// </summary>
    /// <remarks>
    /// This is a interface for the different generators responsible for different parts in the mission generation.
    /// </remarks>
    class Generator : GeneratorBase
    {
        private const string ValueAircraft = "Aircraft";
        private const string ValueStationary = "Stationary";

        #region property 

        internal GeneratorAirOperation GeneratorAirOperation
        {
            get;
            set;
        }

        internal GeneratorGroundOperation GeneratorGroundOperation
        {
            get;
            set;
        }

        internal GeneratorBriefing GeneratorBriefing
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

        #region Constructor

        public Generator(IGamePlay gamePlay, IRandom random, Config config, Career career)
            : base (gamePlay, random, config)
        {
            Career = career;
        }

        #endregion

        public void GenerateInitialMissionTempalte(IEnumerable<string> initialMissionTemplateFiles, out ISectionFile initialMissionTemplateFile, AirGroupInfos airGroupInfos = null)
        {
            initialMissionTemplateFile = null;
            foreach (string fileName in initialMissionTemplateFiles)
            {
                // Use the first template file to load the map.
                initialMissionTemplateFile = GamePlay.gpLoadSectionFile(fileName);
                break;
            }

            if (initialMissionTemplateFile != null)
            {
                // Delete everything from the template file.
                if (initialMissionTemplateFile.exist(MissionFile.SectionAirGroups))
                {
                    SilkySkyCloDFile.DeleteSectionAndSubSection(initialMissionTemplateFile, MissionFile.SectionAirGroups, new string[] { MissionFile.SectionWay });
                }

                if (initialMissionTemplateFile.exist(MissionFile.SectionChiefs))
                {
                    SilkySkyCloDFile.DeleteSectionAndSubSection(initialMissionTemplateFile, MissionFile.SectionChiefs, new string[] { MissionFile.SectionRoad });
                }

                MissionFile initialMission = new MissionFile(GamePlay, initialMissionTemplateFiles, airGroupInfos);

                foreach (AirGroup airGroup in initialMission.AirGroups)
                {
#if DEBUG
                    AirGroupWaypoint way = airGroup.Waypoints.FirstOrDefault();
                    Debug.WriteLine("Init Name={0} Pos=({1:F2},{2:F2},{3:F2}) V={4:F2}, AirStart={5}, SetOnParked={6}, SpawnFromScript={7}({8})", airGroup.DisplayDetailName, way.X, way.Y, way.Z, way.V, airGroup.Airstart, airGroup.SetOnParked, airGroup.SpawnFromScript, airGroup.Spawn != null ? airGroup.Spawn.Time.Value.ToString() : string.Empty);
#endif
                    airGroup.WriteTo(initialMissionTemplateFile);
                }

                foreach (GroundGroup groundGroup in initialMission.GroundGroups)
                {
#if DEBUG
                    GroundGroupWaypoint way = groundGroup.Waypoints.FirstOrDefault();
                    Debug.WriteLine("Init Class={0} Id={1} Pos=({2:F2},{3:F2}) V={4:F2}", groundGroup.Class, groundGroup.Id, way.X, way.Y, way.V.Value);
#endif
                    groundGroup.WriteTo(initialMissionTemplateFile);
                }
            }
        }

        /// <summary>
        /// Generates the next mission template based on the initial mission template. 
        /// </summary>
        /// <param name="staticTemplateFiles"></param>
        /// <param name="initialMissionTemplate"></param>
        /// <param name="missionTemplateFile"></param>
        /// <param name="airGroupInfos"></param>
        /// <remarks>
        /// For now it has a simplified implementaiton. It only generated random supply ships and air groups.
        /// </remarks>
        public void GenerateMissionTemplate(IEnumerable<string> staticTemplateFiles, ISectionFile initialMissionTemplate, out ISectionFile missionTemplateFile, AirGroupInfos airGroupInfos = null)
        {
            MissionFile staticTemplateFile = new MissionFile(GamePlay, staticTemplateFiles, airGroupInfos);

            // Use the previous mission template to initialise the next mission template.
            missionTemplateFile = initialMissionTemplate;

            // Remove the ground groups & stationaries but keep the air groups.

            // Delete all ground groups from the template file. (=initialMissionTemplate)
            if (missionTemplateFile.exist(MissionFile.SectionChiefs))
            {
                SilkySkyCloDFile.DeleteSectionAndSubSection(missionTemplateFile, MissionFile.SectionChiefs, new string[] { MissionFile.SectionRoad });
            }

            // Delete all stationaries from the template file.
            if (missionTemplateFile.exist(MissionFile.SectionStationary))
            {
                missionTemplateFile.delete(MissionFile.SectionStationary);
            }

            if (missionTemplateFile.exist(MissionFile.SectionFrontMarker))
            {
                missionTemplateFile.delete(MissionFile.SectionFrontMarker);
            }

            // Generate supply ships and trains.

            // For now generate a random supply ship on one of the routes to a harbour.
            int chiefIndex = 0;
            int stationaryIndex = 0;

            #region Chiefs

            // 0. Chief                         - Red/Army in Mission File Load process)
            GeneratorGroundOperation.CreateGroundGroups(GamePlay, Career.GroundGroupGenerateType, missionTemplateFile, staticTemplateFile.GroundGroups, ref chiefIndex);

            // 1. Roads (Chiefs Vehicle/Armor)  - None in Mission File Load process)
            // TODO: Only create a random 
            GeneratorGroundOperation.CreateGroundGroupsRoads(GamePlay, Career.GroundGroupGenerateType, missionTemplateFile, staticTemplateFile.Roads, ref chiefIndex);

            // 2. Waterways (Chiefs Ship)       - None in Mission File Load process)
            // TODO: Only create a random (or decent) amount of supply ships.
            GeneratorGroundOperation.CreateGroundGroupsWaterways(GamePlay, Career.GroundGroupGenerateType, missionTemplateFile, staticTemplateFile.Waterways, ref chiefIndex);

            // 3. Railways (Chiefs Train)       - None in Mission File Load process)
            GeneratorGroundOperation.CreateGroundGroupsRailways(GamePlay, Career.GroundGroupGenerateType, missionTemplateFile, staticTemplateFile.Railways, ref chiefIndex);

            #endregion

            #region Stationary

            // 4. Stationary         - Red/Army in Mission File Load process)
            GeneratorGroundOperation.CreateStationaries(GamePlay, Career.StationaryGenerateType, missionTemplateFile, staticTemplateFile.Stationaries, ref stationaryIndex);

            // 5. Depots (Buildings) - None in Mission File Load process)
            GeneratorGroundOperation.CreateStationaryBuildings(GamePlay, Career.StationaryGenerateType, missionTemplateFile, staticTemplateFile.Depots, ref stationaryIndex);

            //6. Other              - None in Mission File Load process)
            GeneratorGroundOperation.CreateStationaries(GamePlay, Career.StationaryGenerateType, missionTemplateFile, staticTemplateFile.OtherStationaries, ref stationaryIndex);

            #endregion

            // 10. FrontMarker
            int i = 0;
            foreach (Point3d point in staticTemplateFile.FrontMarkers)
            {
                string key = string.Format(Config.NumberFormat, "{0}{1}", MissionFile.SectionFrontMarker, i + 1);
                string value = string.Format(Config.NumberFormat, "{0:F2} {1:F2} {2}", point.x, point.y, (int)point.z);
                missionTemplateFile.add(MissionFile.SectionFrontMarker, key, value);
                i++;
            }
        }

        private void OptimizeMissionObjects(MissionFile missionFile, MissionStatus missionStatus, AirGroup airGroupPlayer)
        {
            if (missionStatus != null)
            {
                // Stationaries
                List<Stationary> stationaries = missionFile.Stationaries;
                for (int i = stationaries.Count - 1; i >= 0; i--)
                {
                    Stationary stationary = stationaries[i];
                    MissionStatus.StationaryObject stationaryObject = missionStatus.Stationaries.Where(x => string.Compare(x.Name, stationary.Id) == 0 && string.Compare(x.Class, stationary.Class) == 0).FirstOrDefault();
                    if (stationaryObject != null && !stationaryObject.IsAlive)
                    {
                        stationaries.Remove(stationary);
                    }
                }

                // AirGroups
                List<AirGroup> airGroups = missionFile.AirGroups;
                for (int i = airGroups.Count - 1; i >= 0; i--)
                {
                    AirGroup airGroup = airGroups[i];
                    if (airGroup != airGroupPlayer)
                    {
                        IEnumerable<Stationary> substitutes = stationaries.Where(x => x.Type == EStationaryType.Aircraft && x.Army == airGroup.Army && string.Compare(x.Class.Replace(ValueStationary, ValueAircraft), airGroup.Class, true) == 0);
                        MissionStatus.AirGroupObject airGroupObject = missionStatus.AirGroups.Where(x => string.Compare(x.Name, airGroup.Id) == 0).FirstOrDefault();
                        if (airGroupObject != null)
                        {
                            // Substitute Aircraft  
                            if (airGroupObject.DiedNums > 0 && substitutes.Any())
                            {
                                int numsAdd = Math.Min(airGroupObject.DiedNums, substitutes.Count());
                                airGroupObject.DiedNums -= numsAdd;
                                substitutes.Take(numsAdd).ToList().ForEach(x => stationaries.Remove(x));
                            }

                            if (/*!airGroupObject.IsAlive || !airGroupObject.IsValid || */((airGroupObject.Nums - airGroupObject.DiedNums) / airGroupObject.Nums) < Config.GroupDisableRate)
                            {
                                airGroups.Remove(airGroup);

                                // TODO: Substitute AirGroup
#if false
                                substitutes = stationaries.Where(x => x.Type == EStationaryType.Aircraft && x.Army == airGroup.Army);
                                IEnumerable<Stationary> substituteTarget = substitutes.GroupBy(x => x.Class).OrderBy(x => x.Count()).FirstOrDefault();
                                if (substituteTarget.Count() >= 3)
                                {
                                    string airGroupClass = substituteTarget.FirstOrDefault().Class.Replace(ValueStationary, ValueAircraft);
                                    AirGroupInfo airGroupInfo = AirGroupInfos.Default.GetAirGroupInfoAircraft(airGroupClass, true).Where(x => x.ArmyIndex == airGroup.ArmyIndex && AirForce.IsTrust(x.ArmyIndex, x.AirForceIndex)).FirstOrDefault();
                                    if (airGroupInfo != null)
                                    {
                                    }
                                }
#endif
                            }
                        }
                     }
                }

                // GroundGroups
                List<GroundGroup> groundGroups = missionFile.GroundGroups;
                for (int i = groundGroups.Count - 1; i >= 0; i--)
                {
                    GroundGroup groundGroup = groundGroups[i];
                    IEnumerable<Stationary> substitutes = stationaries.Where(x => x.Army == groundGroup.Army && string.Compare(MissionStatus.MissionObject.CreateClassShortName(x.Class), MissionStatus.MissionObject.CreateClassShortName(groundGroup.Class), true) == 0);
                    MissionStatus.GroundGroupObject groundGroupObject = missionStatus.GroundGroups.Where(x => string.Compare(x.Name, groundGroup.Id) == 0 && x.Army == groundGroup.Army && string.Compare(x.Class, groundGroup.Class) == 0).FirstOrDefault();
                    if (groundGroupObject != null)
                    {
                        // Substitute Aircraft  
                        if (groundGroupObject.Nums - groundGroupObject.AliveNums > 0 && substitutes.Any())
                        {
                            int numsAdd = Math.Min(groundGroupObject.Nums - groundGroupObject.AliveNums, substitutes.Count());
                            groundGroupObject.AliveNums += numsAdd;
                            substitutes.Take(numsAdd).ToList().ForEach(x => stationaries.Remove(x));
                        }

                        if (/*!groundGroupObject.IsAlive || !groundGroupObject.IsValid || */(groundGroupObject.AliveNums / groundGroupObject.Nums) < Config.GroupDisableRate)
                        {
                            groundGroups.Remove(groundGroup);

                            // TODO: Substitute GroundGroup

                        }
                    }
                }
            }
        }

        public void GenerateMission(string environmentTemplateFile, string missionTemplateFileName, string missionId, MissionStatus missionStatus, out ISectionFile missionFile, out BriefingFile briefingFile)
        {
            CampaignInfo campaignInfo = Career.CampaignInfo;
            MissionFile missionTemplateFile = new MissionFile(GamePlay, new string[] { missionTemplateFileName }, campaignInfo.AirGroupInfos);

            AirGroup airGroup = missionTemplateFile.AirGroups.Where(x => x.ArmyIndex == Career.ArmyIndex && string.Compare(x.ToString(), Career.AirGroup) == 0).FirstOrDefault();
            if (airGroup == null)
            {
                throw new NotImplementedException(string.Format("Invalid ArmyIndex[{0}] and AirGroup[{1}]", Career.ArmyIndex, Career.AirGroup));
            }

            OptimizeMissionObjects(missionTemplateFile, missionStatus, airGroup);

            GeneratorGroundOperation = new GeneratorGroundOperation(GamePlay, Random, Config, missionStatus, missionTemplateFile.BattleArea, missionTemplateFile.GroundGroups, missionTemplateFile.Stationaries,
                missionTemplateFile.FrontMarkers, Career.StationaryGenerateType, Career.GroundGroupGenerateType, Career.ArmorUnitNumsSet, Career.ShipUnitNumsSet,
                Config.GroundGroupFormationCountDefault, Career.ArtilleryTimeout, Career.ArtilleryRHide, Career.ArtilleryZOffset, Career.ShipSleep, Career.ShipSkill, Career.ShipSlowfire);
            GeneratorBriefing = new GeneratorBriefing(GamePlay);
            GeneratorAirOperation = new GeneratorAirOperation(GamePlay, Random, Config, GeneratorGroundOperation, GeneratorBriefing, campaignInfo, missionStatus, missionTemplateFile.BattleArea, missionTemplateFile.AirGroups, airGroup, Career.AISkill);

            if (Career.AdditionalAirGroups)
            {
                IEnumerable<string> aircraftsRed;
                IEnumerable<string> aircraftsBlue;
                GetRandomAircraftList(missionTemplateFile, out aircraftsRed, out aircraftsBlue);
                GeneratorAirOperation.AddRandomAirGroups(Career.AdditionalAirOperations, aircraftsRed, aircraftsBlue);
            }

            if (Career.AdditionalGroundGroups)
            {
                GeneratorGroundOperation.AddRandomGroundGroups(Career.AdditionalGroundOperations);
            }

            if (Career.AdditionalStationaries)
            {
                GeneratorGroundOperation.AddRandomStationaries(Career.AdditionalGroundOperations);
            }

            // Load the environment template file for the generated mission.
            missionFile = GetEnvironmenMissionTemplateFile(environmentTemplateFile);
            briefingFile = new BriefingFile();

            briefingFile.MissionName = missionId;
            briefingFile.MissionDescription = string.Empty;

            // Add things to the template file.
            double time = MissionTime.OptimizeTime(Random, (Career.Time == (int)MissionTime.Random ? Random.Next(Config.RandomTimeBegin, Config.RandomTimeEnd + 1) : Career.Time < 0 ? missionTemplateFile.Time : Career.Time), 0.25);
            missionFile.set(MissionFile.SectionMain, MissionFile.KeyTime, time.ToString(Config.NumberFormat));

            int weatherIndex = Career.Weather == (int)EWeather.Random ? Random.Next((int)EWeather.Clear, (int)EWeather.Count) : Career.Weather < 0 ? missionTemplateFile.WeatherIndex : (int)Career.Weather;
            missionFile.set(MissionFile.SectionMain, MissionFile.KeyWeatherIndex, weatherIndex.ToString(Config.NumberFormat));

            int cloudsHeight = Career.CloudAltitude == (int)CloudAltitude.Random ? Random.Next(CloudAltitude.Min / 100, CloudAltitude.Max / 100 + 1) * 100 : Career.CloudAltitude < 0 ? missionTemplateFile.CloudsHeight : Career.CloudAltitude;
            missionFile.set(MissionFile.SectionMain, MissionFile.KeyCloudsHeight, cloudsHeight.ToString(Config.NumberFormat));

            string weatherString = string.Empty;
            if (weatherIndex == (int)EWeather.Clear)
            {
                weatherString = EWeather.Clear.ToDescription();
            }
            else if (weatherIndex == (int)EWeather.LightClouds)
            {
                weatherString = string.Format(Config.NumberFormat, "Light clouds at {0}m", cloudsHeight);
            }
            else if (weatherIndex == (int)EWeather.MediumClouds)
            {
                weatherString = string.Format(Config.NumberFormat, "Medium clouds at {0}m", cloudsHeight);
            }

            briefingFile.MissionDescription = string.Format("{0}\nDate: {1}\nTime: {2}\nWeather: {3}",
                                                                campaignInfo.Id, Career.Date.Value.ToString("d", DateTimeFormatInfo.InvariantInfo), MissionTime.ToString(time), weatherString);

            // Create a air operation for the player.
            EMissionType? missionType = Career.MissionType;
            Spawn spawn = new Spawn(Career.Spawn, Career.SpawnRandomAltitudeFriendly, Career.SpawnRandomAltitudeEnemy, new SpawnLocation(Career.SpawnRandomLocationPlayer, Career.SpawnRandomLocationFriendly, Career.SpawnRandomLocationEnemy),
                Career.SpawnRandomTimeFriendly, Career.SpawnRandomTimeEnemy, Career.SpawnRandomTimeFriendly || Career.SpawnRandomTimeEnemy ? new SpawnTime(true, 0, Career.SpawnRandomTimeBeginSec, Career.SpawnRandomTimeEndSec) : null);
            bool result = false;
            if (missionType == null)
            {
                List<EMissionType> availableMissionTypes = GeneratorAirOperation.GetAvailableMissionTypes(airGroup).ToList();
                foreach (var item in Config.DisableMissionType)
                {
                    availableMissionTypes.Remove(item);
                }
                while (availableMissionTypes.Count > 0)
                {
                    int randomMissionTypeIndex = Random.Next(availableMissionTypes.Count);
                    missionType = availableMissionTypes[randomMissionTypeIndex];
                    if (GeneratorAirOperation.CreateAirOperation(missionFile, briefingFile, airGroup, missionType.Value, Career.AllowDefensiveOperation,
                                                            Career.EscortAirGroup, Career.EscoredtAirGroup, Career.OffensiveAirGroup, Career.TargetGroundGroup, Career.TargetStationary, spawn, Career.PlayerAirGroupSkill, Career.Speed, Career.Fuel, Career.Flight, Career.Formation))
                    {
                        result = true;
                        break;
                    }
                    availableMissionTypes.RemoveAt(randomMissionTypeIndex);
                }
            }
            else
            {
                result = GeneratorAirOperation.CreateAirOperation(missionFile, briefingFile, airGroup, missionType.Value, Career.AllowDefensiveOperation,
                                                            Career.EscortAirGroup, Career.EscoredtAirGroup, Career.OffensiveAirGroup, Career.TargetGroundGroup, Career.TargetStationary, spawn, Career.PlayerAirGroupSkill, Career.Speed, Career.Fuel, Career.Flight, Career.Formation);
            }

            if (!result)
            {
                throw new ArgumentException(string.Format("no available Player Mission[{0}] AirGroup[{1}]", missionId, airGroup.DisplayDetailName));
            }

            // Determine the aircraft that is controlled by the player.
            IEnumerable<string> aircraftOrder = determineAircraftOrder(airGroup);

            string playerAirGroupKey = airGroup.AirGroupKey;
            int playerSquadronIndex = airGroup.SquadronIndex;
            if (aircraftOrder.Any())
            {
                string playerPosition = aircraftOrder.Last();

                double factor = aircraftOrder.Count() / 6;
                int playerPositionIndex = (int)(Math.Floor(Career.RankIndex * factor));
                playerPosition = aircraftOrder.ElementAt(aircraftOrder.Count() - 1 - playerPositionIndex);
                string playerInfo = AirGroup.CreateSquadronString(playerAirGroupKey, playerSquadronIndex) + playerPosition;
                SilkySkyCloDFile.Write(missionFile, MissionFile.SectionMain, MissionFile.KeyPlayer, playerInfo, true);
            }

            // Add additional air operations.
            int i;
            if (GeneratorAirOperation.HasAvailableAirGroup)
            {
                spawn = Spawn.Create((int)ESpawn.Default, spawn);
                i = 0;
                while (i < Career.AdditionalAirOperations && GeneratorAirOperation.HasAvailableAirGroup)
                {
                    AirGroup randomAirGroup = GeneratorAirOperation.GetAvailableRandomAirGroup();
                    if (GeneratorAirOperation.CreateRandomAirOperation(missionFile, briefingFile, randomAirGroup, spawn))
                    {
                        i++;
                    }
                }
            }

            // Add additional ground operations.
            if (GeneratorGroundOperation.HasAvailableGroundGroup)
            {
                i = 0;
                while (i < Career.AdditionalGroundOperations && GeneratorGroundOperation.HasAvailableGroundGroup)
                {
                    GroundGroup randomGroundGroup = GeneratorGroundOperation.GetAvailableRandomGroundGroup();
                    if (GeneratorGroundOperation.CreateRandomGroundOperation(missionFile, randomGroundGroup))
                    {
                        i++;
                    }
                }
            }

            // Add all stationaries.
            GeneratorGroundOperation.StationaryWriteTo(missionFile);

            // FrontMarker
            i = 0;
            foreach (Point3d point in missionTemplateFile.FrontMarkers)
            {
                string key = string.Format(Config.NumberFormat, "{0}{1}", MissionFile.SectionFrontMarker, i + 1);
                string value = string.Format(Config.NumberFormat, "{0:F2} {1:F2} {2}", point.x, point.y, (int)point.z);
                missionFile.add(MissionFile.SectionFrontMarker, key, value);
                i++;
            }

#if DEBUG
            GeneratorAirOperation.TraceAssignedAirGroups();
#endif
        }

        private ISectionFile GetEnvironmenMissionTemplateFile(string environmentTemplateFile)
        {
            // Load the environment template file for the generated mission.
            ISectionFile missionFile = GamePlay.gpLoadSectionFile(environmentTemplateFile);

            // Delete things from the template file.

            // It is not necessary to delete air groups and ground groups from the missionFile as it 
            // is based on the environment template. If there is anything in it (air groups, ...) it is intentional.
            SilkySkyCloDFile.Delete(missionFile, MissionFile.SectionMain, MissionFile.KeyPlayer);
#if false
            int i = 0;
            string esction;
            while (missionFile.exist(esction = string.Format(Config.NumberFormat, "{0}_{1}", MissionFile.SectionGlobalWind, i++)))
            {
                missionFile.delete(esction);
            }
#endif
            missionFile.delete(MissionFile.SectionSplines);
            SilkySkyCloDFile.DeleteSectionAndSubSection(missionFile, MissionFile.SectionAirGroups, new string[] { MissionFile.SectionWay });
            SilkySkyCloDFile.DeleteSectionAndSubSection(missionFile, MissionFile.SectionChiefs, new string[] { MissionFile.SectionRoad });
            SilkySkyCloDFile.DeleteSectionAndSubSection(missionFile, MissionFile.SectionCustomChiefs);
            missionFile.delete(MissionFile.SectionStationary);
            missionFile.delete(MissionFile.SectionBuildings);
            missionFile.delete(MissionFile.SectionBuildingsLinks);
            missionFile.delete(MissionFile.SectionFrontMarker);
            missionFile.delete(MissionFile.SectionTrigger);
            missionFile.delete(MissionFile.SectionAction);
            missionFile.delete(MissionFile.SectionAirdromes);

            return missionFile;
        }

        private void GetRandomAircraftList(MissionFile missionFile, out IEnumerable<string> aircraftsRed, out IEnumerable<string> aircraftsBlue)
        {
            IEnumerable<string> dlc = missionFile.DLC;
            string[] aircraftRandomRed;
            string[] aircraftRandomBlue;
            CampaignInfo campaignInfo = Career.CampaignInfo;
            if (missionFile.AircraftRandomRed.Any() && missionFile.AircraftRandomBlue.Any())
            {
                aircraftRandomRed = missionFile.AircraftRandomRed;
                aircraftRandomBlue = missionFile.AircraftRandomBlue;
            }
            else if (campaignInfo.AircraftRandomRed.Any() && campaignInfo.AircraftRandomBlue.Any())
            {
                aircraftRandomRed = campaignInfo.AircraftRandomRed;
                aircraftRandomBlue = campaignInfo.AircraftRandomBlue;
            }
            else
            {
                aircraftRandomRed = Config.AircraftRandomRed;
                aircraftRandomBlue = Config.AircraftRandomBlue;
            }
            aircraftsRed = aircraftRandomRed.Where(x => x.IndexOf(":") == -1 || dlc.Any(y => x.StartsWith(y, StringComparison.InvariantCultureIgnoreCase)));
            aircraftsBlue = aircraftRandomBlue.Where(x => x.IndexOf(":") == -1 || dlc.Any(y => x.StartsWith(y, StringComparison.InvariantCultureIgnoreCase)));
        }

        private static IEnumerable<string> determineAircraftOrder(AirGroup airGroup)
        {
            List<string> aircraftOrder = new List<string>();
            AirGroupInfo airGroupInfo = airGroup.AirGroupInfo;
            if (airGroupInfo.FlightSize % 3 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    foreach (int key in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[key].Count > i)
                        {
                            aircraftOrder.Add(key.ToString(Config.NumberFormat) + i.ToString(Config.NumberFormat));
                        }
                    }

                    foreach (int key in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[key].Count > i + 3)
                        {
                            aircraftOrder.Add(key.ToString(Config.NumberFormat) + (i + 3).ToString(Config.NumberFormat));
                        }
                    }
                }
            }
            else if (airGroupInfo.FlightSize % 2 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    foreach (int key in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[key].Count > i)
                        {
                            aircraftOrder.Add(key.ToString(Config.NumberFormat) + i.ToString(Config.NumberFormat));
                        }
                    }

                    foreach (int key in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[key].Count > i + 2)
                        {
                            aircraftOrder.Add(key.ToString(Config.NumberFormat) + (i + 2).ToString(Config.NumberFormat));
                        }
                    }
                }
            }
            else
            {
                foreach (int key in airGroup.Flights.Keys)
                {
                    if (airGroup.Flights[key].Count == 1)
                    {
                        aircraftOrder.Add(key.ToString(Config.NumberFormat) + "0");
                    }
                }
            }

            return aircraftOrder;
        }

        public void ReinForce(MissionStatus missionStatus, DateTime dateTime)
        {
            if (missionStatus != null)
            {
                int reinForceHour = Config.ReinForceDay * 24;
                double IntervalHour = (dateTime - missionStatus.DateTime).TotalHours;
                double rate = IntervalHour / reinForceHour;

                foreach (var item in missionStatus.AirGroups)
                {
                    DateTime? reinForceDate = item.ReinForceDate;
                    if (reinForceDate == null)
                    {
                        int nums = (int)Math.Floor(((item.Nums - item.DiedNums) / item.Nums * rate * item.ReinForceRate()));
                        // TODO: Change Skill Value ...

                        item.InitNums = item.DiedNums = nums;
                    }
                    else if (reinForceDate.Value <= dateTime)
                    {
                        item.InitNums = item.Nums;
                        item.DiedNums = 0;
                    }
                }

                foreach (var item in missionStatus.GroundGroups)
                {
                    DateTime? reinForceDate = item.ReinForceDate;
                    if (reinForceDate == null)
                    {
                        int nums = (int)Math.Floor((item.AliveNums / item.Nums * rate * item.ReinForceRate()));
                        // TODO: Change Skill Value ...

                        item.AliveNums = nums;
                    }
                    else if (reinForceDate.Value <= dateTime)
                    {
                        item.AliveNums = item.Nums;
                    }
                }

                foreach (var item in missionStatus.Stationaries)
                {
                    DateTime? reinForceDate = item.ReinForceDate;
                    if (reinForceDate == null)
                    {
                        item.IsAlive = rate * item.ReinForceRate() >= 1.0;
                    }
                    else if (reinForceDate.Value <= dateTime)
                    {
                        item.IsAlive = true;
                    }
                }

                foreach (var item in missionStatus.Aircrafts)
                {
                    DateTime? reinForceDate = item.ReinForceDate;
                    if (reinForceDate == null)
                    {
                        item.IsAlive = rate * item.ReinForceRate() >= 1.0;
                    }
                    else if (reinForceDate.Value <= dateTime)
                    {
                        item.IsAlive = true;
                    }
                }

                foreach (var item in missionStatus.GroundActors)
                {
                    DateTime? reinForceDate = item.ReinForceDate;
                    if (reinForceDate == null)
                    {
                        item.IsAlive = rate * item.ReinForceRate() >= 1.0;
                    }
                    else if (reinForceDate.Value <= dateTime)
                    {
                        item.IsAlive = true;
                    }
                }
            }
        }
    }
}