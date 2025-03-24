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
using System.Linq;
using static IL2DCE.MissionObjectModel.Skill;

namespace IL2DCE.MissionObjectModel
{
    // RED
    //  FighterEarly    ECHELONLEFT, ECHELONRIGHT, LINEABREAST, LINEASTERN, VIC3                           early BoB/France
    //  Fighter         DIAMOND, ECHELONLEFT, ECHELONRIGHT, FINGERFOUR, LINEABREAST, LINEASTERN, VIC       Fighter (BoB)/Fighter (Channel)/Fighter (Desert)/Fighter (Channel) USA/Fighter (Desert) USA
    //  Bomber          ECHELONLEFT, ECHELONRIGHT, LINEABREAST, LINEASTERN, VIC                            Bomber (BoB)/Bomber (Channel)/Bomber (Desert)/Recon (Desert)
    // 
    // BLUE
    //  Recon           LINEABREAST, LINEASTERN                                                            Recon(BoB)
    //  Fighter         DIAMOND, ECHELONLEFT, ECHELONRIGHT, FINGERFOUR, LINEABREAST, LINEASTERN, VIC       Fighter (BoB)/Fighter (Channel)/Fighter (Desert)/Fighter Stabstaffel (BoB)/Fighter Staffel (Channel)/Fighter Staffel (Desert)/Zerstörer (BoB)/Zerstörer (Desert)/Zerstörer Stabstaffel (BoB)/Zerstörer Staffel (Desert)
    //  Bomber          ECHELONLEFT, ECHELONRIGHT, LINEABREAST, LINEASTERN, VIC3                           Bomber (BoB)/Bomber (BoB) IT/Bomber (Desert)/Bomber (Desert) IT/Dive Bomber (Desert) IT/Bomber Stabstaffel (BoB)/Stuka (BoB)/Stuka (Desert)/Stuka Stabstaffel (BoB)/Transport (BoB)/Fighter (BoB) IT/Fighter (Desert) IT/Fighter-Bomber (Desert) IT
    //  Transport       ECHELONLEFT, ECHELONRIGHT, LINEABREAST, LINEASTERN, VIC                            Transport (BoB)

    public enum EFormation
    {

        [Description("Random")]
        Random = -2,

        [Description("Default")]
        Default = -1,

        [Description("Diamond")]
        DIAMOND,

        [Description("Echelon Left")]
        ECHELONLEFT,

        [Description("Echelon Right")]
        ECHELONRIGHT,

        [Description("Finger Four")]
        FINGERFOUR,

        [Description("Line Abreast")]
        LINEABREAST,

        [Description("Line Astern")]
        LINEASTERN,

        [Description("Vic")]
        VIC,

        [Description("Vic-3")]
        VIC3,

        Count,
    }

    // Type 0   LINEABREAST, LINEASTERN
    // Type 1   ECHELONLEFT, ECHELONRIGHT, LINEABREAST, LINEASTERN, VIC
    // Type 2   ECHELONLEFT, ECHELONRIGHT, LINEABREAST, LINEASTERN, VIC3
    // Type 3   DIAMOND, ECHELONLEFT, ECHELONRIGHT, FINGERFOUR, LINEABREAST, LINEASTERN, VIC
    // Type 4   LINEABREAST	(Unknown group type use LINEABREAST)

    public enum EFormationsType
    {
        BlueRecon = 0,
        RedBomber = 1,
        BlueTransport = 1,
        RedFighterEarly = 2,
        BlueBomber = 2,
        RedFighter = 3,
        BlueFighter = 3,
        Unknown = 4,
        Count = 5,
    }

    public class Formation
    {

    }

    public class Formations : List<EFormation>
    {
        public static readonly Formations[] Default = new Formations[(int)EFormationsType.Count]
        {
             new Formations(new EFormation [] { EFormation.LINEABREAST, EFormation.LINEASTERN }),
             new Formations(new EFormation [] { EFormation.ECHELONLEFT, EFormation.ECHELONRIGHT, EFormation.LINEABREAST, EFormation.LINEASTERN, EFormation.VIC }),
             new Formations(new EFormation [] { EFormation.ECHELONLEFT, EFormation.ECHELONRIGHT, EFormation.LINEABREAST, EFormation.LINEASTERN, EFormation.VIC3 }),
             new Formations(new EFormation [] { EFormation.DIAMOND, EFormation.ECHELONLEFT, EFormation.ECHELONRIGHT, EFormation.FINGERFOUR, EFormation.LINEABREAST, EFormation.LINEASTERN, EFormation.VIC }),
             new Formations(new EFormation [] { EFormation.LINEABREAST }),
        };

        public Formations(IEnumerable<EFormation> collection)
            : base(collection)
        {
        }

        public EFormation GetRandom(Random random = null)
        {
            if (random == null)
            {
                random = Random.Default;
            }
            return this[random.Next(Count)];
        }
    }
}
