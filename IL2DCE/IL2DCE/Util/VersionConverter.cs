using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
    }
}
