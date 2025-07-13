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
using maddox.game;

namespace IL2DCE.MissionObjectModel
{
    public class RandomUnitSet
    {
        #region Definition

        public const string SectionAircraft = "Aircraft";
        public const string SectionGroundVehicle = "Ground.Vehicle";
        public const string SectionGroundArmor = "Ground.Armor";
        public const string SectionGroundShip = "Ground.Ship";
        public const string SectionGroundTrain = "Ground.Train";
        public const string SectionStationaryRadar = "Stationary.Radar";
        public const string SectionStationaryAircraft = "Stationary.Aircraft";
        public const string SectionStationaryArtillery = "Stationary.Artillery";
        public const string SectionStationaryFlak = "Stationary.Flak";
        public const string SectionStationaryDepot = "Stationary.Depot";
        public const string SectionStationaryShip = "Stationary.Ship";
        public const string SectionStationaryAmmo = "Stationary.Ammo";
        public const string SectionStationaryWeapons = "Stationary.Weapons";
        public const string SectionStationaryCar = "Stationary.Car";
        public const string SectionStationaryConstCar = "Stationary.ConstCar";
        public const string SectionStationaryEnvironment = "Stationary.Environment";
        public const string SectionStationarySearchlight = "Stationary.Searchlight";
        public const string SectionStationaryAeroanchored = "Stationary.Aeroanchored";
        public const string SectionStationaryAirfield = "Stationary.Airfield";
        public const string SectionStationaryUnknown = "Stationary.Unknown";

        public const string KeyRandomRed = "RandomRed";
        public const string KeyRandomBlue = "RandomBlue";

        #endregion

        public string[] AircraftRandomRed
        {
            get;
            private set;
        }

        public string[] AircraftRandomBlue
        {
            get;
            private set;
        }

        public string[] GroundVehicleRandomRed
        {
            get;
            private set;
        }

        public string[] GroundVehicleRandomBlue
        {
            get;
            private set;
        }

        public string[] GroundArmorRandomRed
        {
            get;
            private set;
        }

        public string[] GroundArmorRandomBlue
        {
            get;
            private set;
        }

        public string[] GroundShipRandomRed
        {
            get;
            private set;
        }

        public string[] GroundShipRandomBlue
        {
            get;
            private set;
        }

        public string[] GroundTrainRandomRed
        {
            get;
            private set;
        }

        public string[] GroundTrainRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryRadarRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryRadarRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryAircraftRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryAircraftRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryArtilleryRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryArtilleryRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryFlakRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryFlakRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryDepotRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryDepotRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryShipRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryShipRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryAmmoRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryAmmoRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryWeaponsRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryWeaponsRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryCarRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryCarRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryConstCarRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryConstCarRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryEnvironmentRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryEnvironmentRandomBlue
        {
            get;
            private set;
        }

        public string[] StationarySearchlightRandomRed
        {
            get;
            private set;
        }

        public string[] StationarySearchlightRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryAeroanchoredRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryAeroanchoredRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryAirfieldRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryAirfieldRandomBlue
        {
            get;
            private set;
        }

        public string[] StationaryUnknownRandomRed
        {
            get;
            private set;
        }

        public string[] StationaryUnknownRandomBlue
        {
            get;
            private set;
        }

