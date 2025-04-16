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
using System.IO;
using System.Text;

namespace IL2DCE.Util
{
    public class FileUtil
    {
        public static bool IsFileWritable(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {

                }
                return true;
            }
            catch (Exception ex)
            {
                string message = string.Format("IsFileWritable[Error={0}][Path={1}]", ex.Message, path);
                Core.WriteLog(message);
            }
            return false;
        }

        public static string AsciitoUtf8String(string str)
        {
            return Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(str));
        }
    }
}
