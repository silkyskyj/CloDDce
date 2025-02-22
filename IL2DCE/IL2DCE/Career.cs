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
using System.Linq;
using System.Text;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game;

namespace IL2DCE
{
    public enum ArmyType
    {
        Red = 1,
        Blue = 2,
        Count = 2,
    };

    public enum AirForceRed
    {
        Raf = 1,
        Aa = 2,
        Usaaf = 3,
        Count = 3,
    };

    public enum AirForceBlue
    {
        Lw = 1,
        Ra = 2,
        Count = 2,
    };

    public enum EBattleType
    {
        Unknown,
        Campaign,
        QuickMission,
        Count,
    };

    public class Career
    {
        public const string KeyVersion = "Ver";
        public const string SectionMain = "Main";
        public const string SectionCampaign = "Campaign";
        public const string SectionStat = "Stat";
        public const string SectionKillsResult = "KillsResult";
        public const string SectionKillsGroundResult = "KillsGroundResult";
        public const string KeyTakeoffs = "Takeoffs";
        public const string KeyLandings = "Landings";
        public const string KeyBails = "Bails";
        public const string KeyDeaths = "Deaths";
        public const string KeyKills = "Kills";
        public const string KeyKillsGround = "KillsGround";
        public const string KeyAircraft = "Aircraft";

        public const string KillsFormat = "F0";
        public const string DateFormat = "yyyy/M/d";

        public const int RankMax = 5;

