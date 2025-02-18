using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace IL2DCE.Util
{
    public class VersionConverter
    {
        public VersionConverter()
        {
        }

        public static Version GetCurrentVersion()
        {
            Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return assembly.GetName().Version;
        }

        public static string ReplaceKillsHistory(string str)
        {
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                char[] sz = str.ToCharArray();
                int len = sz.Length;
                for (int i = 0; i < len; i++)
                {
                    if (sz[i] == '=')
                    {
                        builder.Append(' ');
                    }
                    else if (sz[i] == 'x' && i > 0 && sz[i - 1] == ' ' && (i + 1) < len && sz[i + 1] == ' ')
                    {
                        i++;
                    }
                    else if (sz[i] == ',' && i > 0 && sz[i - 1] >= '0' && sz[i - 1] <= '9' && (i + 1) < len && sz[i + 1] >= '0' && sz[i + 1] <= '9')
                    {
                        builder.Append('.');
                        builder.Append(sz[i + 1]);
                        i++;
                    }
                    else if (sz[i] == ']' || sz[i] == '[')
                    {
                        ;
                    }
                    else
                    {
                        builder.Append(sz[i]);
                    }
                }
            }
            return builder.ToString();
        }
    }
}
