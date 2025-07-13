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

using System.ComponentModel;

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

    public enum ESpawnRandomLocation
    {
        Player = 0,
        Friendly = 1,
        Enemy = 2,
    }

    public enum ESpawnTime
    {
        Random = -2,
        Default = -1,
        Live = 0,
    }

    public class Spawn
    {
        #region Definition

        public const int SelectStartAltitude = 500;
        public const int SelectEndAltitude = 10000;
        public const int SelectStepAltitude = 500;

        public class SpawnLocation
        {
            public bool IsRandomizePlayer
            {
                get;
                set;
            }

            public bool IsRandomizeFriendly
            {
                get;
                set;
            }

            public bool IsRandomizeEnemy
            {
                get;
                set;
            }

            public SpawnLocation(bool isRandomizePlayer = false, bool isRandomizeFriendly = false, bool isRandomizeEnemy = false)
            {
                IsRandomizePlayer = isRandomizePlayer;
                IsRandomizeFriendly = isRandomizeFriendly;
                IsRandomizeEnemy = isRandomizeEnemy;
            }
        }

        public class SpawnTime
        {
            public const int MinimumBeginSec = 15;
            public const int MaximumEndSec = 1800;
            public const int DefaultBeginSec = 15;
            public const int DefaultEndSec = 45;
            public const int DefaultSelectSec = 120;
            public const int DefaultSec = 0;

            public int Value
            {
                get;
                set;
            }

            public bool IsDelay
            {
                get
                {
                    return Value > 0;
                }
            }

            public bool IsLive
            {
                get
                {
                    return !IsDelay;
                }
            }

            public bool IsRandom
            {
                get;
                set;
            }

            public int BeginSec
            {
                get;
                set;
            }

            public int EndSec
            {
                get;
                set;
            }

            public SpawnTime(bool isRandom = false, int sec = DefaultSec, int beginSec = MinimumBeginSec, int endSec = MaximumEndSec)
            {
                IsRandom = isRandom;
                if (isRandom)
                {
                    Value = sec >= MinimumBeginSec ? sec <= MaximumEndSec ? sec : MaximumEndSec : MinimumBeginSec;
                }
                else
                {
                    Value = sec;
                }
                BeginSec = beginSec >= MinimumBeginSec ? beginSec : MinimumBeginSec;
                EndSec = endSec <= MaximumEndSec ? endSec < BeginSec ? BeginSec : endSec : MaximumEndSec;
            }

            public SpawnTime Clone()
            {
                return (SpawnTime)this.MemberwiseClone();
            }
        }

        #endregion

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

        public SpawnLocation Location
        {
            get;
            protected set;
        }

        public SpawnTime Time
        {
            get;
            protected set;
        }

		public bool IsRandomizeTimeFriendly
		{
			get;
			set;
		}

		public bool IsRandomizeTimeEnemy
		{
			get;
			set;
		}

		public bool IsRandomizeAltitudeFriendly
		{
			get;
			set;
		}

		public bool IsRandomizeAltitudeEnemy
		{
			get;
			set;
		}


		public string DisplayName
        {
            get
            {
                return CreateDisplayString(Altitude);
            }
        }

        public Spawn(int altitude, bool isRandomizeAltitudeFriendly = false, bool isRandomizeAltitudeEnemy = false, SpawnLocation location = null, bool isRandomizeTimeFriendly = false, bool isRandomizeTimeEnemy = false, SpawnTime spawnTime = null)
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
                switch (rnd.Next(1, 5))
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
                        Altitude = rnd.Next(SelectStartAltitude / SelectStepAltitude, SelectEndAltitude / SelectStepAltitude + 1) * SelectStepAltitude;
                        break;
                }
            }
            else/* if (altitude == (int)ESpawn.Default)*/
            {
                Type = ESpawn.Default;
                Altitude = altitude;
            }
			IsRandomizeAltitudeFriendly = isRandomizeAltitudeFriendly;
			IsRandomizeAltitudeEnemy = isRandomizeAltitudeEnemy;

			Location = location != null ? location: new SpawnLocation();

            IsRandomizeTimeFriendly = isRandomizeTimeFriendly;
			IsRandomizeTimeEnemy = isRandomizeTimeEnemy;
			Time = spawnTime != null ? spawnTime : new SpawnTime();
        }

        public static ESpawn CreateRandomSpawnType()
        {
            ESpawn type;
			Random rnd = new Random();
			switch (rnd.Next(1, 5))
			{
				case 1:
					type = ESpawn.Parked;
					break;
				case 2:
					type = ESpawn.Idle;
					break;
				case 3:
					type = ESpawn.Scramble;
					break;
                default:
				case 4:
					type = ESpawn.AirStart;
					break;
			}
            return type;
		}

		public static Spawn Create(int altitude, Spawn spawn)
        {
            return new Spawn(altitude, spawn.IsRandomizeAltitudeFriendly, spawn.IsRandomizeAltitudeEnemy, spawn.Location, spawn.IsRandomizeTimeFriendly, spawn.IsRandomizeTimeEnemy, spawn.Time.Clone());
        }

        public static string CreateDisplayString(int altitude)
        {
            if (altitude >= (int)ESpawn.Parked && altitude <= (int)ESpawn.Default)
            {
                return ((ESpawn)altitude).ToDescription();
            }
            else if (altitude >= SelectStartAltitude && altitude <= SelectEndAltitude)
            {
                return altitude.ToString(Config.NumberFormat);
            }

            return string.Empty;
        }
    }
}
