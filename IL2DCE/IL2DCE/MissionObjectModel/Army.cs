// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover
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

namespace IL2DCE.MissionObjectModel
{
    public enum EArmy
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Count = 2,
    }

    public class Army
    {
        public EArmy Value
        {
            get;
            set;
        }

        public static EArmy Parse(ECountry country)
        {
            if (country == ECountry.gb || country == ECountry.fr || country == ECountry.us || country == ECountry.ru || country == ECountry.rz || country == ECountry.pl)
            {
                return EArmy.Red;
            }
            else if (country == ECountry.de || country == ECountry.it || country == ECountry.ja || country == ECountry.ro || country == ECountry.fi || country == ECountry.hu)
            {
                return EArmy.Blue;
            }
            else
            {
                return EArmy.None;
            }
        }

        public static ECountry DefaultCountry(EArmy army)
        {
            if (army == EArmy.Red)
            {
                return ECountry.gb;
            }
            else if (army == EArmy.Blue)
            {
                return ECountry.de;
            }
            else
            {
                return ECountry.nn;
            }
        }

        public static int Enemy(int army)
        {
            return (int)Enemy((EArmy)army);
        }

        public static EArmy Enemy(EArmy army)
        {
            if (army == EArmy.Red)
            {
                return EArmy.Blue;
            }
            else if (army == EArmy.Blue)
            {
                return EArmy.Red;
            }
            else
            {
                return EArmy.None;
            }
        }
    }
}
