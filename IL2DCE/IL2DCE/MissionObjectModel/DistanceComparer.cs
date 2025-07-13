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

using System.Collections.Generic;
using maddox.game.world;
using maddox.GP;

namespace IL2DCE.MissionObjectModel
{
    public class DistanceComparer : IComparer<AirGroup>, IComparer<GroundGroup>, IComparer<Stationary>, IComparer<AiAirport>
    {
        private Point3d position3d;
        private Point2d position2d;

        public DistanceComparer(Point3d position)
        {
            position3d = position;
            position2d = new Point2d(position.x, position.y);
        }

        public DistanceComparer(Point2d position)
        {
            position3d = new Point3d(position.x, position.y, 0.0);
            position2d = position;
        }

        public int Compare(AirGroup x, AirGroup y)
        {
            return x.Position.distance(ref position3d).CompareTo(y.Position.distance(ref position3d));
        }

        public int Compare(GroundGroup x, GroundGroup y)
        {
            return x.Position.distance(ref position2d).CompareTo(y.Position.distance(ref position2d));
        }

        public int Compare(Stationary x, Stationary y)
        {
            return x.Position.distance(ref position2d).CompareTo(y.Position.distance(ref position2d));
        }

        //public int Compare(KeyValuePair<int, Point3d> x, KeyValuePair<int, Point3d> y)
        //{
        //    return x.Value.distance(ref _position3d).CompareTo(y.Value.distance(ref _position3d));
        //}        

        public int Compare(AiAirport x, AiAirport y)
        {
            return x.Pos().distance(ref position3d).CompareTo(y.Pos().distance(ref position3d));
        }
    }
}