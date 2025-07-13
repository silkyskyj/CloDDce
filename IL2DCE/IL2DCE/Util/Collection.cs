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

using System.Collections.Generic;
using System;
using System.Linq;

namespace IL2DCE.Util
{
    public class Collection
    {
        public static List<List<T>> SplitList<T>(List<T> locations, int numberOfChunks)
        {
            List<List<T>> result = new List<List<T>>();
            for (int i = 0; i < locations.Count; i += numberOfChunks)
            {
                result.Add(locations.GetRange(i, Math.Min(numberOfChunks, locations.Count - i)));
            }
            return result;
        }

        public static IEnumerable<int> GetRange(int min, int max, int stepMin = 0)
        {
            List<int> list = new List<int>();
            stepMin = stepMin == 0 ? min == 0 ? min < max ? 1 : -1 : min : stepMin;
            int old = stepMin;
            for (int i = min; i <= max;)
            {
                list.Add(i);
                int j = i;
                int p = 0;
                while (j >= 10)
                {
                    j /= 10;
                    p++;
                }
                if (j == 5)
                {
                    old = (int)Math.Floor((double)(i / 2));
                    i += old;
                }
                else if (j == 7)
                {
                    int k = (int)Math.Pow(10, p);
                    i += p ==0 ? 3 * k: old;
                    old = k * 10;
                }
                else
                {
                    j = i;
                    i += old == 0 ? stepMin: old;
                    old = j;
                }
            }
            if (!list.Contains(max))
            {
                list.Add(max);
            }
            return list.OrderBy(x => x);
        }
    }
}
