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
using System.Diagnostics;
using System.Globalization;
using maddox.game;
using maddox.GP;

namespace IL2DCE.MissionObjectModel
{

    public class AirGroupWaypoint : GroupWaypoint
    {
        # region Public enums

        public enum AirGroupWaypointTypes
        {
            NORMFLY,
            TAKEOFF,
            LANDING,
            COVER,
            HUNTING,
            RECON,
            GATTACK_POINT,
            GATTACK_TARG,
            ESCORT,
            AATTACK_FIGHTERS,
            AATTACK_BOMBERS,
            FOLLOW,
        };

        #endregion

        public const int DefaultTakeoffZ = 0;
        public const int DefaultTakeoffV = 0;
        public const int DefaultNormaflyZ = 500;
        public const int DefaultNormaflyV = 300;
        public const int DefaultLandingZ = 0;
        public const int DefaultLandingV = 0;
        public const int DefaultFlyV = 300;

        #region Public properties

        public AirGroupWaypointTypes Type
        {
            get;
            set;
        }

        public Point3d Position
        {
            get
            {
                return new Point3d(X, Y, Z);
            }
        }

        // Altitude
        public double Z;

        // Speed
        public double V;

        public string Target
        {
            get;
            set;
        }

        #endregion

        #region Public constructors

        public AirGroupWaypoint(AirGroupWaypointTypes type, Point3d position, double v, string target = null)
        {
            Type = type;
            X = position.x;
            Y = position.y;
            Z = position.z;
            V = v;
            Target = target;
        }

        public AirGroupWaypoint(AirGroupWaypointTypes type, double x, double y, double z, double v, string target = null)
        {
            Type = type;
            X = x;
            Y = y;
            Z = z;
            V = v;
            Target = target;
        }

        public static AirGroupWaypoint Create(ISectionFile sectionFile, string id, int line)
        {
            string key;
            string value;
            sectionFile.get(id + "_Way", line, out key, out value);
            AirGroupWaypointTypes type;
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value) && Enum.TryParse(key, true, out type))
            {
                string[] valueList = value.Split(new char[] { ' ' });
                if (valueList.Length >= 4)
                {
                    double dX;
                    double dY;
                    double dZ;
                    double dV;
                    if (double.TryParse(valueList[0], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out dX) &&
                        double.TryParse(valueList[1], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out dY) &&
                        double.TryParse(valueList[2], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out dZ) &&
                        double.TryParse(valueList[3], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out dV))
                    {
                        return new AirGroupWaypoint(type, dX, dY, dZ, dV);
                    }
                    else
                    {
                        Debug.Assert(false, "Parse AirGroupWaypointType");
                    }
                }
            }

            return null;
        }

        #endregion
    }
}