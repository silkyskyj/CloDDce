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
using System.Globalization;
using System.Text;
using maddox.game;

namespace IL2DCE.MissionObjectModel
{
    public enum EArmorUnitNumsSet
    {
        [Description("Random(1-8)")]
        Random = -2,
        
        [Description("Default")]
        Default = -1,
        
        [Description("1-3")]
        Range1_3,
        
        [Description("3-5")]
        Range3_5,
        
        [Description("5-8")]
        Range5_8,

        count,
    }

    public class ArmorOption : Option
    {
        public const string KeyNumUnits = "num_units";

        public const string RandomString = "Random";
        public const string DefaultString = "Default";

        public const int NumUnitsRandom = -2;
        public const int NumUnitsMissionDefault = -1;
        public const int MinNumUnits = 1;
        public const int StepNumUnits = 1;
        public const int MaxNumUnits = 8;

        public int NumUnits
        {
            get;
            set;
        }

        public ArmorOption(int numUnits, string others = null)
            : base (others)
        {
            NumUnits = numUnits;
        }

        public static ArmorOption Create(string options)
        {
            string[] vals = Parse(options);
            if (vals.Length >= 2)
            {
                int i = 0;
                int numUnits = MinNumUnits;
                StringBuilder sb = new StringBuilder();
                while (i + 1 < vals.Length)
                {
                    if (string.Compare(vals[i].Trim(), KeyNumUnits, true) == 0 && int.TryParse(vals[i + 1].Trim(), NumberStyles.Integer, Config.NumberFormat, out numUnits))
                    {
                    }
                    else
                    {
                        sb.AppendFormat(Config.NumberFormat, "/{0} {1}", vals[i].Trim(), vals[i + 1].Trim());
                    }
                    i++;
                }
                return new ArmorOption(numUnits, sb.ToString());
            }
            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (NumUnits > 1)
            {
                sb.AppendFormat(Config.NumberFormat, "/{0} {1}",
                                                            KeyNumUnits, NumUnits);
            }
            if (!string.IsNullOrEmpty(Others))
            {
                sb.Append(Others);
            }
            return sb.ToString();
        }

        public static int CreateRandomNumUnits(IRandom random)
        {
            return random.Next(MinNumUnits, MaxNumUnits + 1);
        }

        public static string CreateDisplayNumUnits(int numUnits)
        {
            if (numUnits == NumUnitsRandom)
            {
                return RandomString;
            }
            else if (numUnits == NumUnitsMissionDefault)
            {
                return DefaultString;
            }
            else if (numUnits >= MinNumUnits && numUnits <= MaxNumUnits)
            {
                return numUnits.ToString(Config.NumberFormat);
            }
            return string.Empty;
        }
    }

    internal class Armor : GroundGroup
    {
        public ArmorOption Option
        {
            get;
            set;
        }

        public override string Options
        {
            get
            {
                return Option != null ? Option.ToString(): string.Empty;
            }
            protected set
            {
                base.Options = value;
            }
        }

        public Armor(string id, string @class, int army, ECountry country, string options, ArmorOption armorOption, List<GroundGroupWaypoint> waypoints, string customChief = null, IEnumerable<string> customChiefValues = null)
            : base(id, @class, army, country, options, waypoints, customChief, customChiefValues)
        {
            Option = armorOption;
        }

        public override void WriteTo(ISectionFile sectionFile)
        {
            base.WriteTo(sectionFile);
        }

        public void SetNumUnits(int numUnits)
        {
            if (Option == null)
            {
                Option = new ArmorOption(numUnits);
            }
            else
            {
                Option.NumUnits = numUnits;
            }
        }
    }
}
