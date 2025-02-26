using System;
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

    public class Spawn
    {
        public const int SelectStartAltitude = 500;
        public const int SelectEndAltitude = 5000;
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
                return altitude.ToString(Config.Culture);
            }

            return string.Empty;
        }
    }
}
