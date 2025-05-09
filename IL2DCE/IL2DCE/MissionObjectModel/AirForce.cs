// IL2DCE: A dynamic campaign engine & dynamic mission for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
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
using System.ComponentModel;

namespace IL2DCE.MissionObjectModel
{
    public enum EAirForceRed
    {
        [Description("Royal Air Force")]
        Raf = 1,
        [Description("Armee de l'air")]
        Aa = 2,
        [Description("United States Army Air Forces")]
        Usaaf = 3,
        [Description("Soviet Air Force")]
        Ru = 4,
        [Description("Air Force of Polish government-in-exile")]
        Pl = 5,
        [Description("Air Force of Czechoslovak government-in-exile")]
        Cz = 6,
        [Description("Royal Netherlands Air Force")]
        Nl = 7,
        [Description("Belgian Air Component")]
        Be = 8,
    };

    public enum EAirForceBlue
    {
        [Description("Luftwaffe")]
        Lw = 1,
        [Description("Regia Aeronautica")]
        Ra = 2,
        [Description("Royal Hungarian Air Force")]
        Hu = 3,
        [Description("Royal Romanian Air Force")]
        Ro = 4,
        [Description("Finnish Air Force")]
        Fi = 5,
        [Description("Slovak Air Force")]
        Sv = 6,
        [Description("Bulgarian Air Force")]
        Bg = 7,
        [Description("Air Force of the Independent State of Croatia")]
        Nd = 8,
    };

    public class AirForce
    {
        public static readonly string[] PilotNameRedDefault = new string[] {
            "",
            "Joe Bloggs",
            "Jean Dupont",
            "John Smith",
            "Ivanov Ivanovich",
            "Jan Kowalski" ,
            "Josef Janousek" ,
            "Martijn Jansen" ,
            "Gabriel Maes" ,
        };
        public static readonly string[] PilotNameBlueDefault = new string[] {
            "",
            "Max Mustermann",
            "Mario Rossi",
            "Kis Janos",
            "Ion Costescu",
            "Matti Meikalainen",
            "Jozef Pavlík",
            "Stoyan Spisarevski",
            "Ljudevit Acinger",
        };

        public int ArmyIndex
        {
            get;
            set;
        }

        public int AirForceIndex
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string PilotNameDefault
        {
            get;
            set;
        }

        public string [] Ranks
        {
            get;
            set;
        }

        public AirForce(int army, int airForce, string id, string name, string pilotDefaultname, string[] ranks)
        {
            ArmyIndex = army;
            AirForceIndex = airForce;
            Id = id;
            Name = name;
            PilotNameDefault = pilotDefaultname;
            Ranks = ranks;
        }

        public static bool IsTrust(int army, int airForce)
        {
            return airForce >= 1 && ((army == (int)EArmy.Red && airForce <= (int)EAirForceRed.Usaaf) || (army == (int)EArmy.Blue && airForce <= (int)EAirForceBlue.Ra));
        }
    }

    public class AirForces : List<AirForce>
    {
        public static AirForces Default;
        
