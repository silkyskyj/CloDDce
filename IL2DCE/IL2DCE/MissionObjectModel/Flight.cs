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
using System.ComponentModel;

namespace IL2DCE.MissionObjectModel
{
    public enum EFlight
    {
        [Description("Mission Default")]
        MissionDefault = -2,

        [Description("Default")]
        Default = -1,
    }

    public class Flight
    {
        public const int SplitValue = 100;
        public const int MinCount = 1;
        public const int MaxCount = 4;
        public const int MinSize = 1;
        public const int MaxSize = 10;

        public static int Value(int count, int size)
        {
            return OptimizeCount(count) * SplitValue + OptimizeSize(size);
        }

        public static int Count(int value)
        {
            int count = value / SplitValue;
            return OptimizeCount(count);
        }

        public static int Size(int value)
        {
            int size = value % SplitValue;
            return OptimizeSize(size);
        }

        public static int OptimizeCount(int count)
        {
            if (count < MinCount)
            {
                count = MinCount;
            }
            else if (count > MaxCount)
            {
                count = MaxCount;
            }
            return count;
        }

        public static int OptimizeSize(int size)
        {
            if (size < MinSize)
            {
                size = MinSize;
            }
            else if (size > MaxSize)
            {
                size = MaxSize;
            }
            return size;
        }

        public static string CreateDisplayString(int value)
        {
            if (Enum.IsDefined(typeof(EFlight), value))
            {
                return ((EFlight)value).ToDescription();
            }
            else
            {
                return string.Format(Config.NumberFormat, "{0} x {1}", Count(value), Size(value));
            }
        }

        public static string CreateDisplayString(int count, int size)
        {
            return CreateDisplayString(Value(count, size));
        }

        public static void GetOptimaizeValue(out int count, out int size, EMissionType missionType, int countDefault, int sizeDefault, double countMultiplier, double sizeMultiplier, int targetCount = 0)
        {
            count = (int)Math.Ceiling(countDefault * countMultiplier);
            size = (int)Math.Ceiling(sizeDefault * sizeMultiplier);

            if (missionType == EMissionType.RECON || missionType == EMissionType.MARITIME_RECON)
            {
                count = 1;
                size = 1;
            }
            else if (missionType == EMissionType.ARMED_RECON || missionType == EMissionType.ARMED_MARITIME_RECON)
            {
                count = 1;
            }
            else if (missionType == EMissionType.ESCORT || missionType == EMissionType.FOLLOW || missionType == EMissionType.INTERCEPT || missionType == EMissionType.COVER)
            {
                if (targetCount > 0)
                {
                    if (count > targetCount)
                    {
                        count = targetCount;
                    }
                }
            }
        }

        public static void GetOptimaizeValue(out int count, out int size, int countDefault, int sizeDefault, double countMultiplier, double sizeMultiplier)
        {
            count = (int)Math.Ceiling(countDefault * countMultiplier);
            size = (int)Math.Ceiling(sizeDefault * sizeMultiplier);
        }
    }
}
