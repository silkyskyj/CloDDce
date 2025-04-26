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
using IL2DCE.Util;
using maddox.game;
using maddox.GP;
using static IL2DCE.MissionObjectModel.MissionStatus;
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

        private const float FrontMarkerMoveXRate = 0.6f;
        private const float FrontMarkerMoveYRate = 0.7f;

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

        public void GenerateMission(ISectionFile missionTemplateFileName, string missionId, MissionStatus missionStatus, out ISectionFile missionFile, out BriefingFile briefingFile)
        {
            CampaignInfo campaignInfo = Career.CampaignInfo;
            MissionFile missionTemplateFile = new MissionFile(missionTemplateFileName, campaignInfo.AirGroupInfos);

            // Create Base Mission file info for the generated mission.
            missionFile = GamePlay.gpCreateSectionFile();
            SilkySkyCloDFile.CopySection(missionTemplateFileName, missionFile, MissionFile.SectionParts);
            SilkySkyCloDFile.CopySection(missionTemplateFileName, missionFile, MissionFile.SectionMain);
            SilkySkyCloDFile.Delete(missionFile, MissionFile.SectionMain, MissionFile.KeyPlayer);
            SilkySkyCloDFile.CopySection(missionTemplateFileName, missionFile, MissionFile.SectionGlobalWind);
            int i = 0;
            while (SilkySkyCloDFile.CopySection(missionTemplateFileName, missionFile, string.Format(Config.NumberFormat, "{0}_{1}", MissionFile.SectionGlobalWind, i)) > 0)
            {
                i++;
            }

            // Player Air Group
            AirGroup airGroup = missionTemplateFile.AirGroups.Where(x => x.ArmyIndex == Career.ArmyIndex && string.Compare(x.ToString(), Career.AirGroup, true) == 0).FirstOrDefault();
            if (airGroup == null)
            {
                throw new NotImplementedException(string.Format("Invalid ArmyIndex[{0}] and AirGroup[{1}]", Career.ArmyIndex, Career.AirGroup));
            }
            Career.PlayerAirGroup = airGroup;

            // Remove AirGouup / GrounGroup / Stationary  if !IsAlive or rate < Config.DisableRate
            OptimizeMissionObjects(missionTemplateFile, missionStatus, airGroup, Career.Date.Value);

            // TODO: UpdateFrontMarker

            GeneratorGroundOperation = new GeneratorGroundOperation(GamePlay, Random, Config, missionStatus, missionTemplateFile.BattleArea, missionTemplateFile.GroundGroups, missionTemplateFile.Stationaries,
                missionTemplateFile.FrontMarkers, Career.GroundGroupGenerateType, Career.StationaryGenerateType, Career.ArmorUnitNumsSet, Career.ShipUnitNumsSet,
                Career.ArtilleryTimeout, Career.ArtilleryRHide, Career.ArtilleryZOffset, Career.ShipSleep, Career.ShipSkill, Career.ShipSlowfire);

            // For now generate a random supply ship on one of the routes to a harbour.

            #region Chiefs

            List<GroundGroup> groundGroupsUpdate = new List<GroundGroup>();

            // 0. Chief                         - Red/Army in Mission File Load process)
            IEnumerable<GroundGroup> groundGroups = GeneratorGroundOperation.CreateGroundGroups(null, missionTemplateFile.GroundGroups);
            groundGroupsUpdate.AddRange(groundGroups);

            // 1. Roads (Chiefs Vehicle/Armor)  - None in Mission File Load process)
            // TODO: Only create a random 
            IEnumerable<Armor> armors = GeneratorGroundOperation.CreateGroundGroupsRoads(null, missionTemplateFile.Roads);
            groundGroupsUpdate.AddRange(armors);

            // 2. Waterways (Chiefs Ship)       - None in Mission File Load process)
            // TODO: Only create a random (or decent) amount of supply ships.
            IEnumerable<ShipGroup> ships = GeneratorGroundOperation.CreateGroundGroupsWaterways(null, missionTemplateFile.Waterways);
            groundGroupsUpdate.AddRange(ships);

            // 3. Railways (Chiefs Train)       - None in Mission File Load process)
            IEnumerable<GroundGroup> trains = GeneratorGroundOperation.CreateGroundGroupsRailways(null, missionTemplateFile.Railways);
            groundGroupsUpdate.AddRange(trains);

            #endregion

            #region Stationary

            List<Stationary> stationaryUpdate = new List<Stationary>();

            // 4. Stationary         - Red/Blue Army in Mission File Load process)
            IEnumerable<Stationary> stationaryDefaults = GeneratorGroundOperation.CreateStationaries(null, missionTemplateFile.Stationaries);
            stationaryUpdate.AddRange(stationaryDefaults);

            // 5. Depots (Buildings) - None in Mission File Load process)
            IEnumerable<MissionObjectModel.GroundObject> buildings = GeneratorGroundOperation.CreateStationaryBuildings(null, missionTemplateFile.Depots);
            stationaryUpdate.AddRange(buildings.Where(x => x is Stationary).Select(x => x as Stationary));

            //6. Other              - None Army in Mission File Load process) or Unknown
            IEnumerable<Stationary> stationaryOthers = GeneratorGroundOperation.CreateStationaries(null, missionTemplateFile.OtherStationaries);
            stationaryUpdate.AddRange(stationaryOthers);

            IEnumerable<Building> depots = buildings.Where(x => x is Building).Select(x => x as Building);

            #endregion

            GeneratorGroundOperation.SetGroundObjects(groundGroupsUpdate, stationaryUpdate);
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
                IEnumerable<IEnumerable<string>> groundActors;
                GetRandomGroundActorList(missionTemplateFile, out groundActors);
                GeneratorGroundOperation.AddRandomGroundGroups(Career.AdditionalGroundOperations, groundActors);
            }

            if (Career.AdditionalStationaries)
            {
                IEnumerable<IEnumerable<string>> stationaries;
                GetRandomStationaryList(missionTemplateFile, out stationaries);
                GeneratorGroundOperation.AddRandomStationaries(Career.AdditionalGroundOperations, stationaries);
            }

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

            // Buildings(Depots)
            foreach (var item in depots)
            {
                item.WriteTo(missionFile);
            }

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
            GeneratorGroundOperation.TraceAssignedGroundGroups();
