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
using System.Linq;
using maddox.game;

namespace IL2DCE.MissionObjectModel
{
    public enum EGroundGroupType
    {
        Vehicle,
        Armor,
        Ship,
        Train,
        Unknown,
        Count,
    }

    public enum EGroundGroupGenerateType
    {
        Random = -2,
        Default = -1,
        Generic,
        User,
        Couunt,
    }

    public class GroundGroup : GroundObject
    {
        public const float DefaultNormalMoveZ = 38.40f;

        public const int DefaultClassesOption = 2;

        public static readonly string[][] DefaultClasses = new string[(int)EGroundGroupType.Count][]
        {
            new string [] { "Vehicle.Morris_CS8", "Vehicle.Ford_G917", "/num_units 8", }, // Vehicle
            new string [] { "Armor.Cruiser_Mk_IVA", "Armor.Pz_38t", "/num_units 8", }, // Armor
            new string [] { "Ship.Tanker_Medium1", "Ship.Tanker_Medium2", "/sleep 0/skill 2/slowfire 1", }, // Ship
            new string [] { "Train.57xx_0-6-0PT_c0", "Train.BR56-00_c2", "", }, // Train
            new string [] { "", "", "",}, // Unknown
        };

        public EGroundGroupType Type
        {
            get;
            private set;
        }

        public override double X
        {
            get
            {
                return Waypoints != null && Waypoints.Any() ? Waypoints[0].X: -1;
            }
            protected set
            {
                if (value != -1 && Waypoints != null && Waypoints.Any())
                {
                    Waypoints[0].X = value;
                }
            }
        }

        public override double Y
        {
            get
            {
                return Waypoints != null && Waypoints.Any() ? Waypoints[0].Y: -1;
            }
            protected set
            {
                if (value != -1 && Waypoints != null && Waypoints.Any())
                {
                    Waypoints[0].Y = value;
                }
            }
        }

        public virtual string Options
        {
            get;
            protected set;
        }

        public List<GroundGroupWaypoint> Waypoints
        {
            get;
            private set;
        }

        public string CustomChief
        {
            get;
            private set;
        }

        public IEnumerable<string> CustomChiefValues
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

        public bool MissionAssigned
        {
            get;
            set;
        }

        public GroundGroup(string id, string @class, int army, ECountry country, string options, List<GroundGroupWaypoint> waypoints, string customChief = null, IEnumerable<string> customChiefValues = null)
            : base(id, @class, army, country, -1, -1, -1)
        {
            Type = ParseType(Class);
            Options = options;
            Waypoints = waypoints;
            CustomChief = customChief;
            CustomChiefValues = customChiefValues;

            MissionAssigned = false;
        }

        public static GroundGroup Create(ISectionFile sectionFile, string id)
        {
            string value = sectionFile.get(MissionFile.SectionChiefs, id, string.Empty);
            if (!string.IsNullOrEmpty(value))
            {
                // Class
                string @class = value.Substring(0, value.IndexOf(" "));
                value = value.Remove(0, @class.Length + 1);

                // Army
                ECountry country;
                if (Enum.TryParse(value.Substring(0, 2), true, out country))
                {
                    // Country = country;
                    value = value.Remove(0, 2);

                    EArmy army = MissionObjectModel.Army.Parse(country);

                    // Options
                    string options = value.Trim();

                    // Waypoints
                    Groundway groundway = Groundway.Create(sectionFile, id);

                    // CustomChiefs
                    string customChief = null;
                    List<string> customChiefValues = null;
                    value = sectionFile.get(MissionFile.SectionCustomChiefs, @class, string.Empty);
                    if (!string.IsNullOrEmpty(value))
                    {
                        customChief = value;
                        customChiefValues = new List<string>();
                        int lines = sectionFile.lines(@class);
                        for (int i = 0; i < lines; i++)
                        {
                            string key;
                            sectionFile.get(@class, i, out key, out value);
                            customChiefValues.Add(key);
                        }
                    }

                    if (groundway != null)
                    {
                        EGroundGroupType type = GroundGroup.ParseType(@class);
                        if (type == EGroundGroupType.Ship)
                        {
                            ShipOption shipOption = ShipOption.Create(options);
                            if (shipOption != null)
                            {
                                return new ShipGroup(id, @class, (int)army, country, options, shipOption, groundway.Waypoints, customChief, customChiefValues);
                            }
                            else
                            {
                                string msg = string.Format("Invalid Ship Option", "Id={0} Option={1}", id, options);
                                Core.WriteLog(msg);
                                Debug.Assert(false, msg);
                            }
                        }
                        else if (type == EGroundGroupType.Armor || type == EGroundGroupType.Vehicle)
                        {
                            ArmorOption armorOption = ArmorOption.Create(options);
                            return new Armor(id, @class, (int)army, country, options, armorOption, groundway.Waypoints, customChief, customChiefValues);
                        }
                        else
                        {
                            return new GroundGroup(id, @class, (int)army, country, options, groundway.Waypoints, customChief, customChiefValues);
                        }
                    }
                }
            }

            return null;
        }

        public static EGroundGroupType ParseType(string classString)
        {
            if (classString.StartsWith("Vehicle"))
            {
                return EGroundGroupType.Vehicle;
            }
            else if (classString.StartsWith("Armor"))
            {
                return EGroundGroupType.Armor;
            }
            else if (classString.StartsWith("Ship"))
            {
                return EGroundGroupType.Ship;
            }
            else if (classString.StartsWith("Train"))
            {
                return EGroundGroupType.Train;
            }
            return EGroundGroupType.Unknown;
        }

