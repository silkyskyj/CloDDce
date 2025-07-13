// IL2DCE: A dynamic campaign engine & quick mission for IL-2 Sturmovik: Cliffs of Dover Blitz + DLC
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

using System.Collections.Generic;
using System.IO;

namespace IL2DCE.MissionObjectModel
{
    public class BriefingFile
    {
        public class Text
        {
            public IDictionary<string, string> Sections
            {
                get
                {
                    return this.sections;
                }
            }
            IDictionary<string, string> sections = new Dictionary<string, string>();

            public override string ToString()
            {
                string result = "";
                foreach (string key in Sections.Keys)
                {
                    result += Sections[key] + "\n\n";
                }
                return result;
            }
        }

        public string MissionDescription
        {
            get
            {
                return missionDescription;
            }
            set
            {
                missionDescription = value;
            }
        }
        private string missionDescription = "";

        public IDictionary<string, string> Name
        {
            get
            {
                return name;
            }
        }
        private Dictionary<string, string> name = new Dictionary<string, string>();

        public IDictionary<string, Text> Description
        {
            get
            {
                return description;
            }
        }
        private Dictionary<string, Text> description = new Dictionary<string, Text>();

        public void SaveTo(string systemFileName, string missionName)
        {
            using (TextWriter briefingFileWriter = new StreamWriter(systemFileName, false))
            {
                briefingFileWriter.WriteLine("[Info]");
                briefingFileWriter.WriteLine("<Name>");
                briefingFileWriter.WriteLine("Info");
                briefingFileWriter.WriteLine("<Caption>");
                briefingFileWriter.WriteLine(missionName);
                briefingFileWriter.WriteLine("<Caption>");
                briefingFileWriter.WriteLine(MissionDescription);

                foreach (string key in Name.Keys)
                {
                    briefingFileWriter.WriteLine("[" + key + "]");
                    briefingFileWriter.WriteLine("<Name>");
                    briefingFileWriter.WriteLine(Name[key]);

                    briefingFileWriter.WriteLine("<Description>");
                    if (Description.ContainsKey(key))
                    {
                        briefingFileWriter.WriteLine(Description[key]);
                    }
                    else
                    {
                        briefingFileWriter.WriteLine("");
                    }
                }

                briefingFileWriter.Close();
            }
        }
    }
}