        public static readonly string[] RafRanks = new string[] {
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

        public static readonly string[][] Rank = new string[][] {
            RafRanks,
            AaRanks,
            UsaafRanks,
            LwRanks,
            RaRanks,
        };

        public static readonly string[] Army = new string[] {
            "Red",
            "Blue",
        };

        public static readonly string[] AirForce = new string[] {
                "Royal Air Force",
                "Armee de l'air",
                "United States Army Air Forces",
                "Luftwaffe",
                "Regia Aeronautica",
                 "",
        };

        public static readonly string[] PilotNameDefault = new string[] {
                "Joe Bloggs",
                "Jean Dupont",
                "John Smith",
                "Max Mustermann",
                "Mario Rossi",
                 "",
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

        public string Aircraft
        {
            get;
            set;
        }

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

        public double KillsGround
        {
            get;
            set;
        }

        public Dictionary<DateTime, string> KillsHistory
        {
            get;
            set;
        }

        public Dictionary<DateTime, string> KillsGroundHistory
        {
            get;
            set;
        }

        #region Quick Mission Info

        public EBattleType BattleType
        {
            get;
            set;
        }

        public EMissionType? MissionType
        {
            get;
            set;
        }

        public bool AllowDefensiveOperation
        {
            get;
            set;
        }

        public AirGroup EscortAirGroup
        {
            get;
            set;
        }

        public GroundGroup TargetGroundGroup
        {
            get;
            set;
        }

        public Stationary TargetStationary
        {
            get;
            set;
        }

        public AirGroup PlayerAirGroup
        {
            get;
            set;
        }

        public Skill PlayerAirGroupSkill
        {
            get;
            set;
        }

        public double Time
        {
            get;
            set;
        }

        public int Weather
        {
            get;
            set;
        }

        public int CloudAltitude
        {
            get;
            set;
        }

        public int BreezeActivity
        {
            get;
            set;
        }

        public int ThermalActivity
        {
            get;
            set;
        }


        #endregion

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

            Aircraft = string.Empty;

            KillsHistory = new Dictionary<DateTime, string>();
            KillsGroundHistory = new Dictionary<DateTime, string>();

            AllowDefensiveOperation = true;

            #region Quick Mission Info 

            BattleType = EBattleType.Unknown;
            MissionType = null;
            PlayerAirGroupSkill = null;
            Time = -1;
            Weather = -1;
            CloudAltitude = -1;
            BreezeActivity = -1;
            ThermalActivity = -1;

            #endregion
        }

        public Career(string pilotName, IList<CampaignInfo> campaignInfos, ISectionFile careerFile)
        {
            _pilotName = pilotName;

            string ver = careerFile.exist(SectionMain, KeyVersion) ? careerFile.get(SectionMain, KeyVersion): string.Empty;

            if (careerFile.exist(SectionMain, "armyIndex")
                && careerFile.exist(SectionMain, "rankIndex")
                && careerFile.exist(SectionMain, "experience"))
            {
                _armyIndex = int.Parse(careerFile.get(SectionMain, "armyIndex"));
                _airForceIndex = int.Parse(careerFile.get(SectionMain, "airForceIndex"));
                _rankIndex = int.Parse(careerFile.get(SectionMain, "rankIndex"));
                _experience = int.Parse(careerFile.get(SectionMain, "experience"));
            }
            else
            {
                throw new FormatException();
            }

            if (careerFile.exist(SectionCampaign, "date")
                && careerFile.exist(SectionCampaign, "id")
                && careerFile.exist(SectionCampaign, "airGroup")
                && careerFile.exist(SectionCampaign, "missionFile"))
            {
                string id = careerFile.get(SectionCampaign, "id");
                foreach (CampaignInfo campaignInfo in campaignInfos)
                {
                    if (campaignInfo.Id == id)
                    {
                        CampaignInfo = campaignInfo;
                        break;
                    }
                }
                _date = DateTime.Parse(careerFile.get(SectionCampaign, "date"));
                _airGroup = careerFile.get(SectionCampaign, "airGroup");
                _missionFileName = careerFile.get(SectionCampaign, "missionFile");
                Aircraft = careerFile.get(SectionCampaign, KeyAircraft) ?? string.Empty;

                Takeoffs = careerFile.get(SectionStat, KeyTakeoffs, 0);
                Landings = careerFile.get(SectionStat, KeyLandings, 0);
                Bails = careerFile.get(SectionStat, KeyBails, 0);
                Deaths = careerFile.get(SectionStat, KeyDeaths, 0);
                if (string.IsNullOrEmpty(ver) && careerFile.exist(SectionStat, KeyKills))
                {
                    string temp = careerFile.get(SectionStat, KeyKills);
                    double d;
                    Kills = double.TryParse(temp.Replace(",", "."), NumberStyles.Float, Config.Culture, out d) ? d : 0;
                }
                else
                {
                    Kills = careerFile.get(SectionStat, KeyKills, 0f);
                }

                string key;
                string value;
                DateTime dt;
                int lines;

                KillsHistory = new Dictionary<DateTime, string>();
                lines = careerFile.lines(SectionKillsResult);
                for (int i = 0; i < lines; i++)
                {
                    careerFile.get(SectionKillsResult, i, out key, out value);
                    if (DateTime.TryParse(key, out dt))
                    {
                        value = VersionConverter.ReplaceKillsHistory(value);
                        if (KillsHistory.ContainsKey(dt.Date))
                        {
                            KillsHistory[dt.Date] += ", " + value;
                        }
                        else
                        {
                            KillsHistory.Add(dt.Date, value);
                        }
                    }
                }

                KillsGround = careerFile.get(SectionStat, KeyKillsGround, 0f);
                KillsGroundHistory = new Dictionary<DateTime, string>();
                lines = careerFile.lines(SectionKillsGroundResult);
                for (int i = 0; i < lines; i++)
                {
                    careerFile.get(SectionKillsGroundResult, i, out key, out value);
                    if (DateTime.TryParse(key, out dt))
                    {
                        if (KillsGroundHistory.ContainsKey(dt.Date))
                        {
                            KillsGroundHistory[dt.Date] += ", " + value;
                        }
                        else
                        {
                            KillsGroundHistory.Add(dt.Date, value);
                        }
                    }
                }
            }
            else
            {
                throw new FormatException("Career File Format Error");
            }

            AllowDefensiveOperation = true;
        }

        public override string ToString()
        {
            int army = ArmyIndex - 1;
            int airforce = army * 3 + AirForceIndex - 1;
            if (airforce < Rank.GetLength(0))
            {
                return string.Format("{0} {1}", Rank[airforce][RankIndex], PilotName);
            }

            return PilotName;
        }

        public void WriteTo(ISectionFile careerFile)
        {
            careerFile.add(SectionMain, KeyVersion, VersionConverter.GetCurrentVersion().ToString());
            
            careerFile.add(SectionMain, "armyIndex", ArmyIndex.ToString(Config.Culture));
            careerFile.add(SectionMain, "airForceIndex", AirForceIndex.ToString(Config.Culture));
            careerFile.add(SectionMain, "rankIndex", RankIndex.ToString(Config.Culture));
            careerFile.add(SectionMain, "experience", Experience.ToString(Config.Culture));

            careerFile.add(SectionCampaign, "date", Date.Value.Year.ToString(Config.Culture) + "-" + Date.Value.Month.ToString(Config.Culture) + "-" + Date.Value.Day.ToString(Config.Culture.NumberFormat));
            careerFile.add(SectionCampaign, "airGroup", AirGroup);
            careerFile.add(SectionCampaign, "missionFile", MissionFileName);
            careerFile.add(SectionCampaign, "id", CampaignInfo.Id);
            careerFile.add(SectionCampaign, KeyAircraft, Aircraft);

            careerFile.add(SectionStat, KeyTakeoffs, Takeoffs.ToString(Config.Culture));
            careerFile.add(SectionStat, KeyLandings, Landings.ToString(Config.Culture));
            careerFile.add(SectionStat, KeyBails, Bails.ToString(Config.Culture));
            careerFile.add(SectionStat, KeyDeaths, Deaths.ToString(Config.Culture));
            careerFile.add(SectionStat, KeyKills, Kills.ToString(KillsFormat, Config.Culture));
            foreach (var item in KillsHistory)
            {
                careerFile.add(SectionKillsResult, item.Key.ToString(DateFormat, Config.Culture), item.Value);
            }
            careerFile.add(SectionStat, KeyKillsGround, KillsGround.ToString(KillsFormat, Config.Culture));
            foreach (var item in KillsGroundHistory)
            {
                careerFile.add(SectionKillsGroundResult, item.Key.ToString(DateFormat, Config.Culture), item.Value);
            }
        }

        public string ToCurrestStatusString()
        {
            int army = ArmyIndex - 1;
            int airforce = army * 3 + AirForceIndex - 1;
            return String.Format("Current Status\n Date: {0}\n Army: {1}\n AirForce: {2}\n Rank: {3}\n AirGroup: {4}\n Aircraft: {5}\n Experience: {6}\n",
                                    Date.Value.ToString("d", DateTimeFormatInfo.InvariantInfo),
                                    Army[army],
                                    AirForce[airforce],
                                    Rank[airforce][RankIndex],
                                    AirGroup, 
                                    Aircraft,
                                    Experience);
        }

        public string ToTotalResultString()
        {
            StringBuilder sb = new StringBuilder();
            List<DateTime> dtList = KillsHistory.Keys.ToList();
            dtList.AddRange(KillsGroundHistory.Keys);
            var orderd = dtList.OrderByDescending(x => x.Date);
            foreach (var item in orderd)
            {
                if (KillsHistory.ContainsKey(item))
                {
                    sb.AppendFormat("    {0} {1}\n", item.ToString("d", DateTimeFormatInfo.InvariantInfo), KillsHistory[item]);
                }
                if (KillsGroundHistory.ContainsKey(item))
                {
                    sb.AppendFormat("    {0} {1}\n", item.ToString("d", DateTimeFormatInfo.InvariantInfo), KillsGroundHistory[item]);
                }
            }

            return String.Format("Total Result\n Takeoffs: {0}\n Landings: {1}\n Deaths: {2}\n Bails: {3}\n Kills[Aircraft]: {4}\n Kills[GroundUnit]: {5}\n Kills History:\n{6}\n",
                                    Takeoffs,
                                    Landings,
                                    Deaths,
                                    Bails,
                                    Kills.ToString(KillsFormat, Config.Culture),
                                    KillsGround.ToString(KillsFormat, Config.Culture),
                                    sb.ToString());

        }
    }
}