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

using maddox.GP;

namespace IL2DCE.MissionObjectModel
{
    public class GroundObject : MissionObject
    {
        public virtual double X
        {
            get;
            protected set;
        }

        public virtual double Y
        {
            get;
            protected set;
        }

        public double Direction
        {
            get;
            protected set;
        }

        public virtual Point2d Position
        {
            get
            {
                return new Point2d(this.X, this.Y);
            }
        }

        public GroundObject(string id, string @class, int army, ECountry country, double x, double y, double direction)
            : base(id, @class, army, country)
        {
            X = x;
            Y = y;
            Direction = direction;
        }
    }
}