        public static LandTypes[] GetLandTypes(EGroundGroupType groupType)
        {
            if (groupType == EGroundGroupType.Vehicle || groupType == EGroundGroupType.Armor)
            {
                return new LandTypes[] { LandTypes.ROAD, LandTypes.HIGHWAY, LandTypes.RAIL, LandTypes.NONE, };
            }
            else if (groupType == EGroundGroupType.Ship)
            {
                return new LandTypes[] { LandTypes.WATER, };
            }
            else if (groupType == EGroundGroupType.Train)
            {
                return new LandTypes[] { LandTypes.RAIL, };
            }
            else/* if (groupType == EGroundGroupType.Unknown)*/
            {
                return new LandTypes[] { LandTypes.NONE, };
            }
        }

        public void UpdateId(string id)
        {
            // Debug.WriteLine("GroundGroup.UpdateId(Id:{0} -> {1})", Id, id);
            if (!string.IsNullOrEmpty(id))
            {
                Id = id;
            }
        }

        public void UpdateArmy(int army)
        {
            // Debug.WriteLine("GroundGroup.UpdateArmy(Army:{0} -> {1}, Country:{2})", Army, army, Country);
            Army = army;
            if (army != (int)MissionObjectModel.Army.Parse(Country))
            {
                Country = MissionObjectModel.Army.DefaultCountry((EArmy)army);
            }
        }

        public void UpdateIdArmy(int army, string id)
        {
            // Debug.WriteLine("GroundGroup.UpdateIdArmy(Army:{0} -> {1}, Country:{2} Id:{3} -> {4})", Army, army, Country, Id, id);
            UpdateArmy(army);
            UpdateId(id);
        }

        public virtual void WriteTo(ISectionFile sectionFile)
        {
            if (Waypoints.Count > 1)
            {
                // Chiefs
                sectionFile.add(MissionFile.SectionChiefs, Id, string.Format("{0} {1} {2}", Class, Country.ToString(), Options));

                // Road
                // Write all waypoints except for the last one.
                string section = string.Format("{0}_{1}", Id, MissionFile.SectionRoad);
                for (int i = 0; i < Waypoints.Count - 1; i++)
                {
                    if (Waypoints[i] is GroundGroupWaypointLine)
                    {
                        //if (Waypoints[i].V.HasValue)
                        //{
                            sectionFile.add(section, Waypoints[i].X.ToString(Config.PointValueFormat, Config.NumberFormat),
                                            string.Format(Config.NumberFormat, "{0:F2} {1:F2}  0 {2} {3:F2}",
                                                            Waypoints[i].Y,
                                                            (Waypoints[i] as GroundGroupWaypointLine).Z,
                                                            (Waypoints[i].SubWaypoints.Count + 2),
                                                            Waypoints[i].V.HasValue ? Waypoints[i].V.Value: 0));
                        //}
                        //else
                        //{
                        //    Debug.Assert(false);
                        //}
                    }
                    else if (Waypoints[i] is GroundGroupWaypointSpline)
                    {
                        //if (Waypoints[i].V.HasValue)
                        //{
                            sectionFile.add(section, "S",
                                            string.Format(Config.NumberFormat, "{0} P {1:F2} {2:F2}  0 {3} {4:F2}",
                                                            (Waypoints[i] as GroundGroupWaypointSpline).S,
                                                            Waypoints[i].X,
                                                            Waypoints[i].Y,
                                                            (Waypoints[i].SubWaypoints.Count + 2),
                                                            Waypoints[i].V.HasValue ? Waypoints[i].V.Value: 0));
                        //}
                        //else
                        //{
                        //    Debug.Assert(false);
                        //}
                    }

                    foreach (GroundGroupWaypoint subWaypoint in Waypoints[i].SubWaypoints)
                    {
                        if (subWaypoint is GroundGroupWaypointLine)
                        {
                            sectionFile.add(section, subWaypoint.X.ToString(Config.PointValueFormat, Config.NumberFormat),
                                string.Format(Config.NumberFormat, "{0:F2} {1:F2}", subWaypoint.Y, (subWaypoint as GroundGroupWaypointLine).Z));
                        }
                        else if (subWaypoint is GroundGroupWaypointSpline)
                        {
                            sectionFile.add(section, "S",
                                string.Format(Config.NumberFormat, "{0} P {1:F2} {2:F2}",
                                (subWaypoint as GroundGroupWaypointSpline).S, subWaypoint.X, subWaypoint.Y));
                        }
                        else
                        {
                            Debug.Assert(false);
                        }
                    }
                }

                // For the last waypoint don't write the subwaypoint count and the speed.
                GroundGroupWaypoint wayPointLast = Waypoints.Last();
                if (wayPointLast is GroundGroupWaypointLine)
                {
                    sectionFile.add(section, wayPointLast.X.ToString(Config.NumberFormat),
                                    string.Format(Config.NumberFormat, "{0:F2} {1:F2}",
                                                            wayPointLast.Y, (wayPointLast as GroundGroupWaypointLine).Z));
                }
                else if (wayPointLast is GroundGroupWaypointSpline)
                {
                    sectionFile.add(section, "S",
                                    string.Format(Config.NumberFormat, "{0} P {1:F2} {2:F2}",
                                                            (wayPointLast as GroundGroupWaypointSpline).S, wayPointLast.X, wayPointLast.Y));
                }
                else
                {
                    Debug.Assert(false);
                }

                // CustomChief
                if (!string.IsNullOrEmpty(CustomChief) && CustomChiefValues != null)
                {
                    if (!sectionFile.exist(MissionFile.SectionCustomChiefs, Class))
                    {
                        sectionFile.add(MissionFile.SectionCustomChiefs, Class, CustomChief);
                        foreach (var item in CustomChiefValues)
                        {
                            sectionFile.add(Class, item, string.Empty);
                        }
                    }
                }
            }
        }
    }
}