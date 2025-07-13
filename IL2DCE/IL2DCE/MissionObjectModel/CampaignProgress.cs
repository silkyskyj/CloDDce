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

using System.ComponentModel;

namespace IL2DCE.MissionObjectModel
{

    public enum ECampaignProgress
    {
        [Description("Daily")]
        Daily,          // 1Day

        [Description("Any Day (1 - 5days)")]
        AnyDay,         // 1 - 5days

        [Description("Any Time (3 - 16hours)")]
        AnyTime,        // 3 - 16hours(0 - 1Day)

        [Description("Any Day Any Time")]
        AnyDayAnyTime,  // 3 - 136

        Count,
    }

    public class CampaignProgress
    {
        public const int DailyDay = 1;
        public const int AnyDayBebin = 1;
        public const int AnyDayEnd = 5;
        public const int AnyTimeBebin = 3;
        public const int AnyTimeEnd = 16;
        public const int AnyDayAnyTimeBebin = AnyTimeBebin;
        public const int AnyDayAnyTimeEnd = AnyDayEnd * 24 + AnyTimeEnd;
    }
}
