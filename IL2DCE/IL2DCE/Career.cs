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
using System.Globalization;
using System.Linq;
using System.Text;
using IL2DCE.MissionObjectModel;
using IL2DCE.Util;
using maddox.game;

namespace IL2DCE
{
    public class Career : IPlayerStatTotal
    {
        #region Definition

        public const string SectionMain = "Main";
        public const string SectionCampaign = "Campaign";
        public const string SectionStat = "Stat";
        public const string SectionKillsResult = "KillsResult";
        public const string SectionKillsGroundResult = "KillsGroundResult";
        public const string SectionSkill = "Skill";
        public const string KeyVersion = "Ver";
        public const string KeyArmyIndex = "armyIndex";
        public const string KeyAirForceIndex = "airForceIndex";
        public const string KeyRankIndex = "rankIndex";
        public const string KeyExperience = "experience";
        public const string KeyDate = "date";
        public const string KeyTime = "time";
        public const string KeyAirGroup = "airGroup";
        public const string KeyMissionFile = "missionFile";
        public const string KeyId = "id";

        public const string KeySorties = "Sorties";
        public const string KeyTakeoffs = "Takeoffs";
        public const string KeyLandings = "Landings";
        public const string KeyBails = "Bails";
        public const string KeyDeaths = "Deaths";
        public const string KeyKills = "Kills";
        public const string KeyKillsGround = "KillsGround";
        public const string KeyAircraft = "Aircraft";
        public const string KeyAdditionalAirOperations = "AdditionalAirOperations";
        public const string KeyAdditionalGroundOperations = "AdditionalGroundOperations";
        public const string KeyAdditionalAirGroups = "AdditionalAirGroups";
        public const string KeyAirGroupDislplay = "AirGroupDisplay";
        public const string KeySpawnRandomLocationPlayer = "SpawnRandomLocationPlayer";
        public const string KeySpawnRandomLocationFriendly = "SpawnRandomLocationFriendly";
        public const string KeySpawnRandomLocationEnemy = "SpawnRandomLocationEnemy";
        public const string KeySpawnRandomTimeFriendly = "SpawnRandomTimeFriendly";
        public const string KeySpawnRandomTimeEnemy = "SpawnRandomTimeEnemy";
        public const string KeySpawnRandomTimeBeginSec = "SpawnRandomTimeBeginSec";
        public const string KeySpawnRandomTimeEndSec = "SpawnRandomTimeEndSec";
        public const string KeySpawnRandomAltitudeFriendly = "SpawnRandomAltitudeFriendly";
        public const string KeySpawnRandomAltitudeEnemy = "SpawnRandomAltitudeEnemy";
        public const string KeyReArm = "ReArm";
        public const string KeyReFuel = "ReFuel";
        public const string KeyTrackRecording = "TrackRecording";
        public const string KeyAISkill = "AISkill";
        public const string KeyCampaignProgress = "CampaignProgress";
        // public const string KillsFormat = "F0";
        public const string DateFormat = "yyyy/M/d";
        public const string PlayerCurrentStatusFormat = "{0,10}: {1}";

        #endregion

        #region Property & Variable

        public string PilotName
        {
            get;
            private set;
        }

        public int ArmyIndex
        {
            get;
            private set;
        }

        public int AirForceIndex
        {
            get;
            private set;
        }

        public int RankIndex
        {
            get;
            private set;
        }

        public int Experience
        {
            get;
            private set;
        }

        public CampaignInfo CampaignInfo
        {
            get;
            set;
        }

        public DateTime? Date
        {
            get;
            private set;
        }

        public string MissionFileName
        {
            get;
            set;
        }

        public string MissionTemplateFileName
        {
            get
            {
                return MissionFileName.Replace(Config.MissionFileExt, "_Template.mis");
            }
        }

        public string AirGroup
        {
            get;
            set;
        }

        public string Aircraft
        {
            get;
            set;
        }

        public int AdditionalAirOperations
        {
            get;
            set;
        }

        public int AdditionalGroundOperations
        {
            get;
            set;
        }

        public bool AdditionalAirGroups
        {
            get;
            set;
        }

        public string AirGroupDisplay
        {
            get;
            set;
        }

        public bool SpawnRandomLocationPlayer
        {
            get;
            set;
        }

