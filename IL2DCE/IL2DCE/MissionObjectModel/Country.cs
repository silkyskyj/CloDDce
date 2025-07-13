// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC
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
using System.Linq;

namespace IL2DCE.MissionObjectModel
{
    public enum ECountry
    {
        nn,
        gb,
        de,
        fr,
        pl,
        ru,
        rz,
        us,
        hu,
        it,
        ja,
        fi,
        ro,
        sk,
        count,
    }

    public class Country
    {
        //public static readonly string[] Countrys = new string[]
        //    {
        //    "gb", "de", "fr", "pl", "ru", "rz", "us", "hu", "it", "ja", "fi",  "ro", "sk"
        //    };

        public static IEnumerable<string> ToStrings(bool exceptNeutral = true)
        {
            if (exceptNeutral)
            {
                return Enum.GetNames(typeof(ECountry)).Except(new string[] { "nn" });
            }
            else
            {
                return Enum.GetNames(typeof(ECountry));
            }
        }
    }
}
