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

namespace IL2DCE.MissionObjectModel
{
    public enum EMissionType
    {
        //LIASON,

        [Description("Recon")]
        RECON,

        [Description("Martime Recon")]
        MARITIME_RECON,

        [Description("Armed Recon")]
        ARMED_RECON,

        [Description("Armed Martime Recon")]
        ARMED_MARITIME_RECON,

        [Description("Attack Armor")]
        ATTACK_ARMOR,

        [Description("Attack Vehicle")]
        ATTACK_VEHICLE,

        [Description("Attack Train")]
        ATTACK_TRAIN,

        [Description("Attack Ship")]
        ATTACK_SHIP,

        [Description("Attack Artillery")]
        ATTACK_ARTILLERY,

        [Description("Attack Radar")]
        ATTACK_RADAR,

        [Description("Attack Aircraft")]
        ATTACK_AIRCRAFT,

        [Description("Attack Depot")]
        ATTACK_DEPOT,

        [Description("Intercept")]
        INTERCEPT,

        //MARITIME_INTERCEPT,
        //NIGHT_INTERCEPT,

        [Description("Escort")]
        ESCORT,

        [Description("Cover")]
        COVER,
        //MARITIME_COVER,

        //SWEEP,
        //INTRUDER,
        //NIGHT_INTRUDER,
    };
}
