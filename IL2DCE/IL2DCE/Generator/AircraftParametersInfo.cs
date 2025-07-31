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
using System.Linq;
using IL2DCE.MissionObjectModel;

namespace IL2DCE.Generator
{
    public class AircraftParametersInfo
    {
        public const string DefaulLoadoutId = "Default";
        public const int DefaultMinAltitude = 0;
        public const int DefaultMaxAltitude = 5000;

        public string LoadoutId
        {
            get
            {
                return this.loadoutId;
            }
        }
        private string loadoutId = string.Empty;

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

        public AircraftParametersInfo(string loadoutId, double? minAltitude = null, double? maxAltitude = null)
        {
            this.loadoutId = loadoutId;
            this.minAltitude = minAltitude;
            this.maxAltitude = maxAltitude;
        }

        public AircraftParametersInfo(string valuePart)
        {
            AircraftParametersInfo aircraftParametersInfo = Create(valuePart, true);
            loadoutId = aircraftParametersInfo.loadoutId;
            minAltitude = aircraftParametersInfo.minAltitude;
            maxAltitude = aircraftParametersInfo.maxAltitude;
        }

        public static AircraftParametersInfo Create(string valuePart, bool throwException = false)
        {
            string[] parameters = valuePart.Split(MissionFile.SplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (parameters != null && parameters.Length == 1)
            {
                return new AircraftParametersInfo(parameters.First());
            }
            else if (parameters != null && parameters.Length >= 3)
            {
                double minAltitude;
                double maxAltitude;
                if (double.TryParse(parameters[1], out minAltitude) && double.TryParse(parameters[2], out maxAltitude))
                {
                    return new AircraftParametersInfo(parameters.First(), minAltitude, maxAltitude);
                }
            }

            if (throwException)
            {
                throw new FormatException(string.Format("Invalid Aircraft Parameters Info[{0}]", valuePart));
            }

            return null;
        }
    }
}