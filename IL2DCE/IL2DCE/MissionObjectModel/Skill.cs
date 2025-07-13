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
using static IL2DCE.MissionObjectModel.Skill;

namespace IL2DCE.MissionObjectModel
{
    public class Skill
    {
        public const string SkillFormat = "F2";
        public const string SkillNameCustom = "Custom";
        public const string SkillNameMulti = "Different";
        public const string SkillNameRandom = "[Random] Tweaked Rookie - Ace";
        public const string SkillNameDefault = "Mission Default";
        public const string SkillNameRandomShip = "[Random]";
        public const string SkillNameDefaultShip = "Default";

        public static readonly char[] SplitChar = new char[] { ' ', };

        public enum ESystemType
        {
            Rookie,
            Avarage,
            Veteran,
            Ace,
            Count,
        }

        public enum ETweakedAircraftType
        {
            Fighter,
            Bomber,
            FighterBomber,
            count,
        }

        public enum ETweakedType
        {
            FighterRookie,
            FighterAvarage,
            FighterExperienced,
            FighterVeteran,
            FighterAce,
            BomberRookie,
            BomberAvarage,
            BomberExperienced,
            BomberVeteran,
            BomberAce,
            FighterBomberRookie,
            FighterBomberAvarage,
            FighterBomberExperienced,
            FighterBomberVeteran,
            FighterBomberAce,
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

        public static readonly float[] DefaultValue = new float[] { -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, -2.0f, };
        public static readonly float[] RandomValue = new float[] { -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, };

        public static readonly float[][] SystemSkillValue = new float[(int)ESystemType.Count][]
        {
            new float [] { 0.79f, 0.26f, 0.26f, 0.16f, 0.16f, 0.26f, 0.37f, 0.26f, },    // Rookie 
            new float [] { 0.84f, 0.53f, 0.53f, 0.37f, 0.37f, 0.53f, 0.53f, 0.53f, },    // Avarage
            new float [] { 1.00f, 0.74f, 0.84f, 0.63f, 0.84f, 0.84f, 0.74f, 0.74f, },    // Veteran
            new float [] { 1.00f, 0.95f, 0.95f, 0.84f, 0.84f, 0.95f, 0.89f, 0.89f, },    // Ace
        };

        /// <summary>
        /// Tweaked AI settings http://bobgamehub.blogspot.de/2012/03/ai-settings-in-cliffs-of-dover.html
        /// </summary>
        public static readonly float[][] TweakedSkillValue = new float[(int)ETweakedType.Count][]
        {
            new float [] { 0.30f, 0.11f, 0.78f, 0.40f, 0.64f, 0.85f, 0.85f, 0.21f, },    // Rookie      (Fighter)
            new float [] { 0.32f, 0.12f, 0.87f, 0.60f, 0.74f, 0.90f, 0.90f, 0.31f, },    // Avarage
            new float [] { 0.52f, 0.13f, 0.89f, 0.70f, 0.74f, 0.95f, 0.92f, 0.31f, },    // Experienced
            new float [] { 0.73f, 0.14f, 0.92f, 0.80f, 0.74f, 1.00f, 0.95f, 0.41f, },    // Veteran
            new float [] { 0.93f, 0.15f, 0.96f, 0.92f, 0.84f, 1.00f, 1.00f, 0.51f, },    // Ace

            new float [] { 0.30f, 0.11f, 0.78f, 0.20f, 0.74f, 0.85f, 0.90f, 0.88f, },    // Rookie      (Bomber)
            new float [] { 0.32f, 0.12f, 0.87f, 0.25f, 0.74f, 0.90f, 0.95f, 0.91f, },    // Avarage
            new float [] { 0.52f, 0.13f, 0.89f, 0.28f, 0.74f, 0.92f, 0.95f, 0.91f, },    // Experienced
            new float [] { 0.73f, 0.14f, 0.92f, 0.30f, 0.74f, 0.95f, 0.95f, 0.95f, },    // Veteran
            new float [] { 0.93f, 0.15f, 0.96f, 0.35f, 0.74f, 1.00f, 1.00f, 0.97f, },    // Ace

            new float [] { 0.30f, 0.11f, 0.78f, 0.40f, 0.64f, 0.85f, 0.85f, 0.21f, },    // Rookie      (FighterBomber)
            new float [] { 0.32f, 0.12f, 0.87f, 0.60f, 0.74f, 0.90f, 0.90f, 0.31f, },    // Avarage
            new float [] { 0.52f, 0.13f, 0.89f, 0.70f, 0.74f, 0.95f, 0.92f, 0.31f, },    // Experienced
            new float [] { 0.73f, 0.14f, 0.92f, 0.80f, 0.74f, 1.00f, 0.95f, 0.41f, },    // Veteran
            new float [] { 0.93f, 0.15f, 0.96f, 0.92f, 0.84f, 1.00f, 1.00f, 0.51f, },    // Ace
        };