        static AirForces()
        {
            Default = new AirForces();
            // Red Army
            Default.Add(new AirForce((int)EArmy.Red, (int)EAirForceRed.Raf, EAirForceRed.Raf.ToString(), EAirForceRed.Raf.ToDescription(), 
                            AirForce.PilotNameRedDefault[(int)EAirForceRed.Raf], Rank.Raf));
            Default.Add(new AirForce((int)EArmy.Red, (int)EAirForceRed.Aa, EAirForceRed.Aa.ToString(), EAirForceRed.Aa.ToDescription(), 
                            AirForce.PilotNameRedDefault[(int)EAirForceRed.Aa], Rank.Aa));
            Default.Add(new AirForce((int)EArmy.Red, (int)EAirForceRed.Usaaf, EAirForceRed.Usaaf.ToString(), EAirForceRed.Usaaf.ToDescription(), 
                            AirForce.PilotNameRedDefault[(int)EAirForceRed.Usaaf], Rank.Usaaf));
            Default.Add(new AirForce((int)EArmy.Red, (int)EAirForceRed.Ru, EAirForceRed.Ru.ToString(), EAirForceRed.Ru.ToDescription(),
                            AirForce.PilotNameRedDefault[(int)EAirForceRed.Ru], Rank.Ru));
            Default.Add(new AirForce((int)EArmy.Red, (int)EAirForceRed.Pl, EAirForceRed.Pl.ToString(), EAirForceRed.Pl.ToDescription(),
                            AirForce.PilotNameRedDefault[(int)EAirForceRed.Pl], Rank.Pl));
            Default.Add(new AirForce((int)EArmy.Red, (int)EAirForceRed.Cz, EAirForceRed.Cz.ToString(), EAirForceRed.Cz.ToDescription(),
                            AirForce.PilotNameRedDefault[(int)EAirForceRed.Cz], Rank.Cz));
            Default.Add(new AirForce((int)EArmy.Red, (int)EAirForceRed.Nl, EAirForceRed.Nl.ToString(), EAirForceRed.Nl.ToDescription(),
                            AirForce.PilotNameRedDefault[(int)EAirForceRed.Nl], Rank.Nl));
            Default.Add(new AirForce((int)EArmy.Red, (int)EAirForceRed.Be, EAirForceRed.Be.ToString(), EAirForceRed.Be.ToDescription(),
                            AirForce.PilotNameRedDefault[(int)EAirForceRed.Be], Rank.Be));

            // Blue Army
            Default.Add(new AirForce((int)EArmy.Blue, (int)EAirForceBlue.Lw, EAirForceBlue.Lw.ToString(), EAirForceBlue.Lw.ToDescription(), 
                            AirForce.PilotNameBlueDefault[(int)EAirForceBlue.Lw], Rank.Lw));
            Default.Add(new AirForce((int)EArmy.Blue, (int)EAirForceBlue.Ra, EAirForceBlue.Ra.ToString(), EAirForceBlue.Ra.ToDescription(), 
                            AirForce.PilotNameBlueDefault[(int)EAirForceBlue.Ra], Rank.Ra));
            Default.Add(new AirForce((int)EArmy.Blue, (int)EAirForceBlue.Hu, EAirForceBlue.Hu.ToString(), EAirForceBlue.Hu.ToDescription(),
                            AirForce.PilotNameBlueDefault[(int)EAirForceBlue.Hu], Rank.Hu));
            Default.Add(new AirForce((int)EArmy.Blue, (int)EAirForceBlue.Ro, EAirForceBlue.Ro.ToString(), EAirForceBlue.Ro.ToDescription(),
                            AirForce.PilotNameBlueDefault[(int)EAirForceBlue.Ro], Rank.Ro));
            Default.Add(new AirForce((int)EArmy.Blue, (int)EAirForceBlue.Fi, EAirForceBlue.Fi.ToString(), EAirForceBlue.Fi.ToDescription(),
                            AirForce.PilotNameBlueDefault[(int)EAirForceBlue.Fi], Rank.Fi));
            Default.Add(new AirForce((int)EArmy.Blue, (int)EAirForceBlue.Sv, EAirForceBlue.Sv.ToString(), EAirForceBlue.Sv.ToDescription(),
                            AirForce.PilotNameBlueDefault[(int)EAirForceBlue.Sv], Rank.Sv));
            Default.Add(new AirForce((int)EArmy.Blue, (int)EAirForceBlue.Bg, EAirForceBlue.Bg.ToString(), EAirForceBlue.Bg.ToDescription(),
                            AirForce.PilotNameBlueDefault[(int)EAirForceBlue.Bg], Rank.Bg));
            Default.Add(new AirForce((int)EArmy.Blue, (int)EAirForceBlue.Nd, EAirForceBlue.Nd.ToString(), EAirForceBlue.Nd.ToDescription(),
                            AirForce.PilotNameBlueDefault[(int)EAirForceBlue.Nd], Rank.Nd));
        }
    }
}
