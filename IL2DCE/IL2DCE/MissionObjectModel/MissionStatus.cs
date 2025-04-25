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
using System.Linq;
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;
using maddox.GP;
using static IL2DCE.MissionObjectModel.MissionStatus;

namespace IL2DCE.MissionObjectModel
{
    public class MissionStatus
    {

        #region Definition

        public static readonly char[] SplitChars = new char[] { '|' };
        public const string SplitString = "|";
        public const string SectionMain = "Main";
        public const string SectionPlayer = "Player";
        public const string SectionAirports = "Airports";
        public const string SectionAirGroups = "AirGroups";
        public const string SectionChiefs = "Chiefs";
        public const string SectionStationary = "Stationaries";
        public const string SectionAircraft = "Aircrafts";
        public const string SectionGroundActor = "GroundActors";
        public const string KeyDateTime = "DateTime";
        public const string ValueNoName = "NONAME";
        public const string ValueUnknown = "Unknown";

        public const float ResetReinForceDateRate = 0.5f;

        public enum ReinForcePart
        {
            Day,
            Hour,
            Count,
        }

        public class MissionObjBase
        {
            public string Name
            {
                get;
                set;
            }

            public bool IsAlive
            {
                get;
                set;
            }

            public double X
            {
                get;
                set;
            }

            public double Y
            {
                get;
                set;
            }

            public double Z
            {
                get;
                set;
            }

            public Point3d Point
            {
                get
                {
                    return new Point3d(X, Y, Z);
                }
            }

            public DateTime? ReinForceDate
            {
                get;
                set;
            }

            public bool IsValidPoint
            {
                get
                {
                    return X > 0 && Y > 0/*&& Z > 0*/;
                }
            }

            public bool IsSamePosition(MissionObjBase target)
            {
                return X == target.X && Y == target.Y && Z == target.Z;
            }

            public static string CreateShortName(string name)
            {
                if (name != null)
                {
                    int idx = name.IndexOf(":");
                    if (idx != -1)
                    {
                        name = name.Substring(idx + 1);
                    }
                }
                else
                {
                    name = string.Empty;
                }
                return name;
            }

            public static string CreateClassShortName(string name)
            {
                if (name != null)
                {
                    int idx = name.IndexOf(".");
                    if (idx != -1)
                    {
                        name = name.Substring(idx + 1);
                    }
                }
                else
                {
                    name = string.Empty;
                }
                return name;
            }

            public static string CreateClassShortShortName(string name)
            {
                if (name != null)
                {
                    int idx = name.IndexOf(".");
                    if (idx != -1)
                    {
                        name = name.Substring(idx + 1);
                    }
                    idx = name.IndexOf(":");
                    if (idx != -1)
                    {
                        name = name.Substring(idx + 1);
                    }
                }
                else
                {
                    name = string.Empty;
                }
                return name;
            }

            public static string CreateActorName(string name)
            {
                const string del = "bob:";
                int idx = name.IndexOf(del, StringComparison.InvariantCultureIgnoreCase);
                if (idx != -1)
                {
                    name = name.Substring(idx + del.Length);
                }
                return name;
            }

            protected bool Update(ref string target, string compare)
            {
                if ((string.IsNullOrEmpty(target) || string.Compare(target, ValueNoName, true) == 0) &&
                    (!string.IsNullOrEmpty(compare) && string.Compare(compare, ValueNoName, true) != 0))
                {
                    target = compare;
                    return true;
                }
                return false;
            }

            public virtual double ReinForceRate()
            {
                return 1.0;
            }

            public int [] ReinForceDayHour(int reinForceDay)
            {
                double rate = ReinForceRate();
                int needDay = (int)Math.Floor(reinForceDay / rate);
                int needHour = (int)Math.Floor((((reinForceDay * 100 / rate) % 100) * 24) / 100);
                return new int[(int)ReinForcePart.Count] { needDay, needHour };
            }
        }

        public class PlayerObject : MissionObjBase
        {
            public int Army
            {
                get;
                set;
            }

            public string AirGroup
            {
                get;
                set;
            }

            public string ActorName
            {
                get;
                set;
            }

            public string Class
            {
                get;
                set;
            }

            public bool IsValid
            {
                get;
                set;
            }

            public bool IsTaskComplete
            {
                get;
                set;
            }

            public static PlayerObject Create(string name, string[] values)
            {
                if (values != null && values.Length >= 11)
                {
                    EArmy army;
                    bool isValid;
                    bool isAlive;
                    bool isTaskComplete;
                    double x;
                    double y;
                    double z;
                    DateTime reinForceDate;
                    Variable.TryParse(values[4], out isValid);
                    Variable.TryParse(values[5], out isAlive);
                    Variable.TryParse(values[6], out isTaskComplete);
                    System.DateTime.TryParseExact(values[10], Config.DateTimeDefaultLongFormat, Config.DateTimeFormat, DateTimeStyles.AssumeLocal, out reinForceDate);
                    if (Enum.TryParse(values[0], true, out army) /*&& !string.IsNullOrEmpty(values[1])*/ && !string.IsNullOrEmpty(values[2]) &&
                        /* && !string.IsNullOrEmpty(values[3]) && Variable.TryParse(values[4], out isValid) && Variable.TryParse(values[5], out isAlive) && Variable.TryParse(values[6], out isTaskComplete) &&*/
                        double.TryParse(values[7], NumberStyles.Float, Config.NumberFormat, out x) && double.TryParse(values[8], NumberStyles.Float, Config.NumberFormat, out y) &&
                        double.TryParse(values[9], NumberStyles.Float, Config.NumberFormat, out z))
                    {
                        return new PlayerObject()
                        {
                            Name = name,
                            Army = (int)army,
                            AirGroup = values[1],
                            ActorName = values[2],
                            Class = values[3],
                            IsValid = isValid,
                            IsAlive = isAlive,
                            IsTaskComplete = isTaskComplete,
                            X = x,
                            Y = y,
                            Z = z,
                            ReinForceDate = reinForceDate != DateTime.MinValue ? (DateTime?)reinForceDate : null,
                        };
                    }
                    else
                    {
                        Debug.Assert(false, "Player Create");
                    }

                }

                return null;
            }

            public static PlayerObject Create(maddox.game.Player player, string playerActorName)
            {
                if (player != null)
                {
                    AiPerson person = player.PersonPrimary();
                    AiActor actor = player.Place();
                    AiAircraft aircraft = actor != null ? actor as AiAircraft : null;
                    AiAirGroup airGroup = aircraft != null ? aircraft.Group() as AiAirGroup : null;
                    string airGroupName = CreateShortName(airGroup != null ? airGroup.Name() : string.Empty);
                    string aircraftName = CreateShortName(aircraft != null ? aircraft.Name() : string.Empty);
                    Point3d pos = aircraft != null ? aircraft.Pos() : new Point3d(0, 0, 0);
                    return new PlayerObject()
                    {
                        Name = player.Name(),
                        Army = player.Army(),
                        AirGroup = airGroupName,
                        ActorName = aircraft != null ? aircraftName : playerActorName ?? string.Empty,
                        Class = aircraft != null ? CreateActorName(aircraft.InternalTypeName()) : string.Empty,
                        IsValid = aircraft != null ? aircraft.IsValid() : false,
                        IsAlive = aircraft != null ? aircraft.IsAlive() : false,
                        IsTaskComplete = aircraft != null ? aircraft.IsTaskComplete() : false,
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z,
                        ReinForceDate = null,
                    };
                }
                else
                {
                    Debug.Assert(false, "Player Create");
                }

                return null;
            }

            public override double ReinForceRate()
            {
                Debug.Assert(false, "PlayerObject.ReinForceRate()");
                return base.ReinForceRate();
            }

            public bool Update(PlayerObject playerObject)
            {
                bool updated = false;

                IsValid = playerObject.IsValid;
                IsAlive = playerObject.IsAlive;
                IsTaskComplete = playerObject.IsTaskComplete;

                string target = AirGroup;
                if (Update(ref target, playerObject.AirGroup))
                {
                    AirGroup = target;
                }

                target = ActorName;
                if (Update(ref target, playerObject.ActorName))
                {
                    ActorName = target;
                }

                target = Class;
                if (Update(ref target, playerObject.Class))
                {
                    Class = target;
                }

                if (playerObject.IsValidPoint)
                {
                    X = playerObject.X;
                    Y = playerObject.Y;
                    Z = playerObject.Z;
                }

                return updated;
            }

            public void WriteTo(ISectionFile file)
            {
                try
                {
                    string[] vals = new string[]
                        {
                            Army.ToString(Config.NumberFormat),                             // Amry
                            AirGroup ?? string.Empty,                                       // AirGroup Name
                            ActorName ?? string.Empty,                                      // ActorName
                            Class ?? string.Empty,                                          // Class
                            IsValid ? "1" : "0",                                            // Valid
                            IsAlive ? "1" : "0",                                            // Alive 
                            IsTaskComplete ? "1" : "0",                                     // TaskComplete 
                            X.ToString(Config.PointValueFormat, Config.NumberFormat),       // X
                            Y.ToString(Config.PointValueFormat, Config.NumberFormat),       // Y
                            Z.ToString(Config.PointValueFormat, Config.NumberFormat),       // Z
                            ReinForceDate.HasValue ? ReinForceDate.Value.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat): string.Empty,
                        };
                    SilkySkyCloDFile.Write(file, SectionPlayer, Name, string.Join(SplitString, vals), true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Player.WriteTo {0} {1}", ex.Message, ex.StackTrace));
                }
            }
        }