        public bool SpawnRandomLocationFriendly
        {
            get;
            set;
        }

        public bool SpawnRandomLocationEnemy
        {
            get;
            set;
        }

        public bool SpawnRandomAltitudeFriendly
        {
            get;
            set;
        }

        public bool SpawnRandomAltitudeEnemy
        {
            get;
            set;
        }

        public bool SpawnRandomTimeFriendly
        {
            get;
            set;
        }

        public bool SpawnRandomTimeEnemy
        {
            get;
            set;
        }

        public int SpawnRandomTimeBeginSec
        {
            get;
            set;
        }

        public int SpawnRandomTimeEndSec
        {
            get;
            set;
        }

        public int ReArmTime
        {
            get;
            set;
        }

        public int ReFuelTime
        {
            get;
            set;
        }

        public bool TrackRecording
        {
            get;
            set;
        }

        public ESkillSet AISkill
        {
            get;
            set;
        }

        public ECampaignProgress CampaignProgress
        {
            get;
            set;
        }

        #region Stats

        public int Sorties
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

        #endregion

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

        public int Flight
        {
            get;
            set;
        }

        public EFormation Formation
        {
            get;
            set;
        }

        public int Spawn
        {
            get;
            set;
        }

        public int Speed
        {
            get;
            set;
        }

        public int Fuel
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

        public AirGroup EscoredtAirGroup
        {
            get;
            set;
        }

        public AirGroup OffensiveAirGroup
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

        public Skill[] PlayerAirGroupSkill
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

        #endregion

        #region Constructor

        public Career(string pilotName, int armyIndex, int airForceIndex, int rankIndex)
        {
            PilotName = pilotName;
            ArmyIndex = armyIndex;
            AirForceIndex = airForceIndex;
            RankIndex = rankIndex;
            Experience = 0;

            CampaignInfo = null;
            Date = null;
            AirGroup = null;
            MissionFileName = null;
            AirGroupDisplay = null;

            Aircraft = string.Empty;

            AdditionalAirOperations = Config.DefaultAdditionalAirOperations;
            AdditionalGroundOperations = Config.DefaultAdditionalGroundOperations;
            AdditionalAirGroups = false;
            SpawnRandomLocationPlayer = false;
            SpawnRandomLocationFriendly = false;
            SpawnRandomLocationEnemy = true;
            SpawnRandomAltitudeFriendly = false;
            SpawnRandomAltitudeEnemy = true;
            SpawnRandomTimeFriendly = false;
            SpawnRandomTimeEnemy = true;
            SpawnRandomTimeBeginSec = MissionObjectModel.Spawn.SpawnTime.DefaultBeginSec;
            SpawnRandomTimeEndSec = MissionObjectModel.Spawn.SpawnTime.DefaultEndSec;

            KillsHistory = new Dictionary<DateTime, string>();
            KillsGroundHistory = new Dictionary<DateTime, string>();

            AllowDefensiveOperation = true;

            ReArmTime = -1;
            ReFuelTime = -1;

            AISkill = ESkillSet.Random;
            CampaignProgress = ECampaignProgress.Daily;

            #region Quick Mission Info 

            InitQuickMssionInfo();

            #endregion
        }

