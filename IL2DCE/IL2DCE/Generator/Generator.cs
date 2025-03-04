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
        private const string SectionMain = "MAIN";

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
            get
            {
                return _core.Random;
            }
        }

        internal Core Core
        {
            get
            {
                return _core;
            }
        }
        private Core _core;

        private IGamePlay GamePlay
        {
            get
            {
                return _core.GamePlay;
            }
        }

        private Config Config
        {
            get
            {
                return _core.Config;
            }

        }

        private Career Career
        {
            get
            {
                return _core.CurrentCareer;
            }
        }

        public Generator(Core core)
        {
            _core = core;
        }

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
                if (initialMissionTemplateFile.exist("AirGroups"))
                {
                    // Delete all air groups from the template file.
                    for (int i = 0; i < initialMissionTemplateFile.lines("AirGroups"); i++)
                    {
                        string key;
                        string value;
                        initialMissionTemplateFile.get("AirGroups", i, out key, out value);
                        initialMissionTemplateFile.delete(key);
                        initialMissionTemplateFile.delete(key + "_Way");
                    }
                    initialMissionTemplateFile.delete("AirGroups");
                }

                if (initialMissionTemplateFile.exist("Chiefs"))
                {
                    // Delete all ground groups from the template file.
                    for (int i = 0; i < initialMissionTemplateFile.lines("Chiefs"); i++)
                    {
                        string key;
                        string value;
                        initialMissionTemplateFile.get("Chiefs", i, out key, out value);
                        initialMissionTemplateFile.delete(key + "_Road");
                    }
                    initialMissionTemplateFile.delete("Chiefs");
                }

                MissionFile initialMission = new MissionFile(GamePlay, initialMissionTemplateFiles, airGroupInfos);

                foreach (AirGroup airGroup in initialMission.AirGroups)
                {
                    airGroup.WriteTo(initialMissionTemplateFile);
                }

                foreach (GroundGroup groundGroup in initialMission.GroundGroups)
                {
                    groundGroup.WriteTo(initialMissionTemplateFile);
                }
            }
        }

        /// <summary>
        /// Generates the next mission template based on the previous mission template. 
        /// </summary>
        /// <param name="staticTemplateFiles"></param>
        /// <param name="previousMissionTemplate"></param>
        /// <param name="missionTemplateFile"></param>
        /// <param name="airGroupInfos"></param>
        /// <remarks>
        /// For now it has a simplified implementaiton. It only generated random supply ships and air groups.
        /// </remarks>
        public void GenerateMissionTemplate(IEnumerable<string> staticTemplateFiles, ISectionFile previousMissionTemplate, out ISectionFile missionTemplateFile, AirGroupInfos airGroupInfos = null)
        {
            MissionFile staticTemplateFile = new MissionFile(GamePlay, staticTemplateFiles, airGroupInfos);

            // Use the previous mission template to initialise the next mission template.
            missionTemplateFile = previousMissionTemplate;

            // Remove the ground groups but keep the air groups.
            if (missionTemplateFile.exist("Chiefs"))
            {
                // Delete all ground groups from the template file.
                for (int i = 0; i < missionTemplateFile.lines("Chiefs"); i++)
                {
                    string key;
                    string value;
                    missionTemplateFile.get("Chiefs", i, out key, out value);
                    missionTemplateFile.delete(key + "_Road");
                }
                missionTemplateFile.delete("Chiefs");
            }

            if (missionTemplateFile.exist("Stationary"))
            {
                // Delete all stationaries from the template file.
                missionTemplateFile.delete("Stationary");
            }

            // Generate supply ships and trains.

            // For now generate a random supply ship on one of the routes to a harbour.
            int chiefIndex = 0;
            int stationaryIndex = 0;

            int army;
            int army2;
            // TODO: Only create a random (or decent) amount of supply ships.
            foreach (Waterway waterway in staticTemplateFile.Waterways)
            {
                army = GamePlay.gpFrontArmy(waterway.End.X, waterway.End.Y);
                // For waterways only the end must be in friendly territory.
                if (army == (int)EArmy.Red)
                {
                    string id = chiefIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For red army
                    GroundGroup supplyShip = new GroundGroup(id, "Ship.Tanker_Medium1", ECountry.gb, "/sleep 0/skill 2/slowfire 1", waterway.Waypoints);
                    supplyShip.WriteTo(missionTemplateFile);
                }
                else if (army == (int)EArmy.Blue)
                {
                    string id = chiefIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For blue army
                    GroundGroup supplyShip = new GroundGroup(id, "Ship.Tanker_Medium1", ECountry.de, "/sleep 0/skill 2/slowfire 1", waterway.Waypoints);
                    supplyShip.WriteTo(missionTemplateFile);
                }
                else
                {
                    Debug.WriteLine("no Army Waterway[End.X:{0}, End.Y:{1}]", waterway.End.X, waterway.End.Y);
                }
            }

            foreach (Waterway railway in staticTemplateFile.Railways)
            {
                army = GamePlay.gpFrontArmy(railway.Start.X, railway.Start.Y);
                army2 = GamePlay.gpFrontArmy(railway.End.X, railway.End.Y);
                // For railways the start and the end must be in friendly territory.
                if (army == (int)EArmy.Red && army2 == (int)EArmy.Red)
                {
                    string id = chiefIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For red army
                    GroundGroup supplyShip = new GroundGroup(id, "Train.57xx_0-6-0PT_c0", ECountry.gb, "", railway.Waypoints);
                    supplyShip.WriteTo(missionTemplateFile);
                }
                else if (army == (int)EArmy.Blue && army2 == (int)EArmy.Blue)
                {
                    string id = chiefIndex.ToString(CultureInfo.InvariantCulture.NumberFormat) + "_Chief";
                    chiefIndex++;

                    // For blue army
                    GroundGroup supplyShip = new GroundGroup(id, "Train.BR56-00_c2", ECountry.de, "", railway.Waypoints);
                    supplyShip.WriteTo(missionTemplateFile);
                }
                else
                {
                    Debug.WriteLine("no Army Railway[Start.X:{0}, Start.Y:{1}, End.X:{2}, End.Y:{3}]", railway.Start.X, railway.Start.Y, railway.End.X, railway.End.Y);
                }
            }

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

            foreach (Stationary aircraft in staticTemplateFile.Aircraft)
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
                    Debug.WriteLine("no Army Aircraft[Class:{0}, Id{:1}, X:{2}, Y:{3}]", aircraft.Class, aircraft.Id, aircraft.X, aircraft.Y);
                }
            }

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
                    Debug.WriteLine("no Army Artillery[Class:{0}, Id{:1}, X:{2}, Y:{3}]", artillery.Class, artillery.Id, artillery.X, artillery.Y);
                }
            }

            foreach (Stationary radar in staticTemplateFile.Radar)
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
                    Debug.WriteLine("no Army Radar[Class:{0}, Id{:1}, X:{2}, Y:{3}]", radar.Class, radar.Id, radar.X, radar.Y);
                }
            }
        }

        public void GenerateMission(string environmentTemplateFile, string missionTemplateFileName, string missionId, out ISectionFile missionFile, out BriefingFile briefingFile)
        {
            MissionFile missionTemplateFile = new MissionFile(GamePlay.gpLoadSectionFile(missionTemplateFileName), Career.CampaignInfo.AirGroupInfos);

            GeneratorAirOperation = new GeneratorAirOperation(this, Career.CampaignInfo, missionTemplateFile, Core.GamePlay, Core.Config);
            GeneratorGroundOperation = new GeneratorGroundOperation(this, Career.CampaignInfo, missionTemplateFile, Core.GamePlay, Core.Config);
            GeneratorBriefing = new GeneratorBriefing(Core, this);

            // Load the environment template file for the generated mission.

            missionFile = GamePlay.gpLoadSectionFile(environmentTemplateFile);
            briefingFile = new BriefingFile();

            briefingFile.MissionName = missionId;
            briefingFile.MissionDescription = "";

            // Delete things from the template file.

            // It is not necessary to delete air groups and ground groups from the missionFile as it 
            // is based on the environment template. If there is anything in it (air groups, ...) it is intentional.
            for (int i = 0; i < missionFile.lines(SectionMain); i++)
            {
                // Delete player from the template file.
                string key;
                string value;
                missionFile.get(SectionMain, i, out key, out value);
                if (key == "player")
                {
                    missionFile.delete(SectionMain, i);
                    break;
                }
            }

            // Add things to the template file.
            double time = Career.Time < 0 ? Core.Random.Next(5, 21): Career.Time;
            missionFile.set(SectionMain, "TIME", time.ToString(CultureInfo.InvariantCulture.NumberFormat));

            int weatherIndex = Career.Weather < 0 ? Core.Random.Next(0, 3): (int)Career.Weather;
            missionFile.set(SectionMain, "WeatherIndex", weatherIndex.ToString(CultureInfo.InvariantCulture.NumberFormat));

            int cloudsHeight = Career.CloudAltitude < 0 ? Core.Random.Next(5, 15) * 100: Career.CloudAltitude;
            missionFile.set(SectionMain, "CloudsHeight", (cloudsHeight).ToString(CultureInfo.InvariantCulture.NumberFormat));

            string weatherString = "";
            if (weatherIndex == 0)
            {
                weatherString = "Clear";
            }
            else if (weatherIndex == 1)
            {
                weatherString = "Light clouds at " + cloudsHeight + "m";
            }
            else if (weatherIndex == 2)
            {
                weatherString = "Medium clouds at " + cloudsHeight + "m";
            }

            briefingFile.MissionDescription += Career.CampaignInfo.Name + "\n";
            briefingFile.MissionDescription += "Date: " + Career.Date.Value.ToShortDateString() + "\n";
            briefingFile.MissionDescription += "Time: " + MissionTime.ToString(time) + "\n";
            briefingFile.MissionDescription += "Weather: " + weatherString + "\n";

            // Create a air operation for the player.
            AirGroup airGroup = GeneratorAirOperation.AvailableAirGroups.Where(x => x.ArmyIndex == Career.ArmyIndex && string.Compare(x.ToString(), Career.AirGroup) == 0).FirstOrDefault();
            if (airGroup != null)
            {
                bool result;
                EMissionType? missionType = Career.MissionType;
                Spawn spawn = new Spawn(Career.Spawn);
                if (missionType == null)
                {
                    result = GeneratorAirOperation.CreateRandomAirOperation(missionFile, briefingFile, airGroup, Career.PlayerAirGroupSkill, spawn);
                }
                else
                {
                    result = GeneratorAirOperation.CreateAirOperation(missionFile, briefingFile, airGroup, missionType.Value, Career.AllowDefensiveOperation,
                                                                Career.EscortAirGroup, Career.TargetGroundGroup, Career.TargetStationary, Career.PlayerAirGroupSkill, spawn);
                }

                if (!result)
                {
                    throw new ArgumentException(string.Format("no available Player Mission[Mission:{0} AirGroup:{1}]", missionId, airGroup));
                }

                // Determine the aircraft that is controlled by the player.
                List<string> aircraftOrder = determineAircraftOrder(airGroup);

                string playerAirGroupKey = airGroup.AirGroupKey;
                int playerSquadronIndex = airGroup.SquadronIndex;
                if (aircraftOrder.Count > 0)
                {
                    string playerPosition = aircraftOrder[aircraftOrder.Count - 1];

                    double factor = aircraftOrder.Count / 6;
                    int playerPositionIndex = (int)(Math.Floor(Career.RankIndex * factor));
                    playerPosition = aircraftOrder[aircraftOrder.Count - 1 - playerPositionIndex];
                    string playerInfo = AirGroup.CreateSquadronString(playerAirGroupKey, playerSquadronIndex) + playerPosition;
                    if (missionFile.exist(SectionMain, "player"))
                    {
                        missionFile.set(SectionMain, MissionFile.KeyPlayer, playerInfo);
                    }
                    else
                    {
                        missionFile.add(SectionMain, MissionFile.KeyPlayer, playerInfo);
                    }
                }
            }
            else
            {
                throw new NotImplementedException(string.Format("Invalid ArmyIndex[{0}] and AirGroup[{1}]", Career.ArmyIndex, Career.AirGroup));
            }

            // Add additional air operations.
            if (GeneratorAirOperation.AvailableAirGroups.Count > 0)
            {
                for (int i = 0; i < Config.AdditionalAirOperations; i++)
                {
                    if (GeneratorAirOperation.AvailableAirGroups.Count > 0)
                    {
                        int randomAirGroupIndex = Core.Random.Next(GeneratorAirOperation.AvailableAirGroups.Count);
                        AirGroup randomAirGroup = GeneratorAirOperation.AvailableAirGroups[randomAirGroupIndex];
                        GeneratorAirOperation.CreateRandomAirOperation(missionFile, briefingFile, randomAirGroup);
                    }
                }
            }

            // Add additional ground operations.
            if (GeneratorGroundOperation.AvailableGroundGroups.Count > 0)
            {
                for (int i = 0; i < Config.AdditionalGroundOperations; i++)
                {
                    if (GeneratorGroundOperation.AvailableGroundGroups.Count > 0)
                    {
                        int randomGroundGroupIndex = Core.Random.Next(GeneratorGroundOperation.AvailableGroundGroups.Count);
                        GroundGroup randomGroundGroup = GeneratorGroundOperation.AvailableGroundGroups[randomGroundGroupIndex];
                        GeneratorGroundOperation.CreateRandomGroundOperation(missionFile, randomGroundGroup);
                    }
                }
            }

            // Add all stationaries.
            if (GeneratorGroundOperation.AvailableStationaries.Count > 0)
            {
                for (int i = 0; i < GeneratorGroundOperation.AvailableStationaries.Count; i++)
                {
                    Stationary stationary = GeneratorGroundOperation.AvailableStationaries[i];
                    stationary.WriteTo(missionFile);
                }
            }
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
            else if (airGroupInfo.FlightSize % 1 == 0)
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