// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC
// Copyright (C) 2016 Stefan Rothdach & 2025 silkysky
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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using maddox.game;
using static IL2DCE.MissionObjectModel.Skill;

namespace IL2DCE.MissionObjectModel
{
    public enum EShipUnitNumsSet
    {
        [Description("Random(1-5)")]
        Random = -2,

        [Description("Default")]
        Default = -1,

        [Description("1")]
        Range1,

        [Description("1-3")]
        Range1_3,

        [Description("3-5")]
        Range3_5,

        count,
    }

    public class ShipOption : Option
    {
        public const string KeySleep = "sleep";
        public const string KeySkill = "skill";
        public const string KeySlowfire = "slowfire";

        public const string RandomString = "Random";
        public const string DefaultString = "Default";

        public const int SleepRandom = -2;
        public const int SleepMissionDefault = -1;
        public const int MinSleep = 0;
        public const int StepSleep = 5;
        public const int MaxSleep = 5939;

        public const float SlowFireRandom = -2;
        public const float SlowFireMissionDefault = -1;
        public const float MinSlowFire = 0.5f;
        public const float StepSlowFire = 0.5f;
        public const float MaxSlowFire = 100.0f;

        public int Sleep
        {
            get;
            set;
        }

        public ESystemType Skill
        {
            get;
            set;
        }

        public float Slowfire
        {
            get;
            set;
        }

        public ShipOption(int sleep, ESystemType skill, float slowfire, string others = null)
            : base(others)
        {
            Sleep = sleep;
            Skill = skill;
            Slowfire = slowfire;
        }

        public static ShipOption Create(string options)
        {
            string[] vals = Parse(options);
            if (vals.Length >= 6)
            {
                if (string.Compare(vals[0].Trim(), KeySleep, true) == 0 && string.Compare(vals[2].Trim(), KeySkill, true) == 0 && string.Compare(vals[4].Trim(), KeySlowfire, true) == 0)
                {
                    int sleep;
                    ESystemType skill;
                    float slowfire;
                    if (int.TryParse(vals[1].Trim(), NumberStyles.Integer, Config.NumberFormat, out sleep) &&
                        Enum.TryParse(vals[3].Trim(), true, out skill) &&
                        float.TryParse(vals[5].Trim(), NumberStyles.Float, Config.NumberFormat, out slowfire))
                    {
                        return new ShipOption(sleep, skill, slowfire, ToString(vals.Skip(6)));
                    }
                }
            }

            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Config.NumberFormat, "/{0} {1}/{2} {3}/{4} {5}",
                                                        KeySleep, Sleep, KeySkill, (int)Skill, KeySlowfire, Slowfire < 1 ? "0.5" : Slowfire.ToString("F0", Config.NumberFormat));
            if (!string.IsNullOrEmpty(Others))
            {
                sb.Append(Others);
            }
            return sb.ToString();
        }

        public static int CreateRandomSleep(IRandom random)
        {
            return random.Next(MinSleep, MaxSleep + 1);
        }

        public static float CreateRandomSlowfire(IRandom random)
        {
            return random.Next((int)(MinSlowFire * 10), (int)((MaxSlowFire + 1) * 10)) / 10.0f;
        }

        public static string CreateDisplayStringSleep(int sleep)
        {
            if (sleep == SleepRandom)
            {
                return RandomString;
            }
            else if (sleep == SleepMissionDefault)
            {
                return DefaultString;
            }
            else if (sleep >= MinSleep && sleep <= MaxSleep)
            {
                return MissionTime.ToString(sleep / 60.0);
            }
            return string.Empty;
        }

        public static string CreateDisplayStringSlowfire(float slowfire)
        {
            if (slowfire == SlowFireRandom)
            {
                return RandomString;
            }
            else if (slowfire == SlowFireMissionDefault)
            {
                return DefaultString;
            }
            else if (slowfire >= MinSlowFire && slowfire <= MaxSlowFire)
            {
                return slowfire.ToString(Config.NumberFormat);
            }
            return string.Empty;
        }
    }

    internal class ShipGroup : GroundGroup
    {
        public ShipOption Option
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

        public ShipGroup(string id, string @class, int army, ECountry country, string options, ShipOption shipOption, List<GroundGroupWaypoint> waypoints, string customChief = null, IEnumerable<string> customChiefValues = null)
            : base(id, @class, army, country, options, waypoints, customChief, customChiefValues)
        {
            Option = shipOption;
        }

        public override void WriteTo(ISectionFile sectionFile)
        {
            base.WriteTo(sectionFile);
        }
    }

    internal class ShipUnit : Stationary
    {
        public ShipOption Option
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

        public ShipUnit(string id, string @class, int army, ECountry country, double x, double y, double direction, string options, ShipOption shipOption)
            : base(id, @class, army, country, x, y, direction, options)
        {
            Option = shipOption;
        }

        public override void WriteTo(ISectionFile sectionFile)
        {
            base.WriteTo(sectionFile);
        }
    }
}