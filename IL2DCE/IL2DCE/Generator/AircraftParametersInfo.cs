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
using System.Linq;

namespace IL2DCE.Generator
{
    public class AircraftParametersInfo
    {
        public string LoadoutId
        {
            get
            {
                return this.loadoutId;
            }
        }
        private string loadoutId = "";

        public double? MinAltitude
        {
            get
            {
                return this.minAltitude;
            }
        }
        private double? minAltitude = null;

        public double? MaxAltitude
        {
            get
            {
                return this.maxAltitude;
            }
        }
        private double? maxAltitude = null;

        public AircraftParametersInfo(string valuePart)
        {
            string[] parameters = valuePart.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parameters != null && parameters.Length == 1)
            {
                this.loadoutId = parameters.First();
            }
            else if (parameters != null && parameters.Length >= 3)
            {
                this.loadoutId = parameters.First();
                minAltitude = double.Parse(parameters[1]);
                maxAltitude = double.Parse(parameters[2]);
            }
            else
            {
                throw new FormatException(string.Format("Invalid Aircraft Parameters Info[{0}]", valuePart));
            }
        }
    }
}