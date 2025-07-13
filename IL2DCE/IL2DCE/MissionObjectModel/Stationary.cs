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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using maddox.game;

namespace IL2DCE.MissionObjectModel
{
    public enum EStationaryType
    {
        Radar,
        Aircraft,
        Artillery,
        Flak,
        Depot,
        Ship,
        Ammo,
        Weapons,
        Car,
        ConstCar,
        Environment,
        Searchlight,
        Aeroanchored,
        Airfield,
        Unknown,
        Count,
    }

    public enum EStationaryGenerateType
    {
        Random = -2,
        Default = -1,
        Generic,
        User,
        Couunt,
    }

    public class Stationary : GroundObject
    {
        public const float DefaultSpawnZ = 0f;
        public const int DefaultClassesOption = 2;

        private static readonly List<string> Depots = new List<string>
        {
            "Stationary.Morris_CS8_tank",
            "Stationary.Morris_CS8",
            "Stationary.Opel_Blitz_fuel",
            "Stationary.Opel_Blitz_cargo",
        };

        private static readonly List<string> Aircrafts = new List<string>
        {
            "Stationary.AnsonMkI",
            "Stationary.BeaufighterMkIF",
            "Stationary.BeaufighterMkINF",
            "Stationary.Bf-108B-2",
            "Stationary.Bf-109E-1",
            "Stationary.Bf-109E-1B",
            "Stationary.Bf-109E-3",
            "Stationary.Bf-109E-3B",
            "Stationary.Bf-109E-4",
            "Stationary.Bf-109E-4_Late",
            "Stationary.Bf-109E-4N",
            "Stationary.Bf-109E-4N_Late",
            "Stationary.Bf-109E-4B",
            "Stationary.Bf-109E-4B_Late",
            "Stationary.Bf-110C-2",
            "Stationary.Bf-110C-4",
            "Stationary.Bf-110C-4Late",
            "Stationary.Bf-110C-4B",
            "Stationary.Bf-110C-4N",
            "Stationary.Bf-110C-4N_Late",
            "Stationary.Bf-110C-4-NJG",
            "Stationary.Bf-110C-6",
            "Stationary.Bf-110C-7",
            "Stationary.BlenheimMkI",
            "Stationary.BlenheimMkIF",
            "Stationary.BlenheimMkINF",
            "Stationary.BlenheimMkIV",
            "Stationary.BlenheimMkIVF",
            "Stationary.BlenheimMkIVNF",
            "Stationary.BlenheimMkIV_Late",
            "Stationary.BlenheimMkIVF_Late",
            "Stationary.BlenheimMkIVNF_Late",
            "Stationary.BR-20M",
            "Stationary.CR42",
            "Stationary.DefiantMkI",
            "Stationary.Do-17Z-1",
            "Stationary.Do-17Z-2",
            "Stationary.Do-215B-1",
            "Stationary.FW-200C-1",
            "Stationary.G50",
            "Stationary.GladiatorMkII",
            "Stationary.He-111H-2",
            "Stationary.He-111P-2",
            "Stationary.He-115B-2",
            "Stationary.SpitfireMkI_Heartbreaker",
            "Stationary.HurricaneMkI_dH5-20",
            "Stationary.HurricaneMkI_dH5-20_100oct",
            "Stationary.HurricaneMkI",
            "Stationary.HurricaneMkI_100oct",
            "Stationary.HurricaneMkI_100oct-NF",
            "Stationary.HurricaneMkI_FB",
            "Stationary.Ju-87B-2",
            "Stationary.Ju-88A-1",
            "Stationary.SpitfireMkI",
            "Stationary.SpitfireMkI_100oct",
            "Stationary.SpitfireMkIa",
            "Stationary.SpitfireMkIa_100oct",
            "Stationary.SpitfireMkIIa",
            "Stationary.SunderlandMkI",
            "Stationary.DH82A",
            "Stationary.WalrusMkI",
            "Stationary.WellingtonMkIc",
            "Stationary.Su-26M",
            "tobruk:Stationary.BeaufighterMkIC",
            "tobruk:Stationary.BeaufighterMkIC_trop",
            "tobruk:Stationary.BeaufighterMkIF_Late",
            "tobruk:Stationary.BeaufighterMkIF_Late_trop",
            "tobruk:Stationary.BeaufighterMkINF_Late",
            "tobruk:Stationary.BeaufighterMkINF_Late_trop",
            "tobruk:Stationary.GladiatorMkII_trop",
            "tobruk:Stationary.HurricaneMkIIa",
            "tobruk:Stationary.HurricaneMkIIaTrop",
            "tobruk:Stationary.HurricaneMkIIb",
            "tobruk:Stationary.HurricaneMkIIb-Late",
            "tobruk:Stationary.HurricaneMkIIbTrop",
            "tobruk:Stationary.HurricaneMkIIbTrop-Late",
            "tobruk:Stationary.HurricaneMkIIc",
            "tobruk:Stationary.HurricaneMkIIc-Late",
            "tobruk:Stationary.HurricaneMkIIc-Trop",
            "tobruk:Stationary.HurricaneMkIIc-Trop-Late",
            "tobruk:Stationary.HurricaneMkIId",
            "tobruk:Stationary.HurricaneMkIId-Trop",
            "tobruk:Stationary.HurricaneMkI_FB-Trop",
            "tobruk:Stationary.KittyhawkMkIA",
            "tobruk:Stationary.KittyhawkMkIA-trop",
            "tobruk:Stationary.MartletMkIII",
            "tobruk:Stationary.MartletMkIII_Trop",
            "tobruk:Stationary.SpitfireMkIIb",
            "tobruk:Stationary.SpitfireMkVa",
            "tobruk:Stationary.SpitfireMkVb",
            "tobruk:Stationary.SpitfireMkVb-HF",
            "tobruk:Stationary.SpitfireMkVb-HF-Late",
            "tobruk:Stationary.SpitfireMkVb-HF-Trop",
            "tobruk:Stationary.SpitfireMkVbLate",
            "tobruk:Stationary.SpitfireMkVbTrop",
            "tobruk:Stationary.TomahawkMkII",
            "tobruk:Stationary.TomahawkMkII-Late",
            "tobruk:Stationary.TomahawkMkII-Late-trop",
            "tobruk:Stationary.TomahawkMkII-trop",
            "tobruk:Stationary.D520_Serie1",
            "tobruk:Stationary.D520_Serie1_trop",
            "tobruk:Stationary.SunderlandMkI_trop",
            "tobruk:Stationary.BlenheimMkIVF_Late_Trop",
            "tobruk:Stationary.BlenheimMkIVNF_Late_Trop",
            "tobruk:Stationary.BlenheimMkIV_Late_Trop",
            "tobruk:Stationary.BlenheimMkIV_Trop",
            "tobruk:Stationary.BlenheimMkI_Trop",
            "tobruk:Stationary.WalrusMkI_trop",
            "tobruk:Stationary.WellingtonMkIa_trop",
            "tobruk:Stationary.WellingtonMkIc_Late",
            "tobruk:Stationary.WellingtonMkIc_Late_trop",
            "tobruk:Stationary.WellingtonMkIC_t",
            "tobruk:Stationary.WellingtonMkIc_Torpedo",
            "tobruk:Stationary.WellingtonMkIc_Torpedo_trop",
            "tobruk:Stationary.WellingtonMkIc_trop",
            "tobruk:Stationary.DH82A_Trop",
            "tobruk:Stationary.Bf-109F-4",
            "tobruk:Stationary.Bf-109E-7",
            "tobruk:Stationary.Bf-109E-7N",
            "tobruk:Stationary.Bf-109E-7N_Trop",
            "tobruk:Stationary.Bf-109E-7Z",
            "tobruk:Stationary.Bf-109E-7_Trop",
            "tobruk:Stationary.Bf-109F-1",
            "tobruk:Stationary.Bf-109F-2",
            "tobruk:Stationary.Bf-109F-2_Late",
            "tobruk:Stationary.Bf-109F-2_Trop",
            "tobruk:Stationary.Bf-109F-4",
            "tobruk:Stationary.Bf-109F-4Z",
            "tobruk:Stationary.Bf-109F-4Z_Trop",
            "tobruk:Stationary.Bf-109F-4_Derated",
            "tobruk:Stationary.Bf-109F-4_Trop",
            "tobruk:Stationary.Bf-109F-4_Trop_Derated",
            "tobruk:Stationary.Bf-110C-4B_Trop",
            "tobruk:Stationary.Bf-110C-4N-NJG_Trop",
            "tobruk:Stationary.Bf-110C-6_Trop",
            "tobruk:Stationary.Bf-110C-7_Trop",
            "tobruk:Stationary.Ju-87B-2_Trop",
            "tobruk:Stationary.He-111H-2_Trop",
            "tobruk:Stationary.He-111H-6",
            "tobruk:Stationary.He-111H-6_Trop",
            "tobruk:Stationary.Ju-88A-5",
            "tobruk:Stationary.Ju-88A-5Late",
            "tobruk:Stationary.Ju-88A-5Late_Trop",
            "tobruk:Stationary.Ju-88A-5_Trop",
            "tobruk:Stationary.Ju-88C-2",
            "tobruk:Stationary.Ju-88C-2Late",
            "tobruk:Stationary.Ju-88C-2_Trop",
            "tobruk:Stationary.Ju-88C-4",
            "tobruk:Stationary.Ju-88C-4Late",
            "tobruk:Stationary.Ju-88C-4Late_Trop",
            "tobruk:Stationary.Ju-88C-4_Trop",
            "tobruk:Stationary.CR42_Trop",
            "tobruk:Stationary.G50_Trop",
            "tobruk:Stationary.Macchi-C202-SeriesIII",
            "tobruk:Stationary.Macchi-C202-SeriesIII-AltoQuota",
            "tobruk:Stationary.Macchi-C202-SeriesVII",
            "tobruk:Stationary.Macchi-C202-SeriesVII-AltoQuota",
            "tobruk:Stationary.BR-20M_Trop"
        };

