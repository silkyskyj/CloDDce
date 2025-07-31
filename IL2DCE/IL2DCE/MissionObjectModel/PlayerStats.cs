// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover
// Copyright (C) 2016 Stefan Rothdach & 2025 silkysky
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IL2DCE.Generator;
using IL2DCE.Util;
using maddox.game;
using maddox.game.world;
using static IL2DCE.MissionObjectModel.MissionStatus;
using static IL2DCE.MissionObjectModel.Skill;

namespace IL2DCE.MissionObjectModel
{
    public enum EPlayerStatsType
    {
        ActorDeadKillsScoreOver = 0,
        Api = 1,
        DamageVictimsKillsScoreOver = 2,
        ActorDeadKillsHighestScore = 3,
        DamageVictimsHighestScore = 4,
        Count,
    }

    public enum EPlayerStatus
    {
        [Description("alive")]
        Alive = 0,

        [Description("dead")]
        Dead = 1,

        count,
    }

    public interface IPlayerStatTotal
    {
        long FlyingTime
        {
            get;
            set;
        }

        int Sorties
        {
            get;
            set;
        }

        int Takeoffs
        {
            get;
            set;
        }

        int Landings
        {
            get;
            set;
        }

        int Bails
        {
            get;
            set;
        }

        int Deaths
        {
            get;
            set;
        }

        double Kills
        {
            get;
            set;
        }

        double KillsGround
        {
            get;
            set;
        }

        Dictionary<DateTime, string> KillsHistory
        {
            get;
            set;
        }

        Dictionary<DateTime, string> KillsGroundHistory
        {
            get;
            set;
        }
    }

    public class PlayerStats
    {
        #region Definition

        public const char ActorDeadInfoSplitChar = '|';

        public const string PlayerStatFormat = "{0,19}: {1,2}";
        public const string PlayerStatTotalFormat = "{0,17}: {1,5}";
        public const string PlayerStatKillsHistoryFormat = " {0:d} {1} {2}";
        public const string PlayerStatKillsTypeFormat = "{0,19}: {1}";
        public const string PlayerStatTimeSpanShortFormat = "hh\\:mm\\:ss";
        public const string PlayerStatTimeSpanLongFormat = "d\\.hh\\:mm\\:ss";

        private const string KeyFlyingTime = "FlyingTime";
        private const string KeySorties = "Sorties";
        private const string KeyTakeoff = "Takeoffs";
        private const string KeyLandings = "Landings";
        private const string KeyDeaths = "Deaths";
        private const string KeyBails = "Bails";
        private const string KeyAircraft = "Aircraft";
        private const string KeyGroundUnit = "GroundUnit";
        private const string KeyAircraftKills = "Aircraft Kills";
        private const string KeyGroundUnitKills = "GroundUnit Kills";
        private const string KeyEnemyAircraft = "Enemy Aircraft";
        private const string KeyEnemyGroundUnit = "Enemy GroundUnit";
        private const string KeyFriendlyAircraft = "Friendly Aircraft";
        private const string KeyFriendlyGroundUnit = "Friendly GroundUnit";
        private const string KeyDitches = "Ditches";
        private const string KeyPlanesWrittenOff = "PlanesWrittenOff";
        private const string KeyKills = "Kills";
        private const string KeyFriendlyKills = "Friendly Kills";
        private const string KeyKillsHistory = "Kills History";

        public enum ActorDeadInfoKey
        {
            Army,
            ActorType,
            ActorName,
            ActorTypeName,
            Count,
        }

        #endregion

        #region Property

        public IGame Game
        {
            get;
            set;
        }

        public string PlayerActorName
        {
            get;
            set;
        }

        public double KillsScoreOver
        {
            get;
            set;
        }

        public int KillsAircraftTotal
        {
            get
            {
                return killsAircraft.Sum(x => x.Value);
            }
        }

        public int KillsGroundUnitTotal
        {
            get
            {
                return killsGroundUnit.Sum(x => x.Value);
            }
        }

        public int KillsFriendlyAircraftTotal
        {
            get
            {
                return killsFriendlyAircraft.Sum(x => x.Value);
            }
        }

