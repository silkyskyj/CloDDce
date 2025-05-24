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
using part;
using static IL2DCE.MissionObjectModel.MissionStatus;

namespace IL2DCE.MissionObjectModel
{

    public interface IMissionStatus
    {
        DateTime DateTime
        {
            get;
        }

        PlayerObj PlayerInfo
        {
            get;
        }

        List<AirGroupObj> AirGroups
        {
            get;
        }

        List<GroundGroupObj> GroundGroups
        {
            get;
        }

        List<StationaryObj> Stationaries
        {
            get;
        }

        List<AircraftObj> Aircrafts
        {
            get;
        }

        List<GroundObj> GroundActors
        {
            get;
        }
    }

    public class MissionStatus : IMissionStatus
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

        interface IMissionObj
        {
            string Name
            {
                get;
                set;
            }

            bool IsAlive
            {
                get;
                set;
            }

            double X
            {
                get;
                set;
            }

            double Y
            {
                get;
                set;
            }

            double Z
            {
                get;
                set;
            }

            Point3d Point
            {
                get;
            }

            DateTime? ReinForceDate
            {
                get;
                set;
            }

            bool IsValidPoint
            {
                get;
            }

            bool IsSamePosition(MissionObjBase target);
            bool Update(ref string target, string compare);
            double ReinForceRate();
            int[] ReinForceDayHour(int reinForceDay);
            void WriteTo(ISectionFile file, bool overwrite);
        }

        public class MissionObjBase : IMissionObj
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