        public static readonly string[][] DefaultClasses = new string[(int)EStationaryType.Count][]
        {
            new string [] { "Stationary.Radar.EnglishRadar1", "Stationary.Radar.Wotan_II", "",}, // Radar
            new string [] { "Stationary.HurricaneMkI_dH5-20", "Stationary.Bf-109E-1", "",}, // Aircraft
            new string [] { "Artillery.Bofors", "Artillery.4_cm_Flak_28", "/timeout 0/radius_hide 0", }, // Artillery
            new string [] { "Stationary.Bofors_ENG_Transport", "Stationary.Bofors_GER_Transport", "", }, // Flak
            new string [] { "Stationary.Morris_CS8_tank", "Stationary.Opel_Blitz_fuel", "",}, // Depot
            new string [] { "ShipUnit.Tanker_Medium1", "ShipUnit.Tanker_Medium2", "/sleep 0/skill 0/slowfire 1", }, // Ship
            new string [] { "Stationary.Ammo_Vehicles.40mm_Bofors_UK1", "Stationary.Ammo_Vehicles.2cmFlack38_40_GER1", "",}, // Ammo
            new string [] { "Stationary.Weapons_.Bomb_B_GP_500lb_MkIV", "Stationary.Weapons_.Bomb_B_SC-500_GradeIII_J", "",}, // Weapons 
            new string [] { "Stationary.Austin_K2_Ambulance", "Stationary.BMW_R71_w_MG_34", "",}, // Car
            new string [] { "Stationary.Fordson_N", "Stationary.Kubelwagen", "",}, // ConstCar
            new string [] { "Stationary.Environment.Portable_Siren_UK1", "Stationary.Environment.TentZeltbahn_GER1", "",}, // Environment
            new string [] { "Searchlight.90_cm_SearchLight_UK1", "Searchlight.60_cm_Flakscheinwerfer_GER1", "",}, // Searchlight
            new string [] { "Aeroanchored.Balloon_winch_UK1", "Aeroanchored.Balloon_winch_GER1", "",}, // Aeroanchored
            new string [] { "Stationary.Airfield.BombLoadingCart_UK1", "Stationary.Airfield.HydraulicBombLoader_GER1", "",}, // Airfield
            new string [] { "Stationary.Environment.TentSmall_UK1", "Stationary.Environment.TentZeltbahn_GER1", "",}, // Unknown
        };

