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

using System.ComponentModel;
using System.Globalization;

namespace IL2DCE.MissionObjectModel
{
    public enum ESpawn
    {
        [Description("Parked")]
        Parked = -1005,

        [Description("Idle")]
        Idle = -1004,

        [Description("Scramble")]
        Scramble = -1003,

        [Description("AirStart")]
        AirStart = -1002,

        [Description("Random")]
        Random = -1001,

        [Description("Default")]
        Default = -1,
    }

    public class Spawn
    {
        public const int SelectStartAltitude = 500;
        public const int SelectEndAltitude = 10000;
        public const int SelectStepAltitude = 500;

        public ESpawn Type
        {
            get;
            protected set;
        }

        public int Altitude
        {
            get;
            protected set;
        }

        public string DisplayName
        {
            get
            {
                return CreateDisplayName(Altitude);
            }
        }

        public Spawn(int altitude)
        {
            if (altitude >= (int)ESpawn.Parked && altitude <= (int)ESpawn.Scramble)
            {
                Type = (ESpawn)altitude;
                Altitude = altitude;
            }
            else if (altitude >= SelectStartAltitude && altitude <= SelectEndAltitude)
            {
                Type = ESpawn.AirStart;
                Altitude = altitude;
            }
            else if (altitude == (int)ESpawn.Random)
            {
                Random rnd = new Random();
                switch (rnd.Next(1, 4))
                {
                    case 1:
                        Type = ESpawn.Parked;
                        Altitude = (int)Type;
                        break;
                    case 2:
                        Type = ESpawn.Idle;
                        Altitude = (int)Type;
                        break;
                    case 3:
                        Type = ESpawn.Scramble;
                        Altitude = (int)Type;
                        break;
                    case 4:
                        Type = ESpawn.AirStart;
                        Altitude = rnd.Next(SelectStartAltitude / SelectStepAltitude, SelectEndAltitude / SelectStepAltitude) * SelectStepAltitude;
                        break;
                }
            }
            else/* if (altitude == (int)ESpawn.Default)*/
            {
                Type = ESpawn.Default;
                Altitude = altitude;
            }
        }

        public static string CreateDisplayName(int altitude)
        {
            if (altitude >= (int)ESpawn.Parked && altitude <= (int)ESpawn.Default)
            {
                return ((ESpawn)altitude).ToDescription();
            }
            else if (altitude >= SelectStartAltitude && altitude <= SelectEndAltitude)
            {
                return altitude.ToString(CultureInfo.InvariantCulture.NumberFormat);
            }

            return string.Empty;
        }
    }
}
