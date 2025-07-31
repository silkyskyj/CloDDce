// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover
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
using System.Linq;
using System.Reflection;
using IL2DCE.MissionObjectModel;

namespace IL2DCE.Util
{
    public class VersionConverter
    {
        public VersionConverter()
        {
        }

        public static Version GetCurrentVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetName().Version;
        }

        public static string ReplaceKillsHistory(string str)
        {
#if false
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                char[] sz = str.ToCharArray();
                int len = sz.Length;
                for (int i = 0; i < len; i++)
                {
                    if (sz[i] == '=')
                    {
                        sb.Append(' ');
                    }
                    else if (sz[i] == 'x' && i > 0 && sz[i - 1] == ' ' && (i + 1) < len && sz[i + 1] == ' ')
                    {
                        i++;
                    }
                    else if (sz[i] == ',' && i > 0 && sz[i - 1] >= '0' && sz[i - 1] <= '9' && (i + 1) < len && sz[i + 1] >= '0' && sz[i + 1] <= '9')
                    {
                        sb.Append('.');
                        sb.Append(sz[i + 1]);
                        i++;
                    }
                    else if (sz[i] == ']' || sz[i] == '[')
                    {
                        ;
                    }
                    else
                    {
                        sb.Append(sz[i]);
                    }
                }
            }
            return sb.ToString();
#else
            if (!string.IsNullOrEmpty(str))
            {
                return str.Replace(" x ", " ").Replace("[", string.Empty).Replace("]", string.Empty).Replace("=", " ").Replace(", ", "|").Replace(",", ".").Replace("|", ", ");
            }

            return string.Empty;
#endif
        }

        public static void OptimizePlayerStatTotal(IPlayerStatTotal playerStatTotal)
        {
            if (playerStatTotal.Sorties == 0)
            {
                playerStatTotal.Sorties = new int[] { playerStatTotal.Takeoffs, playerStatTotal.Landings, playerStatTotal.Bails, playerStatTotal.Deaths }.Max();
            }

            if (playerStatTotal.FlyingTime == 0)
            {
                playerStatTotal.FlyingTime = playerStatTotal.Sorties * 15 * 60; // 15min * Sorties
            }

        }
    }
}
