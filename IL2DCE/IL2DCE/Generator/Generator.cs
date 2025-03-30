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
    class Generator
    {

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

        internal IRandom Random
        {
            get;
            set;
        }

        private IGamePlay GamePlay
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

        #region Constructor

        public Generator(IGamePlay gamePlay, IRandom random, Config config, Career career)
        {
            GamePlay = gamePlay;
            Random = random;
            Config = config;
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
                    // Delete all air groups from the template file.
                    int lines = initialMissionTemplateFile.lines(MissionFile.SectionAirGroups);
                    for (int i = 0; i < lines; i++)
                    {
                        string key;
                        string value;
                        initialMissionTemplateFile.get(MissionFile.SectionAirGroups, i, out key, out value);
                        initialMissionTemplateFile.delete(key);
                        initialMissionTemplateFile.delete(string.Format("{0}_{1}", key, MissionFile.SectionWay));
                    }
                    initialMissionTemplateFile.delete(MissionFile.SectionAirGroups);
                }

                if (initialMissionTemplateFile.exist(MissionFile.SectionChiefs))
                {
                    // Delete all ground groups from the template file.
                    int lines = initialMissionTemplateFile.lines(MissionFile.SectionChiefs);
                    for (int i = 0; i < lines; i++)
                    {
                        string key;
                        string value;
                        initialMissionTemplateFile.get(MissionFile.SectionChiefs, i, out key, out value);
                        initialMissionTemplateFile.delete(string.Format("{0}_{1}", key, MissionFile.SectionRoad));
                    }
                    initialMissionTemplateFile.delete(MissionFile.SectionChiefs);
                }
                MissionFile initialMission = new MissionFile(GamePlay, initialMissionTemplateFiles, airGroupInfos);

                foreach (AirGroup airGroup in initialMission.AirGroups)
                {
#if DEBUG
                    AirGroupWaypoint way = airGroup.Waypoints.FirstOrDefault();
                    Debug.WriteLine("Init Name={0} Pos=({1:F2},{2:F2},{3:F2}) V={4:F2}, AirStart={5}, SetOnParked={6}, SpawnFromScript={7}({8})", airGroup.DisplayDetailName, way.X, way.Y, way.Z, way.V, airGroup.Airstart, airGroup.SetOnParked, airGroup.SpawnFromScript, airGroup.Spawn != null ? airGroup.Spawn.Time.Value.ToString(): string.Empty);
#endif
                    airGroup.WriteTo(initialMissionTemplateFile);
                }

                foreach (GroundGroup groundGroup in initialMission.GroundGroups)
                {
#if DEBUG
                    GroundGroupWaypoint way = groundGroup.Waypoints.FirstOrDefault();
                    Debug.WriteLine("Init Class={0} Id={1} Pos=({2:F2},{3:F2}) V={4:F2}", groundGroup.Class, groundGroup.Id, way.X, way.Y,  way.V.Value);
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

            int i;
            // Delete all ground groups from the template file. (=initialMissionTemplate)
            if (missionTemplateFile.exist(MissionFile.SectionChiefs))
            {
                int lines = missionTemplateFile.lines(MissionFile.SectionChiefs);
                for (i = 0; i < lines; i++)
                {
                    string key;
                    string value;
                    missionTemplateFile.get(MissionFile.SectionChiefs, i, out key, out value);
                    missionTemplateFile.delete(string.Format("{0}_{1}", key, MissionFile.SectionRoad));
                }
                missionTemplateFile.delete(MissionFile.SectionChiefs);
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

            int army;
            int army2;

            //// 0. Chief
            //foreach (GroundGroup groundGroup in staticTemplateFile.GroundGroups)
            //{
            //    Point2d point = groundGroup.Position;
            //    army = GamePlay.gpFrontArmy(point.x, point.y);
            //    if (army == (int)EArmy.Red || army == (int)EArmy.Blue)
            //    {
            //        groundGroup.UpdateArmy(army);
            //    }
            //    else
            //    {
            //        Debug.WriteLine("no Army GroundGroup[X:{0:F2}, Y:{1:F2}] {2}[{3}]", point.x, point.y, groundGroup.DisplayName, groundGroup.Id);
            //    }
            //}

            // 1. Roads
            // TODO: Only create a random 
            foreach (Groundway groundway in staticTemplateFile.Roads)
            {
                army = GamePlay.gpFrontArmy(groundway.End.X, groundway.End.Y);
                // For waterways only the end must be in friendly territory.
                if (army == (int)EArmy.Red)
                {
                    string id = chiefIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For red army        // Armor.Cruiser_Mk_IVA nn /num_units 8
                    GroundGroup armor = new GroundGroup(id, "Armor.Cruiser_Mk_IVA", (int)EArmy.Red, ECountry.gb, "/num_units 8", groundway.Waypoints);
                    armor.WriteTo(missionTemplateFile);
                }
                else if (army == (int)EArmy.Blue)
                {
                    string id = chiefIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For blue army        // Armor.Pz_35t nn /num_units 8
                    GroundGroup armor = new GroundGroup(id, "Armor.Pz_35t", (int)EArmy.Blue, ECountry.de, "/num_units 8", groundway.Waypoints);
                    armor.WriteTo(missionTemplateFile);
                }
                else
                {
                    Debug.WriteLine("no Army Roads[End.X:{0}, End.Y:{1}]", groundway.End.X, groundway.End.Y);
                }
            }

            // 2. Waterways
            // TODO: Only create a random (or decent) amount of supply ships.
            foreach (Groundway groundway in staticTemplateFile.Waterways)
            {
                army = GamePlay.gpFrontArmy(groundway.End.X, groundway.End.Y);
                // For waterways only the end must be in friendly territory.
                if (army == (int)EArmy.Red)
                {
                    string id = chiefIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For red army
                    GroundGroup supplyShip = new GroundGroup(id, "Ship.Tanker_Medium1", (int)EArmy.Red, ECountry.gb, "/sleep 0/skill 2/slowfire 1", groundway.Waypoints);
                    supplyShip.WriteTo(missionTemplateFile);
                }
                else if (army == (int)EArmy.Blue)
                {
                    string id = chiefIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For blue army
                    GroundGroup supplyShip = new GroundGroup(id, "Ship.Tanker_Medium1", (int)EArmy.Blue, ECountry.de, "/sleep 0/skill 2/slowfire 1", groundway.Waypoints);
                    supplyShip.WriteTo(missionTemplateFile);
                }
                else
                {
                    Debug.WriteLine("no Army Waterway[End.X:{0}, End.Y:{1}]", groundway.End.X, groundway.End.Y);
                }
            }

            // 3. Railways
            foreach (Groundway railway in staticTemplateFile.Railways)
            {
                army = GamePlay.gpFrontArmy(railway.Start.X, railway.Start.Y);
                army2 = GamePlay.gpFrontArmy(railway.End.X, railway.End.Y);
                // For railways the start and the end must be in friendly territory.
                if (army == (int)EArmy.Red && army2 == (int)EArmy.Red)
                {
                    string id = chiefIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For red army
                    GroundGroup train = new GroundGroup(id, "Train.57xx_0-6-0PT_c0", (int)EArmy.Red, ECountry.gb, "", railway.Waypoints);
                    train.WriteTo(missionTemplateFile);
                }
                else if (army == (int)EArmy.Blue && army2 == (int)EArmy.Blue)
                {
                    string id = chiefIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For blue army
                    GroundGroup train = new GroundGroup(id, "Train.BR56-00_c2", (int)EArmy.Blue, ECountry.de, "", railway.Waypoints);
                    train.WriteTo(missionTemplateFile);
                }
                else
                {
                    Debug.WriteLine("no Army Railway[Start.X:{0}, Start.Y:{1}, End.X:{2}, End.Y:{3}]", railway.Start.X, railway.Start.Y, railway.End.X, railway.End.Y);
                }
            }

            // 4. Depots (Buildings)
            foreach (Building depot in staticTemplateFile.Depots)
            {
                army = GamePlay.gpFrontArmy(depot.X, depot.Y);
                // For depots the position must be in friendly territory.
                if (army == (int)EArmy.Red)
                {
                    string id = string.Format(CultureInfo.InvariantCulture.NumberFormat, "Static{0:D}", stationaryIndex);
                    stationaryIndex++;

                    // For red army
                    Stationary fuelTruck = new Stationary(id, "Stationary.Morris_CS8_tank", ECountry.gb, depot.X, depot.Y, depot.Direction);
                    fuelTruck.WriteTo(missionTemplateFile);
                }
                else if (army == (int)EArmy.Blue)
                {
                    string id = string.Format(CultureInfo.InvariantCulture.NumberFormat, "Static{0:D}", stationaryIndex);
                    stationaryIndex++;

                    // For blue army
                    Stationary fuelTruck = new Stationary(id, "Stationary.Opel_Blitz_fuel", ECountry.de, depot.X, depot.Y, depot.Direction);
                    fuelTruck.WriteTo(missionTemplateFile);
                }
                else
                {
                    Debug.WriteLine("no Army Building[Class:{0}, Id:{1}, X:{2}, Y:{3}]", depot.Class, depot.Id, depot.X, depot.Y);
                }
            }

            // 5. Aircraft
            foreach (Stationary aircraft in staticTemplateFile.Aircrafts)
            {
                army = GamePlay.gpFrontArmy(aircraft.X, aircraft.Y);
                // For aircraft the position must be in friendly territory.
                if (army == (int)EArmy.Red)
                {
                    string id = string.Format(CultureInfo.InvariantCulture.NumberFormat, "Static{0:D}", stationaryIndex);
                    stationaryIndex++;

                    // For red army
                    Stationary fuelTruck = new Stationary(id, "Stationary.HurricaneMkI_dH5-20", ECountry.gb, aircraft.X, aircraft.Y, aircraft.Direction);
                    fuelTruck.WriteTo(missionTemplateFile);
                }
                else if (army == (int)EArmy.Blue)
                {
                    string id = string.Format(CultureInfo.InvariantCulture.NumberFormat, "Static{0:D}", stationaryIndex);
                    stationaryIndex++;

                    // For blue army
                    Stationary fuelTruck = new Stationary(id, "Stationary.Bf-109E-1", ECountry.de, aircraft.X, aircraft.Y, aircraft.Direction);
                    fuelTruck.WriteTo(missionTemplateFile);
                }
                else
                {
                    Debug.WriteLine("no Army Aircraft[Class:{0}, Id:{1}, X:{2}, Y:{3}]", aircraft.Class, aircraft.Id, aircraft.X, aircraft.Y);
                }
            }

            // 6. Artilleries
            foreach (Stationary artillery in staticTemplateFile.Artilleries)
            {
                army = GamePlay.gpFrontArmy(artillery.X, artillery.Y);
                // For artillery the position must be in friendly territory.
                if (army == (int)EArmy.Red)
                {
                    string id = string.Format(CultureInfo.InvariantCulture.NumberFormat, "Static{0:D}", stationaryIndex);
                    stationaryIndex++;

                    // For red army
                    Stationary aaGun = new Stationary(id, "Artillery.Bofors", ECountry.gb, artillery.X, artillery.Y, artillery.Direction, "/timeout 0/radius_hide 0");
                    aaGun.WriteTo(missionTemplateFile);
                }
                else if (army == (int)EArmy.Blue)
                {
                    string id = string.Format(CultureInfo.InvariantCulture.NumberFormat, "Static{0:D}", stationaryIndex);
                    stationaryIndex++;

                    // For blue army
                    Stationary aaGun = new Stationary(id, "Artillery.4_cm_Flak_28", ECountry.de, artillery.X, artillery.Y, artillery.Direction, "/timeout 0/radius_hide 0");
                    aaGun.WriteTo(missionTemplateFile);
                }
                else
                {
                    Debug.WriteLine("no Army Artillery[Class:{0}, Id:{1}, X:{2}, Y:{3}]", artillery.Class, artillery.Id, artillery.X, artillery.Y);
                }
            }

            // 7. Radar
            foreach (Stationary radar in staticTemplateFile.Radars)
            {
                army = GamePlay.gpFrontArmy(radar.X, radar.Y);
                // For artillery the position must be in friendly territory.
                if (army == (int)EArmy.Red)
                {
                    string id = string.Format(CultureInfo.InvariantCulture.NumberFormat, "Static{0:D}", stationaryIndex);
                    stationaryIndex++;

                    // For red army
                    Stationary radarSite = new Stationary(id, "Stationary.Radar.EnglishRadar1", ECountry.gb, radar.X, radar.Y, radar.Direction);
                    radarSite.WriteTo(missionTemplateFile);
                }
                else if (army == (int)EArmy.Blue)
                {
                    string id = string.Format(CultureInfo.InvariantCulture.NumberFormat, "Static{0:D}", stationaryIndex);
                    stationaryIndex++;

                    // For blue army
                    Stationary radarSite = new Stationary(id, "Stationary.Radar.Wotan_II", ECountry.de, radar.X, radar.Y, radar.Direction);
                    radarSite.WriteTo(missionTemplateFile);
                }
                else
                {
                    Debug.WriteLine("no Army Radar[Class:{0}, Id:{1}, X:{2}, Y:{3}]", radar.Class, radar.Id, radar.X, radar.Y);
                }
            }

            // 8. FrontMarker
            i = 0;
            foreach (Point3d point in staticTemplateFile.FrontMarkers)
            {
                string key = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}{1}", MissionFile.SectionFrontMarker, i + 1);
                string value = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:F2} {1:F2} {2}", point.x, point.y, (int)point.z);
                missionTemplateFile.add(MissionFile.SectionFrontMarker, key, value);
                i++;
            }
        }

        public void GenerateMission(string environmentTemplateFile, string missionTemplateFileName, string missionId, out ISectionFile missionFile, out BriefingFile briefingFile)
        {
            CampaignInfo campaignInfo = Career.CampaignInfo;
            MissionFile missionTemplateFile = new MissionFile(GamePlay.gpLoadSectionFile(missionTemplateFileName), campaignInfo.AirGroupInfos);

            AirGroup airGroup = missionTemplateFile.AirGroups.Where(x => x.ArmyIndex == Career.ArmyIndex && string.Compare(x.ToString(), Career.AirGroup) == 0).FirstOrDefault();
            if (airGroup == null)
            {
                throw new NotImplementedException(string.Format("Invalid ArmyIndex[{0}] and AirGroup[{1}]", Career.ArmyIndex, Career.AirGroup));
            }

            GeneratorGroundOperation = new GeneratorGroundOperation(GamePlay, Config, Random, campaignInfo, missionTemplateFile.GroundGroups, missionTemplateFile.Stationaries, missionTemplateFile.FrontMarkers);
            GeneratorBriefing = new GeneratorBriefing(GamePlay);
            GeneratorAirOperation = new GeneratorAirOperation(GamePlay, Config, Random, GeneratorGroundOperation, GeneratorBriefing, campaignInfo, missionTemplateFile.AirGroups, airGroup);

            if (Career.AdditionalAirGroups)
            {
                IEnumerable<string> aircraftsRed;
                IEnumerable<string> aircraftsBlue;
                GetRandomAircraftList(missionTemplateFile, out aircraftsRed, out aircraftsBlue);
                GeneratorAirOperation.AddRandomAirGroups(Career.AdditionalAirOperations, aircraftsRed, aircraftsBlue);
            }

            // Load the environment template file for the generated mission.
            missionFile = GamePlay.gpLoadSectionFile(environmentTemplateFile);
            briefingFile = new BriefingFile();

            briefingFile.MissionName = missionId;
            briefingFile.MissionDescription = string.Empty;

            // Delete things from the template file.

            // It is not necessary to delete air groups and ground groups from the missionFile as it 
            // is based on the environment template. If there is anything in it (air groups, ...) it is intentional.
            SilkySkyCloDFile.Delete(missionFile, MissionFile.SectionMain, MissionFile.KeyPlayer);

            // Add things to the template file.
            double time = Career.Time == (int)MissionTime.Random ? Random.Next((int)MissionTime.Begin, (int)MissionTime.End + 1) : Career.Time < 0 ? missionTemplateFile.Time : Career.Time;
            missionFile.set(MissionFile.SectionMain, MissionFile.KeyTime, time.ToString(CultureInfo.InvariantCulture.NumberFormat));

            int weatherIndex = Career.Weather == (int)EWeather.Random ? Random.Next((int)EWeather.Clear, (int)EWeather.Count) : Career.Weather < 0 ? missionTemplateFile.WeatherIndex: (int)Career.Weather;
            missionFile.set(MissionFile.SectionMain, MissionFile.KeyWeatherIndex, weatherIndex.ToString(CultureInfo.InvariantCulture.NumberFormat));

            int cloudsHeight = Career.CloudAltitude == (int)CloudAltitude.Random ? Random.Next(CloudAltitude.Min / 100, CloudAltitude.Max / 100 + 1) * 100: Career.CloudAltitude < 0 ? missionTemplateFile.CloudsHeight: Career.CloudAltitude;
            missionFile.set(MissionFile.SectionMain, MissionFile.KeyCloudsHeight, cloudsHeight.ToString(CultureInfo.InvariantCulture.NumberFormat));

            string weatherString = string.Empty;
            if (weatherIndex == (int)EWeather.Clear)
            {
                weatherString = EWeather.Clear.ToDescription();
            }
            else if (weatherIndex == (int)EWeather.LightClouds)
            {
                weatherString = string.Format(CultureInfo.InvariantCulture.NumberFormat, "Light clouds at {0}m", cloudsHeight);
            }
            else if (weatherIndex == (int)EWeather.MediumClouds)
            {
                weatherString = string.Format(CultureInfo.InvariantCulture.NumberFormat, "Medium clouds at {0}m", cloudsHeight);
            }

            briefingFile.MissionDescription = string.Format("{0}\nDate: {1}\nTime: {2}\nWeather: {3}", 
                                                                campaignInfo.Id, Career.Date.Value.ToString("d", DateTimeFormatInfo.InvariantInfo), MissionTime.ToString(time), weatherString);

            // Create a air operation for the player.
            EMissionType? missionType = Career.MissionType;
            Spawn spawn = new Spawn(Career.Spawn, Career.SpawnRandomAltitudeFriendly, Career.SpawnRandomAltitudeEnemy, new SpawnLocation(Career.SpawnRandomLocationPlayer, Career.SpawnRandomLocationFriendly, Career.SpawnRandomLocationEnemy), 
                Career.SpawnRandomTimeFriendly, Career.SpawnRandomTimeEnemy, Career.SpawnRandomTimeFriendly || Career.SpawnRandomTimeEnemy ? new SpawnTime(true, 0, Career.SpawnRandomTimeBeginSec, Career.SpawnRandomTimeEndSec): null);
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
            List<string> aircraftOrder = determineAircraftOrder(airGroup);

            string playerAirGroupKey = airGroup.AirGroupKey;
            int playerSquadronIndex = airGroup.SquadronIndex;
            if (aircraftOrder.Count > 0)
            {
                string playerPosition = aircraftOrder.Last();

                double factor = aircraftOrder.Count / 6;
                int playerPositionIndex = (int)(Math.Floor(Career.RankIndex * factor));
                playerPosition = aircraftOrder[aircraftOrder.Count - 1 - playerPositionIndex];
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
                string key = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}{1}", MissionFile.SectionFrontMarker, i + 1);
                string value = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:F2} {1:F2} {2}", point.x, point.y, (int)point.z);
                missionFile.add(MissionFile.SectionFrontMarker, key, value);
                i++;
            }

#if DEBUG
            GeneratorAirOperation.TraceAssignedAirGroups();
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

        private static List<string> determineAircraftOrder(AirGroup airGroup)
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
                            aircraftOrder.Add(key.ToString(CultureInfo.InvariantCulture.NumberFormat) + i.ToString(CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }

                    foreach (int key in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[key].Count > i + 3)
                        {
                            aircraftOrder.Add(key.ToString(CultureInfo.InvariantCulture.NumberFormat) + (i + 3).ToString(CultureInfo.InvariantCulture.NumberFormat));
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
                            aircraftOrder.Add(key.ToString(CultureInfo.InvariantCulture.NumberFormat) + i.ToString(CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }

                    foreach (int key in airGroup.Flights.Keys)
                    {
                        if (airGroup.Flights[key].Count > i + 2)
                        {
                            aircraftOrder.Add(key.ToString(CultureInfo.InvariantCulture.NumberFormat) + (i + 2).ToString(CultureInfo.InvariantCulture.NumberFormat));
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
                        aircraftOrder.Add(key.ToString(CultureInfo.InvariantCulture.NumberFormat) + "0");
                    }
                }
            }

            return aircraftOrder;
        }
    }
}