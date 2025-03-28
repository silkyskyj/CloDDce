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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using maddox.game;
using maddox.GP;

namespace IL2DCE.MissionObjectModel
{
    public enum EGroundGroupType
    {
        Vehicle,
        Armor,
        Ship,
        Train,
        Unknown,
    }

    public class GroundGroup
    {
        public EGroundGroupType Type
        {
            get;
            private set;
        }

        public Point2d Position
        {
            get
            {
                return new Point2d(Waypoints[0].X, Waypoints[0].Y);
            }
        }

        public string Id
        {
            get;
            set;
        }

        public string Class
        {
            get;
            private set;
        }

        public ECountry Country
        {
            get;
            private set;
        }

        public int Army
        {
            get;
            private set;
        }

        public string Options
        {
            get;
            private set;
        }

        public List<GroundGroupWaypoint> Waypoints
        {
            get
            {
                return _waypoints;
            }
        }
        private List<GroundGroupWaypoint> _waypoints = new List<GroundGroupWaypoint>();

        public string DisplayName
        {
            get
            {
                return Class.Replace(".", " ");
            }
        }

        public GroundGroup(string id, string @class, int army, ECountry country, string options, List<GroundGroupWaypoint> waypoints)
        {
            Id = id;
            Class = @class;
            Type = ParseType(Class);
            Country = country;
            Army = army;
            Options = options;
            Waypoints.AddRange(waypoints);
        }

        public static GroundGroup Create(ISectionFile sectionFile, string id)
        {
            // _id = id;

            string value = sectionFile.get("Chiefs", id, string.Empty);
            if (!string.IsNullOrEmpty(value))
            {
                // Class
                string @class = value.Substring(0, value.IndexOf(" "));
                value = value.Remove(0, @class.Length + 1);

                // Army
                ECountry country;
                if (Enum.TryParse(value.Substring(0, 2), true, out country))
                {
                    // Country = country;
                    value = value.Remove(0, 2);

                    EArmy army = MissionObjectModel.Army.Parse(country);

                    // Options
                    string options = value.Trim();

                    // Waypoints
                    List<GroundGroupWaypoint> waypoints = new List<GroundGroupWaypoint>();
                    GroundGroupWaypoint lastWaypoint = null;
                    string section = string.Format("{0}_{1}", id, MissionFile.SectionRoad);
                    int lines = sectionFile.lines(section);
                    for (int i = 0; i < lines; i++)
                    {
                        string key;
                        sectionFile.get(section, i, out key, out value);

                        GroundGroupWaypoint waypoint = null;
                        if (!key.Contains("S"))
                        {
                            waypoint = GroundGroupWaypointLine.Create(sectionFile, id, i);
                        }
                        else if (key.Contains("S"))
                        {
                            waypoint = GroundGroupWaypointSpline.Create(sectionFile, id, i);
                        }

                        if (waypoint != null)
                        {
                            // Check if it's a subwaypoint or the last waypoint (which looks like a subwaypoint but is none).
                            if (waypoint.IsSubWaypoint(sectionFile, id, i) && i < lines - 1)
                            {
                                if (lastWaypoint != null)
                                {
                                    lastWaypoint.SubWaypoints.Add(waypoint);
                                }
                                else
                                {
                                    // Debug.Assert(false, string.Format("no GroundGroup sub Waypoint[{0}]", id));
                                    // throw new FormatException(string.Format("no GroundGroup sub Waypoint[{0}]", id));
                                }
                            }
                            else
                            {
                                waypoints.Add(waypoint);
                                lastWaypoint = waypoint;
                            }
                        }
                    }

                    if (waypoints.Count > 0)
                    {
                        return new GroundGroup(id, @class, (int)army, country, options, waypoints);
                    }
                }
            }

            return null;
        }

        public static EGroundGroupType ParseType(string classString)
        {
            if (classString.StartsWith("Vehicle"))
            {
                return EGroundGroupType.Vehicle;
            }
            else if (classString.StartsWith("Armor"))
            {
                return EGroundGroupType.Armor;
            }
            else if (classString.StartsWith("Ship"))
            {
                return EGroundGroupType.Ship;
            }
            else if (classString.StartsWith("Train"))
            {
                return EGroundGroupType.Train;
            }
            return EGroundGroupType.Unknown;
        }

        public void UpdateArmy(int army)
        {
            Debug.WriteLine("GroundGroup.UpdateArmy({0} -> {1})", Army, army);
            Army = army;
        }

        public void WriteTo(ISectionFile sectionFile)
        {
            if (Waypoints.Count > 1)
            {
                sectionFile.add(MissionFile.SectionChiefs, Id, string.Format("{0} {1} {2}", Class, Country.ToString(), Options));
                // Write all waypoints except for the last one.
                string section = string.Format("{0}_{1}", Id, MissionFile.SectionRoad);
                for (int i = 0; i < Waypoints.Count - 1; i++)
                {
                    if (Waypoints[i] is GroundGroupWaypointLine)
                    {
                        if (Waypoints[i].V.HasValue)
                        {
                            sectionFile.add(section, Waypoints[i].X.ToString("F2", CultureInfo.InvariantCulture.NumberFormat),
                                            string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:F2} {1:F2}  0 {2} {3:F2}", 
                                                            Waypoints[i].Y, 
                                                            (Waypoints[i] as GroundGroupWaypointLine).Z, 
                                                            (Waypoints[i].SubWaypoints.Count + 2), 
                                                            Waypoints[i].V.Value));
                        }
                    }
                    else if (Waypoints[i] is GroundGroupWaypointSpline)
                    {
                        if (Waypoints[i].V.HasValue)
                        {
                            sectionFile.add(section, "S", 
                                            string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0} P {1:F2} {2:F2}  0 {3} {4:F2}", 
                                                            (Waypoints[i] as GroundGroupWaypointSpline).S, 
                                                            Waypoints[i].X, 
                                                            Waypoints[i].Y, 
                                                            (Waypoints[i].SubWaypoints.Count + 2), 
                                                            Waypoints[i].V.Value));
                        }
                    }

                    foreach (GroundGroupWaypoint subWaypoint in Waypoints[i].SubWaypoints)
                    {
                        if (subWaypoint is GroundGroupWaypointLine)
                        {
                            sectionFile.add(section, subWaypoint.X.ToString("F2", CultureInfo.InvariantCulture.NumberFormat),
                                string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:F2} {1:F2}", subWaypoint.Y, (subWaypoint as GroundGroupWaypointLine).Z));
                        }
                        else if (subWaypoint is GroundGroupWaypointSpline)
                        {
                            sectionFile.add(section, "S",
                                string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0} P {1:F2} {2:F2}", 
                                (subWaypoint as GroundGroupWaypointSpline).S, subWaypoint.X, subWaypoint.Y));
                        }
                    }
                }

                // For the last waypoint don't write the subwaypoint count and the speed.
                GroundGroupWaypoint wayPointLast = Waypoints.Last();
                if (wayPointLast is GroundGroupWaypointLine)
                {
                    sectionFile.add(section, wayPointLast.X.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                    string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0:F2} {1:F2}", 
                                                            wayPointLast.Y, (wayPointLast as GroundGroupWaypointLine).Z));
                }
                else if (wayPointLast is GroundGroupWaypointSpline)
                {
                    sectionFile.add(section, "S", 
                                    string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0} P {1:F2} {2:F2}", 
                                                            (wayPointLast as GroundGroupWaypointSpline).S, wayPointLast.X,  wayPointLast.Y));
                }
            }
        }
     }
}