        public EStationaryType Type
        {
            get;
            private set;
        }

        public virtual string Options
        {
            get;
            protected set;
        }

        public string DisplayName
        {
            get
            {
                return Class.Replace(".", " ");
            }
        }

        public Stationary(string id, string @class, int army, ECountry country, double x, double y, double direction, string options = null)
            : base (id, @class, army, country, x, y, direction)
        {
            Type = ParseType(Class);
            Options = options;
        }

        public static Stationary Create(ISectionFile sectionFile, string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                string value = sectionFile.get(MissionFile.SectionStationary, id);
                if (!string.IsNullOrEmpty(value))
                {
                    string[] valueParts = value.Split(MissionFile.SplitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (valueParts.Length > 4)
                    {
                        // Class
                        string @class = valueParts[0];

                        ECountry country = ParseCountry(valueParts[1]);

                        EArmy army = MissionObjectModel.Army.Parse(country);

                        double x;
                        double y;
                        double direction;
                        if (double.TryParse(valueParts[2], NumberStyles.Float, Config.NumberFormat, out x) &&
                            double.TryParse(valueParts[3], NumberStyles.Float, Config.NumberFormat, out y) &&
                            double.TryParse(valueParts[4], NumberStyles.Float, Config.NumberFormat, out direction))
                        {
                            StringBuilder options = new StringBuilder();
                            if (valueParts.Length > 5)
                            {
                                for (int i = 5; i < valueParts.Length; i++)
                                {
                                    options.Append(valueParts[i]);
                                    options.Append(" ");
                                }
                            }

                            string option = options.ToString();
                            EStationaryType type = Stationary.ParseType(@class);
                            if (type == EStationaryType.Ship && options.Length > 0)
                            {
                                ShipOption shipOption = ShipOption.Create(option);
                                if (shipOption != null)
                                {
                                    return new ShipUnit(id, @class, (int)army, country, x, y, direction, option, shipOption);
                                }
                                else
                                {
                                    Debug.Assert(false, "Invalid Ship Option", "Id={0} Option={1}", id, options);
                                    Debug.WriteLine("Invalid Ship Option", "Id={0} Option={1}", id, options);
                                }
                            }
                            else
                            {
                                ArtilleryOption artilleryOption = ArtilleryOption.Create(option);
                                if (artilleryOption != null)
                                {
                                    return new Artillery(id, @class, (int)army, country, x, y, direction, options.ToString().TrimEnd(), artilleryOption);
                                }
                                else
                                {
                                    return new Stationary(id, @class, (int)army, country, x, y, direction, options.ToString().TrimEnd());
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static ECountry ParseCountry(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                ECountry country;
                if (Enum.TryParse(str, true, out country))
                {
                    return country;
                }
                string[] strs = str.Split("_-.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length >= 1)
                {
                    if (Enum.TryParse(strs[0], true, out country))
                    {
                        return country;
                    }
                }
            }
            Debug.Assert(false, "Parse Country");
            return ECountry.nn;
        }

        public static EStationaryType ParseType(string classString)
        {
            int idx = classString.IndexOf(":");
            if (idx != -1)
            {
                classString = classString.Substring(idx + 1);
            }

            if (classString.StartsWith("Stationary.Radar"))
            {
                return EStationaryType.Radar;
            }
            else if (classString.StartsWith("Artillery"))
            {
                return EStationaryType.Artillery;
            }
            else if (classString.StartsWith("Stationary.Bofors"))
            {
                return EStationaryType.Flak;
            }
            else if (Aircrafts.Contains(classString))
            {
                return EStationaryType.Aircraft;
            }
            else if (Depots.Contains(classString))
            {
                return EStationaryType.Depot;
            }
            else if (classString.StartsWith("Ship"))
            {
                return EStationaryType.Ship;
            }
            else if (classString.StartsWith("Stationary.Ammo"))
            {
                return EStationaryType.Ammo;
            }
            else if (classString.StartsWith("Stationary.Weapons"))
            {
                return EStationaryType.Weapons;
            }
            else if (classString.StartsWith("Stationary.Opel") || classString.StartsWith("Stationary.Ford") || classString.StartsWith("Stationary.BMW") || classString.StartsWith("Stationary.Albion") ||
                    classString.StartsWith("Stationary.Renault") || classString.StartsWith("Stationary.Krupp") || classString.StartsWith("Stationary.MG") || classString.StartsWith("Stationary.Guy") ||
                    classString.StartsWith("Stationary.Austin") || classString.StartsWith("Stationary.Morris") || classString.StartsWith("Stationary.Bedford") || classString.StartsWith("Stationary.AEC_Matador)") ||
                    classString.StartsWith("Stationary.Scammell") || classString.StartsWith("Stationary.Horch"))
            {
                return EStationaryType.Car;
            }
            else if (classString.StartsWith("Stationary.Fordson") || classString.StartsWith("Stationary.Unic") || classString.StartsWith("Stationary.Kubelwagen"))
            {
                return EStationaryType.ConstCar;
            }
            else if (classString.StartsWith("Stationary.Environment"))
            {
                return EStationaryType.Environment;
            }
            else if (classString.StartsWith("Searchlight"))
            {
                return EStationaryType.Searchlight;
            }
            else if (classString.StartsWith("Aeroanchored"))
            {
                return EStationaryType.Aeroanchored;
            }
            else if (classString.StartsWith("Airfield"))
            {
                return EStationaryType.Airfield;
            }

            return EStationaryType.Unknown;
        }

        public static LandTypes[] GetLandTypes(EStationaryType type)
        {
            if (type == EStationaryType.Car || type == EStationaryType.ConstCar)
            {
                return new LandTypes[] { LandTypes.ROAD, LandTypes.HIGHWAY, LandTypes.NONE, };
            }
            else if (type == EStationaryType.Ship)
            {
                return new LandTypes[] { LandTypes.WATER, };
            }
            else/* if (groupType == EGroundGroupType.Unknown)*/
            {
                return new LandTypes[] { LandTypes.NONE, };
            }
        }

        public void UpdateId(string id)
        {
            // Debug.WriteLine("Stationary.UpdateId(Id:{0} -> {1})", Id, id);
            if (!string.IsNullOrEmpty(id))
            {
                Id = id;
            }
        }

        public void UpdateArmy(int army)
        {
            // Debug.WriteLine("Stationary.UpdateArmy(Army:{0} -> {1}, Country:{2}", Army, army, Country);
            Army = army;
            if (army != (int)MissionObjectModel.Army.Parse(Country))
            {
                Country = MissionObjectModel.Army.DefaultCountry((EArmy)army);
            }
        }

        public void UpdateIdArmy(int army, string id)
        {
            // Debug.WriteLine("Stationary.UpdateIdArmy(Army:{0} -> {1}, Country:{2} Id:{3} -> {4})", Army, army, Country, Id, id);
            UpdateId(id);
            UpdateArmy(army);
        }

        public virtual void WriteTo(ISectionFile sectionFile)
        {
            string value = string.Format(Config.NumberFormat, "{0} {1} {2:F2} {3:F2} {4:F2} {5}", Class, Country.ToString(), X, Y, Direction, Options ?? string.Empty);
            sectionFile.add("Stationary", Id, value.TrimEnd());
        }
    }
 }