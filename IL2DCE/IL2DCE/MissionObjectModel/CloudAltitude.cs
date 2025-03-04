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

using System.Globalization;

namespace IL2DCE.MissionObjectModel
{
    public sealed class CloudAltitude
    {
        public const int Default = -2;
        public const int Random = -1;

        public const int Min = 500;
        public const int Max = 1500;
        public const int Step = 100;

        public static string CreateDisplayString(int alt)
        {
            return alt.ToString("#####", CultureInfo.InvariantCulture.NumberFormat);
        }
    }
}
