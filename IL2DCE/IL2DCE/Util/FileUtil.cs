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
using System.Diagnostics;
using System.IO;
using System.Text;
using maddox.game;

namespace IL2DCE.Util
{
    public class FileUtil
    {
        public static bool IsFileWritable(string path, bool errorLog = false)
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
                if (errorLog)
                {
                    Core.WriteLog(message);
                }
                else
                {
                    Debug.WriteLine(message);
                }
            }
            return false;
        }

        public static string AsciitoUtf8String(string str)
        {
            return Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(str));
        }

        public static void DeleteGameFile(GameIterface gameIterface, string pathGame, bool errorLog = false)
        {
            try
            {
                string systemPath = gameIterface.ToFileSystemPath(pathGame);
                if (!string.IsNullOrEmpty(systemPath) && File.Exists(systemPath))
                {
                    File.Delete(systemPath);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("FileUtil.DeleteFile[Error={0}][Path={1}]", ex.Message, pathGame);
                if (errorLog)
                {
                    Core.WriteLog(message);
                }
                else
                {
                    Debug.WriteLine(message);
                }
            }
        }

        public static string GetGameFileNameWithoutExtension(string pathGame, bool errorLog = false)
        {
            try
            {
                int idx = pathGame.LastIndexOf("/");
                if (idx != -1)
                {
                    pathGame = pathGame.Substring(idx + 1);
                }
                idx = pathGame.LastIndexOf(".");
                if (idx != -1)
                {
                    pathGame = pathGame.Substring(0, idx);
                }
                return pathGame;
            }
            catch (Exception ex)
            {
                string message = string.Format("FileUtil.DeleteFile[Error={0}][Path={1}]", ex.Message, pathGame);
                if (errorLog)
                {
                    Core.WriteLog(message);
                }
                else
                {
                    Debug.WriteLine(message);
                }
            }
            return string.Empty;
        }

        public static void MoveFile(string src, string dest, bool errorLog = false)
        {
            try
            {
                if (File.Exists(dest))
                {
                    File.Delete(dest);
                }
                File.Move(src, dest);
            }
            catch (Exception ex)
            {
                string message = string.Format("FileUtil.MoveFile[Error={0}][Path={1} -> {2}]", ex.Message, src, dest);
                if (errorLog)
                {
                    Core.WriteLog(message);
                }
                else
                {
                    Debug.WriteLine(message);
                }
            }
        }

        public static long FileLength(string path, bool errorLog = false)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                fileInfo.Refresh();
                return fileInfo.Length;
            }
            catch (Exception ex)
            {
                string message = string.Format("FileUtil.FileLength[Error={0}][Path={1}]", ex.Message, path);
                if (errorLog)
                {
                    Core.WriteLog(message);
                }
                else
                {
                    Debug.WriteLine(message);
                }
            }
            return 0;
        }

        public static void BackupFiles(string path, int backupCount, bool errorLog = false)
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string dir = Path.GetDirectoryName(path);
                    string name = Path.GetFileNameWithoutExtension(path);
                    string ext = Path.GetExtension(path);

                    for (int i = backupCount - 1; i >= 0; i--)
                    {
                        string old = CreatePath(dir, name, ext, ".", i);
                        if (File.Exists(old) && FileLength(old, errorLog) > 0)
                        {
                            MoveFile(old, CreatePath(dir, name, ext, ".", i + 1), errorLog);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string message = string.Format("FileUtil.BackupFiles[Error={0}][path={1}]", ex.Message, path);
                    if (errorLog)
                    {
                        Core.WriteLog(message);
                    }
                    else
                    {
                        Debug.WriteLine(message);
                    }
                }
            }
        }

        public static string CreatePath(string dir, string name, string ext, string separator, int idx)
        {
            return string.Format("{0}{1}{2}{3}{4}", dir, Path.DirectorySeparatorChar, name, idx > 0 ? string.Format(Config.NumberFormat, "{0}{1:D}", separator, idx) : string.Empty, ext);
        }

        public static string CreateWritablePath(string path, int maxErrorCount, bool errorLog = false)
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                string name = Path.GetFileNameWithoutExtension(path);
                string ext = Path.GetExtension(path);
                int i = 0;
                while (!IsFileWritable(path, errorLog) && i < maxErrorCount)
                {
                    path = CreatePath(dir, name, ext, "-", ++i);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("FileUtil.CreateWritablePath[Error={0}][path={1}]", ex.Message, path);
                if (errorLog)
                {
                    Core.WriteLog(message);
                }
                else
                {
                    Debug.WriteLine(message);
                }
            }
            return path;
        }
    }
}