using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SlugEditor.Core.Utility
{
    public class PathUtil
    {
        private static string TempRootPath = "";

        public static void SetTempRootPath(string path) 
        {
            TempRootPath = path;
        }

        public static string GetTempPath() 
        {
            if (string.IsNullOrEmpty(TempRootPath)) 
            {
                return Path.Combine(Path.GetTempPath(), "SlugEngine", "Editor");
            }
            return TempRootPath;
        }

        public static void CreateDirectory(string path) 
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void DeleteDirectory(string path) 
        {
            if (Directory.Exists(path)) 
            {
                Directory.Delete(path);
            }
        }

        public static bool Exists(string path) 
        {
            return Path.Exists(path);
        }

        public static string Combine(params string[] paths) 
        {
            var ret = Path.Combine(paths);
            ret = ret.Replace("\\", "/");
            return ret;
        }

        public static string GetFileName(string filePath, bool hasExtension = false) 
        {
            if (hasExtension)
            {
                return Path.GetFileName(filePath);
            }
            else 
            {
                return Path.GetFileNameWithoutExtension(filePath);
            }
        }

        public static string GetExtension(string path, int index = 0) 
        {
            string[] extensions = GetExtensions(path);
            if (extensions.Length == 0) 
            {
                return "";
            }

            return extensions[index];
        }

        public static string[] GetExtensions(string path) 
        {
            if (string.IsNullOrEmpty(path))
            {
                return Array.Empty<string>();
            }

            string fileName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(fileName))
            {
                return Array.Empty<string>();
            }

            string[] parts = fileName.Split('.');

            if (parts.Length <= 1)
            {
                return Array.Empty<string>();
            }

            return parts[1..];
        }
    }
}
