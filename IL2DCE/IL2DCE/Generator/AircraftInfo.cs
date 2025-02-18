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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using maddox.game;

namespace IL2DCE
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

    public static class EnumExtensions
    {
        public static string ToDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            object attr = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
            return attr != null ? (attr as DescriptionAttribute).Description: value.ToString();
        }
    }

    public class AircraftInfo
    {
        public bool IsFlyable
        {
            get
            {
                if (_aircraftInfoFile.exist(Aircraft, "Player"))
                {
                    string value = _aircraftInfoFile.get(Aircraft, "Player");
                    int player = int.Parse(value);
                    if (player == 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    throw new FormatException();
                }
            }
        }

        public IList<EMissionType> MissionTypes
        {
            get
            {
                IList<EMissionType> missionTypes = new List<EMissionType>();
                for (int i = 0; i < _aircraftInfoFile.lines(Aircraft); i++)
                {
                    string key;
                    string value;
                    _aircraftInfoFile.get(Aircraft, i, out key, out value);

                    EMissionType missionType;
                    if (Enum.TryParse<EMissionType>(key, false, out missionType))
                    {
                        missionTypes.Add(missionType);
                    }
                }
                return missionTypes;
            }
        }

        public string Aircraft
        {
            get;
            set;
        }

        public string DisplayName
        {
            get
            {
                return CreateDisplayName(Aircraft);
            }
        }

        private ISectionFile _aircraftInfoFile;

        public AircraftInfo(ISectionFile aircraftInfoFile, string aircraft)
        {
            _aircraftInfoFile = aircraftInfoFile;
            Aircraft = aircraft;
        }

        public static string CreateDisplayName(string aircraftInfo)
        {
            // Aircraft.Bf-110C-7 -> Bf-110C-7
            // tobruk:Aircraft.Macchi-C202-SeriesVII -> Macchi-C202-SeriesVII
            const string del = "Aircraft.";
            int idx = aircraftInfo.IndexOf(del, StringComparison.CurrentCultureIgnoreCase);
            return idx != -1 ? aircraftInfo.Substring(idx + del.Length) : aircraftInfo;
        }

        public IList<AircraftParametersInfo> GetAircraftParametersInfo(EMissionType missionType)
        {
            IList<AircraftParametersInfo> missionParameters = new List<AircraftParametersInfo>();
            string value = _aircraftInfoFile.get(Aircraft, missionType.ToString());
            string[] valueParts = value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (valueParts != null && valueParts.Length > 0)
            {
                foreach (string valuePart in valueParts)
                {
                    AircraftParametersInfo missionParameter = new AircraftParametersInfo(valuePart);
                    missionParameters.Add(missionParameter);
                }
            }
            else
            {
                throw new FormatException(Aircraft + "." + missionType.ToString() + " " + value);
            }

            return missionParameters;
        }

        public AircraftLoadoutInfo GetAircraftLoadoutInfo(string loadoutId)
        {
            return new AircraftLoadoutInfo(this._aircraftInfoFile, Aircraft, loadoutId);
        }

        #region GetImagePath

        private static readonly string[] endIs = { "trop", "late", "late_trop", "Derated", "AltoQuota", "Trop_Derated", "100oct", "NF", "Heartbreaker", "Torpedo", "Torpedo_trop", };
        private static readonly string[] middleIs = { "Mk" };

        public static string GetImagePath(string partsFolder, string aircraftClass)
        {
            const string defaultPart = "bob";
            string path;

            int idx = aircraftClass.IndexOf(":");
            string part = idx != -1 ? aircraftClass.Substring(0, idx) : defaultPart;
            aircraftClass = idx != -1 ? aircraftClass.Substring(idx + 1) : aircraftClass;
            if (!string.IsNullOrEmpty(path = GetImagePathfromPart(partsFolder, part, aircraftClass)))
            {
                return path;
            }

            if (string.Compare(part, defaultPart) != 0)
            {
                if (!string.IsNullOrEmpty(path = GetImagePathfromPart(partsFolder, defaultPart, aircraftClass)))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private static string GetImagePathfromPart(string partsFolder, string part, string aircraftClass)
        {
            const string del = "Aircraft.";
            string path;

            int idx = aircraftClass.IndexOf(del);
            string cls = idx != -1 ? aircraftClass.Substring(idx + del.Length) : aircraftClass;
            string folderBase = string.Format("{0}\\{1}\\3do\\Plane", partsFolder, part);
            string folder = string.Format("{0}\\{1}", folderBase, cls);
            if (Directory.Exists(folder))
            {
                if (!string.IsNullOrEmpty(path = GetImagePathfromFolder(folder, cls)))
                {
                    return path;
                }
            }

            foreach (var item in endIs)
            {
                if (cls.EndsWith(item, StringComparison.InvariantCultureIgnoreCase))
                {
                    folder = string.Format("{0}\\{1}", folderBase, cls.Substring(0, cls.Length - item.Length - 1));
                    if (!string.IsNullOrEmpty(path = GetImagePathfromFolder(folder, cls)))
                    {
                        return path;
                    }
                }
            }

            foreach (var item in middleIs)
            {
                if ((idx = cls.IndexOf(item)) != -1)
                {
                    string cls2 = string.Format("{0}_{1}", cls.Substring(0, idx), cls.Substring(idx));
                    folder = string.Format("{0}\\{1}", folderBase, cls2);
                    if (!string.IsNullOrEmpty(path = GetImagePathfromFolder(folder, cls)))
                    {
                        return path;
                    }

                    foreach (var item2 in endIs)
                    {
                        if (cls2.EndsWith(item2, StringComparison.InvariantCultureIgnoreCase))
                        {
                            folder = string.Format("{0}/{1}", folderBase, cls.Substring(0, cls2.Length - item2.Length - 1));
                            if (!string.IsNullOrEmpty(path = GetImagePathfromFolder(folder, cls2)))
                            {
                                return path;
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        private static string GetImagePathfromFolder(string folder, string aircraftClass)
        {
            const string file = "item.tif";
            const string fileTrop = "item_trop.tif";
            const string fileTropLate = "item_trop_late.tif";

            string path = string.Format("{0}\\{1}", folder, file);
            if (File.Exists(path))
            {
                return path;
            }

            if (aircraftClass.EndsWith("trop", StringComparison.InvariantCultureIgnoreCase))
            {
                path = string.Format("{0}\\{1}", folder, file);
                if (File.Exists(fileTrop))
                {
                    return path;
                }
            }

            if (aircraftClass.EndsWith("late", StringComparison.InvariantCultureIgnoreCase))
            {
                path = string.Format("{0}\\{1}", folder, fileTropLate);
                if (File.Exists(fileTrop))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        #endregion
    }
}