            public string PointString
            {
                get 
                {
                    return string.Format(Config.NumberFormat, "({0:F2},{1:F2},{2:F2})", X, Y, Z);
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

            public bool Update(ref string target, string compare)
            {
                if ((string.IsNullOrEmpty(target) || string.Compare(target, ValueNoName, true) == 0) &&
                    (!string.IsNullOrEmpty(compare) && string.Compare(compare, ValueNoName, true) != 0))
                {
                    target = compare;
                    return true;
                }
                return false;
            }

            protected void UpdateName(MissionObjBase src)
            {
                string target = Name;
                if (Update(ref target, src.Name))
                {
                    Name = target;
                }
            }

            protected void UpdatePoint(MissionObjBase src)
            {
                if (src.IsValidPoint)
                {
                    CopyPoint(src);
                }
            }

            public virtual double ReinForceRate()
            {
                return 1.0;
            }

            public int[] ReinForceDayHour(int reinForceDay)
            {
                double rate = ReinForceRate();
                int needDay = (int)Math.Floor(reinForceDay / rate);
                int needHour = (int)Math.Floor((((reinForceDay * 100 / rate) % 100) * 24) / 100);
                return new int[(int)ReinForcePart.Count] { needDay, needHour };
            }

            public virtual void WriteTo(ISectionFile file, bool overwrite)
            {
                ;
            }

            public void CopyPoint(MissionObjBase src)
            {
                X = src.X;
                Y = src.Y;
                Z = src.Z;
            }

            public static string ToString(Point3d point)
            {

                return string.Format(Config.NumberFormat, "({0:F2},{1:F2},{2:F2})", point.x, point.y, point.z);

            }
        }

        public class MissionObjEx : MissionObjBase
        {
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

            public string ClassShortName
            {
                get
                {
                    return CreateClassShortShortName(Class);
                }
            }

            protected void UpdateClass(MissionObjEx src)
            {
                string target = Class;
                if (Update(ref target, src.Class))
                {
                    Class = target;
                }
            }

            protected void UpdateType(MissionObjEx src)
            {
                string target = Type;
                if (Update(ref target, src.Type))
                {
                    Type = target;
                }
            }
        }

        public class MissionActorObj : MissionObjEx
        {
            public int Army
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

            public bool IsMissionCompleted
            {
                get;
                set;
            }
        }

        public class MissionGroupObj : MissionActorObj
        {
            public int InitNums
            {
                get;
                set;
            }

            public int Nums
            {
                get;
                set;
            }

            public long TaskComplateTime
            {
                get;
                set;
            }

            public AiAirGroupTask RequestTask
            {
                get;
                set;
            }

            public MissionGroupObj()
            {
                RequestTask = AiAirGroupTask.UNKNOWN;
            }
        }

        public class PlayerObj : MissionActorObj
        {

            public string AirGroup
            {
                get;
                set;
            }

            public static PlayerObj Create(string name, string[] values)
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
                        return new PlayerObj()
                        {
                            Name = name,
                            Army = (int)army,
                            AirGroup = values[1],
                            Type = values[2],
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

            public static PlayerObj Create(maddox.game.Player player, string playerActorName)
            {
                try
                {
                    if (player != null)
                    {
                        AiPerson person = player.PersonPrimary();
#if DEBUG
                        MissionDebug.TraceAiPerson(null, person);
#endif
                        AiActor actor = player.Place();
                        AiAircraft aircraft = actor != null ? actor as AiAircraft : null;
                        AiAirGroup airGroup = aircraft != null ? aircraft.Group() as AiAirGroup : null;
                        string airGroupName = CreateShortName(airGroup != null ? airGroup.Name() : string.Empty);
                        string aircraftName = CreateShortName(aircraft != null ? aircraft.Name() : string.Empty);
                        Point3d pos = aircraft != null ? aircraft.Pos() : new Point3d(0, 0, 0);
                        return new PlayerObj()
                        {
                            Name = player.Name(),
                            Army = player.Army(),
                            AirGroup = airGroupName,
                            Type = aircraft != null ? aircraftName : playerActorName ?? string.Empty,
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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("PlayerObject.Create {0} {1}", ex.Message, ex.StackTrace));
                }

                return null;
            }

            public override double ReinForceRate()
            {
                Debug.Assert(false, "PlayerObject.ReinForceRate()");
                return base.ReinForceRate();
            }

            public bool Update(PlayerObj playerObject)
            {
                bool updated = false;
                try
                {
                    IsValid = playerObject.IsValid;
                    IsAlive = playerObject.IsAlive;
                    IsTaskComplete = playerObject.IsTaskComplete;

                    string target = AirGroup;
                    if (Update(ref target, playerObject.AirGroup))
                    {
                        AirGroup = target;
                    }

                    UpdateType(playerObject);
                    UpdateClass(playerObject);
                    UpdatePoint(playerObject);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("PlayerObject.Update {0} {1}", ex.Message, ex.StackTrace));
                }

                return updated;
            }

            public override void WriteTo(ISectionFile file, bool overwrite)
            {
                base.WriteTo(file, overwrite);

                try
                {
                    string[] vals = new string[]
                        {
                            Army.ToString(Config.NumberFormat),                             // Amry
                            AirGroup ?? string.Empty,                                       // AirGroup Name
                            Type ?? string.Empty,                                           // ActorName
                            Class ?? string.Empty,                                          // Class
                            IsValid ? "1" : "0",                                            // Valid
                            IsAlive ? "1" : "0",                                            // Alive 
                            IsTaskComplete ? "1" : "0",                                     // TaskComplete 
                            X.ToString(Config.PointValueFormat, Config.NumberFormat),       // X
                            Y.ToString(Config.PointValueFormat, Config.NumberFormat),       // Y
                            Z.ToString(Config.PointValueFormat, Config.NumberFormat),       // Z
                            ReinForceDate.HasValue ? ReinForceDate.Value.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat): string.Empty,
                        };
                    SilkySkyCloDFile.Write(file, SectionPlayer, Name, string.Join(SplitString, vals), overwrite);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Player.WriteTo {0} {1}", ex.Message, ex.StackTrace));
                }
            }
        }

        public class AirGroupObj : MissionGroupObj
        {
            public int Id
            {
                get;
                set;
            }

            public string NameItem
            {
                get;
                set;
            }

            public int DiedNums
            {
                get;
                set;
            }

            public string SquadronName
            {
                get
                {
                    return Name2SquadronName(Name);
                }
            }

            public string SquadronNameItem
            {
                get
                {
                    return Name2SquadronName(NameItem);
                }
            }

            public override double ReinForceRate()
            {
                AircraftType type;
                if (!Enum.TryParse<AircraftType>(Type, true, out type))
                {
                    type = AircraftType.UNKNOWN;
                }
                return AircraftObj.ReinForceRate(type);
            }

            public static AirGroupObj Create(string name, string[] values)
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
                        return new AirGroupObj()
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

            public static AirGroupObj Create(AiAirGroup airGroup)
            {
                try
                {
                    if (airGroup != null)
                    {
                        string name = CreateShortName(airGroup.Name());
                        AiActor[] actors = CloDAPIUtil.GetItems(airGroup);
                        AiAircraft aircraft = actors != null ? actors.FirstOrDefault() as AiAircraft : null;
                        string nameItem = aircraft != null ? CreateShortName(aircraft.Name()) : string.Empty;
                        Point3d pos = airGroup.Pos();
                        return new AirGroupObj()
                        {
                            Id = airGroup.ID(),
                            Name = name,
                            NameItem = nameItem,
                            Army = airGroup.Army(),
                            Class = CreateActorName(aircraft != null ? aircraft.InternalTypeName() : string.Empty),
                            Type = aircraft != null ? aircraft.Type().ToString() : string.Empty,
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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("AirGroupObject.Create {0} {1}", ex.Message, ex.StackTrace));
                }

                return null;
            }

            public bool Update(AirGroupObj airGroupObject)
            {
                Debug.WriteLine("  AirGroupObject.Update[Id={0,3}] Name={1,-45}[{2,-40}] Class={3,-35} Nums={4,2}->{5,2} InitNums={6,2}->{7,2} DiedNums={8,2}->{9,2}, IsValid={10,-5}->{11,-5}, IsAlive={12,-5}->{13,-5}, IsTask={14,-5}->{15,-5}",
                    airGroupObject.Id, airGroupObject.Name, airGroupObject.NameItem, airGroupObject.ClassShortName, Nums, airGroupObject.Nums, InitNums, airGroupObject.InitNums, DiedNums, airGroupObject.DiedNums, IsValid, airGroupObject.IsValid, IsAlive, airGroupObject.IsAlive, IsTaskComplete, airGroupObject.IsTaskComplete);
                bool updated = false;

                try
                {
                    // Name = airGroupObject.Name;
                    Nums = airGroupObject.Nums;
                    InitNums = airGroupObject.InitNums;
                    DiedNums = airGroupObject.DiedNums;
                    IsValid = airGroupObject.IsValid;
                    IsAlive = airGroupObject.IsAlive;
                    IsTaskComplete = airGroupObject.IsTaskComplete;
                    UpdateName(airGroupObject);

                    string target = NameItem;
                    if (Update(ref target, airGroupObject.NameItem))
                    {
                        NameItem = target;
                    }

                    UpdateClass(airGroupObject);
                    UpdateType(airGroupObject);
                    UpdatePoint(airGroupObject);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("AirGroupObject.Update {0} {1}", ex.Message, ex.StackTrace));
                }
                return updated;
            }

            public override void WriteTo(ISectionFile file, bool overwrite)
            {
                base.WriteTo(file, overwrite);

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
                    if (overwrite)
                    {
                        SilkySkyCloDFile.Write(file, SectionAirGroups, Name, string.Join(SplitString, vals), true);
                    }
                    else
                    {
                        SilkySkyCloDFile.Add(file, SectionAirGroups, Name, string.Join(SplitString, vals));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("AirGroup.WriteTo {0} {1}", ex.Message, ex.StackTrace));
                }
            }

            public static string Name2SquadronName(string name)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    int idx = name.LastIndexOf('.');
                    if (idx != -1 && idx < name.Length - 2)
                    {
                        return name.Substring(0, idx + 2);
                    }
                }
                return name;
            }
        }

        public class GroundGroupObj : MissionGroupObj
        {
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
                return GroundObj.ReinForceRate(type);
            }

            public static GroundGroupObj Create(string name, string[] values)
            {
                try
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
                            return new GroundGroupObj()
                            {
                                Name = name,
                                Army = (int)army,
                                Class = values[1],
                                Type = values[2],
                                InitNums = nums,
                                Nums = aliveNums,
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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("AiGroundGroup.Create {0} {1}", ex.Message, ex.StackTrace));
                }
                return null;
            }

            public static GroundGroupObj Create(AiGroundGroup groundGroup)
            {
                try
                {
                    if (groundGroup != null)
                    {
                        string name = CreateShortName(groundGroup.Name());
                        AiActor[] aiActors = CloDAPIUtil.GetItems(groundGroup);
                        AiGroundActor groundActor = aiActors != null && aiActors.Length > 0 ? aiActors.FirstOrDefault() as AiGroundActor : null;
                        Point3d pos = groundGroup.Pos();

                        return new GroundGroupObj()
                        {
                            Name = name,
                            Army = groundGroup.Army(),
                            Class = CreateActorName(groundActor != null ? groundActor.InternalTypeName() : string.Empty),
                            Type = groundActor != null ? groundActor.Type().ToString() : string.Empty,
                            InitNums = aiActors != null ? aiActors.Length : 0,
                            Nums = aiActors != null ? aiActors.Where(x => x.IsAlive()).Count() : 0,
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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("AiGroundGroup.Create {0} {1}", ex.Message, ex.StackTrace));
                }
                return null;
            }

            public bool Update(GroundGroupObj groundGroupObject)
            {
                //Debug.WriteLine("  GroundGroupObject.Update[{0,-15}] Class={1,-30} Nums={2,2}->{3,2} AliveNums={4,2}->{5,2}, IsValid={6,-5}->{7,-5}, IsAlive={8,-5}->{9,-5}, IsTask={10,-5}->{12,-5}",
                //                groundGroupObject.Name, groundGroupObject.Type, groundGroupObject.Class, InitNums, groundGroupObject.InitNums, Nums, groundGroupObject.Nums, IsValid, groundGroupObject.IsValid, IsAlive, groundGroupObject.IsAlive, IsTaskComplete, groundGroupObject.IsTaskComplete);
                bool updated = false;
                try
                {
                    InitNums = groundGroupObject.InitNums;
                    Nums = groundGroupObject.Nums;
                    IsValid = groundGroupObject.IsValid;
                    IsAlive = groundGroupObject.IsAlive;
                    IsTaskComplete = groundGroupObject.IsTaskComplete;
                    UpdateName(groundGroupObject);
                    UpdateClass(groundGroupObject);
                    UpdateType(groundGroupObject);
                    UpdatePoint(groundGroupObject);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("GroundGroupObject.Update {0} {1}", ex.Message, ex.StackTrace));
                }
                return updated;
            }

            public override void WriteTo(ISectionFile file, bool overwrite)
            {
                base.WriteTo(file, overwrite);

                try
                {
                    string[] vals = new string[]
                    {
                        Army.ToString(Config.NumberFormat),
                        Class,
                        Type,
                            InitNums.ToString(Config.NumberFormat),
                            Nums.ToString(Config.NumberFormat),
                            IsValid ? "1" : "0",
                            IsAlive ? "1" : "0",
                            IsTaskComplete ? "1" : "0",
                            X.ToString(Config.PointValueFormat, Config.NumberFormat),
                            Y.ToString(Config.PointValueFormat, Config.NumberFormat),
                            Z.ToString(Config.PointValueFormat, Config.NumberFormat),
                            ReinForceDate.HasValue ? ReinForceDate.Value.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat): string.Empty,
                        };
                    if (overwrite)
                    {
                        SilkySkyCloDFile.Write(file, SectionChiefs, Name, string.Join(SplitString, vals), true);
                    }
                    else
                    {
                        SilkySkyCloDFile.Add(file, SectionChiefs, Name, string.Join(SplitString, vals));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("GroundGroupObject.WriteTo {0} {1}", ex.Message, ex.StackTrace));
                }
            }
        }

        public class StationaryObj : MissionObjEx
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
                return GroundObj.ReinForceRate(type);
            }

            public static StationaryObj Create(string name, string[] values)
            {
                try
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
                            return new StationaryObj()
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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("StationaryObject.Create {0} {1}", ex.Message, ex.StackTrace));
                }
                return null;
            }

            public static StationaryObj Create(GroundStationary groundStationary)
            {
                try
                {
                    if (groundStationary != null)
                    {
                        string name = CreateShortName(groundStationary.Name);
                        Point3d pos = groundStationary.pos;
                        string category = groundStationary.Category;
                        return new StationaryObj()
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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("StationaryObject.Create {0} {1}", ex.Message, ex.StackTrace));
                }
                return null;
            }

            public bool Update(StationaryObj stationaryObject)
            {
                bool updated = false;
                try
                {
                    IsAlive = stationaryObject.IsAlive;
                    UpdateName(stationaryObject);

                    string target = Country;
                    if (Update(ref target, stationaryObject.Country))
                    {
                        Country = target;
                    }

                    UpdateClass(stationaryObject);
                    UpdateType(stationaryObject);

                    target = Category;
                    if (Update(ref target, stationaryObject.Category))
                    {
                        Category = target;
                    }

                    UpdatePoint(stationaryObject);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("StationaryObject.Update {0} {1}", ex.Message, ex.StackTrace));
                }
                return updated;
            }

            public override void WriteTo(ISectionFile file, bool overwrite)
            {
                base.WriteTo(file, overwrite);

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
                    SilkySkyCloDFile.Write(file, SectionStationary, Name, string.Join(SplitString, vals), overwrite);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Stationary.WriteTo {0} {1}", ex.Message, ex.StackTrace));
                }
            }
        }

        public class AircraftObj : MissionActorObj
        {
            public bool IsLanded
            {
                get;
                set;
            }

            public bool IsStoped
            {
                get;
                set;
            }

            public bool IsReArmed
            {
                get;
                set;
            }

            public bool IsReFueled
            {
                get;
                set;
            }

            public double StopedTime
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

            public static AircraftObj Create(string name, string[] values)
            {
                try
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
                            return new AircraftObj()
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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("AircraftObject.Create {0} {1}", ex.Message, ex.StackTrace));
                }
                return null;
            }

            public static AircraftObj Create(AiAircraft aiAircraft)
            {
                try
                {
                    if (aiAircraft != null)
                    {
                        string name = CreateShortName(aiAircraft.Name());
                        Point3d pos = aiAircraft.Pos();
                        return new AircraftObj()
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
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("AircraftObject.Create {0} {1}", ex.Message, ex.StackTrace));
                }
                return null;
            }

            public static AiAirGroupTask? GetCurrentTask(AiAircraft aiAircraft)
            {
                if (aiAircraft != null)
                {
                    AiAirGroup group = aiAircraft.Group() as AiAirGroup;
                    if (group != null)
                    {
                        return CloDAPIUtil.GetTask(group);
                    }
                }
                return null;
            }

            public static bool IsLastWaypoint(AiAircraft aiAircraft)
            {
                if (aiAircraft != null)
                {
                    AiAirGroup group = aiAircraft.Group() as AiAirGroup;
                    if (group != null)
                    {
                        return CloDAPIUtil.IsLastWaypoint(group);
                    }
                }
                return false;
            }

            public static bool IsStop(AiAircraft aiAircraft)
            {
                if (aiAircraft != null)
                {
                    Debug.WriteLine("Aircraft.IsStop Name={0}={1},{2},{3},{4},{5},{6},{7},{8}", aiAircraft.Name(),
                        aiAircraft.getParameter(ParameterTypes.C_Magneto, 0), aiAircraft.getParameter(ParameterTypes.C_Magneto, -1), aiAircraft.getParameter(ParameterTypes.C_Throttle, 0), aiAircraft.getParameter(ParameterTypes.C_Throttle, -1),
                        aiAircraft.getParameter(ParameterTypes.Z_VelocityIAS, 0), aiAircraft.getParameter(ParameterTypes.Z_VelocityIAS, -1), aiAircraft.getParameter(ParameterTypes.Z_VelocityTAS, 0), aiAircraft.getParameter(ParameterTypes.Z_VelocityTAS, -1));
                    //return aiAircraft.getParameter(ParameterTypes.C_Magneto, 0) == 0 && aiAircraft.getParameter(ParameterTypes.C_Throttle, 0) <= 0
                    //     /* && aiAircraft.getParameter(ParameterTypes.Z_VelocityIAS, 0) <= 0*/;
                    return aiAircraft.getParameter(ParameterTypes.C_Magneto, 0) <= 0 || aiAircraft.getParameter(ParameterTypes.C_Throttle, 0) <= 0
                        || aiAircraft.getParameter(ParameterTypes.Z_VelocityIAS, 0) <= 0 || aiAircraft.getParameter(ParameterTypes.Z_VelocityTAS, 0) <= 0;
                }
                return false;
            }

            public bool Update(AircraftObj aircraftObject)
            {
                bool updated = false;
                try
                {
                    IsValid = aircraftObject.IsValid;
                    IsAlive = aircraftObject.IsAlive;
                    IsTaskComplete = aircraftObject.IsTaskComplete;
                    UpdateName(aircraftObject);
                    UpdateClass(aircraftObject);
                    UpdateType(aircraftObject);
                    UpdatePoint(aircraftObject);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("AircraftObject.Update {0} {1}", ex.Message, ex.StackTrace));
                }

                return updated;
            }

            public override void WriteTo(ISectionFile file, bool overwrite)
            {
                base.WriteTo(file, overwrite);

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
                    SilkySkyCloDFile.Write(file, SectionAircraft, Name, string.Join(SplitString, vals), overwrite);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Aircraft.WriteTo {0} {1}", ex.Message, ex.StackTrace));
                }
            }
        }

        public class GroundObj : MissionActorObj
        {
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

            public static GroundObj Create(string name, string[] values)
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
                        return new GroundObj()
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

            public static GroundObj Create(AiGroundActor aiGroundActor)
            {
                try
                {
                    if (aiGroundActor != null)
                    {
                        string name = CreateShortName(aiGroundActor.Name());
                        Point3d pos = aiGroundActor.Pos();
                        return new GroundObj()
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
                        Debug.Assert(false, "GroundObject Create");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("GroundObject.Create {0} {1}", ex.Message, ex.StackTrace));
                }
                return null;
            }

            public bool Update(GroundObj groundObject)
            {
                bool updated = false;
                try
                {
                    IsValid = groundObject.IsValid;
                    IsAlive = groundObject.IsAlive;
                    IsTaskComplete = groundObject.IsTaskComplete;
                    UpdateName(groundObject);
                    UpdateClass(groundObject);
                    UpdateType(groundObject);
                    UpdatePoint(groundObject);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("GroundObject.Update {0} {1}", ex.Message, ex.StackTrace));
                }
                return updated;
            }

            public override void WriteTo(ISectionFile file, bool overwrite)
            {
                base.WriteTo(file, overwrite);

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
                    SilkySkyCloDFile.Write(file, SectionGroundActor, Name, string.Join(SplitString, vals), overwrite);
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

        public PlayerObj PlayerInfo
        {
            get;
            private set;
        }

        public List<AirGroupObj> AirGroups
        {
            get;
            private set;
        }

        public List<GroundGroupObj> GroundGroups
        {
            get;
            private set;
        }

        public List<StationaryObj> Stationaries
        {
            get;
            private set;
        }

        public List<AircraftObj> Aircrafts
        {
            get;
            private set;
        }

        public List<GroundObj> GroundActors
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

        public MissionStatus(IRandom random, DateTime dateTime)
        {
            Random = random;
            DateTime = dateTime;
            PlayerInfo = null;
            AirGroups = new List<AirGroupObj>();
            GroundGroups = new List<GroundGroupObj>();
            Stationaries = new List<StationaryObj>();
            Aircrafts = new List<AircraftObj>();
            GroundActors = new List<GroundObj>();
        }

        public MissionStatus(IRandom random, DateTime dateTime, PlayerObj playerInfo, IEnumerable<AirGroupObj> airGroups, IEnumerable<GroundGroupObj> groundGroups, IEnumerable<StationaryObj> stationaries, IEnumerable<AircraftObj> aircrafts, IEnumerable<GroundObj> groundActors)
        {
            Random = random;
            DateTime = dateTime;
            PlayerInfo = playerInfo;
            AirGroups = new List<AirGroupObj>(airGroups);
            GroundGroups = new List<GroundGroupObj>(groundGroups);
            Stationaries = new List<StationaryObj>(stationaries);
            Aircrafts = new List<AircraftObj>(aircrafts);
            GroundActors = new List<GroundObj>(groundActors);
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
                    PlayerObj player = PlayerObj.Create(key, value.Split(SplitChars));

                    int i;
                    List<AirGroupObj> airGroups = new List<AirGroupObj>();
                    lines = file.lines(SectionAirGroups);
                    for (i = 0; i < lines; i++)
                    {
                        file.get(SectionAirGroups, i, out key, out value);
                        AirGroupObj airGroup = AirGroupObj.Create(key, value.Split(SplitChars));
                        if (airGroup != null)
                        {
                            airGroups.Add(airGroup);
                        }
                    }

                    List<GroundGroupObj> groundGroups = new List<GroundGroupObj>();
                    lines = file.lines(SectionChiefs);
                    for (i = 0; i < lines; i++)
                    {
                        file.get(SectionChiefs, i, out key, out value);
                        GroundGroupObj groundGroup = GroundGroupObj.Create(key, value.Split(SplitChars));
                        if (groundGroup != null)
                        {
                            groundGroups.Add(groundGroup);
                        }
                    }

                    List<StationaryObj> stationaries = new List<StationaryObj>();
                    lines = file.lines(SectionStationary);
                    for (i = 0; i < lines; i++)
                    {
                        file.get(SectionStationary, i, out key, out value);
                        StationaryObj stationary = StationaryObj.Create(key, value.Split(SplitChars));
                        if (stationary != null)
                        {
                            stationaries.Add(stationary);
                        }
                    }

                    List<AircraftObj> aircrafts = new List<AircraftObj>();
                    lines = file.lines(SectionAircraft);
                    for (i = 0; i < lines; i++)
                    {
                        file.get(SectionAircraft, i, out key, out value);
                        AircraftObj aircraft = AircraftObj.Create(key, value.Split(SplitChars));
                        if (aircraft != null)
                        {
                            aircrafts.Add(aircraft);
                        }
                    }

                    List<GroundObj> groundActors = new List<GroundObj>();
                    lines = file.lines(SectionGroundActor);
                    for (i = 0; i < lines; i++)
                    {
                        file.get(SectionGroundActor, i, out key, out value);
                        GroundObj groundActor = GroundObj.Create(key, value.Split(SplitChars));
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

        public void Update(AiActor aiActor, GameEventId eventId, bool group = true, bool items = false)
        {
            if (aiActor is AiAircraft)
            {
                Update(aiActor as AiAircraft, eventId, group);
            }
            else if (aiActor is AiGroundActor)
            {
                Update(aiActor as AiGroundActor, eventId, group);
            }
            else if (aiActor is AiAirGroup)
            {
                Update(aiActor as AiAirGroup, eventId, items);
            }
            else if (aiActor is AiGroundGroup)
            {
                Update(aiActor as AiGroundGroup, eventId, items);
            }
            //else if (aiActor is GroundStationary)
            //{
            //    Update(aiActor as GroundStationary);
            //}
            else if (aiActor is AiPerson)
            {
                Update(aiActor as AiPerson, eventId, group/*, true*/);
            }
        }

        public void Update(IGame game, GameEventId eventId, string playerActoName, DateTime dateTime, bool items = false)
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
                    Update(item, eventId, items);
                }
            }

            AiAirGroup[] aiAirGroupBlue = game.gpAirGroups((int)EArmy.Blue);
            if (aiAirGroupBlue != null)
            {
                foreach (var item in aiAirGroupBlue)
                {
                    Update(item, eventId, items);
                }
            }

            // GroudGroup
            AiGroundGroup[] aiGroundGroupRed = game.gpGroundGroups((int)EArmy.Red);
            if (aiGroundGroupRed != null)
            {
                foreach (var item in aiGroundGroupRed)
                {
                    Update(item, eventId, items);
                }
            }
            AiGroundGroup[] aiGroundGroupBlue = game.gpGroundGroups((int)EArmy.Blue);
            if (aiGroundGroupBlue != null)
            {
                foreach (var item in aiGroundGroupBlue)
                {
                    Update(item, eventId, items);
                }
            }

            // Stationary
            GroundStationary[] groundStationary = game.gpGroundStationarys();
            if (groundStationary != null)
            {
                foreach (var item in groundStationary)
                {
                    Update(item, eventId);
                }
            }
        }

        public void Update(maddox.game.Player player, string playerActorName)
        {
            PlayerObj playerInfoNew = PlayerObj.Create(player, playerActorName);
            if (playerInfoNew != null)
            {
                if (PlayerInfo == null)
                {
                    PlayerInfo = playerInfoNew;
                }
                else
                {
                    PlayerInfo.Update(playerInfoNew);
                }
            }
        }

        private void Update(AiAirGroup aiAirGroup, GameEventId eventId, bool items = false)
        {
            AirGroupObj airGroupNew = AirGroupObj.Create(aiAirGroup);
            if (airGroupNew != null)
            {
                AirGroupObj airGroup = airGroupNew.Id > 0 ? AirGroups.Where(x => x.Id == airGroupNew.Id).FirstOrDefault() :
                    !string.IsNullOrEmpty(airGroupNew.Name) ? AirGroups.Where(x => string.Compare(x.SquadronName, airGroupNew.SquadronName, true) == 0).FirstOrDefault() : null;
                if (airGroup == null)
                {
                    Debug.WriteLine("  AiAirGroup.Add(Id={0,3} Name={1,-45}[{2,-40}] Class={3,-35})", airGroupNew.Id, airGroupNew.Name, airGroupNew.NameItem, airGroupNew.ClassShortName);
                    AirGroups.Add(airGroupNew);
                }
                else
                {
                    // Debug.WriteLine("  AiAirGroup.Update(Id={0,2} {1,-30} Class={2,-35})", airGroupNew.Id, airGroupNew.Name, airGroupNew.Class);
                    airGroup.Update(airGroupNew);
                }

                if (items)
                {
                    AiActor[] actors = CloDAPIUtil.GetItems(aiAirGroup);
                    if (actors != null)
                    {
                        foreach (var item in actors)
                        {
                            Update(item, eventId, false, false);
                        }
                    }
                }
            }
        }

        private void Update(AiGroundGroup aiGroundGroup, GameEventId eventId, bool items = false)
        {
            GroundGroupObj groundGroupNew = GroundGroupObj.Create(aiGroundGroup);
            if (groundGroupNew != null)
            {
                GroundGroupObj groundGroup = !string.IsNullOrEmpty(groundGroupNew.Name) ? GroundGroups.Where(x => string.Compare(x.Name, groundGroupNew.Name) == 0).FirstOrDefault() : null;
                if (groundGroup == null)
                {
                    Debug.WriteLine("  AiGroundGroup.Add({0,-45} Class={1,-35})", groundGroupNew.Name, groundGroupNew.ClassShortName);
                    GroundGroups.Add(groundGroupNew);
                }
                else
                {
                    // Debug.WriteLine("  AiGroundGroup.Update({0,-30} Class={1,-35})", groundGroupNew.Name, groundGroupNew.Class);
                    groundGroup.Update(groundGroupNew);
                }
            }

            if (items)
            {

                AiActor[] actors = CloDAPIUtil.GetItems(aiGroundGroup);
                if (actors != null)
                {
                    foreach (var item in actors)
                    {
                        Update(item, eventId, false, false);
                    }
                }
            }
        }

        public void Update(GroundStationary groundStationary, GameEventId eventId)
        {
            StationaryObj stationaryNew = StationaryObj.Create(groundStationary);
            if (stationaryNew != null)
            {
                StationaryObj stationary = !string.IsNullOrEmpty(stationaryNew.Id) ? Stationaries.Where(x => x.Id == stationaryNew.Id).FirstOrDefault() :
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
        }

        private void Update(AiAircraft aiAircraft, GameEventId eventId, bool group = false)
        {
            Debug.WriteLine("  AiAircraft.Update Army={0,1}, Name={1,-45}, TypeName={2,-30}, Group={3,-45},  IsValid={4,-5}, IsAlive={5,-5}, IsTask={6,-5}", 
                aiAircraft.Army(), aiAircraft.Name(), MissionObjBase.CreateClassShortShortName(aiAircraft.InternalTypeName()), aiAircraft.Group() != null ? aiAircraft.Group().Name() : string.Empty, aiAircraft.IsValid(), aiAircraft.IsAlive(), aiAircraft.IsTaskComplete());
            AircraftObj aircraftNew = AircraftObj.Create(aiAircraft);
            if (aircraftNew != null)
            {
                AircraftObj aircraft = !string.IsNullOrEmpty(aircraftNew.Name) ? Aircrafts.Where(x => string.Compare(x.Name, aircraftNew.Name) == 0).FirstOrDefault() : null;
                if (aircraft == null)
                {
                    Aircrafts.Add(aircraftNew);
                }
                else
                {
                    if (aircraft.IsMissionCompleted && (eventId == GameEventId.AircraftKilled || eventId == GameEventId.ActorDead || eventId == GameEventId.ActorDestroyed))
                    {
                        // Safe Destroyed
                        aircraftNew.IsValid = aircraftNew.IsAlive = aircraftNew.IsTaskComplete = true;
                    }
                    aircraft.Update(aircraftNew);
                }

                if (eventId == GameEventId.AircraftLanded)
                {
                    aircraft.IsLanded = true;
                }
                else if (eventId == GameEventId.AircraftTookOff)
                {
                    aircraft.IsLanded = false;
                    aircraft.IsStoped = false;
                    aircraft.IsReArmed = false;
                    aircraft.IsReFueled = false;
                }
            }

            if (group)
            {
                AiAirGroup aiAirGroup = aiAircraft.AirGroup();
                if (aiAirGroup != null/* && string.Compare(aiAirGroup.Name(), ValueNoName, true) != 0*/)
                {
                    Update(aiAirGroup, eventId, false);
                }
                else
                {
                    aiAirGroup = aiAircraft.Group() as AiAirGroup;
                    if (aiAirGroup != null/* && string.Compare(aiAirGroup.Name(), ValueNoName, true) != 0*/)
                    {
                        Update(aiAirGroup, eventId, false);
                    }
                }
            }
        }

        private void Update(AiGroundActor aiGroundActor, GameEventId eventId, bool group = false)
        {
            Debug.WriteLine("  AiGroundActor.Update Army={0,1}, Name={1,-35}, TypeName={2,-30}, Group={3,-30}", aiGroundActor.Army(), aiGroundActor.Name(), MissionObjBase.CreateClassShortShortName(aiGroundActor.InternalTypeName()), aiGroundActor.Group() != null ? aiGroundActor.Group().Name() : string.Empty);
            try
            {
                GroundObj groundActorNew = GroundObj.Create(aiGroundActor);
                if (groundActorNew != null)
                {
                    GroundObj groundActor = !string.IsNullOrEmpty(groundActorNew.Name) ? GroundActors.Where(x => string.Compare(x.Name, groundActorNew.Name) == 0).FirstOrDefault() : null;
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
                            groundActor.CopyPoint(groundActorNew);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("AiGroundActor.Update {0} {1} [{2} {3}]", ex.Message, ex.StackTrace, aiGroundActor.Name(), MissionObjBase.CreateClassShortShortName(aiGroundActor.InternalTypeName()));
                Core.WriteLog(msg);
            }

            if (group)
            {
                AiGroundGroup aiGroundGroup = aiGroundActor.Group() as AiGroundGroup;
                if (aiGroundGroup != null && string.Compare(aiGroundActor.Name(), aiGroundGroup.Name(), true) != 0)
                {
                    Update(aiGroundGroup, eventId, false);
                }
            }
        }

        private void Update(AiPerson aiPerson, GameEventId eventId, bool group = true, bool cart = true)
        {
            if (group)
            {
                AiGroup aiGroup = aiPerson.Group();
                if (aiGroup != null)
                {
                    if (aiGroup is AiAirGroup/* && string.Compare(aiGroup.Name(), ValueNoName, true) != 0*/)
                    {
                        Update(aiGroup as AiAirGroup, eventId, true);
                    }
                    else if (aiGroup is AiGroundGroup/* && string.Compare(aiGroup.Name(), ValueNoName, true) != 0*/)
                    {
                        Update(aiGroup as AiGroundGroup, eventId, true);
                    }
                }
            }

            if (cart)
            {
                AiCart aiCart = aiPerson.Cart();
                if (aiCart != null)
                {
                    if (aiCart is AiAircraft)
                    {
                        AiAircraft aiAircraf = aiCart as AiAircraft;
                        if (aiAircraf != null)
                        {
                            Update(aiAircraf, eventId, group);
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
                            Update(aiGroundActor, eventId, group);
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

        public void WriteTo(ISectionFile file, bool overwrite)
        {
            SilkySkyCloDFile.Write(file, SectionMain, KeyDateTime, DateTime.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat), true);
            if (PlayerInfo != null)
            {
                PlayerInfo.WriteTo(file, overwrite);
            }
            foreach (var item in AirGroups.OrderBy(x => x.Name))
            {
                item.WriteTo(file, overwrite);
            }
            foreach (var item in GroundGroups.OrderBy(x => x.Name))
            {
                item.WriteTo(file, overwrite);
            }
            foreach (var item in Stationaries.OrderBy(x => x.Name))
            {
                item.WriteTo(file, overwrite);
            }

            foreach (var item in Aircrafts.OrderBy(x => x.Name))
            {
                item.WriteTo(file, overwrite);
            }
            foreach (var item in GroundActors.OrderBy(x => x.Name))
            {
                item.WriteTo(file, overwrite);
            }
        }

        public void UpdateWriteTo(ISectionFile file, int reinForceDay, bool overwrite)
        {
            MissionStatus missionStatusPrevious = MissionStatus.Create(file, Random);

            // Player
            SilkySkyCloDFile.Write(file, SectionMain, KeyDateTime, DateTime.ToString(Config.DateTimeDefaultLongFormat, Config.DateTimeFormat), true);
            if (PlayerInfo != null)
            {
                PlayerInfo.WriteTo(file, overwrite);
            }

            if (missionStatusPrevious != null)
            {
                // AirGroup
                UpdateWriteTo(file, reinForceDay, overwrite, missionStatusPrevious.AirGroups);

                // GroundGroups
                UpdateWriteTo(file, reinForceDay, overwrite, missionStatusPrevious.GroundGroups);

                // Stationary
                UpdateWriteTo(file, reinForceDay, overwrite, missionStatusPrevious.Stationaries);

                // Aircraft
                UpdateWriteTo(file, reinForceDay, overwrite, missionStatusPrevious.Aircrafts);

                // GroundActor
                UpdateWriteTo(file, reinForceDay, overwrite, missionStatusPrevious.GroundActors);
            }
            else
            {
                // AirGroup
                UpdateWriteTo(file, reinForceDay, overwrite, new List<AirGroupObj>());

                // GroundGroups
                UpdateWriteTo(file, reinForceDay, overwrite, new List<GroundGroupObj>());

                // Stationary
                UpdateWriteTo(file, reinForceDay, overwrite, new List<StationaryObj>());

                // Aircraft
                UpdateWriteTo(file, reinForceDay, overwrite, new List<AircraftObj>());

                // GroundActor
                UpdateWriteTo(file, reinForceDay, overwrite, new List<GroundObj>());
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

        private void UpdateWriteTo(ISectionFile file, int reinForceDay, bool overwrite, List<AirGroupObj> airGroups)
        {
            IEnumerable<string> squadronNames = AirGroups.Select(x => string.Compare(x.Name, ValueNoName, true) != 0 ? x.SquadronName : x.SquadronNameItem).Distinct();
            foreach (var squadronName in squadronNames)
            {
                IEnumerable<AirGroupObj> targets = AirGroups.Where(x => string.Compare(string.Compare(x.Name, ValueNoName, true) != 0 ? x.SquadronName : x.SquadronNameItem, squadronName, true) == 0);
                try
                {
                    AirGroupObj item = targets.Where(x => string.Compare(x.Name, ValueNoName, true) != 0).FirstOrDefault() ?? targets.First();
                    int[] reinForce = GetRandomReinForceDayHour(item.ReinForceDayHour(reinForceDay));
                    AirGroupObj previousAirGroup = airGroups.Where(x => string.Compare(x.SquadronName, squadronName, true) == 0).LastOrDefault();
                    IEnumerable<AircraftObj> aircrafts = Aircrafts.Where(x => string.Compare(AirGroupObj.Name2SquadronName(x.Name), squadronName, true) == 0);
                    int alives = aircrafts.Sum(x => x.IsAlive ? 1: 0);
                    item.InitNums = targets.Max(x => x.InitNums);
                    // item.Nums = targets.Sum(x => x.Nums);
                    // Debug.Assert(item.Nums == alives);
                    item.Nums = alives;
                    // item.DiedNums = targets.Sum(x => x.DiedNums);
                    item.DiedNums = item.InitNums - alives;
                    item.Name = string.Compare(item.Name, ValueNoName, true) != 0 ? item.Name : squadronName + "0";
                    if (item.InitNums > 0/* && nums > 0*/)
                    {
                        float rate = (item.InitNums - item.DiedNums) / (float)item.InitNums;
                        if (previousAirGroup != null)
                        {
                            SilkySkyCloDFile.Delete(file, SectionAirGroups, previousAirGroup.Name);
                            previousAirGroup.Name = item.Name;
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
                                previousAirGroup.CopyPoint(item);
                            }
                            previousAirGroup.Nums = item.Nums;
                            previousAirGroup.InitNums = item.InitNums;
                            previousAirGroup.DiedNums = item.DiedNums;
                            previousAirGroup.IsTaskComplete = item.IsTaskComplete;
                        }
                        else
                        {
                            if (rate < 1)
                            {
                                item.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                            }
                            airGroups.Add(item);
                        }
                    }
                    else
                    {
                        if (previousAirGroup != null)
                        {
                            // previousGroundObject = item;
                            SilkySkyCloDFile.Delete(file, SectionAirGroups, previousAirGroup.Name);
                            airGroups.Remove(previousAirGroup);
                        }
                        item.IsAlive = false;
                        item.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                        airGroups.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error MissionStatus.UpdateWriteTo(AirGroupObject): {0} {1}", ex.Message, ex.StackTrace);
                }
            }
            foreach (var item in airGroups.OrderBy(x => x.Name))
            {
                item.WriteTo(file, overwrite);
            }
        }

        private void UpdateWriteTo(ISectionFile file, int reinForceDay, bool overwrite, List<GroundGroupObj> groundGroups)
        {
            foreach (var item in GroundGroups.OrderBy(x => x.Name))
            {
                try
                {
                    int[] reinForce = GetRandomReinForceDayHour(item.ReinForceDayHour(reinForceDay));
                    GroundGroupObj previousGroundGroup = groundGroups.Where(x => string.Compare(x.Name, item.Name, true) == 0).LastOrDefault();
                    if (item.InitNums > 0/* && item.AliveNums > 0*/)
                    {
                        float rate = item.Nums / (float)item.InitNums;
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
                                previousGroundGroup.CopyPoint(item);
                            }
                            previousGroundGroup.InitNums = item.InitNums;
                            previousGroundGroup.Nums = item.Nums;
                            previousGroundGroup.IsTaskComplete = item.IsTaskComplete;
                        }
                        else
                        {
                            if (rate < 1)
                            {
                                item.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                            }
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
                        item.ReinForceDate = DateTime.AddDays(reinForce[(int)ReinForcePart.Day]).AddHours(reinForce[(int)ReinForcePart.Hour]);
                        groundGroups.Add(item);
                        item.IsAlive = false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error MissionStatus.UpdateWriteTo(GroundGroupObject): {0} {1}", ex.Message, ex.StackTrace);
                }
            }
            foreach (var item in groundGroups.OrderBy(x => x.Name))
            {
                item.WriteTo(file, overwrite);
            }
        }

        private void UpdateWriteTo(ISectionFile file, int reinForceDay, bool overwrite, List<StationaryObj> stationaries)
        {
            foreach (var item in Stationaries.OrderBy(x => x.Name))
            {
                try
                {
                    StationaryObj previousStationary = stationaries.Where(x => x.Country == item.Country && x.Class == item.Class && (x.Name == item.Name || x.IsSamePosition(item))).FirstOrDefault();
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
                                previousStationary.CopyPoint(item);
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
                catch (Exception ex)
                {
                    Debug.WriteLine("Error MissionStatus.UpdateWriteTo(StationaryObject): {0} {1}", ex.Message, ex.StackTrace);
                }
            }
            foreach (var item in stationaries.OrderBy(x => x.Name))
            {
                item.WriteTo(file, overwrite);
            }
        }

        private void UpdateWriteTo(ISectionFile file, int reinForceDay, bool overwrite, List<AircraftObj> aircrafts)
        {
            foreach (var item in Aircrafts.OrderBy(x => x.Name))
            {
                try
                {
                    if (item.IsMissionCompleted)
                    {
                        // Safe Destroyed
                        item.IsValid = item.IsAlive = item.IsTaskComplete = true;
                    }

                    AircraftObj previousAircraft = aircrafts.Where(x => x.Army == item.Army && x.Name == item.Name && x.Class == item.Class).FirstOrDefault();
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
                                previousAircraft.CopyPoint(item);
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
                catch (Exception ex)
                {
                    Debug.WriteLine("Error MissionStatus.UpdateWriteTo(AircraftObject): {0} {1}", ex.Message, ex.StackTrace);
                }
            }
            foreach (var item in aircrafts.OrderBy(x => x.Name))
            {
                item.WriteTo(file, overwrite);
            }
        }

        private void UpdateWriteTo(ISectionFile file, int reinForceDay, bool overwrite, List<GroundObj> groundActors)
        {
            foreach (var item in GroundActors.OrderBy(x => x.Name))
            {
                try
                {
                    GroundObj previousGroundObject = groundActors.Where(x => x.Army == item.Army && x.Class == item.Class && (x.Name == item.Name || x.IsSamePosition(item))).FirstOrDefault();
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
                                previousGroundObject.CopyPoint(item);
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
                catch (Exception ex)
                {
                    Debug.WriteLine("Error MissionStatus.UpdateWriteTo(GroundObject): {0} {1}", ex.Message, ex.StackTrace);
                }
            }
            foreach (var item in groundActors.OrderBy(x => x.Name))
            {
                item.WriteTo(file, overwrite);
            }
        }

        #endregion

        public AirGroupObj GetPlayerAirGroup()
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
