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

using System.ComponentModel;

namespace IL2DCE.MissionObjectModel
{
    public enum EWeather
    {
        [Description("Default")]
        Default = -2,

        [Description("Random")]
        Random = -1,

        [Description("Clear")]
        Clear = 0,

        [Description("Light Clouds")]
        LightClouds = 1,

        [Description("Medium Clouds")]
        MediumClouds = 2,

        Count,
    }
}