#endif
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

        private void GetRandomGroundActorList(MissionFile missionFile, out IEnumerable<IEnumerable<string>> groundActors)
        {
            IEnumerable<string> dlc = missionFile.DLC;
            CampaignInfo campaignInfo = Career.CampaignInfo;

            if (campaignInfo.GroundVehicleRandomRed.Any() && campaignInfo.GroundVehicleRandomBlue.Any())
            {
                groundActors = new IEnumerable<string>[(int)EGroundGroupType.Count * (int)EArmy.Count] 
                    {
                        campaignInfo.GroundVehicleRandomRed, campaignInfo.GroundVehicleRandomBlue,
                        campaignInfo.GroundArmorRandomRed, campaignInfo.GroundArmorRandomBlue,
                        campaignInfo.GroundShipRandomRed, campaignInfo.GroundShipRandomBlue,
                        campaignInfo.GroundTrainRandomRed, campaignInfo.GroundTrainRandomBlue,
                        new string [] { "" }, new string [] { "" },
                    };
            }
            else
            {
                groundActors = new IEnumerable<string> [(int)EGroundGroupType.Count * (int)EArmy.Count]
                    {
                        Config.GroundVehicleRandomRed, Config.GroundVehicleRandomBlue,
                        Config.GroundArmorRandomRed, Config.GroundArmorRandomBlue,
                        Config.GroundShipRandomRed, Config.GroundShipRandomBlue,
                        Config.GroundTrainRandomRed, Config.GroundTrainRandomBlue,
                        new string [] { "" }, new string [] { "" },
                    };
            }
        }

        private void GetRandomStationaryList(MissionFile missionFile, out IEnumerable<IEnumerable<string>> stationaries)
        {
            IEnumerable<string> dlc = missionFile.DLC;
            CampaignInfo campaignInfo = Career.CampaignInfo;

            if (campaignInfo.StationaryRadarRandomRed.Any() && campaignInfo.StationaryRadarRandomBlue.Any())
            {
                stationaries = new IEnumerable<string>[(int)EStationaryType.Count * (int)EArmy.Count]
                    {
                        campaignInfo.StationaryRadarRandomRed, campaignInfo.StationaryRadarRandomBlue,
                        campaignInfo.StationaryAircraftRandomRed, campaignInfo.StationaryAircraftRandomBlue,
                        campaignInfo.StationaryArtilleryRandomRed, campaignInfo.StationaryArtilleryRandomBlue,
                        campaignInfo.StationaryFlakRandomRed, campaignInfo.StationaryFlakRandomBlue,
                        campaignInfo.StationaryDepotRandomRed, campaignInfo.StationaryDepotRandomBlue,
                        campaignInfo.StationaryShipRandomRed, campaignInfo.StationaryShipRandomBlue,
                        campaignInfo.StationaryAmmoRandomRed, campaignInfo.StationaryAmmoRandomBlue,
                        campaignInfo.StationaryWeaponsRandomRed, campaignInfo.StationaryWeaponsRandomBlue,
                        campaignInfo.StationaryCarRandomRed, campaignInfo.StationaryCarRandomBlue,
                        campaignInfo.StationaryConstCarRandomRed, campaignInfo.StationaryConstCarRandomBlue,
                        campaignInfo.StationaryEnvironmentRandomRed, campaignInfo.StationaryEnvironmentRandomBlue,
                        campaignInfo.StationarySearchlightRandomRed, campaignInfo.StationarySearchlightRandomBlue,
                        campaignInfo.StationaryAeroanchoredRandomRed, campaignInfo.StationaryAeroanchoredRandomBlue,
                        campaignInfo.StationaryAirfieldRandomRed, campaignInfo.StationaryAirfieldRandomBlue,
                        campaignInfo.StationaryUnknownRandomRed, campaignInfo.StationaryUnknownRandomBlue,
                    };
            }
            else
            {
                stationaries = new IEnumerable<string>[(int)EStationaryType.Count * (int)EArmy.Count]
                    {
                        Config.StationaryRadarRandomRed, Config.StationaryRadarRandomBlue,
                        Config.StationaryAircraftRandomRed, Config.StationaryAircraftRandomBlue,
                        Config.StationaryArtilleryRandomRed, Config.StationaryArtilleryRandomBlue,
                        Config.StationaryFlakRandomRed, Config.StationaryFlakRandomBlue,
                        Config.StationaryDepotRandomRed, Config.StationaryDepotRandomBlue,
                        Config.StationaryShipRandomRed, Config.StationaryShipRandomBlue,
                        Config.StationaryAmmoRandomRed, Config.StationaryAmmoRandomBlue,
                        Config.StationaryWeaponsRandomRed, Config.StationaryWeaponsRandomBlue,
                        Config.StationaryCarRandomRed, Config.StationaryCarRandomBlue,
                        Config.StationaryConstCarRandomRed, Config.StationaryConstCarRandomBlue,
                        Config.StationaryEnvironmentRandomRed, Config.StationaryEnvironmentRandomBlue,
                        Config.StationarySearchlightRandomRed, Config.StationarySearchlightRandomBlue,
                        Config.StationaryAeroanchoredRandomRed, Config.StationaryAeroanchoredRandomBlue,
                        Config.StationaryAirfieldRandomRed, Config.StationaryAirfieldRandomBlue,
                        Config.StationaryUnknownRandomRed, Config.StationaryUnknownRandomBlue,
                    };
            }
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
                                                                                    // 2
            int reinForceHour = Config.ReinForceDay * 24;                                       // 72
            double IntervalHour = (dateTime - missionStatus.DateTime).TotalHours;               // 48
            double rate = IntervalHour / reinForceHour;                                         // 0.7

            foreach (var item in missionStatus.AirGroups)
            {
                // if (item.InitNums > 0)
                {
                    DateTime? reinForceDate = item.ReinForceDate;
                    if (reinForceDate != null)
                    {
                        if (reinForceDate.Value <= dateTime)
                        {
                            item.Nums = item.InitNums > 0 ? item.InitNums: 1;
                            item.InitNums = item.Nums;
                            item.DiedNums = 0;
                            item.ReinForceDate = null;
                            item.IsAlive = true;
                            item.IsValid = true;
                        }
                    }
                }
            }

            foreach (var item in missionStatus.GroundGroups)
            {
                // if (item.Nums > 0)
                {
                    DateTime? reinForceDate = item.ReinForceDate;
                    if (reinForceDate != null)
                    {
                        if (reinForceDate.Value <= dateTime)
                        {
                            item.AliveNums = item.Nums > 0 ? item.Nums: 1;
                            item.Nums = item.AliveNums;
                            item.ReinForceDate = null;
                            item.IsAlive = true;
                            item.IsValid = true;
                        }
                    }
                }
            }

            foreach (var item in missionStatus.Stationaries)
            {
                DateTime? reinForceDate = item.ReinForceDate;
                if (reinForceDate != null)
                {
                    if (reinForceDate.Value <= dateTime)
                    {
                        item.IsAlive = true;
                        item.ReinForceDate = null;
                    }
                }
            }

            foreach (var item in missionStatus.Aircrafts)
            {
                DateTime? reinForceDate = item.ReinForceDate;
                if (reinForceDate != null)
                {
                    if (reinForceDate.Value <= dateTime)
                    {
                        item.IsAlive = true;
                        item.ReinForceDate = null;
                    }
                }
            }

            foreach (var item in missionStatus.GroundActors)
            {
                DateTime? reinForceDate = item.ReinForceDate;
                if (reinForceDate != null)
                {
                    if (reinForceDate.Value <= dateTime)
                    {
                        Debug.WriteLine("ReinForce to Alive[{0}] ReinForceDate:{1} [Next Date: {2}]", item.Name, item.ReinForceDate.Value.ToString(Config.DateTimeDefaultLongFormat), dateTime.ToString(Config.DateTimeDefaultLongFormat));
                        item.IsAlive = true;
                        item.ReinForceDate = null;
                    }
                }
            }
        }

        private void OptimizeMissionObjects(MissionFile missionFile, MissionStatus missionStatus, AirGroup airGroupPlayer, DateTime dateTime)
        {
            if (missionStatus != null)
            {
                double IntervalHour = (dateTime - missionStatus.DateTime).TotalHours;

                // Stationaries
#if DEBUG && false
                foreach (var item in missionStatus.Stationaries)
                {
                    Debug.WriteLine("Stationaries[{0}/{1}/{2}]", item.Name, item.Class, item.IsAlive);
                }
                foreach (var item in missionStatus.GroundActors)
                {
                    Debug.WriteLine("GroundActors[{0}/{1}/{2}]", item.Name, item.Class, item.IsAlive);
                }
#endif
                List<Stationary> stationaries = missionFile.Stationaries;
                for (int i = stationaries.Count - 1; i >= 0; i--)
                {
                    Stationary stationary = stationaries[i];
                    MissionStatus.StationaryObject stationaryObject = missionStatus.Stationaries.Where(x => string.Compare(x.Name, stationary.Id, true) == 0 && 
                                                                string.Compare(x.Class, stationary.Class, true) == 0).FirstOrDefault();
                    MissionStatus.GroundObject groundObject = missionStatus.GroundActors.Where(x => string.Compare(x.Name, stationary.Id, true) == 0 && 
                                                                string.Compare(x.Class, stationary.Class, true) == 0).FirstOrDefault();
                    // Debug.WriteLine("stationary[{0}] StationaryObject={1}[{2}] GroundObject={1}[{2}]", stationary.Id, stationaryObject != null ? stationaryObject.IsAlive.ToString() : "NONE", groundObject != null ? groundObject.IsAlive.ToString() : "NONE");
                    if (stationaryObject != null && !stationaryObject.IsAlive || groundObject != null && !groundObject.IsAlive)
                    {
                        Debug.WriteLine("Remove Stationary {0}[{1}]", stationary.Id, stationary.Class);
                        stationaries.Remove(stationary);
                    }
                }

                // AirGroups
                List<AirGroup> airGroups = missionFile.AirGroups;
                for (int i = airGroups.Count - 1; i >= 0; i--)
                {
                    AirGroup airGroup = airGroups[i];
//                    if (airGroup != airGroupPlayer)
                    {
                        IEnumerable<Stationary> substitutes = stationaries.Where(x => x.Type == EStationaryType.Aircraft && x.Army == airGroup.Army && 
                                                                string.Compare(x.Class.Replace(ValueStationary, ValueAircraft), airGroup.Class, true) == 0);
                        substitutes = substitutes.Where(x => missionStatus.Stationaries.Any(y => string.Compare(x.Id, y.Name, true) == 0 && 
                                                                string.Compare(x.Class, y.Class, true) == 0 && y.IsAlive));
                        MissionStatus.AirGroupObject airGroupObject = missionStatus.AirGroups.Where(x => string.Compare(x.Name, airGroup.Id, true) == 0).FirstOrDefault();
                        if (airGroupObject != null)
                        {
                            // Substitute Aircraft  
                            if (airGroupObject.DiedNums > 0 && substitutes.Any())
                            {
                                int numsAdd = Math.Min(airGroupObject.DiedNums, substitutes.Count());
                                airGroupObject.DiedNums -= numsAdd;
                                substitutes.Take(numsAdd).ToList().ForEach(x => stationaries.Remove(x));
                            }

                            // Position ?

                            if (airGroupObject.Nums == 0 || airGroupObject.InitNums == 0 || /*!airGroupObject.IsAlive || !airGroupObject.IsValid || */((airGroupObject.InitNums - airGroupObject.DiedNums) / (float)airGroupObject.InitNums) < Config.GroupDisableRate)
                            {
                                if (airGroup != airGroupPlayer)
                                {
                                    Debug.WriteLine("Remove AirGroups {0}[{1}]", airGroup.Id, airGroup.Class);
                                    airGroups.Remove(airGroup);
                                }

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
                    IEnumerable<Stationary> substitutes = stationaries.Where(x => x.Army == groundGroup.Army &&
                        string.Compare(MissionObjBase.CreateClassShortShortName(x.Class), MissionObjBase.CreateClassShortShortName(groundGroup.Class), true) == 0 &&
                        !x.Id.StartsWith(groundGroup.Id, true, CultureInfo.InvariantCulture));
                    substitutes = substitutes.Where(x => missionStatus.Stationaries.Any(y => string.Compare(x.Id, y.Name, true) == 0 && string.Compare(x.Class, y.Class, true) == 0 && y.IsAlive));
                    MissionStatus.GroundGroupObject groundGroupObject = missionStatus.GroundGroups.Where(x => string.Compare(x.Name, groundGroup.Id, true) == 0 && x.Army == groundGroup.Army &&
                        string.Compare(MissionObjBase.CreateClassShortShortName(x.Class), MissionObjBase.CreateClassShortShortName(groundGroup.Class), true) == 0).FirstOrDefault();
                    if (groundGroupObject != null)
                    {
                        // Substitute Unit  
                        if (groundGroupObject.Nums - groundGroupObject.AliveNums > 0 && substitutes.Any())
                        {
                            int numsAdd = Math.Min(groundGroupObject.Nums - groundGroupObject.AliveNums, substitutes.Count());
                            groundGroupObject.AliveNums += numsAdd;
                            substitutes.Take(numsAdd).ToList().ForEach(x => stationaries.Remove(x));
                        }

                        if (groundGroupObject.Nums == 0 || /*!groundGroupObject.IsAlive || !groundGroupObject.IsValid || */(groundGroupObject.AliveNums / (float)groundGroupObject.Nums) < Config.GroupDisableRate)
                        {
                            groundGroups.Remove(groundGroup);
                            Debug.WriteLine("Remove GroundGroup {0}[{1}]", groundGroup.Id, groundGroup.Class);

                            // TODO: Substitute GroundGroup

                        }
                        else
                        {
                            // Position
                            GroundGroupWaypoint wayPoint = groundGroup.Waypoints.FirstOrDefault();
                            if (wayPoint != null)
                            {
                                Point3d? pos = GeneratorGroundOperation.CreateRandomPoint(GamePlay, Random, groundGroup.Army,
                                                (float)groundGroupObject.X, (float)groundGroupObject.Y, (float)(100 * IntervalHour / 10), (float)groundGroupObject.Z,
                                                GroundGroup.GetLandTypes(groundGroup.Type));
                                if (pos != null)
                                {
                                    Debug.WriteLine("Position Update GroundGroup {0}[{1}] ({2},{3}) -> ({4},{5})", groundGroup.Id, groundGroup.Class, wayPoint.X, wayPoint.Y, pos.Value.x, pos.Value.y);
                                    wayPoint.X = pos.Value.x;
                                    wayPoint.Y = pos.Value.y;
                                }
                                else
                                {
                                    Debug.WriteLine("Position No Update GroundGroup {0}[{1}] ({2},{3})", groundGroup.Id, groundGroup.Class, wayPoint.X, wayPoint.Y);
                                }
                            }
                        }
                    }
                }

                // FrontMarker
                CampaignInfo campaignInfo = Career.CampaignInfo;
                if (campaignInfo.DynamicFrontMarker)
                {
                    IEnumerable<Point3d> positionsRed = groundGroups.Where(x => x.Army == (int)EArmy.Red).Select(x => x.Position)
                                        .Concat(stationaries.Where(x => x.Army == (int)EArmy.Red).Select(x => x.Position)).Select(x => new Point3d(x.x, x.y, (int)EArmy.Red));
                    IEnumerable<Point3d> positionsBlue = groundGroups.Where(x => x.Army == (int)EArmy.Blue).Select(x => x.Position)
                                        .Concat(stationaries.Where(x => x.Army == (int)EArmy.Blue).Select(x => x.Position)).Select(x => new Point3d(x.x, x.y, (int)EArmy.Blue));
                    List<Point3d> frontMarkers = new List<Point3d>();
                    for (int i = 0; i < missionFile.FrontMarkers.Count; i++)
                    {
                        Point3d pos = missionFile.FrontMarkers[i];
                        int army = (int)pos.z;
                        Point3d? nearestRed = MapUtil.NeaestPoint(positionsRed, ref pos);
                        Point3d? nearestBlue = MapUtil.NeaestPoint(positionsBlue, ref pos);
                        if (nearestRed != null && nearestBlue != null)
                        {
                            double distanceRed = nearestRed.Value.distance(ref pos);
                            double distanceBlue = nearestBlue.Value.distance(ref pos);
                            double distance = (distanceRed + distanceBlue) / 2 - ((army == (int)EArmy.Red) ? distanceRed : distanceBlue);
                            pos.x += distance * FrontMarkerMoveXRate * ((army == (int)EArmy.Red) ? nearestRed.Value.x < nearestBlue.Value.x ? 1 : -1 : nearestRed.Value.x < nearestBlue.Value.x ? -1 : 1);
                            pos.y += distance * FrontMarkerMoveYRate * ((army == (int)EArmy.Red) ? nearestRed.Value.y < nearestBlue.Value.y ? 1 : -1 : nearestRed.Value.x < nearestBlue.Value.x ? -1 : 1);
                        }
                        frontMarkers.Add(pos);
                    }
                    missionFile.FrontMarkers.Clear();
                    missionFile.FrontMarkers.AddRange(frontMarkers);
                }
            }
        }
    }
}