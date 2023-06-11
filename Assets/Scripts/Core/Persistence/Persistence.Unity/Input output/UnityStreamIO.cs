using System;
using System.IO;
using System.Text;

namespace HereticalSolutions.Persistence.IO
{
    public static class UnityStreamIO
    {
        public static bool  OpenReadStream(
            UnityFileSystemSettings settings,
            out FileStream dataStream)
        {
            string savePath = settings.FullPath;

            dataStream = default(FileStream);

            if (!FileExists(settings.FullPath))
                return false;

            dataStream = new FileStream(savePath, FileMode.Open);

            return true;
        }
        
        public static bool  OpenReadStream(
            UnityFileSystemSettings settings,
            out StreamReader streamReader)
        {
            string savePath = settings.FullPath;

            streamReader = default(StreamReader);

            if (!FileExists(settings.FullPath))
                return false;

            streamReader = new StreamReader(savePath, Encoding.UTF8);

            return true;
        }
        
        public static bool OpenWriteStream(
            UnityFileSystemSettings settings,
            out FileStream dataStream)
        {
            string savePath = settings.FullPath;

            EnsureDirectoryExists(savePath);
            
            dataStream = new FileStream(savePath, FileMode.Create);

            return true;
        }

        public static bool OpenWriteStream(
            UnityFileSystemSettings settings,
            out StreamWriter streamWriter)
        {
            string savePath = settings.FullPath;

            EnsureDirectoryExists(savePath);
            
            streamWriter = new StreamWriter(savePath, false, Encoding.UTF8);

            return true;
        }
        
        public static void CloseStream(FileStream dataStream)
        {
            dataStream.Close();
        }
        
        public static void CloseStream(StreamReader streamReader)
        {
            streamReader.Close();
        }
        
        public static void CloseStream(StreamWriter streamWriter)
        {
            streamWriter.Close();
        }
        
        public static void Erase(UnityFileSystemSettings settings)
        {
            string savePath = settings.FullPath;

            if (File.Exists(savePath))
                File.Delete(savePath);
        }

        /// <summary>
        /// Checks whether the file at the specified path exists
        /// - Also makes sure the folder directory specified in the path actually exists anyway
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>Does the file exist</returns>
        private static bool FileExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception("[UnityStreamIO] INVALID PATH");
			
            string directoryPath = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(directoryPath))
                throw new Exception("[UnityStreamIO] INVALID DIRECTORY PATH");
			
            if (!Directory.Exists(directoryPath))
            {
                return false;
            }

            return File.Exists(path);
        }
        
        private static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception("[TextFileIO] INVALID PATH");
			
            string directoryPath = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(directoryPath))
                throw new Exception("[TextFileIO] INVALID DIRECTORY PATH");
			
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}