        public int KillsFriendyGroundUnitTotal
        {
            get
            {
                return killsFriendlyGroundUnit.Sum(x => x.Value);
            }
        }

        public EPlayerStatsType StatsType
        {
            get;
            set;
        }

        #endregion

        #region Variable

        private Dictionary<string, int> killsAircraft;
        private Dictionary<string, int> killsFriendlyAircraft;
        private Dictionary<string, int> killsGroundUnit;
        private Dictionary<string, int> killsFriendlyGroundUnit;

        #endregion

        public PlayerStats(IGame game, string playerActorName, double killsScoreOver)
        {
            Game = game;
            PlayerActorName = playerActorName;
            KillsScoreOver = killsScoreOver;
            killsAircraft = new Dictionary<string, int>();
            killsFriendlyAircraft = new Dictionary<string, int>();
            killsGroundUnit = new Dictionary<string, int>();
            killsFriendlyGroundUnit = new Dictionary<string, int>();
        }

        public EPlayerStatsType Create(EPlayerStatsType statType, object data)
        {
            switch (statType)
            {
                case EPlayerStatsType.ActorDeadKillsScoreOver: // Check Mission OnActorDead method score value (KillsScoreOver)
                    {
                        // Check Type 2: 
                        Dictionary<string, List<DamagerScore>> actorDead = data as Dictionary<string, List<DamagerScore>>;
                        if (actorDead != null && !string.IsNullOrEmpty(PlayerActorName))
                        {
                            CreatePlayerStat(actorDead, true);
                        }
                        else
                        {
                            Debug.Assert(false, "Mission is null or PlayerActorName is empty");
                            statType = EPlayerStatsType.Api;
                        }
                    }
                    break;

                case EPlayerStatsType.Api: // DefaultAPI
                    {
                        ;
                    }
                    break;

                case EPlayerStatsType.DamageVictimsKillsScoreOver: // Battle's DamageInfo (KillsScoreOver)
                    {
                        ArrayList battleDamageVictims = Game.battleGetDamageVictims();
                        if (battleDamageVictims != null)
                        {
                            CreatePlayerStat(battleDamageVictims, true);
                        }
                        else
                        {
                            Debug.Assert(false, "battleDamageVictims is nul");
                            statType = EPlayerStatsType.Api;
                        }
                    }
                    break;

                case EPlayerStatsType.ActorDeadKillsHighestScore: // Check Mission OnActorDead method score value (HighestScore)
                    {
                        Dictionary<string, List<DamagerScore>> actorDead = data as Dictionary<string, List<DamagerScore>>;
                        if (actorDead != null && !string.IsNullOrEmpty(PlayerActorName))
                        {
                            CreatePlayerStat(actorDead, false);
                        }
                        else
                        {
                            Debug.Assert(false, "Mission is null or PlayerActorName is empty");
                            statType = EPlayerStatsType.Api;
                        }
                    }
                    break;

                case EPlayerStatsType.DamageVictimsHighestScore: // Battle's DamageInfo (HighestScore)
                    {
                        ArrayList battleDamageVictims = Game.battleGetDamageVictims();
                        if (battleDamageVictims != null)
                        {
                            CreatePlayerStat(battleDamageVictims, false);
                        }
                        else
                        {
                            Debug.Assert(false, "battleDamageVictims is nul");
                            statType = EPlayerStatsType.Api;
                        }
                    }
                    break;

            }
            StatsType = statType;
            return statType;
        }

