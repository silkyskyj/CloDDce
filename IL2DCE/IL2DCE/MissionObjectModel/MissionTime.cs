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

namespace IL2DCE.MissionObjectModel
{
    public sealed class MissionTime
    {
        public const double Default = -2;
        public const double Random = -1;

        public const double Begin = 5.0;
        public const double End = 21.0;

        public static string ToString(double d)
        {
            return string.Format("{0:D2}:{1:D2}", (int)d, (((int)(d * 100)) % 100) * 60 / 100);
        }
    }
}
