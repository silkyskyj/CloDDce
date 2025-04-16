using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2DCE.MissionObjectModel
{
    public class Option
    {
        public static readonly char[] SplitChars = new char[] { '/', ' ' };

        public string Others
        {
            get;
            set;
        }

        public Option(string others = null)
        {
            Others = others;
        }

        public static string[] Parse(string str)
        {
            return str.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }

        public static string ToString(IEnumerable<string> str)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i + 1 < str.Count(); i += 2)
            {
                sb.AppendFormat("/{0} {1}", str.ElementAt(i), str.ElementAt(i + 1));
            }
            return sb.ToString();
        }
    }
}
