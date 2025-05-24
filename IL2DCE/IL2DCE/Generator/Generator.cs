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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;
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

        private Config Config
        {
            get;
            set;
        }

        #endregion

        #region Constructor

        public Generator(IGamePlay gamePlay, IRandom random, Config config, Career career)
            : base(gamePlay, random)
        {
            Career = career;
            Config = config;
        }

        #endregion

        public void GenerateMission(ISectionFile missionTemplateFileName, IMissionStatus missionStatus, out ISectionFile missionFile, out BriefingFile briefingFile)
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
            AirGroup airGroup = missionTemplateFile.AirGroups.Where(x => x.ArmyIndex == Career.ArmyIndex && string.Compare(x.SquadronName, Career.AirGroup, true) == 0).FirstOrDefault();
            if (airGroup == null)
            {
                airGroup = GeneratorAirOperation.CreateAirGroup(GamePlay, Random, campaignInfo, missionTemplateFile.AirGroups, Career.ArmyIndex, Career.AirGroup, Career.Aircraft, Career.Spawn);
                if (airGroup == null)
                {
                    throw new NotImplementedException(string.Format("Invalid ArmyIndex[{0}] and AirGroup[{1}]", Career.ArmyIndex, Career.AirGroup));
                }
                missionTemplateFile.AirGroups.Add(airGroup);
            }
            Career.PlayerAirGroup = airGroup;
            Career.Aircraft = AircraftInfo.CreateDisplayName(airGroup.Class);

            // Remove AirGouup / GrounGroup / Stationary  if !IsAlive or rate < Config.DisableRate
            OptimizeMissionObjects(missionTemplateFile, missionStatus, airGroup, Career.Date.Value);

            // TODO: UpdateFrontMarker

            GeneratorGroundOperation = new GeneratorGroundOperation(GamePlay, Random, missionStatus, missionTemplateFile.BattleArea, missionTemplateFile.GroundGroups, missionTemplateFile.Stationaries,
                missionTemplateFile.FrontMarkers, false, Career.GroundGroupGenerateType, Career.StationaryGenerateType, Career.ArmorUnitNumsSet, Career.ShipUnitNumsSet,
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
            GeneratorAirOperation = new GeneratorAirOperation(GamePlay, Random, GeneratorGroundOperation, GeneratorBriefing, campaignInfo, missionStatus, missionTemplateFile.BattleArea, missionTemplateFile.AirGroups, airGroup, Career.AISkill, Config.Skills, Config.EnableMissionMultiAssign, Config.FlightCount, Config.FlightSize, Config.SpawnParked, true);

            if (Career.AdditionalAirGroups)
            {
                IEnumerable<string> aircraftsRed;
                IEnumerable<string> aircraftsBlue;
                GetRandomAircraftList(missionTemplateFile, out aircraftsRed, out aircraftsBlue);
                GeneratorAirOperation.AddRandomAirGroupsByOperations(Career.AdditionalAirOperations, aircraftsRed, aircraftsBlue);
            }

            if (Career.AdditionalGroundGroups)
            {
                IEnumerable<IEnumerable<string>> groundActors;
                GetRandomGroundActorList(missionTemplateFile, out groundActors);
                GeneratorGroundOperation.AddRandomGroundGroupsByOperations(Career.AdditionalGroundOperations, groundActors);
            }

            if (Career.AdditionalStationaries)
            {
                IEnumerable<IEnumerable<string>> stationaries;
                GetRandomStationaryList(missionTemplateFile, out stationaries);
                GeneratorGroundOperation.AddRandomStationariesByOperations(Career.AdditionalGroundOperations, stationaries);
            }

            briefingFile = new BriefingFile();
            briefingFile.MissionDescription = string.Empty;

            // Add things to the template file.
            double time = MissionTime.OptimizeTime(Random, (Career.Time == (int)MissionTime.Random ? Random.Next((int)(Career.RandomTimeBegin * 100), (int)(Career.RandomTimeEnd * 100) + 1) / 100 : Career.Time < 0 ? missionTemplateFile.Time : Career.Time), 0.25);
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

            briefingFile.MissionDescription = string.Format("{0} [{1}]\nDate: {2}\nTime: {3}\nWeather: {4}",
                campaignInfo.Name, FileUtil.GetGameFileNameWithoutExtension(Career.MissionTemplateFileName), Career.Date.Value.ToString("d", DateTimeFormatInfo.InvariantInfo), MissionTime.ToString(time), weatherString);

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
                throw new ArgumentException(string.Format("no available Player Mission[{0}] AirGroup[{1}]", campaignInfo.Id, airGroup.DisplayDetailName));
            }

            // Palyer Determine the aircraft that is controlled by the player.
            IEnumerable<string> aircraftOrder = determineAircraftOrder(airGroup);
            if (aircraftOrder.Any())
            {
                double factor = aircraftOrder.Count() / 6;
                int playerPositionIndex = (int)(Math.Floor(Career.RankIndex * factor));
                string playerPosition = aircraftOrder.ElementAt(aircraftOrder.Count() - 1 - playerPositionIndex);
                string playerInfo = AirGroup.CreateSquadronString(airGroup.AirGroupKey, airGroup.SquadronIndex) + playerPosition;
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

        public bool GenerateSubMission(IMissionStatus missionStatus, out ISectionFile subMissionFile, out BriefingFile subBriefingFile, DoWorkEventArgs eventArgs)
        {
            subMissionFile = null;
            subBriefingFile = null;

            CampaignInfo campaignInfo = Career.CampaignInfo;
            string missionTemplateFileName = Career.MissionTemplateFile(Random);
            ISectionFile missionTemplateSectionFile = GamePlay.gpLoadSectionFile(missionTemplateFileName);
            MissionFile missionTemplateFile = new MissionFile(missionTemplateSectionFile, campaignInfo.AirGroupInfos, Career.SpawnDynamicStationaries ? MissionFile.LoadLevel.AirGroundGroupUnit : Career.SpawnDynamicGroundGroups ? MissionFile.LoadLevel.AirGroundGroup : MissionFile.LoadLevel.AirGroup);

            IMissionStatus missionStatusTotal = GetTotalMissionStatus();

            int needAirGroups = GetNeedAirGroupCount(missionTemplateFile, missionStatus, false);
            int needGoundGroups = GetNeedGroundGroupCount(missionTemplateFile, missionStatus);
            int needGroundUnits = GetNeedGroundUnitCount(missionTemplateFile, missionStatus);
            if (needAirGroups < 1 && needGoundGroups < 1 && needGroundUnits < 1)
            {
                return false;
            }

            IEnumerable<string> airGroups  = needAirGroups > 0 ? GetAddAirGroups(missionTemplateFile, missionStatus, missionStatusTotal): new string [0];
            IEnumerable<string> groundGroups = needGoundGroups > 0 ? GetAddGroundGroups(missionTemplateFile, missionStatus, missionStatusTotal) : new string[0];
            IEnumerable<string> groundUnits = needGroundUnits > 0 ? GetAddGroundUnits(missionTemplateFile, missionStatus, missionStatusTotal): new string[0];
            if (!((needAirGroups > 0 && (airGroups.Any() || Career.AdditionalAirGroups)) || (needGoundGroups > 0 && (groundGroups.Any() || Career.AdditionalGroundGroups)) || (needGroundUnits > 0 && (groundUnits.Any() || Career.AdditionalStationaries))))
            {
                return false;
            }

            // Create Base Mission file info for the generated mission.
            subMissionFile = GamePlay.gpCreateSectionFile();

            MissionFile missionFile = new MissionFile(GamePlay.gpLoadSectionFile(Career.MissionFileName), campaignInfo.AirGroupInfos);

            if (eventArgs.Cancel)
            {
                return false;
            }

            // Player Air Group
            AirGroup airGroup = missionFile.AirGroups.Where(x => x.ArmyIndex == Career.ArmyIndex && string.Compare(x.SquadronName, Career.AirGroup, true) == 0).FirstOrDefault();
            if (airGroup == null)
            {
                throw new NotImplementedException(string.Format("Invalid ArmyIndex[{0}] and AirGroup[{1}]", Career.ArmyIndex, Career.AirGroup));
            }

            // Remove & RePoint AirGouup / GrounGroup / Stationary  if !IsAlive or rate < Config.DisableRate
            OptimizeMissionObjects(missionFile, missionStatus, airGroup);

            if (needAirGroups > 0)
            {
                if (airGroups != null)
                {
                    var targets = missionTemplateFile.AirGroups.Where(x => airGroups.Contains(x.SquadronName));
                    missionFile.AirGroups.AddRange(targets);
                    needAirGroups -= Math.Min(needAirGroups, targets.Count());
                }
            }

            if (needGoundGroups > 0)
            {
                if (groundGroups != null)
                {
                    var targets = missionTemplateFile.GroundGroups.Where(x => groundGroups.Contains(x.Id));
                    missionFile.GroundGroups.AddRange(targets);
                    needGoundGroups -= Math.Min(needGoundGroups, targets.Count());
                }
            }

            if (needGroundUnits > 0)
            {
                if (groundUnits != null)
                {
                    var targets = missionTemplateFile.Stationaries.Where(x => groundUnits.Contains(x.Id));
                    missionFile.Stationaries.AddRange(targets);
                    needGroundUnits -= Math.Min(needGroundUnits, targets.Count());
                }
            }

            if (eventArgs.Cancel)
            {
                return false;
            }

            GeneratorGroundOperation = new GeneratorGroundOperation(GamePlay, Random, missionStatus, missionFile.BattleArea, missionFile.GroundGroups, missionFile.Stationaries,
            missionFile.FrontMarkers, true, Career.GroundGroupGenerateType, Career.StationaryGenerateType, Career.ArmorUnitNumsSet, Career.ShipUnitNumsSet,
            Career.ArtilleryTimeout, Career.ArtilleryRHide, Career.ArtilleryZOffset, Career.ShipSleep, Career.ShipSkill, Career.ShipSlowfire);

            GeneratorBriefing = new GeneratorBriefing(GamePlay);
            GeneratorAirOperation = new GeneratorAirOperation(GamePlay, Random, GeneratorGroundOperation, GeneratorBriefing, campaignInfo, missionStatus, missionFile.BattleArea, missionFile.AirGroups, airGroup, Career.AISkill, Config.Skills, Config.EnableMissionMultiAssign, Config.FlightCount, Config.FlightSize, Config.SpawnParked, false);

            if (eventArgs.Cancel)
            {
                return false;
            }

            if (Career.AdditionalAirGroups && needAirGroups > 0)
            {
                IEnumerable<string> aircraftsRed;
                IEnumerable<string> aircraftsBlue;
                GetRandomAircraftList(missionFile, out aircraftsRed, out aircraftsBlue);
                needAirGroups -= GeneratorAirOperation.AddRandomAirGroups(needAirGroups, aircraftsRed, aircraftsBlue);
            }

            if (eventArgs.Cancel)
            {
                return false;
            }

            if (Career.AdditionalGroundGroups && needGoundGroups > 0)
            {
                IEnumerable<IEnumerable<string>> groundActors;
                GetRandomGroundActorList(missionFile, out groundActors);
                needGoundGroups -= GeneratorGroundOperation.AddRandomGroundGroups(needGoundGroups, groundActors);
            }

            if (eventArgs.Cancel)
            {
                return false;
            }

            if (Career.AdditionalStationaries && needGroundUnits > 0)
            {
                IEnumerable<IEnumerable<string>> stationaries;
                GetRandomStationaryList(missionFile, out stationaries);
                needGroundUnits -= GeneratorGroundOperation.AddRandomStationaries(needGroundUnits, stationaries);
            }

            if (eventArgs.Cancel)
            {
                return false;
            }

            GeneratorGroundOperation.OptimizeMissionObjects();
            GeneratorAirOperation.OptimizeMissionObjects();

            if (eventArgs.Cancel)
            {
                return false;
            }

            subBriefingFile = new BriefingFile();
            subBriefingFile.MissionDescription = string.Empty;

            Spawn spawn = new Spawn(Career.Spawn, Career.SpawnRandomAltitudeFriendly, Career.SpawnRandomAltitudeEnemy, 
                new SpawnLocation(Career.SpawnRandomLocationPlayer, Career.SpawnRandomLocationFriendly, Career.SpawnRandomLocationEnemy), false, false, null);

            int AddCounts = 0;
            int i = 0;
            // Add additional air operations.
            if (GeneratorAirOperation.HasAvailableAirGroup)
            {
                spawn = Spawn.Create((int)ESpawn.Default, spawn);
                i = 0;
                while (i < Career.AdditionalAirOperations && GeneratorAirOperation.HasAvailableAirGroup)
                {
                    AirGroup randomAirGroup = GeneratorAirOperation.GetAvailableRandomAirGroup();
                    if (GeneratorAirOperation.CreateRandomAirOperation(subMissionFile, subBriefingFile, randomAirGroup, spawn))
                    {
                        i++;
                    }

                    if (eventArgs.Cancel)
                    {
                        return false;
                    }
                }
            }
            AddCounts += i;

            // Add additional ground operations.
            i = 0;
            if (GeneratorGroundOperation.HasAvailableGroundGroup)
            {
                while (i < Career.AdditionalGroundOperations && GeneratorGroundOperation.HasAvailableGroundGroup)
                {
                    GroundGroup randomGroundGroup = GeneratorGroundOperation.GetAvailableRandomGroundGroup();
                    if (GeneratorGroundOperation.CreateRandomGroundOperation(subMissionFile, randomGroundGroup))
                    {
                        i++;
                    }

                    if (eventArgs.Cancel)
                    {
                        return false;
                    }
                }
            }
            AddCounts += i;

            // Add all stationaries.
            AddCounts += GeneratorGroundOperation.StationaryWriteTo(subMissionFile);

            return AddCounts > 0;
        }

        private IMissionStatus GetTotalMissionStatus()
        {
            GameIterface gameInterface = (GamePlay as IGame).gameInterface;
            string missionStatusFileName = string.Format("{0}/{1}/{2}", Config.UserMissionFolder, Career.PilotName, Config.MissionStatusResultFileName);
            string missionStatusFileNameSystemPath = gameInterface.ToFileSystemPath(missionStatusFileName);
            if (File.Exists(missionStatusFileNameSystemPath)) 
            {
                ISectionFile missionStatusFile = gameInterface.SectionFileLoad(missionStatusFileName);
                return MissionStatus.Create(missionStatusFile, Random);
            }
            return null;
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
            else
            {
                RandomUnitSet randomUnitInfo = campaignInfo.RandomUnits;
                if (randomUnitInfo.AircraftRandomRed.Any() && randomUnitInfo.AircraftRandomBlue.Any())
                {
                    aircraftRandomRed = randomUnitInfo.AircraftRandomRed;
                    aircraftRandomBlue = randomUnitInfo.AircraftRandomBlue;
                }
                else
                {
                    randomUnitInfo = Config.RandomUnits;
                    aircraftRandomRed = randomUnitInfo.AircraftRandomRed;
                    aircraftRandomBlue = randomUnitInfo.AircraftRandomBlue;
                }
            }
            aircraftsRed = aircraftRandomRed.Where(x => x.IndexOf(":") == -1 || dlc.Any(y => x.StartsWith(y, StringComparison.InvariantCultureIgnoreCase)));
            aircraftsBlue = aircraftRandomBlue.Where(x => x.IndexOf(":") == -1 || dlc.Any(y => x.StartsWith(y, StringComparison.InvariantCultureIgnoreCase)));
        }

        private void GetRandomGroundActorList(MissionFile missionFile, out IEnumerable<IEnumerable<string>> groundActors)
        {
            IEnumerable<string> dlc = missionFile.DLC.Select(x => string.Format(".{0}:", x));
            CampaignInfo campaignInfo = Career.CampaignInfo;

            RandomUnitSet randomUnitInfo = campaignInfo.RandomUnits;
            if (randomUnitInfo.GroundVehicleRandomRed.Any() && randomUnitInfo.GroundVehicleRandomBlue.Any())
            {
                groundActors = new IEnumerable<string>[(int)EGroundGroupType.Count * (int)EArmy.Count]
                    {
                        CompatibleGroundTypeDLC(randomUnitInfo.GroundVehicleRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.GroundVehicleRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.GroundArmorRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.GroundArmorRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.GroundShipRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.GroundShipRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.GroundTrainRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.GroundTrainRandomBlue, dlc),
                        new string [] { "" }, new string [] { "" },
                    };
            }
            else
            {
                randomUnitInfo = Config.RandomUnits;
                groundActors = new IEnumerable<string>[(int)EGroundGroupType.Count * (int)EArmy.Count]
                    {
                        CompatibleGroundTypeDLC(randomUnitInfo.GroundVehicleRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.GroundVehicleRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.GroundArmorRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.GroundArmorRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.GroundShipRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.GroundShipRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.GroundTrainRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.GroundTrainRandomBlue, dlc),
                        new string [] { "" }, new string [] { "" },
                    };
            }
        }

        private void GetRandomStationaryList(MissionFile missionFile, out IEnumerable<IEnumerable<string>> stationaries)
        {
            IEnumerable<string> dlc = missionFile.DLC;
            CampaignInfo campaignInfo = Career.CampaignInfo;

            RandomUnitSet randomUnitInfo = campaignInfo.RandomUnits;
            if (randomUnitInfo.StationaryRadarRandomRed.Any() && randomUnitInfo.StationaryRadarRandomBlue.Any())
            {
                stationaries = new IEnumerable<string>[(int)EStationaryType.Count * (int)EArmy.Count]
                    {
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryRadarRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryRadarRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryAircraftRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryAircraftRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryArtilleryRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryArtilleryRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryFlakRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryFlakRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryDepotRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryDepotRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryShipRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryShipRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryAmmoRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryAmmoRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryWeaponsRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryWeaponsRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryCarRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryCarRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryConstCarRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryConstCarRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryEnvironmentRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryEnvironmentRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationarySearchlightRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationarySearchlightRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryAeroanchoredRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryAeroanchoredRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryAirfieldRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryAirfieldRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryUnknownRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryUnknownRandomBlue, dlc),
                    };
            }
            else
            {
                randomUnitInfo = Config.RandomUnits;
                stationaries = new IEnumerable<string>[(int)EStationaryType.Count * (int)EArmy.Count]
                    {
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryRadarRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryRadarRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryAircraftRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryAircraftRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryArtilleryRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryArtilleryRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryFlakRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryFlakRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryDepotRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryDepotRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryShipRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryShipRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryAmmoRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryAmmoRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryWeaponsRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryWeaponsRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryCarRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryCarRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryConstCarRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryConstCarRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryEnvironmentRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryEnvironmentRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationarySearchlightRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationarySearchlightRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryAeroanchoredRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryAeroanchoredRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryAirfieldRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryAirfieldRandomBlue, dlc),
                        CompatibleGroundTypeDLC(randomUnitInfo.StationaryUnknownRandomRed, dlc), CompatibleGroundTypeDLC(randomUnitInfo.StationaryUnknownRandomBlue, dlc),
                    };
            }
        }

        private IEnumerable<string> CompatibleGroundTypeDLC(IEnumerable<string> target, IEnumerable<string> dlc)
        {
            return target.Where(x => x.IndexOf(":") == -1 || dlc.Any(y => x.IndexOf(y, StringComparison.InvariantCultureIgnoreCase) != -1));
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

        public void ReinForce(IMissionStatus missionStatus, DateTime dateTime)
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
                            item.Nums = item.InitNums > 0 ? item.InitNums : 1;
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
                            item.Nums = item.InitNums > 0 ? item.InitNums : 1;
                            item.InitNums = item.Nums;
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

#if false
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
#endif

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

        private void OptimizeMissionObjects(MissionFile missionFile, IMissionStatus missionStatus, AirGroup airGroupPlayer, DateTime dateTime)
        {
            if (missionStatus != null)
            {
                double IntervalHour = (dateTime - missionStatus.DateTime).TotalHours;
                if (IntervalHour < CampaignProgress.AnyTimeBebin)
                {
                    IntervalHour = Career.CampaignProgress == ECampaignProgress.AnyTime || Career.CampaignProgress == ECampaignProgress.AnyDayAnyTime ?
                                    CampaignProgress.AnyTimeBebin : CampaignProgress.AnyDayBebin * 24;
                }

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
                    StationaryObj stationaryObject = missionStatus.Stationaries.Where(x => string.Compare(x.Name, stationary.Id, true) == 0 &&
                                                                string.Compare(x.Class, stationary.Class, true) == 0).FirstOrDefault();
                    GroundObj groundObject = missionStatus.GroundActors.Where(x => string.Compare(x.Name, stationary.Id, true) == 0 &&
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
                        AirGroupObj airGroupObject = missionStatus.AirGroups.Where(x => string.Compare(x.SquadronName, airGroup.SquadronName, true) == 0).FirstOrDefault();
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

                            if (airGroupObject.Nums == 0 || airGroupObject.InitNums == 0 || /*!airGroupObject.IsAlive || !airGroupObject.IsValid || */
                                ((airGroupObject.InitNums - airGroupObject.DiedNums) / (float)airGroupObject.InitNums) < Config.GroupDisableRate)
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
                    GroundGroupObj groundGroupObject = missionStatus.GroundGroups.Where(x => string.Compare(x.Name, groundGroup.Id, true) == 0 && x.Army == groundGroup.Army &&
                        string.Compare(MissionObjBase.CreateClassShortShortName(x.Class), MissionObjBase.CreateClassShortShortName(groundGroup.Class), true) == 0).FirstOrDefault();
                    if (groundGroupObject != null)
                    {
                        // Substitute Unit  
                        if (groundGroupObject.InitNums - groundGroupObject.Nums > 0 && substitutes.Any())
                        {
                            int numsAdd = Math.Min(groundGroupObject.InitNums - groundGroupObject.Nums, substitutes.Count());
                            groundGroupObject.Nums += numsAdd;
                            substitutes.Take(numsAdd).ToList().ForEach(x => stationaries.Remove(x));
                        }

                        if (groundGroupObject.InitNums == 0 || /*!groundGroupObject.IsAlive || !groundGroupObject.IsValid || */(groundGroupObject.Nums / (float)groundGroupObject.InitNums) < Config.GroupDisableRate)
                        {
                            groundGroups.Remove(groundGroup);
                            Debug.WriteLine("Remove GroundGroup {0}[{1}]", groundGroup.Id, groundGroup.Class);

                            // TODO: Substitute GroundGroup

                        }
                        else
                        {
                            // Position
                            if (groundGroupObject.IsValidPoint)
                            {
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
                }

                // FrontMarker
                if (Career.DynamicFrontMarker)
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
                            pos.y += distance * FrontMarkerMoveYRate * ((army == (int)EArmy.Red) ? nearestRed.Value.y < nearestBlue.Value.y ? 1 : -1 : nearestRed.Value.y < nearestBlue.Value.y ? -1 : 1);
                        }
                        frontMarkers.Add(pos);
                    }
                    missionFile.FrontMarkers.Clear();
                    missionFile.FrontMarkers.AddRange(frontMarkers);
                }
            }
        }

        private void OptimizeMissionObjects(MissionFile missionFile, IMissionStatus missionStatus, AirGroup airGroupPlayer)
        {
            if (missionStatus != null)
            {
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
                    StationaryObj stationaryObject = missionStatus.Stationaries.Where(x => string.Compare(x.Name, stationary.Id, true) == 0 &&
                                                                string.Compare(x.Class, stationary.Class, true) == 0).FirstOrDefault();
                    GroundObj groundObject = missionStatus.GroundActors.Where(x => string.Compare(x.Name, stationary.Id, true) == 0 &&
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
                        AirGroupObj airGroupObject = missionStatus.AirGroups.Where(x => string.Compare(x.SquadronName, airGroup.SquadronName, true) == 0).FirstOrDefault();
                        if (airGroupObject != null)
                        {
                            if ((airGroupObject.Nums == 0 || airGroupObject.InitNums == 0) && airGroup != airGroupPlayer)
                            {
                                Debug.WriteLine("Remove AirGroups {0}[{1}]", airGroup.Id, airGroup.Class);
                                airGroups.Remove(airGroup);
                            }
                            else
                            {
                                // Position
                                if (airGroupObject.IsValidPoint)
                                {
                                    AirGroupWaypoint wayPoint = airGroup.Waypoints.FirstOrDefault();
                                    Point3d pos = airGroupObject.Point;
                                    Debug.WriteLine("Position Update AirGroups {0}[{1}] ({2},{3},{4}) -> ({5},{6},{7})", airGroup.Id, airGroup.Class, wayPoint.X, wayPoint.Y, wayPoint.Z, pos.x, pos.y, pos.z);
                                    airGroup.UpdateStartPoint(ref pos, AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY);
                                    airGroup.MissionType = EMissionType.RESERVE;
                                }
                            }
                        }
                    }
                }

                // GroundGroups
                List<GroundGroup> groundGroups = missionFile.GroundGroups;
                for (int i = groundGroups.Count - 1; i >= 0; i--)
                {
                    GroundGroup groundGroup = groundGroups[i];
                    GroundGroupObj groundGroupObject = missionStatus.GroundGroups.Where(x => string.Compare(x.Name, groundGroup.Id, true) == 0 && x.Army == groundGroup.Army &&
                        string.Compare(MissionObjBase.CreateClassShortShortName(x.Class), MissionObjBase.CreateClassShortShortName(groundGroup.Class), true) == 0).FirstOrDefault();
                    if (groundGroupObject != null)
                    {
                        if (groundGroupObject.InitNums == 0)
                        {
                            groundGroups.Remove(groundGroup);
                            Debug.WriteLine("Remove GroundGroup {0}[{1}]", groundGroup.Id, groundGroup.Class);
                        }
                        else
                        {
                            // Position
                            if (groundGroupObject.IsValidPoint)
                            {
                                Point3d pos = groundGroupObject.Point;
                                GroundGroupWaypoint wayPoint = groundGroup.Waypoints.FirstOrDefault();
                                wayPoint.X = pos.x;
                                wayPoint.Y = pos.y;
                                Debug.WriteLine("Position Update GroundGroup {0}[{1}] ({2},{3}) -> ({4},{5})", groundGroup.Id, groundGroup.Class, wayPoint.X, wayPoint.Y, pos.x, pos.y);
                                groundGroup.MissionAssigned = true;
                            }
                        }
                    }
                }
            }
        }
        private int GetNeedAirGroupCount(MissionFile missionFile, IMissionStatus missionStatus, bool aliveMissionStatus = true)
        {
            int needAirGroups = 0;
            if (Career.SpawnDynamicAirGroups)
            {
                AiAirGroup[] aiAirGroups;
                int aliveCount = aliveMissionStatus ? missionStatus.AirGroups.Where(x => x.Nums > 0).Count() :
                                        ((aiAirGroups = GamePlay.gpAirGroups((int)EArmy.Red)) != null ? aiAirGroups.Count(x => x.NOfAirc > 0) : 0) +
                                        ((aiAirGroups = GamePlay.gpAirGroups((int)EArmy.Blue)) != null ? aiAirGroups.Count(x => x.NOfAirc > 0) : 0) +
                                        ((aiAirGroups = GamePlay.gpAirGroups((int)EArmy.None)) != null ? aiAirGroups.Count(x => x.NOfAirc > 0) : 0);
                int averages = Career.AdditionalAirOperations * Config.AverageAirOperationAirGroupCount;
                needAirGroups = averages - aliveCount;
                Debug.WriteLine("Need AirGroups={0}[{1}/{2}]", needAirGroups, aliveCount, averages);
            }
            return needAirGroups;
        }

        private int GetNeedGroundGroupCount(MissionFile missionFile, IMissionStatus missionStatus)
        {
            int needGoundGroups = 0;
            if (Career.SpawnDynamicGroundGroups)
            {
                int aliveCount = missionStatus.GroundGroups.Where(x => x.Nums >= 0).Count();
                int averages = Career.AdditionalGroundOperations * Config.AverageGroundOperationGroundGroupCount;
                needGoundGroups = averages - aliveCount;
                Debug.WriteLine("Need GroundGroups={0}[{1}/{2}]", needGoundGroups, aliveCount, averages);
            }
            return needGoundGroups;
        }

        private int GetNeedGroundUnitCount(MissionFile missionFile, IMissionStatus missionStatus)
        {
            int needGroundUnits = 0;
            if (Career.SpawnDynamicStationaries)
            {
                int aliveCount = missionStatus.Stationaries.Where(x => x.IsAlive).Count();
                int averages = Career.AdditionalGroundOperations * Config.AverageStationaryOperationUnitCount;
                int aliveGroundGroups = missionStatus.GroundGroups.Where(x => x.Nums >= 0).Count();
                needGroundUnits = averages - aliveCount - aliveGroundGroups;
                Debug.WriteLine("Need GroundUnits={0}[{1}/{2}]", needGroundUnits, aliveCount, averages);
            }
            return needGroundUnits;
        }

        private IEnumerable<string> GetAddAirGroups(MissionFile missionFile, IMissionStatus missionStatus, IMissionStatus missionStatusTotal)
        {
            IEnumerable<string> airGroups = missionFile.AirGroups.Select(x => x.SquadronName).Except(missionStatus.AirGroups.Select(x => x.SquadronName));
            if (missionStatusTotal != null)
            {
                airGroups = airGroups.Except(missionStatusTotal.AirGroups.Select(x => x.SquadronName));
            }
            return airGroups;
        }

        private IEnumerable<string> GetAddGroundGroups(MissionFile missionFile, IMissionStatus missionStatus, IMissionStatus missionStatusTotal)
        {
            IEnumerable<string> groundGroups = missionFile.GroundGroups.Select(x => x.Id).Except(missionStatus.GroundGroups.Select(x => x.Name));
            if (missionStatusTotal != null)
            {
                groundGroups = groundGroups.Except(missionStatusTotal.GroundGroups.Select(x => x.Name));
            }
            return groundGroups;
        }

        private IEnumerable<string> GetAddGroundUnits(MissionFile missionFile, IMissionStatus missionStatus, IMissionStatus missionStatusTotal)
        {
            IEnumerable<string> groundUnits = missionFile.Stationaries.Select(x => x.Id).Except(missionStatus.Stationaries.Select(x => x.Name)).Except(missionStatus.GroundActors.Select(x => x.Name));
            if (missionStatusTotal != null)
            {
                groundUnits = groundUnits.Except(missionStatusTotal.Stationaries.Select(x => x.Name)).Except(missionStatusTotal.GroundActors.Select(x => x.Name));
            }
            return groundUnits;
        }
    }
}