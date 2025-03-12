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

using System;
using System.Collections.Generic;
using System.Globalization;
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
            get
            {
                // Type
                if (Class.StartsWith("Vehicle"))
                {
                    return EGroundGroupType.Vehicle;
                }
                else if (Class.StartsWith("Armor"))
                {
                    return EGroundGroupType.Armor;
                }
                else if (Class.StartsWith("Ship"))
                {
                    return EGroundGroupType.Ship;
                }
                else if (Class.StartsWith("Train"))
                {
                    return EGroundGroupType.Train;
                }
                else
                {
                    return EGroundGroupType.Unknown;
                }
            }
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
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        private string _id;

        public string Class
        {
            get;
            set;
        }

        public ECountry Country
        {
            get;
            set;
        }

        public int Army
        {
            get
            {
                if (Country == ECountry.gb || Country == ECountry.fr || Country == ECountry.us || Country == ECountry.ru || Country == ECountry.rz || Country == ECountry.pl)
                {
                    return (int)EArmy.Red;
                }
                else if (Country == ECountry.de || Country == ECountry.it || Country == ECountry.ja || Country == ECountry.ro || Country == ECountry.fi || Country == ECountry.hu)
                {
                    return (int)EArmy.Blue;
                }
                else
                {
                    return (int)EArmy.None;
                }
            }
        }

        public string Options
        {
            get;
            set;
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

        public GroundGroup(string id, string @class, ECountry country, string options, List<GroundGroupWaypoint> waypoints)
        {
            _id = id;

            Class = @class;
            Country = country;
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

                    // Options
                    string options = value.Trim();

                    // Waypoints
                    List<GroundGroupWaypoint> waypoints = new List<GroundGroupWaypoint>();
                    GroundGroupWaypoint lastWaypoint = null;
                    int lines = sectionFile.lines(id + "_Road");
                    for (int i = 0; i < lines; i++)
                    {
                        string key;
                        sectionFile.get(id + "_Road", i, out key, out value);

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
                        return new GroundGroup(id, @class, country, options, waypoints);
                    }
                }

            }

            return null;
        }

        //public GroundGroup(ISectionFile sectionFile, string id)
        //{
        //    _id = id;

        //    string value = sectionFile.get("Chiefs", id);

        //     Class
        //    Class = value.Substring(0, value.IndexOf(" "));
        //    value = value.Remove(0, Class.Length + 1);

        //     Army
        //    ECountry country;
        //    if (Enum.TryParse(value.Substring(0, 2), true, out country))
        //    {
        //        Country = country;
        //        value = value.Remove(0, 2);
        //    }
        //    else
        //    {
        //        Debug.Assert(false, "Parse Country");
        //    }

        //     Options
        //    Options = value.Trim();

        //     Waypoints
        //    GroundGroupWaypoint lastWaypoint = null;
        //    for (int i = 0; i < sectionFile.lines(id + "_Road"); i++)
        //    {
        //        string key;
        //        sectionFile.get(id + "_Road", i, out key, out value);

        //        GroundGroupWaypoint waypoint = null;
        //        if (!key.Contains("S"))
        //        {
        //            waypoint = GroundGroupWaypointLine.Create(sectionFile, id, i);
        //        }
        //        else if (key.Contains("S"))
        //        {
        //            waypoint = GroundGroupWaypointSpline.Create(sectionFile, id, i);
        //        }

        //        if (waypoint != null)
        //        {
        //             Check if it's a subwaypoint or the last waypoint (which looks like a subwaypoint but is none).
        //            if (waypoint.IsSubWaypoint(sectionFile, id, i) && i < sectionFile.lines(id + "_Road") - 1)
        //            {
        //                if (lastWaypoint != null)
        //                {
        //                    lastWaypoint.SubWaypoints.Add(waypoint);
        //                }
        //                else
        //                {
        //                    throw new FormatException(string.Format("no GroundGroup sub Waypoint[{0}]", id));
        //                }
        //            }
        //            else
        //            {
        //                Waypoints.Add(waypoint);
        //                lastWaypoint = waypoint;
        //            }
        //        }
        //    }
        //}

        public void WriteTo(ISectionFile sectionFile)
        {
            if (Waypoints.Count > 1)
            {
                sectionFile.add("Chiefs", Id, Class + " " + Country.ToString() + " " + Options);
                // Write all waypoints except for the last one.
                for (int i = 0; i < Waypoints.Count - 1; i++)
                {
                    if (Waypoints[i] is GroundGroupWaypointLine)
                    {
                        if (Waypoints[i].V.HasValue)
                        {
                            sectionFile.add(Id + "_Road", Waypoints[i].X.ToString(CultureInfo.InvariantCulture.NumberFormat), Waypoints[i].Y.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + (Waypoints[i] as GroundGroupWaypointLine).Z.ToString(CultureInfo.InvariantCulture.NumberFormat) + "  0 " + (Waypoints[i].SubWaypoints.Count + 2).ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + Waypoints[i].V.Value.ToString(CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }
                    else if (Waypoints[i] is GroundGroupWaypointSpline)
                    {
                        if (Waypoints[i].V.HasValue)
                        {
                            sectionFile.add(Id + "_Road", "S", (Waypoints[i] as GroundGroupWaypointSpline).S + " P " + Waypoints[i].X.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + Waypoints[i].Y.ToString(CultureInfo.InvariantCulture.NumberFormat) + "  0 " + (Waypoints[i].SubWaypoints.Count + 2).ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + Waypoints[i].V.Value.ToString(CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }

                    foreach (GroundGroupWaypoint subWaypoint in Waypoints[i].SubWaypoints)
                    {
                        if (subWaypoint is GroundGroupWaypointLine)
                        {
                            sectionFile.add(Id + "_Road", subWaypoint.X.ToString(CultureInfo.InvariantCulture.NumberFormat), subWaypoint.Y.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + (subWaypoint as GroundGroupWaypointLine).Z.ToString(CultureInfo.InvariantCulture.NumberFormat));
                        }
                        else if (subWaypoint is GroundGroupWaypointSpline)
                        {
                            sectionFile.add(Id + "_Road", "S", (subWaypoint as GroundGroupWaypointSpline).S + " P " + subWaypoint.X.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + subWaypoint.Y.ToString(CultureInfo.InvariantCulture.NumberFormat));
                        }
                    }
                }

                // For the last waypoint don't write the subwaypoint count and the speed.
                if (Waypoints[Waypoints.Count - 1] is GroundGroupWaypointLine)
                {
                    sectionFile.add(Id + "_Road", Waypoints[Waypoints.Count - 1].X.ToString(CultureInfo.InvariantCulture.NumberFormat), Waypoints[Waypoints.Count - 1].Y.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + (Waypoints[Waypoints.Count - 1] as GroundGroupWaypointLine).Z.ToString(CultureInfo.InvariantCulture.NumberFormat));
                }
                else if (Waypoints[Waypoints.Count - 1] is GroundGroupWaypointSpline)
                {
                    sectionFile.add(Id + "_Road", "S", (Waypoints[Waypoints.Count - 1] as GroundGroupWaypointSpline).S + " P " + Waypoints[Waypoints.Count - 1].X.ToString(CultureInfo.InvariantCulture.NumberFormat) + " " + Waypoints[Waypoints.Count - 1].Y.ToString(CultureInfo.InvariantCulture.NumberFormat));
                }
            }
        }
     }
}