        private void CreatePlayerStat(Dictionary<string, List<DamagerScore>> actorDead, bool calcKillsScoreOver)
        {
            char[] split = new char[] { ActorDeadInfoSplitChar };
            var playerDamegedActor = actorDead.Where(x => x.Value.ToArray().Any(y => y.initiator != null && y.initiator.Player != null));
            List<string> playerKillsActor = new List<string>();
            foreach (var item in playerDamegedActor)
            {
                string[] keys = item.Key.Split(split);
                if (keys.Length >= (int)ActorDeadInfoKey.Count)
                {
                    int armyActor;
                    int actorType;
                    if (int.TryParse(keys[(int)ActorDeadInfoKey.Army], out armyActor) && int.TryParse(keys[(int)ActorDeadInfoKey.ActorType], out actorType))
                    {
                        if (string.Compare(keys[(int)ActorDeadInfoKey.ActorName], PlayerActorName, true) != 0)
                        {
                            double totalScore = item.Value.Sum(x => x.score);
                            var playerDameges = item.Value.Where(x => x.initiator != null && x.initiator.Player != null).OrderByDescending(x => x.score);
                            if (playerDameges.Any())
                            {
                                int army = CloDAPIUtil.GetActorArmy(playerDameges.First().initiator);
                                if (army != (int)EArmy.None)
                                {
                                    double playerScore = playerDameges.Sum(x => x.score);
                                    if (calcKillsScoreOver)
                                    {
                                        if (playerScore > totalScore * KillsScoreOver)
                                        {
                                            Debug.WriteLine("Paler Kill: {0}={1}/{2}", item.Key, playerScore, totalScore);
                                            string typeName = keys[(int)ActorDeadInfoKey.ActorTypeName];
                                            AddKillsCount(armyActor, actorType, typeName, army);
                                        }
                                    }
                                    else
                                    {
                                        AiDamageInitiator initiator = GetHighestDamagedInitiator(item.Value);
                                        if (initiator != null && initiator.Player != null)
                                        {
                                            Debug.WriteLine("Paler Kill: {0}={1}/{2}", item.Key, playerScore, totalScore);
                                            string typeName = keys[(int)ActorDeadInfoKey.ActorTypeName];
                                            AddKillsCount(armyActor, actorType, typeName, army);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreatePlayerStat(ArrayList battleDamageVictims, bool calcKillsScoreOver)
        {
            Debug.WriteLine(string.Format("CreatePlayerStatKillsScoreOver(Count={0})", battleDamageVictims.Count));
            IEnumerable<AiActor> actorDamageInitiatorsArray = battleDamageVictims.ToArray().Where(x => x is AiActor && !(x as AiActor).IsAlive()).Select(x => x as AiActor);
            foreach (AiActor actor in actorDamageInitiatorsArray)
            {
                Debug.WriteLine("battleGetDamageVictims Army={0} Actor={1} IsValid={2} IsAlive={3}", actor.Army(), actor.Name(), actor.IsValid(), actor.IsAlive());
                IEnumerable<DamagerScore> damages = Game.battleGetDamageInitiators(actor).ToArray().Where(x => x is DamagerScore).Select(x => x as DamagerScore);

                if (damages.Any())
                {
                    if (calcKillsScoreOver)
                    {
                        double totalScore = damages.Sum(x => x.score);
                        var playerDameges = damages.Where(x => x.initiator != null && x.initiator.Player != null).OrderByDescending(x => x.score);
                        if (playerDameges.Any())
                        {
                            int army = CloDAPIUtil.GetActorArmy(playerDameges.First().initiator);
                            if (army != (int)EArmy.None)
                            {
                                double playerScore = playerDameges.Sum(x => x.score);
                                Debug.WriteLine("   PaylerScore/TotalScore=[{0}/{1}]", playerScore, totalScore);
                                if (playerScore > totalScore * KillsScoreOver)
                                {
                                    AddKillsCount(actor, army);
                                }
                            }
                        }
                    }
                    else
                    {
                        AiDamageInitiator initiator = GetHighestDamagedInitiator(damages);
                        int army = CloDAPIUtil.GetActorArmy(initiator);
                        if (army != (int)EArmy.None)
                        {
                            if (initiator != null && initiator.Player != null)
                            {
                                AddKillsCount(actor, army);
                            }
                        }
                    }
                }
            }
        }

        private AiDamageInitiator GetHighestDamagedInitiator(IEnumerable<DamagerScore> damages)
        {
            AiDamageInitiator initiator = null;
            double scoreHighest = 0;
            string name;
            IEnumerable<string> actorNames = damages.Where(x => !string.IsNullOrEmpty(name = CloDAPIUtil.GetName(x.initiator))).Select(x => CloDAPIUtil.GetName(x.initiator)).Distinct();
            foreach (string actorName in actorNames)
            {
                var damageScores = damages.Where(x => string.Compare(CloDAPIUtil.GetName(x.initiator), actorName) == 0);
                double score = damageScores.Sum(x => x.score);
                if (score > scoreHighest)
                {
                    scoreHighest = score;
                    initiator = damageScores.First().initiator;
                }
            }
            return initiator;
        }


        private void AddKillsCount(AiActor actor, int army)
        {
            int armyActor = actor.Army();
            if (actor is AiAircraft)
            {
                AiAircraft aiAircraft = actor as AiAircraft;
                Debug.WriteLine("  AiAircraft: {0}={1}", MissionActorObj.GetInternalTypeName(aiAircraft), aiAircraft.Group() != null ? aiAircraft.Group().Name() : string.Empty);
                if (armyActor == MissionObjectModel.Army.Enemy(army))
                {
                    AddKillsCount(killsAircraft, MissionActorObj.GetInternalTypeName(aiAircraft));
                }
                else
                {
                    AddKillsCount(killsFriendlyAircraft, MissionActorObj.GetInternalTypeName(aiAircraft));
                }
            }
            else if (actor is AiGroundActor)
            {
                AiGroundActor aiGroundActor = actor as AiGroundActor;
                Debug.WriteLine("  AiGroundActor: {0}={1}", MissionActorObj.GetInternalTypeName(aiGroundActor), aiGroundActor.Group() != null ? aiGroundActor.Group().Name() : string.Empty);
                if (armyActor == MissionObjectModel.Army.Enemy(army))
                {
                    AddKillsCount(killsGroundUnit, MissionActorObj.GetInternalTypeName(aiGroundActor));
                }
                else
                {
                    AddKillsCount(killsFriendlyGroundUnit, MissionActorObj.GetInternalTypeName(aiGroundActor));
                }
            }
        }

        private void AddKillsCount(int armyActor, int actorType, string typeName, int army)
        {
            if (armyActor == MissionObjectModel.Army.Enemy(army))
            {   // Enemy kill
                if (actorType == 0)
                {
                    AddKillsCount(killsAircraft, typeName);
                }
                else
                {
                    AddKillsCount(killsGroundUnit, typeName);
                }
            }
            else
            {   // Friendly kill
                if (actorType == 0)
                {
                    AddKillsCount(killsFriendlyAircraft, typeName);
                }
                else
                {
                    AddKillsCount(killsFriendlyGroundUnit, typeName);
                }
            }
        }

        private void AddKillsCount(Dictionary<string, int> dic, string name, int count = 1)
        {
            const string delStart = ".";
            int idx = name.IndexOf(delStart, StringComparison.CurrentCultureIgnoreCase);
            if (idx != -1)
            {
                name = name.Substring(idx + delStart.Length);
            }
            if (string.IsNullOrEmpty(name))
            {
                name = MissionStatus.ValueNoName;
            }
            if (dic.ContainsKey(name))
            {
                dic[name] += count;
            }
            else
            {
                dic.Add(name, count);
            }
        }

        public string KillsAircraftToStrings(string separator = Config.CommaStr)
        {
            return ToString(killsAircraft, separator);
        }

        public string KillsGroundUnitToStrings(string separator = Config.CommaStr)
        {
            return ToString(killsGroundUnit, separator);
        }

        public string KillsFriendlyAircraftToStrings(string separator = Config.CommaStr)
        {
            return ToString(killsFriendlyAircraft, separator);
        }

        public string KillsFriendlyGroundUnitToStrings(string separator = Config.CommaStr)
        {
            return ToString(killsFriendlyGroundUnit, separator);
        }

        public void UpdatePlayerStatsDefaultAPI(IPlayerStatTotal playerStatTotal, DateTime dt, string valueSummary = null, string separator = Config.CommaStr)
        {
            IGameSingle game = (Game as IGameSingle);
            IPlayer player = game.gameInterface.Player();
            IPlayerStat st = player.GetBattleStat();

            playerStatTotal.FlyingTime += (long)st.tTotalTypes.Sum(x => x.Value);
            playerStatTotal.Sorties += 1;
            playerStatTotal.Takeoffs += st.takeoffs;
            playerStatTotal.Landings += st.landings;
            playerStatTotal.Bails += st.bails;
            playerStatTotal.Deaths += st.deaths;
            playerStatTotal.Kills += ((int)(st.kills * 100)) / 100.0;
            string killsTypes = ToStringkillsTypes(st.killsTypes, separator);
            if (!string.IsNullOrEmpty(killsTypes))
            {
                if (string.IsNullOrEmpty(valueSummary))
                {
                    if (playerStatTotal.KillsHistory.ContainsKey(dt.Date))
                    {
                        playerStatTotal.KillsHistory[dt.Date] += separator + killsTypes;
                    }
                    else
                    {
                        playerStatTotal.KillsHistory.Add(dt.Date, killsTypes);
                    }
                }
                else
                {
                    string val = string.Format("{0}|{1}", valueSummary, killsTypes);
                    if (playerStatTotal.KillsHistory.ContainsKey(dt.Date))
                    {
                        playerStatTotal.KillsHistory[dt.Date] += separator + val;
                    }
                    else
                    {
                        playerStatTotal.KillsHistory.Add(dt.Date, val);
                    }
                }
            }
        }

        public void Update(IPlayerStatTotal playerStatTotal, DateTime dt, string valueSummary = null, string separator = Config.CommaStr)
        {
            if (StatsType == EPlayerStatsType.Api)
            {
                UpdatePlayerStatsDefaultAPI(playerStatTotal, dt, valueSummary, separator);
            }
            else
            {
                IPlayerStat st = Game.gameInterface.Player().GetBattleStat();
                playerStatTotal.FlyingTime += (long)st.tTotalTypes.Sum(x => x.Value);
                playerStatTotal.Sorties += 1;
                playerStatTotal.Takeoffs += st.takeoffs;
                playerStatTotal.Landings += st.landings;
                playerStatTotal.Bails += st.bails;
                playerStatTotal.Deaths += st.deaths;
                playerStatTotal.Kills += KillsAircraftTotal;
                string killsTypes = KillsAircraftToStrings(separator);
                if (!string.IsNullOrEmpty(killsTypes))
                {
                    if (string.IsNullOrEmpty(valueSummary))
                    {
                        if (playerStatTotal.KillsHistory.ContainsKey(dt.Date))
                        {
                            playerStatTotal.KillsHistory[dt.Date] += separator + killsTypes;
                        }
                        else
                        {
                            playerStatTotal.KillsHistory.Add(dt.Date, killsTypes);
                        }
                    }
                    else
                    {
                        string val = string.Format("{0}|{1}", valueSummary, killsTypes);
                        if (playerStatTotal.KillsHistory.ContainsKey(dt.Date))
                        {
                            playerStatTotal.KillsHistory[dt.Date] += separator + val;
                        }
                        else
                        {
                            playerStatTotal.KillsHistory.Add(dt.Date, val);
                        }
                    }
                }

                playerStatTotal.KillsGround += KillsGroundUnitTotal;
                killsTypes = KillsGroundUnitToStrings();
                if (!string.IsNullOrEmpty(killsTypes))
                {
                    if (string.IsNullOrEmpty(valueSummary))
                    {
                        if (playerStatTotal.KillsGroundHistory.ContainsKey(dt.Date))
                        {
                            playerStatTotal.KillsGroundHistory[dt.Date] += separator + killsTypes;
                        }
                        else
                        {
                            playerStatTotal.KillsGroundHistory.Add(dt.Date, killsTypes);
                        }
                    }
                    else
                    {
                        string val = string.Format("{0}|{1}", valueSummary, killsTypes);
                        if (playerStatTotal.KillsGroundHistory.ContainsKey(dt.Date))
                        {
                            playerStatTotal.KillsGroundHistory[dt.Date] += separator + val;
                        }
                        else
                        {
                            playerStatTotal.KillsGroundHistory.Add(dt.Date, val);
                        }
                    }
                }
            }
        }

        public int Digit()
        {
            return Digit(this);
        }

        public static int Digit(PlayerStats playerStats)
        {
            int max = (new int[] { playerStats.KillsAircraftTotal, playerStats.KillsGroundUnitTotal, playerStats.KillsFriendlyAircraftTotal,
                                                                                                        playerStats.KillsFriendyGroundUnitTotal,}).Max();
            return max > 0 ? (int)Math.Ceiling(Math.Log10(max)) : 1;
        }

        public static int Digit(IPlayerStatTotal playerStatTotal)
        {
            int max = (new int[] { playerStatTotal.Sorties, playerStatTotal.Takeoffs, playerStatTotal.Landings, playerStatTotal.Deaths,
                                                                playerStatTotal.Bails, (int)playerStatTotal.Kills, (int)playerStatTotal.KillsGround, }).Max();
            return max > 0 ? (int)Math.Ceiling(Math.Log10(max)) : 1;
        }

        public static int Digit(IPlayerStat playerStat, bool AddKills)
        {
            int[] target = new int[] { playerStat.takeoffs, playerStat.landings, playerStat.deaths, playerStat.bails, playerStat.ditches, playerStat.planesWrittenOff, };
            int max = (AddKills ? target.Concat(new int[] { (int)playerStat.kills, (int)playerStat.fkills, }) : target).Max();
            return max > 0 ? (int)Math.Ceiling(Math.Log10(max)) : 1;
        }

        public static string ToStringTotalResult(IPlayerStatTotal playerStatTotal, string format = PlayerStatTotalFormat, string formatHistory = PlayerStatKillsHistoryFormat, string separator = Config.CommaStr, bool optimizeDigit = false)
        {
            StringBuilder sbHistory = new StringBuilder();
            List<DateTime> dtList = playerStatTotal.KillsHistory.Keys.ToList();
            dtList.AddRange(playerStatTotal.KillsGroundHistory.Keys);
            var orderd = dtList.Distinct().OrderByDescending(x => x.Date);
            foreach (var item in orderd)
            {
                if (playerStatTotal.KillsHistory.ContainsKey(item))
                {
                    sbHistory.AppendFormat(DateTimeFormatInfo.InvariantInfo, formatHistory, item, KeyAircraft, FormatingDisplayKillsHistoryValue(playerStatTotal.KillsHistory[item], separator));
                    sbHistory.AppendLine();
                }
                if (playerStatTotal.KillsGroundHistory.ContainsKey(item))
                {
                    sbHistory.AppendFormat(DateTimeFormatInfo.InvariantInfo, formatHistory, item, KeyGroundUnit, FormatingDisplayKillsHistoryValue(playerStatTotal.KillsGroundHistory[item], separator));
                    sbHistory.AppendLine();
                }
            }

            if (optimizeDigit)
            {
                int digit = Digit(playerStatTotal);
                format = Regex.Replace(format, @"\{1,\d}", "{1," + digit.ToString(Config.NumberFormat) + "}");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(format, KeyFlyingTime, ToStringFlyingTime(playerStatTotal.FlyingTime));
            sb.AppendLine();
            sb.AppendFormat(format, KeySorties, playerStatTotal.Sorties);
            sb.AppendLine();
            sb.AppendFormat(format, KeyTakeoff, playerStatTotal.Takeoffs);
            sb.AppendLine();
            sb.AppendFormat(format, KeyLandings, playerStatTotal.Landings);
            sb.AppendLine();
            sb.AppendFormat(format, KeyDeaths, playerStatTotal.Deaths);
            sb.AppendLine();
            sb.AppendFormat(format, KeyBails, playerStatTotal.Bails);
            sb.AppendLine();
            sb.AppendFormat(format, KeyAircraftKills, playerStatTotal.Kills.ToString(Config.KillsFormat, Config.NumberFormat));
            sb.AppendLine();
            sb.AppendFormat(format, KeyGroundUnitKills, playerStatTotal.KillsGround.ToString(Config.KillsFormat, Config.NumberFormat));
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("{0}:", KeyKillsHistory);
            sb.AppendLine();
            sb.Append(sbHistory.ToString());
            return sb.ToString();
        }

        public static string FormatingDisplayKillsHistoryValue(string val, string separator = Config.CommaStr)
        {
            StringBuilder sb = new StringBuilder();
            string[] vals = val.Split(Config.SplitComma, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in vals)
            {
                string[] valOnes = item.Split(Config.SplitOr, StringSplitOptions.RemoveEmptyEntries);
                if (valOnes.Length >= 4)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(separator);
                    }
                    sb.AppendFormat("[{0}] {1} ({2}) {3}", valOnes[0].Trim(), valOnes[1].Trim(), valOnes[2].Trim(), valOnes[3].Trim());
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendFormat("{0}", " ");
                    }
                    sb.Append(item.Trim());
                }
            }
            return sb.ToString();
        }

        public string ToStringTotal(string format = PlayerStatFormat)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(format, KeyEnemyAircraft, KillsAircraftTotal);
            sb.AppendLine();
            sb.AppendFormat(format, KeyEnemyGroundUnit, KillsGroundUnitTotal);
            sb.AppendLine();
            sb.AppendFormat(format, KeyFriendlyAircraft, KillsFriendlyAircraftTotal);
            sb.AppendLine();
            sb.AppendFormat(format, KeyFriendlyGroundUnit, KillsFriendyGroundUnitTotal);
            sb.AppendLine();
            return sb.ToString();
        }

        public string ToStringKillsType(string format = PlayerStatKillsTypeFormat, string separator = Config.CommaStr)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(format, KeyEnemyAircraft, KillsAircraftToStrings(separator));
            sb.AppendLine();
            sb.AppendFormat(format, KeyEnemyGroundUnit, KillsGroundUnitToStrings(separator));
            sb.AppendLine();
            sb.AppendFormat(format, KeyFriendlyAircraft, KillsFriendlyAircraftToStrings(separator));
            sb.AppendLine();
            sb.AppendFormat(format, KeyFriendlyGroundUnit, KillsFriendlyGroundUnitToStrings(separator));
            sb.AppendLine();
            return sb.ToString();
        }

        public static string ToStringSummary(IPlayerStat stat, bool AddKills = true, string format = PlayerStatFormat, bool optimizeDigit = false)
        {
            if (optimizeDigit)
            {
                int digit = Digit(stat, optimizeDigit);
                format = Regex.Replace(format, @"\{1,\d}", "{1," + digit.ToString(Config.NumberFormat) + "}");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(format, KeyTakeoff, stat.takeoffs);
            sb.AppendLine();
            sb.AppendFormat(format, KeyLandings, stat.landings);
            sb.AppendLine();
            sb.AppendFormat(format, KeyDeaths, stat.deaths);
            sb.AppendLine();
            sb.AppendFormat(format, KeyBails, stat.bails);
            sb.AppendLine();
            sb.AppendFormat(format, KeyDitches, stat.ditches);
            sb.AppendLine();
            sb.AppendFormat(format, KeyPlanesWrittenOff, stat.planesWrittenOff);
            sb.AppendLine();
            if (AddKills)
            {
                sb.AppendFormat(format, KeyKills, stat.kills.ToString(Config.KillsFormat, Config.NumberFormat));
                sb.AppendLine();
                sb.AppendFormat(format, KeyFriendlyKills, stat.fkills.ToString(Config.KillsFormat, Config.NumberFormat));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string ToString<T>(Dictionary<string, T> dic, string separator = Config.CommaStr)
        {
            return string.Join(separator, dic.Select(x => string.Format("{0} {1}", x.Key, x.Value)));
        }

        public static string ToStringkillsTypes(Dictionary<string, double> dic, string separator = Config.CommaStr)
        {
            return string.Join(separator, dic.Select(x => string.Format("{0} {1}", AircraftInfo.CreateDisplayName(x.Key), x.Value.ToString(Config.KillsFormat, Config.NumberFormat))));
        }

        public static string ToStringFlyingTime(long time)
        {
            TimeSpan tm = new TimeSpan(0, 0, (int)time);
            return tm.Days < 1 ? tm.ToString(PlayerStatTimeSpanShortFormat, Config.DateTimeFormat) : tm.ToString(PlayerStatTimeSpanLongFormat, Config.DateTimeFormat);
        }

        private DamagerScore[] GetPlayerDamageScore(ArrayList listDamage)
        {
            return listDamage.ToArray().Where(x => x is DamagerScore && (x as DamagerScore).initiator.Player != null).ToArray() as DamagerScore[];
        }

        public bool IsPlayerAlive(bool primary = false)
        {
            IPlayer player = (Game as IGameSingle).gameInterface.Player();
            if (primary)
            {
                return player.PersonPrimary() != null && player.PersonPrimary().IsAlive();
            }
            else
            {
                return player.GetBattleStat().deaths == 0;
            }
        }

        public void UpdateSkill(float[] skills, EBattleResult result, MissionStatus missionStatus)
        {
            if (skills != null && skills.Length >= (int)ESkilType.Count)
            {
                IPlayerStat st = Game.gameInterface.Player().GetBattleStat();

                long flyingTime = (long)st.tTotalTypes.Sum(x => x.Value);
                int flying = st.takeoffs + st.landings;
                if (flying > 0)
                {
                    if (flyingTime >= 600)
                    {
                        skills[(int)ESkilType.BasicFlying] += 0.01f;
                        skills[(int)ESkilType.Awareness] += 0.01f;
                        skills[(int)ESkilType.Vision] += 0.01f;
                        if (flyingTime >= 1200)
                        {
                            skills[(int)ESkilType.Discipline] += 0.01f;
                            if (flying > 1 && flyingTime >= 1800)
                            {
                                skills[(int)ESkilType.AdvancedFlying] += 0.01f;
                            }
                        }
                    }
                }
                double kills = StatsType == EPlayerStatsType.Api ? st.kills : KillsAircraftTotal;
                double gkills = StatsType == EPlayerStatsType.Api ? st.gkills.Sum() : KillsGroundUnitTotal;
                double fkills = StatsType == EPlayerStatsType.Api ? st.fkills : KillsFriendlyAircraftTotal;
                double fgkills = StatsType == EPlayerStatsType.Api ? st.fgkills.Sum() : KillsFriendyGroundUnitTotal;
                if (kills + gkills > 0)
                {
                    skills[(int)ESkilType.AerialGunnnery] += (float)Math.Floor(kills + gkills / 2) * 0.01f;
                    skills[(int)ESkilType.Discipline] += 0.01f;
                }
                if (fkills + fgkills > 0)
                {
                    skills[(int)ESkilType.AerialGunnnery] -= (float)Math.Floor(kills + gkills / 3) * 0.02f;
                    skills[(int)ESkilType.Discipline] -= (float)Math.Floor(kills + gkills / 3) * 0.03f;
                }

                skills[(int)ESkilType.Tactics] += result == EBattleResult.SUCCESS ? 0.02f : result == EBattleResult.FAIL ? -0.02f : 0f;

                if (st.bails > 0 && st.deaths == 0)
                {
                    skills[(int)ESkilType.Bravery] += 0.01f;
                }

                if (missionStatus != null)
                {
                    AirGroupObj airGroup = missionStatus.GetPlayerAirGroup();
                    if (airGroup != null)
                    {
                        if (airGroup.DiedNums > 0)
                        {
                            skills[(int)ESkilType.Discipline] -= airGroup.DiedNums * 0.01f;
                            if (airGroup.InitNums > 0)
                            {
                                skills[(int)ESkilType.Tactics] -= airGroup.DiedNums / airGroup.InitNums * 0.05f;
                            }
                        }
                    }
                }

                for (int i = 0; i < skills.Length; i++)
                {
                    skills[i] = Math.Min((float)Math.Floor(skills[i] * 100) / 100, 1.0f);
                }
            }
        }

        public void UpdateSkill(Skill skill, EBattleResult result, MissionStatus missionStatus)
        {
            UpdateSkill(skill.Skills, result, missionStatus);
        }

        public void UpdateSkills(IEnumerable<Skill> skills, EBattleResult result, MissionStatus missionStatus)
        {
            if (skills != null)
            {
                foreach (var item in skills)
                {
                    UpdateSkill(item, result, missionStatus);
                }
            }
        }
    }
}