        public static readonly Skill Default = new Skill(DefaultValue, SkillNameDefault);
        public static readonly Skill Random = new Skill(RandomValue, SkillNameRandom);

        public static readonly Skill[] SystemSkills = new Skill[(int)ESystemType.Count]
        {
             new Skill(ESystemType.Rookie),
             new Skill(ESystemType.Avarage),
             new Skill(ESystemType.Veteran),
             new Skill(ESystemType.Ace),
        };

        public static readonly Skill[] TweakedSkills = new Skill[(int)ETweakedType.Count]
        {
             new Skill(ETweakedType.FighterRookie),
             new Skill(ETweakedType.FighterAvarage),
             new Skill(ETweakedType.FighterExperienced),
             new Skill(ETweakedType.FighterVeteran),
             new Skill(ETweakedType.FighterAce),
             new Skill(ETweakedType.BomberRookie),
             new Skill(ETweakedType.BomberAvarage),
             new Skill(ETweakedType.BomberExperienced),
             new Skill(ETweakedType.BomberVeteran),
             new Skill(ETweakedType.BomberAce),
             new Skill(ETweakedType.FighterBomberRookie),
             new Skill(ETweakedType.FighterBomberAvarage),
             new Skill(ETweakedType.FighterBomberExperienced),
             new Skill(ETweakedType.FighterBomberVeteran),
             new Skill(ETweakedType.FighterBomberAce),
        };

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

        public Skill(ESystemType type)
        {
            Name = type.ToString();
            Skills = SystemSkillValue[(int)type];
        }

        public Skill(ETweakedType type)
        {
            Name = type.ToString();
            Skills = TweakedSkillValue[(int)type];
        }

