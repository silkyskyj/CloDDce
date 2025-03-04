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
using System.IO;
using System.Linq;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;

namespace IL2DCE.Generator
{
    public class AircraftInfo
    {
        public const string SectionMain = "Main";
        public const string KeyPlayer = "Player";

        public bool IsFlyable
        {
            get
            {
                if (_aircraftInfoFile.exist(Aircraft, KeyPlayer))
                {
                    string value = _aircraftInfoFile.get(Aircraft, KeyPlayer);
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
            return new AircraftLoadoutInfo(this._aircraftInfoFile, Aircraft, loadoutId);
        }

        public void Write(ISectionFile file)
        {
            SectionFileUtil.Write(file, SectionMain, Aircraft, string.Empty);
            SectionFileUtil.Write(file, Aircraft, KeyPlayer, (IsFlyable ? 1 : 0).ToString());
            for (int i = 0; i < _aircraftInfoFile.lines(Aircraft); i++)
            {
                string key;
                string value;
                _aircraftInfoFile.get(Aircraft, i, out key, out value);
                EMissionType missionType;
                if (Enum.TryParse<EMissionType>(key, false, out missionType))
                {
                    SectionFileUtil.Write(file, Aircraft, key, value);
                    AircraftParametersInfo aircraftParametersInfo = GetAircraftParametersInfo(missionType).FirstOrDefault();
                    if (aircraftParametersInfo != null)
                    {
                        string keyLoadOut = string.Format("{0}_{1}", Aircraft, aircraftParametersInfo.LoadoutId);
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