        public Career(string pilotName, IList<CampaignInfo> campaignInfos, ISectionFile careerFile, Config config, string separator = Config.CommaStr)
        {
            PilotName = pilotName;

            #region Quick Mission Info 

            InitQuickMssionInfo();

            #endregion

            string ver = careerFile.exist(SectionMain, KeyVersion) ? careerFile.get(SectionMain, KeyVersion) : string.Empty;

            if (careerFile.exist(SectionMain, KeyArmyIndex)
                && careerFile.exist(SectionMain, KeyAirForceIndex)
                && careerFile.exist(SectionMain, KeyRankIndex)
                && careerFile.exist(SectionMain, KeyExperience))
            {
                ArmyIndex = int.Parse(careerFile.get(SectionMain, KeyArmyIndex));
                AirForceIndex = int.Parse(careerFile.get(SectionMain, KeyAirForceIndex));
                RankIndex = int.Parse(careerFile.get(SectionMain, KeyRankIndex));
                Experience = int.Parse(careerFile.get(SectionMain, KeyExperience));
            }
            else
            {
                throw new FormatException(string.Format("Career File Format Error[{0}]", "armyIndex/rankIndex/experience"));
            }

            if (careerFile.exist(SectionCampaign, KeyDate)
                && careerFile.exist(SectionCampaign, KeyId)
                && careerFile.exist(SectionCampaign, KeyAirGroup)
                && careerFile.exist(SectionCampaign, KeyMissionFile))
            {
                string id = careerFile.get(SectionCampaign, KeyId);
                CampaignInfo = campaignInfos.Where(x => string.Compare(x.Id, id, true) == 0).FirstOrDefault();
                Time = careerFile.get(SectionCampaign, KeyTime, 0);
                Date = DateTime.Parse(careerFile.get(SectionCampaign, KeyDate)).AddHours(Time);
                AirGroup = careerFile.get(SectionCampaign, KeyAirGroup);
                MissionFileName = careerFile.get(SectionCampaign, KeyMissionFile);
                Aircraft = careerFile.get(SectionCampaign, KeyAircraft) ?? string.Empty;

                AdditionalAirOperations = careerFile.get(SectionCampaign, KeyAdditionalAirOperations, config.AdditionalAirOperations);
                AdditionalGroundOperations = careerFile.get(SectionCampaign, KeyAdditionalGroundOperations, config.AdditionalGroundOperations);
                AdditionalAirGroups = careerFile.get(SectionCampaign, KeyAdditionalAirGroups, false);
                AirGroupDisplay = careerFile.get(SectionCampaign, KeyAirGroupDislplay, string.Empty);
                SpawnRandomLocationPlayer = careerFile.get(SectionCampaign, KeySpawnRandomLocationPlayer, false);
                SpawnRandomLocationFriendly = careerFile.get(SectionCampaign, KeySpawnRandomLocationFriendly, false);
                SpawnRandomLocationEnemy = careerFile.get(SectionCampaign, KeySpawnRandomLocationEnemy, true);
                SpawnRandomAltitudeFriendly = careerFile.get(SectionCampaign, KeySpawnRandomAltitudeFriendly, false);
                SpawnRandomAltitudeEnemy = careerFile.get(SectionCampaign, KeySpawnRandomAltitudeEnemy, true);
                SpawnRandomTimeFriendly = careerFile.get(SectionCampaign, KeySpawnRandomTimeFriendly, false);
                SpawnRandomTimeEnemy = careerFile.get(SectionCampaign, KeySpawnRandomTimeEnemy, true);
                SpawnRandomTimeBeginSec = careerFile.get(SectionCampaign, KeySpawnRandomTimeBeginSec, MissionObjectModel.Spawn.SpawnTime.DefaultBeginSec);
                SpawnRandomTimeEndSec = careerFile.get(SectionCampaign, KeySpawnRandomTimeEndSec, MissionObjectModel.Spawn.SpawnTime.DefaultEndSec);

                ReArmTime = careerFile.get(SectionCampaign, KeyReArm, false) ? Config.DefaultProcessTimeReArm : -1;
                ReFuelTime = careerFile.get(SectionCampaign, KeyReFuel, false) ? Config.DefaultProcessTimeReFuel : -1;

                TrackRecording = careerFile.get(SectionCampaign, KeyTrackRecording, false);

                string val = careerFile.get(SectionCampaign, KeyAISkill, string.Empty);
                AISkill = (Enum.IsDefined(typeof(ESkillSet), val)) ? (ESkillSet)Enum.Parse(typeof(ESkillSet), val) : ESkillSet.Random;

                val = careerFile.get(SectionCampaign, KeyCampaignProgress, string.Empty);
                CampaignProgress = (Enum.IsDefined(typeof(ECampaignProgress), val)) ? (ECampaignProgress)Enum.Parse(typeof(ECampaignProgress), val) : ECampaignProgress.Daily;

                #region Stats

                Sorties = careerFile.get(SectionStat, KeySorties, 0);
                Takeoffs = careerFile.get(SectionStat, KeyTakeoffs, 0);
                Landings = careerFile.get(SectionStat, KeyLandings, 0);
                Bails = careerFile.get(SectionStat, KeyBails, 0);
                Deaths = careerFile.get(SectionStat, KeyDeaths, 0);
                if (string.IsNullOrEmpty(ver) && careerFile.exist(SectionStat, KeyKills))
                {
                    string temp = careerFile.get(SectionStat, KeyKills);
                    double d;
                    Kills = double.TryParse(temp.Replace(",", "."), NumberStyles.Float, Config.NumberFormat, out d) ? d : 0;
                }
                else
                {
                    Kills = careerFile.get(SectionStat, KeyKills, 0f);
                }
                VersionConverter.OptimizeSorties(this);

                string key;
                string value;
                DateTime dt;
                int lines;

                KillsHistory = new Dictionary<DateTime, string>();
                lines = careerFile.lines(SectionKillsResult);
                for (int i = 0; i < lines; i++)
                {
                    careerFile.get(SectionKillsResult, i, out key, out value);
                    if (DateTime.TryParseExact(key, DateFormat, Config.DateTimeFormat, DateTimeStyles.AssumeLocal, out dt))
                    // if (DateTime.TryParse(key, out dt))
                    {
                        value = VersionConverter.ReplaceKillsHistory(value);
                        if (KillsHistory.ContainsKey(dt.Date))
                        {
                            KillsHistory[dt.Date] += separator + value;
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
                    if (DateTime.TryParseExact(key, DateFormat, Config.DateTimeFormat, DateTimeStyles.AssumeLocal, out dt))
                    // if (DateTime.TryParse(key, out dt))
                    {
                        if (KillsGroundHistory.ContainsKey(dt.Date))
                        {
                            KillsGroundHistory[dt.Date] += separator + value;
                        }
                        else
                        {
                            KillsGroundHistory.Add(dt.Date, value);
                        }
                    }
                }

                #endregion

                #region Skill

                if (careerFile.exist(SectionSkill))
                {
                    Skills skills = new Skills();
                    lines = careerFile.lines(SectionSkill);
                    for (int i = 0; i < lines; i++)
                    {
                        careerFile.get(SectionSkill, i, out key, out value);
                        System.Diagnostics.Debug.WriteLine("Skill[{0}] name={1} Value={2}", i, key, value ?? string.Empty);
                        Skill skill;
                        if (Skill.TryParse(value, out skill))
                        {
                            if (Skill.EqualsValue(skill, Skill.Default))
                            {
                                skills.Add(Skill.Default);
                            }
                            else if (!Skill.EqualsValue(skill, Skill.Random))
                            {
                                skill.Name = key;
                                skills.Add(skill);
                            }
                        }
                    }
                    PlayerAirGroupSkill = skills.ToArray();
                }

                #endregion
            }
            else
            {
                throw new FormatException(string.Format("Career File Format Error[{0}]", "date/id/airGroup/missionFile"));
            }

            AllowDefensiveOperation = true;
        }

        #endregion

        public void InitQuickMssionInfo()
        {
            BattleType = EBattleType.Unknown;
            MissionType = null;
            Flight = (int)EFlight.Default;
            Formation = EFormation.Default;
            Spawn = (int)ESpawn.Default;
            Speed = -1;
            Fuel = -1;
            PlayerAirGroupSkill = null;
            Time = -1;
            Weather = -1;
            CloudAltitude = -1;
            BreezeActivity = -1;
            ThermalActivity = -1;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}",
                AirForces.Default.Where(x => x.ArmyIndex == ArmyIndex && x.AirForceIndex == AirForceIndex).FirstOrDefault().Ranks[RankIndex],
                PilotName);
        }

