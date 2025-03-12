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
using System.Text.RegularExpressions;
using maddox.game;

namespace IL2DCE.MissionObjectModel
{
    public class GroundGroupWaypointSpline : GroundGroupWaypoint
    {

        #region Public properties

        public string S
        {
            get;
            private set;
        }

        #endregion

        // Example: S 503 91 0.61 5.00 P 360207.22 223055.25  0 2 11.11
        // S ? ? ? ? P X Y  0 SubCount V
        private static readonly Regex waypointLong = new Regex(@"^([0-9]+) ([0-9]+) ([0-9]+[.0-9]*) ([-+]?[0-9]+[.0-9]*) P ([0-9]+[.0-9]*) ([0-9]+[.0-9]*)  ([0-9]+) ([0-9]+) ([0-9]+[.0-9]*)$");
        // Example: S 503 79 0.00 -1330.00 P 354764.81 223866.00
        // S ? ? ? ? P X Y
        private static readonly Regex waypointShort = new Regex(@"^([0-9]+) ([0-9]+) ([0-9]+[.0-9]*) ([-+]?[0-9]+[.0-9]*) P ([0-9]+[.0-9]*) ([0-9]+[.0-9]*)$");

        #region Public constructors

        public GroundGroupWaypointSpline(double x, double y, double? v, string s)
        {
            X = x;
            Y = y;
            V = v;
            S = s;
        }

        #endregion

        public static GroundGroupWaypointSpline Create(ISectionFile sectionFile, string id, int line)
        {
            string key;
            string value;
            sectionFile.get(id + "_Road", line, out key, out value);
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                if (waypointLong.IsMatch(value))
                {
                    Match match = waypointLong.Match(value);
                    if (match.Groups.Count == 10)
                    {
                        double x;
                        double y;
                        double v;
                        if (double.TryParse(match.Groups[5].Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x) && 
                            double.TryParse(match.Groups[6].Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y) && 
                            double.TryParse(match.Groups[9].Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out v))
                        {
                            return new GroundGroupWaypointSpline(x, y, v, match.Groups[1].Value + " " + match.Groups[2].Value + " " + match.Groups[3].Value + " " + match.Groups[4].Value); 
                        }
                    }
                }
                else if (waypointShort.IsMatch(value))
                {
                    Match match = waypointShort.Match(value);
                    if (match.Groups.Count == 7)
                    {
                        double x;
                        double y;
                        if (double.TryParse(match.Groups[5].Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x) &&
                            double.TryParse(match.Groups[6].Value, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y))
                        {
                            return new GroundGroupWaypointSpline(x, y, null, match.Groups[1].Value + " " + match.Groups[2].Value + " " + match.Groups[3].Value + " " + match.Groups[4].Value);
                        }
                    }
                }
            }
            return null;
        }

        public override bool IsSubWaypoint(ISectionFile sectionFile, string id, int line)
        {
            string key;
            string value;
            sectionFile.get(id + "_Road", line, out key, out value);

            if (waypointShort.IsMatch(value) && !waypointLong.IsMatch(value))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}