        public class AirGroupObject : MissionObjBase
        {
            public int Id
            {
                get;
                set;
            }

            public int Army
            {
                get;
                set;
            }

            public string Class
            {
                get;
                set;
            }

            public string Type
            {
                get;
                set;
            }

            public int Nums
            {
                get;
                set;
            }

            public int InitNums
            {
                get;
                set;
            }

            public int DiedNums
            {
                get;
                set;
            }

            public bool IsValid
            {
                get;
                set;
            }

            public bool IsTaskComplete
            {
                get;
                set;
            }

            public override double ReinForceRate()
            {
                AircraftType type;
                if (!Enum.TryParse<AircraftType>(Type, true, out type))
                {
                    type = AircraftType.UNKNOWN;
                }
                return AircraftObject.ReinForceRate(type);
            }

            public static AirGroupObject Create(string name, string[] values)
            {
                if (values != null && values.Length >= 13)
                {
                    EArmy army;
                    int nums;
                    int initNums;
                    int diedNums;
                    bool isValid;
                    bool isAlive;
                    bool isTaskComplete;
                    double x;
                    double y;
                    double z;
                    DateTime reinForceDate;
                    System.DateTime.TryParseExact(values[12], Config.DateTimeDefaultLongFormat, Config.DateTimeFormat, DateTimeStyles.AssumeLocal, out reinForceDate);
                    if (Enum.TryParse(values[0], true, out army)/* && !string.IsNullOrEmpty(values[1]) && !string.IsNullOrEmpty(values[2])*/ && int.TryParse(values[3], out nums) && 
                        int.TryParse(values[4], out initNums) && int.TryParse(values[5], out diedNums) && Variable.TryParse(values[6], out isValid) && Variable.TryParse(values[7], out isAlive) &&
                        Variable.TryParse(values[8], out isTaskComplete) && double.TryParse(values[9], NumberStyles.Float, Config.NumberFormat, out x) &&
                        double.TryParse(values[10], NumberStyles.Float, Config.NumberFormat, out y) && double.TryParse(values[11], NumberStyles.Float, Config.NumberFormat, out z))
                    {
                        return new AirGroupObject()
                        {
                            Id = 0,
                            Name = name,
                            Army = (int)army,
                            Class = values[1],
                            Type = values[2],
                            Nums = nums,
                            InitNums = initNums,
                            DiedNums = diedNums,
                            IsValid = isValid,
                            IsAlive = isAlive,
                            IsTaskComplete = isTaskComplete,
                            X = x,
                            Y = y,
                            Z = z,
                            ReinForceDate = reinForceDate != DateTime.MinValue ? (DateTime?)reinForceDate : null,
                        };
                    }
                    else
                    {
                        Debug.Assert(false, "AirGroup Create");
                    }
                }
                return null;
            }

            public static AirGroupObject Create(AiAirGroup airGroup)
            {
                if (airGroup != null)
                {
                    string name = CreateShortName(airGroup.Name());
                    AiAircraft aircraft = airGroup.GetItems()?.FirstOrDefault() as AiAircraft ?? null;
                    Point3d pos = airGroup.Pos();
                    return new AirGroupObject()
                    {
                        Id = airGroup.ID(),
                        Name = name,
                        Army = airGroup.Army(),
                        Class = CreateActorName(aircraft != null ? aircraft.InternalTypeName() : string.Empty),
                        Type = aircraft != null ? aircraft.Type().ToString(): string.Empty,
                        Nums = airGroup.NOfAirc,
                        InitNums = airGroup.InitNOfAirc,
                        DiedNums = airGroup.DiedAircrafts,
                        IsValid = airGroup.IsValid(),
                        IsAlive = airGroup.IsAlive(),
                        IsTaskComplete = airGroup.IsTaskComplete(),
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z,
                        ReinForceDate = null,
                    };
                }
                else
                {
                    Debug.Assert(false, "AirGroup Create");
                }
                return null;
            }

            public bool Update(AirGroupObject airGroupObject)
            {
                Debug.WriteLine("AirGroupObject.Update[{0}] Nums={1}->{2} InitNums={3}->{4} DiedNums={5}->{6}, IsValid={7}->{8}, IsAlive={9}->{10}, IsTaskComplete{11}->{12}",
                    airGroupObject.Name, Nums, airGroupObject.Nums, InitNums, airGroupObject.InitNums, DiedNums, airGroupObject.DiedNums, IsValid, airGroupObject.IsValid, IsAlive, airGroupObject.IsAlive, IsTaskComplete, airGroupObject.IsTaskComplete);
                bool updated = false;

                Nums = airGroupObject.Nums;
                InitNums = airGroupObject.InitNums;
                DiedNums = airGroupObject.DiedNums;
                IsValid = airGroupObject.IsValid;
                IsAlive = airGroupObject.IsAlive;
                IsTaskComplete = airGroupObject.IsTaskComplete;

                string target = Name;
                if (Update(ref target, airGroupObject.Name))
                {
                    Name = target;
                }

                target = Class;
                if (Update(ref target, airGroupObject.Class))
                {
                    Class = target;
                }

                target = Type;
                if (Update(ref target, airGroupObject.Type))
                {
                    Type = target;
                }

                if (airGroupObject.IsValidPoint)
                {
                    X = airGroupObject.X;
                    Y = airGroupObject.Y;
                    Z = airGroupObject.Z;
                }

                return updated;
            }

            public void WriteTo(ISectionFile file)
            {
                try
                {
                    string[] vals = new string[]
                        {
                            Army.ToString(Config.NumberFormat),
                            Class,
                            Type,
                            Nums.ToString(Config.NumberFormat),
                            InitNums.ToString(Config.NumberFormat),
                            DiedNums.ToString(Config.NumberFormat),
                            IsValid ? "1" : "0",
                            IsAlive ? "1" : "0",
                            IsTaskComplete ? "1" : "0",
                            X.ToString(Config.PointValueFormat, Config.NumberFormat),
                            Y.ToString(Config.PointValueFormat, Config.NumberFormat),
                            Z.ToString(Config.PointValueFormat, Config.NumberFormat),
                            ReinForceDate.HasValue ? ReinForceDate.Value.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat) : string.Empty,
                        };
                    SilkySkyCloDFile.Write(file, SectionAirGroups, Name, string.Join(SplitString, vals), true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("AirGroup.WriteTo {0} {1}", ex.Message, ex.StackTrace));
                }
            }
        }

        public class GroundGroupObject : MissionObjBase
        {
            public string Id
            {
                get;
                set;
            }

            public int Army
            {
                get;
                set;
            }

            public string Class
            {
                get;
                set;
            }

            public string Type
            {
                get;
                set;
            }

            public int Nums
            {
                get;
                set;
            }

            public int AliveNums
            {
                get;
                set;
            }

            public bool IsValid
            {
                get;
                set;
            }

            public bool IsTaskComplete
            {
                get;
                set;
            }

            public override double ReinForceRate()
            {
                AiGroundActorType type;
                if (!Enum.TryParse<AiGroundActorType>(Type, true, out type))
                {
                    type = AiGroundActorType.Unknown;
                }
                return ReinForceRate(type);
            }

            public static double ReinForceRate(AiGroundActorType type)
            {
                return GroundObject.ReinForceRate(type);
            }

            public static GroundGroupObject Create(string name, string[] values)
            {
                if (values != null && values.Length >= 12)
                {
                    EArmy army;
                    int nums;
                    int aliveNums;
                    bool isValid;
                    bool isAlive;
                    bool isTaskComplete;
                    double x;
                    double y;
                    double z;
                    DateTime reinForceDate;
                    System.DateTime.TryParseExact(values[11], Config.DateTimeDefaultLongFormat, Config.DateTimeFormat, DateTimeStyles.AssumeLocal, out reinForceDate);
                    if (Enum.TryParse(values[0], true, out army) && !string.IsNullOrEmpty(values[1]) && !string.IsNullOrEmpty(values[2]) && int.TryParse(values[3], out nums) &&
                        int.TryParse(values[4], out aliveNums) && Variable.TryParse(values[5], out isValid) && Variable.TryParse(values[6], out isAlive) &&
                        Variable.TryParse(values[7], out isTaskComplete) && double.TryParse(values[8], NumberStyles.Float, Config.NumberFormat, out x) &&
                         double.TryParse(values[9], NumberStyles.Float, Config.NumberFormat, out y) && double.TryParse(values[10], NumberStyles.Float, Config.NumberFormat, out z))
                    {
                        return new GroundGroupObject()
                        {
                            Id = string.Empty,
                            Name = name,
                            Army = (int)army,
                            Class = values[1],
                            Type = values[2],
                            Nums = nums,
                            AliveNums = aliveNums,
                            IsValid = isValid,
                            IsAlive = isAlive,
                            IsTaskComplete = isTaskComplete,
                            X = x,
                            Y = y,
                            Z = z,
                            ReinForceDate = reinForceDate != DateTime.MinValue ? (DateTime?)reinForceDate : null,
                        };
                    }
                    else
                    {
                        Debug.Assert(false, "GroundGroupObject Create");
                    }

                }
                return null;
            }