        public void WriteTo(ISectionFile careerFile, int historyMax)
        {
            careerFile.add(SectionMain, KeyVersion, VersionConverter.GetCurrentVersion().ToString());

            careerFile.add(SectionMain, KeyArmyIndex, ArmyIndex.ToString(Config.NumberFormat));
            careerFile.add(SectionMain, KeyAirForceIndex, AirForceIndex.ToString(Config.NumberFormat));
            careerFile.add(SectionMain, KeyRankIndex, RankIndex.ToString(Config.NumberFormat));
            careerFile.add(SectionMain, KeyExperience, Experience.ToString(Config.NumberFormat));

            careerFile.add(SectionCampaign, KeyDate, Date.Value.Year.ToString(Config.NumberFormat) + "-" + Date.Value.Month.ToString(Config.NumberFormat) + "-" + Date.Value.Day.ToString(Config.NumberFormat));
            careerFile.add(SectionCampaign, KeyTime, ((int)Time).ToString(Config.NumberFormat));
            careerFile.add(SectionCampaign, KeyAirGroup, AirGroup);
            careerFile.add(SectionCampaign, KeyMissionFile, MissionFileName);
            careerFile.add(SectionCampaign, KeyId, CampaignInfo.Id);
            careerFile.add(SectionCampaign, KeyAircraft, Aircraft);
            careerFile.add(SectionCampaign, KeyAdditionalAirOperations, AdditionalAirOperations.ToString(Config.NumberFormat));
            careerFile.add(SectionCampaign, KeyAdditionalGroundOperations, AdditionalGroundOperations.ToString(Config.NumberFormat));
            careerFile.add(SectionCampaign, KeyAdditionalAirGroups, AdditionalAirGroups ? "1" : "0");
            careerFile.add(SectionCampaign, KeyAirGroupDislplay, AirGroupDisplay ?? string.Empty);
            careerFile.add(SectionCampaign, KeySpawnRandomLocationPlayer, SpawnRandomLocationPlayer ? "1" : "0");
            careerFile.add(SectionCampaign, KeySpawnRandomLocationFriendly, SpawnRandomLocationFriendly ? "1" : "0");
            careerFile.add(SectionCampaign, KeySpawnRandomLocationEnemy, SpawnRandomLocationEnemy ? "1" : "0");
            careerFile.add(SectionCampaign, KeySpawnRandomAltitudeFriendly, SpawnRandomAltitudeFriendly ? "1" : "0");
            careerFile.add(SectionCampaign, KeySpawnRandomAltitudeEnemy, SpawnRandomAltitudeEnemy ? "1" : "0");
            careerFile.add(SectionCampaign, KeySpawnRandomTimeFriendly, SpawnRandomTimeFriendly ? "1" : "0");
            careerFile.add(SectionCampaign, KeySpawnRandomTimeEnemy, SpawnRandomTimeEnemy ? "1" : "0");
            careerFile.add(SectionCampaign, KeySpawnRandomTimeBeginSec, SpawnRandomTimeBeginSec.ToString(Config.NumberFormat));
            careerFile.add(SectionCampaign, KeySpawnRandomTimeEndSec, SpawnRandomTimeEndSec.ToString(Config.NumberFormat));
            careerFile.add(SectionCampaign, KeyReArm, ReArmTime >= 0 ? "1" : "0");
            careerFile.add(SectionCampaign, KeyReFuel, ReFuelTime >= 0 ? "1" : "0");
            careerFile.add(SectionCampaign, KeyTrackRecording, TrackRecording ? "1" : "0");
            careerFile.add(SectionCampaign, KeyAISkill, AISkill.ToString());
            careerFile.add(SectionCampaign, KeyCampaignProgress, CampaignProgress.ToString());

            careerFile.add(SectionStat, KeySorties, Sorties.ToString(Config.NumberFormat));
            careerFile.add(SectionStat, KeyTakeoffs, Takeoffs.ToString(Config.NumberFormat));
            careerFile.add(SectionStat, KeyLandings, Landings.ToString(Config.NumberFormat));
            careerFile.add(SectionStat, KeyBails, Bails.ToString(Config.NumberFormat));
            careerFile.add(SectionStat, KeyDeaths, Deaths.ToString(Config.NumberFormat));
            careerFile.add(SectionStat, KeyKills, Kills.ToString(Config.KillsFormat, Config.NumberFormat));
            foreach (var item in KillsHistory.OrderByDescending(x => x.Key).Take(historyMax))
            {
                careerFile.add(SectionKillsResult, item.Key.ToString(DateFormat, Config.DateTimeFormat), item.Value);
            }
            careerFile.add(SectionStat, KeyKillsGround, KillsGround.ToString(Config.KillsFormat, Config.NumberFormat));
            foreach (var item in KillsGroundHistory.OrderByDescending(x => x.Key).Take(historyMax))
            {
                careerFile.add(SectionKillsGroundResult, item.Key.ToString(DateFormat, Config.DateTimeFormat), item.Value);
            }

            if (PlayerAirGroupSkill != null)
            {
                int i = 0;
                foreach (var item in PlayerAirGroupSkill)
                {
                    careerFile.add(SectionSkill, string.Format(Config.NumberFormat, "{0}{1}", SectionSkill, i++), item.ToString());
                }
            }
        }

