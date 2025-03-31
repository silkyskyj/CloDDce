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

using System.Globalization;
using System.Text;
using IL2DCE.MissionObjectModel;
using maddox.game;

namespace IL2DCE.Generator
{
    class GeneratorBriefing
    {
        private IGamePlay GamePlay
        {
            get;
            set;
        }

        public GeneratorBriefing(IGamePlay gamePlay)
        {
            GamePlay = gamePlay;
        }

        public void CreateBriefing(BriefingFile briefingFile, AirGroup airGroup, EMissionType missionType, AirGroup escortAirGroup)
        {
            briefingFile.Name[airGroup.Id] = airGroup.Id;
            briefingFile.Description[airGroup.Id] = new BriefingFile.Text();

            briefingFile.Description[airGroup.Id].Sections.Add("descriptionSection", string.Format("{0}\n\n{1}", airGroup.DisplayDetailName, briefingFile.MissionDescription));

            string mainSection = missionType.ToString();
            if (airGroup.TargetAirGroup != null && (missionType == EMissionType.ESCORT || missionType == EMissionType.FOLLOW))
            {
                mainSection += string.Format("\n  [ {0} ]", airGroup.TargetAirGroup.DisplayDetailName);
            }
            else if ((airGroup.TargetGroundGroup != null || airGroup.TargetStationary != null) && missionType == EMissionType.COVER)
            {
                mainSection += string.Format("\n  [ {0} ]", 
                    airGroup.TargetGroundGroup != null ? airGroup.TargetGroundGroup.DisplayName: airGroup.TargetStationary != null ? airGroup.TargetStationary.DisplayName: string.Empty);
            }
            briefingFile.Description[airGroup.Id].Sections.Add("mainSection", mainSection);

            if (airGroup.Altitude != null && airGroup.Altitude.HasValue)
            {
                string altitudeSection = "Altitude: " + airGroup.Altitude + "m";

                briefingFile.Description[airGroup.Id].Sections.Add("altitudeSection", altitudeSection);
            }

            if (airGroup.Waypoints.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Waypoints:");

                int i = 1;
                foreach (AirGroupWaypoint waypoint in airGroup.Waypoints)
                {
                    if (waypoint.Type == AirGroupWaypoint.AirGroupWaypointTypes.NORMFLY)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, "{0,2}. {1,-5}", i, GamePlay.gpSectorName(waypoint.X, waypoint.Y));
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture.NumberFormat, "{0,2}. {1,-5} {2}", i, GamePlay.gpSectorName(waypoint.X, waypoint.Y), waypoint.Type.ToString());
                        sb.AppendLine();
                    }
                    i++;
                }
                briefingFile.Description[airGroup.Id].Sections.Add("waypointSection", sb.ToString().TrimEnd("\n".ToCharArray()));
            }

            if (airGroup.EscortAirGroup != null)
            {
                briefingFile.Description[airGroup.Id].Sections.Add("escortSection", "Escorted by: " + escortAirGroup.DisplayDetailName);
            }
        }
    }
}