            public static GroundGroupObject Create(AiGroundGroup groundGroup)
            {
                if (groundGroup != null)
                {
                    string name = CreateShortName(groundGroup.Name());
                    AiActor[] aiActors = groundGroup.GetItems();
                    AiGroundActor groundActor = aiActors != null ? aiActors.FirstOrDefault() as AiGroundActor : null;
                    Point3d pos = groundGroup.Pos();
                    return new GroundGroupObject()
                    {
                        Id = groundGroup.ID(),
                        Name = name,
                        Army = groundGroup.Army(),
                        Class = CreateActorName(groundActor != null ? groundActor.InternalTypeName() : string.Empty),
                        Type = groundActor != null ? groundActor.Type().ToString(): string.Empty,
                        Nums = aiActors != null ? aiActors.Length : 0,
                        AliveNums = aiActors != null ? aiActors.Where(x => x.IsAlive()).Count() : 0,
                        IsValid = groundGroup.IsValid(),
                        IsAlive = groundGroup.IsAlive(),
                        IsTaskComplete = groundGroup.IsTaskComplete(),
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z,
                        ReinForceDate = null,
                    };
                }
                else
                {
                    Debug.Assert(false, "GroundGroupObject Create");
                }
                return null;
            }

            public bool Update(GroundGroupObject groundGroupObject)
            {
                Debug.WriteLine("GroundGroupObject.Update[{0}] Nums={1}->{2} AliveNums={3}->{4}, IsValid={5}->{6}, IsAlive={7}->{8}, IsTaskComplete{9}->{12}",
                                groundGroupObject.Name, Nums, groundGroupObject.Nums, AliveNums, groundGroupObject.AliveNums, IsValid, groundGroupObject.IsValid, IsAlive, groundGroupObject.IsAlive, IsTaskComplete, groundGroupObject.IsTaskComplete);
                bool updated = false;
                Nums = groundGroupObject.Nums;
                AliveNums = groundGroupObject.AliveNums;
                IsValid = groundGroupObject.IsValid;
                IsAlive = groundGroupObject.IsAlive;
                IsTaskComplete = groundGroupObject.IsTaskComplete;
                string target = Name;
                if (Update(ref target, groundGroupObject.Name))
                {
                    Name = target;
                }
                target = Class;
                if (Update(ref target, groundGroupObject.Class))
                {
                    Class = target;
                }
                target = Type;
                if (Update(ref target, groundGroupObject.Type))
                {
                    Type = target;
                }
                if (groundGroupObject.IsValidPoint)
                {
                    X = groundGroupObject.X;
                    Y = groundGroupObject.Y;
                    Z = groundGroupObject.Z;
                }
                return updated;
            }
            public void WriteTo(ISectionFile file)
            {
                try
                {
                    string[] vals = new string[]
                    {
                        Army.ToString(Config.NumberFormat),
                        Class,
                        Type,
                            Nums.ToString(Config.NumberFormat),
                            AliveNums.ToString(Config.NumberFormat),
                            IsValid ? "1" : "0",
                            IsAlive ? "1" : "0",
                            IsTaskComplete ? "1" : "0",
                            X.ToString(Config.PointValueFormat, Config.NumberFormat),
                            Y.ToString(Config.PointValueFormat, Config.NumberFormat),
                            Z.ToString(Config.PointValueFormat, Config.NumberFormat),
                            ReinForceDate.HasValue ? ReinForceDate.Value.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat): string.Empty,
                        };
                    SilkySkyCloDFile.Write(file, SectionChiefs, Name, string.Join(SplitString, vals), true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("GroundGroupObject.WriteTo {0} {1}", ex.Message, ex.StackTrace));
                }
            }
        }

        public class StationaryObject : MissionObjBase
        {
            public string Id
            {
                get;
                set;
            }

            public string Country
            {
                get;
                set;
            }

            public string Class
            {
                get;
                set;
            }

            public string Type
            {
                get;
                set;
            }

            public string Category
            {
                get;
                set;
            }

            public override double ReinForceRate()
            {
                AiGroundActorType type;
                if (!Enum.TryParse<AiGroundActorType>(Type, true, out type))
                {
                    type = AiGroundActorType.Unknown;
                }
                return ReinForceRate(type);
            }

            public static double ReinForceRate(AiGroundActorType type)
            {
                return GroundObject.ReinForceRate(type);
            }

            public static StationaryObject Create(string name, string[] values)
            {
                if (values != null && values.Length >= 9)
                {
                    bool isAlive;
                    double x;
                    double y;
                    double z;
                    DateTime reinForceDate;
                    System.DateTime.TryParseExact(values[8], Config.DateTimeDefaultLongFormat, Config.DateTimeFormat, DateTimeStyles.AssumeLocal, out reinForceDate);
                    if (!string.IsNullOrEmpty(values[0]) && !string.IsNullOrEmpty(values[1])/* && !string.IsNullOrEmpty(values[2])*/ && !string.IsNullOrEmpty(values[3]) &&
                        Variable.TryParse(values[4], out isAlive) && double.TryParse(values[5], NumberStyles.Float, Config.NumberFormat, out x) &&
                         double.TryParse(values[6], NumberStyles.Float, Config.NumberFormat, out y) && double.TryParse(values[7], NumberStyles.Float, Config.NumberFormat, out z))
                    {
                        return new StationaryObject()
                        {
                            Id = string.Empty,
                            Name = name,
                            Class = values[0],
                            Type = values[1],
                            Category = values[2],
                            Country = values[3],
                            IsAlive = isAlive,
                            X = x,
                            Y = y,
                            Z = z,
                            ReinForceDate = reinForceDate != DateTime.MinValue ? (DateTime?)reinForceDate : null,
                        };
                    }
                    else
                    {
                        Debug.Assert(false, "Stationary Create");
                    }
                }
                return null;
            }

            public static StationaryObject Create(GroundStationary groundStationary)
            {
                if (groundStationary != null)
                {
                    string name = CreateShortName(groundStationary.Name);
                    Point3d pos = groundStationary.pos;
                    string category = groundStationary.Category;
                    return  new StationaryObject()
                    {
                        Id = name,
                        Name = name,
                        Class = FileUtil.AsciitoUtf8String(groundStationary.Title).Replace("?", ".").Replace(" ", "."),
                        Type = groundStationary.Type.ToString(),
                        Category = category ?? string.Empty,
                        Country = groundStationary.country,
                        IsAlive = groundStationary.IsAlive,
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z,
                        ReinForceDate = null,
                    };
                }
                else
                {
                    Debug.Assert(false, "Stationary Create");
                }
                return null;
            }

            public bool Update(StationaryObject stationaryObject)
            {
                bool updated = false;

                IsAlive = stationaryObject.IsAlive;

                string target = Name;
                if (Update(ref target, stationaryObject.Name))
                {
                    Name = target;
                }

                target = Country;
                if (Update(ref target, stationaryObject.Country))
                {
                    Country = target;
                }

                target = Class;
                if (Update(ref target, stationaryObject.Class))
                {
                    Class = target;
                }

                target = Type;
                if (Update(ref target, stationaryObject.Type))
                {
                    Type = target;
                }

                target = Category;
                if (Update(ref target, stationaryObject.Category))
                {
                    Category = target;
                }

                if (stationaryObject.IsValidPoint)
                {
                    X = stationaryObject.X;
                    Y = stationaryObject.Y;
                    Z = stationaryObject.Z;
                }

                return updated;
            }

            public void WriteTo(ISectionFile file)
            {
                try
                {
                    string[] vals = new string[]
                    {
                        Class,                                              // Class
                        Type,                                               // Type
                        Category ?? string.Empty,                           // Category
                        Country,
                        IsAlive ? "1" : "0",
                        X.ToString(Config.PointValueFormat, Config.NumberFormat),
                        Y.ToString(Config.PointValueFormat, Config.NumberFormat),
                        Z.ToString(Config.PointValueFormat, Config.NumberFormat),
                        ReinForceDate.HasValue ? ReinForceDate.Value.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat): string.Empty,
                    };
                    SilkySkyCloDFile.Write(file, SectionStationary, Name, string.Join(SplitString, vals), true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Stationary.WriteTo {0} {1}", ex.Message, ex.StackTrace));
                }
            }
        }

        public class AircraftObject : MissionObjBase
        {
            public int Army
            {
                get;
                set;
            }

            public string Class
            {
                get;
                set;
            }

            public string Type
            {
                get;
                set;
            }

            public bool IsValid
            {
                get;
                set;
            }

            public bool IsTaskComplete
            {
                get;
                set;
            }

            public override double ReinForceRate()
            {
                AircraftType type;
                if (!Enum.TryParse<AircraftType>(Type, true, out type))
                {
                    type = AircraftType.UNKNOWN;
                }
                return ReinForceRate(type);
            }

            public static double ReinForceRate(AircraftType type)
            {
                double rate;
                switch (type)
                {
                    case AircraftType.Fighter:
                        rate = 1.00;
                        break;
                    case AircraftType.BNZFighter:
                        rate = 1.00;
                        break;
                    case AircraftType.TNBFighter:
                        rate = 1.33;
                        break;
                    case AircraftType.HeavyFighter:
                        rate = 1.66;
                        break;
                    case AircraftType.JaBo:
                        rate = 1.5;
                        break;
                    case AircraftType.Sturmovik:
                        rate = 1.5;
                        break;
                    case AircraftType.Bomber:
                        rate = 1.5;
                        break;
                    case AircraftType.DiveBomber:
                        rate = 1.66;
                        break;
                    case AircraftType.TorpedoBomber:
                        rate = 1.66;
                        break;
                    case AircraftType.AmphibiousPlane:
                        rate = 0.75;
                        break;
                    case AircraftType.Glider:
                        rate = 0.50;
                        break;
                    case AircraftType.SailPlane:
                        rate = 1.5;
                        break;
                    case AircraftType.Scout:
                        rate = 2.00;
                        break;
                    case AircraftType.Transport:
                        rate = 1.33;
                        break;
                    case AircraftType.Blenheim:
                        rate = 1.33;
                        break;

                    case AircraftType.UNKNOWN:
                    default:
                        rate = 1;
                        break;
                }
                return 1 / rate;
            }

            public static AircraftObject Create(string name, string[] values)
            {
                if (values != null && values.Length >= 10)
                {
                    EArmy army;
                    bool isValid;
                    bool isAlive;
                    bool isTaskComplete;
                    double x;
                    double y;
                    double z;
                    DateTime reinForceDate;
                    System.DateTime.TryParseExact(values[9], Config.DateTimeDefaultLongFormat, Config.DateTimeFormat, DateTimeStyles.AssumeLocal, out reinForceDate);
                    if (Enum.TryParse(values[0], true, out army) && !string.IsNullOrEmpty(values[1]) && !string.IsNullOrEmpty(values[2]) && Variable.TryParse(values[3], out isValid) &&
                        Variable.TryParse(values[4], out isAlive) && Variable.TryParse(values[5], out isTaskComplete) && double.TryParse(values[6], NumberStyles.Float, Config.NumberFormat, out x) &&
                        double.TryParse(values[7], NumberStyles.Float, Config.NumberFormat, out y) && double.TryParse(values[8], NumberStyles.Float, Config.NumberFormat, out z))
                    {
                        return new AircraftObject()
                        {
                            Name = name,
                            Army = (int)army,
                            Class = values[1],
                            Type = values[2],
                            IsValid = isValid,
                            IsAlive = isAlive,
                            IsTaskComplete = isTaskComplete,
                            X = x,
                            Y = y,
                            Z = z,
                            ReinForceDate = reinForceDate != DateTime.MinValue ? (DateTime?)reinForceDate : null,
                        };
                    }
                    else
                    {
                        Debug.Assert(false, "Aircraft Create");
                    }
                }
                return null;
            }

            public static AircraftObject Create(AiAircraft aiAircraft)
            {
                if (aiAircraft != null)
                {
                    string name = CreateShortName(aiAircraft.Name());
                    Point3d pos = aiAircraft.Pos();
                    return new AircraftObject()
                    {
                        Name = name,
                        Army = aiAircraft.Army(),
                        Class = CreateActorName(aiAircraft.InternalTypeName()),
                        Type = aiAircraft.Type().ToString(),
                        IsValid = aiAircraft.IsValid(),
                        IsAlive = aiAircraft.IsAlive(),
                        IsTaskComplete = aiAircraft.IsTaskComplete(),
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z,
                        ReinForceDate = null,
                    };
                }
                else
                {
                    Debug.Assert(false, "GroundActor Create");
                }
                return null;
            }

            public bool Update(AircraftObject aircraftObject)
            {
                bool updated = false;

                IsValid = aircraftObject.IsValid;
                IsAlive = aircraftObject.IsAlive;
                IsTaskComplete = aircraftObject.IsTaskComplete;

                string target = Name;
                if (Update(ref target, aircraftObject.Name))
                {
                    Name = target;
                }

                target = Class;
                if (Update(ref target, aircraftObject.Class))
                {
                    Class = target;
                }

                target = Type;
                if (Update(ref target, aircraftObject.Type))
                {
                    Type = target;
                }

                if (aircraftObject.IsValidPoint)
                {
                    X = aircraftObject.X;
                    Y = aircraftObject.Y;
                    Z = aircraftObject.Z;
                }

                return updated;
            }

            public void WriteTo(ISectionFile file)
            {
                try
                {
                    string[] vals = new string[]
                    {
                        Army.ToString(Config.NumberFormat),
                        Class,                                              // Class
                        Type,                                               // Type
                        IsValid ? "1" : "0",
                        IsAlive ? "1" : "0",
                        IsTaskComplete ? "1" : "0",
                        X.ToString(Config.PointValueFormat, Config.NumberFormat),
                        Y.ToString(Config.PointValueFormat, Config.NumberFormat),
                        Z.ToString(Config.PointValueFormat, Config.NumberFormat),
                        ReinForceDate.HasValue ? ReinForceDate.Value.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat): string.Empty,
                    };
                    SilkySkyCloDFile.Write(file, SectionAircraft, Name, string.Join(SplitString, vals), true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Aircraft.WriteTo {0} {1}", ex.Message, ex.StackTrace));
                }
            }
        }

        public class GroundObject : MissionObjBase
        {
            public int Army
            {
                get;
                set;
            }

            public string Class
            {
                get;
                set;
            }

            public string Type
            {
                get;
                set;
            }

            public bool IsValid
            {
                get;
                set;
            }

            public bool IsTaskComplete
            {
                get;
                set;
            }

            public override double ReinForceRate()
            {
                AiGroundActorType type;
                if (!Enum.TryParse<AiGroundActorType>(Type, true, out type))
                {
                    type = AiGroundActorType.Unknown;
                }
                return ReinForceRate(type);
            }

            public static double ReinForceRate(AiGroundActorType type)
            {
                double rate;
                switch (type)
                {
                    case AiGroundActorType.Medic:
                        rate = 10.00;
                        break;
                    case AiGroundActorType.Motorcycle:
                        rate = 0.17;
                        break;
                    case AiGroundActorType.ArmoredCar:
                        rate = 1.00;
                        break;
                    case AiGroundActorType.Tractor:
                        rate = 0.66;
                        break;
                    case AiGroundActorType.Car:
                        rate = 0.66;
                        break;
                    case AiGroundActorType.Amphibian:
                        rate = 5.00;
                        break;
                    case AiGroundActorType.SPG:
                        rate = 1.00;
                        break;
                    case AiGroundActorType.Tank:
                        rate = 1.33;
                        break;
                    case AiGroundActorType.Bus:
                        rate = 0.66;
                        break;
                    case AiGroundActorType.LightTruck:
                        rate = 0.60;
                        break;
                    case AiGroundActorType.Truck:
                        rate = 0.50;
                        break;
                    case AiGroundActorType.Trailer:
                        rate = 0.66;
                        break;
                    case AiGroundActorType.Balloon:
                        rate = 0.66;
                        break;
                    case AiGroundActorType.Generator:
                        rate = 5.00;
                        break;
                    case AiGroundActorType.Predictor:
                        rate = 3.00;
                        break;
                    case AiGroundActorType.Radar:
                        rate = 3.00;
                        break;
                    case AiGroundActorType.RadioBeacon:
                        rate = 0.25;
                        break;
                    case AiGroundActorType.RadioBeamProjector:
                        rate = 0.25;
                        break;
                    case AiGroundActorType.Listener:
                        rate = 3.00;
                        break;
                    case AiGroundActorType.AmmoComposition:
                        rate = 0.25;
                        break;
                    case AiGroundActorType.ContainerShort:
                        rate = 0.25;
                        break;
                    case AiGroundActorType.ContainerLong:
                        rate = 0.25;
                        break;
                    case AiGroundActorType.Artillery:
                        rate = 0.5;
                        break;
                    case AiGroundActorType.AAGun:
                        rate = 0.33;
                        break;
                    case AiGroundActorType.Plane:
                        rate = 1;
                        break;
                    case AiGroundActorType.GroundCrew:
                        rate = 1;
                        break;
                    case AiGroundActorType.EngineWagon:
                        rate = 5;
                        break;
                    case AiGroundActorType.FreightWagon:
                        rate = 5;
                        break;
                    case AiGroundActorType.PassengerWagon:
                        rate = 5;
                        break;
                    case AiGroundActorType.ShipMisc:
                        rate = 20;
                        break;
                    case AiGroundActorType.ShipTransport:
                        rate = 20;
                        break;
                    case AiGroundActorType.ShipSmallWarship:
                        rate = 30;
                        break;
                    case AiGroundActorType.ShipDestroyer:
                        rate = 50;
                        break;
                    case AiGroundActorType.ShipCruiser:
                        rate = 75;
                        break;
                    case AiGroundActorType.ShipBattleship:
                        rate = 100;
                        break;
                    case AiGroundActorType.ShipCarrier:
                        rate = 100;
                        break;
                    case AiGroundActorType.ShipSubmarine:
                        rate = 100;
                        break;
                    case AiGroundActorType.Bridge:
                        rate = 10;
                        break;
                    case AiGroundActorType.House:
                        rate = 7;
                        break;

                    case AiGroundActorType.Unknown:
                    default:
                        rate = 1;
                        break;
                }
                return 1 / rate;
            }

            public static GroundObject Create(string name, string[] values)
            {
                if (values != null && values.Length >= 10)
                {
                    EArmy army;
                    bool isValid;
                    bool isAlive;
                    bool isTaskComplete;
                    double x;
                    double y;
                    double z;
                    DateTime reinForceDate;
                    System.DateTime.TryParseExact(values[9], Config.DateTimeDefaultLongFormat, Config.DateTimeFormat, DateTimeStyles.AssumeLocal, out reinForceDate);
                    if (Enum.TryParse(values[0], true, out army) && !string.IsNullOrEmpty(values[1]) && !string.IsNullOrEmpty(values[2]) && Variable.TryParse(values[3], out isValid) &&
                        Variable.TryParse(values[4], out isAlive) && Variable.TryParse(values[5], out isTaskComplete) && double.TryParse(values[6], NumberStyles.Float, Config.NumberFormat, out x) &&
                        double.TryParse(values[7], NumberStyles.Float, Config.NumberFormat, out y) && double.TryParse(values[8], NumberStyles.Float, Config.NumberFormat, out z))
                    {
                        return new GroundObject()
                        {
                            Name = name,
                            Army = (int)army,
                            Class = values[1],
                            Type = values[2],
                            IsValid = isValid,
                            IsAlive = isAlive,
                            IsTaskComplete = isTaskComplete,
                            X = x,
                            Y = y,
                            Z = z,
                            ReinForceDate = reinForceDate != DateTime.MinValue ? (DateTime?)reinForceDate : null,
                        };
                    }
                    else
                    {
                        Debug.Assert(false, "GroundActor Create");
                    }
                }
                return null;
            }

            public static GroundObject Create(AiGroundActor aiGroundActor)
            {
                if (aiGroundActor != null)
                {
                    string name = CreateShortName(aiGroundActor.Name());
                    Point3d pos = aiGroundActor.Pos();
                    return new GroundObject()
                    {
                        Name = name,
                        Army = aiGroundActor.Army(),
                        Class = CreateActorName(aiGroundActor.InternalTypeName()),
                        Type = aiGroundActor.Type().ToString(),
                        IsValid = aiGroundActor.IsValid(),
                        IsAlive = aiGroundActor.IsAlive(),
                        IsTaskComplete = aiGroundActor.IsTaskComplete(),
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z,
                        ReinForceDate = null,
                    };
                }
                else
                {
                    Debug.Assert(false, "GroundActor Create");
                }
                return null;
            }

            public bool Update(GroundObject groundObject)
            {
                bool updated = false;

                IsValid = groundObject.IsValid;
                IsAlive = groundObject.IsAlive;
                IsTaskComplete = groundObject.IsTaskComplete;

                string target = Name;
                if (Update(ref target, groundObject.Name))
                {
                    Name = target;
                }

                target = Class;
                if (Update(ref target, groundObject.Class))
                {
                    Class = target;
                }

                target = Type;
                if (Update(ref target, groundObject.Type))
                {
                    Type = target;
                }

                if (groundObject.IsValidPoint)
                {
                    X = groundObject.X;
                    Y = groundObject.Y;
                    Z = groundObject.Z;
                }

                return updated;
            }

            public void WriteTo(ISectionFile file)
            {
                try
                {
                    string[] vals = new string[]
                    {
                        Army.ToString(Config.NumberFormat),
                        Class,                                              // Class
                        Type,                                               // Type
                        IsValid ? "1" : "0",
                        IsAlive ? "1" : "0",
                        IsTaskComplete ? "1" : "0",
                        X.ToString(Config.PointValueFormat, Config.NumberFormat),
                        Y.ToString(Config.PointValueFormat, Config.NumberFormat),
                        Z.ToString(Config.PointValueFormat, Config.NumberFormat),
                        ReinForceDate.HasValue ? ReinForceDate.Value.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat): string.Empty,
                    };
                    SilkySkyCloDFile.Write(file, SectionGroundActor, Name, string.Join(SplitString, vals), true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("GroundActor.WriteTo {0} {1}", ex.Message, ex.StackTrace));
                }
            }
        }

        #endregion

        #region Property

        public DateTime DateTime
        {
            get;
            private set;
        }

        public PlayerObject PlayerInfo
        {
            get;
            private set;
        }

        public List<AirGroupObject> AirGroups
        {
            get;
            private set;
        }

        public List<GroundGroupObject> GroundGroups
        {
            get;
            private set;
        }

        public List<StationaryObject> Stationaries
        {
            get;
            private set;
        }

        public List<AircraftObject> Aircrafts
        {
            get;
            private set;
        }

        public List<GroundObject> GroundActors
        {
            get;
            private set;
        }

        private IRandom Random
        {
            get;
            set;
        }

        #endregion

        #region Constructor

        public MissionStatus(IRandom random)
        {
            Random = random;
            DateTime = DateTime.Now;
            PlayerInfo = null;
            AirGroups = new List<AirGroupObject>();
            GroundGroups = new List<GroundGroupObject>();
            Stationaries = new List<StationaryObject>();
            Aircrafts = new List<AircraftObject>();
            GroundActors = new List<GroundObject>();
        }

        public MissionStatus(IRandom random, DateTime dateTime, PlayerObject playerInfo, IEnumerable<AirGroupObject> airGroups, IEnumerable<GroundGroupObject> groundGroups, IEnumerable<StationaryObject> stationaries, IEnumerable<AircraftObject> aircrafts, IEnumerable<GroundObject> groundActors)
        {
            Random = random;
            DateTime = dateTime;
            PlayerInfo = playerInfo;
            AirGroups = new List<AirGroupObject>(airGroups);
            GroundGroups = new List<GroundGroupObject>(groundGroups);
            Stationaries = new List<StationaryObject>(stationaries);
            Aircrafts = new List<AircraftObject>(aircrafts);
            GroundActors = new List<GroundObject>(groundActors);
        }

        #endregion

        #region Create

        public static MissionStatus Create(ISectionFile file, IRandom random)
        {
            DateTime dt;
            string dtStr = file.get(SectionMain, KeyDateTime, string.Empty);
            if (DateTime.TryParseExact(dtStr, Config.DateTimeDefaultLongFormat, Config.DateTimeFormat, DateTimeStyles.AssumeLocal, out dt))
            {
                string key;
                string value;
                int lines = file.lines(SectionPlayer);
                if (lines > 0)
                {
                    file.get(SectionPlayer, 0, out key, out value);
                    PlayerObject player = PlayerObject.Create(key, value.Split(SplitChars));

                    int i;
                    List<AirGroupObject> airGroups = new List<AirGroupObject>();
                    lines = file.lines(SectionAirGroups);
                    for (i = 0; i < lines; i++)
                    {
                        file.get(SectionAirGroups, i, out key, out value);
                        AirGroupObject airGroup = AirGroupObject.Create(key, value.Split(SplitChars));
                        if (airGroup != null)
                        {
                            airGroups.Add(airGroup);
                        }
                    }

                    List<GroundGroupObject> groundGroups = new List<GroundGroupObject>();
                    lines = file.lines(SectionChiefs);
                    for (i = 0; i < lines; i++)
                    {
                        file.get(SectionChiefs, i, out key, out value);
                        GroundGroupObject groundGroup = GroundGroupObject.Create(key, value.Split(SplitChars));
                        if (groundGroup != null)
                        {
                            groundGroups.Add(groundGroup);
                        }
                    }

                    List<StationaryObject> stationaries = new List<StationaryObject>();
                    lines = file.lines(SectionStationary);
                    for (i = 0; i < lines; i++)
                    {
                        file.get(SectionStationary, i, out key, out value);
                        StationaryObject stationary = StationaryObject.Create(key, value.Split(SplitChars));
                        if (stationary != null)
                        {
                            stationaries.Add(stationary);
                        }
                    }

                    List<AircraftObject> aircrafts = new List<AircraftObject>();
                    lines = file.lines(SectionAircraft);
                    for (i = 0; i < lines; i++)
                    {
                        file.get(SectionAircraft, i, out key, out value);
                        AircraftObject aircraft = AircraftObject.Create(key, value.Split(SplitChars));
                        if (aircraft != null)
                        {
                            aircrafts.Add(aircraft);
                        }
                    }

                    List<GroundObject> groundActors = new List<GroundObject>();
                    lines = file.lines(SectionGroundActor);
                    for (i = 0; i < lines; i++)
                    {
                        file.get(SectionGroundActor, i, out key, out value);
                        GroundObject groundActor = GroundObject.Create(key, value.Split(SplitChars));
                        if (groundActor != null)
                        {
                            groundActors.Add(groundActor);
                        }
                    }

                    if (player != null)
                    {
                        return new MissionStatus(random, dt, player, airGroups, groundGroups, stationaries, aircrafts, groundActors);
                    }
                }
            }

            return null;
        }

        #endregion

        #region Update

        public void Update(AiActor aiActor)
        {
            if (aiActor is AiAircraft)
            {
                Update(aiActor as AiAircraft);
            }
            else if (aiActor is AiGroundActor)
            {
                Update(aiActor as AiGroundActor);
            }
            else if (aiActor is AiAirGroup)
            {
                Update(aiActor as AiAirGroup);
            }
            else if (aiActor is AiGroundGroup)
            {
                Update(aiActor as AiGroundGroup);
            }
            //else if (aiActor is GroundStationary)
            //{
            //    Update(aiActor as GroundStationary);
            //}
            else if (aiActor is AiPerson)
            {
                Update(aiActor as AiPerson);
            }
        }

        public void Update(IGame game, string playerActoName, DateTime dateTime)
        {
            DateTime = dateTime;

            IPlayer iplayer = game.gameInterface.Player();
            Update(game.gpPlayer(), playerActoName);

            // AirGroup
            AiAirGroup[] aiAirGroupRed = game.gpAirGroups((int)EArmy.Red);
            if (aiAirGroupRed != null)
            {
                foreach (var item in aiAirGroupRed)
                {
                    Update(item);
                }
            }

            AiAirGroup[] aiAirGroupBlue = game.gpAirGroups((int)EArmy.Blue);
            if (aiAirGroupBlue != null)
            {
                foreach (var item in aiAirGroupBlue)
                {
                    Update(item);
                }
            }

            // GroudGroup
            AiGroundGroup[] aiGroundGroupRed = game.gpGroundGroups((int)EArmy.Red);
            if (aiGroundGroupRed != null)
            {
                foreach (var item in aiGroundGroupRed)
                {
                    Update(item);
                }
            }
            AiGroundGroup[] aiGroundGroupBlue = game.gpGroundGroups((int)EArmy.Blue);
            if (aiGroundGroupBlue != null)
            {
                foreach (var item in aiGroundGroupBlue)
                {
                    Update(item);
                }
            }

            // Stationary
            GroundStationary[] groundStationary = game.gpGroundStationarys();
            if (groundStationary != null)
            {
                foreach (var item in groundStationary)
                {
                    Update(item);
                }
            }
        }

        public void Update(maddox.game.Player player, string playerActorName)
        {
            PlayerObject playerInfoNew = PlayerObject.Create(player, playerActorName);
            if (PlayerInfo == null)
            {
                PlayerInfo = playerInfoNew;
            }
            else
            {
                PlayerInfo.Update(playerInfoNew);
            }
        }

        private void Update(AiAirGroup aiAirGroup)
        {
            AirGroupObject airGroupNew = AirGroupObject.Create(aiAirGroup);
            AirGroupObject airGroup = airGroupNew.Id > 0 ? AirGroups.Where(x => x.Id == airGroupNew.Id).FirstOrDefault() :
                !string.IsNullOrEmpty(airGroupNew.Name) ? AirGroups.Where(x => x.Name == airGroupNew.Name).FirstOrDefault() : null;
            if (airGroup == null)
            {
                AirGroups.Add(airGroupNew);
            }
            else
            {
                airGroup.Update(airGroupNew);
            }
        }

        private void Update(AiGroundGroup aiGroundGroup)
        {
            GroundGroupObject groundGroupNew = GroundGroupObject.Create(aiGroundGroup);
            GroundGroupObject groundGroup = !string.IsNullOrEmpty(groundGroupNew.Name) ? GroundGroups.Where(x => string.Compare(x.Name, groundGroupNew.Name) == 0).FirstOrDefault() : null;
            if (groundGroup == null)
            {
                GroundGroups.Add(groundGroupNew);
            }
            else
            {
                groundGroup.Update(groundGroupNew);
            }
        }

        public void Update(GroundStationary groundStationary)
        {
            StationaryObject stationaryNew = StationaryObject.Create(groundStationary);
            StationaryObject stationary = !string.IsNullOrEmpty(stationaryNew.Id) ? Stationaries.Where(x => x.Id == stationaryNew.Id).FirstOrDefault() :
                !string.IsNullOrEmpty(stationaryNew.Name) ? Stationaries.Where(x => x.Name == stationaryNew.Name).FirstOrDefault() : null;
            if (stationary == null)
            {
                Stationaries.Add(stationaryNew);
            }
            else
            {
                stationary.Update(stationaryNew);
            }
        }

        private void Update(AiAircraft aiAircraft)
        {
            Debug.WriteLine("AiAircraft Army={0}, Name={1}, TypeName={2}, Group={3}", aiAircraft.Army(), aiAircraft.Name(), aiAircraft.InternalTypeName(), aiAircraft.Group() != null ? aiAircraft.Group().Name() : string.Empty);
            AircraftObject aircraftNew = AircraftObject.Create(aiAircraft);
            AircraftObject aircraft = !string.IsNullOrEmpty(aircraftNew.Name) ? Aircrafts.Where(x => string.Compare(x.Name, aircraftNew.Name) == 0).FirstOrDefault() : null;
            if (aircraft == null)
            {
                Aircrafts.Add(aircraftNew);
            }
            else
            {
                aircraft.Update(aircraftNew);
            }

            AiAirGroup aiAirGroup = aiAircraft.AirGroup();
            if (aiAirGroup != null)
            {
                Update(aiAirGroup);
            }
            else
            {
                aiAirGroup = aiAircraft.Group() as AiAirGroup;
                if (aiAirGroup != null)
                {
                    Update(aiAirGroup);
                }
            }
        }

        private void Update(AiGroundActor aiGroundActor)
        {
            Debug.WriteLine("AiGroundActor Army={0}, Name={1}, TypeName={2}, Group={3}", aiGroundActor.Army(), aiGroundActor.Name(), aiGroundActor.InternalTypeName(), aiGroundActor.Group() != null ? aiGroundActor.Group().Name() : string.Empty);
            GroundObject groundActorNew = GroundObject.Create(aiGroundActor);
            GroundObject groundActor = !string.IsNullOrEmpty(groundActorNew.Name) ? GroundActors.Where(x => string.Compare(x.Name, groundActorNew.Name) == 0).FirstOrDefault() : null;
            if (groundActor == null)
            {
                GroundActors.Add(groundActorNew);
            }
            else
            {
                groundActor.IsValid = groundActorNew.IsValid;
                groundActor.IsAlive = groundActorNew.IsAlive;
                groundActor.IsTaskComplete = groundActorNew.IsTaskComplete;
                if (groundActorNew.IsValidPoint)
                {
                    groundActor.X = groundActorNew.X;
                    groundActor.Y = groundActorNew.Y;
                    groundActor.Z = groundActorNew.Z;
                }
            }

            AiGroundGroup aiGroundGroup = aiGroundActor.Group() as AiGroundGroup;
            if (aiGroundGroup != null)
            {
                Update(aiGroundGroup);
            }
        }

        private void Update(AiPerson aiPerson)
        {
            AiGroup aiGroup = aiPerson.Group();
            if (aiGroup != null)
            {
                if (aiGroup is AiAirGroup)
                {
                    Update(aiGroup as AiAirGroup);
                }
                else if (aiGroup is AiGroundGroup)
                {
                    Update(aiGroup as AiGroundGroup);
                }
            }
            else
            {
                AiCart aiCart = aiPerson.Cart();
                if (aiCart != null)
                {
                    if (aiCart is AiAircraft)
                    {
                        AiAircraft aiAircraf = aiCart as AiAircraft;
                        if (aiAircraf != null)
                        {
                            Update(aiAircraf);
                        }
                        //AiAirGroup aiAirGroup = aiAircraf.AirGroup();
                        //Debug.WriteLine("    AiAircraft={0}[{1}], AiAirGroup={2}", aiAircraf.Name(), aiAircraf.InternalTypeName(), aiAirGroup != null ? aiAirGroup.Name() : string.Empty);
                        //if (aiAirGroup != null)
                        //{
                        //    Update(aiAirGroup);
                        //}
                    }
                    else if (aiCart is AiGroundActor)
                    {
                        AiGroundActor aiGroundActor = aiCart as AiGroundActor;
                        if (aiGroundActor != null)
                        {
                            Update(aiGroundActor);
                        }
                        //AiGroundGroup aiGroundGroup = aiGroundActor.Group() as AiGroundGroup;
                        //Debug.WriteLine("    AiGroundActor={0}[{1}], AiGroundGroup={2}", aiGroundActor.Name(), aiGroundActor.InternalTypeName(), aiGroundGroup != null ? aiGroundGroup.Name() : string.Empty);
                        //if (aiGroundGroup != null)
                        //{
                        //    Update(aiGroundGroup);
                        //}
                    }
                }
            }
        }

        #endregion

        #region WriteTo

        public void WriteTo(ISectionFile file)
        {
            SilkySkyCloDFile.Write(file, SectionMain, KeyDateTime, DateTime.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat), true);
            if (PlayerInfo != null)
            {
                PlayerInfo.WriteTo(file);
            }
            AirGroups.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
            GroundGroups.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
            Stationaries.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
            Aircrafts.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
            GroundActors.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
        }

        public void UpdateWriteTo(ISectionFile file, int reinForceDay)
        {
            MissionStatus missionStatusPrevious = MissionStatus.Create(file, Random);

            // Player
            SilkySkyCloDFile.Write(file, SectionMain, KeyDateTime, DateTime.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat), true);
            if (PlayerInfo != null)
            {
                PlayerInfo.WriteTo(file);
            }

            if (missionStatusPrevious != null)
            {
                // AirGroup
                UpdateWriteTo(file, reinForceDay, missionStatusPrevious.AirGroups);

                // GroundGroups
                UpdateWriteTo(file, reinForceDay, missionStatusPrevious.GroundGroups);

                // Stationary
                UpdateWriteTo(file, reinForceDay, missionStatusPrevious.Stationaries);

                // Aircraft
                UpdateWriteTo(file, reinForceDay, missionStatusPrevious.Aircrafts);

                // GroundActor
                UpdateWriteTo(file, reinForceDay, missionStatusPrevious.GroundActors);
            }
            else
            {
                AirGroups.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
                GroundGroups.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
                Stationaries.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
                Aircrafts.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
                GroundActors.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
            }
        }

        private int[] GetRandomReinForceDayHour(int[] reinForceDayHour)
        {
            return new int[(int)ReinForcePart.Count] 
            {
                Random.Next((int)Math.Ceiling(reinForceDayHour[(int)ReinForcePart.Day] * 0.5), (int)Math.Ceiling(reinForceDayHour[(int)ReinForcePart.Day] * 1.5)),
                Random.Next((int)Math.Ceiling(reinForceDayHour[(int)ReinForcePart.Hour] * 0.5), (int)Math.Ceiling(reinForceDayHour[(int)ReinForcePart.Hour] * 1.5)),
            };
        }

        private void UpdateWriteTo(ISectionFile file, int reinForceDay, List<AirGroupObject> airGroups)
        {
            foreach (var item in AirGroups.OrderBy(x => x.Name))
            {
                try
                {
                    AirGroupObject previousAirGroup = airGroups.Where(x => string.Compare(x.Name, item.Name, true) == 0).FirstOrDefault();
                    if (item.InitNums > 0/* && item.Nums > 0*/)
                    {
                        int[] reinForce = GetRandomReinForceDayHour(item.ReinForceDayHour(reinForceDay));
                        float rate = (item.InitNums - item.DiedNums) / (float)item.InitNums;
                        if (previousAirGroup != null)
                        {
                            DateTime? reinForceDate = previousAirGroup.ReinForceDate;
                            if (reinForceDate == null)
                            {
                                if (rate < 1)
                                {
                                    previousAirGroup.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                                }
                            }
                            else
                            {
                                // float previouRate = (previousAirGroup.Nums - previousAirGroup.DiedNums) / previousAirGroup.Nums;
                                if (reinForceDate.Value < DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]))
                                {
                                    if (rate < ResetReinForceDateRate)
                                    {
                                        previousAirGroup.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                                    }
                                }
                            }
                            // Debug.Assert(!previousAirGroup.IsAlive, "previousAirGroup=True[{0}]", previousAirGroup.Name);
                            // previousAirGroup.IsAlive = false;//item.IsAlive;
                            if (item.IsValidPoint)
                            {
                                previousAirGroup.X = item.X;
                                previousAirGroup.Y = item.Y;
                                previousAirGroup.Z = item.Z;
                            }
                            previousAirGroup.Nums = item.Nums;
                            previousAirGroup.InitNums = item.InitNums;
                            previousAirGroup.DiedNums = item.DiedNums;
                            previousAirGroup.IsTaskComplete = item.IsTaskComplete;
                        }
                        else
                        {
                            item.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                            airGroups.Add(item);
                        }
                    }
                    else
                    {
                        if (previousAirGroup != null)
                        {
                            // previousGroundObject = item;
                            airGroups.Remove(previousAirGroup);
                        }
                        item.IsAlive = false;
                        airGroups.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error MissionStatus.UpdateWriteTo: {0} {1}", ex.Message, ex.StackTrace);
                }
            }
            airGroups.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
        }

        private void UpdateWriteTo(ISectionFile file, int reinForceDay, List<GroundGroupObject> groundGroups)
        {
            foreach (var item in GroundGroups.OrderBy(x => x.Name))
            {
                try
                {
                    GroundGroupObject previousGroundGroup = groundGroups.Where(x => string.Compare(x.Name, item.Name, true) == 0).FirstOrDefault();
                    if (item.Nums > 0/* && item.AliveNums > 0*/)
                    {
                        int[] reinForce = GetRandomReinForceDayHour(item.ReinForceDayHour(reinForceDay));
                        float rate = item.AliveNums / (float)item.Nums;
                        if (previousGroundGroup != null)
                        {
                            DateTime? reinForceDate = previousGroundGroup.ReinForceDate;
                            if (reinForceDate == null)
                            {
                                if (rate < 1)
                                {
                                    previousGroundGroup.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                                }
                            }
                            else
                            {
                                // float previouRate = (previousAirGroup.Nums - previousAirGroup.DiedNums) / previousAirGroup.Nums;
                                if (reinForceDate.Value < DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]))
                                {
                                    if (rate < ResetReinForceDateRate)
                                    {
                                        previousGroundGroup.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                                    }
                                }
                            }
                            // Debug.Assert(!previousGroundGroup.IsAlive, "previousGroundGroup=True[{0}]", previousGroundGroup.Name);
                            // previousGroundGroup.IsAlive = false;//item.IsAlive;
                            if (item.IsValidPoint)
                            {
                                previousGroundGroup.X = item.X;
                                previousGroundGroup.Y = item.Y;
                                previousGroundGroup.Z = item.Z;
                            }
                            previousGroundGroup.Nums = item.Nums;
                            previousGroundGroup.AliveNums = item.AliveNums;
                            previousGroundGroup.IsTaskComplete = item.IsTaskComplete;
                        }
                        else
                        {
                            item.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                            groundGroups.Add(item);
                        }
                    }
                    else
                    {
                        if (previousGroundGroup != null)
                        {
                            // previousGroundObject = item;
                            groundGroups.Remove(previousGroundGroup);
                        }
                        item.IsAlive = false;
                        groundGroups.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error MissionStatus.UpdateWriteTo: {0} {1}", ex.Message, ex.StackTrace);
                }
            }
            groundGroups.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
        }

        private void UpdateWriteTo(ISectionFile file, int reinForceDay, List<StationaryObject> stationaries)
        {
            foreach (var item in Stationaries.OrderBy(x => x.Name))
            {
                StationaryObject previousStationary = stationaries.Where(x => x.Country == item.Country && x.Class == item.Class && (x.Name == item.Name || x.IsSamePosition(item))).FirstOrDefault();
                if (!item.IsAlive)
                {
                    int[] reinForce = GetRandomReinForceDayHour(item.ReinForceDayHour(reinForceDay));
                    if (previousStationary != null)
                    {
                        DateTime? reinForceDate = previousStationary.ReinForceDate;
                        if (reinForceDate == null)
                        {
                            previousStationary.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                        }
                        else
                        {
                            // float previouRate = (previousAirGroup.Nums - previousAirGroup.DiedNums) / previousAirGroup.Nums;
                            if (reinForceDate.Value < DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]))
                            {
                                previousStationary.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                            }
                        }
                        // Debug.Assert(!previousStationary.IsAlive, "previousStationary=True[{0}]", previousStationary.Name);
                        previousStationary.IsAlive = false; // item.IsAlive;
                        if (item.IsValidPoint)
                        {
                            previousStationary.X = item.X;
                            previousStationary.Y = item.Y;
                            previousStationary.Z = item.Z;
                        }
                    }
                    else
                    {
                        item.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                        stationaries.Add(item);
                    }
                }
                else
                {
                    if (previousStationary != null)
                    {
                        // previousStationary = item;
                        stationaries.Remove(previousStationary);
                    }
                    stationaries.Add(item);
                }
            }
            stationaries.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
        }

        private void UpdateWriteTo(ISectionFile file, int reinForceDay, List<AircraftObject> aircrafts)
        {
            foreach (var item in Aircrafts.OrderBy(x => x.Name))
            {
                AircraftObject previousAircraft = aircrafts.Where(x => x.Army == item.Army && x.Name == item.Name && x.Class == item.Class).FirstOrDefault();
                if (!item.IsAlive)
                {
                    int[] reinForce = GetRandomReinForceDayHour(item.ReinForceDayHour(reinForceDay));
                    if (previousAircraft != null)
                    {
                        DateTime? reinForceDate = previousAircraft.ReinForceDate;
                        if (reinForceDate == null)
                        {
                            previousAircraft.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                        }
                        else
                        {
                            // float previouRate = (previousAirGroup.Nums - previousAirGroup.DiedNums) / previousAirGroup.Nums;
                            if (reinForceDate.Value < DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]))
                            {
                                previousAircraft.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                            }
                        }
                        // Debug.Assert(!previousAircraft.IsAlive, "previousAircraft=True[{0}]", previousAircraft.Name);
                        previousAircraft.IsAlive = false;// item.IsAlive;
                        if (item.IsValidPoint)
                        {
                            previousAircraft.X = item.X;
                            previousAircraft.Y = item.Y;
                            previousAircraft.Z = item.Z;
                        }
                        previousAircraft.IsTaskComplete = item.IsTaskComplete;
                    }
                    else
                    {
                        item.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                        aircrafts.Add(item);
                    }
                }
                else
                {
                    if (previousAircraft != null)
                    {
                        // previousAircraft = item;
                        aircrafts.Remove(previousAircraft);
                    }
                    aircrafts.Add(item);
                }
            }
            aircrafts.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
        }

        private void UpdateWriteTo(ISectionFile file, int reinForceDay, List<GroundObject> groundActors)
        {
            foreach (var item in GroundActors.OrderBy(x => x.Name))
            {
                GroundObject previousGroundObject = groundActors.Where(x => x.Army == item.Army && x.Class == item.Class && (x.Name == item.Name || x.IsSamePosition(item))).FirstOrDefault();
                if (!item.IsAlive)
                {
                    int[] reinForce = GetRandomReinForceDayHour(item.ReinForceDayHour(reinForceDay));
                    if (previousGroundObject != null)
                    {
                        DateTime? reinForceDate = previousGroundObject.ReinForceDate;
                        if (reinForceDate == null)
                        {
                            previousGroundObject.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                        }
                        else
                        {
                            // float previouRate = (previousAirGroup.Nums - previousAirGroup.DiedNums) / previousAirGroup.Nums;
                            if (reinForceDate.Value < DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]))
                            {
                                previousGroundObject.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                            }
                        }
                        // Debug.Assert(!previousGroundObject.IsAlive, "previousGroundObject=True[{0}]", previousGroundObject.Name);
                        previousGroundObject.IsAlive = false;//item.IsAlive;
                        if (item.IsValidPoint)
                        {
                            previousGroundObject.X = item.X;
                            previousGroundObject.Y = item.Y;
                            previousGroundObject.Z = item.Z;
                        }
                        previousGroundObject.IsTaskComplete = item.IsTaskComplete;
                    }
                    else
                    {
                        item.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                        groundActors.Add(item);
                    }
                }
                else
                {
                    if (previousGroundObject != null)
                    {
                        // previousGroundObject = item;
                        groundActors.Remove(previousGroundObject);
                    }
                    groundActors.Add(item);
                }
            }
            groundActors.OrderBy(x => x.Name).ToList().ForEach(x => x.WriteTo(file));
        }

        #endregion

        public AirGroupObject GetPlayerAirGroup()
        {
            if (PlayerInfo != null && !string.IsNullOrEmpty(PlayerInfo.AirGroup))
            {
                return AirGroups.Where(x => string.Compare(x.Name, PlayerInfo.AirGroup, true) == 0).FirstOrDefault();
            }
            return null;
        }

