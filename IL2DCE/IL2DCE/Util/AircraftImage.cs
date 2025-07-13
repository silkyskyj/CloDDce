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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IL2DCE.Util
{
    public class AircraftImage
    {
        private static readonly string[] endIs = { "late_trop", "Trop_Derated", "Torpedo_trop", "Derated", "AltoQuota", "late", "trop", "100oct", "NF", "Heartbreaker", "Torpedo", "t", };
        private static readonly string[] enddel = { "late_trop", "late", "trop", "t", };
        // private static readonly string[] endReplace = { "late_trop", "t", };
        private static readonly string[] middleIs = { "Mk" };

        private const string defaultPart = "bob";

        private string baseFolder;

        public AircraftImage(string baseFolder)
        {
            this.baseFolder = baseFolder;
        }

        //public ImageSource GetImageSource(string aircraftClass)
        //{
        //    string path = GetImagePath(baseFolder);
        //    if (!string.IsNullOrEmpty(path))
        //    {
        //        // using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        //        {
        //            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        //            var decoder = new TiffBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
        //            BitmapSource source = decoder.Frames[0];

        //        }
        //    }
        //}

        public string GetImagePath(string aircraftClass)
        {
            string path;

            int idx = aircraftClass.IndexOf(":");
            string part = idx != -1 ? aircraftClass.Substring(0, idx) : defaultPart;
            aircraftClass = idx != -1 ? aircraftClass.Substring(idx + 1) : aircraftClass;
            if (!string.IsNullOrEmpty(path = GetImagePathfromPart(part, aircraftClass)))
            {
                return path;
            }

            if (string.Compare(part, defaultPart) != 0)
            {
                if (!string.IsNullOrEmpty(path = GetImagePathfromPart(defaultPart, aircraftClass)))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private string GetImagePathfromPart(string part, string aircraftClass)
        {
            const string del = "Aircraft.";
            string path;

            int idx = aircraftClass.IndexOf(del);
            string cls = idx != -1 ? aircraftClass.Substring(idx + del.Length) : aircraftClass;
            string folderBase = string.Format("{0}\\{1}\\3do\\Plane", baseFolder, part);
            string folder = string.Format("{0}\\{1}", folderBase, cls);
            if (Directory.Exists(folder))
            {
                if (!string.IsNullOrEmpty(path = GetImagePathfromFolder(folder, cls)))
                {
                    return path;
                }
            }

            IEnumerable<string> dirs = FindFolder(folderBase, cls);
            foreach (var item in dirs)
            {
                if (!string.IsNullOrEmpty(path = GetImagePathfromFolder(item, cls)))
                {
                    return path;
                }
            }

            foreach (var item in endIs)
            {
                if (cls.EndsWith(item, StringComparison.InvariantCultureIgnoreCase))
                {
                    folder = string.Format("{0}\\{1}", folderBase, cls.Substring(0, cls.Length - item.Length - 1));
                    if (!string.IsNullOrEmpty(path = GetImagePathfromFolder(folder, cls)))
                    {
                        return path;
                    }
                }
            }

            foreach (var item in middleIs)
            {
                if ((idx = cls.IndexOf(item)) != -1)
                {
                    string cls2 = string.Format("{0}_{1}", cls.Substring(0, idx), cls.Substring(idx));
                    folder = string.Format("{0}\\{1}", folderBase, cls2);
                    if (!string.IsNullOrEmpty(path = GetImagePathfromFolder(folder, cls)))
                    {
                        return path;
                    }

                    foreach (var item2 in endIs)
                    {
                        if (cls2.EndsWith(item2, StringComparison.InvariantCultureIgnoreCase))
                        {
                            folder = string.Format("{0}\\{1}", folderBase, cls.Substring(0, cls2.Length - item2.Length - 1));
                            if (!string.IsNullOrEmpty(path = GetImagePathfromFolder(folder, cls2)))
                            {
                                return path;
                            }
                        }
                    }
                }
            }

            foreach (var item in enddel)
            {
                if (cls.EndsWith(item, StringComparison.InvariantCultureIgnoreCase))
                {
                    folder = string.Format("{0}\\{1}", folderBase, cls.Substring(0, cls.Length - item.Length - 1));
                    if (!string.IsNullOrEmpty(path = GetImagePathfromFolder(folder, cls)))
                    {
                        return path;
                    }
                }
            }

            //for (idx = 0; idx < endReplace.Length / 2; idx += 2)
            //{
            //    if (cls.EndsWith(endReplace[idx], StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        folder = string.Format("{0}\\{1}{2}", folderBase, cls.Substring(0, cls.Length - endReplace[idx].Length), endReplace[idx + 1]);
            //        if (!string.IsNullOrEmpty(path = GetImagePathfromFolder(folder, cls)))
            //        {
            //            return path;
            //        }
            //    }
            //}

            return string.Empty;
        }

        private IEnumerable<string> FindFolder(string folderBase, string aircraftClass)
        {
            const string replace = "[-_]";
            string[] dirs = Directory.GetDirectories(folderBase);
            string cls = Regex.Replace(aircraftClass, replace, string.Empty);
            return dirs.Where(x => string.Compare(Regex.Replace(x.Split(Path.DirectorySeparatorChar).LastOrDefault(), replace, string.Empty), cls, false) == 0);
        }

        private string GetImagePathfromFolder(string folder, string aircraftClass)
        {
            const string file = "item.tif";
            const string fileTrop = "item_trop.tif";
            const string fileTropLate = "item_trop_late.tif";

            string path = string.Format("{0}\\{1}", folder, file);
            if (File.Exists(path))
            {
                return path;
            }

            if (aircraftClass.EndsWith("trop", StringComparison.InvariantCultureIgnoreCase))
            {
                path = string.Format("{0}\\{1}", folder, file);
                if (File.Exists(fileTrop))
                {
                    return path;
                }
            }

            if (aircraftClass.EndsWith("late", StringComparison.InvariantCultureIgnoreCase))
            {
                path = string.Format("{0}\\{1}", folder, fileTropLate);
                if (File.Exists(fileTrop))
                {
                    return path;
                }
            }

            path = string.Format("{0}\\{1}", folder, fileTrop);
            if (File.Exists(path))
            {
                return path;
            }

            path = string.Format("{0}\\{1}", folder, fileTropLate);
            if (File.Exists(path))
            {
                return path;
            }

            return string.Empty;
        }
    }
}
