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

using System.Collections.Generic;
using System.Linq;
using maddox.game;

namespace IL2DCE.MissionObjectModel
{
    public class Waterway
    {
        public List<GroundGroupWaypoint> Waypoints
        {
            get
            {
                return _waypoints;
            }
        }
        private List<GroundGroupWaypoint> _waypoints = new List<GroundGroupWaypoint>();

        public GroundGroupWaypoint Start
        {
            get
            {
                return Waypoints.First();
            }
        }

        public GroundGroupWaypoint End
        {
            get
            {
                return Waypoints.Last();
            }
        }

        public Waterway(List<GroundGroupWaypoint> waypoints)
        {
            _waypoints = waypoints;
        }

        //public Waterway(ISectionFile sectionFile, string id)
        //{
        //    // Waypoints
        //    GroundGroupWaypoint lastWaypoint = null;
        //    for (int i = 0; i < sectionFile.lines(id + "_Road"); i++)
        //    {
        //        string key;
        //        string value;
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
        //            // Check if it's a subwaypoint or the last waypoint (which looks like a subwaypoint but is none).
        //            if (waypoint.IsSubWaypoint(sectionFile, id, i) && i < sectionFile.lines(id + "_Road") - 1)
        //            {
        //                if (lastWaypoint != null)
        //                {
        //                    lastWaypoint.SubWaypoints.Add(waypoint);
        //                }
        //                else
        //                {
        //                    throw new FormatException(string.Format("no last Waterway Waypoint[{0}]", id));
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

        public static Waterway Create(ISectionFile sectionFile, string id)
        {
            // Waypoints
            List<GroundGroupWaypoint> waypoints = new List<GroundGroupWaypoint>();
            GroundGroupWaypoint lastWaypoint = null;
            int lines = sectionFile.lines(id + "_Road");
            for (int i = 0; i < lines; i++)
            {
                string key;
                string value;
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
                            // throw new FormatException(string.Format("no last Waterway Waypoint[{0}]", id));
                            // Debug.Assert(false, string.Format("no last Waterway Waypoint[{0}]", id));
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
                return new Waterway(waypoints);
            }

            return null;
        }
    }
}