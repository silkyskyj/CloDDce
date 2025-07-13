// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC
// Copyright (C) 2016 Stefan Rothdach & 2025 silkysky
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

namespace IL2DCE.MissionObjectModel
{
    public class Rank
    {
        public const int RankMin = 0;
        public const int RankMax = 5;

        public static readonly string[] Raf = new string[] {
            "Pilot Officer",
            "Flying Officer",
            "Flight Lieutenant",
            "Squadron Leader",
            "Wing Commander",
            "Group Captain",
        };

        public static readonly string[] Lw = new string[] {
            "Leutnant",
            "Oberleutnant",
            "Hauptmann",
            "Major",
            "Oberstleutnant",
            "Oberst",
        };

        public static readonly string[] Ra = new string[] {
            "Sottotenente",
            "Tenente",
            "Capitano",
            "Maggiore",
            "Tenente Colonnello",
            "Colonnello",
        };

        public static readonly string[] Aa = new string[] {
            "Sous-Lieutenant",
            "Lieutenant",
            "Capitaine",
            "Commandant",
            "Lieutenant-Colonel",
            "Colonel",
        };

        public static readonly string[] Usaaf = new string[] {
            "Flight Officer",
            "Lieutenant",
            "Captain",
            "Major",
            "Lt. Colonel",
            "Colonel",
        };

        public static readonly string[] Ru = new string[] {
            "Leitenant",
            "Starshiy Leitenant",
            "Kapitan",
            "Maior",
            "Podpolkovnik",
            "Polkovnik",
        };

        public static readonly string[] Pl = new string[] {
            "Podporucznik",
            "Porucznik",
            "Kapitan",
            "Major",
            "Podpułkownik",
            "Pułkownik",
        };

        public static readonly string[] Cz = new string[] {
            "Rotník",
            "Zástavník",
            "Porucík let.",
            "Nadporucík let.",
            "Stotník let.",
            "Major let.",
        };

        public static readonly string[] Nl = new string[] {
            "Sergeant",
            "Sergeant-Majoor",
            "Wachtmeester",
            "Luitenant",
            "Kapitein",
            "Majoor",
        };

        public static readonly string[] Be = new string[] {
            "Sergeant",
            "Sergeant-Majoor",
            "Onderluitenant",
            "Luitenant",
            "Kapitein",
            "Majoor",
        };

        public static readonly string[] Hu = new string[] {
            "Hadnagy",
            "Főhadnagy",
            "Százados",
            "Őrnagy",
            "Alezredes",
            "Ezredes",
        };

        public static readonly string[] Ro = new string[] {
            "Sublocotenent",
            "Locotenent",
            "Capitan",
            "Locotenent-comandor",
            "Capitan-comandor",
            "Comandor",
        };

        public static readonly string[] Fi = new string[] {
            "Ylikersantti",
            "Vääpeli",
            "Lentomestari",
            "Vänrikki",
            "Luutnantti",
            "Kapteeni",
        };

        public static readonly string[] Sv = new string[] {
            "Rotník",
            "Zástavník",
            "Porucík let.",
            "Nadporucík let.",
            "Stotník let.",
            "Major let.",
        };

        public static readonly string[] Bg = new string[] {
            "Leytenant",
            "Starsi Leytenant",
            "Kaptain",
            "Mayor",
            "Podpolkovnik",
            "Polkovnik",
        };

        public static readonly string[] Nd = new string[] {
            "Poručnik",
            "Natporučnik",
            "Satnik",
            "Nadsatnik",
            "Podpukovnik",
            "Pukovnik",
        };
    }
}
