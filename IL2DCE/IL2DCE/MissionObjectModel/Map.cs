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
using System.ComponentModel;
using System.Linq;
using maddox.GP;

namespace IL2DCE.MissionObjectModel
{
    public enum EMap
    {
        [Description("Land$English_Channel_1940")]                                      // 03/22 - 09/15
        English_Channel_1940_Summer,    // Land$English_Channel_1940_Summer

        [Description("Land$English_Channel_1940_Autumn")]                               // 09/16 - 11/20
        English_Channel_1940_Autumn,    // Land$English_Channel_1940_Autumn

        [Description("Land$English_Channel_1940_Winter")]                               // 11/20 - 03/21
        English_Channel_1940_Winter,    // Land$English_Channel_1940_Winter

        [Description("Land$Online_Carrier_War")]
        Online_Carrier_War,             // Land$Online_Carrier_War

        [Description("Land$Online_Cobra_(8x8)")]
        Online_Cobra,                   // Land$Online_Cobra_(8x8)

        [Description("Land$Online_Cross_v_Roundel")]
        Online_Cross_v_Roundel,         // Land$Online_Cross_v_Roundel

        [Description("Land$Online_Isles_of_Doom")]
        Online_Isles_of_Doom,           // Land$Online_Isles_of_Doom

        [Description("Land$Online_Map")]
        Online_Map,                     // Land$Online_Map

        [Description("Land$Online_Map2")]
        Online_Map2,                    // Land$Online_Map2

        [Description("Land$Online_Volcanic_Island")]
        Online_Volcanic_Island,         // Land$Online_Volcanic_Island

        [Description("tobruk:Land$Tobruk")]
        Tobruk,                         // tobruk:Land$Tobruk

        Count,
    }

    public class Map
    {
        private static Dictionary<string, IEnumerable<Point3d>> defaultFrontMarkers;
        private static Dictionary<string, IEnumerable<KeyValuePair<int, string>>> defaultMapPeriods;

        private static readonly Point3d[] frontMarkersEC1940 = new Point3d[]
        {
            new Point3d(126322.71f, 138413.07f, 1f),
            new Point3d(199567.26f, 173940.06f, 1f),
            new Point3d(260434.99f, 227480.47f, 1f),
            new Point3d(307684.35f, 300133.58f, 1f),
            new Point3d(317400.94f, 296042.89f, 2f),
            new Point3d(266603.90f, 221313.65f, 2f),
            new Point3d(213428.12f, 160032.81f, 2f),
            new Point3d(126322.71f, 132795.60f, 2f),
        };

        private static readonly KeyValuePair<int, string>[] defaultMapPeriodEC1940 = new KeyValuePair<int, string>[]
        {
            new KeyValuePair<int, string>(0322, "Land$English_Channel_1940"),
            new KeyValuePair<int, string>(0916, "Land$English_Channel_1940_Autumn"),
            new KeyValuePair<int, string>(1120, "Land$English_Channel_1940_Winter"),
        };

        static Map()
        {
            defaultFrontMarkers = new Dictionary<string, IEnumerable<Point3d>>
            {
                { EMap.English_Channel_1940_Summer.ToDescription(), frontMarkersEC1940 },
                { EMap.English_Channel_1940_Autumn.ToDescription(), frontMarkersEC1940 },
                { EMap.English_Channel_1940_Winter.ToDescription(), frontMarkersEC1940 }
            };

            defaultMapPeriods = new Dictionary<string, IEnumerable<KeyValuePair<int, string>>>
            {
                { EMap.English_Channel_1940_Summer.ToDescription(), defaultMapPeriodEC1940 },
                { EMap.English_Channel_1940_Autumn.ToDescription(), defaultMapPeriodEC1940 },
                { EMap.English_Channel_1940_Winter.ToDescription(), defaultMapPeriodEC1940 }
            };
        }

        public static IEnumerable<string> DefaultList()
        {
            List<string> list = new List<string>();
            for (EMap i = (EMap)0; i < EMap.Count; i++)
            {
                list.Add(i.ToDescription());
            }
            return list;
        }

        public static IEnumerable<Point3d> GetDefaultFrontMarkers(string mapName)
        {
            if (defaultFrontMarkers.ContainsKey(mapName))
            {
                return defaultFrontMarkers[mapName];
            }
            return null;
        }

        public static IEnumerable<KeyValuePair<int, string>> GetDefaultMapPeriod(string mapName)
        {
            if (defaultMapPeriods.ContainsKey(mapName))
            {
                return defaultMapPeriods[mapName];
            }
            return null;
        }
    }
}
