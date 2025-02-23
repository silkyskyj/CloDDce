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

using IL2DCE.MissionObjectModel;

namespace IL2DCE.Generator
{
    class GeneratorBriefing
    {
        private Generator Generator
        {
            get;
            set;
        }

        private Core Core
        {
            get;
            set;
        }

        public GeneratorBriefing(Core core, Generator generator)
        {
            Core = core;
            Generator = generator;
        }

        private IRandom Random
        {
            get
            {
                return Core.Random;
            }
        }

        public void CreateBriefing(BriefingFile briefingFile, AirGroup airGroup, EMissionType missionType, AirGroup escortAirGroup)
        {
            briefingFile.Name[airGroup.Id] = airGroup.Id;
            briefingFile.Description[airGroup.Id] = new BriefingFile.Text();

            briefingFile.Description[airGroup.Id].Sections.Add("descriptionSection", briefingFile.MissionDescription);

            string mainSection = missionType.ToString();
            briefingFile.Description[airGroup.Id].Sections.Add("mainSection", mainSection);

            if (airGroup.Altitude != null && airGroup.Altitude.HasValue)
            {
                string altitudeSection = "Altitude: " + airGroup.Altitude + "m";

                briefingFile.Description[airGroup.Id].Sections.Add("altitudeSection", altitudeSection);
            }

            if (airGroup.Waypoints.Count > 0)
            {
                string waypointSection = "Waypoints:\n";

                int i = 1;
                foreach (AirGroupWaypoint waypoint in airGroup.Waypoints)
                {
                    if (waypoint.Type == AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY)
                    {
                        waypointSection += i + ". " + Core.GamePlay.gpSectorName(waypoint.X, waypoint.Y) + "\n";
                    }
                    else
                    {
                        waypointSection += i + ". " + Core.GamePlay.gpSectorName(waypoint.X, waypoint.Y) + " " + waypoint.Type.ToString() + "\n";
                    }
                    i++;
                }

                briefingFile.Description[airGroup.Id].Sections.Add("waypointSection", waypointSection);
            }

            if (airGroup.EscortAirGroup != null)
            {
                briefingFile.Description[airGroup.Id].Sections.Add("escortSection", "Escorted by: " + escortAirGroup.Id);
            }

            if (airGroup.TargetAirGroup != null)
            {
                if (missionType == EMissionType.ESCORT)
                {
                    briefingFile.Description[airGroup.Id].Sections.Add("escortSection", "Escort: " + airGroup.TargetAirGroup.Id);
                }
            }
        }
    }
}