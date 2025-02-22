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

using System.Linq;

namespace IL2DCE.MissionObjectModel
{
    public class Skill
    {
        public string SkillFormat = "F2";

        public enum SystemType
        {
            Rookie,
            Avarage,
            Veteran,
            Ace,
            Count,
        }

        public enum SkilType
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

        public static readonly float[][] SystemSkillValue = new float[(int)SystemType.Count][]
        {
            new float [] { 0.79f, 0.26f, 0.26f, 0.16f, 0.16f, 0.26f, 0.37f, 0.26f, },    // Rookie 
            new float [] { 0.84f, 0.53f, 0.53f, 0.37f, 0.37f, 0.53f, 0.53f, 0.53f, },    // Avarage
            new float [] { 1.00f, 0.74f, 0.84f, 0.63f, 0.84f, 0.84f, 0.74f, 0.74f, },    // Veteran
            new float [] { 1.00f, 0.95f, 0.95f, 0.84f, 0.84f, 0.95f, 0.89f, 0.89f, },    // Ace
        };

        public static Skill[] SystemSkills = new Skill[(int)SystemType.Count]
        {
             new Skill(SystemType.Rookie),
             new Skill(SystemType.Avarage),
             new Skill(SystemType.Veteran),
             new Skill(SystemType.Ace),
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
            Skills = new float[(int)SkilType.Count];
        }

        public Skill(SystemType systemType)
        {
            Name = systemType.ToString();
            Skills = SystemSkillValue[(int)systemType];
        }

        public float GetSkill(SkilType type)
        {
            return Skills[(int)type];
        }

        public void SetSkill(SkilType type, float value)
        {
            if (value >= 0.0f && value <= 1.0f)
            {
                Skills[(int)type] = value;
            }
        }

        public override string ToString()
        {
            return string.Join(" ", Skills.Select(x => x.ToString(SkillFormat, Config.Culture)));
        }

        public static Skill GetSystemType(SystemType skill)
        {
            return SystemSkills[(int)skill];
        }
    }
}
