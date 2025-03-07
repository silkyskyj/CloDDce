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

using System;
using System.Globalization;
using System.Linq;

namespace IL2DCE.MissionObjectModel
{
    public class Skill
    {
        public const string SkillFormat = "F2";
        public const string SkillNameCustom = "Custom";
        public const string SkillNameMulti = "Different";
        public static readonly char[] SplitChar = new char[] { ' ', };

        public enum ESystemType
        {
            Rookie,
            Avarage,
            Veteran,
            Ace,
            Count,
        }

        public enum ESkilType
        {
            BasicFlying,
            AdvancedFlying,
            Awareness,
            AerialGunnnery,
            Tactics,
            Vision,
            Bravery,
            Discipline,
            Count,
        }

        public static readonly float[][] SystemSkillValue = new float[(int)ESystemType.Count][]
        {
            new float [] { 0.79f, 0.26f, 0.26f, 0.16f, 0.16f, 0.26f, 0.37f, 0.26f, },    // Rookie 
            new float [] { 0.84f, 0.53f, 0.53f, 0.37f, 0.37f, 0.53f, 0.53f, 0.53f, },    // Avarage
            new float [] { 1.00f, 0.74f, 0.84f, 0.63f, 0.84f, 0.84f, 0.74f, 0.74f, },    // Veteran
            new float [] { 1.00f, 0.95f, 0.95f, 0.84f, 0.84f, 0.95f, 0.89f, 0.89f, },    // Ace
        };

        public static readonly Skill[] SystemSkills = new Skill[(int)ESystemType.Count]
        {
             new Skill(ESystemType.Rookie),
             new Skill(ESystemType.Avarage),
             new Skill(ESystemType.Veteran),
             new Skill(ESystemType.Ace),
        };

        public static readonly Skill Default = new Skill();
        public static readonly Skill Rundom = new Skill();

        public float[] Skills
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public Skill()
        {
            Name = string.Empty;
            Skills = new float[(int)ESkilType.Count];
        }

        public Skill(ESystemType systemType)
        {
            Name = systemType.ToString();
            Skills = SystemSkillValue[(int)systemType];
        }

        public Skill(float[] skills, string name = null)
        {
            Name = name != null ? name: string.Empty;
            if (skills != null && skills.Length >= (int)ESkilType.Count)
            {
                Skills = skills;
            }
            else
            {
                throw new ArgumentException("Invalid Skill Value [null or Length]");
            }
        }

        public float GetSkill(ESkilType type)
        {
            return Skills[(int)type];
        }

        public void SetSkill(ESkilType type, float value)
        {
            if (value >= 0.0f && value <= 1.0f)
            {
                Skills[(int)type] = value;
            }
        }

        public bool IsSystemType()
        {
            return IsSystemType(Skills);
        }

        public string GetSystemTypeName()
        {
            return GetSystemTypeName(Skills);
        }

        public Skill GetSystemType()
        {
            if (Skills != null && Skills.Length == (int)ESkilType.Count)
            {
                return SystemSkills.Where(x => x.Skills.Except(Skills).Count() == 0).FirstOrDefault();
            }

            return null;
        }

        public override string ToString()
        {
            return string.Join(" ", Skills.Select(x => x.ToString(SkillFormat, Config.Culture)));
        }

        public static Skill GetSystemType(ESystemType skill)
        {
            return SystemSkills[(int)skill];
        }

        public static Skill GetSystemType(ESystemType? skill = null)
        {
            return skill != null ? GetSystemType(skill.Value) : GetSystemType(ESystemType.Avarage);
        }

        public static bool IsSystemType(float[] skills)
        {
            if (skills != null && skills.Length == (int)ESkilType.Count)
            {
                return SystemSkills.Any(x => x.Skills.Except(skills).Count() == 0);
            }

            return false;
        }

        public static string GetSystemTypeName(float[] skills)
        {

            Skill skill = GetSystemType(skills);
            return skill != null ? skill.Name : string.Empty;
        }

        public static Skill GetSystemType(float[] skills)
        {
            if (skills != null && skills.Length == (int)ESkilType.Count)
            {
                return SystemSkills.Where(x => x.Skills.Except(skills).Count() == 0).FirstOrDefault();
            }

            return null;
        }

        public static Skill Parse(string skillString)
        {
            Skill skill;
            if (TryParse(skillString, out skill))
            {
                return skill;
            }
            return null;
        }

        public static bool TryParse(string skillString, out Skill skill, IFormatProvider provider = null)
        {
            if (!string.IsNullOrEmpty(skillString))
            {
                string[] str = skillString.Split(SplitChar);
                if (str.Length >= (int)ESkilType.Count)
                {
                    if (provider == null)
                    {
                        provider = Config.Culture.NumberFormat;
                    }
                    float[] val = new float[(int)ESkilType.Count];
                    int i;
                    for (i = 0; i < (int)ESkilType.Count; i++)
                    {
                        float.TryParse(str[i], NumberStyles.Float, provider, out val[i]);
                    }
                    if (i == (int)ESkilType.Count)
                    {
                        skill = new Skill(val.Take((int)ESkilType.Count).ToArray());
                        return true;
                    }
                }
            }
            skill = null;
            return false;
        }
    }
}