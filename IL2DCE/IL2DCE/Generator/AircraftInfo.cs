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
using System.Diagnostics;
using System.Globalization;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game;

namespace IL2DCE.Generator
{
    public enum EAircraftType
    {
        Unknown = 0,
        Fighter = 1,            // = no gunner seat
        FighterSub = 2,      // = gunner seat
        FighterBomber = 3,      // = no gunner seat
        FighterBomberSub = 4,   // = gunner seat
        Bomber = 5,             // = no gunner seat
        BomberSub = 6,          // = gunner seat 
        Other = 7,              // = no gunner seat
        OtherSub = 8,           // = gunner seat 
        Count,
    }

    public class AircraftInfo
    {
        public const string SectionMain = "Main";
        public const string KeyPlayer = "Player";
        public const string KeyType = "Type";
        public const string KeyImage = "Image";

        public bool IsFlyable
        {
            get
            {
                if (_aircraftInfoFile.exist(Aircraft, KeyPlayer))
                {
                    string value = _aircraftInfoFile.get(Aircraft, KeyPlayer);
                    int player;
                    return int.TryParse(value, out player) && player != 0;
                }
                else
                {
                    string error = string.Format("no Palyer info[{0}] in the file[{1}]", Aircraft, "Aircraftinfo.ini");
                    Debug.WriteLine(error);
                    throw new FormatException(error);
                }
            }
        }

        public IList<EMissionType> MissionTypes
        {
            get
            {
                IList<EMissionType> missionTypes = new List<EMissionType>();
                int lines = _aircraftInfoFile.lines(Aircraft);
                for (int i = 0; i < lines; i++)
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

        public string ImageFolderName
        {
            get
            {
                return string.Empty;
            }
        }

        public EAircraftType AircraftType
        {
            get
            {
                int value = _aircraftInfoFile.get(Aircraft, KeyPlayer, 0);
                if (value > (int)EAircraftType.Unknown && value < (int)EAircraftType.Count)
                {
                    return (EAircraftType)value;
                }
                return EAircraftType.Unknown;
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
            if (valueParts.Length > 0)
            {
                foreach (string valuePart in valueParts)
                {
                    AircraftParametersInfo missionParameter = new AircraftParametersInfo(valuePart);
                    missionParameters.Add(missionParameter);
                }
            }
            else
            {
                throw new FormatException(string.Format("Invalid Aircraft Parameter Info[{0} {1}]", Aircraft + "." + missionType.ToString(), value));
            }

            return missionParameters;
        }

        public AircraftLoadoutInfo GetAircraftLoadoutInfo(string loadoutId)
        {
            return new AircraftLoadoutInfo(_aircraftInfoFile, Aircraft, loadoutId);
        }

        public void Write(ISectionFile file)
        {
            SectionFileUtil.Write(file, SectionMain, Aircraft, string.Empty);
            SectionFileUtil.Write(file, Aircraft, KeyPlayer, (IsFlyable ? 1 : 0).ToString(CultureInfo.InvariantCulture.NumberFormat));
            SectionFileUtil.Write(file, Aircraft, KeyType, ((int)AircraftType).ToString(CultureInfo.InvariantCulture.NumberFormat));
            int lines = _aircraftInfoFile.lines(Aircraft);
            for (int i = 0; i < lines; i++)
            {
                string key;
                string value;
                _aircraftInfoFile.get(Aircraft, i, out key, out value);
                EMissionType missionType;
                if (Enum.TryParse<EMissionType>(key, false, out missionType))
                {
                    SectionFileUtil.Write(file, Aircraft, key, value);
                    IList<AircraftParametersInfo> aircraftParametersInfos = GetAircraftParametersInfo(missionType);
                    foreach (var item in aircraftParametersInfos)
                    {
                        string keyLoadOut = string.Format("{0}_{1}", Aircraft, item.LoadoutId);
                        if (_aircraftInfoFile.exist(keyLoadOut))
                        {
                            SectionFileUtil.CopySection(_aircraftInfoFile, file, keyLoadOut, false);
                        }
                    }
                }
            }
        }
    }
}