        public void ReadResult(ISectionFile careerFile, string separator = Config.CommaStr)
        {
            Sorties = careerFile.get(SectionStat, KeySorties, 0);
            Takeoffs = careerFile.get(SectionStat, KeyTakeoffs, 0);
            Landings = careerFile.get(SectionStat, KeyLandings, 0);
            Bails = careerFile.get(SectionStat, KeyBails, 0);
            Deaths = careerFile.get(SectionStat, KeyDeaths, 0);
            Kills = careerFile.get(SectionStat, KeyKills, 0f);
            VersionConverter.OptimizeSorties(this);

            string key;
            string value;
            DateTime dt;
            int lines;

            KillsHistory = new Dictionary<DateTime, string>();
            lines = careerFile.lines(SectionKillsResult);
            for (int i = 0; i < lines; i++)
            {
                careerFile.get(SectionKillsResult, i, out key, out value);
                if (DateTime.TryParseExact(key, DateFormat, Config.DateTimeFormat, DateTimeStyles.AssumeLocal, out dt))
                // if (DateTime.TryParse(key, out dt))
                {
                    if (KillsHistory.ContainsKey(dt.Date))
                    {
                        KillsHistory[dt.Date] += separator + value;
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
                if (DateTime.TryParseExact(key, DateFormat, Config.DateTimeFormat, DateTimeStyles.AssumeLocal, out dt))
                // if (DateTime.TryParse(key, out dt))
                {
                    if (KillsGroundHistory.ContainsKey(dt.Date))
                    {
                        KillsGroundHistory[dt.Date] += separator + value;
                    }
                    else
                    {
                        KillsGroundHistory.Add(dt.Date, value);
                    }
                }
            }
        }

        public void WriteResult(ISectionFile careerFile, int historyMax)
        {
            careerFile.add(SectionStat, KeySorties, Sorties.ToString(Config.NumberFormat));
            careerFile.add(SectionStat, KeyTakeoffs, Takeoffs.ToString(Config.NumberFormat));
            careerFile.add(SectionStat, KeyLandings, Landings.ToString(Config.NumberFormat));
            careerFile.add(SectionStat, KeyBails, Bails.ToString(Config.NumberFormat));
            careerFile.add(SectionStat, KeyDeaths, Deaths.ToString(Config.NumberFormat));
            careerFile.add(SectionStat, KeyKills, Kills.ToString(Config.KillsFormat, Config.NumberFormat));
            foreach (var item in KillsHistory.OrderByDescending(x => x.Key).Take(historyMax))
            {
                careerFile.add(SectionKillsResult, item.Key.ToString(DateFormat, Config.DateTimeFormat), item.Value);
            }
            careerFile.add(SectionStat, KeyKillsGround, KillsGround.ToString(Config.KillsFormat, Config.NumberFormat));
            foreach (var item in KillsGroundHistory.OrderByDescending(x => x.Key).Take(historyMax))
            {
                careerFile.add(SectionKillsGroundResult, item.Key.ToString(DateFormat, Config.DateTimeFormat), item.Value);
            }
        }

        public string ToStringCurrestStatus()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(PlayerCurrentStatusFormat, "Date", Date.Value.ToString(Date.Value.Hour != 0 ? "M/d/yyyy h tt": "M/d/yyyy", DateTimeFormatInfo.InvariantInfo));
            sb.AppendLine();
            sb.AppendFormat(PlayerCurrentStatusFormat, "Army", ((EArmy)ArmyIndex).ToString());
            sb.AppendLine();
            sb.AppendFormat(PlayerCurrentStatusFormat, "AirForce", ((EArmy)ArmyIndex) == EArmy.Red ? ((EAirForceRed)AirForceIndex).ToDescription() : ((EAirForceBlue)AirForceIndex).ToDescription());
            sb.AppendLine();
            sb.AppendFormat(PlayerCurrentStatusFormat, "Rank", AirForces.Default.Where(x => x.ArmyIndex == ArmyIndex && x.AirForceIndex == AirForceIndex).FirstOrDefault().Ranks[RankIndex]);
            sb.AppendLine();
            sb.AppendFormat(PlayerCurrentStatusFormat, "AirGroup", string.IsNullOrEmpty(AirGroupDisplay) ? MissionObjectModel.AirGroup.CreateDisplayName(AirGroup) : AirGroupDisplay);
            sb.AppendLine();
            sb.AppendFormat(PlayerCurrentStatusFormat, "Aircraft", Aircraft);
            sb.AppendLine();
            sb.AppendFormat(PlayerCurrentStatusFormat, "Experience", Experience);
            return sb.ToString();
        }

        public string ToStringTotalResult()
        {
            return PlayerStats.ToStringTotalResult(this, PlayerStats.PlayerStatTotalFormat, " {0:d} {1}\n  {2}", "\n  ");
        }

        public void InitializeExperience()
        {
            Experience = RankIndex * Config.RankupExp;
        }

        public void UpdateExperience(EBattleResult result)
        {
            if (result == EBattleResult.SUCCESS)
            {
                Experience += Config.ExpSuccess;
            }
            else if (result == EBattleResult.DRAW)
            {
                Experience += Config.ExpDraw;
            }

            if (RankIndex < Rank.RankMax && Experience >= (RankIndex + 1) * Config.RankupExp)
            {
                RankIndex += 1;
            }
        }

        public void InitializeDateTime(Config config = null, IRandom random = null)
        {
            if (config != null && random != null)
            {
                Time = random.Next(config.RandomTimeBegin, config.RandomTimeEnd + 1);
                Date = CampaignInfo.StartDate.AddHours(Time);
            }
            else if (config != null)
            {
                Date = CampaignInfo.StartDate;
            }
            else
            {
                Date = null;
            }
        }

        public void ProgressDateTime(Config config, IRandom random)
        {
            if (Date.Value.Hour == 0)
            {
                Date = Date.Value.AddHours(config.RandomTimeEnd);
            }
            DateTime dt;
            switch (CampaignProgress)
            {
                case ECampaignProgress.AnyDay:
                    dt = Date.Value.Add(new TimeSpan(
                                                    random.Next(MissionObjectModel.CampaignProgress.AnyDayBebin, MissionObjectModel.CampaignProgress.AnyDayEnd + 1) , 
                                                    random.Next(MissionObjectModel.CampaignProgress.AnyTimeBebin, MissionObjectModel.CampaignProgress.AnyTimeEnd + 1) - Date.Value.Hour, 
                                                    0, 0));
                    break;

                case ECampaignProgress.AnyTime:
                    dt = Date.Value.AddHours(random.Next(MissionObjectModel.CampaignProgress.AnyTimeBebin, MissionObjectModel.CampaignProgress.AnyTimeEnd + 1));
                    break;

                case ECampaignProgress.AnyDayAnyTime:
                    dt = Date.Value.AddHours(random.Next(MissionObjectModel.CampaignProgress.AnyDayAnyTimeBebin, MissionObjectModel.CampaignProgress.AnyDayAnyTimeEnd + 1));
                    break;

                case ECampaignProgress.Daily:
                default:
                    dt = Date.Value.Add(new TimeSpan(
                                                    MissionObjectModel.CampaignProgress.DailyDay, 
                                                    random.Next(MissionObjectModel.CampaignProgress.AnyTimeBebin, MissionObjectModel.CampaignProgress.AnyTimeEnd + 1) - Date.Value.Hour, 
                                                    0, 0));
                    break;
            }
            if (dt.Date > CampaignInfo.EndDate)
            {
                CampaignInfo.EndDate = dt.Date;
            }
            if (dt.Hour < config.RandomTimeBegin)
            {
                dt = dt.AddHours(config.RandomTimeBegin - dt.Hour);
            }
            else if (dt.Hour > config.RandomTimeEnd)
            {
                dt = dt.AddHours(dt.Date < CampaignInfo.EndDate ? config.RandomTimeBegin + 24 - dt.Hour: config.RandomTimeEnd - dt.Hour);
            }
            Date = dt;
            Time = dt.Hour;
        }
    }
}