        public Skill(float[] skills, string name = null)
        {
            Name = name != null ? name : string.Empty;
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

        public override string ToString()
        {
            return string.Join(" ", Skills.Select(x => x.ToString(SkillFormat, Config.NumberFormat)));
        }

        public string ToDetailString(char splitCharLabelValue = ' ', int spaceBegin = 0)
        {
            if (Skills != null && Skills.Length >= (int)ESkilType.Count)
            {
                StringBuilder sb = new StringBuilder();
                // var types = Enum.GetNames(typeof(ESkilType));
                string spaceBegins = new string(' ', spaceBegin);
                for (ESkilType type = ESkilType.BasicFlying; type < ESkilType.Count; type++)
                {
                    sb.AppendFormat(Config.NumberFormat, "{0}{1,-15}{2}{3:F2}", spaceBegins, type.ToString(), splitCharLabelValue, Skills[(int)type]);
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            return string.Empty;
        }

        public static string ToIncrementDetailString(float[] skills, float[] newSkills, char splitCharLabelValue = ' ', int spaceBegin = 0)
        {
            if (skills != null && skills.Length >= (int)ESkilType.Count && newSkills != null && newSkills.Length >= (int)ESkilType.Count)
            {
                StringBuilder sb = new StringBuilder();
                // var types = Enum.GetNames(typeof(ESkilType));
                string spaceBegins = new string(' ', spaceBegin);
                for (ESkilType type = ESkilType.BasicFlying; type < ESkilType.Count; type++)
                {
                    sb.AppendFormat(Config.NumberFormat, "{0}{1,-15}{2}{3:F2} [{4,5}]", spaceBegins, type.ToString(), splitCharLabelValue, skills[(int)type], newSkills[(int)type].ToString("+0.00;-0.00", Config.NumberFormat));
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            return string.Empty;
        }

        public static string ToDetailString(IEnumerable<string> skills, char splitCharLabelValue = ' ', int spaceBegin = 0)
        {
            if (skills != null && skills.Any())
            {
                IEnumerable<string[]> skillsArray = skills.Select(x => x.Split(SplitChar));
                StringBuilder sb = new StringBuilder();
                // var types = Enum.GetNames(typeof(ESkilType));
                string spaceBegins = new string(' ', spaceBegin);
                for (ESkilType type = ESkilType.BasicFlying; type < ESkilType.Count; type++)
                {
                    sb.AppendFormat("{0}{1,-15}{2}", spaceBegins, type.ToString(), splitCharLabelValue);
                    foreach (var item in skillsArray)
                    {
                        float val;
                        if (item.Length >= (int)type + 1 && float.TryParse(item[(int)type], NumberStyles.Float, Config.NumberFormat, out val))
                        {
                            sb.AppendFormat(Config.NumberFormat, "{0}{1:F2}{2}", new string(' ', spaceBegin), val, splitCharLabelValue);
                        }
                        else
                        {
                            sb.AppendFormat("{0,4}{1}", spaceBegins, "----", splitCharLabelValue);
                        }
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            return string.Empty;
        }

        public static string ToDetailString(IEnumerable<Skill> skills, char splitCharLabelValue = ' ', int spaceBegin = 0)
        {
            if (skills != null && skills.Any())
            {
                StringBuilder sb = new StringBuilder();
                // var types = Enum.GetNames(typeof(ESkilType));
                string spaceBegins = new string(' ', spaceBegin);
                for (ESkilType type = ESkilType.BasicFlying; type < ESkilType.Count; type++)
                {
                    sb.AppendFormat("{0}{1,-15}{2}", new string(' ', spaceBegin), type.ToString(), splitCharLabelValue);
                    foreach (var item in skills)
                    {
                        sb.AppendFormat(Config.NumberFormat, "{0}{1:F2}{2}", spaceBegins, item.Skills[(int)type], splitCharLabelValue);
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            return string.Empty;
        }

        public static string ToDetailDisplayString(IEnumerable<Skill> skills, char splitCharLabelValue = ' ', int spaceBegin = 0)
        {
            string str = string.Empty;
            if (skills != null && skills.Any())
            {
                if (skills.Count() == 1)
                {
                    Skill skill = skills.First();
                    if (Skill.EqualsValue(skill, Skill.Default))
                    {
                        str = Skill.Default.Name;
                    }
                    else if (Skill.EqualsValue(skill, Skill.Random))
                    {
                        str = Skill.Random.Name;
                    }
                    else
                    {
                        str = skill.ToDetailString(splitCharLabelValue, spaceBegin);
                    }
                }
                else
                {
                    str = Skill.ToDetailString(skills, splitCharLabelValue, spaceBegin);
                }
            }
            return str;
        }

        public static string ToDetailDisplayStringHorizontal(IEnumerable<Skill> skills, char splitCharLabelValue = ' ', int spaceBegin = 0)
        {
            string str = string.Empty;
            if (skills != null && skills.Any())
            {
                if (skills.Count() == 1)
                {
                    Skill skill = skills.First();
                    if (Skill.EqualsValue(skill, Skill.Default))
                    {
                        str = Skill.Default.Name;
                    }
                    else if (Skill.EqualsValue(skill, Skill.Random))
                    {
                        str = Skill.Random.Name;
                    }
                    else
                    {
                        str = skill.ToDetailString(splitCharLabelValue, spaceBegin);
                    }
                }
                else
                {
                    str = string.Join("\n", skills.Select(x => x.ToDetailString(splitCharLabelValue, spaceBegin))); // Skill.ToDetailString(skills, splitCharLabelValue, spaceBegin);
                }
            }
            return str;
        }


#if false

        public bool IsSystemType()
        {
            return IsSystemType(Skills);
        }

        public static bool IsSystemType(float[] skills)
        {
            if (skills != null && skills.Length == (int)ESkilType.Count)
            {
                return SystemSkills.Any(x => EqualsValue(x.Skills, skills));
            }

            return false;
        }

        public Skill GetSystemType()
        {
            if (Skills != null && Skills.Length >= (int)ESkilType.Count)
            {
                return SystemSkills.Where(x => EqualsValue(x.Skills, Skills)).FirstOrDefault();
            }

            return null;
        }

        public static Skill GetSystemType(ESystemType skill)
        {
            return SystemSkills[(int)skill];
        }

        public static Skill GetSystemType(float[] skills)
        {
            if (skills != null && skills.Length == (int)ESkilType.Count)
            {
                return SystemSkills.Where(x => EqualsValue(x.Skills, skills)).FirstOrDefault();
            }

            return null;
        }

        public static Skill GetSystemType(ESystemType? skill = null)
        {
            return skill != null ? GetSystemType(skill.Value) : GetSystemType(ESystemType.Avarage);
        }

        public string GetSystemTypeName()
        {
            return GetSystemTypeName(Skills);
        }

        public static string GetSystemTypeName(float[] skills)
        {
            Skill skill = GetSystemType(skills);
            return skill != null ? skill.Name : string.Empty;
        }

        public bool IsTweakedType()
        {
            return IsTweakedType(Skills);
        }

        public static bool IsTweakedType(float[] skills)
        {
            if (skills != null && skills.Length == (int)ESkilType.Count)
            {
                return TweakedSkills.Any(x => EqualsValue(x.Skills, skills));
            }

            return false;
        }

        public Skill GetTweakedType()
        {
            if (Skills != null && Skills.Length >= (int)ESkilType.Count)
            {
                return TweakedSkills.Where(x => EqualsValue(x.Skills, Skills)).FirstOrDefault();
            }

            return null;
        }

        public static Skill GetTweakedType(ETweakedType skill)
        {
            return TweakedSkills[(int)skill];
        }

        public static Skill GetTweakedType(float[] skills)
        {
            if (skills != null && skills.Length == (int)ESkilType.Count)
            {
                return TweakedSkills.Where(x => EqualsValue(x.Skills, skills)).FirstOrDefault();
            }

            return null;
        }

        public static Skill GetTweakedType(ETweakedType? skill = null)
        {
            return skill != null ? GetTweakedType(skill.Value) : GetTweakedType(ETweakedType.FighterAvarage);
        }

        public string GetTweakedTypeName()
        {
            return GetTweakedTypeName(Skills);
        }

        public static string GetTweakedTypeName(float[] skills)
        {
            Skill skill = GetTweakedType(skills);
            return skill != null ? skill.Name : string.Empty;
        }

#endif

        public bool IsTyped()
        {
            return IsTyped(Skills);
        }

        public static bool IsTyped(float[] skills)
        {
            if (skills != null && skills.Length == (int)ESkilType.Count)
            {
                return SystemSkills.Any(x => EqualsValue(x.Skills, skills)) || TweakedSkills.Any(x => EqualsValue(x.Skills, skills));
            }
            return false;
        }

        public Skill GetTyped()
        {
            if (Skills != null && Skills.Length >= (int)ESkilType.Count)
            {
                Skill skill = SystemSkills.Where(x => EqualsValue(x.Skills, Skills)).FirstOrDefault();
                if (skill == null)
                {
                    skill = TweakedSkills.Where(x => EqualsValue(x.Skills, Skills)).FirstOrDefault();
                }
                return skill;
            }

            return null;
        }

        public static Skill GetTyped(float[] skills)
        {
            if (skills != null && skills.Length == (int)ESkilType.Count)
            {
                Skill skill = SystemSkills.Where(x => EqualsValue(x.Skills, skills)).FirstOrDefault();
                if (skill == null)
                {
                    skill = TweakedSkills.Where(x => EqualsValue(x.Skills, skills)).FirstOrDefault();
                }
                return skill;
            }

            return null;
        }

        public static Skill GetDefaultTyped()
        {
            return TweakedSkills[(int)ETweakedType.BomberAvarage];
        }

        public string GetTypedName()
        {
            return GetTypedName(Skills);
        }

        public static string GetTypedName(float[] skills)
        {
            Skill skill = GetTyped(skills);
            return skill != null ? skill.Name : string.Empty;
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
                        provider = Config.NumberFormat;
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

#if false
        public static bool operator ==(Skill a, Skill b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(Skill a, Skill b)
        {
            return !Equals(a, b);
        }
#endif

        public static bool Equals(Skill a, Skill b)
        {
            if (!EqualsValue(a, b))
            {
                return false;
            }

            if (a.Name != b.Name)
            {
                return false;
            }

            return true;
        }

        public static bool EqualsValue(Skill a, Skill b)
        {
            if ((object)a == b)
            {
                return true;
            }

            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            if (a.Skills.Length != b.Skills.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Skills.Length; i++)
            {
                if (a.Skills[i] != b.Skills[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool EqualsValue(float[] a, float[] b)
        {
            if ((object)a == b)
            {
                return true;
            }

            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class Skills : List<Skill>
    {
        public static readonly Skills Default;

        static Skills()
        {
            Default = CreateDefault();
        }

        public Skills()
        {

        }

        public Skills(IEnumerable<Skill> collection)
            : base(collection)
        {

        }

        public static Skills CreateDefault()
        {
            Skills skills = new Skills();
            skills.AddRange(Skill.SystemSkills);
            skills.AddRange(Skill.TweakedSkills);
            return skills;
        }

        public static Skills Create(ESkillSet skillSet)
        {
            Skills skills = new Skills();

            switch (skillSet)
            {
                case ESkillSet.Rookie:
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Rookie]);
                    break;
                case ESkillSet.RookieAvarage:
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Rookie]);
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Avarage]);
                    break;
                case ESkillSet.RookieVeteran:
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Rookie]);
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Avarage]);
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Veteran]);
                    break;
                case ESkillSet.RookieAce:
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Rookie]);
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Avarage]);
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Veteran]);
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Ace]);
                    break;
                case ESkillSet.Avarage:
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Avarage]);
                    break;
                case ESkillSet.AvarageVeteran:
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Avarage]);
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Veteran]);
                    break;
                case ESkillSet.AvarageAce:
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Avarage]);
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Veteran]);
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Ace]);
                    break;
                case ESkillSet.Veteran:
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Veteran]);
                    break;
                case ESkillSet.VeteranAce:
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Veteran]);
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Ace]);
                    break;
                case ESkillSet.Ace:
                    skills.Add(Skill.SystemSkills[(int)ESystemType.Ace]);
                    break;

                case ESkillSet.TweakedRookie:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterRookie]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberRookie]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberRookie]);
                    break;
                case ESkillSet.TweakedRookieAvarage:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterRookie]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberRookie]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberRookie]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAvarage]);
                    break;
                case ESkillSet.TweakedRookieExperienced:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterRookie]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberRookie]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberRookie]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberExperienced]);
                    break;
                case ESkillSet.TweakedRookieVeteran:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterRookie]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberRookie]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberRookie]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAce]);
                    break;
                case ESkillSet.TweakedAvarage:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAvarage]);
                    break;
                case ESkillSet.TweakedAvarageExperienced:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberExperienced]);
                    break;
                case ESkillSet.TweakedAvarageVeteran:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberVeteran]);
                    break;
                case ESkillSet.TweakedAvarageAce:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAvarage]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterAce]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberAce]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAce]);
                    break;
                case ESkillSet.TweakedExperienced:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberExperienced]);
                    break;
                case ESkillSet.TweakedExperiencedVeteran:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberVeteran]);
                    break;
                case ESkillSet.TweakedExperiencedAce:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberExperienced]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterAce]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberAce]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAce]);
                    break;
                case ESkillSet.TweakedVeteran:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberVeteran]);
                    break;
                case ESkillSet.TweakedVeteranAce:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberVeteran]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterAce]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberAce]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAce]);
                    break;
                case ESkillSet.TweakedAce:
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterAce]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.BomberAce]);
                    skills.Add(Skill.TweakedSkills[(int)ETweakedType.FighterBomberAce]);
                    break;

                case ESkillSet.Default:
                case ESkillSet.Random:
                default:
                    break;
            }

            return skills;
        }

        public static IEnumerable<ESystemType> Create(ESkillSetShip skillSet)
        {
            List<ESystemType> skills = new List<ESystemType>();

            switch (skillSet)
            {
                case ESkillSetShip.Rookie:
                    skills.Add(ESystemType.Rookie);
                    break;
                case ESkillSetShip.RookieAvarage:
                    skills.Add(ESystemType.Rookie);
                    skills.Add(ESystemType.Avarage);
                    break;
                case ESkillSetShip.RookieVeteran:
                    skills.Add(ESystemType.Rookie);
                    skills.Add(ESystemType.Avarage);
                    skills.Add(ESystemType.Veteran);
                    break;
                case ESkillSetShip.RookieAce:
                    skills.Add(ESystemType.Rookie);
                    skills.Add(ESystemType.Avarage);
                    skills.Add(ESystemType.Veteran);
                    skills.Add(ESystemType.Ace);
                    break;
                case ESkillSetShip.Avarage:
                    skills.Add(ESystemType.Avarage);
                    break;
                case ESkillSetShip.AvarageVeteran:
                    skills.Add(ESystemType.Avarage);
                    skills.Add(ESystemType.Veteran);
                    break;
                case ESkillSetShip.AvarageAce:
                    skills.Add(ESystemType.Avarage);
                    skills.Add(ESystemType.Veteran);
                    skills.Add(ESystemType.Ace);
                    break;
                case ESkillSetShip.Veteran:
                    skills.Add(ESystemType.Veteran);
                    break;
                case ESkillSetShip.VeteranAce:
                    skills.Add(ESystemType.Veteran);
                    skills.Add(ESystemType.Ace);
                    break;
                case ESkillSetShip.Ace:
                    skills.Add(ESystemType.Ace);
                    break;

                case ESkillSetShip.Default:
                case ESkillSetShip.Random:
                default:
                    break;
            }

            return skills;
        }

        public static void UpdateSkillValue(Skills skillsUpdate, Skills skills)
        {
            foreach (var item in skills)
            {
                var changed = skillsUpdate.Where(x => string.Compare(x.Name, item.Name) == 0 && !Skill.EqualsValue(x.Skills, item.Skills));
                foreach (var change in changed)
                {
                    change.Skills = item.Skills;
                }
            }
        }
    }

    public enum ESkillSet
    {
        [Description(SkillNameRandom)]
        Random = -2,
        [Description(SkillNameDefault)]
        Default = -1,
        [Description("Rookie")]
        Rookie,
        [Description("Rookie - Avarage")]
        RookieAvarage,
        [Description("Rookie - Veteran")]
        RookieVeteran,
        [Description("Rookie - Ace")]
        RookieAce,
        [Description("Avarage")]
        Avarage,
        [Description("Avarage - Veteran")]
        AvarageVeteran,
        [Description("Avarage - Ace")]
        AvarageAce,
        [Description("Veteran")]
        Veteran,
        [Description("Veteran - Ace")]
        VeteranAce,
        [Description("Ace")]
        Ace,
        [Description("Tweaked Rookie")]
        TweakedRookie,
        [Description("Tweaked Rookie - Avarage")]
        TweakedRookieAvarage,
        [Description("Tweaked Rookie - Experienced")]
        TweakedRookieExperienced,
        [Description("Tweaked Rookie - Veteran")]
        TweakedRookieVeteran,
        [Description("Tweaked Avarage")]
        TweakedAvarage,
        [Description("Tweaked Avarage - Experienced")]
        TweakedAvarageExperienced,
        [Description("Tweaked Avarage - Veteran")]
        TweakedAvarageVeteran,
        [Description("Tweaked Avarage - Ace")]
        TweakedAvarageAce,
        [Description("Tweaked Experienced")]
        TweakedExperienced,
        [Description("Tweaked Experienced - Veteran")]
        TweakedExperiencedVeteran,
        [Description("Tweaked Experienced - Ace")]
        TweakedExperiencedAce,
        [Description("Tweaked Veteran")]
        TweakedVeteran,
        [Description("Tweaked Veteran - Ace")]
        TweakedVeteranAce,
        [Description("Tweaked Ace")]
        TweakedAce,
        [Description("User Settings")]
        UserSettings,

        Count,
    }

    public enum ESkillSetShip
    {
        [Description(SkillNameRandomShip)]
        Random = -2,
        [Description(SkillNameDefaultShip)]
        Default = -1,
        [Description("Rookie")]
        Rookie,
        [Description("Rookie - Avarage")]
        RookieAvarage,
        [Description("Rookie - Veteran")]
        RookieVeteran,
        [Description("Rookie - Ace")]
        RookieAce,
        [Description("Avarage")]
        Avarage,
        [Description("Avarage - Veteran")]
        AvarageVeteran,
        [Description("Avarage - Ace")]
        AvarageAce,
        [Description("Veteran")]
        Veteran,
        [Description("Veteran - Ace")]
        VeteranAce,
        [Description("Ace")]
        Ace,

        Count,
    }

    public class SkillSet
    {
        public Skill[] Player
        {
            get;
            set;
        }

        public Skill Friendly
        {
            get;
            set;
        }

        public Skill Enemy
        {
            get;
            set;
        }

        public SkillSet()
        {

        }
    }
}