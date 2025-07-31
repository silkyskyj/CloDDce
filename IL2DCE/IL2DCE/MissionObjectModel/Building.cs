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

using System;
using System.Globalization;
using maddox.game;

namespace IL2DCE.MissionObjectModel
{
    internal class Building : GroundObject
    {
        public int Status
        {
            get;
            private set;
        }

        public Building(string id, string @class, int status, double x, double y, double direction)
            : base(id, @class, (int)EArmy.None, ECountry.nn, x, y, direction)
        {
            Status = status;
        }

        public static Building Create(ISectionFile sectionFile, string id)
        {
            string value = sectionFile.get(MissionFile.SectionBuildings, id);
            return Create(id, value);
        }

        public static Building Create(string id, string value)
        {
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(value))
            {
                string[] valueParts = value.Split(MissionFile.SplitChars, StringSplitOptions.RemoveEmptyEntries);
                if (valueParts.Length > 4)
                {
                    double x;
                    double y;
                    double direction;
                    int status;
                    if (int.TryParse(valueParts[1], NumberStyles.Integer, Config.NumberFormat, out status) &&
                        double.TryParse(valueParts[2], NumberStyles.Float, Config.NumberFormat, out x) &&
                        double.TryParse(valueParts[3], NumberStyles.Float, Config.NumberFormat, out y) &&
                        double.TryParse(valueParts[4], NumberStyles.Float, Config.NumberFormat, out direction))
                    {
                        return new Building(id, valueParts[0], status, x, y, direction);
                    }
                }
            }
            return null;
        }

        public void WriteTo(ISectionFile sectionFile)
        {
            string value = string.Format(Config.NumberFormat, "{0} {1} {2:F2} {3:F2} {4:F2}", Class, Status, X, Y, Direction);
            sectionFile.add(MissionFile.SectionBuildings, Id, value);
        }
    }
}