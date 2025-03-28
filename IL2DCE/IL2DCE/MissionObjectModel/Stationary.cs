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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using maddox.game;
using maddox.GP;

namespace IL2DCE.MissionObjectModel
{
    public enum EStationaryType
    {
        Radar,
        Aircraft,
        Artillery,
        Depot,
        Ship,
        Ammo,
        Weapons,
        Car,
        ConstCar,
        Environment,
        Unknown,
    }

    public class Stationary
    {
        private static readonly List<string> _depots = new List<string>
        {
            "Stationary.Morris_CS8_tank",
            "Stationary.Morris_CS8",
            "Stationary.Opel_Blitz_fuel",
            "Stationary.Opel_Blitz_cargo",
        };

        private static readonly List<string> _aircrafts = new List<string>
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

        public string Id
        {
            get;
            private set;
        }

        public double X
        {
            get;
            private set;
        }

        public double Y
        {
            get;
            private set;
        }

        public double Direction
        {
            get;
            private set;
        }

        public string Class
        {
            get;
            private set;
        }

        public ECountry Country
        {
            get;
            private set;
        }

        public Point2d Position
        {
            get
            {
                return new Point2d(this.X, this.Y);
            }
        }

        public EStationaryType Type
        {
            get
            {
                // Type
                if (!string.IsNullOrEmpty(Class))
                {
                    if (Class.StartsWith("Stationary.Radar"))
                    {
                        return EStationaryType.Radar;
                    }
                    else if (Class.StartsWith("Artillery"))
                    {
                        return EStationaryType.Artillery;
                    }
                    else if (_aircrafts.Contains(Class))
                    {
                        return EStationaryType.Aircraft;
                    }
                    else if (_depots.Contains(Class))
                    {
                        return EStationaryType.Depot;
                    }
                    else if (Class.StartsWith("Ship"))
                    {
                        return EStationaryType.Ship;
                    }
                    else if (Class.StartsWith("Stationary.Ammo"))
                    {
                        return EStationaryType.Ammo;
                    }
                    else if (Class.StartsWith("Stationary.Weapons"))
                    {
                        return EStationaryType.Weapons;
                    }
                    else if (Class.StartsWith("Stationary.Opel") || Class.StartsWith("Stationary.Ford") || Class.StartsWith("Stationary.BMW") || 
                            Class.StartsWith("Stationary.Renault") || Class.StartsWith("Stationary.Krupp") ||Class.StartsWith("Stationary.MG") || 
                            Class.StartsWith("Stationary.Austin") || Class.StartsWith("Stationary.Morris") || Class.StartsWith("Stationary.Bedford"))
                    {
                        return EStationaryType.Car;
                    }
                    else if (Class.StartsWith("Stationary.Unic") || Class.StartsWith("Stationary.Kubelwagen")) 
                    {
                        return EStationaryType.ConstCar;
                    }
                    else if (Class.StartsWith("Stationary.Environment"))
                    {
                        return EStationaryType.Environment;
                    }
                }

                return EStationaryType.Unknown;
            }
        }

        public int Army
        {
            get
            {
                if (Country == ECountry.gb || Country == ECountry.fr || Country == ECountry.us || Country == ECountry.ru || Country == ECountry.rz || Country == ECountry.pl)
                {
                    return (int)EArmy.Red;
                }
                else if (Country == ECountry.de || Country == ECountry.it || Country == ECountry.ja || Country == ECountry.ro || Country == ECountry.fi || Country == ECountry.hu)
                {
                    return (int)EArmy.Blue;
                }
                else
                {
                    return 0;
                }
            }
        }

        public string Options
        {
            get;
            private set;
        }

        public string DisplayName
        {
            get
            {
                return Class.Replace(".", " ");
            }
        }

        public Stationary(string id, string @class, ECountry country, double x, double y, double direction, string options = null)
        {
            Id = id;
            X = x;
            Y = y;
            Direction = direction;
            Class = @class;
            Country = country;
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
                        ECountry country = ParseCountry(valueParts[1]);
                        double x;
                        double y;
                        double direction;
                        if (double.TryParse(valueParts[2], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x) &&
                            double.TryParse(valueParts[3], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y) &&
                            double.TryParse(valueParts[4], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out direction))
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

                            return new Stationary(id, valueParts[0], country, x ,y, direction, options.ToString().TrimEnd());
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

        public void WriteTo(ISectionFile sectionFile)
        {
            string value = string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0} {1} {2:F2} {3:F2} {4:F2} {5}", Class, Country.ToString(), X, Y, Direction, Options ?? string.Empty);
            sectionFile.add("Stationary", Id, value.TrimEnd());
        }
    }
}