        public void Read(ISectionFile file)
        {
            string value;

            if (file.exist(SectionAircraft, KeyRandomRed))
            {
                value = file.get(SectionAircraft, KeyRandomRed);
                AircraftRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomRed = new string[0];
            }

            if (file.exist(SectionAircraft, KeyRandomBlue))
            {
                value = file.get(SectionAircraft, KeyRandomBlue);
                AircraftRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                AircraftRandomBlue = new string[0];
            }

            if (file.exist(SectionGroundVehicle, KeyRandomRed))
            {
                value = file.get(SectionGroundVehicle, KeyRandomRed);
                GroundVehicleRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundVehicleRandomRed = new string[0];
            }

            if (file.exist(SectionGroundVehicle, KeyRandomBlue))
            {
                value = file.get(SectionGroundVehicle, KeyRandomBlue);
                GroundVehicleRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundVehicleRandomBlue = new string[0];
            }

            if (file.exist(SectionGroundArmor, KeyRandomRed))
            {
                value = file.get(SectionGroundArmor, KeyRandomRed);
                GroundArmorRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundArmorRandomRed = new string[0];
            }

            if (file.exist(SectionGroundArmor, KeyRandomBlue))
            {
                value = file.get(SectionGroundArmor, KeyRandomBlue);
                GroundArmorRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundArmorRandomBlue = new string[0];
            }

            if (file.exist(SectionGroundShip, KeyRandomRed))
            {
                value = file.get(SectionGroundShip, KeyRandomRed);
                GroundShipRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundShipRandomRed = new string[0];
            }

            if (file.exist(SectionGroundShip, KeyRandomBlue))
            {
                value = file.get(SectionGroundShip, KeyRandomBlue);
                GroundShipRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundShipRandomBlue = new string[0];
            }

            if (file.exist(SectionGroundTrain, KeyRandomRed))
            {
                value = file.get(SectionGroundTrain, KeyRandomRed);
                GroundTrainRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundTrainRandomRed = new string[0];
            }

            if (file.exist(SectionGroundTrain, KeyRandomBlue))
            {
                value = file.get(SectionGroundTrain, KeyRandomBlue);
                GroundTrainRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                GroundTrainRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryRadar, KeyRandomRed))
            {
                value = file.get(SectionStationaryRadar, KeyRandomRed);
                StationaryRadarRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryRadarRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryRadar, KeyRandomBlue))
            {
                value = file.get(SectionStationaryRadar, KeyRandomBlue);
                StationaryRadarRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryRadarRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryAircraft, KeyRandomRed))
            {
                value = file.get(SectionStationaryAircraft, KeyRandomRed);
                StationaryAircraftRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAircraftRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryAircraft, KeyRandomBlue))
            {
                value = file.get(SectionStationaryAircraft, KeyRandomBlue);
                StationaryAircraftRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAircraftRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryArtillery, KeyRandomRed))
            {
                value = file.get(SectionStationaryArtillery, KeyRandomRed);
                StationaryArtilleryRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryArtilleryRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryArtillery, KeyRandomBlue))
            {
                value = file.get(SectionStationaryArtillery, KeyRandomBlue);
                StationaryArtilleryRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryArtilleryRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryFlak, KeyRandomRed))
            {
                value = file.get(SectionStationaryFlak, KeyRandomRed);
                StationaryFlakRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryFlakRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryFlak, KeyRandomBlue))
            {
                value = file.get(SectionStationaryFlak, KeyRandomBlue);
                StationaryFlakRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryFlakRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryDepot, KeyRandomRed))
            {
                value = file.get(SectionStationaryDepot, KeyRandomRed);
                StationaryDepotRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryDepotRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryDepot, KeyRandomBlue))
            {
                value = file.get(SectionStationaryDepot, KeyRandomBlue);
                StationaryDepotRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryDepotRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryShip, KeyRandomRed))
            {
                value = file.get(SectionStationaryShip, KeyRandomRed);
                StationaryShipRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryShipRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryShip, KeyRandomBlue))
            {
                value = file.get(SectionStationaryShip, KeyRandomBlue);
                StationaryShipRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryShipRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryAmmo, KeyRandomRed))
            {
                value = file.get(SectionStationaryAmmo, KeyRandomRed);
                StationaryAmmoRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAmmoRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryAmmo, KeyRandomBlue))
            {
                value = file.get(SectionStationaryAmmo, KeyRandomBlue);
                StationaryAmmoRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAmmoRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryWeapons, KeyRandomRed))
            {
                value = file.get(SectionStationaryWeapons, KeyRandomRed);
                StationaryWeaponsRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryWeaponsRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryWeapons, KeyRandomBlue))
            {
                value = file.get(SectionStationaryWeapons, KeyRandomBlue);
                StationaryWeaponsRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryWeaponsRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryCar, KeyRandomRed))
            {
                value = file.get(SectionStationaryCar, KeyRandomRed);
                StationaryCarRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryCarRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryCar, KeyRandomBlue))
            {
                value = file.get(SectionStationaryCar, KeyRandomBlue);
                StationaryCarRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryCarRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryConstCar, KeyRandomRed))
            {
                value = file.get(SectionStationaryConstCar, KeyRandomRed);
                StationaryConstCarRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryConstCarRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryConstCar, KeyRandomBlue))
            {
                value = file.get(SectionStationaryConstCar, KeyRandomBlue);
                StationaryConstCarRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryConstCarRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryEnvironment, KeyRandomBlue))
            {
                value = file.get(SectionStationaryEnvironment, KeyRandomBlue);
                StationaryEnvironmentRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryEnvironmentRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryEnvironment, KeyRandomRed))
            {
                value = file.get(SectionStationaryEnvironment, KeyRandomRed);
                StationaryEnvironmentRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryEnvironmentRandomBlue = new string[0];
            }

            if (file.exist(SectionStationarySearchlight, KeyRandomBlue))
            {
                value = file.get(SectionStationarySearchlight, KeyRandomBlue);
                StationarySearchlightRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationarySearchlightRandomRed = new string[0];
            }

            if (file.exist(SectionStationarySearchlight, KeyRandomRed))
            {
                value = file.get(SectionStationarySearchlight, KeyRandomRed);
                StationarySearchlightRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationarySearchlightRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryAeroanchored, KeyRandomBlue))
            {
                value = file.get(SectionStationaryAeroanchored, KeyRandomBlue);
                StationaryAeroanchoredRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAeroanchoredRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryAeroanchored, KeyRandomRed))
            {
                value = file.get(SectionStationaryAeroanchored, KeyRandomRed);
                StationaryAeroanchoredRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAeroanchoredRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryAirfield, KeyRandomBlue))
            {
                value = file.get(SectionStationaryAirfield, KeyRandomBlue);
                StationaryAirfieldRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAirfieldRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryAirfield, KeyRandomRed))
            {
                value = file.get(SectionStationaryAirfield, KeyRandomRed);
                StationaryAirfieldRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryAirfieldRandomBlue = new string[0];
            }

            if (file.exist(SectionStationaryUnknown, KeyRandomBlue))
            {
                value = file.get(SectionStationaryUnknown, KeyRandomBlue);
                StationaryUnknownRandomRed = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryUnknownRandomRed = new string[0];
            }

            if (file.exist(SectionStationaryUnknown, KeyRandomBlue))
            {
                value = file.get(SectionStationaryUnknown, KeyRandomBlue);
                StationaryUnknownRandomBlue = value.Split(Config.SplitSpace, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                StationaryUnknownRandomBlue = new string[0];
            }
        }
    }
}
