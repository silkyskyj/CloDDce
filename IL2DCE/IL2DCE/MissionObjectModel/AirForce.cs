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
        [Description("イギリス空軍")]
        Raf = 1,
        [Description("フランス空軍")]
        Aa = 2,
        [Description("アメリカ陸軍航空軍")]
        Usaaf = 3,
        [Description("ソビエト空軍")]
        Ru = 4,
        [Description("ポーランド空軍")]
        Pl = 5,
        [Description("チェコスロバキア空軍")]
        Cz = 6,
        [Description("オランダ空軍")]
        Nl = 7,
        [Description("ベルギー空軍")]
        Be = 8,
    };

    public enum EAirForceBlue
    {
        [Description("ドイツ空軍")]
        Lw = 1,
        [Description("イタリア空軍")]
        Ra = 2,
        [Description("ハンガリー空軍")]
        Hu = 3,
        [Description("ルーマニア空軍")]
        Ro = 4,
        [Description("フィンランド空軍")]
        Fi = 5,
        [Description("スロバキア空軍")]
        Sv = 6,
        [Description("大日本帝国陸軍")]
        Ja = 7,
        [Description("大日本帝国海軍")]
        Jn = 8,
    };

    public class AirForce
    {
        public static readonly string[] PilotNameRedDefault = new string[] {
            "",
            "ジョー ブログス",
            "ジーン デュポン",
            "ジョン スミス",
            "イワン イワノビッチ",
            "ジャン コワルスキー" ,
            "ジョセフ ジャノーセク" ,
            "マージン ジャンセン" ,
            "ガブリエル メース" ,
        };
        public static readonly string[] PilotNameBlueDefault = new string[] {
            "",
            "マックス マスターマン",
            "マリオ ロッシ",
            "キス ジャノス",
            "イオン コステッシュ",
            "マッチ メイカライネン",
            "ジョセフ パブリック",
            "上坊 良太郎",
            "岩本 徹三",
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
            Default.Add(new AirForce((int)EArmy.Blue, (int)EAirForceBlue.Ja, EAirForceBlue.Sv.ToString(), EAirForceBlue.Ja.ToDescription(),
                            AirForce.PilotNameBlueDefault[(int)EAirForceBlue.Ja], Rank.Ja));
            Default.Add(new AirForce((int)EArmy.Blue, (int)EAirForceBlue.Jn, EAirForceBlue.Sv.ToString(), EAirForceBlue.Ja.ToDescription(),
                            AirForce.PilotNameBlueDefault[(int)EAirForceBlue.Jn], Rank.Jn));
        }
    }
}