#if DEBUG

        [Conditional("DEBUG")]
        public static void Update(ISectionFile missionStatusFile, IGame game, string playerActorName)
        {
            // Player 
            IPlayer iplayer = game.gameInterface.Player();
            MissionStatus.WriteTo(missionStatusFile, iplayer, playerActorName);

            // Airport
            // AiAirport[] aiAirport = GamePlay.gpAirports();
            // MissionResult.WriteTo(currentStatusFile, aiAirport);

            // AirGroup
            AiAirGroup[] aiAirGroupRed = game.gpAirGroups((int)EArmy.Red);
            MissionStatus.WriteTo(missionStatusFile, aiAirGroupRed);
            AiAirGroup[] aiAirGroupBlue = game.gpAirGroups((int)EArmy.Blue);
            MissionStatus.WriteTo(missionStatusFile, aiAirGroupBlue);

            // GroudGroup
            AiGroundGroup[] aiGroundGroupRed = game.gpGroundGroups((int)EArmy.Red);
            MissionStatus.WriteTo(missionStatusFile, aiGroundGroupRed);
            AiGroundGroup[] aiGroundGroupBlue = game.gpGroundGroups((int)EArmy.Blue);
            MissionStatus.WriteTo(missionStatusFile, aiGroundGroupBlue);

            // Stationary
            GroundStationary[] groundStationary = game.gpGroundStationarys();
            MissionStatus.WriteTo(missionStatusFile, groundStationary);
        }

        #region WriteTo(Static)

        [Conditional("DEBUG")]
        public static void WriteTo(ISectionFile file, IPlayer player, string actorName)
        {
            string name = MissionObjBase.CreateShortName(player.Name());

            try
            {
                AiPerson person = player.PersonPrimary();
                AiActor actor = player.Place();
                AiAircraft aircraft = actor != null ? actor as AiAircraft : null;
                AiAirGroup airGroup = aircraft != null ? aircraft.Group() as AiAirGroup : null;
                string airGroupName = MissionObjBase.CreateShortName(airGroup != null ? airGroup.Name() : string.Empty);
                string aircraftName = MissionObjBase.CreateShortName(aircraft != null ? aircraft.Name() : string.Empty);
                Point3d pos = aircraft != null ? aircraft.Pos() : new Point3d(0, 0, 0);
                string[] vals = new string[]
                    {
                        player.Army().ToString(Config.NumberFormat),                            // Amry
                        airGroupName,                                                           // AirGroup Name
                        aircraft != null ? aircraftName: actorName ?? string.Empty,             // ActorName
                        aircraft != null ? MissionObjBase.CreateActorName(aircraft.InternalTypeName()): string.Empty,           // Class
                        aircraft != null ? aircraft.IsValid() ? "1" : "0": string.Empty,        // Valid
                        aircraft != null ? aircraft.IsAlive() ? "1" : "0": string.Empty,        // Alive 
                        aircraft != null ? aircraft.IsTaskComplete() ? "1" : "0": string.Empty, // TaskComplete 
                        pos.x.ToString(Config.PointValueFormat, Config.NumberFormat),            // X
                        pos.y.ToString(Config.PointValueFormat, Config.NumberFormat),            // Y
                        pos.z.ToString(Config.PointValueFormat, Config.NumberFormat),            // Z
                    };
                // file.add(SectionPlayer, name, string.Join(SplitString, vals));
                SilkySkyCloDFile.Write(file, SectionPlayer, name, string.Join(SplitString, vals), true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("{0} {1}", ex.Message, ex.StackTrace));
            }
        }

        [Conditional("DEBUG")]
        public static void WriteTo(ISectionFile file, IEnumerable<AiAirport> airports)
        {
            if (airports != null)
            {
                foreach (var item in airports)
                {
                    WriteTo(file, item);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void WriteTo(ISectionFile file, AiAirport airport)
        {
            string name = MissionObjBase.CreateShortName(airport.Name());

            try
            {
                Point3d pos = airport.Pos();
                string[] vals = new string[]
                    {
                      airport.Type().ToString(Config.NumberFormat),
                      airport.FieldR().ToString(Config.NumberFormat),
                      airport.ParkCountAll().ToString(Config.NumberFormat),
                      airport.ParkCountFree().ToString(Config.NumberFormat),
                      airport.IsValid() ? "1" : "0",
                      airport.IsAlive() ? "1" : "0",
                      pos.x.ToString(Config.PointValueFormat, Config.NumberFormat),
                      pos.y.ToString(Config.PointValueFormat, Config.NumberFormat),
                      pos.z.ToString(Config.PointValueFormat, Config.NumberFormat)
                    };
                // file.add(SectionAirports, name, string.Join(SplitString, vals));
                SilkySkyCloDFile.Write(file, SectionAirports, name, string.Join(SplitString, vals), true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("{0} {1}", ex.Message, ex.StackTrace));
            }
        }

        [Conditional("DEBUG")]
        public static void WriteTo(ISectionFile file, IEnumerable<AiAirGroup> airGroups)
        {
            if (airGroups != null)
            {
                foreach (var item in airGroups)
                {
                    WriteTo(file, item);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void WriteTo(ISectionFile file, AiAirGroup airGroup)
        {
            string name = MissionObjBase.CreateShortName(airGroup.Name());

            try
            {
                AiAircraft aircraft = airGroup.GetItems()?.FirstOrDefault() as AiAircraft ?? null;
                Point3d pos = airGroup.Pos();
                string[] vals = new string[]
                    {
                      airGroup.Army().ToString(Config.NumberFormat),
                      MissionObjBase.CreateActorName(aircraft.InternalTypeName()),
                      airGroup.NOfAirc.ToString(Config.NumberFormat),
                      airGroup.InitNOfAirc.ToString(Config.NumberFormat),
                      airGroup.DiedAircrafts.ToString(Config.NumberFormat),
                      airGroup.IsValid() ? "1" : "0",
                      airGroup.IsAlive() ? "1" : "0",
                      airGroup.IsTaskComplete() ? "1" : "0",
                      pos.x.ToString(Config.PointValueFormat, Config.NumberFormat),
                      pos.y.ToString(Config.PointValueFormat, Config.NumberFormat),
                      pos.z.ToString(Config.PointValueFormat, Config.NumberFormat)
                    };
                SilkySkyCloDFile.Write(file, SectionAirGroups, name, string.Join(SplitString, vals), true);
                // file.add(SectionAirGroups, name, string.Join(SplitString, vals));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("{0} {1}", ex.Message, ex.StackTrace));
            }
        }

        [Conditional("DEBUG")]
        public static void WriteTo(ISectionFile file, IEnumerable<AiGroundGroup> groundGroups)
        {
            if (groundGroups != null)
            {
                foreach (var item in groundGroups)
                {
                    WriteTo(file, item);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void WriteTo(ISectionFile file, AiGroundGroup groundGroup)
        {
            string name = MissionObjBase.CreateShortName(groundGroup.Name());

            try
            {
                AiActor[] aiActors = groundGroup.GetItems();
                AiGroundActor groundActor = aiActors != null ? aiActors.FirstOrDefault() as AiGroundActor : null;
                Point3d pos = groundGroup.Pos();
                string[] vals = new string[]
                    {
                      groundGroup.Army().ToString(Config.NumberFormat),
                      MissionObjBase.CreateActorName(groundActor.InternalTypeName()),
                      groundActor.Type().ToString(),
                      aiActors != null ? aiActors.Length.ToString(Config.NumberFormat) : "0",
                      aiActors != null ? aiActors.Where(x => x.IsAlive()).Count().ToString(Config.NumberFormat) : "0",
                      groundGroup.IsValid() ? "1" : "0",
                      groundGroup.IsAlive() ? "1" : "0",
                      groundGroup.IsTaskComplete() ? "1" : "0",
                      pos.x.ToString(Config.PointValueFormat, Config.NumberFormat),
                      pos.y.ToString(Config.PointValueFormat, Config.NumberFormat),
                      pos.z.ToString(Config.PointValueFormat, Config.NumberFormat)
                    };
                // file.add(SectionChiefs, name, string.Join(SplitString, vals));
                SilkySkyCloDFile.Write(file, SectionChiefs, name, string.Join(SplitString, vals), true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("{0} {1}", ex.Message, ex.StackTrace));
            }
        }

        [Conditional("DEBUG")]
        public static void WriteTo(ISectionFile file, IEnumerable<GroundStationary> GroundStationaies)
        {
            if (GroundStationaies != null)
            {
                foreach (var item in GroundStationaies)
                {
                    WriteTo(file, item);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void WriteTo(ISectionFile file, GroundStationary groundStationary)
        {
            string name = MissionObjBase.CreateShortName(groundStationary.Name);

            try
            {
                Point3d pos = groundStationary.pos;
                string str = groundStationary.Category;
                string[] vals = new string[]
                    {
                      FileUtil.AsciitoUtf8String(groundStationary.Title).Replace("?", ".").Replace(" ", "."),     // Class
                      groundStationary.Type.ToString(),             // Type
                      str ?? string.Empty,                          // Category
                      groundStationary.country,
                      groundStationary.IsAlive ? "1" : "0",
                      pos.x.ToString(Config.PointValueFormat, Config.NumberFormat),
                      pos.y.ToString(Config.PointValueFormat, Config.NumberFormat),
                      pos.z.ToString(Config.PointValueFormat, Config.NumberFormat)
                    };
                // file.add(SectionStationary, name, string.Join(SplitString, vals));
                SilkySkyCloDFile.Write(file, SectionStationary, name, string.Join(SplitString, vals), true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("{0} {1}", ex.Message, ex.StackTrace));
            }
        }
        #endregion

#endif

    }
}
