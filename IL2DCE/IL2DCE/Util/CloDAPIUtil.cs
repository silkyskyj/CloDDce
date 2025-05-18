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

using maddox.game.world;
using System.Diagnostics;
using System;
using System.Text;

namespace IL2DCE.Util
{
    public class CloDAPIUtil
    {
        public static string GetActorName(AiDamageInitiator initiator)
        {
            try
            {
                if (initiator.Actor != null)
                {
                    return initiator.Actor.Name();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return string.Empty;
        }

        public static string GetPlersonName(AiDamageInitiator initiator)
        {
            try
            {
                if (initiator.Person != null)
                {
                    return initiator.Person.Name();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return string.Empty;
        }

        public static string GetPlayerName(AiDamageInitiator initiator)
        {
            try
            {
                if (initiator.Player != null)
                {
                    return initiator.Player.Name();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return string.Empty;
        }

        public static string GetToolName(AiDamageInitiator initiator)
        {
            try
            {
                if (initiator.Tool != null)
                {
                    return initiator.Tool.Name;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return string.Empty;
        }

        public static string GetName(AiDamageInitiator initiator)
        {
            string val = GetActorName(initiator);
            if (string.IsNullOrEmpty(val))
            {
                val = GetPlersonName(initiator);
                if (string.IsNullOrEmpty(val))
                {
                    val = GetPlayerName(initiator);
                    if (string.IsNullOrEmpty(val))
                    {
                        val = GetToolName(initiator);
                    }
                }
            }
            return val;
        }

        public static AiActor[] GetItems(AiGroup group)
        {
            try
            {
                if (group != null)
                {
                    return group.GetItems();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        public static AiWayPoint[] GetWays(AiGroup group)
        {
            try
            {
                if (group != null)
                {
                    return group.GetWay();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        public static int GetCurrentWayPoint(AiGroup group)
        {
            try
            {
                if (group != null)
                {
                    return group.GetCurrentWayPoint();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return -1;
        }

        public static string ActorInfo(AiActor actor)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                if (actor != null)
                {
                    sb.Append(actor.Army());
                    sb.AppendFormat(" {0}", actor.Name());
                    if (actor is AiCart)
                    {
                        sb.AppendFormat("({0})", (actor as AiCart).InternalTypeName());
                    }
                    AiGroup group = actor.Group();
                    if (group != null)
                    {
                        sb.AppendFormat("[{0}]", group.Name());
                    }
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return string.Empty;
        }
    }
}
