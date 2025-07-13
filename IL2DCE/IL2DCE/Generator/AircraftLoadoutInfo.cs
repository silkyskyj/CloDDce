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
using System.Linq;
using IL2DCE.MissionObjectModel;
using maddox.game;

namespace IL2DCE.Generator
{
    public class AircraftLoadoutInfo
    {
        public int[] Weapons
        {
            get;
            private set;
        }

        public List<string> Detonator
        {
            get;
            private set;
        }

        public AircraftLoadoutInfo(int[] weapons, List<string> detonator)
        {
            Weapons = weapons;
            Detonator = detonator;
        }

        public AircraftLoadoutInfo(ISectionFile aircraftInfoFile, string aircraft, string loadoutId)
        {
            AircraftLoadoutInfo aircraftLoadoutInfo = Create(aircraftInfoFile, aircraft, loadoutId, true);
            Weapons = aircraftLoadoutInfo.Weapons;
            Detonator = aircraftLoadoutInfo.Detonator;
        }

        public static AircraftLoadoutInfo Create(ISectionFile aircraftInfoFile, string aircraft, string loadoutId, bool throwException = false)
        {
            string section = string.Format("{0}_{1}", aircraft, loadoutId);
            if (aircraftInfoFile.exist(section))
            {
                if (aircraftInfoFile.exist(section, MissionFile.KeyWeapons))
                {
                    // Weapons
                    string weaponsLine = aircraftInfoFile.get(section, MissionFile.KeyWeapons);
                    string[] weaponsList = weaponsLine.Split(new char[] { ' ' });
                    if (weaponsList.Length > 0)
                    {
                        int weapon;
                        int[] weapons = weaponsList.Where(x => int.TryParse(x, out weapon)).Select(x => int.Parse(x)).ToArray();

                        string key;
                        string value;
                        List<string> detonator = new List<string>();
                        int lines = aircraftInfoFile.lines(section);
                        for (int i = 0; i < lines; i++)
                        {
                            aircraftInfoFile.get(section, i, out key, out value);
                            if (string.Compare(key, MissionFile.KeyDetonator, true) == 0)
                            {
                                detonator.Add(value);
                            }
                        }

                        return new AircraftLoadoutInfo(weapons, detonator);
                    }
                }
                else if (throwException)
                {
                    throw new FormatException(string.Format("Invalid Aircraft Loadout Info[{0}_{1}.Weapons]", aircraft, loadoutId));
                }
            }
            else if (throwException)
            {
                throw new ArgumentException(string.Format("Invalid Aircraft Loadout Info[{0}_{1}]", aircraft, loadoutId));
            }
            return null;
        }
    }
}