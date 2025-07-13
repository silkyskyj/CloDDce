// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC
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
    public class Groundway
    {
        public string Id
        {
            get;
            protected set;
        }

        public List<GroundGroupWaypoint> Waypoints
        {
            get;
            private set;
        }

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

        public Groundway(string id, List<GroundGroupWaypoint> waypoints)
        {
            Id = id;
            Waypoints = waypoints;
        }

        public static Groundway Create(ISectionFile sectionFile, string id)
        {
            // Waypoints
            List<GroundGroupWaypoint> waypoints = new List<GroundGroupWaypoint>();
            GroundGroupWaypoint lastWaypoint = null;
            string section = string.Format("{0}_{1}", id, MissionFile.SectionRoad);
            int lines = sectionFile.lines(section);
            for (int i = 0; i < lines; i++)
            {
                string key;
                string value;
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
                            // throw new FormatException(string.Format("no last Groundway Waypoint[{0}]", id));
                            // Debug.Assert(false, string.Format("no last Groundway Waypoint[{0}]", id));
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
                return new Groundway(id, waypoints);
            }

            return null;
        }
    }
}