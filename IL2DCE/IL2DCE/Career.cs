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
using System.Globalization;
using maddox.game;

namespace IL2DCE
{
    public class Career
    {
        public const string SectionStat = "Stat";
        public const string SectionKillsResult = "KillsResult";
        public const string KeyTakeoffs = "Takeoffs";
        public const string KeyLandings = "Landings";
        public const string KeyBails = "Bails";
        public const string KeyDeaths = "Deaths";
        public const string KeyKills = "Kills";

        public static readonly string [] RafRanks = new string[] {
            "Pilot Officer",
            "Flying Officer",
            "Flight Lieutenant",
            "Squadron Leader",
            "Wing Commander",
            "Group Captain",
        };

        public static readonly string[] LwRanks = new string[] {
            "Leutnant",
            "Oberleutnant",
            "Hauptmann",
            "Major",
            "Oberstleutnant",
            "Oberst",
        };

        public static readonly string[] RaRanks = new string[] {
            "Sottotenente",
            "Tenente",
            "Capitano",
            "Maggiore",
            "Tenente Colonnello",
            "Colonnello",
        };

        public static readonly string[] AaRanks = new string[] {
            "Aspirant",
            "Lieutenant",
            "Capitaine",
            "Commandant",
            "Lieutenant-Colonel",
            "Colonel",
        };

        public static readonly string[] UsaafRanks = new string[] {
            "Flight Officer",
            "Lieutenant",
            "Captain",
            "Major",
            "Lt. Colonel",
            "Colonel",
        };

        public string PilotName
        {
            get
            {
                return _pilotName;
            }
            set
            {
                _pilotName = value;
            }
        }
        private string _pilotName;

        public int ArmyIndex
        {
            get
            {
                return _armyIndex;
            }
            set
            {
                _armyIndex = value;
            }
        }
        private int _armyIndex;

        public int AirForceIndex
        {
            get
            {
                return _airForceIndex;
            }
            set
            {
                _airForceIndex = value;
            }
        }
        private int _airForceIndex;

        public int RankIndex
        {
            get
            {
                return _rankIndex;
            }
            set
            {
                if (value < 6)
                {
                    _rankIndex = value;
                }
            }
        }
        private int _rankIndex;

        public int Experience
        {
            get
            {
                return _experience;
            }
            set
            {
                _experience = value;
            }
        }
        private int _experience;

        public CampaignInfo CampaignInfo
        {
            get
            {
                return _campaignInfo;
            }
            set
            {
                _campaignInfo = value;
            }
        }
        private CampaignInfo _campaignInfo;

