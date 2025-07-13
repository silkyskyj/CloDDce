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

using System.Globalization;
using System.Linq;
using System.Text;
using maddox.game;

namespace IL2DCE.MissionObjectModel
{
    public class ArtilleryOption : Option
    {
        public const string KeyTimeout = "timeout";
        public const string KeyRHide = "radius_hide";
        public const string KeyZOffset = "hstart";

        public const string RandomString = "Random";
        public const string DefaultString = "Default";

        public const int TimeoutRandom = -2;
        public const int TimeoutMissionDefault = -1;
        public const int MinTimeout = 0;    // 0:00
        public const int MaxTimeout = 720;  // 12:00 (12:59?)

        public const int RHideRandom = -2;
        public const int RHideMissionDefault = -1;
        public const int MinRHide = 0;
        public const int MaxRHide = 15000;
        public const int StepRHide = 500;   //  0:05

        public const int ZOffsetRandom = -10002;
        public const int ZOffsetMissionDefault = -10001;
        public const int MinZOffset = -1000;
        public const int MaxZOffset = 2000;
        public const int StepZOffset = 500;

        public int Timeout   // Time-Out [min]
        {
            get;
            set;
        }

        public int RHide   // radius_hide [m]
        {
            get;
            set;
        }

        public int? ZOffset   // hstart [0]
        {
            get;
            set;
        }

        public ArtilleryOption(int timeout, int rHide, int? zOffset = null, string others = null)
            : base(others)
        {
            Timeout = timeout;
            RHide = rHide;
            ZOffset = zOffset;
        }

        public static ArtilleryOption Create(string options)
        {
            string[] vals = Parse(options);
            if (vals.Length >= 4)
            {
                if (string.Compare(vals[0].Trim(), KeyTimeout, true) == 0 && string.Compare(vals[2].Trim(), KeyRHide, true) == 0)
                {
                    int timeout;
                    int rHide;
                    if (int.TryParse(vals[1].Trim(), NumberStyles.Integer, Config.NumberFormat, out timeout) &&
                        int.TryParse(vals[3].Trim(), NumberStyles.Integer, Config.NumberFormat, out rHide))
                    {
                        int zOffset;
                        if (vals.Length >= 6 && string.Compare(vals[4].Trim(), KeyZOffset, true) == 0 && int.TryParse(vals[5].Trim(), NumberStyles.Integer, Config.NumberFormat, out zOffset))
                        {
                            return new ArtilleryOption(timeout, rHide, zOffset, ToString(vals.Skip(6)));
                        }
                        else
                        {
                            return new ArtilleryOption(timeout, rHide, null, ToString(vals.Skip(4)));
                        }
                    }
                }
            }

            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (ZOffset == null)
            {
                sb.AppendFormat(Config.NumberFormat, "/{0} {1}/{2} {3}",
                                                            KeyTimeout, Timeout, KeyRHide, RHide);
            }
            else
            {
                sb.AppendFormat(Config.NumberFormat, "/{0} {1}/{2} {3}/{4} {5}",
                                                            KeyTimeout, Timeout, KeyRHide, RHide, KeyZOffset, ZOffset.Value);
            }
            if (!string.IsNullOrEmpty(Others))
            {
                sb.Append(Others);
            }
            return sb.ToString();
        }

        public static int CreateRandomTimeout(IRandom random)
        {
            return random.Next(MinTimeout, MaxTimeout + 1);
        }

        public static int CreateRandomRHide(IRandom random)
        {
            return random.Next(MinRHide, MaxRHide + 1);
        }

        public static int CreateRandomZOffset(IRandom random)
        {
            return random.Next(MinZOffset, MaxZOffset + 1);
        }

        public static string CreateDisplayStringTimeout(int timeout)
        {
            if (timeout == TimeoutRandom)
            {
                return RandomString;
            }
            else if (timeout == TimeoutMissionDefault)
            {
                return DefaultString;
            }
            else if (timeout >= MinTimeout && timeout <= MaxTimeout)
            {
                return MissionTime.ToString(timeout / 60.0);
            }
            return string.Empty;
        }

        public static string CreateDisplayStringRHide(int rHide)
        {
            if (rHide == RHideRandom)
            {
                return RandomString;
            }
            else if (rHide == RHideMissionDefault)
            {
                return DefaultString;
            }
            else if (rHide >= MinRHide && rHide <= MaxRHide)
            {
                return rHide.ToString(Config.NumberFormat);
            }
            return string.Empty;
        }

        public static string CreateDisplayStringZOffsete(int zOffset)
        {
            if (zOffset == ZOffsetRandom)
            {
                return RandomString;
            }
            else if (zOffset == ZOffsetMissionDefault)
            {
                return DefaultString;
            }
            else if (zOffset >= MinZOffset && zOffset <= MaxZOffset)
            {
                return zOffset.ToString(Config.NumberFormat);
            }
            return string.Empty;
        }
    }

    internal class Artillery : Stationary
    {
        public ArtilleryOption Option
        {
            get;
            set;
        }

        public override string Options
        {
            get
            {
                return Option != null ? Option.ToString() : string.Empty;
            }
            protected set
            {
                base.Options = value;
            }
        }

        public Artillery(string id, string @class, int army, ECountry country, double x, double y, double direction, string options, ArtilleryOption option)
            : base(id, @class, army, country, x, y, direction, options)
        {
            Option = option;
        }

        public override void WriteTo(ISectionFile sectionFile)
        {
            base.WriteTo(sectionFile);
        }
    }
}
