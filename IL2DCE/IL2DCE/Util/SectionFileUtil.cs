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
using maddox.game;

namespace IL2DCE.Util
{
    public class SectionFileUtil
    {
        public static int ReadNumeric(ISectionFile file, string section, string key, string fileInfo = null)
        {
            int num = file.get(section, key, -1);
            if (num == -1)
            {
                InvalidInifileFormatException(fileInfo, section, key);
            }
            return num;
        }

        public static void InvalidInifileFormatException(string file, string section, string key)
        {
            throw new FormatException(string.Format("Invalid Value [File:{0}, Section:{1}, Key:{2}]", file != null ? file: string.Empty, section, key));
        }

        public static void Write(ISectionFile file, string section, string key, string value, bool overwrite = true)
        {
            if (file.exist(section, key))
            {
                if (overwrite)
                {
                    file.set(section, key, value);
                }
            }
            else
            {
                file.add(section, key, value);
            }
        }

        public static int CopySection(ISectionFile fileSrc, ISectionFile fileDest, string section, bool overwrite = true)
        {
            int count = 0;
            if (fileSrc.exist(section))
            {
                string key;
                string value;
                int lines = fileSrc.lines(section);
                for (int i = 0;i < lines; i++)
                {
                    fileSrc.get(section, i, out key, out value);
                    if (!fileDest.exist(section, key) || overwrite)
                    {
                        fileDest.add(section, key, value);
                    }
                    // Debug.WriteLine("{0} {1} Key={2} Value={3}", section, i, key, value);
                    count++;
                }
            }
            return count;
        }

        public static IEnumerable<string> CopySectionGetKey(ISectionFile fileSrc, ISectionFile fileDest, string section, bool overwrite = true)
        {
            List<string> keys = new List<string>();
            if (fileSrc.exist(section))
            {
                string key;
                string value;
                int lines = fileSrc.lines(section);
                for (int i = 0; i < lines; i++)
                {
                    fileSrc.get(section, i, out key, out value);
                    if (!fileDest.exist(section, key) || overwrite)
                    {
                        fileDest.add(section, key, value);
                    }
                    // Debug.WriteLine("{0} {1} Key={2} Value={3}", section, i, key, value);
                    keys.Add(key);
                }
            }
            return keys;
        }

        public static int CopySectionReplace(ISectionFile fileSrc, ISectionFile fileDest, string section, IEnumerable<string> oldValue, string newValue)
        {
            int count = 0;
            if (fileSrc.exist(section))
            {
                string key;
                string value;
                int lines = fileSrc.lines(section);
                for (int i = 0; i < lines; i++)
                {
                    fileSrc.get(section, i, out key, out value);
                    foreach (var item in oldValue)
                    {
                        value = value.Replace(item, newValue);
                    }
                    Write(fileDest, section, key, value);
                    count++;
                }
            }
            return count;
        }
                                                                                                                                
        public static IEnumerable<string> CopySectionReplaceGetKey(ISectionFile fileSrc, ISectionFile fileDest, string section, IEnumerable<string> oldValue, string newValue)
        {
            List<string> keys = new List<string>();
            if (fileSrc.exist(section))
            {
                string key;
                string value;
                int lines = fileSrc.lines(section);
                for (int i = 0; i < lines; i++)
                {
                    fileSrc.get(section, i, out key, out value);
                    foreach (var item in oldValue)
                    {
                        value = value.Replace(item, newValue);
                    }
                    Write(fileDest, section, key, value);
                    keys.Add(key);
                }
            }
            return keys;
        }

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
    }
}
