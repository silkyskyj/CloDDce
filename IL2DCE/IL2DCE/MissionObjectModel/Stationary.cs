// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2016 Stefan Rothdach
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
        Unknown,
    }

    public class Stationary
    {
        private List<string> _depots = new List<string>
        {
            "Stationary.Morris_CS8_tank",
            "Stationary.Opel_Blitz_fuel",
        };

        private List<string> _aircrafts = new List<string>
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
            get
            {
                return _id;
            }
        }
        private string _id;

        public double X;

        public double Y;

        public double Direction;

        public string Class
        {
            get;
            set;
        }

        public ECountry Country
        {
            get;
            set;
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
                else
                {
                    return EStationaryType.Unknown;
                }
            }
        }

        public int Army
        {
            get
            {
                if (Country == ECountry.gb)
                {
                    return 1;
                }
                else if (Country == ECountry.de)
                {
                    return 2;
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
            set;
        }

        public Stationary(ISectionFile sectionFile, string id)
        {
            _id = id;

            string value = sectionFile.get("Stationary", id);

            string[] valueParts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (valueParts.Length > 4)
            {
                Class = valueParts[0];
                Country = (ECountry)Enum.Parse(typeof(ECountry), valueParts[1]);
                double.TryParse(valueParts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out X);
                double.TryParse(valueParts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out Y);
                double.TryParse(valueParts[4], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out Direction);

                if (valueParts.Length > 5)
                {
                    for (int i = 5; i < valueParts.Length; i++)
                    {
                        Options += valueParts[i] + " ";
                    }
                    Options = Options.Trim();
                }
            }
        }

        public Stationary(string id, string @class, ECountry country, double x, double y, double direction, string options = null)
        {
            _id = id;
            X = x;
            Y = y;
            Direction = direction;
            Class = @class;
            Country = country;
            Options = options;
        }

        public void WriteTo(ISectionFile sectionFile)
        {
            string value = Class + " " + Country.ToString() + " " + X.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + Y.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " " + Direction.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            if (Options != null)
            {
                value += Options;
            }
            sectionFile.add("Stationary", Id, value);
        }
    }
}