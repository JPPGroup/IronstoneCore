using System;
using System.IO;
using System.Threading;

namespace Jpp.Ironstone.Core.Tests
{
    class LogHelper
    {
        private const int millisecondDelay = 100;
        private const int retries = 5;
        
        public static string GetLogName()
        {
            DateTime time = DateTime.Now;
            return $"IronstoneLog{time.ToString("yyyyMMdd")}.txt";
        }

        public static TextReader GetLogReader()
        {
            var readStream = File.Open(LogHelper.GetLogName(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return new StreamReader(readStream);
        }
        
        public static bool IsFileLocked()
        {
            try
            {
                using(FileStream stream = File.Open(LogHelper.GetLogName(), FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }

        public static void ClearLog()
        {
            if (File.Exists(LogHelper.GetLogName()))
            {
                int loops = 0;
                while (loops < retries)
                {
                    if (IsFileLocked())
                    {
                        Thread.Sleep(millisecondDelay);
                    }
                    else
                    {
                        break;
                    }
                    loops++;
                }
                File.Delete(LogHelper.GetLogName());
            }
        }
    }
}