        public DateTime? Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
            }
        }
        DateTime? _date;

        public string MissionFileName
        {
            get
            {
                return _missionFileName;
            }
            set
            {
                _missionFileName = value;
            }
        }
        private string _missionFileName;

        public string MissionTemplateFileName
        {
            get
            {
                return MissionFileName.Replace(".mis", "_Template.mis");
            }
        }

        public string AirGroup
        {
            get
            {
                return _airGroup;
            }
            set
            {
                _airGroup = value;
            }
        }
        private string _airGroup;

        public int Takeoffs
        {
            get;
            set;
        }

        public int Landings
        {
            get;
            set;
        }

        public int Bails
        {
            get;
            set;
        }

        public int Deaths
        {
            get;
            set;
        }

        public double Kills
        {
            get;
            set;
        }

        public Dictionary<DateTime, string> KillsHistory
        {
            get;
            set;
        }

        public Career(string pilotName, int armyIndex, int airForceIndex, int rankIndex)
        {
            _pilotName = pilotName;
            _armyIndex = armyIndex;
            _airForceIndex = airForceIndex;
            _rankIndex = rankIndex;
            _experience = 0;

            _campaignInfo = null;
            _date = null;
            _airGroup = null;
            _missionFileName = null;

            KillsHistory = new Dictionary<DateTime, string>();
        }

        public Career(string pilotName, IList<CampaignInfo> campaignInfos, ISectionFile careerFile)
        {
            _pilotName = pilotName;

            if (careerFile.exist("Main", "armyIndex")
                && careerFile.exist("Main", "rankIndex")
                && careerFile.exist("Main", "experience"))
            {
                _armyIndex = int.Parse(careerFile.get("Main", "armyIndex"));
                _airForceIndex = int.Parse(careerFile.get("Main", "airForceIndex"));
                _rankIndex = int.Parse(careerFile.get("Main", "rankIndex"));
                _experience = int.Parse(careerFile.get("Main", "experience"));
            }
            else
            {
                throw new FormatException();
            }

            if (careerFile.exist("Campaign", "date")
                && careerFile.exist("Campaign", "id")
                && careerFile.exist("Campaign", "airGroup")
                && careerFile.exist("Campaign", "missionFile"))
            {
                string id = careerFile.get("Campaign", "id");
                foreach (CampaignInfo campaignInfo in campaignInfos)
                {
                    if (campaignInfo.Id == id)
                    {
                        CampaignInfo = campaignInfo;
                        break;
                    }
                }
                _date = DateTime.Parse(careerFile.get("Campaign", "date"));
                _airGroup = careerFile.get("Campaign", "airGroup");
                _missionFileName = careerFile.get("Campaign", "missionFile");

                Takeoffs = careerFile.get(SectionStat, KeyTakeoffs, 0);
                Landings = careerFile.get(SectionStat, KeyLandings, 0);
                Bails = careerFile.get(SectionStat, KeyBails, 0);
                Deaths = careerFile.get(SectionStat, KeyDeaths, 0);
                Kills = careerFile.get(SectionStat, KeyKills, 0);

                KillsHistory = new Dictionary<DateTime, string>();
                int killsResult = careerFile.lines(SectionKillsResult);
                string key;
                string value;
                DateTime dt;
                for (int i = 0; i < killsResult; i++)
                {
                    careerFile.get(SectionKillsResult, i,  out key, out value);
                    if (DateTime.TryParse(key, out dt))
                    {
                        KillsHistory.Add(dt, value);
                    }
                }
            }
            else
            {
                throw new FormatException();
            }
        }

        public override string ToString()
        {
            if (ArmyIndex == (int)AirGroupInfo.Army.Red)
            {
                if (AirForceIndex == (int)AirGroupInfo.AirForceRed.Raf)
                {
                    return RafRanks[RankIndex] + " " + PilotName;
                }
                else if (AirForceIndex == (int)AirGroupInfo.AirForceRed.Aa)
                {
                    return AaRanks[RankIndex] + " " + PilotName;
                }
                else if (AirForceIndex == (int)AirGroupInfo.AirForceRed.Usaaf)
                {
                    return UsaafRanks[RankIndex] + " " + PilotName;
                }

            }
            else if (ArmyIndex == (int)AirGroupInfo.Army.Blue)
            {
                if (AirForceIndex == (int)AirGroupInfo.AirForceBlue.Lw)
                {
                    return LwRanks[RankIndex] + " " + PilotName;
                }
                else if (AirForceIndex == (int)AirGroupInfo.AirForceBlue.Ra)
                {
                    return RaRanks[RankIndex] + " " + PilotName;
                }
            }

            return PilotName;
        }

        public void WriteTo(ISectionFile careerFile)
        {
            careerFile.add("Main", "armyIndex", ArmyIndex.ToString(CultureInfo.InvariantCulture.NumberFormat));
            careerFile.add("Main", "airForceIndex", AirForceIndex.ToString(CultureInfo.InvariantCulture.NumberFormat));
            careerFile.add("Main", "rankIndex", RankIndex.ToString(CultureInfo.InvariantCulture.NumberFormat));
            careerFile.add("Main", "experience", Experience.ToString(CultureInfo.InvariantCulture.NumberFormat));

            careerFile.add("Campaign", "date", Date.Value.Year.ToString(CultureInfo.InvariantCulture.NumberFormat) + "-" + Date.Value.Month.ToString(CultureInfo.InvariantCulture.NumberFormat) + "-" + Date.Value.Day.ToString(CultureInfo.InvariantCulture.NumberFormat));
            careerFile.add("Campaign", "airGroup", AirGroup);
            careerFile.add("Campaign", "missionFile", MissionFileName);
            careerFile.add("Campaign", "id", CampaignInfo.Id);

            careerFile.add(SectionStat, KeyTakeoffs, Takeoffs.ToString());
            careerFile.add(SectionStat, KeyLandings, Landings.ToString());
            careerFile.add(SectionStat, KeyBails, Bails.ToString());
            careerFile.add(SectionStat, KeyDeaths, Deaths.ToString());
            careerFile.add(SectionStat, KeyKills, Kills.ToString());

            foreach (var item in KillsHistory)
            {
                careerFile.add(SectionKillsResult, item.Key.ToString("yyyy/M/d"), item.Value);
            }
        }
    }
}