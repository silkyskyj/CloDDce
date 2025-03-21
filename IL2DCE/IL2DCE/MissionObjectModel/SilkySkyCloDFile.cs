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
using System.IO;
using System.Linq;
using System.Text;
using maddox.game;

namespace IL2DCE.MissionObjectModel
{
    public class SilkySkyCloDFile : ISectionFile
    {
        public string ErrorText
        {
            get;
            protected set;
        }

        protected bool IgnoreCase
        {
            get;
            set;
        }

        protected IEnumerable<string> Lines;

        protected Dictionary<string, List<KeyValuePair<string, string>>> Sections = new Dictionary<string, List<KeyValuePair<string, string>>>();

        protected SilkySkyCloDFile()
        {
        }

        public static SilkySkyCloDFile Create()
        {
            return new SilkySkyCloDFile();
        }

        public static SilkySkyCloDFile Load(string path, bool ignoreCase = true)
        {
            var item = new SilkySkyCloDFile() { IgnoreCase = ignoreCase };
            if (item.Open(path))
            {
                return item;
            }
            return null;
        }

        public bool Open(string path)
        {
            if (File.Exists(path))
            {
                Lines = File.ReadAllLines(path, Encoding.UTF8);
                Parse();
                return true;
            }
            else
            {
                ErrorText = string.Format("File not found. [{0}]", path);
                return false;
            }
        }

        public bool Save(string path, bool original = false)
        {
            if (original && Lines != null)
            {
                File.WriteAllLines(path, Lines, Encoding.UTF8);
            }
            else
            {
                var list = new List<string>();
                foreach (var item in Sections)
                {
                    list.Add(string.Format("[{0}]", item.Key));
                    item.Value.ForEach(x => 
                    {
                        list.Add(string.Format("  {0} {1}", x.Key, x.Value));
                    });
                    list.Add(string.Empty);
                }
                File.WriteAllLines(path, list, Encoding.UTF8);
            }
            return true;
        }

        protected void Parse()
        {
            if (Lines != null)
            {
                char[] trimChars = "　\t".ToCharArray();
                string section = string.Empty;
                foreach (var item in Lines)
                {
                    string key;
                    string value;
                    string line = item.Trim().Trim(trimChars);
                    if (line.StartsWith("#"))       // Comment
                    {
                        ;
                    }
                    else if (line.StartsWith("["))  // Section
                    {
                        int end = line.IndexOf("]");
                        if (end != -1 && end >= 2)
                        {
                            line = line.Substring(1, end - 1).Trim().Trim(trimChars);
                            if (!string.IsNullOrEmpty(line))
                            {
                                section = line;
                                if (!Exist(section))
                                {
                                    Sections.Add(section, new List<KeyValuePair<string, string>>());
                                }
                            }
                        }
                    }
                    else if (line.Length > 0 && !string.IsNullOrEmpty(section))
                    {
                        int end = line.IndexOf(" ");
                        if (end != -1)              // Key & Value
                        {
                            key = line.Substring(0, end);
                            value = line.Substring(end + 1).TrimStart().TrimStart(trimChars);
                        }
                        else                        // Value Line
                        {
                            key = line;
                            value = string.Empty;
                        }
                        Sections[section].Add(new KeyValuePair<string, string>(key, value));
                    }
                    else                            // Other
                    {
                        ;
                    }
                }
            }
        }

        public bool Exist(string section, string key)
        {
            return Sections.Where(x => string.Compare(x.Key, section, true) == 0).Any(x => x.Value.Any(y => string.Compare(y.Key, key, true) == 0));
        }

        public string Get(string section, string key)
        {
            var resultSection = Sections.Where(x => string.Compare(x.Key, section, true) == 0).Select(x => x.Value).FirstOrDefault();
            if (resultSection != null)
            {
                return resultSection.Where(x => string.Compare(x.Key, key, true) == 0).Select(x => x.Value).FirstOrDefault();
            }
            return string.Empty;
        }

        public string Get(string section, string key, string def)
        {
            string result = Get(section, key);
            return !string.IsNullOrEmpty(result) ? result : def;
        }

        public float Get(string section, string key, float def)
        {
            string result = Get(section, key);
            float val;
            if (!string.IsNullOrEmpty(result) && float.TryParse(result, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out val))
            {
                return val;
            }
            return def;
        }

        public int Get(string section, string key, int def)
        {
            string result = Get(section, key);
            int val;
            if (!string.IsNullOrEmpty(result) && int.TryParse(result, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out val))
            {
                return val;
            }
            return def;
        }

        public bool Get(string section, string key, bool def)
        {
            string result = Get(section, key);
            bool val;
            if (!string.IsNullOrEmpty(result) && bool.TryParse(result, out val))
            {
                return val;
            }
            return def;
        }

