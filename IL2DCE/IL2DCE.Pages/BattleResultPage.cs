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

using maddox.game;
using maddox.game.play;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace IL2DCE.Pages
{
    public class BattleResultPage : PageDefImpl
    {
        public BattleResultPage(string name, FrameworkElement fe)
            : base(name, fe)
        {
        }

        protected string ToString<T>(Dictionary<string, T> ds)
        {
            return string.Join(", ", ds.Select(x => string.Format("[{0}]={1}", x.Key, x.Value)));
        }

        protected string ToStringTimeSpan(Dictionary<string, float> ds)
        {
            return string.Join(", ", ds.Select(x => string.Format("[{0}]={1}", x.Key, new TimeSpan((long)(x.Value * 10000000)).ToString("hh\\:mm\\:ss"))));
        }

        protected virtual string GetResultSummary(IGameSingle game)
        {
            int exp = game.Core.CurrentCareer.Experience;
            int exp2 = game.BattleSuccess == EBattleResult.DRAW ? 100: 200;
            int rank = game.Core.CurrentCareer.RankIndex;
            return string.Format("{0}\nExp: {1} + {2}/{3}\n{4}\n",
                                    game.BattleSuccess.ToString(),
                                    exp,
                                    exp2,
                                    ((rank + 1) * 1000),
                                    (exp + exp2 >= (rank + 1) * 1000) ? "Promition!" : string.Empty);
        }

        protected virtual string GetPlayerStat(IPlayer player)
        {
            IPlayerStat st = player.GetBattleStat();
#if false
            return String.Format("PlayerStat[{0}]\n Flying Time: {1}\n Takeoffs: {2}\n Landings: {3}\n Deaths: {4}\n Bails: {5}\n Ditches: {6}\n PlanesWrittenOff: {7}\n" +
                                    " Kills: {8}\n Friendly Kills: {9}\n KillsTypes: {10}\n" + 
                                    " Bullets Fire: {11}\n         Hit: {12}\n         HitAir: {13}\n Rockets Fire: {14}\n         Hit: {15}\n" +
                                    " Bombs Fire: {16}\n        Weight: {16}\n        Hit: {18}\n Torpedos Fire: {19}\n          Hit: {20}\n",
                                    player?.Name() ?? string.Empty,
                                    ToString(st.tTotalTypes),
                                    st.takeoffs,
                                    st.landings,
                                    st.deaths,
                                    st.bails,
                                    st.ditches,
                                    st.planesWrittenOff,
                                    st.kills,
                                    st.fkills,
                                    ToString(st.killsTypes),
                                    st.bulletsFire,
                                    st.bulletsHit,
                                    st.bulletsHitAir,
                                    st.rocketsFire,
                                    st.rocketsHit,
                                    st.bombsFire,
                                    st.bombsWeight,
                                    st.bombsHit,
                                    st.torpedosFire,
                                    st.torpedosHit);
#else
            return String.Format("PlayerStat[{0}]\n Flying Time: {1}\n Takeoffs: {2}\n Landings: {3}\n Deaths: {4}\n Bails: {5}\n Ditches: {6}\n PlanesWrittenOff: {7}\n" +
                                    " Kills: {8}\n Friendly Kills: {9}\n KillsTypes: {10}\n",
                                    player?.Name() ?? string.Empty,
                                    ToStringTimeSpan(st.tTotalTypes),
                                    st.takeoffs,
                                    st.landings,
                                    st.deaths,
                                    st.bails,
                                    st.ditches,
                                    st.planesWrittenOff,
                                    st.kills,
                                    st.fkills,
                                    ToString(st.killsTypes));
#endif
        }

        public override void _leave(maddox.game.IGame play, object arg)
        {
            base._leave(play, arg);

            _game = null;
        }

        protected IGame Game
        {
            get
            {
                return _game;
            }
        }

        protected IGame _game;
    }
}
