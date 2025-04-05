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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using IL2DCE.Generator;
using maddox.game;
using maddox.game.world;

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

    public interface IPlayerStatTotal
    {
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
        #region definition

        public const char ActorDeadInfoSplitChar = '|';

        public const string PlayerStatFormat = "{0,19}: {1,2}";
        public const string PlayerStatTotalFormat = "{0,17}: {1,5}";
        public const string PlayerStatKillsHistoryFormat = " {0:d} {1} {2}";
        public const string PlayerStatKillsTypeFormat = "{0,19}: {1}";

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

        public int Army
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

        public int KillsFliendlyAircraftTotal
        {
            get
            {
                return killsFliendlyAircraft.Sum(x => x.Value);
            }
        }

        public int KillsFliendlyGroundUnitTotal
        {
            get
            {
                return killsFliendlyGroundUnit.Sum(x => x.Value);
            }
        }

        #endregion

        #region Variable

        private Dictionary<string, int> killsAircraft = new Dictionary<string, int>();
        private Dictionary<string, int> killsFliendlyAircraft = new Dictionary<string, int>();
        private Dictionary<string, int> killsGroundUnit = new Dictionary<string, int>();
        private Dictionary<string, int> killsFliendlyGroundUnit = new Dictionary<string, int>();

        #endregion

        public PlayerStats(IGame game, int army, string playerActorName, double killsScoreOver)
        {
            Game = game;
            Army = army;
            PlayerActorName = playerActorName;
            KillsScoreOver = killsScoreOver;
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
                            double playerScore = item.Value.Where(x => x.initiator != null && x.initiator.Player != null).Sum(x => x.score);
                            if (calcKillsScoreOver)
                            {
                                if (playerScore > totalScore * KillsScoreOver)
                                {
                                    Debug.WriteLine("Paler Kill: {0}={1}/{2}", item.Key, playerScore, totalScore);
                                    string typeName = keys[(int)ActorDeadInfoKey.ActorTypeName];
                                    AddKillsCount(armyActor, actorType, typeName);
                                }
                            }
                            else
                            {
                                AiDamageInitiator initiator = GetMaxDamagedInitiator(item.Value);
                                if (initiator != null && initiator.Player != null)
                                {
                                    Debug.WriteLine("Paler Kill: {0}={1}/{2}", item.Key, playerScore, totalScore);
                                    string typeName = keys[(int)ActorDeadInfoKey.ActorTypeName];
                                    AddKillsCount(armyActor, actorType, typeName);
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
                Debug.WriteLine("battleGetDamageVictims Actor={0} Army={1} IsValid={2} IsAlive={3}", actor.Army(), actor.Name(), actor.IsValid(), actor.IsAlive());
                IEnumerable<DamagerScore> damages = Game.battleGetDamageInitiators(actor).ToArray().Where(x => x is DamagerScore).Select(x => x as DamagerScore);

                if (calcKillsScoreOver)
                {
                    double totalScore = damages.Sum(x => x.score);
                    double playerScore = damages.Where(x => x.initiator != null && x.initiator.Player != null).Sum(x => x.score);
                    Debug.WriteLine("   PaylerScore/TotalScore=[{0}/{1}]", playerScore, totalScore);
                    if (playerScore > totalScore * KillsScoreOver)
                    {
                        AddKillsCount(actor);
                    }
                }
                else
                {
                    AiDamageInitiator initiator = GetMaxDamagedInitiator(damages);
                    if (initiator != null && initiator.Player != null)
                    {
                        AddKillsCount(actor);
                    }
                }
            }
        }

        private AiDamageInitiator GetMaxDamagedInitiator(IEnumerable<DamagerScore> damages)
        {
            AiDamageInitiator initiator = null;
            double maxScore = 0;
            IEnumerable<string> actors = damages.Where(x => !string.IsNullOrEmpty(GetName(x.initiator))).Select(x => x.initiator.Actor.Name()).Distinct();
            foreach (string actor in actors)
            {
                var damageScores = damages.Where(x => string.Compare(x.initiator.Actor.Name(), actor) == 0);
                double score = damageScores.Sum(x => x.score);
                if (score > maxScore)
                {
                    maxScore = score;
                    initiator = damageScores.FirstOrDefault().initiator;
                }
            }
            return initiator;
        }

        private string GetName(AiDamageInitiator initiator)
        {
            return initiator.Actor != null ? initiator.Actor.Name() : initiator.Person != null ? initiator.Person.Name() : initiator.Player != null ? initiator.Player.Name() : initiator.Tool != null ? initiator.Tool.Name : string.Empty;
        }

        private void AddKillsCount(AiActor actor)
        {
            int armyActor = actor.Army();
            if (actor is AiAircraft)
            {
                AiAircraft aiAircraft = actor as AiAircraft;
                Debug.WriteLine("  AiAircraft: {0}={1}", aiAircraft.InternalTypeName(), aiAircraft.Group() != null ? aiAircraft.Group().Name(): string.Empty);
                if (armyActor != Army)
                {
                    AddKillsCount(killsAircraft, aiAircraft.InternalTypeName());
                }
                else
                {
                    AddKillsCount(killsFliendlyAircraft, aiAircraft.InternalTypeName());
                }
            }
            else if (actor is AiGroundActor)
            {
                AiGroundActor aiGroundActor = actor as AiGroundActor;
                Debug.WriteLine("  AiGroundActor: {0}={1}", aiGroundActor.InternalTypeName(), aiGroundActor.Group() != null ? aiGroundActor.Group().Name(): string.Empty);
                if (armyActor != Army)
                {
                    AddKillsCount(killsGroundUnit, aiGroundActor.InternalTypeName());
                }
                else
                {
                    AddKillsCount(killsFliendlyGroundUnit, aiGroundActor.InternalTypeName());
                }
            }
        }

        private void AddKillsCount(int armyActor, int actorType, string typeName)
        {
            if (armyActor != Army)
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
                    AddKillsCount(killsFliendlyAircraft, typeName);
                }
                else
                {
                    AddKillsCount(killsFliendlyGroundUnit, typeName);
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

        public string KillsFliendlyAircraftToStrings(string separator = Config.CommaStr)
        {
            return ToString(killsFliendlyAircraft, separator);
        }

        public string KillsFliendlyGroundUnitToStrings(string separator = Config.CommaStr)
        {
            return ToString(killsFliendlyGroundUnit, separator);
        }

        public void UpdatePlayerStatsDefaultAPI(IPlayerStatTotal playerStats, DateTime dt, string valueSummary = null, string separator = Config.CommaStr)
        {
            IGameSingle game = (Game as IGameSingle);
            IPlayer player = game.gameInterface.Player();
            IPlayerStat st = player.GetBattleStat();

            playerStats.Takeoffs += st.takeoffs;
            playerStats.Landings += st.landings;
            playerStats.Bails += st.bails;
            playerStats.Deaths += st.deaths;
            playerStats.Kills += ((int)(st.kills * 100)) / 100.0;
            string killsTypes = ToStringkillsTypes(st.killsTypes, separator);
            if (!string.IsNullOrEmpty(killsTypes))
            {
                if (string.IsNullOrEmpty(valueSummary))
                {
                    if (playerStats.KillsHistory.ContainsKey(dt.Date))
                    {
                        playerStats.KillsHistory[dt.Date] += separator + killsTypes;
                    }
                    else
                    {
                        playerStats.KillsHistory.Add(dt.Date, killsTypes);
                    }
                }
                else
                {
                    string val = string.Format("{0}|{1}", valueSummary, killsTypes);
                    if (playerStats.KillsHistory.ContainsKey(dt.Date))
                    {
                        playerStats.KillsHistory[dt.Date] += separator + val;
                    }
                    else
                    {
                        playerStats.KillsHistory.Add(dt.Date, val);
                    }
                }
            }
        }

        public void UpdatePlayerStat(EPlayerStatsType statType, IPlayerStatTotal playerStatTotal, DateTime dt, string valueSummary = null, string separator = Config.CommaStr)
        {
            if (statType == EPlayerStatsType.Api)
            {
                UpdatePlayerStatsDefaultAPI(playerStatTotal, dt, valueSummary, separator);
            }
            else
            {
                IPlayerStat st = Game.gameInterface.Player().GetBattleStat();
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

        public static string ToStringTotalResult(IPlayerStatTotal playerStatTotal, string format = PlayerStatTotalFormat, string formatHistory = PlayerStatKillsHistoryFormat, string separator = Config.CommaStr)
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

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(format, KeyTakeoff, playerStatTotal.Takeoffs);
            sb.AppendLine();
            sb.AppendFormat(format, KeyLandings, playerStatTotal.Landings);
            sb.AppendLine();
            sb.AppendFormat(format, KeyDeaths, playerStatTotal.Deaths);
            sb.AppendLine();
            sb.AppendFormat(format, KeyBails, playerStatTotal.Bails);
            sb.AppendLine();
            sb.AppendFormat(format, KeyAircraftKills, playerStatTotal.Kills.ToString(Config.KillsFormat, Config.Culture));
            sb.AppendLine();
            sb.AppendFormat(format, KeyGroundUnitKills, playerStatTotal.KillsGround.ToString(Config.KillsFormat, Config.Culture));
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
            sb.AppendFormat(format, KeyFriendlyAircraft, KillsFliendlyAircraftTotal);
            sb.AppendLine();
            sb.AppendFormat(format, KeyFriendlyGroundUnit, KillsFliendlyGroundUnitTotal);
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
            sb.AppendFormat(format, KeyFriendlyAircraft, KillsFliendlyAircraftToStrings(separator));
            sb.AppendLine();
            sb.AppendFormat(format, KeyFriendlyGroundUnit, KillsFliendlyGroundUnitToStrings(separator));
            sb.AppendLine();
            return sb.ToString();
        }

        public static string ToStringSummary(IPlayerStat stat, bool AddKills = true, string format = PlayerStatFormat)
        {
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
                sb.AppendFormat(format, KeyKills, stat.kills.ToString(Config.KillsFormat, Config.Culture));
                sb.AppendLine();
                sb.AppendFormat(format, KeyFriendlyKills, stat.fkills.ToString(Config.KillsFormat, Config.Culture));
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
            return string.Join(separator, dic.Select(x => string.Format("{0} {1}", AircraftInfo.CreateDisplayName(x.Key), x.Value.ToString(Config.KillsFormat, Config.Culture))));
        }

        private DamagerScore[] GetPlayerDamageScore(ArrayList listDamage)
        {
            return listDamage.ToArray().Where(x => x is DamagerScore && (x as DamagerScore).initiator.Player != null).ToArray() as DamagerScore[];
        }
    }
}