        public void Set(string section, string key, string value)
        {
            var resultSection = Sections.Where(x => string.Compare(x.Key, section, true) == 0).Select(x => x.Value).FirstOrDefault();
            if (resultSection == null)
            {
                Sections.Add(section, new List<KeyValuePair<string, string>>());
                resultSection = Sections.Where(x => string.Compare(x.Key, section, true) == 0).Select(x => x.Value).FirstOrDefault();
            }
            var resultKeyValue = resultSection.Where(x => string.Compare(x.Key, key, true) == 0);
            foreach (var item in resultKeyValue)
            {
                resultSection.Remove(item);
            }
            resultSection.Add(new KeyValuePair<string, string>(key, value));
        }

        public void Set(string section, string key, int value)
        {
            Set(section, key, value.ToString(CultureInfo.InvariantCulture.NumberFormat));
        }

        public void Set(string section, string key, float value)
        {
            Set(section, key, value.ToString(CultureInfo.InvariantCulture.NumberFormat));
        }

        public void Set(string section, string key, bool value)
        {
            Set(section, key, value.ToString(CultureInfo.InvariantCulture.NumberFormat));
        }

        public bool Exist(string section)
        {
            return Sections.Any(x => string.Compare(x.Key, section, true) == 0);
        }

        public bool Delete(string section)
        {
            int i = 0;
            foreach (var item in Sections.Where(x => string.Compare(x.Key, section, true) == 0))
            {
                Sections.Remove(item.Key);
                i++;
            }
            return i > 0;
        }

        public int LinesCount(string section)
        {
            var resultSection = Sections.Where(x => string.Compare(x.Key, section, true) == 0).Select(x => x.Value).FirstOrDefault();
            if (resultSection != null)
            {
                return resultSection.Count;
            }
            return 0;
        }

        public bool Delete(string section, int line)
        {
            var resultSection = Sections.Where(x => string.Compare(x.Key, section, true) == 0).Select(x => x.Value).FirstOrDefault();
            if (resultSection != null && line < resultSection.Count())
            {
                resultSection.Remove(resultSection[line]);
                return true;
            }
            return false;
        }

        public void Add(string section, string key, string value)
        {
            var resultSection = Sections.Where(x => string.Compare(x.Key, section, true) == 0).Select(x => x.Value).FirstOrDefault();
            if (resultSection == null)
            {
                Sections.Add(section, new List<KeyValuePair<string, string>>());
                resultSection = Sections.Where(x => string.Compare(x.Key, section, true) == 0).Select(x => x.Value).FirstOrDefault();
            }
            resultSection.Add(new KeyValuePair<string, string>(key, value));
        }

        public bool Get(string section, int line, out string key, out string value)
        {
            var resultSection = Sections.Where(x => string.Compare(x.Key, section, true) == 0).Select(x => x.Value).FirstOrDefault();
            if (resultSection != null && line < resultSection.Count())
            {
                key = resultSection[line].Key;
                value = resultSection[line].Value;
                return  true;
            }

            key = string.Empty;
            value = string.Empty;
            return false;
        }

        #region ISectionFile

        public bool isReadOnly()
        {
            throw new NotImplementedException();
        }

        public bool exist(string section, string key)
        {
            return Exist(section, key);
        }

        public string get(string section, string key)
        {
            return Get(section, key);
        }

        public string get(string section, string key, string def)
        {
            return Get(section, key, def);
        }

        public float get(string section, string key, float def)
        {
            return Get(section, key, def);
        }

        public float get(string section, string key, float def, float min, float max)
        {
            throw new NotImplementedException();
        }

        public int get(string section, string key, int def)
        {
            return Get(section, key, def);
        }

        public int get(string section, string key, int def, int min, int max)
        {
            throw new NotImplementedException();
        }

        public bool get(string section, string key, bool def)
        {
            return Get(section, key, def);
        }

        public void set(string section, string key, string value)
        {
            Set(section, key, value);
        }

        public void set(string section, string key, int value)
        {
            Set(section, key, value);
        }

        public void set(string section, string key, float value)
        {
            Set(section, key, value);
        }

        public void set(string section, string key, bool value)
        {
            Set(section, key, value);
        }

        public bool exist(string section)
        {
            return Exist(section);
        }

        public void delete(string section)
        {
            Delete(section);
        }

        public int lines(string section)
        {
            return LinesCount(section);
        }

        public void get(string section, int line, out string key, out string value)
        {
            Get(section, line, out key, out value);
        }

        public void delete(string section, int line)
        {
            Delete(section, line);
        }

        public void add(string section, string key, string value)
        {
            Add(section, key, value);
        }

        public void save()
        {
            throw new NotImplementedException();
        }

        public void save(string fileName)
        {
            Save(fileName);
        }

        #endregion

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
            throw new FormatException(string.Format("Invalid Value [File:{0}, Section:{1}, Key:{2}]", file != null ? file : string.Empty, section, key));
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

        public static IEnumerable<string> ReadSectionKeies(ISectionFile fileSrc, string section)
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
                    keys.Add(key);
                }
            }
            return keys;
        }


        public static int CopySection(ISectionFile fileSrc, ISectionFile fileDest, string section, bool overwrite = true)
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
    }
}
